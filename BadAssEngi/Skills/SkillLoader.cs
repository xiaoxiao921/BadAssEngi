using System;
using System.Linq;
using BadAssEngi.Assets;
using BadAssEngi.Skills.Primary.States;
using BadAssEngi.Skills.Secondary.ClusterMine.EngiStates;
using BadAssEngi.Skills.Secondary.ClusterMine.MineStates.ArmingStateMachine;
using BadAssEngi.Skills.Secondary.ClusterMine.MineStates.MainStateMachine;
using BadAssEngi.Skills.Secondary.OrbitalStrike.EngiStates;
using BadAssEngi.Skills.Secondary.OrbitalStrike.MineState.ArmingStateMachine;
using BadAssEngi.Skills.Secondary.OrbitalStrike.MineState.MainStateMachine;
using BadAssEngi.Skills.Secondary.SatchelMine.EngiStates;
using BadAssEngi.Skills.Secondary.SatchelMine.MineStates.ArmingStateMachine;
using BadAssEngi.Skills.Secondary.SatchelMine.MineStates.MainStateMachine;
using BadAssEngi.Util;
using EntityStates;
using EntityStates.Engi.EngiWeapon;
using R2API;
using RoR2;
using RoR2.Skills;
using UnityEngine;

namespace BadAssEngi.Skills
{
    internal static class SkillLoader
    {
        private static SkillLocator _engiSkillLocator;

        internal static SkillDef ChargeSwappableGrenadesSkillDef { get; private set; }
        internal static SkillDef ChargeSeekerMissileSkillDef { get; private set; }

        internal static SkillDef SwappableMineSkillDef { get; private set; }
        internal static SkillDef SatchelMineSkillDef { get; private set; }
        internal static SkillDef OrbitalStrikeSkillDef { get; private set; }

        internal static SkillDef SwappableTurretSkillDef { get; private set; }

        internal static int SwappableGrenadeSkillVariant;
        internal static int SeekerSwarmSkillVariant;

        internal static int SatchelMineSkillVariant;
        internal static int SwappableMineSkillVariant;
        internal static int OrbitalStrikeSkillVariant;

        internal static int SwappableTurretSkillVariant;

        internal static void Init()
        {
            _engiSkillLocator = BaeAssets.EngiBodyPrefab.GetComponent<SkillLocator>();

            InitEngiPrimarySkills();
            InitEngiSecondarySkills();
            InitEngiSpecialSkills();

            RegisterStatesTypes();
        }

        private static void InitEngiPrimarySkills()
        {
            SeekerMissile();
            SwappableGrenade();

            var skills = _engiSkillLocator.primary._skillFamily.variants
                .Select(variant => variant.skillDef).ToList();

            SeekerSwarmSkillVariant = skills.Count;
            skills.Add(ChargeSeekerMissileSkillDef);

            SwappableGrenadeSkillVariant = skills.Count;
            skills.Add(ChargeSwappableGrenadesSkillDef);

            _engiSkillLocator.primary.SetSkillFamily(_engiSkillLocator.primary._skillFamily.name, skills.ToArray());
        }

        private static void InitEngiSecondarySkills()
        {
            SwappableMine();
            SatchelMine();
            OrbitalStrikeMine();

            var skills = _engiSkillLocator.secondary._skillFamily.variants
                .Select(variant => variant.skillDef).ToList();

            SatchelMineSkillVariant = 0;
            skills[SatchelMineSkillVariant] = SatchelMineSkillDef;

            SwappableMineSkillVariant = skills.Count;
            skills.Add(SwappableMineSkillDef);

            OrbitalStrikeSkillVariant = skills.Count;
            skills.Add(OrbitalStrikeSkillDef);

            _engiSkillLocator.secondary.SetSkillFamily(_engiSkillLocator.secondary._skillFamily.name, skills.ToArray());
        }

        private static void InitEngiSpecialSkills()
        {
            SwappableTurret();

            var skills = _engiSkillLocator.special._skillFamily.variants
                .Select(variant => variant.skillDef).ToList();

            SwappableTurretSkillVariant = skills.Count;
            skills.Add(SwappableTurretSkillDef);

            _engiSkillLocator.special.SetSkillFamily(_engiSkillLocator.special._skillFamily.name, skills.ToArray());
        }

