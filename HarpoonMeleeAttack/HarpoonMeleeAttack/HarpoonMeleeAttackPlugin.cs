using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using ServerSync;
using System.Reflection;

namespace HarpoonMeleeAttack
{
    [BepInPlugin(GUID, NAME, VERSION)]
    public class HarpoonMeleeAttackPlugin : BaseUnityPlugin
    {
        public const string GUID = "goldenrevolver.HarpoonMeleeAttack";
        public const string NAME = "Harpoon Melee Attack And Upgrading";
        public const string VERSION = "1.0.0";

        internal static ConfigSync serverSyncInstance;
        internal static ConfigEntry<bool> UseServerSync;

        protected void Awake()
        {
            LoadConfig();

            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
        }

        private void LoadConfig()
        {
            serverSyncInstance = ServerSyncWrapper.CreateRequiredConfigSync(GUID, NAME, VERSION);

            string sectionName = "General";

            UseServerSync = Config.BindHiddenForceEnabledSyncLocker(serverSyncInstance, sectionName, nameof(UseServerSync));
        }
    }
}