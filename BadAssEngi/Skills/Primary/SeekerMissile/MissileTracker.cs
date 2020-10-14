using System.Linq;
using BadAssEngi.Assets;
using RoR2;
using UnityEngine;

namespace BadAssEngi.Skills.Primary.SeekerMissile
{
    public class MissileTracker : MonoBehaviour
    {
        private void Awake()
        {
            //var visualizerPrefab = Resources.Load<GameObject>("Prefabs/HuntressTrackingIndicator").InstantiateClone("MissileTrackerIndicator", false);
            var visualizerPrefab = BaeAssets.PrefabEngiRocketCrosshair;
            var spriteRenderers = visualizerPrefab.GetComponentsInChildren<Renderer>(true);
            
            if (Configuration.CustomTrackerIndicatorColor.Value)
            {
                var rgb = Configuration.TrackerIndicatorColor.Value.Split(',');
                var color = new Color(float.Parse(rgb[0]), float.Parse(rgb[1]), float.Parse(rgb[2]));
                foreach (var spriteRenderer in spriteRenderers)
                {
                    var materials = spriteRenderer.materials;
                    foreach (var material in materials)
                    {
                        if (material.name.Contains("circle"))
                        {
                            material.color = color;
                            material.SetColor("_EmissionColor", color);
                        }
                    }
                }
            }

            indicator = new Indicator(gameObject, visualizerPrefab);
        }

        private void Start()
        {
            inputBank = GetComponent<InputBankTest>();
            teamComponent = GetComponent<TeamComponent>();
        }

        public HurtBox GetTrackingTarget()
        {
            return trackingTarget;
        }

        private void OnEnable()
        {
            indicator.active = true;
        }

        private void OnDisable()
        {
            indicator.active = false;
        }

        private void FixedUpdate()
        {
            trackerUpdateStopwatch += Time.fixedDeltaTime;
            if (trackerUpdateStopwatch >= 1f / TrackerUpdateFrequency)
            {
                trackerUpdateStopwatch -= 1f / TrackerUpdateFrequency;
                Ray aimRay = new Ray(inputBank.aimOrigin, inputBank.aimDirection);
                SearchForTarget(aimRay);
                indicator.targetTransform = (trackingTarget ? trackingTarget.transform : null);

                if (triggerNewTarget)
                {
                    if (animator != null && animator)
                    {
                        animator = indicator.visualizerInstance.GetComponent<Animator>();
                        animator.Play("NewTarget", -1, 0);
                        triggerNewTarget = false;
                    }
                    else
                    {
                        animator = indicator.visualizerInstance.GetComponent<Animator>();
                    }
                }
            }
        }

        private void SearchForTarget(Ray aimRay)
        {
            search.teamMaskFilter = TeamMask.GetUnprotectedTeams(teamComponent.teamIndex);
            search.filterByLoS = true;
            search.searchOrigin = aimRay.origin;
            search.searchDirection = aimRay.direction;
            search.sortMode = BullseyeSearch.SortMode.Distance;
            search.maxDistanceFilter = MaxTrackingDistance;
            search.maxAngleFilter = MaxTrackingAngle;
            search.RefreshCandidates();
            trackingTarget = search.GetResults().FirstOrDefault();

            if (trackingTarget != null && trackingTarget)
            {
                if (oldtrackingTarget != null)
                {
                    if (oldtrackingTarget)
                    {
                        if (oldtrackingTarget != trackingTarget)
                        {
                            triggerNewTarget = true;
                            oldtrackingTarget = trackingTarget;
                        }
                        else
                        {
                            triggerNewTarget = false;
                        }
                    }
                    else
                    {
                        triggerNewTarget = true;
                        oldtrackingTarget = trackingTarget;
                    }
                }
                else
                {
                    triggerNewTarget = true;
                    oldtrackingTarget = trackingTarget;
                }
            }
        }

        private const float MaxTrackingDistance = 999f;
        private const float MaxTrackingAngle = 20f;
        private const float TrackerUpdateFrequency = 10f;
        public HurtBox trackingTarget;
        public HurtBox oldtrackingTarget;
        public bool triggerNewTarget;
        private TeamComponent teamComponent;
        private InputBankTest inputBank;
        private float trackerUpdateStopwatch;
        private Indicator indicator;
        private readonly BullseyeSearch search = new BullseyeSearch();

        private Animator animator;
    }
}
