using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BadAssEngi.Assets.SeekerMissileScripts;
using BadAssEngi.Assets.Sound;
using BadAssEngi.Skills.Primary.SeekerMissile;
using BadAssEngi.Skills.Secondary.ClusterMine;
using BadAssEngi.Skills.Secondary.ClusterMine.MineStates.ArmingStateMachine;
using BadAssEngi.Skills.Secondary.ClusterMine.MineStates.MainStateMachine;
using BadAssEngi.Skills.Secondary.OrbitalStrike;
using BadAssEngi.Skills.Secondary.OrbitalStrike.MineState.ArmingStateMachine;
using BadAssEngi.Skills.Secondary.OrbitalStrike.MineState.MainStateMachine;
using BadAssEngi.Skills.Secondary.SatchelMine.MineStates.ArmingStateMachine;
using BadAssEngi.Skills.Secondary.SatchelMine.MineStates.MainStateMachine;
using BadAssEngi.Skills.Special.ShotgunTurret;
using EntityStates;
using R2API;
using RoR2;
using RoR2.Projectile;
using RoR2.UI;
using UnityEngine;
using UnityEngine.Networking;

namespace BadAssEngi.Assets
{
    public static class BaeAssets
    {
        private const string Prefix = "@BadAssEngi:";
        private const string AssetBundleFileName = "badassengi";
        private const string AssetBundleUIFileName = "badassengiui";

        private const string PathPrefabEngiTurretGhostRocket = "assets/created prefab/engi turret rockets.prefab";
        private const string PathPrefabEngiSwarmGhostRocket = "assets/created prefab/engi swarm rockets.prefab";

        private const string PathPrefabEngiSwarmExplosionEffect = "assets/created prefab/Engi Swarm Rockets Explosion.prefab";
        private const string PathPrefabEngiTurretExplosionEffect = "assets/created prefab/Engi Turret Rockets Explosion.prefab";
        private const string PathPrefabOrbitalStrike = "assets/created prefab/floor detection.prefab";

        private const string PathPrefabEngiRocketCrosshair = "assets/created prefab/engi rocket crosshair.prefab";

        private const string PathPrefabEngiClusterMineVisual = "assets/created prefab/cluster mine visual.prefab";
        private const string PathPrefabEngiClusterMineVisualBounce = "assets/created prefab/cluster mine visual bounce.prefab";

        private const string PathIconSwappableGrenades = "assets/created prefab/swappable grenades.png";
        private const string PathIconMissileM1 = "assets/created prefab/harpoon m1 icon.png";
        private const string PathIconSwappableMines = "assets/created prefab/swappable mine.png";
        private const string PathIconOrbitalStrike = "assets/created prefab/orbital strike icon.png";
        private const string PathIconSwappableTurrets = "assets/created prefab/swappable turret.png";

        private const string PathPrefabEngiCustomAnimation = "assets/created prefab/EngiFix.prefab";

        private const string PathPrefabEmoteWindow = "assets/badassengi/startup prefab.prefab";



        private static bool Loaded { get; set; }

        public static GameObject EngiBodyPrefab { get; private set; }

        public static GameObject EngiClusterMinePrefab { get; private set; }
        public static GameObject EngiClusterMineDepthOnePrefab { get; private set; }
        public static GameObject EngiClusterMineDepthTwoPrefab { get; private set; }

        public static GameObject EngiOrbitalMinePrefab { get; private set; }
        public static GameObject EngiSatchelMinePrefab { get; private set; }

        public static GameObject PrefabEngiTurretRocket { get; private set; }
        public static GameObject PrefabEngiTurretGhostRocket { get; private set; }
        public static GameObject PrefabEngiTurretExplosionEffect { get; private set; }

        public static GameObject PrefabEngiSwarmRocket { get; private set; }
        public static GameObject PrefabEngiSwarmGhostRocket { get; private set; }
        public static GameObject PrefabEngiSwarmExplosionEffect { get; private set; }
        public static GameObject PrefabEngiRocketCrosshair { get; private set; }

        internal static Material RailGunTrailMaterial { get; set; }
        internal static Color OrigRebarColor { get; set; }
        internal static Color EngiRebarColor { get; set; }
        internal static GameObject RailGunPrefab { get; private set; }
        internal static GameObject MiniGunPrefab { get; private set; }

