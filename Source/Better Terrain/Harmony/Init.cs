using System.Reflection;
using Harmony;
using Verse;

namespace Better_Terrain.Harmony
{
    [StaticConstructorOnStartup]
    class Init
    {
        static Init()
        {
            var harmony = HarmonyInstance.Create("com.github.betterTerrain.rimworld.mod");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}