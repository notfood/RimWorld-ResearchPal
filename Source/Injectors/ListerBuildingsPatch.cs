using System;

using Harmony;

using RimWorld;
using Verse;

namespace ResearchPal
{
	[HarmonyPatch (typeof (ListerBuildings))]
	[HarmonyPatch ("Add")]
	[HarmonyPatch (new Type [] { typeof (Building) })]
	public class ListerBuildingsPatch_Add
	{
		static void Postfix (ListerBuildings __instance, Building b) {
			if (b is Building_ResearchBench) {
				ResearchPalMod.allResearchBenches.Add ((Building_ResearchBench) b);
			}
		}
	}

	[HarmonyPatch (typeof (ListerBuildings))]
	[HarmonyPatch ("Remove")]
	[HarmonyPatch (new Type [] { typeof (Building) })]
	public class ListerBuildingsPatch_Remove
	{
		static void Postfix (ListerBuildings __instance, Building b) {
			if (b is Building_ResearchBench) {
				ResearchPalMod.allResearchBenches.Remove ((Building_ResearchBench) b);
			}
		}
	}
}
