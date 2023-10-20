using System;

namespace CombineSpearAndPolearmSkills
{
    internal struct SkillSetupInfo
    {
        internal static readonly Skills.SkillType[] supportedTypes = new Skills.SkillType[] { Skills.SkillType.Knives, Skills.SkillType.Polearms, Skills.SkillType.Crossbows };

        private const string keyPrefix = CombineSpearAndPolearmSkillsPlugin.GUID;

        private const string lastSkillTypeSuffix = ".lastCombinedSkill";
        private const string lastMethodSuffix = ".lastCombineMethod";

        private const string knivesInfix = ".HasCurrentlySynced.Knives";
        private const string polearmsInfix = ".HasCurrentlySynced.Polearms";
        private const string crossbowsInfix = ".HasCurrentlySynced.Crossbows";

        private const string lastSkillTypeKeyKnives = keyPrefix + knivesInfix + lastSkillTypeSuffix;
        private const string lastSkillTypeKeyPolearms = keyPrefix + polearmsInfix + lastSkillTypeSuffix;
        private const string lastSkillTypeKeyCrossbows = keyPrefix + crossbowsInfix + lastSkillTypeSuffix;

        private const string lastMethodKeyKnives = keyPrefix + knivesInfix + lastMethodSuffix;
        private const string lastMethodKeyPolearms = keyPrefix + polearmsInfix + lastMethodSuffix;
        private const string lastMethodKeyCrossbows = keyPrefix + crossbowsInfix + lastMethodSuffix;

        internal string lastSkillTypeKey;
        internal string lastCombineMethodKey;
        //internal ConfigEntry<bool> forceConfig;

        internal SkillSetupInfo(string lastSkillTypeKey, string lastCombineMethodKey)//, ConfigEntry<bool> forceConfig)
        {
            this.lastSkillTypeKey = lastSkillTypeKey;
            this.lastCombineMethodKey = lastCombineMethodKey;
            //this.forceConfig = forceConfig;
        }

        internal static SkillSetupInfo GetSupportedTypeData(Skills.SkillType skillType)
        {
            switch (skillType)
            {
                case Skills.SkillType.Knives:
                    return new SkillSetupInfo(lastSkillTypeKeyKnives, lastMethodKeyKnives);//, CombineConfig.ForceResetSkillKnifeCombination);

                case Skills.SkillType.Polearms:
                    return new SkillSetupInfo(lastSkillTypeKeyPolearms, lastMethodKeyPolearms);//, CombineConfig.ForceResetSkillPolearmCombination);

                case Skills.SkillType.Crossbows:
                    return new SkillSetupInfo(lastSkillTypeKeyCrossbows, lastMethodKeyCrossbows);//, CombineConfig.ForceResetSkillCrossbowCombination);

                default:
                    throw new Exception("Unsupported type in helper method");
            }
        }
    }
}