        private static void SeekerMissile()
        {
            ChargeSeekerMissileSkillDef = ScriptableObject.CreateInstance<SkillDef>();
            ((ScriptableObject)ChargeSeekerMissileSkillDef).name = "BAESeekerSwarm";
            ChargeSeekerMissileSkillDef.skillName = "BAESeekerSwarm";
            ChargeSeekerMissileSkillDef.skillNameToken = "Seeker Swarm";

            ChargeSeekerMissileSkillDef.skillDescriptionToken =
                "Seeker : Auto-targeting. Charge up to <style=cIsDamage>" + Configuration.SeekerMissileMaxProjectileNumber.Value +
                "</style> missiles that deal <style=cIsDamage>" +
                Configuration.SeekerMissileDamageCoefficient.Value * 100f + "% damage</style> each.";

            ChargeSeekerMissileSkillDef.activationStateMachineName = "Weapon";
            ChargeSeekerMissileSkillDef.activationState = new SerializableEntityStateType(typeof(ChargeSeekerMissile));
            ChargeSeekerMissileSkillDef.interruptPriority = InterruptPriority.Any;
            ChargeSeekerMissileSkillDef.baseRechargeInterval = 0;
            ChargeSeekerMissileSkillDef.baseMaxStock = 1;
            ChargeSeekerMissileSkillDef.rechargeStock = 1;
            ChargeSeekerMissileSkillDef.beginSkillCooldownOnSkillEnd = false;
            ChargeSeekerMissileSkillDef.requiredStock = 1;
            ChargeSeekerMissileSkillDef.stockToConsume = 1;
            ChargeSeekerMissileSkillDef.isCombatSkill = true;
            ChargeSeekerMissileSkillDef.cancelSprintingOnActivation = true;
            ChargeSeekerMissileSkillDef.canceledFromSprinting = false;
            ChargeSeekerMissileSkillDef.mustKeyPress = false;
            ChargeSeekerMissileSkillDef.icon = BaeAssets.IconMissileM1;
        }

        private static void SwappableGrenade()
        {
            ChargeSwappableGrenadesSkillDef = ScriptableObject.CreateInstance<SkillDef>();
            ((ScriptableObject)ChargeSwappableGrenadesSkillDef).name = "BAESwappableGrenade";
            ChargeSwappableGrenadesSkillDef.skillName = "BAESwappableGrenade";
            ChargeSwappableGrenadesSkillDef.skillNameToken = "Bouncing Grenades and Seeker Swarm";

            ChargeSwappableGrenadesSkillDef.skillDescriptionToken =
                "Current Key Bind for switching : <style=cIsUtility>" + Configuration.GrenadeTypeKeyBind.Value + "</style>" +
                Environment.NewLine +  "Bouncing : Charge up to <style=cIsDamage>8</style> grenades that deal <style=cIsDamage>100% damage</style> each." +
                Environment.NewLine +  "Seeker : Auto-targeting. Charge up to <style=cIsDamage>" +
                Configuration.SeekerMissileMaxProjectileNumber.Value + "</style> missiles that deal <style=cIsDamage>" +
                Configuration.SeekerMissileDamageCoefficient.Value * 100f + "% damage</style> each.";

            ChargeSwappableGrenadesSkillDef.activationStateMachineName = "Weapon";
            ChargeSwappableGrenadesSkillDef.activationState = new SerializableEntityStateType(typeof(ChargeGrenades));
            ChargeSwappableGrenadesSkillDef.interruptPriority = InterruptPriority.Any;
            ChargeSwappableGrenadesSkillDef.baseRechargeInterval = 0;
            ChargeSwappableGrenadesSkillDef.baseMaxStock = 1;
            ChargeSwappableGrenadesSkillDef.rechargeStock = 1;
            ChargeSwappableGrenadesSkillDef.beginSkillCooldownOnSkillEnd = false;
            ChargeSwappableGrenadesSkillDef.requiredStock = 1;
            ChargeSwappableGrenadesSkillDef.stockToConsume = 1;
            ChargeSwappableGrenadesSkillDef.isCombatSkill = true;
            ChargeSwappableGrenadesSkillDef.cancelSprintingOnActivation = true;
            ChargeSwappableGrenadesSkillDef.canceledFromSprinting = false;
            ChargeSwappableGrenadesSkillDef.mustKeyPress = false;
            ChargeSwappableGrenadesSkillDef.icon = BaeAssets.IconSwappableGrenades;
        }

