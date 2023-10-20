using BepInEx.Bootstrap;
using UnityEngine;

namespace CombineSpearAndPolearmSkills
{
    public static class Helper
    {
        private const string augaMod = "randyknapp.mods.auga";

        internal static bool HasAugaInstalled()
        {
            return Chainloader.PluginInfos.ContainsKey(augaMod);
        }

        internal static void Log(object s)
        {
            if (!CombineConfig.ShowDebugLogs.Value)
            {
                return;
            }

            Debug.Log(ToPrint(s));
        }

        internal static void LogWarning(object s)
        {
            if (!CombineConfig.ShowDebugLogs.Value)
            {
                return;
            }

            Debug.LogWarning(ToPrint(s));
        }

        internal static void LogWarningOverride(object s)
        {
            Debug.LogWarning(ToPrint(s));
        }

        private static string ToPrint(object s)
        {
            return $"{CombineSpearAndPolearmSkillsPlugin.NAME} {CombineSpearAndPolearmSkillsPlugin.VERSION}: {(s != null ? s.ToString() : "null")}";
        }
    }
}