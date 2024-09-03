using EntityStates;
using EntityStates.Engi.EngiWeapon;
using RoR2;
using UnityEngine;

namespace BadAssEngi.Skills.Primary.States
{
    public class ChargeSeekerMissile : BaseState
    {
        private static float _baseTotalDuration;

        private static float _baseMaxChargeTime;
        private static int _maxCharges;

        private static GameObject _chargeEffectPrefab;

        private static string _chargeStockSoundString;
        private static string _chargeLoopStartSoundString;
        private static string _chargeLoopStopSoundString;

        private const int MinGrenadeCount = 2;
        private static int MaxGrenadeCount => Configuration.SeekerMissileMaxProjectileNumber.Value;

        private static float _minBonusBloom;
        private static float _maxBonusBloom;

        private GameObject _chargeLeftInstance;
        private GameObject _chargeRightInstance;

        private int _charge;
        private int _lastCharge;

        private float _totalDuration;

        private float _maxChargeTime;


        public override void OnEnter()
        {
            CheckInitState();

            base.OnEnter();
            _totalDuration = _baseTotalDuration / attackSpeedStat;
            _maxChargeTime = _baseMaxChargeTime / attackSpeedStat;
            var modelTransform = GetModelTransform();
            PlayAnimation("Gesture, Additive", "ChargeGrenades");
            RoR2.Util.PlaySound(_chargeLoopStartSoundString, gameObject);
            if (modelTransform)
            {
                var component = modelTransform.GetComponent<ChildLocator>();
                if (component)
                {
                    var childTransform = component.FindChild("MuzzleLeft");
                    if (childTransform && _chargeEffectPrefab)
                    {
                        _chargeLeftInstance = Object.Instantiate(_chargeEffectPrefab, childTransform.position, childTransform.rotation);
                        _chargeLeftInstance.transform.parent = childTransform;
                        var component2 = _chargeLeftInstance.GetComponent<ScaleParticleSystemDuration>();
                        if (component2)
                        {
                            component2.newDuration = _totalDuration;
                        }
                    }
                    var transform2 = component.FindChild("MuzzleRight");
                    if (transform2 && _chargeEffectPrefab)
                    {
                        _chargeRightInstance = Object.Instantiate(_chargeEffectPrefab, transform2.position, transform2.rotation);
                        _chargeRightInstance.transform.parent = transform2;
                        var component3 = _chargeRightInstance.GetComponent<ScaleParticleSystemDuration>();
                        if (component3)
                        {
                            component3.newDuration = _totalDuration;
                        }
                    }
                }
            }
        }

        private static void CheckInitState()
        {
            if (_chargeEffectPrefab)
            {
                return;
            }

            var _ = new ChargeGrenades();

            _baseTotalDuration = ChargeGrenades.baseTotalDuration;
            _baseMaxChargeTime = ChargeGrenades.baseMaxChargeTime;
            _maxCharges = ChargeGrenades.maxCharges;
            _chargeEffectPrefab = ChargeGrenades.chargeEffectPrefab;
            _chargeStockSoundString = ChargeGrenades.chargeStockSoundString;
            _chargeLoopStartSoundString = ChargeGrenades.chargeLoopStartSoundString;
            _chargeLoopStopSoundString = ChargeGrenades.chargeLoopStopSoundString;
            _minBonusBloom = ChargeGrenades.minBonusBloom;
            _maxBonusBloom = ChargeGrenades.maxBonusBloom;
        }

        public override void OnExit()
        {
            base.OnExit();
            PlayAnimation("Gesture, Additive", "Empty");
            RoR2.Util.PlaySound(_chargeLoopStopSoundString, gameObject);
            Destroy(_chargeLeftInstance);
            Destroy(_chargeRightInstance);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            _lastCharge = _charge;
            _charge = Mathf.Min((int)(fixedAge / _maxChargeTime * _maxCharges), _maxCharges);
            var t = _charge / (float)_maxCharges;
            var value = Mathf.Lerp(_minBonusBloom, _maxBonusBloom, t);
            characterBody.SetSpreadBloom(value);
            var num = Mathf.FloorToInt(Mathf.Lerp(MinGrenadeCount, MaxGrenadeCount, t));
            if (_lastCharge < _charge)
            {
                RoR2.Util.PlaySound(_chargeStockSoundString, gameObject, "engiM1_chargePercent", 100f * ((num - 1) / (float)MaxGrenadeCount));
            }
            if ((fixedAge >= _totalDuration || !inputBank || !inputBank.skill1.down) && isAuthority)
            {
                var fireGrenades = new FireSeekerMissile { GrenadeCountMax = num };
                outer.SetNextState(fireGrenades);
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}