        private static void SwappableMine()
        {
            SwappableMineSkillDef = ScriptableObject.CreateInstance<SkillDef>();
            ((ScriptableObject)SwappableMineSkillDef).name = "BAESwappableMine";
            SwappableMineSkillDef.rechargeStock = 1;
            SwappableMineSkillDef.requiredStock = 1;
            SwappableMineSkillDef.stockToConsume = 1;
            SwappableMineSkillDef.fullRestockOnAssign = false;
            SwappableMineSkillDef.skillName = "BAESwappableMine";

            SwappableMineSkillDef.skillNameToken = "Cluster and Satchel Pressured Mines";
            SwappableMineSkillDef.skillDescriptionToken =
                Environment.NewLine +
                Environment.NewLine + "Current Key Bind for switching : <style=cIsUtility>" + Configuration.MineTypeKeyBind.Value + "</style>. " +
                "Current Key Bind for detonating Satchels : <style=cIsUtility>" + Configuration.SatchelManualDetonateKeyBind.Value + "</style>" +
                Environment.NewLine + "<style=cIsUtility>Cluster</style> : Place a mine that explode into <style=cIsDamage>rebars</style> for <style=cIsDamage>30x" +
                Configuration.ClusterMineDamageCoefficient.Value * 100f +
                "% damage</style> when an enemy walks nearby. " +
                Environment.NewLine + "The first mine and the second set of mines explodes into <style=cIsDamage>3</style> mines." +
                Environment.NewLine + "<style=cIsUtility>Satchel</style> : Place a mine that will explode for <style=cIsDamage>" +
                Configuration.SatchelMineDamageCoefficient.Value * 100f +
                "% damage</style> when an enemy walks nearby. \n<style=cIsDamage>Can be manually detonated</style>. <style=cIsUtility>Knockback nearby units</style>." +
                Environment.NewLine + "Base stock : " +
                Configuration.ClusterMineBaseMaxStock.Value + "." +
                Environment.NewLine + Environment.NewLine;

            SwappableMineSkillDef.baseMaxStock = Configuration.ClusterMineBaseMaxStock.Value;
            SwappableMineSkillDef.baseRechargeInterval = Configuration.ClusterMineCooldown.Value;
            SwappableMineSkillDef.activationStateMachineName = "Weapon";
            SwappableMineSkillDef.activationState = new SerializableEntityStateType(typeof(FireClusterMines));
            SwappableMineSkillDef.beginSkillCooldownOnSkillEnd = true;
            SwappableMineSkillDef.interruptPriority = 0;
            SwappableMineSkillDef.isCombatSkill = true;
            SwappableMineSkillDef.cancelSprintingOnActivation = true;
            SwappableMineSkillDef.canceledFromSprinting = false;
            SwappableMineSkillDef.mustKeyPress = false;
            SwappableMineSkillDef.icon = BaeAssets.IconSwappableMines;
        }

        private static void SatchelMine()
        {
            SatchelMineSkillDef = ScriptableObject.CreateInstance<SkillDef>();
            SatchelMineSkillDef.rechargeStock = 1;
            SatchelMineSkillDef.requiredStock = 1;
            SatchelMineSkillDef.stockToConsume = 1;
            SatchelMineSkillDef.fullRestockOnAssign = false;
            ((ScriptableObject)SatchelMineSkillDef).name = "BAESatchelPressuredMine";
            SatchelMineSkillDef.skillName = "BAESatchelPressuredMine";

            SatchelMineSkillDef.skillNameToken = "Satchel Pressured Mines";
            SatchelMineSkillDef.skillDescriptionToken =
                Environment.NewLine +
                "Current Key Bind for detonating Satchels : <style=cIsUtility>" +
                Configuration.SatchelManualDetonateKeyBind.Value + "</style>" +
                Environment.NewLine +
                "<style=cIsUtility>Satchel</style> : Place a mine that will explode for <style=cIsDamage>" +
                Configuration.SatchelMineDamageCoefficient.Value * 100f +
                "% damage</style> when an enemy walks nearby. \n<style=cIsDamage>Can be manually detonated</style>. <style=cIsUtility>Knockback nearby units</style>.";

            SatchelMineSkillDef.baseMaxStock = Configuration.ClusterMineBaseMaxStock.Value;
            SatchelMineSkillDef.baseRechargeInterval = Configuration.ClusterMineCooldown.Value;
            SatchelMineSkillDef.activationStateMachineName = "Weapon";
            SatchelMineSkillDef.activationState = new SerializableEntityStateType(typeof(FireSatchelMines));
            SatchelMineSkillDef.beginSkillCooldownOnSkillEnd = true;
            SatchelMineSkillDef.interruptPriority = 0;
            SatchelMineSkillDef.isCombatSkill = true;
            SatchelMineSkillDef.cancelSprintingOnActivation = true;
            SatchelMineSkillDef.canceledFromSprinting = false;
            SatchelMineSkillDef.mustKeyPress = false;
            SatchelMineSkillDef.icon = _engiSkillLocator.secondary._skillFamily.variants[0].skillDef.icon;
        }

