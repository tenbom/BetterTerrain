using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Harmony;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace Better_Terrain.Harmony
{
    [HarmonyPatch(typeof(World))]
    //Is compiler generated so needs updating each game version.
    [HarmonyPatch("<NaturalRockTypesIn>m__30E")]
    [HarmonyPatch(new Type[] { typeof(ThingDef)})]
    internal class NaturalRockTypesIn
    {
        static bool Prefix(bool __result, ThingDef d)
        {
            if (d.defName.EndsWith("BTHard"))
            {
                __result = false;
                return false;
            }
            return true;
        }
    }

//    [HarmonyPatch(typeof(World))]
//    //Is compiler generated so needs updating each game version.
//    [HarmonyPatch("NaturalRockTypesIn")]
//    internal class NaturalRockTypesIn
//    {
//        static void Postfix(ref IEnumerable<ThingDef> __result, int tile)
//        {
//            var tmp = __result as IList<ThingDef> ?? __result.ToList();
//            int before = tmp.Count();
//            tmp = tmp.Select(a => a.defName.EndsWith("BTHard") ? GetRock(a) : a).Distinct().ToArray();
//            int after = tmp.Count();
//            if (before != after)
//            {
//                Rand.PushSeed();
//                Rand.Seed = tile;
//                int num = after - before;
//                var list =
//                    DefDatabase<ThingDef>.AllDefs.Where(
//                        a => a.category == ThingCategory.Building && a.building.isNaturalRock
//                             && !a.building.isResourceRock && !a.defName.EndsWith("BTHard") && !tmp.Contains(a)).ToList();
//
//                if (num > list.Count)
//                {
//                    num = list.Count;
//                }
//                for (int i = 0; i < num; i++)
//                {
//                    ThingDef item = list.RandomElement<ThingDef>();
//                    list.Remove(item);
//                    tmp.Add(item);
//                }
//                Rand.PopSeed();
//            }
//            
//            __result = tmp;
//        }
//
//        private static ThingDef GetRock(ThingDef def)
//        {
//            var rock = DefDatabase<ThingDef>.AllDefs.Where(a => a.category == ThingCategory.Building && a.building.isNaturalRock && !a.defName.EndsWith("BTHard")
//                                                    && !a.building.isResourceRock).SingleOrDefault(a => a.defName == def.defName.TrimEnd("BTHard".ToCharArray()));
//            return rock ?? ThingDefOf.Sandstone;
//        }
//    }
}
