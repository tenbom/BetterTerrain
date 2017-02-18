using System.Collections.Generic;
using Harmony;
using RimWorld;
using UnityEngine;
using Verse;

namespace Better_Terrain.Harmony
{
    [HarmonyPatch(typeof(Section))]
    [HarmonyPatch("RegenerateAllLayers")]
    //[HarmonyPatch(new Type[] { typeof(Rect), typeof(int), typeof(int), typeof(float), typeof(float)})]
    internal class RegenerateAllLayers
    {
        private static bool Prefix(Section __instance)
        {
            var _layers = Traverse.Create(__instance).Field("layers").GetValue<List<SectionLayer>>();
            for (var i = 0; i < _layers.Count; i++)
                if (_layers[i].Visible)
                    if (_layers[i].relevantChangeTypes == MapMeshFlag.Terrain)
                        _layers[i] = Regenerate(__instance, _layers[i]);
                    else _layers[i].Regenerate();
            return false;
        }

        private static SectionLayer Regenerate(Section section, SectionLayer layer)
        {
            var ColorWhite = new Color32(255, 255, 255, 255);

            var ColorWhiteClear = new Color32(255, 255, 255, 123);

            var ColorClear = new Color32(255, 255, 255, 0);

            int numSides;
            TerrainDef lastSide;
            foreach (var current in layer.subMeshes)
                current.Clear(MeshParts.All);
            //__instance.ClearSubMeshes(MeshParts.All);
            //TerrainGrid terrainGrid = __instance.Map.terrainGrid;
            var terrainGrid = section.map.terrainGrid;
            //CellRect cellRect = __instance.section.CellRect;
            var cellRect = section.CellRect;
            var array = new TerrainDef[8];
            var hashSet = new HashSet<TerrainDef>();
            var array2 = new bool[8];
            var array3 = new bool[8];
            foreach (var current in cellRect)
            {
                hashSet.Clear();
                var terrainDef = terrainGrid.TerrainAt(current);
                var subMesh = layer.GetSubMesh(terrainDef.DrawMatSingle);
                var count = subMesh.verts.Count;
                subMesh.verts.Add(new Vector3(current.x, 0f, current.z));
                subMesh.verts.Add(new Vector3(current.x, 0f, current.z + 1));
                subMesh.verts.Add(new Vector3(current.x + 1, 0f, current.z + 1));
                subMesh.verts.Add(new Vector3(current.x + 1, 0f, current.z));
                subMesh.colors.Add(ColorWhite);
                subMesh.colors.Add(ColorWhite);
                subMesh.colors.Add(ColorWhite);
                subMesh.colors.Add(ColorWhite);
                subMesh.tris.Add(count);
                subMesh.tris.Add(count + 1);
                subMesh.tris.Add(count + 2);
                subMesh.tris.Add(count);
                subMesh.tris.Add(count + 2);
                subMesh.tris.Add(count + 3);
                for (var i = 0; i < 8; i++)
                {
                    var c = current + GenAdj.AdjacentCellsAroundBottom[i];
                    if (!c.InBounds(section.map))
                    {
                        array[i] = terrainDef;
                    }
                    else
                    {
                        var terrainDef2 = terrainGrid.TerrainAt(c);
                        Thing edifice = c.GetEdifice(section.map);
                        if (edifice != null && edifice.def.coversFloor)
                            terrainDef2 = TerrainDefOf.Underwall;
                        array[i] = terrainDef2;
                        if (terrainDef2 != terrainDef && terrainDef2.edgeType != TerrainDef.TerrainEdgeType.Hard &&
                            terrainDef2.renderPrecedence >= terrainDef.renderPrecedence)
                            if (!hashSet.Contains(terrainDef2))
                                hashSet.Add(terrainDef2);
                    }
                }

                foreach (var current2 in hashSet)
                {
                    var subMesh2 = layer.GetSubMesh(current2.DrawMatSingle);
                    count = subMesh2.verts.Count;
                    subMesh2.verts.Add(new Vector3(current.x + 0.5f, 0f, current.z));
                    subMesh2.verts.Add(new Vector3(current.x, 0f, current.z));
                    subMesh2.verts.Add(new Vector3(current.x, 0f, current.z + 0.5f));
                    subMesh2.verts.Add(new Vector3(current.x, 0f, current.z + 1));
                    subMesh2.verts.Add(new Vector3(current.x + 0.5f, 0f, current.z + 1));
                    subMesh2.verts.Add(new Vector3(current.x + 1, 0f, current.z + 1));
                    subMesh2.verts.Add(new Vector3(current.x + 1, 0f, current.z + 0.5f));
                    subMesh2.verts.Add(new Vector3(current.x + 1, 0f, current.z));
                    subMesh2.verts.Add(new Vector3(current.x + 0.5f, 0f, current.z + 0.5f));
                    for (var j = 0; j < 8; j++)
                        array2[j] = false;
                    for (var k = 0; k < 8; k++)
                        if (k % 2 == 0)
                        {
                            if (array[k] == current2)
                            {
                                var num = k - 1;
                                if (num < 0)
                                    num += 8;
                                //array2[num] = true;
                                array2[k] = true;
                                //array2[(k + 1) % 8] = true;
                            }
                        }
                        else if (array[k] == current2)
                        {
                            //only do corners if one of the sides matches.  Looks REALLY awkword otherwise.
                            var num = k - 1;
                            if (num < 0) num += 8;
                            //
                            if (array[num] == current2 || array[(k + 1) % 8] == current2)
                                array2[k] = true;
                            if (array[num] != terrainDef && array[(k + 1) % 8] != terrainDef)
                                array2[k] = true;
                        }
                    numSides = 0;
                    for (var l = 0; l < 8; l++)
                        if (array2[l])
                        {
                            numSides++;
                            subMesh2.colors.Add(ColorWhite);
                        }
                        else
                        {
                            subMesh2.colors.Add(ColorClear);
                        }
                    if (numSides > 4) subMesh2.colors.Add(ColorWhiteClear);
                    else subMesh2.colors.Add(ColorClear);
                    for (var m = 0; m < 8; m++)
                    {
                        subMesh2.tris.Add(count + m);
                        subMesh2.tris.Add(count + (m + 1) % 8);
                        subMesh2.tris.Add(count + 8);
                    }
                }
            }
            for (var i = 0; i < layer.subMeshes.Count; i++)
                if (layer.subMeshes[i].verts.Count > 0)
                    layer.subMeshes[i].FinalizeMesh(MeshParts.All, false);
            return layer;
            //__instance.FinalizeMesh(MeshParts.All);
        }
    }
}