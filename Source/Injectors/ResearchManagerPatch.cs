using System;

using Harmony;

using RimWorld;
using Verse;

namespace ResearchPal
{

	[HarmonyPatch (typeof (ResearchManager))]
	[HarmonyPatch ("ResearchPerformed")]
	[HarmonyPatch (new Type [] { typeof (float), typeof (Pawn) })]
    public class ResearchManagerPatch_ResearchPerformed
    {
		private static ResearchProjectDef currentResearch;

		static void Prefix (ResearchManager __instance, float amount, Pawn researcher) {
			currentResearch = __instance.currentProj;
		}

		static void Postfix (ResearchManager __instance, float amount, Pawn researcher) {
			if (currentResearch != null && currentResearch.IsFinished) {
				__instance.currentProj = Queue.Next (currentResearch);
			}
		}
    }

	[HarmonyPatch (typeof (ResearchManager))]
	[HarmonyPatch ("DoCompletionDialog")]
	[HarmonyPatch (new Type [] { typeof (ResearchProjectDef), typeof (Pawn) })]
	public class ResearchManagerPatch_DoCompletionDialog
	{
		static bool Prefix (ResearchManager __instance, ResearchProjectDef proj, Pawn researcher)
		{
			return Settings.showNotification;
		}
	}
}