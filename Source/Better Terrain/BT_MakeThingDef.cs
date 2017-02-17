/*
 * Created by SharpDevelop.
 * User: tim
 * Date: 2/14/2017
 * Time: 5:41 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using RimWorld;
using Verse;

namespace Better_Terrain
{
	/// <summary>
	/// Description of BT_MakeThingDef.
	/// </summary>
	public static class BT_MakeThingDef
	{
		public static ThingDef copyPlant(ThingDef plant)
		{
			ThingDef temp = new ThingDef();
			//temp.CompDefFor<plant>();
			return temp;
		}
	}
}
