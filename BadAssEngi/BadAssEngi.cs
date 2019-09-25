using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using EntityStates;
using EntityStates.Engi.Mine;
using MiniRpcLib;
using MiniRpcLib.Action;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using On.EntityStates.Engi.EngiWeapon;
using R2API;
using R2API.Utils;
using RoR2;
using RoR2.CharacterAI;
using RoR2.Projectile;
using RoR2.Skills;
using RoR2.UI;
using UnityEngine;
using UnityEngine.Networking;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

// ReSharper disable UnusedMember.Global


namespace BadAssEngi
{
    [BepInDependency(R2API.R2API.PluginGUID)]
    [BepInDependency(MiniRpcPlugin.Dependency)]
    [BepInDependency(AssetPlus.AssetPlus.modguid)]
    [BepInDependency("at.aster.charactercustomizer", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInPlugin(ModGuid, ModName, ModVer)]
    public class BadAssEngi : BaseUnityPlugin
    {
        private const string ModVer = "0.1.9";
        private const string ModName = "BadAssEngi";
        public const string ModGuid = "com.iDeathHD.BadAssEngi";

        private static BadAssEngi _instance;

        private static AssetBundle _assetBundle;
        private static GameObject _prefab;

        // Configuration File
        private static ConfigFile ConfigFile { get; set; }

        private static ConfigWrapper<string> TurretTypeKeyBind { get; set; }
        private static ConfigWrapper<string> GrenadeTypeKeyBind { get; set; }
        private static ConfigWrapper<string> MineTypeKeyBind { get; set; }
        private static ConfigWrapper<string> SatchelManualDetonateKeyBind { get; set; }

        private static ConfigWrapper<string> EngiColor { get; set; }
        private static ConfigWrapper<string> TurretColor { get; set; }
        private static ConfigWrapper<bool> CustomEngiColor { get; set; }
        private static ConfigWrapper<bool> CustomTurretColor { get; set; }

        private static ConfigWrapper<string> SharedBuffsWithTurret { get; set; }

        private static ConfigWrapper<float> DefaultTurretBaseDamage { get; set; }
        private static ConfigWrapper<float> DefaultTurretAttackSpeed { get; set; }
        private static ConfigWrapper<float> DefaultTurretDamagePerLevel { get; set; }
        private static ConfigWrapper<float> DefaultTurretMaxDistanceTargeting { get; set; }

        private static ConfigWrapper<float> MinigunTurretBaseDamage { get; set; }
        private static ConfigWrapper<float> MinigunTurretAttackSpeed { get; set; }
        private static ConfigWrapper<float> MinigunTurretDamagePerLevel { get; set; }
        private static ConfigWrapper<float> MinigunTurretMaxDistanceTargeting { get; set; }
        private static ConfigWrapper<float> MinigunTurretProcCoefficient { get; set; }

        private static ConfigWrapper<float> RailgunTurretBaseDamage { get; set; }
        private static ConfigWrapper<float> RailgunTurretAttackSpeed { get; set; }
        private static ConfigWrapper<float> RailgunTurretDamagePerLevel { get; set; }
        private static ConfigWrapper<float> RailgunTurretMaxDistanceTargeting { get; set; }

        private static ConfigWrapper<float> SeekerGrenadeDamageCoefficient { get; set; }
        private static ConfigWrapper<float> SeekerGrenadeMaxVelocity { get; set; }
        private static ConfigWrapper<float> SeekerGrenadeLifeTime { get; set; }

        private static ConfigWrapper<float> ClusterMineDamageCoefficient { get; set; }
        private static ConfigWrapper<int> ClusterMineBaseMaxStock { get; set; }
        private static ConfigWrapper<float> ClusterMineCooldown { get; set; }

        private static ConfigWrapper<float> SatchelMineDamageCoefficient { get; set; }
        private static ConfigWrapper<float> SatchelMineForce { get; set; }

        private static string _turretType, _senderTurretType, _grenadeType, _localMineType;
        private static int _localTurretPrefabIndex;
        private static GameObject _miniGunPrefab;
        private static GameObject _railGunPrefab; //_rebarMesh;
        private static Material _railGunTrailMaterial;
        private static Color _origRebarColor, _engiRebarColor;
        private static Texture2D _origEngiTexture, _origTurretTexture;
        private static readonly Transform[] CachedRebarEffects = new Transform[6];
        private static GameObject _minePrefab;
        private static int _rebarTracker;
        private static ConfigWrapper<int> MaximumRebar { get; set; }
        private static FieldInfo _grenadePrefab;

        // Network
        public static IRpcAction<ColorMsg> SendEngiColorToClient { get; set; }
        public static IRpcAction<ColorMsg> SendEngiColorToServer { get; set; }
        public static IRpcAction<ColorMsg> SendTurretColorToClient { get; set; }
        public static IRpcAction<ColorMsg> SendTurretColorToServer { get; set; }
        public static IRpcAction<SoundMsg> SendPlaySoundToClient { get; set; }
        public static IRpcAction<SoundMsg> SendPlaySoundToClientAndDestroy { get; set; }
        public static IRpcAction<uint> SendStopSoundToClient { get; set; }
        public static IRpcAction<int> SendRebarToClient { get; set; }
        public static IRpcAction<int> SendRebarToServer { get; set; }
        public static IRpcAction<int> SendDetonateRequestToServer { get; set; }
        public static IRpcAction<int> SendMineTypeToServer { get; set; }
        public static IRpcAction<string> SendTurretTypeToServer { get; set; }

        public BadAssEngi()
        {
            _instance = this;

            InitConfig();
            InitSounds();
            InitBadAssEngi();
            InitNetworking();
            InitHooks();
        }

        public void Update()
        {
            if (!Run.instance)
                return;

            if (Input.GetKeyDown(TurretTypeKeyBind.Value))
            {
                switch (_turretType)
                {
                    case "Default":
                        _turretType = "Minigun";
                        Chat.AddMessage("<style=cIsUtility>Turret Type is now: </style><style=cDeath>[Minigun]</style>");
                        break;
                    case "Minigun":
                        _turretType = "Railgun";
                        Chat.AddMessage("<style=cIsUtility>Turret Type is now: </style><style=cDeath>[Railgun]</style>");
                        break;
                    case "Railgun":
                        _turretType = "Default";
                        Chat.AddMessage("<style=cIsUtility>Turret Type is now: </style><style=cDeath>[WeakBoy]</style>");
                        break;
                }
            }

            if (Input.GetKeyDown(GrenadeTypeKeyBind.Value))
            {
                switch (_grenadeType)
                {
                    case "Default":
                        if (!Resources.Load<GameObject>("prefabs/projectiles/engiseekergrenadeprojectile")
                            .GetComponent<SphereCollider>())
                        {
                            Resources.Load<GameObject>("prefabs/projectiles/engiseekergrenadeprojectile").AddComponent<SphereCollider>();
                        }

                        var missileController = Resources.Load<GameObject>("prefabs/projectiles/engiseekergrenadeprojectile").GetComponent<MissileController>();
                        
                        missileController.deathTimer = SeekerGrenadeLifeTime.Value;
                        missileController.delayTimer = 0.2f;
                        missileController.maxVelocity = SeekerGrenadeMaxVelocity.Value;
                        missileController.deathTimer -= 2f;

                        _grenadeType = "Seeker";

                        _grenadePrefab.SetValue(null, Resources.Load<GameObject>("prefabs/projectiles/engiseekergrenadeprojectile"));
                        var engiseekergrenadeprojectile =
                            Resources.Load<GameObject>("prefabs/projectiles/engiseekergrenadeprojectile");
                        var stopSound = engiseekergrenadeprojectile.GetComponent<StopSound>();
                        if (!stopSound)
                            engiseekergrenadeprojectile.AddComponent<StopSound>();
                        engiseekergrenadeprojectile.GetComponent<AkEvent>().data = null;

                        typeof(RoR2Application).Assembly.GetType("EntityStates.Engi.EngiWeapon.FireGrenades").SetFieldValue("damageCoefficient", SeekerGrenadeDamageCoefficient.Value);
                        typeof(RoR2Application).Assembly.GetType("EntityStates.Engi.EngiWeapon.ChargeGrenades")
                            .SetFieldValue("minGrenadeCount", 2);
                        typeof(RoR2Application).Assembly.GetType("EntityStates.Engi.EngiWeapon.ChargeGrenades")
                            .SetFieldValue("maxGrenadeCount", 16);

                        Chat.AddMessage("<style=cIsUtility>Grenade Type is now: </style><style=cDeath>[Seeker]</style>");
                        break;
                    case "Seeker":
                        _grenadeType = "Default";

                        _grenadePrefab.SetValue(null, Resources.Load<GameObject>("prefabs/projectiles/engigrenadeprojectile"));
                        typeof(RoR2Application).Assembly.GetType("EntityStates.Engi.EngiWeapon.FireGrenades").SetFieldValue("damageCoefficient", 1f);
                        typeof(RoR2Application).Assembly.GetType("EntityStates.Engi.EngiWeapon.ChargeGrenades")
                            .SetFieldValue("minGrenadeCount", 2);
                        typeof(RoR2Application).Assembly.GetType("EntityStates.Engi.EngiWeapon.ChargeGrenades")
                            .SetFieldValue("maxGrenadeCount", 8);

                        Chat.AddMessage("<style=cIsUtility>Grenade Type is now: </style><style=cDeath>[Default]</style>");
                        break;
                }
            }

            if (Input.GetKeyDown(MineTypeKeyBind.Value))
            {
                var recursiveMine = _minePrefab.GetComponent<RecursiveMine>();
                switch (_localMineType)
                {
                    case "Satchel":
                        _localMineType = "Cluster";

                        if (!recursiveMine)
                        {
                            _minePrefab.AddComponent<RecursiveMine>().Init();
                        }
                        //typeof(RoR2Application).Assembly.GetType("EntityStates.Engi.EngiWeapon.FireMines")
                        //    .SetFieldValue("damageCoefficient", ClusterMineDamageCoefficient.Value);

                        Chat.AddMessage("<style=cIsUtility>Mine Type is now: </style><style=cDeath>[Cluster]</style>");
                        break;
                    case "Cluster":
                        _localMineType = "Satchel";

                        if (recursiveMine)
                        {
                            DestroyImmediate(recursiveMine);
                        }
                        //typeof(RoR2Application).Assembly.GetType("EntityStates.Engi.EngiWeapon.FireMines")
                        //    .SetFieldValue("damageCoefficient", SatchelMineDamageCoefficient.Value);

                        Chat.AddMessage("<style=cIsUtility>Mine Type is now: </style><style=cDeath>[Satchel]</style>");
                        break;
                }
            }

            if (Input.GetKeyDown(SatchelManualDetonateKeyBind.Value))
            {
                //LoadAssets();
                //var currentBody = LocalUserManager.GetFirstLocalUser().currentNetworkUser.master.GetBody();

                //var test = Instantiate(_prefab, currentBody.transform.position, Quaternion.identity);
                //var master = gameObject.GetComponent<CharacterMaster>();
                //NetworkServer.Spawn(gameObject);
                //master.SpawnBody(test, currentBody.transform.position, Quaternion.identity);

                if (NetworkServer.active)
                {
                    var deployableInfos = LocalUserManager.GetFirstLocalUser().currentNetworkUser.master.GetFieldValue<List<DeployableInfo>>("deployablesList");

                    if (deployableInfos != null && deployableInfos.Count >= 1)
                    {
                        foreach (var deployableInfo in deployableInfos)
                        {
                            if (deployableInfo.slot == DeployableSlot.EngiMine &&
                                !deployableInfo.deployable.GetComponent<RecursiveMine>())
                            {
                                EntityStateMachine.FindByCustomName(deployableInfo.deployable.gameObject, "Main").SetNextState(new Detonate());
                                EntityStateMachine.FindByCustomName(deployableInfo.deployable.gameObject, "Arming").SetNextState(new MineArmingFull());
                            }
                                
                            /*if (deployableInfo.slot == DeployableSlot.EngiTurret)
                            {
                                foreach (var rendererInfo in deployableInfo.deployable.GetComponent<CharacterModel>().baseRendererInfos)
                                {
                                    foreach (var material in rendererInfo.renderer.materials)
                                    {
                                        material.color = Color.red;
                                    }
                                }
                            }*/
                        }
                    }
                }
                else
                {
                    SendDetonateRequestToServer.Invoke(0);
                }
            }
        }

        private static void InitConfig()
        {
            ConfigFile = new ConfigFile(Paths.ConfigPath + "\\BadAssEngi.cfg", true);

            const string defaultTurretTypeKeyBind = "f1";
            TurretTypeKeyBind = ConfigFile.Wrap("Keybinds", "TurretType",
                "What keybind should be used for changing the type of turret. List of possible keybind at the end of this page : https://docs.unity3d.com/Manual/ConventionalGameInput.html", defaultTurretTypeKeyBind);

            const string defaultGrenadeTypeKeyBind = "f3";
            GrenadeTypeKeyBind = ConfigFile.Wrap("Keybinds", "GrenadeType",
                "What keybind should be used for changing the type of grenades.", defaultGrenadeTypeKeyBind);

            const string defaultMineTypeKeyBind = "f4";
            MineTypeKeyBind = ConfigFile.Wrap("Keybinds", "MineType",
                "What keybind should be used for changing the type of mines.", defaultMineTypeKeyBind);

            const string defaultSatchelManualDetonateKeyBind = "c";
            SatchelManualDetonateKeyBind = ConfigFile.Wrap("Keybinds", "Satchel Manual Detonate",
                "What keybind should be used for manually detonating satchel mines.", defaultSatchelManualDetonateKeyBind);


            const bool defaultEnableEngiColor = false;
            CustomEngiColor = ConfigFile.Wrap("Colors", "Enable Custom Engi Color",
                "Should the Engi have his custom colors activated ?", defaultEnableEngiColor);

            const bool defaultEnableTurretColor = true;
            CustomTurretColor = ConfigFile.Wrap("Colors", "Enable Custom Turret Color",
                "Should the Engi's Turrets have its custom colors activated ?", defaultEnableTurretColor);

            string defaultEngiColor = string.Join(",", new[]
            {
                1f, 5f, 1f
            });
            EngiColor = ConfigFile.Wrap("Colors", "Engineer",
                "What color should the Engineer be. Example : 7.5 8 18.6", defaultEngiColor);

            string defaultTurretColor = string.Join(",", new[]
            {
                0f, 80f, 0f
            });
            TurretColor = ConfigFile.Wrap("Colors", "Turret",
                "What color should the Turrets be.", defaultTurretColor);


            // https://pastebin.com/EsYMneGY
            string defaultSharedBuffsWithTurret = string.Join(",", new[]
            {
                10,
                11,
                17,
                27,
                28,
                29,
                30,
                31,
                33
            });
            SharedBuffsWithTurret = ConfigFile.Wrap("Shared", "Buffs",
                    "What buffs should be shared between the Engineer and its turrets. https://pastebin.com/EsYMneGY", defaultSharedBuffsWithTurret);


            const float defaultDefaultTurretBaseDamage = 16f;
            DefaultTurretBaseDamage = ConfigFile.Wrap("Default Turret", "Base Damage",
                "How much base damage should deal the Default Turret.", defaultDefaultTurretBaseDamage);

            const float defaultDefaultTurretAttackSpeed = 1f;
            DefaultTurretAttackSpeed = ConfigFile.Wrap("Default Turret", "Attack Speed",
                "How much attack speed should the Default Turret have.", defaultDefaultTurretAttackSpeed);

            const float defaultDefaultTurretDamagePerLevel = 3.8f;
            DefaultTurretDamagePerLevel = ConfigFile.Wrap("Default Turret", "Base Damage per Level",
                "How much base damage should the Default Turret get per level.", defaultDefaultTurretDamagePerLevel);

            const float defaultDefaultTurretMaxDistanceTargeting = 120f;
            DefaultTurretMaxDistanceTargeting = ConfigFile.Wrap("Default Turret", "Max Distance Targeting",
                "How far should the Default Turret target monsters.", defaultDefaultTurretMaxDistanceTargeting);


            const float defaultMinigunTurretBaseDamage = 2.15f;
            MinigunTurretBaseDamage = ConfigFile.Wrap("Minigun Turret", "Base Damage",
                "How much base damage should the Minigun Turret deal.", defaultMinigunTurretBaseDamage);

            const float defaultMinigunTurretAttackSpeed = 6.9166f;
            MinigunTurretAttackSpeed = ConfigFile.Wrap("Minigun Turret", "Attack Speed",
                "How much attack speed should the Minigun Turret have.", defaultMinigunTurretAttackSpeed);

            const float defaultMinigunTurretDamagePerLevel = 0.43f;
            MinigunTurretDamagePerLevel = ConfigFile.Wrap("Minigun Turret", "Base Damage per Level",
                "How much base damage should the Minigun Turret get per level.", defaultMinigunTurretDamagePerLevel);

            const float defaultMinigunTurretMaxDistanceTargeting = 80f;
            MinigunTurretMaxDistanceTargeting = ConfigFile.Wrap("Minigun Turret", "Max Distance Targeting",
                "How far should the Minigun Turret target monsters.", defaultMinigunTurretMaxDistanceTargeting);

            const float defaultMinigunTurretProcCoefficient = 0.5f;
            MinigunTurretProcCoefficient = ConfigFile.Wrap("Minigun Turret", "Proc Coefficient",
                "Defines how strong or frequent procs happen on an on-hit basis (0 to 1).", defaultMinigunTurretProcCoefficient);


            const float defaultRailgunTurretBaseDamage = 116f;
            RailgunTurretBaseDamage = ConfigFile.Wrap("Railgun Turret", "Base Damage",
                "How much base damage should the Railgun Turret deal.", defaultRailgunTurretBaseDamage);

            const float defaultRailgunTurretAttackSpeed = 0.1f;
            RailgunTurretAttackSpeed = ConfigFile.Wrap("Railgun Turret", "Attack Speed",
                "How much attack speed should the Railgun Turret have.", defaultRailgunTurretAttackSpeed);

            const float defaultRailgunTurretDamagePerLevel = 23.2f;
            RailgunTurretDamagePerLevel = ConfigFile.Wrap("Railgun Turret", "Base Damage per Level",
                "How much base damage should the Railgun Turret get per level.", defaultRailgunTurretDamagePerLevel);

            const float defaultRailgunTurretMaxDistanceTargeting = 99999f;
            RailgunTurretMaxDistanceTargeting = ConfigFile.Wrap("Railgun Turret", "Max Distance Targeting",
                "How far should the Railgun Turret target monsters.", defaultRailgunTurretMaxDistanceTargeting);


            const float defaultSeekerGrenadeDamageCoefficient = 0.125f;
            SeekerGrenadeDamageCoefficient = ConfigFile.Wrap("Seeker Grenades", "Damage Coefficient",
                "By how much the damage should be multiplied by.", defaultSeekerGrenadeDamageCoefficient);

            const float defaultSeekerGrenadeMaxVelocity = 40f;
            SeekerGrenadeMaxVelocity = ConfigFile.Wrap("Seeker Grenades", "Max Velocity",
                "How fast should the seeker grenades go.", defaultSeekerGrenadeMaxVelocity);

            const float defaultSeekerGrenadeLifeTime = 30f;
            SeekerGrenadeLifeTime = ConfigFile.Wrap("Seeker Grenades", "Life Time",
                "How long should the grenades should seek a target before dropping. (in seconds)", defaultSeekerGrenadeLifeTime);


            const float defaultClusterMineDamageCoefficient = 0.0355f;
            ClusterMineDamageCoefficient = ConfigFile.Wrap("Cluster Mine", "Damage Coefficient",
                "By how much the damage should be multiplied by.", defaultClusterMineDamageCoefficient);

            const int defaultClusterMineBaseMaxStock = 3;
            ClusterMineBaseMaxStock = ConfigFile.Wrap("Cluster Mine", "Base Stock",
                "How many stock charge should the Engineer get by default.", defaultClusterMineBaseMaxStock);

            const float defaultClusterMineCooldown = 20f;
            ClusterMineCooldown = ConfigFile.Wrap("Cluster Mine", "Cooldown",
                "How long the cooldown between each recharges should be.", defaultClusterMineCooldown);

            const int defaultMaximumRebar = 700;
            MaximumRebar = ConfigFile.Wrap("Cluster Mine", "Maximum Rebar",
                "How many rebars should be allowed to be in a scene at the same time.", defaultMaximumRebar);


            const float defaultSatchelMineDamageCoefficient = 3.0f;
            SatchelMineDamageCoefficient = ConfigFile.Wrap("Satchel Mine", "Damage Coefficient",
                "By how much the damage should be multiplied by.", defaultSatchelMineDamageCoefficient);

            const float defaultSatchelMineForce = 3000f;
            SatchelMineForce = ConfigFile.Wrap("Satchel Mine", "Force",
                "How strong the force vector should be.", defaultSatchelMineForce);
        }

        [ConCommand(commandName = "bae_reload", flags = ConVarFlags.None, helpText = "Reload the config file of Bad Ass Engi.")]
        private static void CCReloadConfig(ConCommandArgs args)
        {
            ConfigFile.Reload();
        }

        [ConCommand(commandName = "bae_engi_color", flags = ConVarFlags.None, helpText = "Change, enable or disable the custom color of your Bad Ass Engineer. Usage : bae_engi_color R G B / bae_engi_color enable/disable. Exemple Usage : bae_engi_color 7 76 3.75 . Or just \"bae_engi_color\" to force a color update.")]
        private static void CCChangeEngiColor(ConCommandArgs args)
        {
            if (args.Count == 1)
            {
                if (args[0].ToLower().Equals("disable"))
                {
                    CustomEngiColor.Value = false;
                } 
                else if (args[0].ToLower().Equals("enable"))
                {
                    CustomEngiColor.Value = true;
                }
            }

            if (Run.instance)
            {
                if (args.Count == 3)
                {
                    EngiColor.Value = string.Join(",", args[0].ToString(CultureInfo.InvariantCulture), args[1].ToString(CultureInfo.InvariantCulture), args[2].ToString(CultureInfo.InvariantCulture));
                    ConfigFile.Save();

                    if (!CustomEngiColor.Value)
                        return;

                    var rgb = EngiColor.Value.Split(',');
                    var colorMsg = new ColorMsg
                    {
                        Color = new Color(float.Parse(rgb[0]), float.Parse(rgb[1]), float.Parse(rgb[2])),
                        NetId = CameraRigController.readOnlyInstancesList.First().viewer.master.GetBody().gameObject.GetComponent<NetworkIdentity>().netId
                    };
                    
                    SendEngiColorToServer.Invoke(colorMsg);

                    return;
                }

                if (args.Count == 0 || args.Count == 1)
                {
                    var colorMsg = new ColorMsg
                    {
                        NetId = CameraRigController.readOnlyInstancesList.First().viewer.master.GetBody()
                            .gameObject.GetComponent<NetworkIdentity>().netId
                    };

                    if (!CustomEngiColor.Value)
                    {
                        colorMsg.Color = new Color(-1, -1f, -1);
                        SendEngiColorToServer.Invoke(colorMsg);
                    }
                    else
                    {
                        var rgb = EngiColor.Value.Split(',');
                        colorMsg.Color = new Color(float.Parse(rgb[0]), float.Parse(rgb[1]), float.Parse(rgb[2]));
                    }

                    SendEngiColorToServer.Invoke(colorMsg);
                    return;
                }

                Debug.Log("Usage : bae_engi_color R G B  / bae_engi_color enable/disable. Exemple Usage : bae_engi_color 7 76 3.75. Or just \"bae_engi_color\" to force a color update.");
            }
            else
            {
                Debug.Log("Please launch a game run to use this command.");
            }
        }

        [ConCommand(commandName = "bae_turret_color", flags = ConVarFlags.None, helpText = "Change, enable or disable the custom color of your turrets, disabling restore original turret's color. Usage : bae_turret_color R G B / bae_turret_color enable/disable. Exemple Usage : bae_turret_color 7 76 3.75")]
        private static void CCChangeTurretColor(ConCommandArgs args)
        {
            if (args.Count == 3)
            {
                TurretColor.Value = string.Join(",", args[0].ToString(CultureInfo.InvariantCulture), args[1].ToString(CultureInfo.InvariantCulture), args[2].ToString(CultureInfo.InvariantCulture));
            }
            else if (args.Count == 1)
            {
                if (args[0].ToLower().Equals("disable"))
                {
                    CustomTurretColor.Value = false;
                }
                else if (args[0].ToLower().Equals("enable"))
                {
                    CustomTurretColor.Value = true;
                }
            }

            ConfigFile.Save();
        }

        private static void InitSounds()
        {
            SoundHelper.AddSoundBank();
        }

        private static void InitNetworking()
        {
            var miniRpc = MiniRpc.CreateInstance(ModGuid);

            SendEngiColorToClient = miniRpc.RegisterAction(Target.Client, (NetworkUser user, ColorMsg data) =>
            {
                // Executed by all clients

                var characterBody = ClientScene.FindLocalObject(data.NetId).GetComponent<CharacterBody>();
                var characterModel = characterBody.modelLocator.modelTransform
                    .GetComponent<CharacterModel>();

                var colorVec = new Vector3(data.Color.r, data.Color.g, data.Color.b);
                var originalColor = colorVec.x == -1 && colorVec.y == -1 && colorVec.z == -1;

                foreach (var rendererInfo in characterModel.baseRendererInfos)
                {
                    var material = rendererInfo.defaultMaterial;
                    var texture = material.GetTexture(3) as Texture2D;

                    if (texture != null)
                    {
                        if (_origEngiTexture == null)
                        {
                            var tmp = RenderTexture.GetTemporary(texture.width, texture.height);
                            Graphics.Blit(texture, tmp);
                            var previous = RenderTexture.active;
                            RenderTexture.active = tmp;
                            _origEngiTexture = new Texture2D(texture.width, texture.height);
                            _origEngiTexture.ReadPixels(new Rect(0, 0, tmp.width, tmp.height), 0, 0);
                            _origEngiTexture.Apply();
                            RenderTexture.active = previous;
                            RenderTexture.ReleaseTemporary(tmp);
                        }

                        if (originalColor)
                        {
                            material.SetTexture(3, _origEngiTexture);
                        }
                        else
                        {
                            material.SetTexture(3, ReplaceWithRamp(texture, colorVec, 0f));
                        }

                        break;
                    }
                }

            });

            SendEngiColorToServer = miniRpc.RegisterAction(Target.Server, (NetworkUser user, ColorMsg data) =>
            {
                // Executed by server

                SendEngiColorToClient.Invoke(data);
            });

            SendTurretColorToClient = miniRpc.RegisterAction(Target.Client, (NetworkUser user, ColorMsg data) =>
            {
                // Executed by all clients

                var characterBody = ClientScene.FindLocalObject(data.NetId).GetComponent<CharacterMaster>().GetBody();

                var characterModel = characterBody.modelLocator.modelTransform
                            .GetComponent<CharacterModel>();

                var colorVec = new Vector3(data.Color.r, data.Color.g, data.Color.b);
                var originalColor = colorVec.x == -1 && colorVec.y == -1 && colorVec.z == -1;

                foreach (var rendererInfo in characterModel.baseRendererInfos)
                {
                    var material = rendererInfo.defaultMaterial;
                    var texture = material.GetTexture(3) as Texture2D;

                    if (texture != null)
                    {
                        if (_origTurretTexture == null)
                        {
                            var tmp = RenderTexture.GetTemporary(texture.width, texture.height);
                            Graphics.Blit(texture, tmp);
                            var previous = RenderTexture.active;
                            RenderTexture.active = tmp;
                            _origTurretTexture = new Texture2D(texture.width, texture.height);
                            _origTurretTexture.ReadPixels(new Rect(0, 0, tmp.width, tmp.height), 0, 0);
                            _origTurretTexture.Apply();
                            RenderTexture.active = previous;
                            RenderTexture.ReleaseTemporary(tmp);
                        }

                        if (originalColor)
                        {
                            material.SetTexture(3, _origTurretTexture);
                        }
                        else
                        {
                            material.SetTexture(3, ReplaceWithRamp(texture, colorVec, -15f));
                        }

                        break;
                    }
                }

            });

            SendTurretColorToServer = miniRpc.RegisterAction(Target.Server, (NetworkUser user, ColorMsg data) =>
            {
                // Executed by server

                SendTurretColorToClient.Invoke(data);
            });

            SendPlaySoundToClient = miniRpc.RegisterAction(Target.Client, (NetworkUser user, SoundMsg data) =>
            {
                // Executed by all clients

                var soundEmitter = ClientScene.FindLocalObject(data.NetId);
                if (soundEmitter)
                {
                    AkSoundEngine.PostEvent(data.SoundName, soundEmitter);

                    // ugly but easiest fix for sound not firing
                    if (soundEmitter.name.Equals("EngiMine(Clone)") || soundEmitter.name.Equals("EngiSeekerGrenadeProjectile(Clone)") || soundEmitter.name.Equals("EngiGrenadeProjectile(Clone)"))
                        Destroy(soundEmitter);
                }
                
            });

            SendPlaySoundToClientAndDestroy = miniRpc.RegisterAction(Target.Client, (NetworkUser user, SoundMsg data) =>
            {
                // Executed by all clients

                var soundEmitter = ClientScene.FindLocalObject(data.NetId);
                if (soundEmitter)
                {
                    AkSoundEngine.PostEvent(data.SoundName, soundEmitter);
                    Destroy(soundEmitter);
                }

            });

            SendStopSoundToClient = miniRpc.RegisterAction(Target.Client, (NetworkUser user, uint data) =>
            {
                // Executed by all clients

                AkSoundEngine.StopPlayingID(data);

            });

            // This command will be called by the host, and executed on all clients
            SendRebarToClient = miniRpc.RegisterAction(Target.Client, (NetworkUser user, int id) =>
            {
                // This is what all clients will execute when the server invokes the IRpcAction

                if (id == 1)
                {
                    _rebarTracker++;
                    _railGunTrailMaterial.SetColor(152, _engiRebarColor);
                    _instance.StartCoroutine(CleanRebarObject(2.25f));
                }

                if (id == 2)
                {
                    _railGunTrailMaterial.SetColor(152, _origRebarColor);
                }

                if (id == 3)
                {
                    _railGunTrailMaterial.SetColor(152, _engiRebarColor);
                }

            });

            SendRebarToServer = miniRpc.RegisterAction(Target.Server, (NetworkUser user, int id) =>
            {
                // Executed by server

                if (id == 2)
                {
                    SendRebarToClient.Invoke(2);
                }

            });

            // This command will be called by the client, and executed on server
            SendDetonateRequestToServer = miniRpc.RegisterAction(Target.Server, (NetworkUser user, int id) =>
            {
                // Executed by server

                if (id == 0)
                {
                    foreach (var networkUser in NetworkUser.readOnlyInstancesList)
                    {
                        var deployableInfos = networkUser.master.GetFieldValue<List<DeployableInfo>>("deployablesList");

                        if (deployableInfos != null && deployableInfos.Count >= 1)
                        {
                            foreach (var deployableInfo in deployableInfos)
                            {
                                if (deployableInfo.slot == DeployableSlot.EngiMine && !deployableInfo.deployable.GetComponent<RecursiveMine>())
                                {
                                    EntityStateMachine.FindByCustomName(deployableInfo.deployable.gameObject, "Main").SetNextState(new Detonate());
                                    EntityStateMachine.FindByCustomName(deployableInfo.deployable.gameObject, "Arming").SetNextState(new MineArmingFull());
                                }
                            }
                        }
                    }
                }
            });

            // This command will be called by the client, and executed on server
            SendMineTypeToServer = miniRpc.RegisterAction(Target.Server, (NetworkUser user, int id) =>
            {
                // Executed by server

                if (id == 0)
                {
                    if (!_minePrefab.GetComponent<RecursiveMine>())
                    {
                        _minePrefab.AddComponent<RecursiveMine>().Init();
                    }
                }
                else
                {
                    if (_minePrefab.GetComponent<RecursiveMine>())
                    {
                        DestroyImmediate(_minePrefab.GetComponent<RecursiveMine>());
                    }
                }
            });

            // This command will be called by the client, and executed on server
            SendTurretTypeToServer = miniRpc.RegisterAction(Target.Server, (NetworkUser user, string data) =>
            {
                // Executed by server

                _senderTurretType = data;
            });
        }

        private void InitHooks()
        {
            On.RoR2.Console.Awake += (orig, self) =>
            {
                R2API.Utils.CommandHelper.RegisterCommands(self);
                orig(self);
            };

            On.RoR2.Run.Start += RemoveRebarEffects;
            On.RoR2.Run.OnDestroy += RestoreRebarEffects;

            IL.RoR2.Stage.RespawnLocalPlayers += ChangeEngiColorOnRespawn;

            //IL.RoR2.BulletAttack.DefaultHitCallback += BulletAttackOnDefaultHitCallback;
            IL.RoR2.BlastAttack.Fire += BlastAttackSelfMineDamageHook;

            PlaceTurret.FixedUpdate += OnPlaceTurretSendTurretType;

            IL.RoR2.CharacterBody.HandleConstructTurret += TurretAddOwnerAndCustomColorHookAndTurretTypeHandler;

            var foundCC = false;
            foreach (var plugin in Chainloader.Plugins)
            {
                var metadata = MetadataHelper.GetMetadata(plugin);
                if (metadata.GUID.Equals("at.aster.charactercustomizer"))
                {
                    //Logger.LogWarning("Found Aether CharacterCustomizer");
                    foundCC = true;
                    break;
                }
            }
            if (!foundCC)
                IL.RoR2.CharacterMaster.GetDeployableSameSlotLimit += ExpendMaxMinesCountHook;

            On.RoR2.CharacterMaster.AddDeployable += AddBATComponentOnAddDeployableHook;

            On.RoR2.CharacterAI.BaseAI.FixedUpdate += ChangeTurretTypeOnPing;

            IL.EntityStates.Engi.EngiWeapon.FireGrenades.FireGrenade += ChangeSoundOnGrenadeType;
            IL.RoR2.Projectile.ProjectileImpactExplosion.FixedUpdate += PIENetworkedSound;
            IL.RoR2.Projectile.ProjectileSingleTargetImpact.OnProjectileImpact += PSTINetworkedSound;
            IL.EntityStates.Engi.EngiWeapon.FireGrenades.FixedUpdate += FireGrenadeHighASFix;

            IL.EntityStates.EngiTurret.EngiTurretWeapon.FireGauss.OnEnter += TurretFiringHook;

            FireMines.OnEnter += FireMinesOnEnter;
            IL.EntityStates.Engi.Mine.Detonate.OnEnter += EngiMineControllerOnFixedUpdate;
            On.EntityStates.Engi.Mine.Detonate.Explode += EngiMineOnExplode;

            On.EntityStates.Toolbot.ChargeSpear.OnExit += RestoreOriginalRebarColorForMULT;
        }

        private static void InitBadAssEngi()
        {
            _turretType = "Default";
            _grenadeType = "Default";
            _localMineType = "Cluster";

            _miniGunPrefab = Resources.Load<GameObject>("prefabs/effects/tracers/tracercommandoboost");
            _railGunPrefab = Resources.Load<GameObject>("prefabs/effects/tracers/tracertoolbotrebar");

            //_rebarMesh = _railGunPrefab.transform.Find("StickEffect").transform.Find("RebarMesh").gameObject;

            _railGunTrailMaterial = _railGunPrefab.transform.Find("BeamObject").GetComponentInChildren<ParticleSystemRenderer>().trailMaterial;
            _origRebarColor = _railGunTrailMaterial.GetColor(152);
            _engiRebarColor = new Color(7.5f, 7.5f, 375f);
            _railGunTrailMaterial.SetColor(152, _engiRebarColor);

            DontDestroyOnLoad(_miniGunPrefab);
            DontDestroyOnLoad(_railGunPrefab);

            On.RoR2.Skills.SkillCatalog.Init += orig =>
            {
                orig();

                var bodyIndex = BodyCatalog.FindBodyIndex("EngiBody");
                var skillLocator = BodyCatalog.GetBodyPrefab(bodyIndex).GetComponent<SkillLocator>();

                var skillFamily = skillLocator.primary.skillFamily;
                var defaultSkillDef = skillFamily.variants[skillFamily.defaultVariantIndex].skillDef;

                defaultSkillDef.skillName = "Bouncing Grenades and Seeker Swarm";
                defaultSkillDef.skillDescriptionToken =
                    "Bouncing : Charge up to <style=cIsDamage>8</style> grenades that deal <style=cIsDamage>100% damage</style> each.\n" +
                    "Seeker : Auto-targeting. Charge up to <style=cIsDamage>16</style> grenades that deal <style=cIsDamage>" +
                    SeekerGrenadeDamageCoefficient.Value * 100f + "% damage</style> each.";


                skillFamily = skillLocator.secondary.skillFamily;
                defaultSkillDef = skillFamily.variants[skillFamily.defaultVariantIndex].skillDef;

                defaultSkillDef.skillName = "Cluster and Satchel Pressured Mines";
                defaultSkillDef.skillDescriptionToken =
                    "\n<style=cIsUtility>Cluster</style> : Place a mine that explode into <style=cIsDamage>rebars</style> for <style=cIsDamage>30x" + ClusterMineDamageCoefficient.Value * 100f +
                    "% damage</style> when an enemy walks nearby. " +
                    "\nThe first mine and the second set of mines explodes into <style=cIsDamage>3</style> mines." +
                    "\n<style=cIsUtility>Satchel</style> : Place a mine that will explode for <style=cIsDamage>" + SatchelMineDamageCoefficient.Value * 100f +
                    "% damage</style> when an enemy walks nearby. \n<style=cIsDamage>Can be manually detonated</style>. <style=cIsUtility>Knockback nearby units</style>." +
                    "\nBase stock : " + ClusterMineBaseMaxStock.Value + ".";
                defaultSkillDef.baseMaxStock = ClusterMineBaseMaxStock.Value;
                defaultSkillDef.baseRechargeInterval = ClusterMineCooldown.Value;


                skillFamily = skillLocator.special.skillFamily;
                defaultSkillDef = skillFamily.variants[skillFamily.defaultVariantIndex].skillDef;

                defaultSkillDef.skillName = "TR69 MultiFunctional Auto-Turret";
                defaultSkillDef.skillDescriptionToken =
                    "Place a turret that <style=cIsUtility>inherits all your items.</style> You can <color=green>ping</color> your turrets to swap between the modes." +
                    "\n<style=cIsUtility>Default Mode</style> : Fires a cannon for <style=cIsDamage>" + DefaultTurretBaseDamage.Value + " base damage</style> with <style=cIsUtility>" + DefaultTurretAttackSpeed.Value + " base attack speed</style>." +
                    "\n<style=cIsUtility>Minigun Mode</style> : Fires a minigun for <style=cIsDamage>" + MinigunTurretBaseDamage.Value + " base damage</style> with <style=cIsUtility>" + MinigunTurretAttackSpeed.Value + " base attack speed</style>." +
                    "\n<style=cIsUtility>Railgun Mode</style> : Fires a railgun for <style=cIsDamage>" + RailgunTurretBaseDamage.Value + " base damage</style> with <style=cIsUtility>" + RailgunTurretAttackSpeed.Value + " base attack speed</style>." +
                    "\nCan place up to 2.";
            };

            

            SurvivorAPI.SurvivorCatalogReady += delegate
            {
                // FireBeam Turret
                
                /*var gameObject = BodyCatalog.FindBodyPrefab("EngiTurretBody");
                var primary = gameObject.GetComponent<SkillLocator>().primary;
                primary.activationState = new SerializableEntityStateType("EntityStates.EngiTurret.EngiTurretWeapon.FireBeam");
                object box = primary.activationState;
                primary.activationState = (SerializableEntityStateType)box;*/
            };

            _minePrefab = Resources.Load<GameObject>("prefabs/projectiles/engimine");
            DontDestroyOnLoad(_minePrefab);
            var recursiveMine = _minePrefab.AddComponent<RecursiveMine>();
            recursiveMine.Init();
            _minePrefab.GetComponent<ProjectileSimple>().velocity = 40f;

            _grenadePrefab = typeof(RoR2Application).Assembly.GetType("EntityStates.Engi.EngiWeapon.FireGrenades")
                .GetFieldCached("projectilePrefab");

            var engiseekergrenadeprojectile =
                Resources.Load<GameObject>("prefabs/projectiles/engiseekergrenadeprojectile");

            if (!engiseekergrenadeprojectile.GetComponent<SphereCollider>())
            {
                engiseekergrenadeprojectile.AddComponent<SphereCollider>();
            }

            var projectileController = engiseekergrenadeprojectile.GetComponent<ProjectileController>();

            projectileController.allowPrediction = false; // make it work in multi


            var missileController = engiseekergrenadeprojectile.GetComponent<MissileController>();

            missileController.deathTimer = SeekerGrenadeLifeTime.Value;
            missileController.delayTimer = 0.2f;
            missileController.maxVelocity = SeekerGrenadeMaxVelocity.Value;
            missileController.deathTimer -= 2f;

            var projectileSingleTargetImpact = engiseekergrenadeprojectile.GetComponent<ProjectileSingleTargetImpact>();
            projectileSingleTargetImpact.SetFieldValue("hitSoundString", SoundHelper.SeekerGrenadeExplosion);

            engiseekergrenadeprojectile.AddComponent<StopSound>();

            BadAssTurret.Buffs = SharedBuffsWithTurret.Value.Split(',').Select(buff => (BuffIndex)int.Parse(buff)).ToArray();
        }

        private static void RemoveRebarEffects(On.RoR2.Run.orig_Start orig, Run self)
        {
            orig(self);

            CachedRebarEffects[0] = _railGunPrefab.transform.Find("StickEffect").transform.Find("FlickeringPointLight").transform;
            CachedRebarEffects[0].transform.parent = null;

            CachedRebarEffects[1] = _railGunPrefab.transform.Find("StickEffect").transform.Find("Flash").transform;
            CachedRebarEffects[1].transform.parent = null;

            CachedRebarEffects[2] = _railGunPrefab.transform.Find("StickEffect").transform.Find("Dust").transform;
            CachedRebarEffects[2].transform.parent = null;

            CachedRebarEffects[3] = _railGunPrefab.transform.Find("StickEffect").transform.Find("Dust, Directional").transform;
            CachedRebarEffects[3].transform.parent = null;

            CachedRebarEffects[4] = _railGunPrefab.transform.Find("StickEffect").transform.Find("Debris").transform;
            CachedRebarEffects[4].transform.parent = null;

            CachedRebarEffects[5] = _railGunPrefab.transform.Find("StickEffect").transform.Find("RebarMesh").transform;
            CachedRebarEffects[5].transform.parent = null;
        }

        private static void RestoreRebarEffects(On.RoR2.Run.orig_OnDestroy orig, Run self)
        {
            CachedRebarEffects[0].transform.parent = _railGunPrefab.transform.Find("StickEffect").transform;
            CachedRebarEffects[1].transform.parent = _railGunPrefab.transform.Find("StickEffect").transform;
            CachedRebarEffects[2].transform.parent = _railGunPrefab.transform.Find("StickEffect").transform;
            CachedRebarEffects[3].transform.parent = _railGunPrefab.transform.Find("StickEffect").transform;
            CachedRebarEffects[4].transform.parent = _railGunPrefab.transform.Find("StickEffect").transform;
            CachedRebarEffects[5].transform.parent = _railGunPrefab.transform.Find("StickEffect").transform;

            orig(self);
        }

        private static void ChangeEngiColorOnRespawn(ILContext il)
        {
            var cursor = new ILCursor(il);
            var characterMasterLoc = 0;

            cursor.GotoNext(
                i => i.MatchLdloc(out characterMasterLoc),
                i => i.MatchLdstr(""),
                i => i.MatchCallvirt<CharacterMaster>("CallCmdRespawn")
            );
            cursor.Index++;
            
            cursor.EmitDelegate<Action>(() => { _instance.StartCoroutine(DelayedEngiColorChange(1f)); });
        }

        // Rebar self damage
        /*private static void BulletAttackOnDefaultHitCallback(ILContext il)
        {
            var cursor = new ILCursor(il);

            cursor.GotoNext(
                i => i.MatchLdloc(out _),
                i => i.MatchCeq(),
                i => i.MatchLdnull(),
                i => i.MatchStloc(out _)
            );
            cursor.Index++;
            
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Ldfld, typeof(BulletAttack).GetFieldCached("weapon"));

            cursor.Emit(OpCodes.Ldloc_1);

            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Ldfld, typeof(BulletAttack).GetFieldCached("owner"));

            cursor.EmitDelegate<Func<int, GameObject, GameObject, GameObject, int>>((flag, weapon, entityObject, owner) =>
            {
                if (flag == 1)
                {
                    if (!weapon)
                        return flag;
                    if (!weapon.name.Contains("EngiMine"))
                        return flag;

                    if (!owner || !entityObject)
                        return flag;

                    var ownerCB = owner.GetComponent<CharacterBody>();
                    var entityObjectCB = entityObject.GetComponent<CharacterBody>();

                    if (!ownerCB || !entityObject)
                    {
                        return flag;
                    }

                    if (ownerCB.netId.Value == entityObjectCB.netId.Value)
                    {
                        flag = 0;
                    }
                }

                return flag;
            });
        }*/

        // Multiply self damage to Engi
        private static void BlastAttackSelfMineDamageHook(ILContext il)
        {
            var cursor = new ILCursor(il);
            var damageInfoLoc = 0;
            var hitPointLoc = 0;

            cursor.GotoNext(
                i => i.MatchNewobj<DamageInfo>(),
                i => i.MatchStloc(out damageInfoLoc)
            );

            cursor.GotoNext(
                i => i.MatchLdloc(out hitPointLoc),
                i => i.MatchLdfld("RoR2.BlastAttack/HitPoint", "hurtBox"),
                i => i.MatchLdfld<HurtBox>("healthComponent")
            );

            cursor.Emit(OpCodes.Ldloc, damageInfoLoc);

            cursor.Emit(OpCodes.Ldarg_0);

            cursor.Emit(OpCodes.Ldloc, hitPointLoc);
            cursor.Emit(OpCodes.Ldfld, typeof(RoR2Application).Assembly
                .GetType("RoR2.BlastAttack/HitPoint").GetFieldCached("hurtBox"));

            cursor.EmitDelegate<Action<DamageInfo, BlastAttack, HurtBox>>((damageInfo, blastAttack, hurtBox) =>
            {
                if (blastAttack == null || damageInfo == null || !hurtBox)
                    return;
                if (blastAttack.canHurtAttacker && hurtBox.healthComponent.gameObject == blastAttack.attacker)
                {
                    if (blastAttack.inflictor.name.Contains("Mine"))
                    {
                        if (blastAttack.inflictor.GetComponent<RecursiveMine>())
                        {
                            damageInfo.damage *= 2f;
                        }
                        else
                        {
                            damageInfo.damage *= 0.1f;
                        }
                    }
                }
            });
        }

        private static void OnPlaceTurretSendTurretType(PlaceTurret.orig_FixedUpdate orig, EntityStates.Engi.EngiWeapon.PlaceTurret self)
        {
            if (self.turretMasterPrefab.gameObject.name.Equals("EngiTurretMaster"))
            {
                if (Util.HasEffectiveAuthority(self.outer.networkIdentity))
                {
                    if (self.outer.commonComponents.inputBank && self.GetFieldValue<float>("entryCountdown") <= 0f)
                    {
                        if ((self.outer.commonComponents.inputBank.skill1.down || self.outer.commonComponents.inputBank.skill4.justPressed))
                        {
                            if (self.outer.commonComponents.characterBody)
                            {
                                SendTurretTypeToServer.Invoke(_turretType);
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
            CharacterMaster cMaster = null;
            var CtmLoc = 0;

            cursor.GotoNext(
                i => i.MatchLdarg(0),
                i => i.MatchCallvirt<NetworkMessage>("ReadMessage"),
                i => i.MatchStloc(out CtmLoc)
            );
            cursor.Index += 3;
            cursor.Emit(OpCodes.Ldloc, CtmLoc);
            cursor.EmitDelegate<Action<ConstructTurretMessage>>(constructTurretMessage =>
            {
                var turretType = RoR2Application.isInSinglePlayer ? _turretType : _senderTurretType;

                if (CameraRigController.readOnlyInstancesList[0].localUserViewer.cachedBody)
                {
                    if (constructTurretMessage.builder.GetComponent<CharacterBody>().netId.Value == CameraRigController
                            .readOnlyInstancesList[0].localUserViewer.cachedBody.netId.Value)
                    {
                        turretType = _turretType;
                    }
                }

                turretType = turretType ?? "Default";

                SetTurretType(turretType);
            });

            
            cursor.GotoNext(i => i.MatchStloc(2));
            cursor.EmitDelegate<Func<CharacterMaster, CharacterMaster>>(master => 
            {
                cMaster = master;
                return master;
            });

            cursor.GotoNext(i => i.MatchStloc(4));
            cursor.EmitDelegate<Func<CharacterMaster, CharacterMaster>>(turret => 
            {
                if (turret.gameObject.GetComponent<AIOwnership>() != null)
                    return turret;

                turret.gameObject.AddComponent<AIOwnership>().ownerMaster = cMaster;

                var colorMsg = new ColorMsg
                {
                    NetId = turret.gameObject.GetComponent<NetworkIdentity>().netId
                };
                if (CustomTurretColor.Value)
                {
                    var rgb = TurretColor.Value.Split(',');
                    colorMsg.Color = new Color(float.Parse(rgb[0]), float.Parse(rgb[1]), float.Parse(rgb[2]));
                }
                else
                {
                    colorMsg.Color = new Color(-1, -1, -1);
                }
                SendTurretColorToServer.Invoke(colorMsg);

                return turret;
            });
        }

        // For some reason it broke, so now doing it on the IL Hook
        /*private static void TurretTypeNetworked(On.RoR2.CharacterBody.orig_HandleConstructTurret orig, NetworkMessage netmsg)
        {
            var constructTurretMessage = netmsg.ReadMessage<ConstructTurretMessage>();
            netmsg.reader.SeekZero();
            
            var turretType = RoR2Application.isInSinglePlayer ? _turretType : _senderTurretType;

            if (CameraRigController.readOnlyInstancesList[0].localUserViewer.cachedBody)
            {
                if (constructTurretMessage.builder.GetComponent<CharacterBody>().netId.Value == CameraRigController
                        .readOnlyInstancesList[0].localUserViewer.cachedBody.netId.Value)
                {
                    turretType = _turretType;
                }
            }

            turretType = turretType ?? "Default";
            
            SetTurretType(turretType);

            orig(netmsg);
        }*/

        private static void ExpendMaxMinesCountHook(ILContext il)
        {
            var cursor = new ILCursor(il);

            cursor.GotoNext(
                i => i.MatchLdcI4(4), // DeployableSlot.EngiMine
                i => i.MatchStloc(0)
            );
            cursor.Next.OpCode = OpCodes.Ldc_I4;
            cursor.Next.Operand = 100;

            cursor.GotoNext(
                i => i.MatchLdarg(0),
                i => i.MatchCallvirt<CharacterMaster>("get_bodyInstanceObject"),
                i => i.MatchCallvirt<GameObject>("GetComponent")
            );
            cursor.RemoveRange(5);
            cursor.Emit(OpCodes.Ldc_I4, 100);
        }

        private static void AddBATComponentOnAddDeployableHook(On.RoR2.CharacterMaster.orig_AddDeployable orig, CharacterMaster self, Deployable deployable, DeployableSlot slot)
        {
            orig(self, deployable, slot);

            if (slot == DeployableSlot.EngiTurret)
            {
                var badAssTurret = deployable.gameObject.AddComponent<BadAssTurret>();

                badAssTurret.Index = _localTurretPrefabIndex;
                badAssTurret.OwnerCharacterMaster = self;
                badAssTurret.Init();
            }
            /*if (slot == DeployableSlot.EngiMine)
            {
                if (!deployable.gameObject.GetComponent<RecursiveMine>() && _mineType.Equals("Cluster"))
                {
                    SendDeployableToServer.Invoke(deployable.gameObject);
                }
            }*/
        }

        // Morris original code - Customised for updating already placed Turret Type
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
                        if (playerCharacterMasterController.master.alive)
                        {
                            if (playerCharacterMasterController.master.GetBody().netId.Value == leader.characterBody.netId.Value)
                            {
                                pingerController = playerCharacterMasterController.GetFieldValue<PingerController>("pingerController");
                            }
                        }
                    }

                    if (pingerController)
                    {
                        if (pingerController.gameObject && pingerController.GetFieldValue<PingIndicator>("pingIndicator"))
                        {
                            if (pingerController.currentPing.targetNetworkIdentity)
                            {
                                if (pingerController.currentPing.targetNetworkIdentity.gameObject != null)
                                {
                                    if (pingerController.currentPing.targetNetworkIdentity.gameObject.name.Equals("EngiTurretBody(Clone)"))
                                    {
                                        SetCurrentTurretType(GiveNextTurretType(pingerController.currentPing.targetNetworkIdentity.gameObject.GetComponent<CharacterBody>().baseNameToken), pingerController.currentPing.targetNetworkIdentity.gameObject, pingerController.currentPing.targetNetworkIdentity.gameObject.GetComponent<CharacterBody>().master.gameObject);

                                        pingerController.GetFieldValue<PingIndicator>("pingIndicator").SetFieldValue("fixedTimer", 0f);
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

        private static void ChangeSoundOnGrenadeType(ILContext il)
        {
            var cursor = new ILCursor(il);

            cursor.GotoNext(
                i => i.MatchLdsfld("EntityStates.Engi.EngiWeapon.FireGrenades", "attackSoundString"),
                i => i.MatchLdarg(0),
                i => i.MatchCall(typeof(EntityState).GetMethodCached("get_gameObject")),
                i => i.MatchCall(typeof(Util).GetMethodCached("PlaySound", new[] { typeof(string), typeof(GameObject) }))
            );
            cursor.Index++;
            cursor.EmitDelegate<Func<string, string>>(soundString => _grenadeType.Equals("Seeker") ? "" : soundString);
        }

        // Sync sound across all clients for all objects using PIE, also make sure the code inside !alive is only called once.
        private static void PIENetworkedSound(ILContext il)
        {
            var cursor = new ILCursor(il);
            var soundMsg = new SoundMsg();

            cursor.GotoNext(
                i => i.MatchLdarg(0),
                i => i.MatchLdfld(typeof(ProjectileImpactExplosion).GetFieldCached("alive")),
                i => i.MatchBrtrue(out _)
            );
            cursor.Index += 2;
            var label = (ILLabel) cursor.Next.Operand;

            cursor.Index++;
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate<Func<ProjectileImpactExplosion, bool>>(instance =>
            {
                var calledComponent = instance.gameObject.GetComponent<Called>();
                if (calledComponent)
                {
                    Destroy(instance.gameObject);
                    return true;
                }

                instance.gameObject.AddComponent<Called>();
                return false;
            });
            cursor.Emit(OpCodes.Brtrue, label);

            cursor.GotoNext(
                i => i.MatchLdarg(0),
                i => i.MatchLdfld(typeof(ProjectileImpactExplosion).GetFieldCached("explosionSoundString")),
                i => i.MatchLdarg(0)
            );
            cursor.Index++;
            cursor.RemoveRange(5);

            cursor.EmitDelegate<Action<ProjectileImpactExplosion>>(instance =>
            {
                var networkIdentity = instance.gameObject.GetComponent<NetworkIdentity>();
                if (networkIdentity && NetworkServer.active)
                {
                    soundMsg.NetId = networkIdentity.netId;
                    soundMsg.SoundName = instance.explosionSoundString;
                    SendPlaySoundToClientAndDestroy.Invoke(soundMsg);
                }
                else
                {
                    AkSoundEngine.PostEvent(instance.explosionSoundString, instance.gameObject);
                }
            });

            cursor.GotoNext(
                i => i.MatchLdarg(0),
                i => i.MatchCall(typeof(Component).GetMethodCached("get_gameObject")),
                i => i.MatchCall(typeof(Object).GetMethodCached("Destroy", new []{typeof(Object)}))
            );
            cursor.Index++;
            cursor.RemoveRange(2);
            cursor.Emit(OpCodes.Pop);
        }

        // Sync sound across all clients for the seeker grenades
        private static void PSTINetworkedSound(ILContext il)
        {
            var cursor = new ILCursor(il);
            var soundMsg = new SoundMsg();

            cursor.GotoNext(
                i => i.MatchLdarg(0),
                i => i.MatchLdfld(typeof(ProjectileSingleTargetImpact).GetFieldCached("hitSoundString")),
                i => i.MatchLdarg(0)
            );
            cursor.Index++;
            cursor.RemoveRange(8);

            cursor.EmitDelegate<Action<ProjectileSingleTargetImpact>>(instance =>
            {
                var networkIdentity = instance.gameObject.GetComponent<NetworkIdentity>();
                if (networkIdentity && NetworkServer.active)
                {
                    soundMsg.NetId = networkIdentity.netId;
                    soundMsg.SoundName = instance.hitSoundString;
                    SendPlaySoundToClientAndDestroy.Invoke(soundMsg);
                }
                else
                {
                    AkSoundEngine.PostEvent(instance.hitSoundString, instance.gameObject);
                }
            });
        }

        // fix for high attack speed
        private static void FireGrenadeHighASFix(ILContext il)
        {
            var cursor = new ILCursor(il);

            cursor.GotoNext(
                i => i.MatchLdarg(0),
                i => i.MatchCall(typeof(EntityState).GetMethodCached("get_fixedAge")),
                i => i.MatchLdarg(0),
                i => i.MatchLdfld("EntityStates.Engi.EngiWeapon.FireGrenades", "duration")
            );

            cursor.Index++;
            cursor.Next.OpCode = OpCodes.Ldfld;
            cursor.Next.Operand = il.Import(typeof(RoR2Application).Assembly.GetType("EntityStates.Engi.EngiWeapon.FireGrenades").GetFieldCached("grenadeCount"));

            cursor.Index += 2;
            cursor.Next.Operand = il.Import(typeof(RoR2Application).Assembly.GetType("EntityStates.Engi.EngiWeapon.FireGrenades").GetFieldCached("grenadeCountMax"));

            cursor.Index++;
            cursor.Next.OpCode = OpCodes.Blt_S;
        }

        private static void TurretFiringHook(ILContext il)
        {
            var cursor = new ILCursor(il);
            var index = 0;
            BadAssTurret currentTurret = null;
            var soundMsg = new SoundMsg();

            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Call, typeof(EntityState).GetMethodCached("get_characterBody"));
            cursor.Emit(OpCodes.Callvirt, typeof(CharacterBody).GetMethodCached("get_master"));

            cursor.EmitDelegate<Action<CharacterMaster>>(characterMaster =>
            {
                currentTurret = characterMaster.GetComponent<BadAssTurret>();
                if (currentTurret != null)
                {
                    SendStopSoundToClient.Invoke(currentTurret.SoundRailGunTargetingId);
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
                    SendPlaySoundToClient.Invoke(soundMsg);
                    return "";
                    //return SoundHelper.MiniGunTurretShot;

                    //typeof(RoR2Application).Assembly
                    //.GetType("EntityStates.Drone.DroneWeapon.FireMegaTurret").GetFieldValue<string>("attackSoundString");
                    //.GetType("EntityStates.Commando.CommandoWeapon.FirePistol2").GetFieldValue<string>("firePistolSoundString");
                }

                if (index == 2)
                {
                    soundMsg.NetId = currentTurret.gameObject.GetComponent<NetworkIdentity>().netId;
                    soundMsg.SoundName = SoundHelper.RailGunTurretShot;
                    SendPlaySoundToClient.Invoke(soundMsg);
                    //_instance.StartCoroutine(DelayedSound(SoundHelper.RailGunTurretShot, currentTurret.gameObject, 0f));
                    //return SoundHelper.RailGunTurretShot;
                    return "";
                    //"Play_MULT_m1_snipe_shoot";
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
                i => i.MatchCall(typeof(Util).GetMethodCached("PlaySound", new[] { typeof(string), typeof(GameObject) }))
            );
            cursor.Remove();
            cursor.Emit(OpCodes.Call, typeof(Util).GetMethodCached("PlaySound", new[] { typeof(string), typeof(GameObject), typeof(string), typeof(float) }));

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
                    prefab = _miniGunPrefab;
                }

                if (index == 2)
                {
                    SendRebarToClient.Invoke(3);
                    prefab = _railGunPrefab;
                }

                return prefab;
            });
            cursor.Index += 2;

            cursor.Emit(OpCodes.Ldc_R4, RailgunTurretMaxDistanceTargeting.Value + 1f);
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
                    procCoefficient = MinigunTurretProcCoefficient.Value;
                }

                return procCoefficient;
            });
            cursor.Emit(OpCodes.Stfld, typeof(BulletAttack).GetFieldCached("procCoefficient"));
            cursor.Emit(OpCodes.Dup);
        }

        private static void FireMinesOnEnter(FireMines.orig_OnEnter orig, EntityStates.Engi.EngiWeapon.FireMines self)
        {
            uint skillVariant = 0;
            var currentCharMaster = CameraRigController.readOnlyInstancesList.FirstOrDefault()
                ?.localUserViewer
                .currentNetworkUser.master;
            if (currentCharMaster != null)
            {
                skillVariant = currentCharMaster.loadout.bodyLoadoutManager.GetSkillVariant(currentCharMaster.GetBody().bodyIndex, 1); 
            }

            // if 1, spider mines.
            if (skillVariant == 1)
            {
                orig(self);
                return;
            } 

            var recursiveMine = _minePrefab.GetComponent<RecursiveMine>();
            var intSend = 0;

            if (_localMineType.Equals("Cluster"))
            {
                if (!recursiveMine)
                {
                    _minePrefab.AddComponent<RecursiveMine>().Init();
                }
                self.damageCoefficient = ClusterMineDamageCoefficient.Value;
            }
            else
            {
                if (recursiveMine)
                {
                    DestroyImmediate(recursiveMine);
                }
                self.damageCoefficient = SatchelMineDamageCoefficient.Value;
                intSend = 1;
            }

            if (!NetworkServer.active)
            {
                SendMineTypeToServer.Invoke(intSend);
            }
            
            orig(self);
        }

        private static void EngiMineControllerOnFixedUpdate(ILContext il)
        {
            var cursor = new ILCursor(il);

            cursor.GotoNext(
                i => i.MatchLdarg(0),
                i => i.MatchCallvirt<Detonate>("Explode")
            );

            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Call, typeof(EntityState).GetMethodCached("get_transform"));

            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Call, typeof(EntityState).GetMethodCached("get_gameObject"));

            cursor.EmitDelegate<Action<Transform, GameObject>>((transform, gameObject) =>
            {
                var ownerMaster = gameObject.GetComponent<Deployable>().ownerMaster;
                var force = gameObject.GetComponent<ProjectileDamage>().force;

                var currentRecursiveMine = gameObject.GetComponent<RecursiveMine>();

                if (currentRecursiveMine)
                {
                    var recursiveMinePrefab = _minePrefab.GetComponent<RecursiveMine>();

                    if (currentRecursiveMine.RecursiveDepth >= 2)
                        return;

                    if (!recursiveMinePrefab)
                    {
                        recursiveMinePrefab = _minePrefab.AddComponent<RecursiveMine>();
                        recursiveMinePrefab.Init();
                    }

                    recursiveMinePrefab.RecursiveDepth = currentRecursiveMine.RecursiveDepth + 1;
                    
                    
                    if (ownerMaster && ownerMaster.GetBodyObject())
                    {
                        var inputBankTest = ownerMaster.GetBodyObject().GetComponent<InputBankTest>();
                        if (inputBankTest)
                        {
                            var aimDirection = inputBankTest.aimDirection;
                            var z = Random.Range(0f, 360f);
                            var z2 = Random.Range(0f, 360f);
                            var angVecLeft = Quaternion.Euler(-20, -90, z) * new Vector3(aimDirection.x, aimDirection.y, aimDirection.z).normalized;
                            var angVecRight = Quaternion.Euler(20, 90, z2) * new Vector3(aimDirection.x, aimDirection.y, aimDirection.z).normalized;

#pragma warning disable 618
                            ProjectileManager.instance.FireProjectile(_minePrefab, transform.position,
                                Util.QuaternionSafeLookRotation(angVecLeft), ownerMaster.GetBodyObject(),
                                ownerMaster.GetBody().damage * ClusterMineDamageCoefficient.Value, force,
                                Util.CheckRoll(ownerMaster.GetBody().crit, ownerMaster), DamageColorIndex.Default, null, 18f);

                            ProjectileManager.instance.FireProjectile(_minePrefab, transform.position,
                                Util.QuaternionSafeLookRotation(Vector3.up), ownerMaster.GetBodyObject(),
                                ownerMaster.GetBody().damage * ClusterMineDamageCoefficient.Value, force,
                                Util.CheckRoll(ownerMaster.GetBody().crit, ownerMaster), DamageColorIndex.Default, null, 18f);

                            ProjectileManager.instance.FireProjectile(_minePrefab, transform.position,
                                Util.QuaternionSafeLookRotation(angVecRight), ownerMaster.GetBodyObject(),
                                ownerMaster.GetBody().damage * ClusterMineDamageCoefficient.Value, force,
                                Util.CheckRoll(ownerMaster.GetBody().crit, ownerMaster), DamageColorIndex.Default, null, 18f);
#pragma warning restore 618

                            if (recursiveMinePrefab.RecursiveDepth >= 2)
                            {
                                recursiveMinePrefab.Init();
                            }
                        }
                    }
                }
                else
                {
                    // Satchel Stuff ?
                }
            });
        }

        private static void EngiMineOnExplode(On.EntityStates.Engi.Mine.Detonate.orig_Explode orig, Detonate self)
        {
            var projectileController = self.outer.gameObject.GetComponent<ProjectileController>();
            var projectileDamage = self.outer.gameObject.GetComponent<ProjectileDamage>();
            var baseMineArmingState = self.GetPropertyValue<EntityStateMachine>("armingStateMachine").state as BaseMineArmingState;
            //var actualBlastRadius = Detonate.blastRadius * baseMineArmingState.blastRadiusScale;
            var actualBlastRadius = Detonate.blastRadius;

            if (self.outer.gameObject.GetComponent<RecursiveMine>())
            {
                SendPlaySoundToClientAndDestroy.Invoke(new SoundMsg
                {
                    SoundName = SoundHelper.ClusterMineExplosion,
                    NetId = self.outer.gameObject.GetComponent<NetworkIdentity>().netId
                });

                _railGunPrefab.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                //_rebarMesh.transform.localScale = new Vector3(0.2f, 0.35f, 0.1f);

                var z = Random.Range(0f, 50f);
                var maxDistance = 99999f;
                //var delay = 0f;
                //var origin = new Vector3(0, 0, 0);

                new BlastAttack
                {
                    procChainMask = projectileController.procChainMask,
                    procCoefficient = projectileController.procCoefficient,
                    attacker = projectileController.owner,
                    inflictor = self.outer.gameObject,
                    teamIndex = projectileController.teamFilter.teamIndex,
                    canHurtAttacker = true,
                    baseDamage = projectileDamage.damage * 10f * baseMineArmingState.damageScale,
                    baseForce = 0.1f,
                    falloffModel = BlastAttack.FalloffModel.None,
                    crit = projectileDamage.crit,
                    radius = actualBlastRadius * 0.6f,
                    position = self.outer.transform.position,
                    damageColorIndex = projectileDamage.damageColorIndex
                }.Fire();

                for (float x = -90; x <= 180; x += 45) // -90 to 90 for upper sphere
                {
                    for (float y = 0f; y <= 360f; y += 72)
                    {
                        /*var vector = ray.origin + ray.direction * maxDistance;
                        if (Physics.Raycast(ray, out var raycastHit, maxDistance, LayerIndex.world.mask | LayerIndex.defaultLayer.mask | LayerIndex.entityPrecise.mask))
                        {
                            vector = raycastHit.point;
                        }

                        new BlastAttack
                        {
                            procChainMask = projectileController.procChainMask,
                            procCoefficient = projectileController.procCoefficient,
                            attacker = projectileController.owner,
                            inflictor = self.gameObject,
                            teamIndex = projectileController.teamFilter.teamIndex,
                            canHurtAttacker = true,
                            //teamIndex = TeamIndex.Neutral,
                            baseDamage = ownerCB.baseDamage,
                            baseForce = projectileDamage.force,
                            falloffModel = BlastAttack.FalloffModel.SweetSpot,
                            crit = projectileDamage.crit,
                            radius = fireLaserBlastRadius,
                            position = vector,
                            damageColorIndex = projectileDamage.damageColorIndex
                        }.Fire();
                        var effectData = new EffectData
                        {
                            origin = vector,
                            start = ray.origin
                        };
                        //instance.StartCoroutine(DelayedSpawnEffect(_railGunPrefab, effectData, projectileController.owner, delay));
                        //instance.StartCoroutine(CleanRebarObject(3f + delay));
                        //delay += 0.02f;
                        if (_rebarTracker < maximumRebar)
                        {
                            _rebarTracker += 1;
                            EffectManager.instance.SpawnEffect(_railGunPrefab, effectData, false);
                            instance.StartCoroutine(CleanRebarObject(2f));
                        }*/

                        if (_rebarTracker <= MaximumRebar.Value)
                        {
                            //_rebarTracker += 1;

                            //Debug.Log("Made Bullet at :" + ray.origin);

                            var ray = new Ray(self.outer.gameObject.transform.position, Quaternion.Euler(x, y, z) * Vector3.up);
                            /*var randomRed = Random.Range(5f, 10f);
                            var randomGreen = Random.Range(5f, 10f);
                            var randomBlue = Random.Range(5, 10) * 50f;*/
                            //Debug.Log($"Color : {randomRed} | {randomGreen} | {randomBlue}");
                            var genericBullet = new EntityStates.Toolbot.FireSpear();
                            new BulletAttack
                            {
                                aimVector = ray.direction,
                                origin = ray.origin,
                                owner = projectileController.owner,
                                weapon = self.outer.gameObject,
                                bulletCount = 1u,
                                damage = projectileDamage.damage * baseMineArmingState.damageScale,
                                damageColorIndex = DamageColorIndex.Default,
                                damageType = DamageType.Generic,
                                falloffModel = BulletAttack.FalloffModel.None,
                                force = 0.1f,
                                HitEffectNormal = false,
                                procChainMask = default,
                                procCoefficient = 0.05f,
                                maxDistance = maxDistance,
                                radius = genericBullet.bulletRadius,
                                isCrit = projectileDamage.crit,
                                muzzleName = genericBullet.muzzleName,
                                minSpread = genericBullet.minSpread,
                                maxSpread = genericBullet.maxSpread,
                                hitEffectPrefab = null,
                                smartCollision = true,
                                sniper = false,
                                spreadPitchScale = 0.5f,
                                spreadYawScale = 1f,
                                tracerEffectPrefab = _railGunPrefab
                            }.Fire();
                            SendRebarToClient.Invoke(1);
                        }
                    }
                }
                //Util.PlaySound(SoundHelper.ClusterMineExplosion, self.gameObject);
                

                //_railGunPrefab.transform.localScale = new Vector3(1f, 1f, 1f);
                //_rebarMesh.transform.localScale = new Vector3(0.4f, 0.7f, 0.2f);
            }
            else
            {
                SendPlaySoundToClientAndDestroy.Invoke(new SoundMsg
                {
                    SoundName = SoundHelper.SatchelMineExplosion,
                    NetId = self.outer.gameObject.GetComponent<NetworkIdentity>().netId
                });

                new BlastAttack
                {
                    procChainMask = projectileController.procChainMask,
                    procCoefficient = projectileController.procCoefficient,
                    attacker = projectileController.owner,
                    inflictor = self.outer.gameObject,
                    teamIndex = projectileController.teamFilter.teamIndex,
                    canHurtAttacker = true,
                    baseDamage = projectileDamage.damage * baseMineArmingState.damageScale,
                    baseForce = SatchelMineForce.Value,
                    falloffModel = BlastAttack.FalloffModel.None,
                    crit = projectileDamage.crit,
                    radius = actualBlastRadius,
                    position = self.outer.transform.position,
                    damageColorIndex = projectileDamage.damageColorIndex
                }.Fire();

                //Util.PlaySound(SoundHelper.SatchelMineExplosion, self.gameObject);

                if (Detonate.explosionEffectPrefab)
                {
                    EffectManager.instance.SpawnEffect(Detonate.explosionEffectPrefab, new EffectData
                    {
                        origin = self.outer.transform.position,
                        rotation = self.outer.transform.rotation,
                        scale = actualBlastRadius
                    }, true);
                }
            }

            // Destroy call now happens after the sound is fired
        }

        private static void RestoreOriginalRebarColorForMULT(On.EntityStates.Toolbot.ChargeSpear.orig_OnExit orig, EntityStates.Toolbot.ChargeSpear self)
        {
            SendRebarToServer.Invoke(2);

            orig(self);
        }

        private static void SetTurretType(string turretType)
        {
            var turretMaster = MasterCatalog.FindMasterPrefab("EngiTurretMaster");
            var bodyPrefab = turretMaster.GetComponent<CharacterMaster>().bodyPrefab;
            switch (turretType)
            {
                case "Default":
                    _localTurretPrefabIndex = 0;

                    bodyPrefab.GetComponent<CharacterBody>().baseAttackSpeed = DefaultTurretAttackSpeed.Value;
                    bodyPrefab.GetComponent<CharacterBody>().baseDamage = DefaultTurretBaseDamage.Value;
                    bodyPrefab.GetComponent<CharacterBody>().levelDamage = DefaultTurretDamagePerLevel.Value;
                    bodyPrefab.GetComponent<CharacterBody>().baseNameToken = "Weak Boi Default";
                    for (int i = 0; i < turretMaster.GetComponents<AISkillDriver>().Length; i++)
                    {
                        if (turretMaster.GetComponents<AISkillDriver>()[i].customName == "FireAtEnemy")
                        {
                            turretMaster.GetComponents<AISkillDriver>()[i].maxDistance = DefaultTurretMaxDistanceTargeting.Value;
                            turretMaster.GetComponents<AISkillDriver>()[i].selectionRequiresTargetLoS = false;
                            turretMaster.GetComponents<AISkillDriver>()[i].activationRequiresTargetLoS = true;
                        }
                    }
                    break;
                case "Minigun":
                    _localTurretPrefabIndex = 1;

                    bodyPrefab.GetComponent<CharacterBody>().baseAttackSpeed = MinigunTurretAttackSpeed.Value;
                    bodyPrefab.GetComponent<CharacterBody>().baseDamage = MinigunTurretBaseDamage.Value;
                    bodyPrefab.GetComponent<CharacterBody>().levelDamage = MinigunTurretDamagePerLevel.Value;
                    bodyPrefab.GetComponent<CharacterBody>().baseNameToken = ".50 Cal Maxim";
                    for (int i = 0; i < turretMaster.GetComponents<AISkillDriver>().Length; i++)
                    {
                        if (turretMaster.GetComponents<AISkillDriver>()[i].customName == "FireAtEnemy")
                        {
                            turretMaster.GetComponents<AISkillDriver>()[i].maxDistance = MinigunTurretMaxDistanceTargeting.Value;
                            turretMaster.GetComponents<AISkillDriver>()[i].selectionRequiresTargetLoS = false;
                            turretMaster.GetComponents<AISkillDriver>()[i].activationRequiresTargetLoS = true;
                        }
                    }
                    break;
                case "Railgun":
                    _localTurretPrefabIndex = 2;

                    SendRebarToClient.Invoke(3);
                    bodyPrefab.GetComponent<CharacterBody>().baseAttackSpeed = RailgunTurretAttackSpeed.Value;
                    bodyPrefab.GetComponent<CharacterBody>().baseDamage = RailgunTurretBaseDamage.Value;
                    bodyPrefab.GetComponent<CharacterBody>().levelDamage = RailgunTurretDamagePerLevel.Value;
                    bodyPrefab.GetComponent<CharacterBody>().baseNameToken = "Portable Railgun";
                    for (int i = 0; i < turretMaster.GetComponents<AISkillDriver>().Length; i++)
                    {
                        if (turretMaster.GetComponents<AISkillDriver>()[i].customName == "FireAtEnemy")
                        {
                            turretMaster.GetComponents<AISkillDriver>()[i].maxDistance = RailgunTurretMaxDistanceTargeting.Value;
                            turretMaster.GetComponents<AISkillDriver>()[i].selectionRequiresTargetLoS = false;
                            turretMaster.GetComponents<AISkillDriver>()[i].activationRequiresTargetLoS = true;
                        }
                    }
                    break;
            }
        }

        private static void SetCurrentTurretType(string turretType, GameObject body, GameObject master)
        {
            var badAssTurret = master.GetComponent<BadAssTurret>();
            switch (turretType)
            {
                case "Default":
                    badAssTurret.Index = 0;

                    body.GetComponent<CharacterBody>().baseAttackSpeed = DefaultTurretAttackSpeed.Value;
                    body.GetComponent<CharacterBody>().baseDamage = DefaultTurretBaseDamage.Value;
                    body.GetComponent<CharacterBody>().levelDamage = DefaultTurretDamagePerLevel.Value;
                    body.GetComponent<CharacterBody>().baseNameToken = "Weak Boi Default";
                    for (int i = 0; i < master.GetComponents<AISkillDriver>().Length; i++)
                    {
                        if (master.GetComponents<AISkillDriver>()[i].customName == "FireAtEnemy")
                        {
                            master.GetComponents<AISkillDriver>()[i].maxDistance = DefaultTurretMaxDistanceTargeting.Value;
                            master.GetComponents<AISkillDriver>()[i].selectionRequiresTargetLoS = false;
                            master.GetComponents<AISkillDriver>()[i].activationRequiresTargetLoS = true;
                        }
                    }
                    break;
                case "Minigun":
                    badAssTurret.Index = 1;

                    body.GetComponent<CharacterBody>().baseAttackSpeed = MinigunTurretAttackSpeed.Value;
                    body.GetComponent<CharacterBody>().baseDamage = MinigunTurretBaseDamage.Value;
                    body.GetComponent<CharacterBody>().levelDamage = MinigunTurretDamagePerLevel.Value;
                    body.GetComponent<CharacterBody>().baseNameToken = ".50 Cal Maxim";
                    for (int i = 0; i < master.GetComponents<AISkillDriver>().Length; i++)
                    {
                        if (master.GetComponents<AISkillDriver>()[i].customName == "FireAtEnemy")
                        {
                            master.GetComponents<AISkillDriver>()[i].maxDistance = MinigunTurretMaxDistanceTargeting.Value;
                            master.GetComponents<AISkillDriver>()[i].selectionRequiresTargetLoS = false;
                            master.GetComponents<AISkillDriver>()[i].activationRequiresTargetLoS = true;
                        }
                    }
                    break;
                case "Railgun":
                    badAssTurret.Index = 2;

                    SendRebarToClient.Invoke(3);
                    body.GetComponent<CharacterBody>().baseAttackSpeed = RailgunTurretAttackSpeed.Value;
                    body.GetComponent<CharacterBody>().baseDamage = RailgunTurretBaseDamage.Value;
                    body.GetComponent<CharacterBody>().levelDamage = RailgunTurretDamagePerLevel.Value;
                    body.GetComponent<CharacterBody>().baseNameToken = "Portable Railgun";
                    for (int i = 0; i < master.GetComponents<AISkillDriver>().Length; i++)
                    {
                        if (master.GetComponents<AISkillDriver>()[i].customName == "FireAtEnemy")
                        {
                            master.GetComponents<AISkillDriver>()[i].maxDistance = RailgunTurretMaxDistanceTargeting.Value;
                            master.GetComponents<AISkillDriver>()[i].selectionRequiresTargetLoS = false;
                            master.GetComponents<AISkillDriver>()[i].activationRequiresTargetLoS = true;
                        }
                    }
                    break;
            }
        }

        // Make Dictionary ?
        private static string GiveNextTurretType(string currentType)
        {
            switch (currentType)
            {
                case "Weak Boi Default":
                    return "Minigun";
                case ".50 Cal Maxim":
                    return "Railgun";
                case "Portable Railgun":
                    return "Default";
            }

            return "Default";
        }

        private static IEnumerator DelayedEngiColorChange(float seconds)
        {
            yield return new WaitForSeconds(seconds);

            var currentCharacterBody = CameraRigController.readOnlyInstancesList.First().viewer.master.GetBody().gameObject;

            if (currentCharacterBody.name.Equals("EngiBody(Clone)"))
            {
                var colorMsg = new ColorMsg();
                colorMsg.NetId = currentCharacterBody.GetComponent<NetworkIdentity>().netId;

                if (CustomEngiColor.Value)
                {
                    var rgb = EngiColor.Value.Split(',');
                    colorMsg.Color = new Color(float.Parse(rgb[0]), float.Parse(rgb[1]), float.Parse(rgb[2]));
                }
                else
                {
                    colorMsg.Color = new Color(-1, -1, -1);
                }

                SendEngiColorToServer.Invoke(colorMsg);
            }
        }

        private static IEnumerator DelayedSpawnEffect(GameObject prefab, EffectData effectData, GameObject soundEmitter, float seconds)
        {
            yield return new WaitForSeconds(seconds);
            if (seconds % 0.04f == 0)
                Util.PlaySound("Play_MULT_m1_snipe_shoot", soundEmitter);
            if (_rebarTracker < MaximumRebar.Value)
            {
                _rebarTracker += 1;

            }
                
        }

        private static IEnumerator DelayedSound(string soundName, GameObject soundEmitter, float seconds)
        {
            yield return new WaitForSeconds(seconds);
            var atkspd = soundEmitter.GetComponent<CharacterMaster>().GetBodyObject().GetComponent<CharacterBody>()
                .attackSpeed;
            soundEmitter.GetComponent<BadAssTurret>().SoundRailGunTargetingId = Util.PlaySound(soundName, soundEmitter,
                SoundHelper.TurretRTPCAttackSpeed, atkspd);
            Debug.Log("Railgun Turret Attack Speed : "+ atkspd);

        }

        private static IEnumerator CleanRebarObject(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            DestroyImmediate(GameObject.Find("BeamObject"));
            DestroyImmediate(GameObject.Find("TracerToolbotRebar(Clone)"));

            _rebarTracker -= 1;
            if (_rebarTracker < 0)
                _rebarTracker = 0;
        }

        private static Texture2D ReplaceWithRamp(Texture2D origTex, Vector3 vec, float startGrad)
        {
            Texture2D texture2D = new Texture2D(origTex.width, origTex.height, TextureFormat.RGBA32, false);
            int num = Mathf.CeilToInt(startGrad * 255f);
            int num2 = texture2D.width - num;
            Color32 color = new Color32(0, 0, 0, 0);
            Color32 c = new Color32(0, 0, 0, 0);
            for (int i = 0; i < texture2D.width; i++)
            {
                if (i >= num)
                {
                    var num3 = ((float)i - num) / num2;
                    c.r = (byte)Mathf.RoundToInt(255f * num3 * vec.x);
                    c.g = (byte)Mathf.RoundToInt(255f * num3 * vec.y);
                    c.b = (byte)Mathf.RoundToInt(255f * num3 * vec.z);
                    c.a = (byte)Mathf.RoundToInt(128f * num3);
                }
                else
                {
                    c = color;
                }
                for (int j = 0; j < texture2D.height; j++)
                {
                    texture2D.SetPixel(i, j, c);
                }
            }
            texture2D.wrapMode = TextureWrapMode.Clamp;
            texture2D.Apply();
            return texture2D;
        }

        private static void LoadAssets() //test
        {
            if (_assetBundle != null)
                return;

            _assetBundle = AssetBundle.LoadFromFile(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/customengi");

            _prefab = _assetBundle.LoadAsset<GameObject>("assets/resources/prefabs/characterbodies/engibody.prefab");
        }
    }
}