﻿using System;
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

            _engiSkillLocator.primary.SetSkillFamily(skills.ToArray());
        }

        private static void InitEngiSecondarySkills()
        {
            SwappableMine();
            SatchelMine();
            OrbitalStrikeMine();

            var skills = _engiSkillLocator.secondary._skillFamily.variants
                .Select(variant => variant.skillDef).ToList();

            SwappableMineSkillVariant = skills.Count;
            skills.Add(SwappableMineSkillDef);

            OrbitalStrikeSkillVariant = skills.Count;
            skills.Add(OrbitalStrikeSkillDef);

            _engiSkillLocator.secondary.SetSkillFamily(skills.ToArray());
        }

        private static void InitEngiSpecialSkills()
        {
            SwappableTurret();

            var skills = _engiSkillLocator.special._skillFamily.variants
                .Select(variant => variant.skillDef).ToList();

            SwappableTurretSkillVariant = skills.Count;
            skills.Add(SwappableTurretSkillDef);

            _engiSkillLocator.special.SetSkillFamily(skills.ToArray());
        }

        private static void SeekerMissile()
        {
            ChargeSeekerMissileSkillDef = ScriptableObject.CreateInstance<SkillDef>();
            ChargeSeekerMissileSkillDef.skillName = "SeekerSwarm";
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
            ChargeSeekerMissileSkillDef.isBullets = false;
            ChargeSeekerMissileSkillDef.shootDelay = 0.1f;
            ChargeSeekerMissileSkillDef.beginSkillCooldownOnSkillEnd = false;
            ChargeSeekerMissileSkillDef.requiredStock = 1;
            ChargeSeekerMissileSkillDef.stockToConsume = 1;
            ChargeSeekerMissileSkillDef.isCombatSkill = true;
            ChargeSeekerMissileSkillDef.noSprint = true;
            ChargeSeekerMissileSkillDef.canceledFromSprinting = false;
            ChargeSeekerMissileSkillDef.mustKeyPress = false;
            ChargeSeekerMissileSkillDef.icon = BaeAssets.IconMissileM1;
        }

        private static void SwappableGrenade()
        {
            ChargeSwappableGrenadesSkillDef = ScriptableObject.CreateInstance<SkillDef>();
            ChargeSwappableGrenadesSkillDef.skillName = "SwappableGrenade";
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
            ChargeSwappableGrenadesSkillDef.isBullets = false;
            ChargeSwappableGrenadesSkillDef.shootDelay = 0.1f;
            ChargeSwappableGrenadesSkillDef.beginSkillCooldownOnSkillEnd = false;
            ChargeSwappableGrenadesSkillDef.requiredStock = 1;
            ChargeSwappableGrenadesSkillDef.stockToConsume = 1;
            ChargeSwappableGrenadesSkillDef.isCombatSkill = true;
            ChargeSwappableGrenadesSkillDef.noSprint = true;
            ChargeSwappableGrenadesSkillDef.canceledFromSprinting = false;
            ChargeSwappableGrenadesSkillDef.mustKeyPress = false;
            ChargeSwappableGrenadesSkillDef.icon = BaeAssets.IconSwappableGrenades;
        }

        private static void SwappableMine()
        {
            SwappableMineSkillDef = ScriptableObject.CreateInstance<SkillDef>();
            SwappableMineSkillDef.rechargeStock = 1;
            SwappableMineSkillDef.requiredStock = 1;
            SwappableMineSkillDef.stockToConsume = 1;
            SwappableMineSkillDef.fullRestockOnAssign = false;
            SwappableMineSkillDef.shootDelay = 0.3f;
            SwappableMineSkillDef.skillName = "SwappableMine";

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
            SwappableMineSkillDef.isBullets = false;
            SwappableMineSkillDef.beginSkillCooldownOnSkillEnd = true;
            SwappableMineSkillDef.interruptPriority = 0;
            SwappableMineSkillDef.isCombatSkill = true;
            SwappableMineSkillDef.noSprint = true;
            SwappableMineSkillDef.canceledFromSprinting = false;
            SwappableMineSkillDef.mustKeyPress = false;
            SwappableMineSkillDef.icon = BaeAssets.IconSwappableMines;
        }

        private static void SatchelMine()
        {
            SatchelMineSkillDef = ScriptableObject.CreateInstance<SkillDef>();
            SatchelMineSkillDef.rechargeStock = 1;
            SwappableMineSkillDef.requiredStock = 1;
            SwappableMineSkillDef.stockToConsume = 1;
            SwappableMineSkillDef.fullRestockOnAssign = false;
            SwappableMineSkillDef.shootDelay = 0.3f;
            SwappableMineSkillDef.skillName = "SwappableMine";

            SwappableMineSkillDef.skillNameToken = "Satchel Pressured Mines";
            SwappableMineSkillDef.skillDescriptionToken =
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
            SatchelMineSkillDef.isBullets = false;
            SatchelMineSkillDef.beginSkillCooldownOnSkillEnd = true;
            SatchelMineSkillDef.interruptPriority = 0;
            SatchelMineSkillDef.isCombatSkill = true;
            SatchelMineSkillDef.noSprint = true;
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
            OrbitalStrikeSkillDef.skillName = "OrbitalMine";
            OrbitalStrikeSkillDef.skillNameToken = "Orbital Strike";

            OrbitalStrikeSkillDef.skillDescriptionToken =
                Environment.NewLine + "Fire an Orbital Strike at the mine position." +
                Environment.NewLine + "Require <style=cIsUtility>100 stocks</style> to be used." +
                Environment.NewLine + "<style=cIsUtility>3 Second cooldown for each charges</style>." +
                Environment.NewLine + "The <style=cIsDamage>cooldown is shared across stages</style>.";

            OrbitalStrikeSkillDef.activationStateMachineName = "Weapon";
            OrbitalStrikeSkillDef.activationState = new SerializableEntityStateType(typeof(FireOrbitalMines));
            OrbitalStrikeSkillDef.isBullets = false;
            OrbitalStrikeSkillDef.beginSkillCooldownOnSkillEnd = true;
            OrbitalStrikeSkillDef.interruptPriority = 0;
            OrbitalStrikeSkillDef.isCombatSkill = true;
            OrbitalStrikeSkillDef.noSprint = false;
            OrbitalStrikeSkillDef.canceledFromSprinting = false;
            OrbitalStrikeSkillDef.mustKeyPress = true;
            OrbitalStrikeSkillDef.icon = BaeAssets.IconOrbitalStrike;
        }

        private static void SwappableTurret()
        {
            SwappableTurretSkillDef = ScriptableObject.CreateInstance<SkillDef>();
            SwappableTurretSkillDef.skillName = "SwappableTurret";
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
            SwappableTurretSkillDef.isBullets = false;
            SwappableTurretSkillDef.shootDelay = 0;
            SwappableTurretSkillDef.beginSkillCooldownOnSkillEnd = false;
            SwappableTurretSkillDef.requiredStock = 1;
            SwappableTurretSkillDef.stockToConsume = 0;
            SwappableTurretSkillDef.isCombatSkill = false;
            SwappableTurretSkillDef.noSprint = true;
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
            LoadoutAPI.AddSkill(typeof(MineArmingFullCluster));
            LoadoutAPI.AddSkill(typeof(MineArmingUnarmedCluster));
            LoadoutAPI.AddSkill(typeof(MineArmingWeakCluster));

            LoadoutAPI.AddSkill(typeof(ArmCluster));
            LoadoutAPI.AddSkill(typeof(DetonateCluster));
            LoadoutAPI.AddSkill(typeof(PreDetonateCluster));
            LoadoutAPI.AddSkill(typeof(WaitForStickCluster));
            LoadoutAPI.AddSkill(typeof(WaitForTargetCluster));
        }

        private static void RegisterSatchelMinesStates()
        {
            LoadoutAPI.AddSkill(typeof(MineArmingFullSatchel));
            LoadoutAPI.AddSkill(typeof(MineArmingUnarmedSatchel));
            LoadoutAPI.AddSkill(typeof(MineArmingWeakSatchel));

            LoadoutAPI.AddSkill(typeof(ArmSatchel));
            LoadoutAPI.AddSkill(typeof(DetonateSatchel));
            LoadoutAPI.AddSkill(typeof(PreDetonateSatchel));
            LoadoutAPI.AddSkill(typeof(WaitForStickSatchel));
            LoadoutAPI.AddSkill(typeof(WaitForTargetSatchel));
        }

        private static void RegisterOrbitalMinesStates()
        {
            LoadoutAPI.AddSkill(typeof(MineArmingFullOrbital));
            LoadoutAPI.AddSkill(typeof(MineArmingUnarmedOrbital));
            LoadoutAPI.AddSkill(typeof(MineArmingWeakOrbital));

            LoadoutAPI.AddSkill(typeof(ArmOrbital));
            LoadoutAPI.AddSkill(typeof(DetonateOrbital));
            LoadoutAPI.AddSkill(typeof(PreDetonateOrbital));
            LoadoutAPI.AddSkill(typeof(WaitForStickOrbital));
            LoadoutAPI.AddSkill(typeof(WaitForTargetOrbital));
        }
    }
}