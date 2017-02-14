using System;
using Verse;
using Verse.Noise;
using RimWorld;

namespace Better_Terrain
{
	internal static class BT_BeachMaker
	{
		private const float PerlinFrequency = 0.03f;

		private const float MaxForDeepWater = 0.1f;

		private const float MaxForShallowWater = 0.45f;

		private const float MaxForSand = 1f;

		private static ModuleBase beachNoise;

		private static readonly FloatRange CoastWidthRange = new FloatRange(20f, 60f);

		public static void Init(Map map)
		{
			Rot4 a = Find.World.CoastDirectionAt(map.Tile);
			if (!a.IsValid)
			{
				BT_BeachMaker.beachNoise = null;
				return;
			}
			ModuleBase moduleBase = new Perlin(0.029999999329447746, 2.0, 0.5, 3, Rand.Range(0, 2147483647), QualityMode.Medium);
			moduleBase = new ScaleBias(0.5, 0.5, moduleBase);
			NoiseDebugUI.StoreNoiseRender(moduleBase, "BeachMaker base", new IntVec2(map.Size.x, map.Size.z));
			ModuleBase moduleBase2 = new DistFromAxis(BT_BeachMaker.CoastWidthRange.RandomInRange);
			if (a == Rot4.North)
			{
				moduleBase2 = new Rotate(0.0, 90.0, 0.0, moduleBase2);
				moduleBase2 = new Translate(0.0, 0.0, (double)(-(double)map.Size.z), moduleBase2);
			}
			else if (a == Rot4.East)
			{
				moduleBase2 = new Translate((double)(-(double)map.Size.x), 0.0, 0.0, moduleBase2);
			}
			else if (a == Rot4.South)
			{
				moduleBase2 = new Rotate(0.0, 90.0, 0.0, moduleBase2);
			}
			moduleBase2 = new ScaleBias(1.0, -1.0, moduleBase2);
			moduleBase2 = new Clamp(-1.0, 2.5, moduleBase2);
			NoiseDebugUI.StoreNoiseRender(moduleBase2, "BeachMaker axis bias");
			BT_BeachMaker.beachNoise = new Add(moduleBase, moduleBase2);
			NoiseDebugUI.StoreNoiseRender(BT_BeachMaker.beachNoise, "beachNoise");
		}

		public static void Cleanup()
		{
			BT_BeachMaker.beachNoise = null;
		}

		public static TerrainDef BeachTerrainAt(IntVec3 loc)
		{
			if (BT_BeachMaker.beachNoise == null)
			{
				return null;
			}
			float value = BT_BeachMaker.beachNoise.GetValue(loc);
			if (value < 0.1f)
			{
				return TerrainDefOf.WaterDeep;
			}
			if (value < 0.45f)
			{
				return TerrainDefOf.WaterShallow;
			}
			if (value < 1f)
			{
				return TerrainDefOf.Sand;
			}
			return null;
		}
	}
}
