using System;
using System.Collections.Generic;

using RimWorld;
using Verse;

namespace ResearchPal
{
	public class ResearchPalMod : Mod
	{
		public static readonly List<Building_ResearchBench> allResearchBenches = new List<Building_ResearchBench> ();

		public ResearchPalMod (ModContentPack mcp) : base (mcp)
		{
			Harmony.HarmonyInstance.Create ("rimworld.research_engine").PatchAll (System.Reflection.Assembly.GetExecutingAssembly ());

			LongEventHandler.QueueLongEvent (ResearchTree.Initialize, "BuildingResearchTree", false, null);
			LongEventHandler.ExecuteWhenFinished (InitializeHelpSuport);

			GetSettings<Settings> ();
		}

		#region Overrides of Mod

		public override string SettingsCategory () { return "ResearchPal".Translate (); }
		public override void DoSettingsWindowContents (UnityEngine.Rect inRect) { Settings.DoSettingsWindowContents (inRect); }

		#endregion

		#region HelpTree Support

		private static MainButtonDef modHelp;
		private static System.Reflection.MethodInfo helpWindow_JumpTo;
		private static bool helpTreeLoaded;

		private void InitializeHelpSuport ()
		{
			var type = GenTypes.GetTypeInAnyAssembly ("HelpTab.IHelpDefView");
			if (type != null) {
				modHelp = DefDatabase<MainButtonDef>.GetNamed ("ModHelp", false);
				helpWindow_JumpTo = type.GetMethod ("JumpTo", new Type [] { typeof (Def) });

				helpTreeLoaded = true;
			}
		}

		public static void JumpToHelp (Def def)
		{
			if (helpTreeLoaded) {
				helpWindow_JumpTo.Invoke (modHelp.TabWindow, new object [] { def });
			}
		}

		public static bool HasHelpTreeLoaded {
			get {
				return helpTreeLoaded;
			}
		}

		#endregion
	}
}
