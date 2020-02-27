using Verse;

namespace ResearchPal
{
  public static class ResourceBank
  {
        public static class String
        {
            const string PREFIX = "ResearchPal.";

            static string TL(string s) => (PREFIX + s).Translate();
            static string TL(string s, params object[] args) => (PREFIX + s).Translate(args);

            #region Settings
            public static readonly string ShowNotificationPopup = TL("ShowNotificationPopup");
            public static readonly string ShowNotificationPopupTip = TL("ShowNotificationPopupTip");

            public static readonly string ShouldSeparateByTechLevels = TL("ShouldSeparateByTechLevels");
            public static readonly string ShouldSeparateByTechLevelsTip = TL("ShouldSeparateByTechLevelsTip");

            public static readonly string ShouldPauseOnOpen = TL("ShouldPauseOnOpen");
            public static readonly string ShouldPauseOnOpenTip = TL("ShouldPauseOnOpenTip");
            public static readonly string ShouldResetOnOpen = TL("ShouldResetOnOpen");
            public static readonly string ShouldResetOnOpenTip = TL("ShouldResetOnOpenTip");

            public static readonly string DebugResearch = TL("DebugResearch");
            public static readonly string DebugResearchTip = TL("DebugResearchTip");
            #endregion

            #region ResearchProjectDef_Extensions
            public static string AllowsBuildingX(string x) => TL("AllowsBuildingX", x);
            public static string AllowsCraftingX(string x) => TL("AllowsCraftingX", x);
            public static string AllowsSowingXinY(string x, string y) => TL("AllowsSowingXinY", x, y);
            public static string AllowsPlantingX(string x) => TL("AllowsPlantingX", x);
            #endregion

            #region ResearchNode
            public static readonly string LClickReplaceQueue = TL("LClickReplaceQueue");
            public static readonly string LClickRemoveFromQueue = TL("LClickRemoveFromQueue");
            public static readonly string SLClickAddToQueue = TL("SLClickAddToQueue");
            public static readonly string CLClickDebugInstant = TL("CLClickDebugInstant");
            public static readonly string RClickForDetails = TL("RClickForDetails");

            public static string MissingFacilities(string list) => TL("MissingFacilities", list);
            public static string MissingTechprints(int techprintsApplied, int techprintCount) => TL("MissingTechprints", techprintsApplied, techprintCount);
            public static string FinishedResearch(string label) => TL("ResearchFinished", label);
            #endregion

            #region MainTabWindow_ResearchTree
            public static readonly string NeedsRestart = TL("NeedsRestart");
            public static readonly string NoResearchFound = TL("NoResearchFound");
            #endregion

            #region Queue
            public static readonly string NothingQueued = TL("NothingQueued");
            public static string NextInQueue(string label) => TL("NextInQueue", label);
            #endregion

            #region Tree
            public static readonly string PreparingTree_Setup = TL("PreparingTree.Setup");
            public static readonly string PreparingTree_CrossingReduction = TL("PreparingTree.CrossingReduction");
            public static readonly string PreparingTree_Layout = TL("PreparingTree.Layout");
            public static readonly string PreparingTree_RestoreQueue = TL("PreparingTree.RestoreQueue");
            #endregion
        }
  }
}