        private static void OrbitalStrikeMine()
        {
            OrbitalStrikeSkillDef = ScriptableObject.CreateInstance<SkillDef>();
            OrbitalStrikeSkillDef.baseRechargeInterval = Configuration.OrbitalStrikeCooldown.Value;
            OrbitalStrikeSkillDef.baseMaxStock = Configuration.OrbitalStrikeRequiredStock.Value;
            OrbitalStrikeSkillDef.rechargeStock = 1;
            OrbitalStrikeSkillDef.requiredStock = Configuration.OrbitalStrikeRequiredStock.Value;
            OrbitalStrikeSkillDef.stockToConsume = Configuration.OrbitalStrikeRequiredStock.Value;
            OrbitalStrikeSkillDef.fullRestockOnAssign = false;
            ((ScriptableObject)OrbitalStrikeSkillDef).name = "BAEOrbitalMine";
            OrbitalStrikeSkillDef.skillName = "BAEOrbitalMine";
            OrbitalStrikeSkillDef.skillNameToken = "Orbital Strike";

            OrbitalStrikeSkillDef.skillDescriptionToken =
                Environment.NewLine + "Fire an Orbital Strike at the mine position." +
                Environment.NewLine + "Require <style=cIsUtility>100 stocks</style> to be used." +
                Environment.NewLine + "<style=cIsUtility>3 Second cooldown for each charges</style>." +
                Environment.NewLine + "The <style=cIsDamage>cooldown is shared across stages</style>.";

            OrbitalStrikeSkillDef.activationStateMachineName = "Weapon";
            OrbitalStrikeSkillDef.activationState = new SerializableEntityStateType(typeof(FireOrbitalMines));
            OrbitalStrikeSkillDef.beginSkillCooldownOnSkillEnd = true;
            OrbitalStrikeSkillDef.interruptPriority = 0;
            OrbitalStrikeSkillDef.isCombatSkill = true;
            OrbitalStrikeSkillDef.cancelSprintingOnActivation = false;
            OrbitalStrikeSkillDef.canceledFromSprinting = false;
            OrbitalStrikeSkillDef.mustKeyPress = true;
            OrbitalStrikeSkillDef.icon = BaeAssets.IconOrbitalStrike;
        }

