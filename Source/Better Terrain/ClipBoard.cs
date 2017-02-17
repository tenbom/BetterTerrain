/*
 * Created by SharpDevelop.
 * User: tim
 * Date: 2/15/2017
 * Time: 2:11 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using Verse;
using Harmony;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;

namespace Better_Terrain
{
    
	public class HarmonyDef : Def
    {
		public bool patched;
    	public HarmonyDef()
    	{
    		if(!patched)
    		{
    			var harmony = HarmonyInstance.Create("com.github.betterTerrain.rimworld.mod.065");
            	harmony.PatchAll(Assembly.GetExecutingAssembly());
            	patched = true;
    		}
    	}
    }

    [HarmonyPatch(typeof(Section))]
    [HarmonyPatch("RegenerateAllLayers")]
    //[HarmonyPatch(new Type[] { typeof(Rect), typeof(int), typeof(int), typeof(float), typeof(float)})]
    //class PatchClass
    {
        static bool Prefix(Section __instance)
        {
        	List<SectionLayer> _layers = Traverse.Create(__instance).Field("layers").GetValue<List<SectionLayer>>();
        	for(int i = 0; i< _layers.Count; i++)
        	{
        		if(_layers[i].Visible)
        		{
        			if(_layers[i].relevantChangeTypes == Verse.MapMeshFlag.Terrain)
        			{
        				_layers[i] = Regenerate(__instance, _layers[i]);
        				//_layers[i].Regenerate();
        			}
        			else _layers[i].Regenerate();
        		}
        	}
        	return false;
        }
        static SectionLayer Regenerate(Section section, SectionLayer layer)
        {
        	Color32 ColorWhite = new Color32(255, 255, 255, 255);
        	
        	Color32 ColorHalfWhite = new Color32(255, 255, 255, 123);
        	Color32 ColorQuarterWhite = new Color32(255, 255, 255, 100);

			Color32 ColorClear = new Color32(255, 255, 255, 0);
			
			int numSides;
			TerrainDef lastSide;
        	foreach (LayerSubMesh current in layer.subMeshes)
			{
				current.Clear(MeshParts.All);
			}
        	//__instance.ClearSubMeshes(MeshParts.All);
			//TerrainGrid terrainGrid = __instance.Map.terrainGrid;
			TerrainGrid terrainGrid = section.map.terrainGrid;
			//CellRect cellRect = __instance.section.CellRect;
			CellRect cellRect = section.CellRect;
			TerrainDef[] array = new TerrainDef[8];
			HashSet<TerrainDef> hashSet = new HashSet<TerrainDef>();
			bool[] array2 = new bool[8];
			bool[] array3 = new bool[8];
			foreach (IntVec3 current in cellRect)
			{
				hashSet.Clear();
				TerrainDef terrainDef = terrainGrid.TerrainAt(current);
				LayerSubMesh subMesh = layer.GetSubMesh(terrainDef.DrawMatSingle);
				int count = subMesh.verts.Count;
				subMesh.verts.Add(new Vector3((float)current.x, 0f, (float)current.z));
				subMesh.verts.Add(new Vector3((float)current.x, 0f, (float)(current.z + 1)));
				subMesh.verts.Add(new Vector3((float)(current.x + 1), 0f, (float)(current.z + 1)));
				subMesh.verts.Add(new Vector3((float)(current.x + 1), 0f, (float)current.z));
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
				for (int i = 0; i < 8; i++)
				{
					IntVec3 c = current + GenAdj.AdjacentCellsAroundBottom[i];
					if (!c.InBounds(section.map))
					{
						array[i] = terrainDef;
					}
					else
					{
						TerrainDef terrainDef2 = terrainGrid.TerrainAt(c);
						Thing edifice = c.GetEdifice(section.map);
						if (edifice != null && edifice.def.coversFloor)
						{
							terrainDef2 = RimWorld.TerrainDefOf.Underwall;
						}
						array[i] = terrainDef2;
						if (terrainDef2 != terrainDef && terrainDef2.edgeType != TerrainDef.TerrainEdgeType.Hard && terrainDef2.renderPrecedence >= terrainDef.renderPrecedence)
						{
							if (!hashSet.Contains(terrainDef2))
							{
								hashSet.Add(terrainDef2);
							}
						}
					}
				}
				
				foreach (TerrainDef current2 in hashSet)
				{
					LayerSubMesh subMesh2 = layer.GetSubMesh(current2.DrawMatSingle);
					count = subMesh2.verts.Count;
					subMesh2.verts.Add(new Vector3((float)current.x + 0.5f, 0f, (float)current.z));
					subMesh2.verts.Add(new Vector3((float)current.x, 0f, (float)current.z));
					subMesh2.verts.Add(new Vector3((float)current.x, 0f, (float)current.z + 0.5f));
					subMesh2.verts.Add(new Vector3((float)current.x, 0f, (float)(current.z + 1)));
					subMesh2.verts.Add(new Vector3((float)current.x + 0.5f, 0f, (float)(current.z + 1)));
					subMesh2.verts.Add(new Vector3((float)(current.x + 1), 0f, (float)(current.z + 1)));
					subMesh2.verts.Add(new Vector3((float)(current.x + 1), 0f, (float)current.z + 0.5f));
					subMesh2.verts.Add(new Vector3((float)(current.x + 1), 0f, (float)current.z));
					subMesh2.verts.Add(new Vector3((float)current.x + 0.5f, 0f, (float)current.z + 0.5f));
					for (int j = 0; j < 8; j++)
					{
						array2[j] = false;
						array3[j] = false;
					}
					for (int k = 0; k < 8; k++)
					{
						//k%2 is a side
						//!k%2 is a corner
						//this i
						if (k % 2 == 0)
						{
							if (array[k] == current2)
							{
								int num = k - 1;
								if (num < 0)
								{
									num += 8;
								}
								//array2[num] = true;
								array2[k] = true;
								//array2[(k + 1) % 8] = true;
							}
						}
						else if (array[k] == current2)
						{
							int num = k - 1;
								if (num < 0)
								{
									num += 8;
								}
								array2[num] = true;
								array2[k] = true;
								array2[(k + 1) % 8] = true;
						}
					}
					numSides = 0;
					for (int l = 0; l < 8; l++)
					{
						
						if (array2[l])
						{
							numSides++;
							subMesh2.colors.Add(ColorWhite);
						}
						else
						{
							subMesh2.colors.Add(ColorClear);
						}
					}
					if(numSides>5) subMesh2.colors.Add(ColorHalfWhite);
					else subMesh2.colors.Add(ColorWhite);
					//subMesh2.colors.Add(ColorWhite);
					for (int m = 0; m < 8; m++)
					{
						subMesh2.tris.Add(count + m);
						subMesh2.tris.Add(count + (m + 1) % 8);
						subMesh2.tris.Add(count + 8);
					}
				}
			}
			for (int i = 0; i < layer.subMeshes.Count; i++)
			{
				if (layer.subMeshes[i].verts.Count > 0)
				{
					layer.subMeshes[i].FinalizeMesh(MeshParts.All, false);
				}
			}
			return layer;
			//__instance.FinalizeMesh(MeshParts.All);
        }
    }

//	[HarmonyPatch(typeof(SectionLayer))]
//    [HarmonyPatch("Regenerate")]
//    //[HarmonyPatch(new Type[] { typeof(Rect), typeof(int), typeof(int), typeof(float), typeof(float)})]
//    class PatchClass
//    {
//        static bool Prefix(SectionLayer __instance)
//        {
//        	foreach (LayerSubMesh current in __instance.subMeshes)
//			{
//				current.Clear(MeshParts.All);
//			}
//        	//__instance.ClearSubMeshes(MeshParts.All);
//			//TerrainGrid terrainGrid = __instance.Map.terrainGrid;
//			//CellRect cellRect = __instance.section.CellRect;
//			TerrainDef[] array = new TerrainDef[8];
//			HashSet<TerrainDef> hashSet = new HashSet<TerrainDef>();
//			bool[] array2 = new bool[8];
//			foreach (IntVec3 current in cellRect)
//			{
//				hashSet.Clear();
//				TerrainDef terrainDef = terrainGrid.TerrainAt(current);
//				LayerSubMesh subMesh = __instance.GetSubMesh(terrainDef.DrawMatSingle);
//				int count = subMesh.verts.Count;
//				subMesh.verts.Add(new Vector3((float)current.x, 0f, (float)current.z));
//				subMesh.verts.Add(new Vector3((float)current.x, 0f, (float)(current.z + 1)));
//				subMesh.verts.Add(new Vector3((float)(current.x + 1), 0f, (float)(current.z + 1)));
//				subMesh.verts.Add(new Vector3((float)(current.x + 1), 0f, (float)current.z));
//				subMesh.colors.Add(SectionLayer_Terrain.ColorWhite);
//				subMesh.colors.Add(SectionLayer_Terrain.ColorWhite);
//				subMesh.colors.Add(SectionLayer_Terrain.ColorWhite);
//				subMesh.colors.Add(SectionLayer_Terrain.ColorWhite);
//				subMesh.tris.Add(count);
//				subMesh.tris.Add(count + 1);
//				subMesh.tris.Add(count + 2);
//				subMesh.tris.Add(count);
//				subMesh.tris.Add(count + 2);
//				subMesh.tris.Add(count + 3);
//				for (int i = 0; i < 8; i++)
//				{
//					IntVec3 c = current + GenAdj.AdjacentCellsAroundBottom[i];
//					if (!c.InBounds(base.Map))
//					{
//						array[i] = terrainDef;
//					}
//					else
//					{
//						TerrainDef terrainDef2 = terrainGrid.TerrainAt(c);
//						Thing edifice = c.GetEdifice(base.Map);
//						if (edifice != null && edifice.def.coversFloor)
//						{
//							terrainDef2 = TerrainDefOf.Underwall;
//						}
//						array[i] = terrainDef2;
//						if (terrainDef2 != terrainDef && terrainDef2.edgeType != TerrainDef.TerrainEdgeType.Hard && terrainDef2.renderPrecedence >= terrainDef.renderPrecedence)
//						{
//							if (!hashSet.Contains(terrainDef2))
//							{
//								hashSet.Add(terrainDef2);
//							}
//						}
//					}
//				}
//				foreach (TerrainDef current2 in hashSet)
//				{
//					LayerSubMesh subMesh2 = base.GetSubMesh(current2.DrawMatSingle);
//					count = subMesh2.verts.Count;
//					subMesh2.verts.Add(new Vector3((float)current.x + 0.5f, 0f, (float)current.z));
//					subMesh2.verts.Add(new Vector3((float)current.x, 0f, (float)current.z));
//					subMesh2.verts.Add(new Vector3((float)current.x, 0f, (float)current.z + 0.5f));
//					subMesh2.verts.Add(new Vector3((float)current.x, 0f, (float)(current.z + 1)));
//					subMesh2.verts.Add(new Vector3((float)current.x + 0.5f, 0f, (float)(current.z + 1)));
//					subMesh2.verts.Add(new Vector3((float)(current.x + 1), 0f, (float)(current.z + 1)));
//					subMesh2.verts.Add(new Vector3((float)(current.x + 1), 0f, (float)current.z + 0.5f));
//					subMesh2.verts.Add(new Vector3((float)(current.x + 1), 0f, (float)current.z));
//					subMesh2.verts.Add(new Vector3((float)current.x + 0.5f, 0f, (float)current.z + 0.5f));
//					for (int j = 0; j < 8; j++)
//					{
//						array2[j] = false;
//					}
//					for (int k = 0; k < 8; k++)
//					{
//						if (k % 2 == 0)
//						{
//							if (array[k] == current2)
//							{
//								int num = k - 1;
//								if (num < 0)
//								{
//									num += 8;
//								}
//								array2[num] = true;
//								array2[k] = true;
//								array2[(k + 1) % 8] = true;
//							}
//						}
//						else if (array[k] == current2)
//						{
//							array2[k] = true;
//						}
//					}
//					for (int l = 0; l < 8; l++)
//					{
//						if (array2[l])
//						{
//							subMesh2.colors.Add(SectionLayer_Terrain.ColorWhite);
//						}
//						else
//						{
//							subMesh2.colors.Add(SectionLayer_Terrain.ColorClear);
//						}
//					}
//					subMesh2.colors.Add(SectionLayer_Terrain.ColorClear);
//					for (int m = 0; m < 8; m++)
//					{
//						subMesh2.tris.Add(count + m);
//						subMesh2.tris.Add(count + (m + 1) % 8);
//						subMesh2.tris.Add(count + 8);
//					}
//				}
//			}
//			for (int i = 0; i < __instance.subMeshes.Count; i++)
//			{
//				if (__instance.subMeshes[i].verts.Count > 0)
//				{
//					__instance.subMeshes[i].FinalizeMesh(MeshParts.All, false);
//				}
//			}
//			//__instance.FinalizeMesh(MeshParts.All);
//			return false;
//		}  
//    }
}
