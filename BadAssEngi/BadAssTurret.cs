using System.Collections;
using System.Reflection;
using RoR2;
using UnityEngine;

// ReSharper disable UnusedMember.Global
// ReSharper disable ClassNeverInstantiated.Global

namespace BadAssEngi
{
    internal class BadAssTurret : MonoBehaviour
    {
        public int Index;
        public uint SoundRailGunTargetingId;
        public CharacterMaster OwnerCharacterMaster;

        public static BuffIndex[] Buffs;

        private static readonly FieldInfo TimedBuffsFieldInfo = typeof(CharacterBody).GetField("timedBuffs", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo TimedBufftimerFieldInfo = typeof(CharacterBody).GetNestedType("TimedBuff", BindingFlags.NonPublic | BindingFlags.Instance).GetField("timer");
        private static readonly FieldInfo TimedBuffbuffIndexFieldInfo = typeof(CharacterBody).GetNestedType("TimedBuff", BindingFlags.NonPublic | BindingFlags.Instance).GetField("buffIndex");

        public void Init()
        {
            OwnerCharacterMaster.inventory.onInventoryChanged += UpdateInventory;
            On.RoR2.CharacterBody.UpdateBuffs += CharacterBodyOnUpdateBuffs;
            
            AkSoundEngine.PostEvent(SoundHelper.TurretAlive, gameObject);

            if (Index == 2)
                SoundRailGunTargetingId = AkSoundEngine.PostEvent(SoundHelper.RailGunTurretTargeting, gameObject);
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

            var currentInventory = currentCm.inventory;
            if (OwnerCharacterMaster.inventory == null || !OwnerCharacterMaster.inventory)
            {
                OwnerCharacterMaster = gameObject.GetComponent<Deployable>().ownerMaster;
            }
                
            var itemCount = currentInventory.GetItemCount(ItemIndex.ExtraLife);
            var itemCount2 = currentInventory.GetItemCount(ItemIndex.ExtraLifeConsumed);

            currentInventory.CopyItemsFrom(OwnerCharacterMaster.inventory);

            currentInventory.ResetItem(ItemIndex.WardOnLevel);
            currentInventory.ResetItem(ItemIndex.BeetleGland);
            currentInventory.ResetItem(ItemIndex.CrippleWardOnLevel);
            currentInventory.ResetItem(ItemIndex.ExtraLife);
            currentInventory.ResetItem(ItemIndex.ExtraLifeConsumed);
            currentInventory.GiveItem(ItemIndex.ExtraLife, itemCount);
            currentInventory.GiveItem(ItemIndex.ExtraLifeConsumed, itemCount2);
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
                if (OwnerCharacterMaster.GetBody().HasBuff(buffType))
                {
                    if (buffType == BuffIndex.AffixBlue || buffType == BuffIndex.AffixWhite || buffType == BuffIndex.AffixRed || buffType == BuffIndex.AffixPoison)
                    {
                        if (gameObject.GetComponent<CharacterMaster>().GetBody().HasBuff(buffType)) continue;

                        var timedBuffs = (IList) TimedBuffsFieldInfo.GetValue(OwnerCharacterMaster.GetBody());
                        foreach (var timedBuff in timedBuffs)
                        {
                            if ((BuffIndex) TimedBuffbuffIndexFieldInfo.GetValue(timedBuff) == buffType)
                            {
                                var buffDuration = (float)TimedBufftimerFieldInfo.GetValue(timedBuff);
                                if (buffDuration <= 1f)
                                    break;

                                gameObject.GetComponent<CharacterMaster>().GetBody().AddTimedBuff(buffType, buffDuration);
                                break;
                            }
                        }
                    }
                    else if (buffType != BuffIndex.NoCooldowns)
                    {
                        gameObject.GetComponent<CharacterMaster>().GetBody().AddBuff(buffType);
                    }   
                }
                else if (gameObject.GetComponent<CharacterMaster>().GetBody().HasBuff(buffType))
                {
                    if (buffType == BuffIndex.AffixBlue || buffType == BuffIndex.AffixWhite || buffType == BuffIndex.AffixRed || buffType == BuffIndex.AffixPoison)
                    {
                        if (OwnerCharacterMaster.GetBody().HasBuff(buffType)) continue;

                        var timedBuffs = (IList) TimedBuffsFieldInfo.GetValue(gameObject.GetComponent<CharacterMaster>().GetBody());
                        foreach (var timedBuff in timedBuffs)
                        {
                            if ((BuffIndex) TimedBuffbuffIndexFieldInfo.GetValue(timedBuff) == buffType)
                            {
                                var buffDuration = (float) TimedBufftimerFieldInfo.GetValue(timedBuff);
                                if (buffDuration <= 1f)
                                    break;

                                OwnerCharacterMaster.GetBody().AddTimedBuff(buffType, buffDuration);
                                break;
                            }
                        }
                    }
                    else if (buffType == BuffIndex.NoCooldowns)
                    {
                        OwnerCharacterMaster.GetBody().AddBuff(buffType);
                    }
                    else
                    {
                        gameObject.GetComponent<CharacterMaster>().GetBody().RemoveBuff(buffType);
                    }
                }
            }
        }

        void OnDisable()
        {
            OwnerCharacterMaster.inventory.onInventoryChanged -= UpdateInventory;
            On.RoR2.CharacterBody.UpdateBuffs -= CharacterBodyOnUpdateBuffs;
            AkSoundEngine.StopPlayingID(SoundRailGunTargetingId);

            DestroyImmediate(this);
        }
    }
}
