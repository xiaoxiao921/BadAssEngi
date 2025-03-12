using System;
using System.Collections;
using System.Linq;
using BadAssEngi.Animations;
using BadAssEngi.Assets;
using BadAssEngi.Assets.Sound;
using BadAssEngi.Networking;
using BadAssEngi.Skills;
using BadAssEngi.Skills.Primary.SeekerMissile;
using BadAssEngi.Skills.Secondary.ClusterMine;
using BadAssEngi.Skills.Secondary.OrbitalStrike;
using BadAssEngi.Skills.Special;
using BepInEx;
using BepInEx.Bootstrap;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using R2API;
using R2API.Networking;
using R2API.Networking.Interfaces;
using R2API.Utils;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using static BadAssEngi.Skills.Special.TurretHooks;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedMember.Global

namespace BadAssEngi
{
    [BepInDependency(NetworkingAPI.PluginGUID)]
    [BepInDependency(PrefabAPI.PluginGUID)]
    [BepInDependency(CommandHelper.PluginGUID)]
    [BepInDependency(SoundAPI.PluginGUID)]
    [BepInDependency(LoadoutAPI.PluginGUID)]
    [BepInDependency("at.aster.charactercustomizer", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInPlugin(ModGuid, ModName, ModVer)]
    [NetworkCompatibility]
    public class BadAssEngi : BaseUnityPlugin
    {
        public const string ModGuid = "iDeathHD." + ModName;
        private const string ModName = "BadAssEngi";
        private const string ModVer = "1.3.10";

        internal static BodyIndex EngiBodyIndex;
        private const string EngiBodyPrefabName = "EngiBody";

        internal static Texture2D OrigEngiTexture, OrigAltEngiTexture, OrigTurretTexture;

        internal static BadAssEngi Instance { get; private set; }

        public void Awake()
        {
            Log.Init(Logger);

            Configuration.Init(Config);
            BaeAssets.Init();
            SoundHelper.AddSoundBank();

            InitBadAssEngi();
            InitNetworking();
            InitHooks();

            Instance = this;
        }

        public void Update()
        {
            if (!Run.instance || Application.isBatchMode)
                return;

            KeybindController.Update();
        }

        private static void InitNetworking()
        {
            NetworkingAPI.RegisterMessageType<AnimMsg>();
            NetworkingAPI.RegisterMessageType<DetonateMsg>();
            NetworkingAPI.RegisterMessageType<EngiColorMsg>();
            NetworkingAPI.RegisterMessageType<PlaySoundMsg>();
            NetworkingAPI.RegisterMessageType<PlaySoundAndDestroyMsg>();
            NetworkingAPI.RegisterMessageType<SendRecalcStat>();
            NetworkingAPI.RegisterMessageType<StopAnimMsg>();
            NetworkingAPI.RegisterMessageType<StopSoundMsg>();
            NetworkingAPI.RegisterMessageType<TurretColorDelayedMsgToServer>();
            NetworkingAPI.RegisterMessageType<TurretColorMsg>();
            NetworkingAPI.RegisterMessageType<TurretTypeMsgToServer>();
        }

        private static void InitHooks()
        {
            CommandHelper.AddToConsoleWhenReady();

            RoR2Application.onLoad += () => { EngiBodyIndex = BodyCatalog.FindBodyIndex(EngiBodyPrefabName); };

            IL.RoR2.Stage.RespawnLocalPlayers += ChangeEngiColorAndAddMissileTrackerOnRespawn;

            IL.RoR2.BlastAttack.HandleHits += BlastAttackMultiplySelfMineDamage;

            ModCompatibilities();

            EmotesHooks.Init();
            OrbitalHooks.Init();
            TurretHooks.Init();

            RoR2Application.onLoad += KeybindController.RetrieveFirstGamePad;
        }

        private static void ModCompatibilities()
        {
            var foundCC = false;
            foreach (var plugin in Chainloader.PluginInfos)
            {
                var metadata = plugin.Value.Metadata;
                if (metadata.GUID.Equals("at.aster.charactercustomizer"))
                {
                    foundCC = true;
                    break;
                }
            }

            if (!foundCC)
            {
                IL.RoR2.CharacterMaster.GetDeployableSameSlotLimit += ExpendMaxMinesCount;
            }
        }

        private static void InitBadAssEngi()
        {
            SkillLoader.Init();

            BadAssTurret.Buffs = Configuration.SharedBuffsWithTurret.Value.Split(',').Select(buff => (BuffIndex)int.Parse(buff)).ToArray();
        }

        private static void ChangeEngiColorAndAddMissileTrackerOnRespawn(ILContext il)
        {
            var cursor = new ILCursor(il);

            cursor.GotoNext(
                i => i.MatchLdloc(out _),
                i => i.MatchLdstr(""),
                i => i.MatchCallvirt<CharacterMaster>(nameof(CharacterMaster.CallCmdRespawn))
            );
            cursor.Index++;

            cursor.EmitDelegate<Action>(() => { Instance.StartCoroutine(DelayedEngiColorChangeAndUpdateGrenadeType(1f)); });
        }

        private static void BlastAttackMultiplySelfMineDamage(ILContext il)
        {
            var cursor = new ILCursor(il);
            var damageInfoLoc = 0;
            var targetLoc = 0;

            cursor.GotoNext(
                i => i.MatchLdfld<BlastAttack.HitPoint>(nameof(BlastAttack.HitPoint.hurtBox)),
                i => i.MatchLdfld<HurtBox>(nameof(HurtBox.healthComponent)),
                i => i.MatchStloc(out targetLoc)
            );

            cursor.GotoNext(
                i => i.MatchInitobj("RoR2.BlastAttack/BlastAttackDamageInfo")
            );
            cursor.GotoNext(
                i => i.MatchStloc(out damageInfoLoc)
            );
            cursor.Index++;

            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Ldloc, damageInfoLoc);
            cursor.Emit(OpCodes.Ldloc, targetLoc);

            cursor.EmitDelegate<Func<BlastAttack, BlastAttack.BlastAttackDamageInfo, HurtBox, BlastAttack.BlastAttackDamageInfo>>(ScaleMineDamageDelegate);
            cursor.Emit(OpCodes.Stloc, damageInfoLoc);
        }

        private static BlastAttack.BlastAttackDamageInfo ScaleMineDamageDelegate(
            BlastAttack blastAttack, BlastAttack.BlastAttackDamageInfo blastAttackDamageInfo, HurtBox target)
        {
            if (blastAttack.attackerFiltering == AttackerFiltering.AlwaysHit && target.gameObject == blastAttackDamageInfo.attacker)
            {
                if (blastAttack.inflictor.name.Contains("Mine"))
                {
                    if (blastAttack.inflictor.GetComponent<RecursiveMine>())
                    {
                        blastAttackDamageInfo.damage *= 2f;
                    }
                    else
                    {
                        blastAttackDamageInfo.damage *= 0.05f;
                    }
                }
            }

            return blastAttackDamageInfo;
        }

        private static void ExpendMaxMinesCount(ILContext il)
        {
            var cursor = new ILCursor(il);

            cursor.GotoNext(
                i => i.MatchLdcI4(4), // DeployableSlot.EngiMine
                i => i.MatchStloc(0)
            );
            cursor.Next.OpCode = OpCodes.Ldc_I4;
            cursor.Next.Operand = 10000;

            cursor.GotoNext(
                i => i.MatchLdarg(0),
                i => i.MatchCallOrCallvirt<CharacterMaster>("get_" + nameof(CharacterMaster.bodyInstanceObject)),
                i => i.MatchCallOrCallvirt<GameObject>(nameof(GameObject.GetComponent))
            );
            cursor.RemoveRange(5);
            cursor.Emit(OpCodes.Ldc_I4, 10000);
        }

        private static IEnumerator DelayedEngiColorChangeAndUpdateGrenadeType(float seconds)
        {
            yield return new WaitForSeconds(seconds);

            foreach (var currentNetworkUser in NetworkUser.readOnlyLocalPlayersList)
            {
                var currentMaster = currentNetworkUser.master;
                if (currentMaster)
                {
                    var currentCharacterBody = currentMaster.GetBody();
                    if (currentCharacterBody)
                    {
                        var bodyIndex = currentCharacterBody.bodyIndex;

                        if (bodyIndex == EngiBodyIndex)
                        {
                            var grenadeSkillVariant =
                                currentNetworkUser.master.loadout.bodyLoadoutManager.GetSkillVariant(bodyIndex, 0);

                            if (grenadeSkillVariant == SkillLoader.SeekerSwarmSkillVariant)
                            {
                                var missileTracker = currentCharacterBody.GetComponent<MissileTracker>();
                                if (!missileTracker)
                                {
                                    currentCharacterBody.gameObject.AddComponent<MissileTracker>();
                                }
                            }
                            else if (grenadeSkillVariant == 0 ||
                                     grenadeSkillVariant == SkillLoader.SwappableGrenadeSkillVariant)
                            {
                                var missileTracker = currentCharacterBody.GetComponent<MissileTracker>();
                                if (missileTracker)
                                {
                                    Destroy(missileTracker);
                                }
                            }

                            var colorMsg = new EngiColorMsg
                            {
                                NetId = currentCharacterBody.GetComponent<NetworkIdentity>().netId
                            };

                            if (Configuration.CustomEngiColor.Value)
                            {
                                var rgb = Configuration.EngiColor.Value.Split(',');
                                colorMsg.Color = new Color(float.Parse(rgb[0]), float.Parse(rgb[1]), float.Parse(rgb[2]));
                            }
                            else
                            {
                                colorMsg.Color = new Color(-1, -1, -1);
                            }

                            colorMsg.Send(NetworkDestination.Clients);
                        }
                    }
                    else
                    {
                        Instance.StartCoroutine(DelayedEngiColorChangeAndUpdateGrenadeType(1));
                    }
                }
                else
                {
                    Instance.StartCoroutine(DelayedEngiColorChangeAndUpdateGrenadeType(1));
                }
            }
        }
    }
}
