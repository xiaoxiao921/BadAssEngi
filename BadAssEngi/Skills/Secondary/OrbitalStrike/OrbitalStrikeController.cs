using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace BadAssEngi.Skills.Secondary.OrbitalStrike
{
    public class OrbitalStrikeController : MonoBehaviour
    {
        public float TimeLeft = 36;
        public float StartTime;

        public const int TotalTime = 37;

        public const float DamageRate = 0.5f;
        public float NextDamageTime;

        private Vector3 _succOffset;
        private Vector3 _upOffset;

        public const float Radius = 50f;
        public const float Damping = 0.2f;
        public const float ForceCoefficientAtEdge = 0.5f;
        public const float ForceMagnitude = -1500f / 5f;

        public GameObject Parent;
        public GameObject Owner;
        public CharacterBody OwnerCharacterBody;
        public TeamIndex OwnerTeam;
        public Transform ChildTransform;

        public void Awake()
        {
            StartTime = Time.time;

            _succOffset = new Vector3(0, 15f, 0);
            _upOffset = new Vector3(0, 15f, 0);

            ChildTransform = transform.GetChild(0).transform;
        }

        public void InitCB()
        {
            OwnerCharacterBody = Owner.GetComponent<CharacterBody>();
        }

        public void Update()
        {
            if (Physics.Raycast(ChildTransform.position + _upOffset, Vector3.down, out var hit, Mathf.Infinity, 1 << 11))
            {
                //LocalUserManager.readOnlyLocalUsersList[0].currentNetworkUser.master.GetBodyObject().transform
                //.position = hit2.point;
                transform.position = new Vector3(transform.position.x, hit.point.y, transform.position.z);
            }

            if (!NetworkServer.active)
                return;

            TimeLeft -= Time.deltaTime;

            if (TimeLeft < 20 && Time.time > NextDamageTime && TimeLeft > 2f)
            {
                NextDamageTime = Time.time + DamageRate;
                DoDamage();
            }

            if (Time.time > StartTime + TotalTime + 10)
                Destroy(gameObject);
        }


        public void FixedUpdate()
        {
            if (TimeLeft < 29f && TimeLeft > 19f)
            {
                DoSucc();
            }
        }

        private void DoSucc()
        {
            var monsters = TeamComponent.GetTeamMembers(TeamIndex.Monster);
            foreach (var monster in monsters)
            {
                var healthComponent = monster.GetComponent<HealthComponent>();
                var characterMotor = monster.GetComponent<CharacterMotor>();

                if (healthComponent)
                {
                    var offsetSucc = transform.position + _succOffset;
                    var distance = monster.transform.position - offsetSucc;
                    var clamp = 1f - Mathf.Clamp(distance.magnitude / Radius, 0f, 1f - ForceCoefficientAtEdge);
                    distance = distance.normalized * ForceMagnitude * (1f - clamp);

                    Vector3 velocity;
                    float mass;

                    if (characterMotor)
                    {
                        velocity = characterMotor.velocity;
                        mass = characterMotor.mass;
                    }
                    else
                    {
                        var rigidBody = healthComponent.GetComponent<Rigidbody>();
                        velocity = rigidBody.velocity;
                        mass = rigidBody.mass;
                    }

                    velocity.y += Physics.gravity.y * Time.fixedDeltaTime;
                    healthComponent.TakeDamageForce(distance - velocity * (Damping * mass * clamp), true);
                }
            }
        }

        private void DoDamage()
        {
            var monsters = TeamComponent.GetTeamMembers(TeamIndex.Monster);
            foreach (var monster in monsters)
            {
                if (monster.transform.position.y - transform.position.y <= 0)
                    continue;

                var distance = FlatDistance(monster.transform.position, transform.position);
                if (distance < Radius)
                {
                    var healthComponent = monster.transform.GetComponent<HealthComponent>();

                    if (healthComponent && healthComponent.GetComponent<TeamComponent>().teamIndex != OwnerTeam)
                    {
                        var damageInfo = new DamageInfo
                        {
                            damage = Configuration.OrbitalStrikeBaseDamage.Value + OwnerCharacterBody.levelDamage * (OwnerCharacterBody.level - 1f),
                            position = transform.position,
                            force = Vector3.zero,
                            damageColorIndex = DamageColorIndex.Bleed,
                            crit = false,
                            attacker = Owner,
                            inflictor = gameObject,
                            damageType = DamageTypeCombo.GenericSecondary,
                            procCoefficient = 0f,
                            procChainMask = default
                        };

                        healthComponent.TakeDamage(damageInfo);
                    }
                }
            }
        }

        private static float FlatDistance(Vector3 pos1, Vector3 pos2)
        {
            pos1.y = pos1.z;
            pos2.y = pos2.z;
            return Vector2.Distance(pos1, pos2);
        }
    }
}
