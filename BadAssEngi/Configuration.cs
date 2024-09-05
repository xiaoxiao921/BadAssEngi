using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BadAssEngi.Assets;
using BadAssEngi.Networking;
using BadAssEngi.Skills.Primary.SeekerMissile;
using BepInEx.Configuration;
using R2API.Networking;
using R2API.Networking.Interfaces;
using Rewired;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace BadAssEngi
{
    internal static class Configuration
    {
        private static ConfigFile _file;

        internal static ConfigEntry<string> TurretTypeKeyBind { get; private set; }
        internal static ConfigEntry<string> GrenadeTypeKeyBind { get; private set; }
        internal static ConfigEntry<string> MineTypeKeyBind { get; private set; }
        internal static ConfigEntry<string> SatchelManualDetonateKeyBind { get; private set; }

        internal static ConfigEntry<string> EngiColor { get; private set; }
        internal static ConfigEntry<string> TurretColor { get; private set; }
        internal static ConfigEntry<string> TrackerIndicatorColor { get; private set; }
        internal static ConfigEntry<string> ClusterMineColor { get; private set; }
        internal static ConfigEntry<bool> CustomEngiColor { get; private set; }
        internal static ConfigEntry<bool> CustomTurretColor { get; private set; }
        internal static ConfigEntry<bool> CustomTrackerIndicatorColor { get; private set; }
        internal static ConfigEntry<bool> CustomClusterMineColor { get; private set; }

        internal static ConfigEntry<string> SharedBuffsWithTurret { get; private set; }

        internal static ConfigEntry<float> DefaultTurretDamageCoefficient { get; private set; }
        internal static ConfigEntry<float> DefaultTurretAttackSpeed { get; private set; }
        internal static ConfigEntry<float> DefaultTurretDamagePerLevel { get; private set; }
        internal static ConfigEntry<float> DefaultTurretMaxDistanceTargeting { get; private set; }

        internal static ConfigEntry<float> MinigunTurretDamageCoefficient { get; private set; }
        internal static ConfigEntry<float> MinigunTurretAttackSpeed { get; private set; }
        internal static ConfigEntry<float> MinigunTurretDamagePerLevel { get; private set; }
        internal static ConfigEntry<float> MinigunTurretMaxDistanceTargeting { get; private set; }
        internal static ConfigEntry<float> MinigunTurretProcCoefficient { get; private set; }

        internal static ConfigEntry<float> RailgunTurretDamageCoefficient { get; private set; }
        internal static ConfigEntry<float> RailgunTurretAttackSpeed { get; private set; }
        internal static ConfigEntry<float> RailgunTurretDamagePerLevel { get; private set; }
        internal static ConfigEntry<float> RailgunTurretMaxDistanceTargeting { get; private set; }

        internal static ConfigEntry<float> ShotgunTurretDamageCoefficient { get; private set; }
        internal static ConfigEntry<float> ShotgunTurretAttackSpeed { get; private set; }
        internal static ConfigEntry<float> ShotgunTurretDamagePerLevel { get; private set; }
        internal static ConfigEntry<float> ShotgunTurretMaxDistanceTargeting { get; private set; }
        internal static ConfigEntry<int> ShotgunTurretBaseProjectileNumber { get; private set; }
        internal static ConfigEntry<float> ShotgunTurretSpreadCoefficient { get; private set; }

        internal static ConfigEntry<float> SeekerMissileDamageCoefficient { get; private set; }
        internal static ConfigEntry<int> SeekerMissileMaxProjectileNumber { get; private set; }

        internal static ConfigEntry<float> ClusterMineDamageCoefficient { get; private set; }
        internal static ConfigEntry<int> ClusterMineBaseMaxStock { get; private set; }
        internal static ConfigEntry<float> ClusterMineCooldown { get; private set; }
        internal static ConfigEntry<bool> ClusterMineVisualBouncing { get; private set; }

        internal static ConfigEntry<float> SatchelMineDamageCoefficient { get; private set; }
        internal static ConfigEntry<float> SatchelMineForce { get; private set; }

        internal static ConfigEntry<float> OrbitalStrikeBaseDamage { get; private set; }
        internal static ConfigEntry<float> OrbitalStrikeCooldown { get; private set; }
        internal static ConfigEntry<int> OrbitalStrikeRequiredStock { get; private set; }

        internal static ConfigEntry<Vector3> EmoteWindowPosition { get; private set; }
        internal static ConfigEntry<Vector3> EmoteWindowSize { get; private set; }

        internal static ConfigEntry<int> EmoteButtonUIPosX { get; private set; }
        internal static ConfigEntry<int> EmoteButtonUIPosY { get; private set; }

        internal static ConfigEntry<float> EmoteVolume { get; private set; }

        internal static IControllerTemplateDPad DPad = null;

        internal static void Init(ConfigFile file)
        {
            _file = file;

            const string defaultTurretTypeKeyBind = "f1";
            TurretTypeKeyBind = _file.Bind("Keybinds", "TurretType", defaultTurretTypeKeyBind,
                "What keybind should be used for changing the type of turret. List of possible keybind at the end of this page : https://docs.unity3d.com/Manual/ConventionalGameInput.html");

            const string defaultGrenadeTypeKeyBind = "f3";
            GrenadeTypeKeyBind = _file.Bind("Keybinds", "GrenadeType", defaultGrenadeTypeKeyBind,
                "What keybind should be used for changing the type of grenades.");

            const string defaultMineTypeKeyBind = "f4";
            MineTypeKeyBind = _file.Bind("Keybinds", "MineType", defaultMineTypeKeyBind,
                "What keybind should be used for changing the type of mines.");

            const string defaultSatchelManualDetonateKeyBind = "c";
            SatchelManualDetonateKeyBind = _file.Bind("Keybinds", "Satchel Manual Detonate", defaultSatchelManualDetonateKeyBind,
                "What keybind should be used for manually detonating satchel mines.");


            const bool defaultEnableEngiColor = false;
            CustomEngiColor = _file.Bind("Colors", "Enable Custom Engi Color", defaultEnableEngiColor,
                "Should the Engi have his custom colors activated ?");

            const bool defaultEnableTurretColor = false;
            CustomTurretColor = _file.Bind("Colors", "Enable Custom Turret Color", defaultEnableTurretColor,
                "Should the Engi's Turrets have its custom colors activated ?");

            const bool defaultEnableTrackerIndicatorColor = false;
            CustomTrackerIndicatorColor = _file.Bind("Colors", "Enable Custom Missile Tracker Indicator Color", defaultEnableTrackerIndicatorColor,
                "Should the Missile Tracker Indicator have a custom color ?");

            const bool defaultEnableClusterMineColor = false;
            CustomClusterMineColor = _file.Bind("Colors", "Enable Custom Cluster Mine Color", defaultEnableClusterMineColor,
                "Should the Cluster Mine have a custom color ?");

            string defaultEngiColor = string.Join(",", new[]
            {
                1f, 5f, 1f
            });
            EngiColor = _file.Bind("Colors", "Engineer", defaultEngiColor,
                "What color should the Engineer be. Example : 7.5 8 18.6");

            string defaultTurretColor = string.Join(",", new[]
            {
                0f, 80f, 0f
            });
            TurretColor = _file.Bind("Colors", "Turret", defaultTurretColor,
                "What color should the Turrets be.");

            string defaultTrackerIndicatorColor = string.Join(",", new[]
            {
                0f, 80f, 0f
            });
            TrackerIndicatorColor = _file.Bind("Colors", "Missile Tracker Indicator", defaultTrackerIndicatorColor,
                "What color should the missile tracker indicator be.");

            string defaultClusterMineColor = string.Join(",", new[]
            {
                2.1f, 0f, 0f
            });
            ClusterMineColor = _file.Bind("Colors", "Cluster Mine", defaultClusterMineColor,
                "What color should the cluster mine effect be.");


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
            SharedBuffsWithTurret = _file.Bind("Shared", "Buffs", defaultSharedBuffsWithTurret,
                    "What buffs should be shared between the Engineer and its turrets. https://pastebin.com/EsYMneGY");


            const float defaultDefaultTurretDamageCoefficient = 0.7f;
            DefaultTurretDamageCoefficient = _file.Bind("Default Turret", "Damage Coefficient", defaultDefaultTurretDamageCoefficient,
                "What should be the damage coefficient for the Default Turret.");

            const float defaultDefaultTurretAttackSpeed = 1f;
            DefaultTurretAttackSpeed = _file.Bind("Default Turret", "Attack Speed", defaultDefaultTurretAttackSpeed,
                "How much attack speed should the Default Turret have.");

            const float defaultDefaultTurretDamagePerLevel = 3.8f;
            DefaultTurretDamagePerLevel = _file.Bind("Default Turret", "Base Damage per Level", defaultDefaultTurretDamagePerLevel,
                "How much base damage should the Default Turret get per level.");

            const float defaultDefaultTurretMaxDistanceTargeting = 60f;
            DefaultTurretMaxDistanceTargeting = _file.Bind("Default Turret", "Max Distance Targeting", defaultDefaultTurretMaxDistanceTargeting,
                "How far should the Default Turret target monsters.");


            const float defaultMinigunTurretDamageCoefficient = 0.134375f;
            MinigunTurretDamageCoefficient = _file.Bind("Minigun Turret", "Damage Coefficient", defaultMinigunTurretDamageCoefficient,
                "What should be the damage coefficient for the Minigun Turret.");

            const float defaultMinigunTurretAttackSpeed = 6.9166f;
            MinigunTurretAttackSpeed = _file.Bind("Minigun Turret", "Attack Speed", defaultMinigunTurretAttackSpeed,
                "How much attack speed should the Minigun Turret have.");

            const float defaultMinigunTurretDamagePerLevel = 3.8f;
            MinigunTurretDamagePerLevel = _file.Bind("Minigun Turret", "Base Damage per Level", defaultMinigunTurretDamagePerLevel,
                "How much base damage should the Minigun Turret get per level.");

            const float defaultMinigunTurretMaxDistanceTargeting = 50f;
            MinigunTurretMaxDistanceTargeting = _file.Bind("Minigun Turret", "Max Distance Targeting", defaultMinigunTurretMaxDistanceTargeting,
                "How far should the Minigun Turret target monsters.");

            const float defaultMinigunTurretProcCoefficient = 0.5f;
            MinigunTurretProcCoefficient = _file.Bind("Minigun Turret", "Proc Coefficient", defaultMinigunTurretProcCoefficient,
                "Defines how strong or frequent procs happen on an on-hit basis (0 to 1).");


            const float defaultRailgunTurretDamageCoefficient = 7.25f;
            RailgunTurretDamageCoefficient = _file.Bind("Railgun Turret", "Damage Coefficient", defaultRailgunTurretDamageCoefficient,
                "What should be the damage coefficient for the Railgun Turret.");

            const float defaultRailgunTurretAttackSpeed = 0.1f;
            RailgunTurretAttackSpeed = _file.Bind("Railgun Turret", "Attack Speed", defaultRailgunTurretAttackSpeed,
                "How much attack speed should the Railgun Turret have.");

            const float defaultRailgunTurretDamagePerLevel = 3.8f;
            RailgunTurretDamagePerLevel = _file.Bind("Railgun Turret", "Base Damage per Level", defaultRailgunTurretDamagePerLevel,
                "How much base damage should the Railgun Turret get per level.");

            const float defaultRailgunTurretMaxDistanceTargeting = 99999f;
            RailgunTurretMaxDistanceTargeting = _file.Bind("Railgun Turret", "Max Distance Targeting", defaultRailgunTurretMaxDistanceTargeting,
                "How far should the Railgun Turret target monsters.");

            const float defaultShotgunTurretDamageCoefficient = 0.4f;
            ShotgunTurretDamageCoefficient = _file.Bind("Shotgun Turret", "Damage Coefficient", defaultShotgunTurretDamageCoefficient,
                "What should be the damage coefficient for the Shotgun Turret.");

            const float defaultShotgunTurretAttackSpeed = 0.33f;
            ShotgunTurretAttackSpeed = _file.Bind("Shotgun Turret", "Attack Speed", defaultShotgunTurretAttackSpeed,
                "How much attack speed should the Shotgun Turret have.");

            const float defaultShotgunTurretDamagePerLevel = 3.8f;
            ShotgunTurretDamagePerLevel = _file.Bind("Shotgun Turret", "Base Damage per Level", defaultShotgunTurretDamagePerLevel,
                "How much base damage should the Shotgun Turret get per level.");

            const float defaultShotgunTurretMaxDistanceTargeting = 40f;
            ShotgunTurretMaxDistanceTargeting = _file.Bind("Shotgun Turret", "Max Distance Targeting", defaultShotgunTurretMaxDistanceTargeting,
                "How far should the Shotgun Turret target monsters.");

            const int defaultShotgunTurretBaseProjectileNumber = 12;
            ShotgunTurretBaseProjectileNumber = _file.Bind("Shotgun Turret", "Base Projectile Number", defaultShotgunTurretBaseProjectileNumber,
                "How many projectiles should the shotgun have by default.");

            const float defaultShotgunTurretSpreadCoefficient = 1.75f;
            ShotgunTurretSpreadCoefficient = _file.Bind("Shotgun Turret", "Spread Coefficient", defaultShotgunTurretSpreadCoefficient,
                "How strong the spread coefficient for the shotgun should be.");


            const float defaultSeekerMissileDamageCoefficient = 0.225f;
            SeekerMissileDamageCoefficient = _file.Bind("Seeker Missile", "Damage Coefficient", defaultSeekerMissileDamageCoefficient,
                "By how much the damage should be multiplied by.");

            const int defaultSeekerMissileMaxProjectileNumber = 24;
            SeekerMissileMaxProjectileNumber = _file.Bind("Seeker Missile", "Projectile Number", defaultSeekerMissileMaxProjectileNumber,
                "How many missiles can you fire at max charge.");

            const float defaultClusterMineDamageCoefficient = 0.0355f;
            ClusterMineDamageCoefficient = _file.Bind("Cluster Mine", "Damage Coefficient", defaultClusterMineDamageCoefficient,
                "By how much the damage should be multiplied by.");

            const int defaultClusterMineBaseMaxStock = 3;
            ClusterMineBaseMaxStock = _file.Bind("Cluster Mine", "Base Stock", defaultClusterMineBaseMaxStock,
                "How many stock charge should the Engineer get by default.");

            const float defaultClusterMineCooldown = 20f;
            ClusterMineCooldown = _file.Bind("Cluster Mine", "Cooldown", defaultClusterMineCooldown,
                "How long the cooldown between each recharges should be.");

            const bool defaultClusterMineVisualBouncing = false;
            ClusterMineVisualBouncing = _file.Bind("Cluster Mine", "Bounce", defaultClusterMineVisualBouncing,
                "Should the cluster mines projectiles B O U N C E ?");

            const float defaultSatchelMineDamageCoefficient = 3.0f;
            SatchelMineDamageCoefficient = _file.Bind("Satchel Mine", "Damage Coefficient",
                defaultSatchelMineDamageCoefficient, "By how much the damage should be multiplied by.");

            const float defaultSatchelMineForce = 3000f;
            SatchelMineForce = _file.Bind("Satchel Mine", "Force", defaultSatchelMineForce,
                "How strong the force vector should be.");

            const float defaultOrbitalStrikeBaseDamage = 50f;
            OrbitalStrikeBaseDamage = _file.Bind("Orbital Strike Mine", "Base Damage", defaultOrbitalStrikeBaseDamage,
                "How much base damage should the Orbital Strike Mine deal.");

            const float defaultOrbitalStrikeCooldown = 3f;
            OrbitalStrikeCooldown = _file.Bind("Orbital Strike Mine", "Recharge Cooldown", defaultOrbitalStrikeCooldown,
                "How long the recharge cooldown for getting 1 stock should be.");

            const int defaultOrbitalStrikeRequiredStock = 100;
            OrbitalStrikeRequiredStock = _file.Bind("Orbital Strike Mine", "Required number of stock", defaultOrbitalStrikeRequiredStock,
                "How many stock are required for using the mine.");

            var defaultEmoteWindowPosition = Vector3.one;
            EmoteWindowPosition = _file.Bind("Emotes", "Emote Window Position", defaultEmoteWindowPosition,
                "The position of the Emote Window");

            var defaultEmoteWindowSize = Vector3.one;
            EmoteWindowSize = _file.Bind("Emotes", "Emote Window Size", defaultEmoteWindowSize,
                "The size of the Emote Window");

            const int defaultX = -170;
            EmoteButtonUIPosX = _file.Bind("Emotes", "Emote Button X Position", defaultX,
                "The X position of the Credit Button relative to the position of the primary skill square box. Higher X value is more left");

            const int defaultY = -155;
            EmoteButtonUIPosY = _file.Bind("Emotes", "Emote Button Y Position", defaultY,
                "The Y position of the Credit Button relative to the position of the primary skill square box. Higher Y value is more down");

            const float defaultEmoteVolume = 100f;
            EmoteVolume = _file.Bind("Emotes", "Music Volume", defaultEmoteVolume,
                "The volume value of the emotes music.");
        }

        private static void Reload()
        {
            _file.Reload();
        }

        internal static void Save()
        {
            _file.Save();
        }

        [ConCommand(commandName = "bae_reload", flags = ConVarFlags.None, helpText = "Reload the config file of Bad Ass Engi.")]
        private static void CCReloadConfig(ConCommandArgs _)
        {
            Reload();
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
                var currentNetworkUser = CameraRigController.readOnlyInstancesList.First().viewer;
                var currentCharacterBody = currentNetworkUser.master.GetBody().gameObject;

                if (args.Count == 3)
                {
                    EngiColor.Value = string.Join(",", args[0].ToString(CultureInfo.InvariantCulture), args[1].ToString(CultureInfo.InvariantCulture), args[2].ToString(CultureInfo.InvariantCulture));
                    CustomEngiColor.Value = true;
                    _file.Save();

                    var rgb = EngiColor.Value.Split(',');
                    var colorMsg = new EngiColorMsg
                    {
                        Color = new Color(float.Parse(rgb[0]), float.Parse(rgb[1]), float.Parse(rgb[2])),
                        NetId = currentCharacterBody.GetComponent<NetworkIdentity>().netId
                    };

                    colorMsg.Send(NetworkDestination.Clients);

                    return;
                }

                if (args.Count == 0 || args.Count == 1)
                {
                    var colorMsg = new EngiColorMsg
                    {
                        NetId = currentCharacterBody.GetComponent<NetworkIdentity>().netId
                    };

                    if (!CustomEngiColor.Value)
                    {
                        colorMsg.Color = new Color(-1, -1f, -1);
                    }
                    else
                    {
                        var rgb = EngiColor.Value.Split(',');
                        colorMsg.Color = new Color(float.Parse(rgb[0]), float.Parse(rgb[1]), float.Parse(rgb[2]));
                    }

                    colorMsg.Send(NetworkDestination.Clients);
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
                TurretColor.Value = string.Join(",", args[0].ToString(CultureInfo.InvariantCulture),
                    args[1].ToString(CultureInfo.InvariantCulture), args[2].ToString(CultureInfo.InvariantCulture));
                CustomTurretColor.Value = true;
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

            _file.Save();
        }

        [ConCommand(commandName = "bae_indicator_color", flags = ConVarFlags.None, helpText = "Change, enable or disable the custom color of your missile tracker indicator, disabling restore original indicator's color. Usage : bae_indicator_color R G B / bae_indicator_color enable/disable. Exemple Usage : bae_indicator_color 7 76 3.75")]
        private static void CCChangeIndicatorColor(ConCommandArgs args)
        {
            if (args.Count == 3)
            {
                TrackerIndicatorColor.Value = string.Join(",", args[0].ToString(CultureInfo.InvariantCulture), args[1].ToString(CultureInfo.InvariantCulture), args[2].ToString(CultureInfo.InvariantCulture));
                CustomTrackerIndicatorColor.Value = true;
            }
            else if (args.Count == 1)
            {
                if (args[0].ToLower().Equals("disable"))
                {
                    CustomTrackerIndicatorColor.Value = false;
                }
                else if (args[0].ToLower().Equals("enable"))
                {
                    CustomTrackerIndicatorColor.Value = true;
                }
            }
            else if (args.Count == 0)
            {
                Debug.Log("Current Color for Missile Tracker Indicator : " + (CustomTrackerIndicatorColor.Value ? TrackerIndicatorColor.Value : "Default"));
            }

            _file.Save();

            if (Run.instance && !Application.isBatchMode)
            {
                if (LocalUserManager.readOnlyLocalUsersList.Count > 0)
                {
                    var nu = LocalUserManager.GetFirstLocalUser().currentNetworkUser;
                    if (nu)
                    {
                        var body = nu.GetCurrentBody();
                        if (body && body.bodyIndex == BadAssEngi.EngiBodyIndex)
                        {
                            var missileTracker = body.GetComponent<MissileTracker>();
                            if (missileTracker)
                            {
                                Object.Destroy(missileTracker);
                                body.gameObject.AddComponent<MissileTracker>();
                            }
                        }
                    }
                }
            }
        }

        [ConCommand(commandName = "bae_cluster_color", flags = ConVarFlags.None, helpText = "Change, enable or disable the custom color of your cluster mines, disabling restore original cluster mine color. Usage : bae_cluster_color R G B / bae_indicator_color enable/disable. Exemple Usage : bae_cluster_color 7 76 3.75")]
        private static void CCChangeClusterColor(ConCommandArgs args)
        {
            if (args.Count == 3)
            {
                ClusterMineColor.Value = string.Join(",", args[0].ToString(CultureInfo.InvariantCulture), args[1].ToString(CultureInfo.InvariantCulture), args[2].ToString(CultureInfo.InvariantCulture));
                CustomClusterMineColor.Value = true;
            }
            else if (args.Count == 1)
            {
                if (args[0].ToLower().Equals("disable"))
                {
                    CustomClusterMineColor.Value = false;
                }
                else if (args[0].ToLower().Equals("enable"))
                {
                    CustomClusterMineColor.Value = true;
                }
            }
            else if (args.Count == 0)
            {
                Debug.Log("Current Color for Cluster Mine : " + (CustomClusterMineColor.Value ? ClusterMineColor.Value : "Default"));
            }

            _file.Save();

            var currentVisual = ClusterMineVisualBouncing.Value
                ? BaeAssets.PrefabEngiClusterMineVisualBounce
                : BaeAssets.PrefabEngiClusterMineVisual;

            var rgb = ClusterMineColor.Value.Split(',');
            var color = new Color(float.Parse(rgb[0]), float.Parse(rgb[1]), float.Parse(rgb[2]));
            var clusterRenderers = currentVisual.GetComponentsInChildren<Renderer>();
            foreach (var clusterRenderer in clusterRenderers)
            {
                var mats = new List<Material>();
                clusterRenderer.GetMaterials(mats);
                var i = 0;
                foreach (var material in mats)
                {
                    material.color = CustomClusterMineColor.Value ?
                        color :
                        BaeAssets.OriginalClusterMineVisual[i];

                    i++;
                }
            }
        }

        private const string BaeEmoteVolumeUsage = "Change the music volume of the emotes. Exemple Usage : bae_emote_volume 100 (range is 0-100)";
        [ConCommand(commandName = "bae_emote_volume", flags = ConVarFlags.None, helpText = BaeEmoteVolumeUsage)]
        private static void CCBaeEmoteVolume(ConCommandArgs args)
        {
            if (args.Count == 1)
            {
                if (float.TryParse(args[0], out var volumeValue))
                {
                    EmoteVolume.Value = volumeValue;
                    _file.Save();
                }
                else
                {
                    Debug.Log("Couldn't parse correctly the volume value. " + BaeEmoteVolumeUsage);
                }
            }
            else
            {
                Debug.Log("Current emote volume value is " + EmoteVolume.Value);
                Debug.Log(BaeEmoteVolumeUsage);
            }

            if (AkSoundEngine.IsInitialized())
            {
                SetEmoteVolumeRTPC();
            }
        }

        internal static void SetEmoteVolumeRTPC() => AkSoundEngine.SetRTPCValue("Volume_Emote", Configuration.EmoteVolume.Value);
    }
}