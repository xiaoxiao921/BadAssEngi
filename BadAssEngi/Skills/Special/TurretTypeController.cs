using System.Collections.Generic;
using BadAssEngi.Networking;
using R2API.Networking;
using R2API.Networking.Interfaces;
using RoR2;
using RoR2.CharacterAI;
using UnityEngine;

namespace BadAssEngi.Skills.Special
{
    internal static class TurretTypeController
    {
        internal enum TurretType
        {
            Default,
            Minigun,
            Railgun,
            Shotgun,
        }

        internal const string InternalDefaultTurretName = "Default";
        internal const string InternalMinigunTurretName = "Minigun";
        internal const string InternalRailgunTurretName = "Railgun";
        internal const string InternalShotgunTurretName = "Shotgun";

        internal const string ExternalDefaultTurretName = "Weak Boi Default";
        internal const string ExternalMinigunTurretName = ".50 Cal Maxim";
        internal const string ExternalRailgunTurretName = "Portable Railgun";
        internal const string ExternalShotgunTurretName = "Hydra Launcher";

        internal const string EngiTurretMasterName = "EngiTurretMaster";
        private const string EngiTurretAiSkillDriver = "FireAtEnemy";

        internal static Dictionary<int, string> TurretTypesIdToNames = new Dictionary<int, string>
        {
            { 0, InternalDefaultTurretName },
            { 1, InternalMinigunTurretName },
            { 2, InternalRailgunTurretName },
            { 3, InternalShotgunTurretName }
        };

        internal static int LocalTurretPrefabIndex;

        internal static TurretType CurrentTurretType;
        internal static TurretType SenderTurretType;

        internal static Color LatestTurretColorReceived;

        internal static string GiveNextTurretTypeExternalToInternal(string currentType)
        {
            switch (currentType)
            {
                case ExternalDefaultTurretName:
                    return InternalMinigunTurretName;
                case ExternalMinigunTurretName:
                    return InternalRailgunTurretName;
                case ExternalRailgunTurretName:
                    return InternalShotgunTurretName;
                case ExternalShotgunTurretName:
                    return InternalDefaultTurretName;
            }

            return InternalDefaultTurretName;
        }

        internal static void SetTurretType(TurretType turretType)
        {
            var turretMaster = MasterCatalog.FindMasterPrefab(EngiTurretMasterName);
            var bodyPrefab = turretMaster.GetComponent<CharacterMaster>().bodyPrefab;
            var characterBody = bodyPrefab.GetComponent<CharacterBody>();
            var aiSkillDrivers = turretMaster.GetComponents<AISkillDriver>();

            switch (turretType)
            {
                case TurretType.Default:
                    LocalTurretPrefabIndex = 0;

                    characterBody.baseAttackSpeed = Configuration.DefaultTurretAttackSpeed.Value;
                    characterBody.levelDamage = Configuration.DefaultTurretDamagePerLevel.Value;
                    characterBody.baseNameToken = ExternalDefaultTurretName;

                    foreach (var aiSkillDriver in aiSkillDrivers)
                    {
                        if (aiSkillDriver.customName == EngiTurretAiSkillDriver)
                        {
                            aiSkillDriver.maxDistance = Configuration.DefaultTurretMaxDistanceTargeting.Value;
                            aiSkillDriver.selectionRequiresTargetLoS = false;
                            aiSkillDriver.activationRequiresTargetLoS = true;
                        }
                    }
                    break;
                case TurretType.Minigun:
                    LocalTurretPrefabIndex = 1;

                    characterBody.baseAttackSpeed = Configuration.MinigunTurretAttackSpeed.Value;
                    characterBody.levelDamage = Configuration.MinigunTurretDamagePerLevel.Value;
                    characterBody.baseNameToken = ExternalMinigunTurretName;

                    foreach (var aiSkillDriver in aiSkillDrivers)
                    {
                        if (aiSkillDriver.customName == EngiTurretAiSkillDriver)
                        {
                            aiSkillDriver.maxDistance = Configuration.MinigunTurretMaxDistanceTargeting.Value;
                            aiSkillDriver.selectionRequiresTargetLoS = false;
                            aiSkillDriver.activationRequiresTargetLoS = true;
                        }
                    }
                    break;
                case TurretType.Railgun:
                    LocalTurretPrefabIndex = 2;

                    //new RebarColorMsg { Id = 3 }.Send(NetworkDestination.Clients);

                    characterBody.baseAttackSpeed = Configuration.RailgunTurretAttackSpeed.Value;
                    characterBody.levelDamage = Configuration.RailgunTurretDamagePerLevel.Value;
                    characterBody.baseNameToken = ExternalRailgunTurretName;

                    foreach (var aiSkillDriver in aiSkillDrivers)
                    {
                        if (aiSkillDriver.customName == EngiTurretAiSkillDriver)
                        {
                            aiSkillDriver.maxDistance = Configuration.RailgunTurretMaxDistanceTargeting.Value;
                            aiSkillDriver.selectionRequiresTargetLoS = false;
                            aiSkillDriver.activationRequiresTargetLoS = true;
                        }
                    }
                    break;
                case TurretType.Shotgun:
                    LocalTurretPrefabIndex = 3;

                    characterBody.baseAttackSpeed = Configuration.ShotgunTurretAttackSpeed.Value;
                    characterBody.levelDamage = Configuration.ShotgunTurretDamagePerLevel.Value;
                    characterBody.baseNameToken = ExternalShotgunTurretName;

                    foreach (var aiSkillDriver in aiSkillDrivers)
                    {
                        if (aiSkillDriver.customName == EngiTurretAiSkillDriver)
                        {
                            aiSkillDriver.maxDistance = Configuration.ShotgunTurretMaxDistanceTargeting.Value;
                            aiSkillDriver.selectionRequiresTargetLoS = false;
                            aiSkillDriver.activationRequiresTargetLoS = true;
                        }
                    }
                    break;
            }
        }

