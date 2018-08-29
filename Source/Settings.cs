using System;

using UnityEngine;
using Verse;

namespace FluffyResearchTree
{
    public class Settings : ModSettings
    {
        #region tuning parameters

        public static bool showNotification = true;
        public static bool shouldPause = false;
        public static bool shouldReset = false;
        public static bool debugResearch = false;

        #endregion tuning parameters

        #region Strings
        public static readonly string ShowNotificationPopup = "Fluffy.ResearchTree.ShowNotificationPopup".Translate();
        public static readonly string ShowNotificationPopupTip = "Fluffy.ResearchTree.ShowNotificationPopupTip".Translate();
        public static readonly string ShouldPauseOnOpen = "Fluffy.ResearchTree.ShouldPauseOnOpen".Translate();
        public static readonly string ShouldPauseOnOpenTip = "Fluffy.ResearchTree.ShouldPauseOnOpenTip".Translate();
        public static readonly string DebugResearch = "Fluffy.ResearchTree.DebugResearch".Translate();
        public static readonly string DebugResearchTip = "Fluffy.ResearchTree.DebugResearchTip".Translate();

        public static readonly string ShouldResetOnOpen = "Fluffy.ResearchTree.ShouldResetOnOpen".Translate();
        public static readonly string ShouldResetOnOpenTip = "Fluffy.ResearchTree.ShouldResetOnOpenTip".Translate();
        #endregion

        public static void DoSettingsWindowContents(Rect rect)
        {
            Listing_Standard list = new Listing_Standard(GameFont.Small);
            list.ColumnWidth = rect.width;
            list.Begin(rect);

            list.CheckboxLabeled(ShowNotificationPopup, ref showNotification,
                                  ShowNotificationPopupTip);
            list.Gap();
            list.CheckboxLabeled(ShouldPauseOnOpen, ref shouldPause,
                                  ShouldPauseOnOpenTip);
            list.CheckboxLabeled(ShouldResetOnOpen, ref shouldReset,
                                  ShouldResetOnOpenTip);
            list.Gap();
            list.CheckboxLabeled(DebugResearch, ref debugResearch,
                      DebugResearchTip);
            list.End();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref showNotification, "ShowNotificationPopup", true);
            Scribe_Values.Look(ref shouldPause, "ShouldPauseOnOpen", false);
            Scribe_Values.Look(ref shouldReset, "ShouldResetOnOpen", false);
            Scribe_Values.Look(ref debugResearch, "DebugResearch", false);
        }
    }
}