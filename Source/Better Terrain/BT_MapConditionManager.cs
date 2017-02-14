using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

namespace Better_Terrain
{
	public sealed class BT_MapConditionManager
	{
		public Map map;

		public BT_MapConditionManager(Map map)
		{
			this.map = map;
		}

		internal float AggregatePlantDensityFactor()
		{
			float num = 1f;
			foreach (MapCondition cond in map.mapConditionManager.ActiveConditions)
			{
				num *= cond.PlantDensityFactor();
			}
			return num;
		}
	}
}
