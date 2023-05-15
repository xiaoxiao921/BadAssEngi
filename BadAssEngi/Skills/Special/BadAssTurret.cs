using BadAssEngi.Assets.Sound;
using RoR2;
using UnityEngine;

// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedMember.Global
// ReSharper disable ClassNeverInstantiated.Global

namespace BadAssEngi.Skills.Special
{
    internal enum TurretType
    {
        Default,
        Minigun,
        Railgun,
        Shotgun,
    }

    internal class BadAssTurret : MonoBehaviour
    {
        internal TurretType Index;
        internal uint SoundGunId;
        internal CharacterMaster OwnerCharacterMaster;

        internal static BuffIndex[] Buffs;

        internal void Init()
        {
            OwnerCharacterMaster.inventory.onInventoryChanged += UpdateInventory;
            On.RoR2.CharacterBody.UpdateBuffs += CharacterBodyOnUpdateBuffs;

            AkSoundEngine.PostEvent(SoundHelper.TurretAlive, gameObject);

            if (Index == TurretType.Railgun)
            {
                SoundGunId = AkSoundEngine.PostEvent(SoundHelper.RailGunTurretTargeting, gameObject);
            }
            else if (Index == TurretType.Shotgun)
            {
                SoundGunId = AkSoundEngine.PostEvent(SoundHelper.RocketTurretReload, gameObject);
            }
        }

        private void UpdateInventory()
        {
            if (gameObject == null)
            {
                DestroyImmediate(this);
                return;
            }

            var currentCm = gameObject.GetComponent<CharacterMaster>();
            if (currentCm == null || !currentCm)
            {
                return;
            }

            var turretInv = currentCm.inventory;
            if (OwnerCharacterMaster.inventory == null || !OwnerCharacterMaster.inventory)
            {
                OwnerCharacterMaster = gameObject.GetComponent<Deployable>().ownerMaster;
            }

            var itemCount = turretInv.GetItemCount(RoR2Content.Items.ExtraLife.itemIndex);
            var itemCount2 = turretInv.GetItemCount(RoR2Content.Items.ExtraLifeConsumed.itemIndex);

            turretInv.CopyItemsFrom(OwnerCharacterMaster.inventory);

            turretInv.ResetItem(RoR2Content.Items.WardOnLevel.itemIndex);
            turretInv.ResetItem(RoR2Content.Items.BeetleGland.itemIndex);
            turretInv.ResetItem(RoR2Content.Items.CrippleWardOnLevel.itemIndex);
            turretInv.ResetItem(RoR2Content.Items.ExtraLife.itemIndex);
            turretInv.ResetItem(RoR2Content.Items.ExtraLifeConsumed.itemIndex);
            turretInv.GiveItem(RoR2Content.Items.ExtraLife.itemIndex, itemCount);
            turretInv.GiveItem(RoR2Content.Items.ExtraLifeConsumed.itemIndex, itemCount2);

            if (turretInv.infusionBonus > OwnerCharacterMaster.inventory.infusionBonus)
            {
                OwnerCharacterMaster.inventory.infusionBonus = turretInv.infusionBonus;
                OwnerCharacterMaster.GetBody().RecalculateStats();
            }
            else
            {
                turretInv.infusionBonus = OwnerCharacterMaster.inventory.infusionBonus;
                currentCm.GetBody().RecalculateStats();
            }
        }

        // This is too messy
        private void CharacterBodyOnUpdateBuffs(On.RoR2.CharacterBody.orig_UpdateBuffs orig, CharacterBody self, float deltaTime)
        {
            orig(self, deltaTime);

            if (gameObject == null)
            {
                DestroyImmediate(this);
                return;
            }

            var currentCm = gameObject.GetComponent<CharacterMaster>();
            if (currentCm == null || !currentCm)
            {
                return;
            }

            if (currentCm.GetBody() == null || !currentCm.GetBody())
            {
                return;
            }

            if (OwnerCharacterMaster == null || !OwnerCharacterMaster)
            {
                return;
            }

            if (OwnerCharacterMaster.GetBody() == null || !OwnerCharacterMaster.GetBody())
            {
                return;
            }

            foreach (var buffType in Buffs)
            {
                var characterMaster = gameObject.GetComponent<CharacterMaster>();

                if (OwnerCharacterMaster.GetBody().HasBuff(buffType))
                {
                    if (buffType == RoR2Content.Buffs.AffixBlue.buffIndex ||
                        buffType == RoR2Content.Buffs.AffixWhite.buffIndex ||
                        buffType == RoR2Content.Buffs.AffixRed.buffIndex ||
                        buffType == RoR2Content.Buffs.AffixPoison.buffIndex ||
                        buffType == RoR2Content.Buffs.NoCooldowns.buffIndex)
                    {
                        if (characterMaster.GetBody().HasBuff(buffType))
                            continue;

                        var timedBuffs = OwnerCharacterMaster.GetBody().timedBuffs;
                        foreach (var timedBuff in timedBuffs)
                        {
                            if (timedBuff.buffIndex == buffType)
                            {
                                var buffDuration = timedBuff.timer;
                                if (buffDuration <= 1f)
                                    break;

                                characterMaster.GetBody().AddTimedBuff(buffType, buffDuration);
                                break;
                            }
                        }
                    }
                    else if (buffType != RoR2Content.Buffs.NoCooldowns.buffIndex)
                    {
                        characterMaster.GetBody().AddBuff(buffType);
                    }
                }
                else if (characterMaster.GetBody().HasBuff(buffType))
                {
                    if (buffType == RoR2Content.Buffs.AffixBlue.buffIndex || buffType == RoR2Content.Buffs.AffixWhite.buffIndex ||
                        buffType == RoR2Content.Buffs.AffixRed.buffIndex || buffType == RoR2Content.Buffs.AffixPoison.buffIndex ||
                        buffType == RoR2Content.Buffs.NoCooldowns.buffIndex)
                    {
                        if (OwnerCharacterMaster.GetBody().HasBuff(buffType))
                            continue;

                        var timedBuffs = characterMaster.GetBody().timedBuffs;
                        foreach (var timedBuff in timedBuffs)
                        {
                            if (timedBuff.buffIndex == buffType)
                            {
                                var buffDuration = timedBuff.timer;
                                if (buffDuration <= 1f)
                                    break;

                                OwnerCharacterMaster.GetBody().AddTimedBuff(buffType, buffDuration);
                                break;
                            }
                        }
                    }
                    else
                    {
                        characterMaster.GetBody().RemoveBuff(buffType);
                    }
                }
            }
        }

        private void OnDisable()
        {
            OwnerCharacterMaster.inventory.onInventoryChanged -= UpdateInventory;
            On.RoR2.CharacterBody.UpdateBuffs -= CharacterBodyOnUpdateBuffs;
            AkSoundEngine.StopPlayingID(SoundGunId);

            DestroyImmediate(this);
        }
    }
}
