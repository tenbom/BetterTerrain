using System;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace Better_Terrain
{
    public class GenStepCaves : GenStep
    {
        private const float ChunkChance = 0.1f;
        private const float ResouceChance = 0.7f;
        private readonly FloatRange ResourceDropAmount = new FloatRange(0.4f, 0.9f);
        private const float BugChance = 0.004f;
        private const int NumberOfHivesPerGroup = 3;

        private Map _map;
        private ModuleBase _cavesMap;
        public override void Generate(Map map)
        {
            if (map.TileInfo.hilliness == Hilliness.Mountainous || map.TileInfo.hilliness == Hilliness.Impassable)
            {
                _map = map;
                _map.regionAndRoomUpdater.Enabled = false;

                ModuleBase caves = new Perlin(0.03, 2.0, 0.5, 6, Rand.Range(0, 2147483647), QualityMode.High);
                caves = new Filter(caves, -0.125f, 0.125f);
                NoiseDebugUI.StoreNoiseRender(caves, "caves");
                _cavesMap = caves;

                CarveCaves();
                _map.pathGrid.ResetPathGrid();
                PopulateCaves();

                _map.regionAndRoomUpdater.Enabled = true;
            }
        }

        private void CarveCaves()
        {
            foreach (var current in _map.AllCells)
                if (current.Roofed(_map)
                    && current.GetRoof(_map) == RoofDefOf.RoofRockThick
                    && _cavesMap.GetValue(current.ToIntVec2) > 0
                    && NeighborsRoofed(current, _map, 2))
                    foreach (var currentThing in _map.thingGrid.ThingsAt(current))
                        if (currentThing.def.building?.mineableThing != null)
                        {
                            if (Rand.Chance(ChunkChance) && currentThing.def.building.mineableThing.stackLimit == 1)
                            {
                                var thing = ThingMaker.MakeThing(currentThing.def.building.mineableThing);
                                thing.stackCount = 1;
                                GenSpawn.Spawn(thing, current, _map);
                            }
                            else if (Rand.Chance(ResouceChance)
                                && currentThing.def.building.mineableThing.stackLimit > 1)
                            {
                                var thing = ThingMaker.MakeThing(currentThing.def.building.mineableThing);
                                thing.stackCount =
                                    Mathf.CeilToInt(currentThing.def.building.mineableYield
                                                    * ResourceDropAmount.RandomInRange);
                                thing.SetForbidden(true);
                                GenSpawn.Spawn(thing, current, _map);
                            }
                            currentThing.Destroy();
                        }
        }

        private void PopulateCaves()
        {
            foreach (var current in _map.AllCells)
            {
                if (current.Roofed(_map)
                    && current.GetRoof(_map) == RoofDefOf.RoofRockThick
                    && _cavesMap.GetValue(current.ToIntVec2) > 0
                    && NeighborsRoofed(current, _map, 9)
                    && Rand.Chance(BugChance))
                    SpawnHiveCluster(current);
            }
        }

        private void SpawnHiveCluster(IntVec3 loc)
        {
            var hive = (Hive)ThingMaker.MakeThing(ThingDefOf.Hive);
            hive.SetFaction(Faction.OfInsects);
            hive.active = false;
            hive = (Hive)GenSpawn.Spawn(hive, loc, _map);
            //Uncomment for a lot of spiders 
            //hive.StartInitialPawnSpawnCountdown();
            if (_map.regionGrid.GetValidRegionAt(loc) == null)
                _map.regionMaker.TryGenerateRegionFrom(loc);
            for (var i = 0; i < NumberOfHivesPerGroup - 1; i++)
            {
                Hive hive2;
                if (hive.GetComp<CompSpawnerHives>().TrySpawnChildHive(true, out hive2))
                {
                    hive = hive2;
                    //Uncomment for a lot of spiders 
                    //hive.StartInitialPawnSpawnCountdown();
                }
            }
        }

        public static bool NeighborsRoofed(IntVec3 c, Map map, int n)
        {
            var left = Math.Max(c.x - n, 0);
            var right = Math.Min(c.x + n, map.Size.x - 1);

            var top = Math.Max(c.z - n, 0);
            var bottom = Math.Min(c.z + n, map.Size.z - 1);

            for (var y = top; y <= bottom; y++)
                for (var x = left; x <= right; x++)
                    if (!map.roofGrid.Roofed(x, y) || map.roofGrid.RoofAt(c) != RoofDefOf.RoofRockThick)
                        return false;
            return true;
        }
    }
}