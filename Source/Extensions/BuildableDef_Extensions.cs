using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Verse;

namespace ResearchPal
{

    [StaticConstructorOnStartup]
    public static class BuildableDef_Extensions
    {
        #region Lists of affected data


        public static List<Def> GetResearchRequirements(this BuildableDef buildableDef)
        {
            var researchDefs = new List<Def>();

            if (buildableDef.researchPrerequisites != null)
            {
                researchDefs.AddRangeUnique (buildableDef.researchPrerequisites.ConvertAll<Def> (def => (Def)def));
            }

            // Return the list of research required
            return researchDefs;
        }

        #endregion

        /*
        #region Comp Properties

        public static ResearchEngine.CompProperties_RestrictedPlacement RestrictedPlacement_Properties(this BuildableDef buildableDef)
        {
            if (buildableDef is TerrainWithComps)
            {
                // Terrain with comps
                return ((TerrainWithComps)buildableDef).GetCompProperties(typeof(CompRestrictedPlacement)) as CompProperties_RestrictedPlacement;
            }
            else if (buildableDef is ThingDef)
            {
                // Thing with comps
                return ((ThingDef)buildableDef).GetCompProperties<CompProperties_RestrictedPlacement>();
            }

            // Something else
            return (CompProperties_RestrictedPlacement)null;
        }

        #endregion
    */

    }

}