        public static GameObject PrefabOrbitalStrike { get; private set; }

        
        public static GameObject PrefabEngiClusterMineVisual { get; private set; }
        public static GameObject PrefabEngiClusterMineVisualBounce { get; private set; }
        public static readonly List<Color> OriginalClusterMineVisual = new List<Color>();

        public static Sprite IconSwappableGrenades { get; private set; }
        public static Sprite IconMissileM1 { get; private set; }
        public static Sprite IconSwappableMines { get; private set; }
        public static Sprite IconOrbitalStrike { get; private set; }
        public static Sprite IconSwappableTurrets { get; private set; }

        public static GameObject PrefabEngiCustomAnimation { get; private set; }
        public static readonly List<string> EngiAnimations = new List<string>();

        public static GameObject PrefabEmoteWindow;
        internal static GameObject MainMenuButtonPrefab { get; private set; }
        internal static GameObject PauseMenuPrefab { get; private set; }

        public static void Init()
        {
            if (Loaded)
                return;

            EngiBodyPrefab = Resources.Load<GameObject>("Prefabs/CharacterBodies/EngiBody");

            InitMinePrefabs();

            InitRebarPrefabs();

            LoadFromAssetBundle();

            AddPrefabsToGame();

            Loaded = true;
        }

        private static void LoadFromAssetBundle()
        {
            var execAssembly = Assembly.GetExecutingAssembly();

            var resourceName = execAssembly.GetManifestResourceNames()
                .Single(str => str.EndsWith(AssetBundleFileName));

            using (var stream = execAssembly.GetManifestResourceStream(resourceName))
            {
                var bundle = AssetBundle.LoadFromStream(stream);
                var provider = new AssetBundleResourcesProvider(Prefix.TrimEnd(':'), bundle);
                ResourcesAPI.AddProvider(provider);

                PrefabEngiTurretGhostRocket = bundle.LoadAsset<GameObject>(PathPrefabEngiTurretGhostRocket);
                PrefabEngiSwarmGhostRocket = bundle.LoadAsset<GameObject>(PathPrefabEngiSwarmGhostRocket);
                PrefabEngiSwarmExplosionEffect = bundle.LoadAsset<GameObject>(PathPrefabEngiSwarmExplosionEffect);
                PrefabEngiTurretExplosionEffect = bundle.LoadAsset<GameObject>(PathPrefabEngiTurretExplosionEffect);
                PrefabOrbitalStrike = bundle.LoadAsset<GameObject>(PathPrefabOrbitalStrike);
                PrefabEngiRocketCrosshair = bundle.LoadAsset<GameObject>(PathPrefabEngiRocketCrosshair);

                PrefabEngiClusterMineVisual = bundle.LoadAsset<GameObject>(PathPrefabEngiClusterMineVisual);
                PrefabEngiClusterMineVisualBounce = bundle.LoadAsset<GameObject>(PathPrefabEngiClusterMineVisualBounce);

                IconSwappableGrenades = bundle.LoadAsset<Texture2D>(PathIconSwappableGrenades).CreateSpriteFromTexture2D();
                IconMissileM1 = bundle.LoadAsset<Texture2D>(PathIconMissileM1).CreateSpriteFromTexture2D();
                IconSwappableMines = bundle.LoadAsset<Texture2D>(PathIconSwappableMines).CreateSpriteFromTexture2D();
                IconOrbitalStrike = bundle.LoadAsset<Texture2D>(PathIconOrbitalStrike).CreateSpriteFromTexture2D();
                IconSwappableTurrets =
                    bundle.LoadAsset<Texture2D>(PathIconSwappableTurrets).CreateSpriteFromTexture2D();

                PrefabEngiCustomAnimation = bundle.LoadAsset<GameObject>(PathPrefabEngiCustomAnimation);
            }

            var resourceNameUI = execAssembly.GetManifestResourceNames()
                .Single(str => str.EndsWith(AssetBundleUIFileName));

            using (var stream = execAssembly.GetManifestResourceStream(resourceNameUI))
            {
                var bundle = AssetBundle.LoadFromStream(stream);

                PrefabEmoteWindow = bundle.LoadAsset<GameObject>(PathPrefabEmoteWindow);
            }
        }

