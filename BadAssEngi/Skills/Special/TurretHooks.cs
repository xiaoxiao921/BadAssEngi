using System;
using BadAssEngi.Assets;
using BadAssEngi.Assets.Sound;
using BadAssEngi.Networking;
using EntityStates;
using EntityStates.Engi.EngiWeapon;
using EntityStates.EngiTurret.EngiTurretWeapon;
using EntityStates.Toolbot;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using R2API.Networking;
using R2API.Networking.Interfaces;
using R2API.Utils;
using RoR2;
using RoR2.CharacterAI;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.Networking;
using Random = UnityEngine.Random;

namespace BadAssEngi.Skills.Special
{
    internal static class TurretHooks
    {
        internal static void Init()
        {
            IL.EntityStates.EngiTurret.EngiTurretWeapon.FireGauss.OnEnter += TurretFiringHook;
            On.EntityStates.EngiTurret.EngiTurretWeapon.FireGauss.OnEnter += TurretFiringRocket;

            On.EntityStates.Engi.EngiWeapon.PlaceTurret.FixedUpdate += OnPlaceTurretSendTurretType;

            IL.RoR2.CharacterBody.SendConstructTurret += SendTurretColorHook;
            IL.RoR2.CharacterBody.HandleConstructTurret += TurretAddOwnerAndCustomColorHookAndTurretTypeHandler;

            On.RoR2.Orbs.InfusionOrb.OnArrival += InfusionStackSharedTurret;

            On.RoR2.CharacterMaster.AddDeployable += AddBATComponentOnAddDeployableHook;

            On.RoR2.CharacterAI.BaseAI.FixedUpdate += ChangeTurretTypeOnPing;
        }

        private static void TurretFiringHook(ILContext il)
        {
            var cursor = new ILCursor(il);
            var index = 0;
            BadAssTurret currentTurret = null;
            var soundMsg = new PlaySoundMsg();

            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Call, typeof(EntityState).GetMethodCached("get_characterBody"));
            cursor.Emit(OpCodes.Callvirt, typeof(CharacterBody).GetMethodCached("get_master"));

            cursor.EmitDelegate<Action<CharacterMaster>>(characterMaster =>
            {
                currentTurret = characterMaster.GetComponent<BadAssTurret>();
                if (currentTurret != null)
                {
                    new StopSoundMsg {soundPlayId = currentTurret.SoundGunId}.Send(NetworkDestination.Clients);
                    index = currentTurret.Index;
                }

            });

            cursor.GotoNext(
                i => i.MatchLdsfld("EntityStates.EngiTurret.EngiTurretWeapon.FireGauss", "attackSoundString")
            );
            cursor.Remove();

            cursor.EmitDelegate<Func<string>>( () =>
            {
                if (index == 1)
                {
                    soundMsg.NetId = currentTurret.gameObject.GetComponent<NetworkIdentity>().netId;
                    soundMsg.SoundName = SoundHelper.MiniGunTurretShot;
                    soundMsg.Send(NetworkDestination.Clients);
                    return "";
                }

                if (index == 2)
                {
                    soundMsg.NetId = currentTurret.gameObject.GetComponent<NetworkIdentity>().netId;
                    soundMsg.SoundName = SoundHelper.RailGunTurretShot;
                    soundMsg.Send(NetworkDestination.Clients);
                    return "";
                }

                return typeof(RoR2Application).Assembly
                    .GetType("EntityStates.EngiTurret.EngiTurretWeapon.FireGauss").GetFieldValue<string>("attackSoundString");
            });

            cursor.GotoNext(
                i => i.MatchCall<EntityState>("get_gameObject")
            );
            cursor.Index++;


            cursor.Emit(OpCodes.Ldstr, SoundHelper.TurretRTPCAttackSpeed);
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Ldfld, typeof(BaseState).GetFieldCached("attackSpeedStat"));

            cursor.GotoNext(
                i => i.MatchCall(typeof(RoR2.Util).GetMethodCached("PlaySound", new[] { typeof(string), typeof(GameObject) }))
            );
            cursor.Remove();
            cursor.Emit(OpCodes.Call, typeof(RoR2.Util).GetMethodCached("PlaySound", new[] { typeof(string), typeof(GameObject), typeof(string), typeof(float) }));

            cursor.GotoNext(
                i => i.MatchLdfld("EntityStates.EngiTurret.EngiTurretWeapon.FireGauss", "duration")
            );
            cursor.Index++;

            cursor.EmitDelegate<Func<float, float>>(duration =>
            {
                if (index == 1)
                {
                    return 0.0000001f;
                }

                return duration;
            });

