using BepInEx;
using HarmonyLib;
using System.Reflection;

namespace CombineSpearAndPolearmSkills
{
    [BepInPlugin(GUID, NAME, VERSION)]
    public class CombineSpearAndPolearmSkillsPlugin : BaseUnityPlugin
    {
        public const string GUID = "goldenrevolver.CombineSpearAndPolearmSkills";
        public const string NAME = "Combined Weapon Skills";
        public const string VERSION = "1.0.0";

        protected void Awake()
        {
            SpriteLoader.LoadSprites();

            CombineConfig.LoadConfig(this);

            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
        }
    }

    [HarmonyPatch(typeof(FejdStartup))]
    internal class FejdStartupPatch
    {
        [HarmonyPatch(nameof(FejdStartup.Awake)), HarmonyPostfix]
        private static void FejdStartupAwakePatch()
        {
            LocalizationLoader.SetupTranslations();
        }
    }
}