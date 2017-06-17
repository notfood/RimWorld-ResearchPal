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
        static void Postfix (ListerBuildings __instance, Building b)
        {
            var researchBench = b as Building_ResearchBench;
            if (researchBench != null) {
                ResearchPalMod.allResearchBenches.Add (researchBench);
            }
        }
    }

    [HarmonyPatch (typeof (ListerBuildings))]
    [HarmonyPatch ("Remove")]
    [HarmonyPatch (new Type [] { typeof (Building) })]
    public class ListerBuildingsPatch_Remove
    {
        static void Postfix (ListerBuildings __instance, Building b)
        {
            var researchBench = b as Building_ResearchBench;
            if (researchBench != null) {
                ResearchPalMod.allResearchBenches.Remove (researchBench);
            }
        }
    }
}