            cursor.GotoNext(
                i => i.MatchLdsfld("EntityStates.EngiTurret.EngiTurretWeapon.FireGauss", "tracerEffectPrefab"),
                i => i.MatchStfld<BulletAttack>("tracerEffectPrefab")
                );
            cursor.Index++;

            cursor.EmitDelegate<Func<GameObject, GameObject>>(prefab =>
            {
                if (index == 1)
                {
                    prefab = BaeAssets.MiniGunPrefab;
                }

                if (index == 2)
                {
                    //new RebarColorMsg { Id = 3 }.Send(NetworkDestination.Clients);
                    prefab = BaeAssets.RailGunPrefab;
                }

                return prefab;
            });
            cursor.Index += 2;


            // we setting the bulletattack instance fields here
            //
            cursor.Emit(OpCodes.Ldc_R4, Configuration.RailgunTurretMaxDistanceTargeting.Value + 1f);
            cursor.Emit(OpCodes.Callvirt, typeof(BulletAttack).GetMethodCached("set_maxDistance"));
            cursor.Emit(OpCodes.Dup);

            cursor.Emit(OpCodes.Ldc_R4, 0f);
            cursor.EmitDelegate<Func<float, float>>(spread =>
            {
                if (index == 1)
                {
                    spread = 1f;
                }

                return spread;
            });
            cursor.Emit(OpCodes.Stfld, typeof(BulletAttack).GetFieldCached("spreadPitchScale"));
            cursor.Emit(OpCodes.Dup);

            cursor.Emit(OpCodes.Ldc_R4, 0f);
            cursor.EmitDelegate<Func<float, float>>(spread =>
            {
                if (index == 1)
                {
                    spread = 1f;
                }

                return spread;
            });
            cursor.Emit(OpCodes.Stfld, typeof(BulletAttack).GetFieldCached("spreadYawScale"));
            cursor.Emit(OpCodes.Dup);

            cursor.Emit(OpCodes.Ldc_R4, 1f);
            cursor.EmitDelegate<Func<float, float>>(procCoefficient =>
            {
                if (index == 1)
                {
                    procCoefficient = Configuration.MinigunTurretProcCoefficient.Value;
                }

                return procCoefficient;
            });
            cursor.Emit(OpCodes.Stfld, typeof(BulletAttack).GetFieldCached("procCoefficient"));
            cursor.Emit(OpCodes.Dup);

