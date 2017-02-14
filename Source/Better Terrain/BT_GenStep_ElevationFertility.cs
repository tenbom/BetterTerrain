using RimWorld.Planet;
using System;
using UnityEngine;
using Verse;
using Verse.Noise;
using RimWorld;

namespace Better_Terrain
{
	public class BT_GenStep_ElevationFertility : GenStep
	{
		private const float ElevationFreq = 0.021f;

		private const float FertilityFreq = 0.021f;

		private const float EdgeMountainSpan = 0.42f;

		public override void Generate(Map map)
		{
			NoiseRenderer.renderSize = new IntVec2(map.Size.x, map.Size.z);
			//MOUNTAINITY
			//ModuleBase moduleBase = new Perlin(0.020999999716877937, 2.0, 0.5, 6, Rand.Range(0, 2147483647), QualityMode.High);
			ModuleBase moduleBase = new Perlin(0.02, 3.0, 0.25, 6, Rand.Range(0, 2147483647), QualityMode.Medium);
			moduleBase = new ScaleBias(0.5, 0.5, moduleBase);
			//FERTILITY
			ModuleBase moduleBase3 = new Perlin(0.008, 7.0, 0.3, 6, Rand.Range(0, 2147483647), QualityMode.High);
			moduleBase3 = new ScaleBias(0.65, 0.25, moduleBase3);
			//Plant
			ModuleBase mPlant = new Perlin(0.01, 6.0, 0.35, 6, Rand.Range(0, 2147483647), QualityMode.Medium);
			mPlant = new ScaleBias(0.6, 0.6, mPlant);
			mPlant = new Clamp(0.0, 0.99, mPlant);
			//Plant Density
			ModuleBase mPlantDensity = new Perlin(0.015, 1.5, 0.1, 6, Rand.Range(0, 2147483647), QualityMode.Medium);
			mPlantDensity = new ScaleBias(0.5, 0.9, mPlantDensity);
			mPlantDensity = new Clamp(0.5, 1.5, mPlantDensity);
			//Rock scatter
			//ModuleBase mHardStone = new Perlin(0.35, 6.5, 0.1, 6, Rand.Range(0, 2147483647), QualityMode.Medium);
			//mHardStone = new ScaleBias(0.7, 0.25, mHardStone);
			ModuleBase mHardStone = new Perlin(0.2, 2.0, 0.1, 6, Rand.Range(0, 2147483647), QualityMode.High);
			mHardStone = new ScaleBias(3.5, 0, mHardStone);
			
			
			NoiseDebugUI.StoreNoiseRender(moduleBase, "elev base");
			float num = 1f;
			switch (map.TileInfo.hilliness)
			{
			case Hilliness.Flat:
				num = .6f;
				//num = MapGenTuning.ElevationFactorFlat;
				break;
			case Hilliness.SmallHills:
				num = .8f;
				//num = MapGenTuning.ElevationFactorSmallHills;
				break;
			case Hilliness.LargeHills:
				num = 1.15f;
				//num = MapGenTuning.ElevationFactorLargeHills;
				break;
			case Hilliness.Mountainous:
				num = 1.3f;
				//num = MapGenTuning.ElevationFactorMountains;
				break;
			case Hilliness.Impassable:
				//num = MapGenTuning.ElevationFactorImpassableMountains;
				break;
			}
			moduleBase = new Multiply(moduleBase, new Const((double)num));
			NoiseDebugUI.StoreNoiseRender(moduleBase, "elev world-factored");
			if (map.TileInfo.hilliness == Hilliness.Mountainous || map.TileInfo.hilliness == Hilliness.Impassable)
			{
				ModuleBase moduleBase2 = new DistFromAxis((float)map.Size.x * 0.42f);
				moduleBase2 = new Clamp(0.0, 1.0, moduleBase2);
				moduleBase2 = new Invert(moduleBase2);
				moduleBase2 = new ScaleBias(1.0, 1.0, moduleBase2);
				Rot4 random;
				do
				{
					random = Rot4.Random;
				}
				while (random == Find.World.CoastDirectionAt(map.Tile));
				if (random == Rot4.North)
				{
					moduleBase2 = new Rotate(0.0, 90.0, 0.0, moduleBase2);
					moduleBase2 = new Translate(0.0, 0.0, (double)(-(double)map.Size.z), moduleBase2);
				}
				else if (random == Rot4.East)
				{
					moduleBase2 = new Translate((double)(-(double)map.Size.x), 0.0, 0.0, moduleBase2);
				}
				else if (random == Rot4.South)
				{
					moduleBase2 = new Rotate(0.0, 90.0, 0.0, moduleBase2);
				}
				else if (random == Rot4.West)
				{
				}
				NoiseDebugUI.StoreNoiseRender(moduleBase2, "mountain");
				moduleBase = new Add(moduleBase, moduleBase2);
				NoiseDebugUI.StoreNoiseRender(moduleBase, "elev + mountain");
			}
			float b = (!map.TileInfo.WaterCovered) ? 3.40282347E+38f : 0f;
			MapGenFloatGrid mapGenFloatGrid = MapGenerator.FloatGridNamed("Elevation", map);
			foreach (IntVec3 current in map.AllCells)
			{
				mapGenFloatGrid[current] = Mathf.Min(moduleBase.GetValue(current), b);
			}
			//ModuleBase moduleBase3 = new Perlin(0.020999999716877937, 2.0, 0.5, 6, Rand.Range(0, 2147483647), QualityMode.High);
			
			NoiseDebugUI.StoreNoiseRender(moduleBase3, "noiseFert base");
			MapGenFloatGrid mapGenFloatGrid2 = MapGenerator.FloatGridNamed("Fertility", map);
			foreach (IntVec3 current2 in map.AllCells)
			{
				mapGenFloatGrid2[current2] = moduleBase3.GetValue(current2);
			}
			MapGenFloatGrid mapGenFloatGrid3 = MapGenerator.FloatGridNamed("Plant", map);
			MapGenFloatGrid mapGenFloatGrid4 = MapGenerator.FloatGridNamed("PlantDensity", map);
			MapGenFloatGrid mapGenFloatGrid5 = MapGenerator.FloatGridNamed("HardStone", map);
			foreach (IntVec3 current3 in map.AllCells)
			{
				mapGenFloatGrid3[current3] = mPlant.GetValue(current3);
				mapGenFloatGrid4[current3] = mPlantDensity.GetValue(current3);
				mapGenFloatGrid5[current3] = mHardStone.GetValue(current3);
			}
			foreach (IntVec3 current4 in map.AllCells)
			{
				
			}
		}
	}
}
