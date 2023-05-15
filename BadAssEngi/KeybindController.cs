using BadAssEngi.Animations;
using BadAssEngi.Networking;
using BadAssEngi.Skills.Primary.SeekerMissile;
using BadAssEngi.Skills.Secondary.ClusterMine;
using BadAssEngi.Skills.Secondary.SatchelMine.MineStates.ArmingStateMachine;
using BadAssEngi.Skills.Secondary.SatchelMine.MineStates.MainStateMachine;
using BadAssEngi.Skills.Special;
using BepInEx.Configuration;
using EntityStates;
using R2API.Networking;
using R2API.Networking.Interfaces;
using Rewired;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using SkillLoader = BadAssEngi.Skills.SkillLoader;

namespace BadAssEngi
{
    internal static class KeybindController
    {
        internal static void RetrieveFirstGamePad()
        {
            foreach (var controller in ReInput.controllers.Controllers)
            {
                var gamePad = controller.GetTemplate<IGamepadTemplate>();
                if (gamePad == null)
                    continue;

                Configuration.DPad = gamePad.dPad;
                break;
            }
        }

        internal static void Update()
        {
            foreach (var networkUser in NetworkUser.readOnlyLocalPlayersList)
            {
                var body = networkUser.GetCurrentBody();
                if (body && body.bodyIndex == BadAssEngi.EngiBodyIndex)
                {
                    var master = body.master;
                    var bodyLoadoutManager = master?.loadout.bodyLoadoutManager;
                    if (bodyLoadoutManager == null)
                        return;

                    var grenadeSkillVariant = bodyLoadoutManager.GetSkillVariant(body.bodyIndex, 0);
                    var mineSkillVariant = bodyLoadoutManager.GetSkillVariant(body.bodyIndex, 1);
                    var turretSkillVariant = bodyLoadoutManager.GetSkillVariant(body.bodyIndex, 3);

                    if (GetKeyBindInput(Configuration.TurretTypeKeyBind) &&
                        turretSkillVariant == SkillLoader.SwappableTurretSkillVariant)
                    {
                        switch (TurretTypeController.CurrentTurretType)
                        {
                            case TurretTypeController.TurretType.Default:
                                TurretTypeController.CurrentTurretType = TurretTypeController.TurretType.Minigun;
                                Chat.AddMessage(
                                    "<style=cIsUtility>Turret Type is now: </style><style=cDeath>[Minigun]</style>");
                                break;
                            case TurretTypeController.TurretType.Minigun:
                                TurretTypeController.CurrentTurretType = TurretTypeController.TurretType.Railgun;
                                Chat.AddMessage(
                                    "<style=cIsUtility>Turret Type is now: </style><style=cDeath>[Railgun]</style>");
                                break;
                            case TurretTypeController.TurretType.Railgun:
                                TurretTypeController.CurrentTurretType = TurretTypeController.TurretType.Shotgun;
                                Chat.AddMessage(
                                    "<style=cIsUtility>Turret Type is now: </style><style=cDeath>[Hydra Launcher]</style>");
                                break;
                            case TurretTypeController.TurretType.Shotgun:
                                TurretTypeController.CurrentTurretType = TurretTypeController.TurretType.Default;
                                Chat.AddMessage(
                                    "<style=cIsUtility>Turret Type is now: </style><style=cDeath>[WeakBoy]</style>");
                                break;
                        }
                    }

                    if (GetKeyBindInput(Configuration.GrenadeTypeKeyBind))
                    {
                        if (grenadeSkillVariant == SkillLoader.SwappableGrenadeSkillVariant)
                        {
                            var primarySkillSlot = body.skillLocator ? body.skillLocator.primary : null;
                            if (primarySkillSlot)
                            {
                                var missileTracker = body.GetComponent<MissileTracker>();
                                if (missileTracker)
                                {
                                    primarySkillSlot.UnsetSkillOverride(body, SkillLoader.ChargeSeekerMissileSkillDef, GenericSkill.SkillOverridePriority.Replacement);

                                    if (body && body.bodyIndex == BadAssEngi.EngiBodyIndex)
                                    {
                                        if (missileTracker)
                                        {
                                            Object.DestroyImmediate(missileTracker);
                                        }
                                    }

                                    Chat.AddMessage("<style=cIsUtility>Grenade Type is now: </style><style=cDeath>[Default]</style>");
                                }
                                else
                                {
                                    primarySkillSlot.SetSkillOverride(body, SkillLoader.ChargeSeekerMissileSkillDef, GenericSkill.SkillOverridePriority.Replacement);

                                    if (body && body.bodyIndex == BadAssEngi.EngiBodyIndex)
                                    {
                                        body.gameObject.AddComponent<MissileTracker>();
                                    }

                                    Chat.AddMessage("<style=cIsUtility>Grenade Type is now: </style><style=cDeath>[Seeker]</style>");
                                }
                            }
                        }
                    }

                    if (GetKeyBindInput(Configuration.MineTypeKeyBind) &&
                        mineSkillVariant == SkillLoader.SwappableMineSkillVariant)
                    {
                        var secondarySkillSlot = body.skillLocator ? body.skillLocator.secondary : null;
                        if (secondarySkillSlot)
                        {
                            var hasSkillOverride = secondarySkillSlot.skillDef == SkillLoader.SatchelMineSkillDef;
                            if (hasSkillOverride)
                            {
                                var oldCd = secondarySkillSlot.rechargeStopwatch;
                                var oldStock = secondarySkillSlot.stock;
                                secondarySkillSlot.UnsetSkillOverride(body, SkillLoader.SatchelMineSkillDef, GenericSkill.SkillOverridePriority.Replacement);
                                secondarySkillSlot.rechargeStopwatch = oldCd;
                                secondarySkillSlot.stock = oldStock;

                                Chat.AddMessage(
                                    "<style=cIsUtility>Mine Type is now: </style><style=cDeath>[Cluster]</style>");
                            }
                            else
                            {
                                var oldCd = secondarySkillSlot.rechargeStopwatch;
                                var oldStock = secondarySkillSlot.stock;
                                secondarySkillSlot.SetSkillOverride(body, SkillLoader.SatchelMineSkillDef, GenericSkill.SkillOverridePriority.Replacement);
                                secondarySkillSlot.rechargeStopwatch = oldCd;
                                secondarySkillSlot.stock = oldStock;

                                Chat.AddMessage(
                                    "<style=cIsUtility>Mine Type is now: </style><style=cDeath>[Satchel]</style>");
                            }
                        }
                    }

                    if (GetKeyBindInput(Configuration.SatchelManualDetonateKeyBind))
                    {
                        if (NetworkServer.active)
                        {
                            var deployableInfos = networkUser.master
                                .deployablesList;

                            if (deployableInfos != null && deployableInfos.Count >= 1)
                            {
                                foreach (var deployableInfo in deployableInfos)
                                {
                                    if (deployableInfo.slot == DeployableSlot.EngiMine)
                                    {
                                        var isSatchel =
                                            body.skillLocator &&
                                            body.skillLocator.secondary.skillDef == SkillLoader.SatchelMineSkillDef &&
                                            !deployableInfo.deployable.GetComponent<RecursiveMine>();
                                        if (isSatchel)
                                        {
                                            EntityStateMachine
                                            .FindByCustomName(deployableInfo.deployable.gameObject, "Arming")
                                            .SetNextState(new MineArmingFullSatchel());
                                            EntityStateMachine
                                                .FindByCustomName(deployableInfo.deployable.gameObject, "Main")
                                                .SetNextState(new DetonateSatchel());
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            new DetonateMsg { SenderUserNetId = networkUser.netId }.Send(NetworkDestination.Server);
                        }
                    }

                    // if player move, delete the engi that does the anim, and restore size.
                    if (body.characterMotor.velocity != Vector3.zero || body.inputBank.CheckAnyButtonDown())
                    {
                        if (EngiEmoteController.IsEmoting)
                        {
                            var stateMachine = body.gameObject.GetComponent<EntityStateMachine>();
                            if (stateMachine.CanInterruptState(InterruptPriority.Skill))
                            {
                                new StopAnimMsg { EngiNetId = body.GetComponent<NetworkIdentity>().netId }.Send(NetworkDestination.Clients);
                                EngiEmoteController.IsEmoting = false;
                            }
                        }
                    }
                }
            }
        }

        private static bool GetKeyBindInput(ConfigEntry<string> entry)
        {
            const string dpadUp = "dpadup";
            const string dpadDown = "dpaddown";
            const string dpadLeft = "dpadleft";
            const string dpadRight = "dpadright";

            if (entry.Value.Equals(dpadUp))
                return Configuration.DPad.up.justPressed;
            if (entry.Value.Equals(dpadDown))
                return Configuration.DPad.down.justPressed;
            if (entry.Value.Equals(dpadLeft))
                return Configuration.DPad.left.justPressed;
            if (entry.Value.Equals(dpadRight))
                return Configuration.DPad.right.justPressed;

            return Input.GetKeyDown(entry.Value);
        }
    }
}
