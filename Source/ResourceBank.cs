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
            public static readonly string CLClickDebugInstant = "CLClickDebugInstant".Translate();

            public static readonly string LeadsTo = "LeadsTo".Translate ();
            public static readonly string RequiresThis = "RequiresThis".Translate ();

            public static readonly string ShowNotificationPopup = "ResearchPal.ShowNotificationPopup".Translate ();
            public static readonly string ShowNotificationTip = "ResearchPal.ShowNotificationTip".Translate ();
            public static readonly string ShouldPauseOnOpen = "ResearchPal.ShouldPauseOnOpen".Translate ();
            public static readonly string ShouldPauseOnOpenTip = "ResearchPal.ShouldPauseOnOpenTip".Translate ();

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
        }
            
    }
}