            cursor.GotoPrev(MoveType.After,
                i => i.MatchLdsfld<FireGauss>("damageCoefficient"));
            cursor.EmitDelegate<Func<float, float>>(damageCoefficient =>
            {
                if (index == 0)
                {
                    damageCoefficient = Configuration.DefaultTurretDamageCoefficient.Value;
                }
                else if (index == 1)
                {
                    damageCoefficient = Configuration.MinigunTurretDamageCoefficient.Value;
                }
                else if (index == 2)
                {
                    damageCoefficient = Configuration.RailgunTurretDamageCoefficient.Value;
                }
                else if (index == 3)
                {
                    damageCoefficient = Configuration.ShotgunTurretDamageCoefficient.Value;
                }

                return damageCoefficient;
            });
        }

        private static void TurretFiringRocket(On.EntityStates.EngiTurret.EngiTurretWeapon.FireGauss.orig_OnEnter orig, FireGauss self)
        {
            if (!NetworkServer.active)
            {
                orig(self);
                return;
            }

            var turretIndex = self.outer.commonComponents.characterBody.master.GetComponent<BadAssTurret>().Index;
            if (turretIndex != 3)
            {
                orig(self);
                return;
            }

            var ptr = typeof(BaseState).GetMethod("OnEnter").MethodHandle.GetFunctionPointer();
            var baseOnEnter = (Action)Activator.CreateInstance(typeof(Action), self, ptr);
            baseOnEnter();

            self.duration = 1f / Configuration.ShotgunTurretAttackSpeed.Value;
            RoR2.Util.PlaySound(SoundHelper.RocketTurretShot, self.outer.gameObject);
            Ray aimRay = self.GetAimRay();
            self.StartAimMode(aimRay);
            self.PlayAnimation("Gesture", "FireGauss", "FireGauss.playbackRate", self.duration);

            var bulletCount = (int)(Configuration.ShotgunTurretBaseProjectileNumber.Value + (self.attackSpeedStat / Configuration.ShotgunTurretAttackSpeed.Value - 1));
            var trajArray = new Vector3[bulletCount];
            Vector3 axis = Vector3.Cross(Vector3.up, aimRay.direction);

            if (self.isAuthority)
            {
                int i = 0;
                while (i < bulletCount)
                {
                    float x = Random.Range(0f, Configuration.ShotgunTurretSpreadCoefficient.Value); // right parameter dictate maxbloom angle / spread coefficient basically
                    float z = Random.Range(0f, 360f);
                    Vector3 vector = Quaternion.Euler(0f, 0f, z) * (Quaternion.Euler(x, 0f, 0f) * Vector3.forward);
                    float y = vector.y;
                    vector.y = 0f;
                    float yaw = (Mathf.Atan2(vector.z, vector.x) * 57.29578f - 90f) * (BaseNailgunState.spreadYawScale * BaseNailgunState.spreadYawScale);
                    float pitch = Mathf.Atan2(y, vector.magnitude) * 57.29578f * (BaseNailgunState.spreadPitchScale * BaseNailgunState.spreadPitchScale);
                    trajArray[i] = Quaternion.AngleAxis(yaw, Vector3.up) * (Quaternion.AngleAxis(pitch, axis) * aimRay.direction);
                    i++;
                }

                i = 0;
                while (i < bulletCount)
                {
                    ProjectileManager.instance.FireProjectile(BaeAssets.PrefabEngiTurretRocket, aimRay.origin,
                        RoR2.Util.QuaternionSafeLookRotation(trajArray[i]), self.outer.gameObject,
                        self.damageStat, 0f,
                        RoR2.Util.CheckRoll(self.critStat, self.characterBody.master));
                    i++;
                }
            }
        }


        // Morris original code - Customised for updating already placed Turret Type
        // This isnt clean, will need to redo that at some point, probably hooking SetCurrentPing or something
        private static void ChangeTurretTypeOnPing(On.RoR2.CharacterAI.BaseAI.orig_FixedUpdate orig, BaseAI self)
        {
            if (!self) return;
            if (self.leader.characterBody != null)
            {
                var leader = self.leader;

                if (leader.characterBody.isPlayerControlled)
                {
                    PingerController pingerController = null;
                    foreach (var playerCharacterMasterController in PlayerCharacterMasterController.instances)
                    {
                        if (playerCharacterMasterController.master.hasBody)
                        {
                            var cb = leader.characterBody;
                            var cm = cb.master;
                            var turretSkillVariant = cm.loadout.bodyLoadoutManager.GetSkillVariant(cb.bodyIndex, 3);
                            if (playerCharacterMasterController.master.GetBody().netId.Value == leader.characterBody.netId.Value &&
                                turretSkillVariant == SkillLoader.SwappableTurretSkillVariant)
                            {
                                pingerController = playerCharacterMasterController.pingerController;
                            }
                        }
                    }

                    if (pingerController)
                    {
                        // ReSharper disable once PossibleNullReferenceException
                        if (pingerController.gameObject && pingerController.pingIndicator)
                        {
                            if (pingerController.currentPing.targetNetworkIdentity)
                            {
                                if (pingerController.currentPing.targetNetworkIdentity.gameObject != null)
                                {
                                    if (pingerController.currentPing.targetNetworkIdentity.gameObject.name.Equals("EngiTurretBody(Clone)"))
                                    {
                                        TurretTypeController.SetCurrentTurretType(
                                            TurretTypeController.GiveNextTurretTypeExternalToInternal(pingerController
                                                .currentPing.targetNetworkIdentity
                                                .gameObject.GetComponent<CharacterBody>().baseNameToken),
                                            pingerController.currentPing.targetNetworkIdentity.gameObject,
                                            pingerController.currentPing.targetNetworkIdentity.gameObject
                                                .GetComponent<CharacterBody>().master.gameObject);

                                        pingerController.pingIndicator.fixedTimer = 0f;
                                        pingerController.currentPing = new PingerController.PingInfo();
                                    }
                                }
                            }
                        }
                    }
                }
            }

            orig(self);
        }

        private static void TurretAddOwnerAndCustomColorHookAndTurretTypeHandler(ILContext il)
        {
            var cursor = new ILCursor(il);
            var ctmLoc = 0;

            cursor.GotoNext(
                i => i.MatchLdarg(0),
                i => i.MatchCallvirt<NetworkMessage>("ReadMessage"),
                i => i.MatchStloc(out ctmLoc)
            );
            cursor.Index += 3;
            cursor.Emit(OpCodes.Ldloc, ctmLoc);
            cursor.EmitDelegate<Action<ConstructTurretMessage>>(constructTurretMessage =>
            {
                var turretType = RoR2Application.isInSinglePlayer
                    ? TurretTypeController.CurrentTurretType
                    : TurretTypeController.SenderTurretType;

                if (!Application.isBatchMode && CameraRigController.readOnlyInstancesList[0].localUserViewer.cachedBody)
                {
                    if (constructTurretMessage.builder.GetComponent<CharacterBody>().netId.Value == CameraRigController
                        .readOnlyInstancesList[0].localUserViewer.cachedBody.netId.Value)
                    {
                        turretType = TurretTypeController.CurrentTurretType;
                    }
                }

                TurretTypeController.SetTurretType(turretType);
            });


            cursor.GotoNext(i => i.MatchStloc(2)); // TODO: ??? safe to remove ?

            cursor.GotoNext(i => i.MatchStloc(4)); // TODO: clean this stloc
            cursor.EmitDelegate<Func<Deployable, Deployable>>(turret =>
            {
                var ai = turret.GetComponent<CharacterMaster>().GetBody().GetComponent<NetworkIdentity>().netId;
                var turretColorMsg = new TurretColorMsg
                {
                    NetId = ai,
                    Color = TurretTypeController.LatestTurretColorReceived
                };

                turretColorMsg.Send(NetworkDestination.Clients);

                return turret;
            });
        }

        private static void OnPlaceTurretSendTurretType(On.EntityStates.Engi.EngiWeapon.PlaceTurret.orig_FixedUpdate orig, PlaceTurret self)
        {
            if (self.turretMasterPrefab.gameObject.name.Equals(TurretTypeController.EngiTurretMasterName))
            {
                if (RoR2.Util.HasEffectiveAuthority(self.outer.networkIdentity))
                {
                    if (self.outer.commonComponents.inputBank && self.entryCountdown <= 0f)
                    {
                        if (self.outer.commonComponents.inputBank.skill1.down || self.outer.commonComponents.inputBank.skill4.justPressed)
                        {
                            if (self.outer.commonComponents.characterBody)
                            {
                                var cb = self.outer.commonComponents.characterBody;
                                var cm = cb.master;
                                var turretSkillVariant = cm.loadout.bodyLoadoutManager.GetSkillVariant(cb.bodyIndex, 3);
                                if (turretSkillVariant == 0)
                                    TurretTypeController.CurrentTurretType = TurretTypeController.TurretType.Default;

                                new TurretTypeMsgToServer {TurretTypeId = (byte)TurretTypeController.CurrentTurretType}
                                    .Send(NetworkDestination.Server);
                            }
                        }
                    }
                }
            }

            orig(self);
        }

        private static void SendTurretColorHook(ILContext il)
        {
            var cursor = new ILCursor(il);

            cursor.EmitDelegate<Action>(() =>
            {
                if (NetworkClient.active)
                {
                    var rgb = Configuration.TurretColor.Value.Split(',');
                    var color = Configuration.CustomTurretColor.Value ? new Color(float.Parse(rgb[0]), float.Parse(rgb[1]), float.Parse(rgb[2])) : new Color(-1, -1, -1);

                    new TurretColorDelayedMsgToServer { Color = color }.Send(NetworkDestination.Server);
                }
            });
        }

        private static void InfusionStackSharedTurret(On.RoR2.Orbs.InfusionOrb.orig_OnArrival orig, RoR2.Orbs.InfusionOrb self)
        {
            orig(self);

            var badAssTurret = self.targetInventory.gameObject.GetComponent<BadAssTurret>();
            if (badAssTurret)
            {
                var deployableInfos = badAssTurret.OwnerCharacterMaster.deployablesList;

                uint maxBonus = badAssTurret.OwnerCharacterMaster.inventory.infusionBonus;
                foreach (var deployableInfo in deployableInfos)
                {
                    var otherTurret = deployableInfo.deployable.GetComponent<BadAssTurret>();
                    if (otherTurret)
                    {
                        var turretMaster = otherTurret.GetComponent<CharacterMaster>();
                        if (turretMaster.inventory)
                        {
                            if (turretMaster.inventory.infusionBonus > maxBonus)
                                maxBonus = turretMaster.inventory.infusionBonus;
                            else
                            {
                                turretMaster.inventory.infusionBonus = maxBonus;
                                turretMaster.GetBody().RecalculateStats();
                            }
                        }
                    }
                }

                badAssTurret.OwnerCharacterMaster.inventory.infusionBonus = maxBonus;
                badAssTurret.OwnerCharacterMaster.GetBody().RecalculateStats();
            }
        }

        private static void AddBATComponentOnAddDeployableHook(On.RoR2.CharacterMaster.orig_AddDeployable orig, CharacterMaster self, Deployable deployable, DeployableSlot slot)
        {
            orig(self, deployable, slot);

            if (slot == DeployableSlot.EngiTurret)
            {
                var badAssTurret = deployable.gameObject.AddComponent<BadAssTurret>();

                badAssTurret.Index = TurretTypeController.LocalTurretPrefabIndex;
                badAssTurret.OwnerCharacterMaster = self;
                badAssTurret.Init();
            }
        }
    }
}
