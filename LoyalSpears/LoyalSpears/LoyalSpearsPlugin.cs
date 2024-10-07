using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using ServerSync;
using System.Reflection;

namespace LoyalSpears
{
    [BepInPlugin(GUID, NAME, VERSION)]
    public class LoyalSpearsPlugin : BaseUnityPlugin
    {
        public const string GUID = "goldenrevolver.LoyalSpears";
        public const string NAME = "Loyal Spears - Auto Pickup And Return To Owner";
        public const string VERSION = "1.1.1";

        internal static ConfigSync serverSyncInstance;
        internal static ConfigEntry<bool> UseServerSync;

        internal static ConfigEntry<float> GroundSecondsUntilAutoReturn;
        internal static ConfigEntry<float> FlightDistanceUntilAutoReturn;

        internal static ConfigEntry<float> MaxSecondsToReserveCarryingCapacityForThrownSpears;
        internal static ConfigEntry<bool> SpearReturnOverencumberProtection;

        protected void Awake()
        {
            LoadConfig();

            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
        }

        internal void LoadConfig()
        {
            // disable saving while we add config values, so it doesn't save to file on every change, then enable it again
            Config.SaveOnConfigSet = false;

            LoadConfigInternal();

            Config.Save();
            Config.SaveOnConfigSet = true;
        }

        private void LoadConfigInternal()
        {
            serverSyncInstance = ServerSyncWrapper.CreateRequiredConfigSync(GUID, NAME, VERSION);

            string sectionName = "General";

            UseServerSync = Config.BindHiddenForceEnabledSyncLocker(serverSyncInstance, sectionName, nameof(UseServerSync));
            GroundSecondsUntilAutoReturn = Config.BindSynced(serverSyncInstance, sectionName, nameof(GroundSecondsUntilAutoReturn), 20f, "After the spear projectile hit the ground and this many seconds passed without anyone picking it up, the spear will return to you. Negative number to disable");
            FlightDistanceUntilAutoReturn = Config.BindSynced(serverSyncInstance, sectionName, nameof(FlightDistanceUntilAutoReturn), -1f, "If the spear projectile travels further than this away from the player without hitting the ground, the spear automatically returns (for example when throwing it off the end of the world). Negative number to disable");

            sectionName = "Overencumber";

            MaxSecondsToReserveCarryingCapacityForThrownSpears = Config.BindSynced(serverSyncInstance, sectionName, nameof(MaxSecondsToReserveCarryingCapacityForThrownSpears), 30f, "Maximum seconds carrying capacity is reserved for a thrown spear you haven't picked up again. Prevents auto pickup of other items if it would overencumber you when you'd pick up your spear. Reservation ends early when you pick up the spear by any means. Negative number to disable");
            SpearReturnOverencumberProtection = Config.BindSynced(serverSyncInstance, sectionName, nameof(SpearReturnOverencumberProtection), false, $"Prevent spear auto return from '{nameof(GroundSecondsUntilAutoReturn)}' feature when it would lead to you becoming overencumbered");
        }
    }
}