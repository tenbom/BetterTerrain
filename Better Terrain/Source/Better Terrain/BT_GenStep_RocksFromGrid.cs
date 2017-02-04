using RimWorld.Planet;
using System;
using System.Collections.Generic;
using Verse;
using Verse.Noise;
using RimWorld;

namespace Better_Terrain
{
	public class BT_GenStep_RocksFromGrid : GenStep
	{
		private class RoofThreshold
		{
			public RoofDef roofDef;

			public float minGridVal;
		}

		private const int MinRoofedCellsPerGroup = 20;

		public static ThingDef RockDefAt(IntVec3 c)
		{
			ThingDef thingDef = null;
			float num = -999999f;
			for (int i = 0; i < RockNoises.rockNoises.Count; i++)
			{
				float value = RockNoises.rockNoises[i].noise.GetValue(c);
				if (value > num)
				{
					thingDef = RockNoises.rockNoises[i].rockDef;
					num = value;
				}
			}
			if (thingDef == null)
			{
				Log.ErrorOnce("Did not get rock def to generate at " + c, 50812);
				thingDef = ThingDefOf.Sandstone;
			}
			return thingDef;
		}

		public override void Generate(Map map)
		{
			if (map.TileInfo.WaterCovered)
			{
				return;
			}
			map.regionAndRoomUpdater.Enabled = false;
			float num = 0.15f;
			List<BT_GenStep_RocksFromGrid.RoofThreshold> list = new List<BT_GenStep_RocksFromGrid.RoofThreshold>();
			list.Add(new BT_GenStep_RocksFromGrid.RoofThreshold
			{
				roofDef = RoofDefOf.RoofRockThick,
				minGridVal = num * 1.14f
			});
			list.Add(new BT_GenStep_RocksFromGrid.RoofThreshold
			{
				roofDef = RoofDefOf.RoofRockThin,
				minGridVal = num * 1.04f
			});
			MapGenFloatGrid elevation = MapGenerator.Elevation;
			MapGenFloatGrid fertility = MapGenerator.FloatGridNamed("Fertility", map);
			//==============Create mountains at (high fert and high elev) or (low fert+low elev+not on water), creates sprawling interesting terrain.
			foreach (IntVec3 current in map.AllCells)
			{
				float num2 = elevation[current] - .1f;
				float fert = fertility[current]-.16f;
				if(fert<0) fert *= -1;
				float mountainability = fert*num2;
				//if (num2 > num)
				//if((mountainability>num) && (elevation[current]*.3f + fertility[current]*.75f > -.05f))
				if(mountainability>num)
				{
					ThingDef def = BT_GenStep_RocksFromGrid.RockDefAt(current);
					GenSpawn.Spawn(def, current, map);
					for (int i = 0; i < list.Count; i++)
					{
						if (num2 > list[i].minGridVal)
						{
							map.roofGrid.SetRoof(current, list[i].roofDef);
							break;
						}
					}
				}
			}
			BoolGrid visited = new BoolGrid(map);
			List<IntVec3> toRemove = new List<IntVec3>();
			foreach (IntVec3 current2 in map.AllCells)
			{
				if (!visited[current2])
				{
					if (this.IsNaturalRoofAt(current2, map))
					{
						toRemove.Clear();
						map.floodFiller.FloodFill(current2, (IntVec3 x) => this.IsNaturalRoofAt(x, map), delegate(IntVec3 x)
						{
							visited[x] = true;
							toRemove.Add(x);
						});
						if (toRemove.Count < 20)
						{
							for (int j = 0; j < toRemove.Count; j++)
							{
								map.roofGrid.SetRoof(toRemove[j], null);
							}
						}
					}
				}
			}
			GenStep_ScatterLumpsMineable genStep_ScatterLumpsMineable = new GenStep_ScatterLumpsMineable();
			float num3 = 10f;
			Tile temp = Find.WorldGrid[map.Tile];
			Hilliness h = temp.hilliness;
			BiomeDef b = temp.biome;
			switch (h)
			{
			case Hilliness.Flat:
				num3 = 6f;
				break;
			case Hilliness.SmallHills:
				num3 = 8f;
				break;
			case Hilliness.LargeHills:
				num3 = 10f;
				break;
			case Hilliness.Mountainous:
				num3 = 12f;
				break;
			case Hilliness.Impassable:
				num3 = 14f;
				break;
			}
			if(b == BiomeDefOf.Tundra) 					num3 *= 1.5f;
			else if(b == BiomeDefOf.TemperateForest) 	num3 -= 6f;
			else if(b == BiomeDefOf.AridShrubland)		num3 *= 1.4f;
			else if(b == BiomeDefOf.BorealForest)		num3 *= 1.25f;
			else if(b == BiomeDefOf.Desert)				num3 *= .7f;
			genStep_ScatterLumpsMineable.countPer10kCellsRange = new FloatRange(num3, num3);
			genStep_ScatterLumpsMineable.Generate(map);
			map.regionAndRoomUpdater.Enabled = true;
		}

		private bool IsNaturalRoofAt(IntVec3 c, Map map)
		{
			return c.Roofed(map) && c.GetRoof(map).isNatural;
		}
	}
}
