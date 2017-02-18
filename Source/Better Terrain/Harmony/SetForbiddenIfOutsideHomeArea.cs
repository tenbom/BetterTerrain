using Harmony;
using RimWorld;
using Verse;

namespace Better_Terrain.Harmony
{
    [HarmonyPatch(typeof(ForbidUtility))]
    [HarmonyPatch("SetForbiddenIfOutsideHomeArea")]
    internal class SetForbiddenIfOutsideHomeArea
    {
        private static bool Prefix(ref Thing t)
        {
            if (t.Map.areaManager.Home == null)
            {
                t.SetForbidden(true, false);
                return false;
            }
            return true;
        }
    }
}