        internal static void SetCurrentTurretType(string turretType, GameObject body, GameObject master)
        {
            var badAssTurret = master.GetComponent<BadAssTurret>();
            var characterBody = body.GetComponent<CharacterBody>();
            var aiSkillDrivers = master.GetComponents<AISkillDriver>();
            switch (turretType)
            {
                case InternalDefaultTurretName:
                    badAssTurret.Index = 0;

                    characterBody.baseAttackSpeed = Configuration.DefaultTurretAttackSpeed.Value;
                    characterBody.levelDamage = Configuration.DefaultTurretDamagePerLevel.Value;
                    characterBody.baseNameToken = ExternalDefaultTurretName;

                    foreach (var aiSkillDriver in aiSkillDrivers)
                    {
                        if (aiSkillDriver.customName == EngiTurretAiSkillDriver)
                        {
                            aiSkillDriver.maxDistance = Configuration.DefaultTurretMaxDistanceTargeting.Value;
                            aiSkillDriver.selectionRequiresTargetLoS = false;
                            aiSkillDriver.activationRequiresTargetLoS = true;
                        }
                    }
                    break;
                case InternalMinigunTurretName:
                    badAssTurret.Index = 1;

                    characterBody.baseAttackSpeed = Configuration.MinigunTurretAttackSpeed.Value;
                    characterBody.levelDamage = Configuration.MinigunTurretDamagePerLevel.Value;
                    characterBody.baseNameToken = ExternalMinigunTurretName;

                    foreach (var aiSkillDriver in aiSkillDrivers)
                    {
                        if (aiSkillDriver.customName == EngiTurretAiSkillDriver)
                        {
                            aiSkillDriver.maxDistance = Configuration.MinigunTurretMaxDistanceTargeting.Value;
                            aiSkillDriver.selectionRequiresTargetLoS = false;
                            aiSkillDriver.activationRequiresTargetLoS = true;
                        }
                    }
                    break;
                case InternalRailgunTurretName:
                    badAssTurret.Index = 2;

                    //new RebarColorMsg { Id = 3 }.Send(NetworkDestination.Clients);

                    characterBody.baseAttackSpeed = Configuration.RailgunTurretAttackSpeed.Value;
                    characterBody.levelDamage = Configuration.RailgunTurretDamagePerLevel.Value;
                    characterBody.baseNameToken = ExternalRailgunTurretName;

                    foreach (var aiSkillDriver in aiSkillDrivers)
                    {
                        if (aiSkillDriver.customName == EngiTurretAiSkillDriver)
                        {
                            aiSkillDriver.maxDistance = Configuration.RailgunTurretMaxDistanceTargeting.Value;
                            aiSkillDriver.selectionRequiresTargetLoS = false;
                            aiSkillDriver.activationRequiresTargetLoS = true;
                        }
                    }
                    break;
                case InternalShotgunTurretName:
                    badAssTurret.Index = 3;

                    characterBody.baseAttackSpeed = Configuration.ShotgunTurretAttackSpeed.Value;
                    characterBody.levelDamage = Configuration.ShotgunTurretDamagePerLevel.Value;
                    characterBody.baseNameToken = ExternalShotgunTurretName;

                    foreach (var aiSkillDriver in aiSkillDrivers)
                    {
                        if (aiSkillDriver.customName == EngiTurretAiSkillDriver)
                        {
                            aiSkillDriver.maxDistance = Configuration.ShotgunTurretMaxDistanceTargeting.Value;
                            aiSkillDriver.selectionRequiresTargetLoS = false;
                            aiSkillDriver.activationRequiresTargetLoS = true;
                        }
                    }
                    break;
            }
        }
    }
}