        private static void InitMinePrefabs()
        {
            EngiClusterMinePrefab = Resources.Load<GameObject>("prefabs/projectiles/engimine").InstantiateClone("BaeEngiMine");
            EngiClusterMinePrefab.GetComponent<ProjectileSimple>().velocity = 40f;
            EngiClusterMinePrefab.AddComponent<RecursiveMine>();

            var clusterArmingStateMachine = EngiClusterMinePrefab.GetComponentsInChildren<EntityStateMachine>().First(machine => machine.customName == "Arming");
            clusterArmingStateMachine.initialStateType = new SerializableEntityStateType(typeof(MineArmingUnarmedCluster));
            clusterArmingStateMachine.mainStateType = new SerializableEntityStateType(typeof(MineArmingUnarmedCluster));

            var clusterMainStateMachine = EngiClusterMinePrefab.GetComponentsInChildren<EntityStateMachine>().First(machine => machine.customName == "Main");
            clusterMainStateMachine.initialStateType = new SerializableEntityStateType(typeof(WaitForStickCluster));
            clusterMainStateMachine.mainStateType = new SerializableEntityStateType(typeof(MineArmingUnarmedCluster));

            EngiClusterMineDepthOnePrefab = EngiClusterMinePrefab.InstantiateClone("BaeEngiMineDepthOne");
            EngiClusterMineDepthOnePrefab.GetComponent<RecursiveMine>().RecursiveDepth = 1;
            EngiClusterMineDepthTwoPrefab = EngiClusterMinePrefab.InstantiateClone("BaeEngiMineDepthTwo");
            EngiClusterMineDepthTwoPrefab.GetComponent<RecursiveMine>().RecursiveDepth = 2;

            EngiSatchelMinePrefab = Resources.Load<GameObject>("prefabs/projectiles/engimine").InstantiateClone("BaeSatchelMine");
            EngiSatchelMinePrefab.GetComponent<ProjectileSimple>().velocity = 40f;

            var satchelArmingStateMachine = EngiSatchelMinePrefab.GetComponentsInChildren<EntityStateMachine>().First(machine => machine.customName == "Arming");
            satchelArmingStateMachine.initialStateType = new SerializableEntityStateType(typeof(MineArmingUnarmedSatchel));
            satchelArmingStateMachine.mainStateType = new SerializableEntityStateType(typeof(MineArmingUnarmedSatchel));

            var satchelMainStateMachine = EngiSatchelMinePrefab.GetComponentsInChildren<EntityStateMachine>().First(machine => machine.customName == "Main");
            satchelMainStateMachine.initialStateType = new SerializableEntityStateType(typeof(WaitForStickSatchel));
            satchelMainStateMachine.mainStateType = new SerializableEntityStateType(typeof(MineArmingUnarmedSatchel));

            EngiOrbitalMinePrefab = Resources.Load<GameObject>("prefabs/projectiles/engimine").InstantiateClone("BaeOrbitalMine");
            EngiOrbitalMinePrefab.GetComponent<ProjectileSimple>().velocity = 40f;

            var orbitalArmingStateMachine = EngiOrbitalMinePrefab.GetComponentsInChildren<EntityStateMachine>().First(machine => machine.customName == "Arming");
            orbitalArmingStateMachine.initialStateType = new SerializableEntityStateType(typeof(MineArmingUnarmedOrbital));
            orbitalArmingStateMachine.mainStateType = new SerializableEntityStateType(typeof(MineArmingUnarmedOrbital));

            var orbitalMainStateMachine = EngiOrbitalMinePrefab.GetComponentsInChildren<EntityStateMachine>().First(machine => machine.customName == "Main");
            orbitalMainStateMachine.initialStateType = new SerializableEntityStateType(typeof(WaitForStickOrbital));
            orbitalMainStateMachine.mainStateType = new SerializableEntityStateType(typeof(MineArmingUnarmedOrbital));

            ProjectileAPI.Add(EngiClusterMinePrefab);
            ProjectileAPI.Add(EngiClusterMineDepthOnePrefab);
            ProjectileAPI.Add(EngiClusterMineDepthTwoPrefab);

            ProjectileAPI.Add(EngiSatchelMinePrefab);
            ProjectileAPI.Add(EngiOrbitalMinePrefab);
        }

