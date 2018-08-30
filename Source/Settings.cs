using UnityEngine;
using Verse;
using static FluffyResearchTree.ResourceBank.String;

namespace FluffyResearchTree
{
  public class Settings : ModSettings
    {
        #region tuning parameters

        public static bool showNotification;
        public static bool shouldPause;
        public static bool shouldReset;
        public static bool shouldSeparateByTechLevels;
        public static bool debugResearch;

        #endregion tuning parameters

        public static void DoSettingsWindowContents(Rect rect)
        {
            Listing_Standard list = new Listing_Standard(GameFont.Small);
            list.ColumnWidth = rect.width;
            list.Begin(rect);

            list.CheckboxLabeled(ShowNotificationPopup, ref showNotification,
                                  ShowNotificationPopupTip);
            list.CheckboxLabeled(ShouldSeparateByTechLevels, ref shouldSeparateByTechLevels,
                                 ShouldSeparateByTechLevelsTip);
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
            Scribe_Values.Look(ref showNotification, "ShowNotificationPopup", true);
            Scribe_Values.Look(ref shouldSeparateByTechLevels, "ShouldSeparateByTechLevels", false);
            Scribe_Values.Look(ref shouldPause, "ShouldPauseOnOpen", true);
            Scribe_Values.Look(ref shouldReset, "ShouldResetOnOpen", false);
            Scribe_Values.Look(ref debugResearch, "DebugResearch", false);
        }
    }
}