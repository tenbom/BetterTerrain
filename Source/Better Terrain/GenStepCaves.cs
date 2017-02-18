using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Noise;

namespace Better_Terrain
{
    public class GenStepCaves : GenStep
    {
        private const float ChunkChance = 0.1f;
        private const float ResouceChance = 0.7f;
        private const float BugChance = 0.004f;
        private const int NumberOfHivesPerGroup = 3;
        private const int CorpseRange = 7;
        private const float ChanceCorpseItemToDrop = 0.5f;
        private readonly FloatRange ItemDamage = new FloatRange(0.1f, 0.3f);
        private readonly IntRange NumberOfCorpses = new IntRange(4, 8);
        private readonly FloatRange ResourceDropAmount = new FloatRange(0.4f, 0.9f);

        private ModuleBase _cavesMap;
        private Map _map;

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
                _map.regionAndRoomUpdater.Enabled = true;
                _map.regionAndRoomUpdater.RebuildAllRegionsAndRooms();
                _map.regionAndRoomUpdater.Enabled = false;
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
                    && NeighborsRoofed(current, _map, 3))
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
                if (current.Roofed(_map)
                    && current.GetRoof(_map) == RoofDefOf.RoofRockThick
                    && _cavesMap.GetValue(current.ToIntVec2) > 0
                    && NeighborsRoofed(current, _map, 9)
                    && Rand.Chance(BugChance))
                    SpawnHiveCluster(current);
        }

        private void SpawnHiveCluster(IntVec3 loc)
        {
            var hive = (Hive) ThingMaker.MakeThing(ThingDefOf.Hive);
            hive.SetFaction(Faction.OfInsects);
            hive.active = false;
            hive = (Hive) GenSpawn.Spawn(hive, loc, _map);
            hive.StartInitialPawnSpawnCountdown();
            for (var i = 0; i < NumberOfHivesPerGroup - 1; i++)
            {
                Hive hive2;
                if (hive.GetComp<CompSpawnerHives>().TrySpawnChildHive(true, out hive2))
                {
                    hive = hive2;
                    hive.StartInitialPawnSpawnCountdown();
                }
            }

            var corpseNumber = NumberOfCorpses.RandomInRange;
            var humanCorpses = Rand.RangeInclusive(0, corpseNumber);
            var animalCorpses = corpseNumber - humanCorpses;
            for (var i = 0; i < humanCorpses; i++)
                SpawnHumanCorpses(loc);
            for (var i = 0; i < animalCorpses; i++)
                SpawnAnimalCorpses(loc);
        }

        private void SpawnAnimalCorpses(IntVec3 loc)
        {
            var pawnKindDef = _map.Biome.AllWildAnimals.Where(
                    a => (a.RaceProps.deathActionWorkerClass == typeof(DeathActionWorker_DropBodyParts)
                          || a.RaceProps.deathActionWorkerClass == null)).
                RandomElementByWeight(def => _map.Biome.CommonalityOfAnimal(def) / def.wildSpawn_GroupSizeRange.Average);
            if (pawnKindDef == null)
            {
                Log.Error("No spawnable animals right now.");
                return;
            }
            IntVec3 loc2;
            Predicate<IntVec3> valid = delegate (IntVec3 cell)
            {
                if (!cell.Standable(_map) 
                || !_map.reachability.CanReach(loc, cell, PathEndMode.OnCell, TraverseParms.For(TraverseMode.PassDoors)))
                {
                    return false;
                }
                foreach (Thing thing in this._map.thingGrid.ThingsListAt(cell))
                {
                    if (thing is Corpse)
                    {
                        return false;
                    }
                }
                return true;
            };

            if (CellFinder.TryFindRandomCellNear(loc, _map, CorpseRange, valid, out loc2))
            {
                var pawn = PawnGenerator.GeneratePawn(pawnKindDef);
                SpawnAndKill(pawn, loc2);
            }
        }

        private void SpawnHumanCorpses(IntVec3 loc)
        {
            Predicate<IntVec3> valid = delegate (IntVec3 cell)
            {
                if (!cell.Standable(_map)
                || !_map.reachability.CanReach(loc, cell, PathEndMode.OnCell, TraverseParms.For(TraverseMode.PassDoors)))
                {
                    return false;
                }
                foreach (Thing thing in this._map.thingGrid.ThingsListAt(cell))
                {
                    if (thing is Corpse)
                    {
                        return false;
                    }
                }
                return true;
            };
            IntVec3 loc2;
            if (CellFinder.TryFindRandomCellNear(loc, _map, CorpseRange, valid, out loc2))
            {
                var random = DefDatabase<PawnKindDef>.AllDefs.
                    Where(a => a.RaceProps.Humanlike && a.RaceProps.IsFlesh &&
                               !a.defaultFactionType.isPlayer).RandomElement();
                if (random == null) return;
                var faction = FactionUtility.DefaultFactionFrom(random.defaultFactionType);
                var pawn = PawnGenerator.GeneratePawn(random, faction);
                using (var en = pawn.apparel.WornApparel.GetEnumerator())
                {
                    var toRemove = new List<Apparel>();
                    while (en.MoveNext())
                        if (Rand.Chance(ChanceCorpseItemToDrop))
                        {
                            if (en.Current != null)
                                en.Current.HitPoints = Mathf.CeilToInt(en.Current.MaxHitPoints * ItemDamage.RandomInRange);
                        }
                        else
                            toRemove.Add(en.Current);
                    foreach (var i in toRemove)
                    {
                        pawn.apparel.Remove(i);
                    }
                }
                using (var en = pawn.equipment.AllEquipment.GetEnumerator())
                {
                    var toRemove = new List<ThingWithComps>();
                    while (en.MoveNext())
                        if (Rand.Chance(ChanceCorpseItemToDrop))
                        {
                            if (en.Current != null)
                                en.Current.HitPoints = Mathf.CeilToInt(en.Current.MaxHitPoints * ItemDamage.RandomInRange);
                        }
                        else
                            toRemove.Add(en.Current);
                    foreach (var i in toRemove)
                    {
                        pawn.equipment.Remove(i);
                    }
                }
                pawn.inventory.DestroyAll();
                SpawnAndKill(pawn, loc2);
            }
        }

        private void SpawnAndKill(Pawn pawn, IntVec3 loc)
        {
            if (GenPlace.TryPlaceThing(pawn, loc, _map, ThingPlaceMode.Near))
            {
                HealthUtility.GiveInjuriesToKill(pawn);
                if (pawn.HasAttachment(ThingDefOf.Fire))
                    pawn.GetAttachment(ThingDefOf.Fire).Destroy();
                if (pawn.Corpse.HasAttachment(ThingDefOf.Fire))
                    pawn.Corpse.GetAttachment(ThingDefOf.Fire).Destroy();
                var comp = pawn.Corpse.GetComp<CompRottable>();
                comp.RotProgress = ((CompProperties_Rottable) comp.props).TicksToDessicated;
                pawn.Corpse.RotStageChanged();
                pawn.Corpse.SetForbidden(true);
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