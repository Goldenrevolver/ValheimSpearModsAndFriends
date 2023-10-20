namespace CombineSpearAndPolearmSkills
{
    internal struct CombinedSkill
    {
        internal Skills.Skill thisSkill;
        internal Skills.Skill otherSkill;

        private CombinedSkill(Skills.Skill thisSkill, Skills.Skill otherSkill)
        {
            this.thisSkill = thisSkill;
            this.otherSkill = otherSkill;
        }

        internal static CombinedSkill? CreateCombinedSkill(Skills skills, Skills.SkillType thisType, Skills.SkillType otherType)
        {
            if (thisType == Skills.SkillType.None || otherType == Skills.SkillType.None)
            {
                return null;
            }

            // this creates the skills if they don't yet exist
            Skills.Skill thisSkill = skills.GetSkill(thisType);
            Skills.Skill otherSkill = skills.GetSkill(otherType);

            if (thisSkill == null || otherSkill == null)
            {
                return null;
            }

            return new CombinedSkill(thisSkill, otherSkill);
        }

        internal static CombinedSkill? GetCombinedSkill(Skills skills, Skills.SkillType skillType)
        {
            var flippedSkill = SkillFlipper.FlipToCombinedSkill(skillType);

            return CreateCombinedSkill(skills, skillType, flippedSkill);
        }
    }
}