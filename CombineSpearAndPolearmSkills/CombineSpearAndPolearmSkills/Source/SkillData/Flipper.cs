namespace CombineSpearAndPolearmSkills
{
    internal class SkillFlipper
    {
        internal static Skills.SkillType FlipToCombinedSkill(Skills.SkillType skill)
        {
            switch (skill)
            {
                case Skills.SkillType.Spears:
                    return CombineConfig.SyncPolearmsWith.Value == PolearmSync.SyncWithSpears ? Skills.SkillType.Polearms : Skills.SkillType.None;

                case Skills.SkillType.Polearms:
                    return CombineConfig.SyncPolearmsWith.Value == PolearmSync.SyncWithSpears ? Skills.SkillType.Spears : Skills.SkillType.None;

                case Skills.SkillType.Knives:
                    return FlipKnifeSkill();

                case Skills.SkillType.Unarmed:
                    return CombineConfig.SyncKnivesWith.Value == KnifeSync.SyncWithFists ? Skills.SkillType.Knives : Skills.SkillType.None;

                case Skills.SkillType.Swords:
                    return CombineConfig.SyncKnivesWith.Value == KnifeSync.SyncWithSwords ? Skills.SkillType.Knives : Skills.SkillType.None;

                case Skills.SkillType.Crossbows:
                    return CombineConfig.SyncCrossbowsWith.Value == CrossbowSync.SyncWithBows ? Skills.SkillType.Bows : Skills.SkillType.None;

                case Skills.SkillType.Bows:
                    return CombineConfig.SyncCrossbowsWith.Value == CrossbowSync.SyncWithBows ? Skills.SkillType.Crossbows : Skills.SkillType.None;

                default:
                    return Skills.SkillType.None;
            }
        }

        private static Skills.SkillType FlipKnifeSkill()
        {
            switch (CombineConfig.SyncKnivesWith.Value)
            {
                case KnifeSync.SyncWithFists:
                    return Skills.SkillType.Unarmed;

                case KnifeSync.SyncWithSwords:
                    return Skills.SkillType.Swords;

                default:
                    return Skills.SkillType.None;
            }
        }
    }
}