        private static void InitRebarPrefabs()
        {
            MiniGunPrefab = Resources.Load<GameObject>("prefabs/effects/tracers/tracercommandoboost");
            RailGunPrefab = Resources.Load<GameObject>("prefabs/effects/tracers/tracertoolbotrebar");

            RailGunTrailMaterial = RailGunPrefab.transform.Find("BeamObject").GetComponentInChildren<ParticleSystemRenderer>().trailMaterial;
            OrigRebarColor = RailGunTrailMaterial.GetColor(152);
            EngiRebarColor = new Color(7.5f, 7.5f, 375f);
            RailGunTrailMaterial.SetColor(152, EngiRebarColor);

            Object.DontDestroyOnLoad(MiniGunPrefab);
            Object.DontDestroyOnLoad(RailGunPrefab);
        }

        private static void AddPrefabsToGame()
        {
            InitUIButtonPrefab();

            InitAnimations();

            InitClusterBouncePrefabs();

            PrefabOrbitalStrike.AddComponent<OrbitalStrikeController>();
            PrefabOrbitalStrike.AddComponent<NetworkIdentity>();
            PrefabOrbitalStrike.RegisterNetworkPrefab();

            InitRocketPrefabs();
        }

        private static void InitRocketPrefabs()
        {
            //set layers for crosshair
            PrefabEngiRocketCrosshair.layer = 5;
            var childCount = PrefabEngiRocketCrosshair.transform.childCount;
            for (var i = 0; i < childCount; i++)
            {
                var child = PrefabEngiRocketCrosshair.transform.GetChild(i);
                child.gameObject.layer = 5;
            }

            PrefabEngiTurretGhostRocket.AddComponent<ProjectileGhostController>();
            PrefabEngiTurretGhostRocket.AddComponent<RocketSmokeController>();

            PrefabEngiSwarmGhostRocket.AddComponent<ProjectileGhostController>();
            PrefabEngiSwarmGhostRocket.AddComponent<RocketSmokeController>();

            var omniExplo = Resources.Load<GameObject>("prefabs/effects/omnieffect/OmniExplosionVFXToolbotQuick");
            var omniEffectComponent = omniExplo.GetComponent<EffectComponent>();

            var turretExploEffectComponent = PrefabEngiTurretExplosionEffect.AddComponent<EffectComponent>();
            turretExploEffectComponent.effectData = omniEffectComponent.effectData;
            var turretExploVFXAttributes = PrefabEngiTurretExplosionEffect.AddComponent<VFXAttributes>();

            var swarmExploEffectComponent = PrefabEngiSwarmExplosionEffect.AddComponent<EffectComponent>();
            swarmExploEffectComponent.effectData = omniEffectComponent.effectData;
            var swarmExploVFXAttributes = PrefabEngiSwarmExplosionEffect.AddComponent<VFXAttributes>();

            var turretEffectDef = new EffectDef
            {
                prefab = PrefabEngiTurretExplosionEffect,
                prefabEffectComponent = turretExploEffectComponent,
                prefabVfxAttributes = turretExploVFXAttributes,
                prefabName = PrefabEngiTurretExplosionEffect.name
            };
            var swarmEffectDef = new EffectDef
            {
                prefab = PrefabEngiSwarmExplosionEffect,
                prefabEffectComponent = swarmExploEffectComponent,
                prefabVfxAttributes = swarmExploVFXAttributes,
                prefabName = PrefabEngiSwarmExplosionEffect.name
            };
            EffectAPI.AddEffect(turretEffectDef);
            EffectAPI.AddEffect(swarmEffectDef);

            var engiHarpoonProjectilePrefab = Resources.Load<GameObject>("prefabs/projectiles/EngiHarpoon");

            PrefabEngiTurretRocket =
                engiHarpoonProjectilePrefab.InstantiateClone("EngiTurretRocket");

            var pcEngiTurret = PrefabEngiTurretRocket.GetComponent<ProjectileController>();
            pcEngiTurret.ghostPrefab = PrefabEngiTurretGhostRocket;
            pcEngiTurret.allowPrediction = false;

            var pieTurret = PrefabEngiTurretRocket.GetComponent<ProjectileSingleTargetImpact>();
            pieTurret.hitSoundString = "";
            var explosionTurretMissileSound = PrefabEngiTurretExplosionEffect.AddComponent<SeekerExplosionSoundFix>();
            explosionTurretMissileSound.SoundEventToPlay = SoundHelper.RocketTurretExplosion;
            pieTurret.impactEffect = PrefabEngiTurretExplosionEffect;

            Object.Destroy(PrefabEngiTurretRocket.GetComponent<MissileController>());
            PrefabEngiTurretRocket.AddComponent<ShotgunProjectileController>();

            PrefabEngiSwarmRocket =
                engiHarpoonProjectilePrefab.InstantiateClone("EngiSwarmRocket");
            PrefabEngiSwarmRocket.AddComponent<SwarmFix>();
            PrefabEngiSwarmRocket.AddComponent<StopSound>();
            Object.Destroy(PrefabEngiSwarmRocket.GetComponent<ApplyTorqueOnStart>());

            var pcEngiSwarm = PrefabEngiSwarmRocket.GetComponent<ProjectileController>();
            pcEngiSwarm.ghostPrefab = PrefabEngiSwarmGhostRocket;
            pcEngiSwarm.allowPrediction = false;

            var pieSwarm = PrefabEngiSwarmRocket.GetComponent<ProjectileSingleTargetImpact>();
            pieSwarm.hitSoundString = "";
            var explosionSwarmMissileSound = PrefabEngiSwarmExplosionEffect.AddComponent<SeekerExplosionSoundFix>();
            explosionSwarmMissileSound.SoundEventToPlay = SoundHelper.RocketTurretExplosion;
            pieSwarm.impactEffect = PrefabEngiSwarmExplosionEffect;

            PrefabEngiSwarmRocket.GetComponent<AkEvent>().data = null;

            var qPid = PrefabEngiSwarmRocket.GetComponent<QuaternionPID>();
            qPid.PID = new Vector3(5.0f, 0.3f, 0);
            qPid.gain = 20f;

            var mc = PrefabEngiSwarmRocket.GetComponent<MissileController>();
            mc.maxVelocity = 40f;

            var missileBoxSize = new Vector3(0.3f, 0.3f, 2f);
            var turretCollider = PrefabEngiTurretRocket.GetComponent<BoxCollider>();
            turretCollider.size = missileBoxSize;
            var swarmCollider = PrefabEngiSwarmRocket.GetComponent<BoxCollider>();
            swarmCollider.size = missileBoxSize;

            ProjectileAPI.Add(PrefabEngiTurretRocket);
            ProjectileAPI.Add(PrefabEngiSwarmRocket);
        }

