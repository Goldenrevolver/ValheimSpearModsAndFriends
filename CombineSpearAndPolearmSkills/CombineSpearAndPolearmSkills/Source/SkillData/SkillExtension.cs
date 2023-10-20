using UnityEngine;

namespace CombineSpearAndPolearmSkills
{
    internal static class SkillExtension
    {
        public static Skills.Skill ShallowCopy(this Skills.Skill toCopy)
        {
            Skills.SkillDef def = new Skills.SkillDef
            {
                m_description = toCopy.m_info.m_description,
                m_skill = toCopy.m_info.m_skill,
                m_increseStep = toCopy.m_info.m_increseStep,
                m_icon = toCopy.m_info.m_icon
            };

            var skill = new Skills.Skill(def)
            {
                m_level = toCopy.m_level,
                m_accumulator = toCopy.m_accumulator
            };

            return skill;
        }

        internal static int CustomCompare(this Skills.Skill thisSkill, Skills.Skill otherSkill)
        {
            if (CombineConfig.SkillsMenuSorting.Value == SortSkillsMenu.ByLevel)
            {
                // sort highest to lowest
                int comp = -thisSkill.CompareSkillLevel(otherSkill);

                if (comp != 0)
                {
                    return comp;
                }
            }

            return thisSkill.CompareSkillTranslation(otherSkill);
        }

        internal static int CompareSkillTranslation(this Skills.Skill thisSkill, Skills.Skill otherSkill)
        {
            return thisSkill.GetSkillTranslation().CompareTo(otherSkill.GetSkillTranslation());
        }

        internal static int CompareSkillLevel(this Skills.Skill thisSkill, Skills.Skill otherSkill)
        {
            int compare = Mathf.FloorToInt(thisSkill.m_level).CompareTo(Mathf.FloorToInt(otherSkill.m_level));

            if (compare != 0)
            {
                return compare;
            }

            return thisSkill.m_accumulator.CompareTo(otherSkill.m_accumulator);
        }

        internal static string GetSkillTranslation(this Skills.Skill skill)
        {
            return Localization.instance.Localize("$skill_" + skill.m_info.m_skill.ToString().ToLower());
        }
    }
}