using R2API;
using RoR2;
using RoR2.Skills;
using UnityEngine;

// ReSharper disable MemberCanBePrivate.Global

namespace BadAssEngi.Util
{
    internal static class SkillExtensions
    {
        internal static SkillFamily AddSkillFamily(params SkillDef[] skills)
        {
            var family = ScriptableObject.CreateInstance<SkillFamily>();
            family.variants = new SkillFamily.Variant[skills.Length];

            LoadoutAPI.AddSkillFamily(family);

            for (var i = 0; i < skills.Length; ++i)
            {
                family.variants[i] = new SkillFamily.Variant
                {
                    skillDef = skills[i],
                    unlockableName = "",
                    viewableNode = new ViewablesCatalog.Node(skills[i].skillName, false)
                };

                LoadoutAPI.AddSkillDef(skills[i]);
                LoadoutAPI.AddSkill(skills[i].activationState.stateType);
            }

            return family;
        }

        internal static void SetSkillFamily(this GenericSkill genericSkill, params SkillDef[] skills)
        {
            var family = AddSkillFamily(skills);

            genericSkill._skillFamily = family;
        }
    }
}