        private static void InitClusterBouncePrefabs()
        {
            PrefabEngiClusterMineVisual.AddComponent<NetworkIdentity>();
            PrefabEngiClusterMineVisual.AddComponent<ClusterController>();
            var prefabEngiClusterMineVisual = PrefabEngiClusterMineVisual.InstantiateClone("ClusterMineVisual");
            Object.Destroy(PrefabEngiClusterMineVisual);
            PrefabEngiClusterMineVisual = prefabEngiClusterMineVisual;

            var clusterRenderers = PrefabEngiClusterMineVisual.GetComponentsInChildren<Renderer>();
            foreach (var clusterRenderer in clusterRenderers)
            {
                var mats = new List<Material>();
                clusterRenderer.GetMaterials(mats);
                foreach (var material in mats)
                {
                    OriginalClusterMineVisual.Add(material.color);
                }
            }

            PrefabEngiClusterMineVisualBounce.AddComponent<NetworkIdentity>();
            PrefabEngiClusterMineVisualBounce.AddComponent<ClusterController>();
            var prefabEngiClusterMineVisualBounce = PrefabEngiClusterMineVisualBounce.InstantiateClone("ClusterMineVisualBounce");
            Object.Destroy(PrefabEngiClusterMineVisualBounce);
            PrefabEngiClusterMineVisualBounce = prefabEngiClusterMineVisualBounce;

            var bounceClusterRenderers = PrefabEngiClusterMineVisual.GetComponentsInChildren<Renderer>();

            if (Configuration.CustomClusterMineColor.Value)
            {
                var rgb = Configuration.ClusterMineColor.Value.Split(',');
                var color = new Color(float.Parse(rgb[0]), float.Parse(rgb[1]), float.Parse(rgb[2]));
                foreach (var clusterRenderer in bounceClusterRenderers)
                {
                    var mats = new List<Material>();
                    clusterRenderer.GetMaterials(mats);
                    foreach (var material in mats)
                    {
                        material.color = color;
                    }
                }
            }
        }

