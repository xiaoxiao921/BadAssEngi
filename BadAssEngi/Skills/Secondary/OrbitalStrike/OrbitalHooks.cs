using System.Collections;
using MonoMod.RuntimeDetour;
using R2API.Utils;
using RoR2;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.Networking;

// ReSharper disable UnusedMember.Local

namespace BadAssEngi.Skills.Secondary.OrbitalStrike
{
    internal static class OrbitalHooks
    {
        private delegate void DServerChangeScene(NetworkManager instance, string newSceneName);

        private static DServerChangeScene _origServerChangeScene;
        private static Hook _onServerChangeSceneHook;

        private delegate void DClientChangeScene(NetworkManager instance, string newSceneName, bool forceReload);

        private static DClientChangeScene _origClientChangeScene;
        private static Hook _onClientChangeSceneHook;

        internal static void Init()
        {
            On.RoR2.Projectile.ProjectileStickOnImpact.Detach += OrbitalStrikeMineDoNotStick;

            _onServerChangeSceneHook = new Hook(typeof(NetworkManager).GetMethodCached("ServerChangeScene"),
                typeof(OrbitalHooks).GetMethodCached("KeepOrbitalCdServerScene"));
            _origServerChangeScene = _onServerChangeSceneHook.GenerateTrampoline<DServerChangeScene>();
            _onClientChangeSceneHook = new Hook(typeof(NetworkManager).GetMethodCached("ClientChangeScene"),
                typeof(OrbitalHooks).GetMethodCached("KeepOrbitalCdClientScene"));
            _origClientChangeScene = _onClientChangeSceneHook.GenerateTrampoline<DClientChangeScene>();

            On.RoR2.TeleportOutController.AddTPOutEffect += KeepOrbitalCd;
        }

        private static void OrbitalStrikeMineDoNotStick(On.RoR2.Projectile.ProjectileStickOnImpact.orig_Detach orig, ProjectileStickOnImpact self)
        {
            var pc = self.GetComponent<ProjectileController>();
            if (pc && pc.owner && self.name.Equals("EngiMine(Clone)"))
            {
                var cb = pc.owner.GetComponent<CharacterBody>();

                if (cb == null || !cb)
                    return;

                var cm = cb.master;
                var skillVariant = cm.loadout.bodyLoadoutManager.GetSkillVariant(cb.bodyIndex, 1);

                if (skillVariant != SkillLoader.OrbitalStrikeSkillVariant)
                    orig(self);
            }
            else
            {
                orig(self);
            }
        }

        private static void KeepOrbitalCdServerScene(NetworkManager instance, string newSceneName)
        {
            TryRestoringOrbitalCd();

            _origServerChangeScene(instance, newSceneName);
        }

        private static void KeepOrbitalCdClientScene(NetworkManager instance, string newSceneName, bool forceReload)
        {
            TryRestoringOrbitalCd();

            _origClientChangeScene(instance, newSceneName, forceReload);
        }

        private static void KeepOrbitalCd(On.RoR2.TeleportOutController.orig_AddTPOutEffect orig, CharacterModel characterModel, float beginAlpha, float endAlpha, float duration)
        {
            TryRestoringOrbitalCd();

            orig(characterModel, beginAlpha, endAlpha, duration);
        }

        private static void TryRestoringOrbitalCd()
        {
            foreach (var localUser in LocalUserManager.readOnlyLocalUsersList)
            {
                var nu = localUser.currentNetworkUser;
                if (nu)
                {
                    var body = nu.GetCurrentBody();
                    if (body && body.baseNameToken.ToLower().Contains("engi"))
                    {
                        var skillLocator = body.GetComponent<SkillLocator>();
                        var orbitalGenericSkill = skillLocator.secondary;

                        var master = body.master;
                        var bodyLoadoutManager = master?.loadout.bodyLoadoutManager;
                        var skillVariant = bodyLoadoutManager?.GetSkillVariant(body.bodyIndex, 1);

                        if (skillVariant != null && skillVariant == SkillLoader.OrbitalStrikeSkillVariant)
                        {
                            BadAssEngi.Instance.StartCoroutine(RestoreOrbitalCooldown(master, orbitalGenericSkill, 1.5f));
                        }
                    }
                }
            }
        }

        private static IEnumerator RestoreOrbitalCooldown(CharacterMaster master, GenericSkill oldOrbitalSkill, float delayInSecond)
        {
            yield return new WaitForSeconds(delayInSecond);

            var newBody = master.GetBody();
            if (newBody)
            {
                var skillLocator = newBody.skillLocator;
                var newOrbital = skillLocator.secondary;

                if (newOrbital != oldOrbitalSkill)
                {
                    newOrbital.stock = oldOrbitalSkill.stock;
                    newOrbital.rechargeStopwatch = oldOrbitalSkill.rechargeStopwatch;
                }
                else
                {
                    BadAssEngi.Instance.StartCoroutine(RestoreOrbitalCooldown(master, oldOrbitalSkill, delayInSecond));
                }
            }
            else
            {
                BadAssEngi.Instance.StartCoroutine(RestoreOrbitalCooldown(master, oldOrbitalSkill, delayInSecond));
            }
        }
    }
}
