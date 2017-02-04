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
			List<ThingDef> treeList = new List<ThingDef>();
			int numTrees = 0;
			List<ThingDef> fillerList = new List<ThingDef>();
			int numFiller = 0;
			List<ThingDef> grassList = new List<ThingDef>();
			int numGrass = 0;
			List<ThingDef> choiceList = new List<ThingDef>();
			int numChoices = 0;
			foreach(ThingDef thing in list)
			{
				if(thing.plant.blockAdjacentSow)
				{
					treeList.Add(thing);
					numTrees++;
				}
				else if(thing.hideAtSnowDepth>.35)
				{
					fillerList.Add(thing);
					numFiller++;
				}
				else
				{
					grassList.Add(thing);
					numGrass++;
				}
			}
			for (int i = 0; i < list.Count; i++)
			{
				BT_GenStep_Plants.numExtant.Add(list[i], 0);
			}
			BT_GenStep_Plants.desiredProportions = GenPlant.CalculateDesiredPlantProportions(map.Biome);
			
			float num = map.Biome.plantDensity * condMan.AggregatePlantDensityFactor();
//			if(Find.WorldGrid[map.Tile].biome == BiomeDefOf.BorealForest)
//			{
			MapGenFloatGrid plantGrid = MapGenerator.FloatGridNamed("Tree", map);
			foreach (IntVec3 c in map.AllCells.InRandomOrder(null))
			{
				if (c.GetEdifice(map) == null && c.GetCover(map) == null)
				{
					float num2 = map.fertilityGrid.FertilityAt(c);
					float num3 = num2 * num;
					if (Rand.Value < num3)
					{
						IEnumerable<ThingDef> source = from def in list
						where def.CanEverPlantAt(c, map)
						select def;
						if (source.Any<ThingDef>())
						{
							choiceList.Clear();
							numChoices = 0;
							if(num2>.5 && numTrees>0)
							{
								choiceList.Add(treeList[(int)(plantGrid[c]*numTrees)]);
								numChoices++;
							}
							if(numFiller>0)
							{
								choiceList.Add(fillerList[(int)(plantGrid[c]*numFiller)]);
								numChoices++;
							}
							if(numGrass>0)
							{
								choiceList.Add(grassList[(int)(plantGrid[c]*numGrass)]);
								numChoices++;
							}
							//else choiceList.Add(ThingDefOf.PlantGrass);
							ThingDef thingDef = choiceList[Rand.Range(0,numChoices)];
							
							
							//thingDef = source.RandomElementByWeight((ThingDef x) => BT_GenStep_Plants.PlantChoiceWeight(x, map));
							int randomInRange = thingDef.plant.wildClusterSizeRange.RandomInRange;
							for (int j = 0; j < randomInRange; j++)
							{
								IntVec3 c2;
								if (j == 0)
								{
									c2 = c;
								}
								else if (!GenPlantReproduction.TryFindReproductionDestination(c, thingDef, SeedTargFindMode.MapGenCluster, map, out c2))
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
	}
}