        private static void InitAnimations()
        {
            var engiOriginalSMR = EngiBodyPrefab.GetComponentInChildren<SkinnedMeshRenderer>();
            var engiMaterials = engiOriginalSMR.materials;

            var engiCustomMeshRenderer = PrefabEngiCustomAnimation.GetComponentInChildren<SkinnedMeshRenderer>();
            engiCustomMeshRenderer.materials = engiMaterials;

            var animationClips = PrefabEngiCustomAnimation.GetComponent<Animator>().runtimeAnimatorController.animationClips;
            foreach (var animClip in animationClips)
            {
                var animName = animClip.name;
                if (!animName.ToLower().Contains("loop"))
                {
                    EngiAnimations.Add(animName);
                }
            }
            EngiAnimations.Sort();
        }

        private static void InitUIButtonPrefab()
        {
            if (!MainMenuButtonPrefab)
            {
                PauseMenuPrefab = Resources.Load<GameObject>("prefabs/ui/PauseScreen");
                PauseMenuPrefab = Object.Instantiate(PauseMenuPrefab);

                InitMainMenuButtonPrefab();

                PrefabEmoteWindow = PrefabEmoteWindow.InstantiateClone("PrefabEmoteWindow", false);
                PrefabEngiCustomAnimation = PrefabEngiCustomAnimation.InstantiateClone("PrefabEngiCustomAnimation", false);
                
                var layerKey = PauseMenuPrefab.GetComponent<UILayerKey>();

                var window = PrefabEmoteWindow.transform.GetChild(0).gameObject;

                window.AddComponent<MPEventSystemProvider>();
                window.AddComponent<MPEventSystemLocator>();
                window.AddComponent<CursorOpener>();

                var emoteLayerKey = window.AddComponent<UILayerKey>();
                emoteLayerKey.layer = layerKey.layer;
                emoteLayerKey.onBeginRepresentTopLayer = layerKey.onBeginRepresentTopLayer;
                emoteLayerKey.onEndRepresentTopLayer = layerKey.onEndRepresentTopLayer;

                window.SetActive(false);
                PrefabEmoteWindow.SetActive(false);

                Object.Destroy(PauseMenuPrefab);
            }
        }

        internal static void InitMainMenuButtonPrefab()
        {
            if (!PauseMenuPrefab)
            {
                PauseMenuPrefab = Resources.Load<GameObject>("prefabs/ui/PauseScreen");
                PauseMenuPrefab = Object.Instantiate(PauseMenuPrefab);
            }

            var menuButtonFromScene = GameObject.Find("GenericMenuButton (Settings)");

            if (!menuButtonFromScene)
                return;

            MainMenuButtonPrefab = menuButtonFromScene.InstantiateClone("MainMenuButtonPrefab", false);

            if (!MainMenuButtonPrefab)
            {
                return;
            }

            var textControllers = MainMenuButtonPrefab.GetComponentsInChildren<LanguageTextMeshController>();
            foreach (var textController in textControllers)
            {
                Object.Destroy(textController);
            }

            var languageTextMeshController = MainMenuButtonPrefab.AddComponent<LanguageTextMeshController>();
            languageTextMeshController.token = "Emote Menu";

            var hgButtonPrefab = MainMenuButtonPrefab.GetComponent<HGButton>();
            hgButtonPrefab.hoverLanguageTextMeshController =
                languageTextMeshController;
            hgButtonPrefab.updateTextOnHover = false;
        }

        private static Sprite CreateSpriteFromTexture2D(this Texture2D texture2D)
        {
            var rect = new Rect(0, 0, texture2D.width, texture2D.height);
            return Sprite.Create(texture2D, rect, new Vector2(0.5f, 0.5f));
        }
    }
}
