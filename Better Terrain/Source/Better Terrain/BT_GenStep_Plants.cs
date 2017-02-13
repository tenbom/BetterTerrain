using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.Noise;
using RimWorld;

namespace Better_Terrain
{
	public class BT_GenStep_Plants : GenStep
	{
		private const float PlantMinGrowth = 0.07f;

		private const float PlantGrowthFactor = 1.2f;

		private static Dictionary<ThingDef, int> numExtant = new Dictionary<ThingDef, int>();

		private static Dictionary<ThingDef, float> desiredProportions = new Dictionary<ThingDef, float>();

		private static int totalExtant = 0;
		
		private ModuleBase PerlinPlantGrid(Map map, List<ThingDef> list, float plantDensity)
		{
			ModuleBase moduleBase = new Perlin(0.008, 7.0, 0.3, 6, Rand.Range(0, 2147483647), QualityMode.Medium);
			return moduleBase;
		}

		public override void Generate(Map map)
		{
			//BT_GenStep_Plants.<Generate>c__AnonStorey2C1 <Generate>c__AnonStorey2C = new BT_GenStep_Plants.<Generate>c__AnonStorey2C1();
			map = map;
			BT_MapConditionManager condMan = new BT_MapConditionManager(map);
			map.regionAndRoomUpdater.Enabled = false;
			List<ThingDef> list = map.Biome.AllWildPlants.ToList<ThingDef>();
			
			//FINDING OUT WHAT PLANTS THERE ARE AND WHERE THEY SHOULD BE SPAWNED, either in fertile or unfertile soil and scatter randomly or clumped up
			
			//BT_PlantsByWeight fPlants = new PlantsByWeight();
			//BT_PlantsByWeight ufPlants = new PlantsByWeight();
			
			for (int i = 0; i < list.Count; i++)
			{
				BT_GenStep_Plants.numExtant.Add(list[i], 0);
			}
			BT_GenStep_Plants.desiredProportions = GenPlant.CalculateDesiredPlantProportions(map.Biome);
			
			List<ThingDef> canPlaceList = new List<ThingDef>();
			float num = map.Biome.plantDensity * condMan.AggregatePlantDensityFactor();
			
			MapGenFloatGrid plantDensity = MapGenerator.FloatGridNamed("PlantDensity", map);
			foreach (IntVec3 c in map.AllCells.InRandomOrder(null))
			{
				if (c.GetEdifice(map) == null && c.GetCover(map) == null)
				{
					float num2 = map.fertilityGrid.FertilityAt(c) * plantDensity[c];
					float num3 = num2 * num;
					if (Rand.Value < num3)
					{
						IEnumerable<ThingDef> source = from def in list
						where def.CanEverPlantAt(c, map)
						select def;
						if (source.Any<ThingDef>())
						{
							List<ThingDef> plants = BT_GenStep_Plants.getPlantsAt(map, c);
							//List<ThingDef> plants =  map.Biome.AllWildPlants.ToList<ThingDef>();
							//plants.Add(ThingDefOf.PlantGrass);
							
							ThingDef thingDef = plants.RandomElementByWeight((ThingDef x) => BT_GenStep_Plants.PlantChoiceWeight(x, map));
							int randomInRange = thingDef.plant.wildClusterSizeRange.RandomInRange;
							for (int j = 0; j < randomInRange; j++)
							{
								IntVec3 c2;
								if (j == 0)
								{
									c2 = c;
								}
								else if (!BT_GenPlantReproduction.TryFindReproductionDestination(c, thingDef, SeedTargFindMode.MapGenCluster, map, plantDensity[c], out c2))
								{
									break;
								}
								Plant plant = (Plant)ThingMaker.MakeThing(thingDef, null);
								plant.Growth = Rand.Range(0.07f, 1f);
								if (plant.def.plant.LimitedLifespan)
								{
									plant.Age = Rand.Range(0, plant.def.plant.LifespanTicks - 50);
								}
								GenSpawn.Spawn(plant, c2, map);
								BT_GenStep_Plants.RecordAdded(thingDef);
							}
						}
					}
				}
			}
			BT_GenStep_Plants.numExtant.Clear();
			BT_GenStep_Plants.desiredProportions.Clear();
			BT_GenStep_Plants.totalExtant = 0;
			map.regionAndRoomUpdater.Enabled = true;
		}

		private static float PlantChoiceWeight(ThingDef def, Map map)
		{
			float num = map.Biome.CommonalityOfPlant(def);
			if (BT_GenStep_Plants.totalExtant > 100)
			{
				float num2 = (float)BT_GenStep_Plants.numExtant[def] / (float)BT_GenStep_Plants.totalExtant;
				if (num2 < BT_GenStep_Plants.desiredProportions[def] * 0.8f)
				{
					num *= 4f;
				}
			}
			return num / def.plant.wildClusterSizeRange.Average;
		}

		private static void RecordAdded(ThingDef plantDef)
		{
			BT_GenStep_Plants.totalExtant++;
			Dictionary<ThingDef, int> dictionary;
			Dictionary<ThingDef, int> expr_11 = dictionary = BT_GenStep_Plants.numExtant;
			int num = dictionary[plantDef];
			expr_11[plantDef] = num + 1;
		}
		
		private static List<ThingDef> getPlantsAt(Map map, IntVec3 c)
		{
			List<ThingDef> list = map.Biome.AllWildPlants.ToList<ThingDef>();
			List<ThingDef> clumpPlants = new List<ThingDef>();
			List<ThingDef> scatterPlants = new List<ThingDef>();
			
			TerrainDef terrain = map.terrainGrid.TerrainAt(c);
			
			bool onTerrain;
			bool isClump;
			bool isKnown;
			foreach(ThingDef thing in list)
			{
				onTerrain = false;
				isClump = false;
				isKnown = false;
				foreach(String tag in thing.plant.sowTags)
				{
					//if the tag is designating a terrain it is to be placed on
					if(tag[0] == '/')
					{
						isKnown = true;
						//check to see if that string matches this terrain
						if(terrain.texturePath.Contains(tag))
						{
							onTerrain = true;
						}
					}
					else if(tag == "Clump") isClump = true;
				}
				if(onTerrain)
				{
					if(isClump) clumpPlants.Add(thing);
					else scatterPlants.Add(thing);
				}
				else if(!isKnown)
				{
					scatterPlants.Add(thing);
				}
			}
			
			MapGenFloatGrid plantGrid = MapGenerator.FloatGridNamed("Plant", map);
			int num = clumpPlants.Count;
			if(num>0)
			{
				num = (int)(clumpPlants.Count*plantGrid[c]);
				scatterPlants.Add(clumpPlants[num]);
			}
			if(scatterPlants.Count ==0) scatterPlants.Add(list[0]); //add an item
			return scatterPlants;
		}
	}
}
