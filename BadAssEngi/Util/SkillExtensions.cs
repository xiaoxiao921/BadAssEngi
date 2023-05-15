using R2API;
using RoR2;
using RoR2.Skills;
using UnityEngine;

// ReSharper disable MemberCanBePrivate.Global

namespace BadAssEngi.Util
{
    internal static class SkillExtensions
    {
        internal static SkillFamily AddSkillFamily(string skillFamilyName, params SkillDef[] skills)
        {
            var family = ScriptableObject.CreateInstance<SkillFamily>();
            ((ScriptableObject)family).name = skillFamilyName;
            family.variants = new SkillFamily.Variant[skills.Length];

            for (var i = 0; i < skills.Length; ++i)
            {
                family.variants[i] = new SkillFamily.Variant
                {
                    skillDef = skills[i],
                    viewableNode = new ViewablesCatalog.Node(skills[i].skillName, false)
                };

                ContentAddition.AddSkillDef(skills[i]);

                LoadoutAPI.AddSkill(skills[i].activationState.stateType);
            }

            ContentAddition.AddSkillFamily(family);

            return family;
        }

        internal static void SetSkillFamily(this GenericSkill genericSkill, string skillFamilyName, params SkillDef[] skills)
        {
            var family = AddSkillFamily(skillFamilyName, skills);

            genericSkill._skillFamily = family;
        }
    }
}
