// ResearchTree.cs
// Copyright Karel Kroeze, 2018-2018

using System;
using System.Reflection;
using Harmony;
using RimWorld;
using Verse;

namespace FluffyResearchTree
{
    public class ResearchTree : Mod
    {
        public ResearchTree( ModContentPack content ) : base( content )
        {
            HarmonyInstance.Create( "Fluffy.ResearchTree" ).PatchAll(Assembly.GetExecutingAssembly());

            LongEventHandler.QueueLongEvent(Tree.Initialize, "Fluffy.ResearchTree.BuildingResearchTree", false, null);
            LongEventHandler.ExecuteWhenFinished(InitializeHelpSuport);
        }

        #region Overrides of Mod

        public override string SettingsCategory() { return "Fluffy.ResearchTree".Translate(); }
        public override void DoSettingsWindowContents(UnityEngine.Rect inRect) { Settings.DoSettingsWindowContents(inRect); }

        #endregion

        #region HelpTree Support

        static MainButtonDef modHelp;
        static MethodInfo helpWindow_JumpTo;
        static bool helpTreeLoaded;

        void InitializeHelpSuport()
        {
            var type = GenTypes.GetTypeInAnyAssembly("HelpTab.IHelpDefView");
            if (type != null)
            {
                modHelp = DefDatabase<MainButtonDef>.GetNamed("ModHelp", false);
                helpWindow_JumpTo = type.GetMethod("JumpTo", new Type[] { typeof(Def) });

                helpTreeLoaded = true;
            }
        }

        public static void JumpToHelp(Def def)
        {
            if (helpTreeLoaded)
            {
                helpWindow_JumpTo.Invoke(modHelp.TabWindow, new object[] { def });
            }
        }

        public static bool HasHelpTreeLoaded
        {
            get
            {
                return helpTreeLoaded;
            }
        }

        #endregion
    }
}