        private static void SwappableTurret()
        {
            SwappableTurretSkillDef = ScriptableObject.CreateInstance<SkillDef>();
            ((ScriptableObject)SwappableTurretSkillDef).name = "BAESwappableTurret";
            SwappableTurretSkillDef.skillName = "BAESwappableTurret";
            SwappableTurretSkillDef.skillNameToken = "TR69 MultiFunctional Auto-Turret";

            SwappableTurretSkillDef.skillDescriptionToken =
                Environment.NewLine +
                Environment.NewLine +
                Environment.NewLine + "Current Key Bind for switching : <style=cIsUtility>" + Configuration.TurretTypeKeyBind.Value + "</style>" +
                Environment.NewLine + "Place a turret that <style=cIsUtility>inherits all your items.</style> You can <color=green>ping</color> your turrets to swap between the modes." +
                Environment.NewLine + "<style=cIsUtility>Default Mode</style> : Fire a cannon for <style=cIsDamage>" +
                Configuration.DefaultTurretDamageCoefficient.Value * 100 + " damage</style> with <style=cIsUtility>" +
                Configuration.DefaultTurretAttackSpeed.Value + " base attack speed</style>." +
                Environment.NewLine + "<style=cIsUtility>Minigun Mode</style> : Fire a minigun for <style=cIsDamage>" +
                Configuration.MinigunTurretDamageCoefficient.Value * 100 + " damage</style> with <style=cIsUtility>" +
                Configuration.MinigunTurretAttackSpeed.Value + " base attack speed</style>." +
                Environment.NewLine + "<style=cIsUtility>Railgun Mode</style> : Fire a railgun for <style=cIsDamage>" +
                Configuration.RailgunTurretDamageCoefficient.Value * 100 + " damage</style> with <style=cIsUtility>" +
                Configuration.RailgunTurretAttackSpeed.Value + " base attack speed</style>." +
                Environment.NewLine + "<style=cIsUtility>Shotgun Mode</style> : Fire rockets for <style=cIsDamage>" +
                Configuration.ShotgunTurretDamageCoefficient.Value * 100 + " damage</style> with <style=cIsUtility>" +
                Configuration.ShotgunTurretAttackSpeed.Value + " base attack speed</style>." +
                Environment.NewLine + "Can place up to 2.";

            SwappableTurretSkillDef.activationStateMachineName = "Weapon";
            SwappableTurretSkillDef.activationState = new SerializableEntityStateType(typeof(PlaceTurret));
            SwappableTurretSkillDef.interruptPriority = InterruptPriority.Skill;
            SwappableTurretSkillDef.baseRechargeInterval = 30;
            SwappableTurretSkillDef.baseMaxStock = 2;
            SwappableTurretSkillDef.rechargeStock = 1;
            SwappableTurretSkillDef.beginSkillCooldownOnSkillEnd = false;
            SwappableTurretSkillDef.requiredStock = 1;
            SwappableTurretSkillDef.stockToConsume = 0;
            SwappableTurretSkillDef.isCombatSkill = false;
            SwappableTurretSkillDef.cancelSprintingOnActivation = true;
            SwappableTurretSkillDef.canceledFromSprinting = false;
            SwappableTurretSkillDef.mustKeyPress = false;
            SwappableTurretSkillDef.fullRestockOnAssign = false;
            SwappableTurretSkillDef.icon = BaeAssets.IconSwappableTurrets;
        }

        private static void RegisterStatesTypes()
        {
            RegisterClusterMinesStates();
            RegisterSatchelMinesStates();
            RegisterOrbitalMinesStates();
        }

        private static void RegisterClusterMinesStates()
        {
            ContentAddition.AddEntityState<MineArmingFullCluster>(out _);
            ContentAddition.AddEntityState<MineArmingUnarmedCluster>(out _);
            ContentAddition.AddEntityState<MineArmingWeakCluster>(out _);

            ContentAddition.AddEntityState<ArmCluster>(out _);
            ContentAddition.AddEntityState<DetonateCluster>(out _);
            ContentAddition.AddEntityState<PreDetonateCluster>(out _);
            ContentAddition.AddEntityState<WaitForStickCluster>(out _);
            ContentAddition.AddEntityState<WaitForTargetCluster>(out _);
        }

        private static void RegisterSatchelMinesStates()
        {
            ContentAddition.AddEntityState<MineArmingFullSatchel>(out _);
            ContentAddition.AddEntityState<MineArmingUnarmedSatchel>(out _);
            ContentAddition.AddEntityState<MineArmingWeakSatchel>(out _);

            ContentAddition.AddEntityState<ArmSatchel>(out _);
            ContentAddition.AddEntityState<DetonateSatchel>(out _);
            ContentAddition.AddEntityState<PreDetonateSatchel>(out _);
            ContentAddition.AddEntityState<WaitForStickSatchel>(out _);
            ContentAddition.AddEntityState<WaitForTargetSatchel>(out _);
        }

        private static void RegisterOrbitalMinesStates()
        {
            ContentAddition.AddEntityState<MineArmingFullOrbital>(out _);
            ContentAddition.AddEntityState<MineArmingUnarmedOrbital>(out _);
            ContentAddition.AddEntityState<MineArmingWeakOrbital>(out _);

            ContentAddition.AddEntityState<ArmOrbital>(out _);
            ContentAddition.AddEntityState<DetonateOrbital>(out _);
            ContentAddition.AddEntityState<PreDetonateOrbital>(out _);
            ContentAddition.AddEntityState<WaitForStickOrbital>(out _);
            ContentAddition.AddEntityState<WaitForTargetOrbital>(out _);
        }
    }
}
