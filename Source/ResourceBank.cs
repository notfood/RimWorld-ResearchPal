using System;

using Verse;

namespace ResearchPal
{
    public static class ResourceBank
    {
        public static class String {
            public static readonly string ResearchPal = "ResearchPal".Translate ();

            public static readonly string RequireMissing = "RequireMissing".Translate ();

            public static readonly string RequireBenchLabel = "RequireBenchLabel".Translate ();
            public static readonly string RequireFacilityLabel = "RequireFacilityLabel".Translate ();
            public static readonly string LClickRemoveFromQueue = "LClickRemoveFromQueue".Translate ();
            public static readonly string LClickReplaceQueue = "LClickReplaceQueue".Translate ();
            public static readonly string SLClickAddToQueue = "SLClickAddToQueue".Translate ();
            public static readonly string RClickForDetails = "RClickForDetails".Translate ();
            public static readonly string CLClickDebugInstant = "CLClickDebugInstant".Translate ();

            public static readonly string LeadsTo = "LeadsTo".Translate ();
            public static readonly string RequiresThis = "RequiresThis".Translate ();

            public static readonly string ShowNotificationPopup = "ResearchPal.ShowNotificationPopup".Translate ();
            public static readonly string ShowNotificationPopupTip = "ResearchPal.ShowNotificationPopupTip".Translate ();
            public static readonly string ShouldPauseOnOpen = "ResearchPal.ShouldPauseOnOpen".Translate ();
            public static readonly string ShouldPauseOnOpenTip = "ResearchPal.ShouldPauseOnOpenTip".Translate ();
            public static readonly string DebugResearch = "ResearchPal.DebugResearch".Translate ();
            public static readonly string DebugResearchTip = "ResearchPal.DebugResearchTip".Translate ();

            public static readonly string ShouldResetOnOpen = "ResearchPal.ShouldResetOnOpen".Translate ();
            public static readonly string ShouldResetOnOpenTip = "ResearchPal.ShouldResetOnOpenTip".Translate ();

            public static readonly string ShowFilteredLinks = "ResearchPal.ShowFilteredLinks".Translate ();
            public static readonly string ShowFilteredLinksTip = "ResearchPal.ShowFilteredLinksTip".Translate ();

            public static readonly string FilterTitleResearch = "FilterTitleResearch".Translate ();
            public static readonly string FilterTitleUnlocks = "FilterTitleUnlocks".Translate ();
            public static readonly string FilterTitleTechLevel = "FilterTitleTechLevel".Translate ();

            public static string ResearchFinished(string label)
            {
                return "ResearchFinished".Translate (label);
            }

            public static string ResearchLevels (RimWorld.TechLevel theirs, RimWorld.TechLevel yours)
            {
                return "ResearchLevels".Translate (theirs, yours);
            }

            public static string ResearchPenalty (float penalty)
            {
                return "ResearchPenalty".Translate (penalty);
            }

            public static string FilterResults (int count)
            {
                return "FilterResults".Translate(count);
            }

            public static string FilterOpacityDesc(float defaultValue, float currentValue)
            {
                return "FilterOpacityDesc".Translate(defaultValue, currentValue);
            }
        }

        public static class Float
        {

        }

    }
}
