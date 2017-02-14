using System;
using Verse;
using RimWorld;

namespace Better_Terrain
{
	public class BT_GenStep_Terrain : GenStep
	{
		private static bool debug_WarnedMissingTerrain;

		public override void Generate(Map map)
		{
			BT_BeachMaker.Init(map);
			MapGenFloatGrid mapGenFloatGrid = MapGenerator.FloatGridNamed("Elevation", map);
			MapGenFloatGrid mapGenFloatGrid2 = MapGenerator.FloatGridNamed("Fertility", map);
			TerrainGrid terrainGrid = map.terrainGrid;
			foreach (IntVec3 current in map.AllCells)
			{
				Building edifice = current.GetEdifice(map);
				if (edifice != null && edifice.def.Fillage == FillCategory.Full)
				{
					terrainGrid.SetTerrain(current, this.TerrainFrom(current, map, mapGenFloatGrid[current], mapGenFloatGrid2[current], true));
				}
				else
				{
					terrainGrid.SetTerrain(current, this.TerrainFrom(current, map, mapGenFloatGrid[current], mapGenFloatGrid2[current], false));
				}
			}
			BT_BeachMaker.Cleanup();
			foreach (TerrainPatchMaker current2 in map.Biome.terrainPatchMakers)
			{
				current2.Cleanup();
			}
		}

		private TerrainDef TerrainFrom(IntVec3 c, Map map, float elevation, float fertility, bool requireSolid)
		{
			float noise;
			if (requireSolid)
			{
				return BT_GenStep_RocksFromGrid.RockDefAt(map, c).naturalTerrain;
			}
			TerrainDef terrainDef = BT_BeachMaker.BeachTerrainAt(c);
			if (terrainDef != null)
			{
				return terrainDef;
			}
			
			if(elevation>.5)
			{
				noise = elevation*.4f + fertility*.5f;
			}
			
			else
			{
				noise = elevation*.3f + fertility*.75f;
			}
			if (noise >= 0.55f)
			{
				return ThingDefOf.Sandstone.naturalTerrain;
				//return GenStep_RocksFromGrid.RockDefAt(c).naturalTerrain;
			}
			if (noise > 0.5f)
			{
				return TerrainDefOf.Gravel;
			}
			
			for (int i = 0; i < map.Biome.terrainPatchMakers.Count; i++)
			{
				//elevation+=fertility;
				terrainDef = TerrainThreshold.TerrainAtValue(map.Biome.terrainPatchMakers[i].thresholds, noise);
				//terrainDef = map.Biome.terrainPatchMakers[i].TerrainAt(c, map);
				if (terrainDef != null)
				{
					return terrainDef;
				}
			}
			
			terrainDef = TerrainThreshold.TerrainAtValue(map.Biome.terrainsByFertility, noise);
			if (terrainDef != null)
			{
				return terrainDef;
			}
			if (!BT_GenStep_Terrain.debug_WarnedMissingTerrain)
			{
				Log.Error(string.Concat(new object[]
				{
					"No terrain found in biome ",
					map.Biome.defName,
					" for elevation=",
					elevation,
					", fertility=",
					fertility
				}));
				BT_GenStep_Terrain.debug_WarnedMissingTerrain = true;
			}
			return TerrainDefOf.Sand;
		}
	}
}
