using BadAssEngi.Assets.Sound;
using RoR2;
using UnityEngine;

// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedMember.Global
// ReSharper disable ClassNeverInstantiated.Global

namespace BadAssEngi.Skills.Special
{
    internal class BadAssTurret : MonoBehaviour
    {
        internal int Index;
        internal uint SoundGunId;
        internal CharacterMaster OwnerCharacterMaster;

        internal static BuffIndex[] Buffs;

        internal void Init()
        {
            OwnerCharacterMaster.inventory.onInventoryChanged += UpdateInventory;
            On.RoR2.CharacterBody.UpdateBuffs += CharacterBodyOnUpdateBuffs;
            
            AkSoundEngine.PostEvent(SoundHelper.TurretAlive, gameObject);

            if (Index == 2)
            {
                SoundGunId = AkSoundEngine.PostEvent(SoundHelper.RailGunTurretTargeting, gameObject);
            }
            else if (Index == 3)
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
                
            var itemCount = turretInv.GetItemCount(ItemIndex.ExtraLife);
            var itemCount2 = turretInv.GetItemCount(ItemIndex.ExtraLifeConsumed);

            turretInv.CopyItemsFrom(OwnerCharacterMaster.inventory);

            turretInv.ResetItem(ItemIndex.WardOnLevel);
            turretInv.ResetItem(ItemIndex.BeetleGland);
            turretInv.ResetItem(ItemIndex.CrippleWardOnLevel);
            turretInv.ResetItem(ItemIndex.ExtraLife);
            turretInv.ResetItem(ItemIndex.ExtraLifeConsumed);
            turretInv.GiveItem(ItemIndex.ExtraLife, itemCount);
            turretInv.GiveItem(ItemIndex.ExtraLifeConsumed, itemCount2);

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
                    if (buffType == BuffIndex.AffixBlue || buffType == BuffIndex.AffixWhite ||
                        buffType == BuffIndex.AffixRed || buffType == BuffIndex.AffixPoison ||
                        buffType == BuffIndex.NoCooldowns)
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
                    else if (buffType != BuffIndex.NoCooldowns)
                    {
                        characterMaster.GetBody().AddBuff(buffType);
                    }   
                }
                else if (characterMaster.GetBody().HasBuff(buffType))
                {
                    if (buffType == BuffIndex.AffixBlue || buffType == BuffIndex.AffixWhite ||
                        buffType == BuffIndex.AffixRed || buffType == BuffIndex.AffixPoison ||
                        buffType == BuffIndex.NoCooldowns)
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
