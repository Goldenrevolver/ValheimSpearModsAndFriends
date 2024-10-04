using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using HarmonyLib;
using ServerSync;
using System.Reflection;

namespace ProperSpears
{
    [BepInPlugin(GUID, NAME, VERSION)]
    public class ProperSpearsPlugin : BaseUnityPlugin
    {
        public const string GUID = "goldenrevolver.ProperSpears";
        public const string NAME = "Proper Spears - Forward Facing With Thrust Attacks";
        public const string VERSION = "1.0.2";

        internal static ConfigSync serverSyncInstance;
        internal static ConfigEntry<bool> UseServerSync;

        protected void Awake()
        {
            LoadConfig();

            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
        }

        internal static bool IsHarpoonAttackModInstalled()
        {
            return Chainloader.PluginInfos.ContainsKey("goldenrevolver.HarpoonMeleeAttack");
        }

        private void LoadConfig()
        {
            serverSyncInstance = ServerSyncWrapper.CreateRequiredConfigSync(GUID, NAME, VERSION);

            string sectionName = "General";

            UseServerSync = Config.BindHiddenForceEnabledSyncLocker(serverSyncInstance, sectionName, nameof(UseServerSync));
        }
    }
}