using BepInEx;
using HarmonyLib;
using System.Reflection;

namespace AtgeirPolearmAnimationFix
{
    [BepInPlugin(GUID, NAME, VERSION)]
    public class AtgeirPolearmAnimationFixPlugin : BaseUnityPlugin
    {
        public const string GUID = "goldenrevolver.AtgeirPolearmAnimationFix";
        public const string NAME = "Atgeir/Polearm Animation Fix";
        public const string VERSION = "1.0.0";

        internal static AtgeirPolearmAnimationFixPlugin plugin;

        protected void Awake()
        {
            plugin = this;

            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
        }
    }
}