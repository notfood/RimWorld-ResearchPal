using System;
using System.Collections.Generic;
using System.Linq;

using RimWorld;
using Verse;

namespace ResearchPal
{
	[StaticConstructorOnStartup]
	public static class ResearchProjectDef_Extensions
	{
		#region Static Data

		private static Dictionary<ushort, List<Pair<Def, string>>> _unlocksCache;

		static ResearchProjectDef_Extensions ()
		{
_unlocksCache = new Dictionary<ushort, List<Pair<Def, string>>> ();
		}

		#endregion Static Data

		#region Lists of affected data

		public static List<ResearchProjectDef> ExclusiveDescendants (this ResearchProjectDef research)
		{
			List<ResearchProjectDef> descendants = new List<ResearchProjectDef> ();

			// recursively go through all children
			// populate initial queue
			Queue<ResearchProjectDef> queue = new Queue<ResearchProjectDef> (DefDatabase<ResearchProjectDef>.AllDefsListForReading.Where (res => res.prerequisites.Contains (research)));

			// for each item in queue, determine if there's something unlocking it
			// if not, add to the list, and queue up children.
			while (queue.Count > 0) {
				ResearchProjectDef current = queue.Dequeue ();

				if (!descendants.Contains (current)) {
					descendants.Add (current);
					foreach (ResearchProjectDef descendant in DefDatabase<ResearchProjectDef>.AllDefsListForReading.Where (res => res.prerequisites.Contains (current))) {
						queue.Enqueue (descendant);
					}
				}
			}

			return descendants;
		}

		public static List<ResearchProjectDef> GetPrerequisitesRecursive (this ResearchProjectDef research)
		{
			List<ResearchProjectDef> result = new List<ResearchProjectDef> ();
			if (research.prerequisites.NullOrEmpty ()) {
				return result;
			}
			Stack<ResearchProjectDef> stack = new Stack<ResearchProjectDef> (research.prerequisites.Where (parent => parent != research));

			while (stack.Count > 0) {
				var parent = stack.Pop ();
				result.Add (parent);

				if (!parent.prerequisites.NullOrEmpty ()) {
					foreach (var grandparent in parent.prerequisites) {
						if (grandparent != parent)
							stack.Push (grandparent);
					}
				}
			}

			return result.Distinct ().ToList ();
		}

		public static List<Pair<Def, string>> GetUnlockDefsAndDescs (this ResearchProjectDef research)
		{
			if (_unlocksCache.ContainsKey (research.shortHash)) {
				return _unlocksCache [research.shortHash];
			}

			List<Pair<Def, string>> unlocks = new List<Pair<Def, string>> ();

			// dumps recipes/plants unlocked, because of the peculiar way CCL helpdefs are done.
			List<ThingDef> dump = new List<ThingDef> ();

			unlocks.AddRange (research.GetThingsUnlocked ()
									  .Where (d => d.IconTexture () != null)
									  .Select (d => new Pair<Def, string> (d, "AllowsBuildingX".Translate (d.LabelCap))));
			unlocks.AddRange (research.GetTerrainUnlocked ()
									  .Where (d => d.IconTexture () != null)
									  .Select (d => new Pair<Def, string> (d, "AllowsBuildingX".Translate (d.LabelCap))));
			unlocks.AddRange (research.GetRecipesUnlocked (ref dump)
									  .Where (d => d.IconTexture () != null)
									  .Select (d => new Pair<Def, string> (d, "AllowsCraftingX".Translate (d.LabelCap))));
			string sowTags = string.Join (" and ", research.GetSowTagsUnlocked (ref dump).ToArray ());
			unlocks.AddRange (dump.Where (d => d.IconTexture () != null)
								  .Select (d => new Pair<Def, string> (d, "AllowsSowingXinY".Translate (d.LabelCap, sowTags))));

			_unlocksCache.Add (research.shortHash, unlocks);
			return unlocks;
		}

		public static List<ThingDef> GetThingsUnlocked (this ResearchProjectDef researchProjectDef)
		{
			// Buildings it unlocks
			var thingsOn = new List<ThingDef> ();
			var researchThings = DefDatabase<ThingDef>.AllDefsListForReading.Where (t => {
				var r = t.GetResearchRequirements ();
				return r != null && r.Contains (researchProjectDef);
			}).ToList ();

			if (!researchThings.NullOrEmpty ()) {
				thingsOn.AddRangeUnique (researchThings);
			}
			return thingsOn;
		}

		public static List<TerrainDef> GetTerrainUnlocked (this ResearchProjectDef researchProjectDef)
		{
			// Buildings it unlocks
			var thingsOn = new List<TerrainDef> ();
			var researchThings = DefDatabase<TerrainDef>.AllDefsListForReading.Where (t => {
				var r = t.GetResearchRequirements ();
				return r != null && r.Contains (researchProjectDef);
			}).ToList ();

			if (!researchThings.NullOrEmpty ()) {
				thingsOn.AddRangeUnique (researchThings);
			}

			return thingsOn;
		}

		public static List<RecipeDef> GetRecipesUnlocked (this ResearchProjectDef researchProjectDef, ref List<ThingDef> thingDefs)
		{
			// Recipes on buildings it unlocks
			var recipes = new List<RecipeDef> ();
			if (thingDefs != null) {
				thingDefs.Clear ();
			}

			// Add all recipes using this research projects
			var researchRecipes = DefDatabase<RecipeDef>.AllDefsListForReading.Where (d => (
				(d.researchPrerequisite == researchProjectDef)
			)).ToList ();

			if (!researchRecipes.NullOrEmpty ()) {
				recipes.AddRangeUnique (researchRecipes);
			}

			if (thingDefs != null) {
				// Add buildings for those recipes
				foreach (var r in recipes) {
					if (!r.recipeUsers.NullOrEmpty ()) {
						thingDefs.AddRangeUnique (r.recipeUsers);
					}
					var recipeThings = DefDatabase<ThingDef>.AllDefsListForReading.Where (d => (
						(!d.recipes.NullOrEmpty ()) &&
						(d.recipes.Contains (r))
					)).ToList ();
					if (!recipeThings.NullOrEmpty ()) {
						thingDefs.AddRangeUnique (recipeThings);
					}
				}
			}

			return recipes;
		}

		public static List<string> GetSowTagsUnlocked (this ResearchProjectDef researchProjectDef, ref List<ThingDef> thingDefs)
		{
			var sowTags = new List<string> ();
			if (thingDefs != null) {
				thingDefs.Clear ();
			}

			// Add all plants using this research project
			var researchPlants = DefDatabase<ThingDef>.AllDefsListForReading.Where (d => (
				(d.plant != null) &&
				(!d.plant.sowResearchPrerequisites.NullOrEmpty ()) &&
				(d.plant.sowResearchPrerequisites.Contains (researchProjectDef))
			)).ToList ();

			if (!researchPlants.NullOrEmpty ()) {
				foreach (var plant in researchPlants) {
					sowTags.AddRangeUnique (plant.plant.sowTags);
				}
				if (thingDefs != null) {
					thingDefs.AddRangeUnique (researchPlants);
				}
			}

			return sowTags;
		}

		public static Node Node (this ResearchProjectDef research)
		{
			return ResearchTree.Forest.FirstOrDefault (node => node.Research == research);
		}

		#endregion Lists of affected data
	}
}