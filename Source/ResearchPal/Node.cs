using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using RimWorld;
using Verse;

namespace ResearchPal
{
    [StaticConstructorOnStartup]
    public class Node
    {
        #region Fields

        public static Texture2D ResearchIcon;
        public static Texture2D WarningIcon;
        public List<Node> Children = new List<Node>();
        public int Depth;
        public string Genus;
        public string Family;
        public List<Node> Parents = new List<Node>();
        public IntVec2 Pos;
        public ResearchProjectDef Research;
        public Tree Tree;
        private const float LabSize = 30f;
        private const float Offset = 2f;

        private bool _largeLabel = false;
        private Vector2 _left = Vector2.zero;

        private Rect _queueRect,
                                      _rect,
                                      _labelRect,
                                      _costLabelRect,
                                      _costIconRect,
                                      _iconsRect;

        private bool _rectSet;
        private Vector2 _right = Vector2.zero;
        private FilterManager.FilterMatchType _matchType = FilterManager.FilterMatchType.NONE;

        #endregion Fields

        #region Constructors

        static Node()
        {
            ResearchIcon = ContentFinder<Texture2D>.Get("UI/Research/Icon");
            WarningIcon = ContentFinder<Texture2D>.Get("UI/Research/Warning");
        }


        public Node(ResearchProjectDef research)
        {
            Research = research;

            // get the Genus, this is the research family name, and will be used to group research together.
            // First see if we have a ":" in the name
            List<string> parts = research.LabelCap.Split (":".ToCharArray ()).ToList ();
            if (parts.Count > 1) {
                Genus = parts.First ();
            } else // otherwise, strip the last word (intended to catch 1,2,3/ I,II,III,IV suffixes)
              {
                parts = research.LabelCap.Split (" ".ToCharArray ()).ToList ();
                parts.Remove (parts.Last ());
                Genus = string.Join (" ", parts.ToArray ());
            }

            Parents = new List<Node>();
            Children = new List<Node>();
        }

        #endregion Constructors

        #region Properties

        public Rect CostIconRect
        {
            get
            {
                if (!_rectSet)
                {
                    CreateRects();
                }
                return _costIconRect;
            }
        }

        public Rect CostLabelRect
        {
            get
            {
                if (!_rectSet)
                {
                    CreateRects();
                }
                return _costLabelRect;
            }
        }

        public Rect IconsRect
        {
            get
            {
                if (!_rectSet)
                {
                    CreateRects();
                }
                return _iconsRect;
            }
        }

        public Rect LabelRect
        {
            get
            {
                if (!_rectSet)
                {
                    CreateRects();
                }
                return _labelRect;
            }
        }

        /// <summary>
        /// Middle of left node edge
        /// </summary>
        public Vector2 Left
        {
            get
            {
                if (_left == Vector2.zero)
                {
                    _left = new Vector2(Pos.x * (Settings.NodeSize.x + Settings.NodeMargins.x) + Offset,
                                         (Pos.z-1) * (Settings.NodeSize.y + Settings.NodeMargins.y) + Offset + Settings.NodeSize.y / 2);
                }
                return _left;
            }
        }

        /// <summary>
        /// Tag UI Rect
        /// </summary>
        public Rect QueueRect
        {
            get
            {
                if (!_rectSet)
                {
                    CreateRects();
                }
                return _queueRect;
            }
        }

        /// <summary>
        /// Static UI rect for this node
        /// </summary>
        public Rect Rect
        {
            get
            {
                if (!_rectSet)
                {
                    CreateRects();
                }
                return _rect;
            }
        }

        /// <summary>
        /// Middle of right node edge
        /// </summary>
        public Vector2 Right
        {
            get
            {
                if (_right == Vector2.zero)
                {
                    _right = new Vector2(Pos.x * (Settings.NodeSize.x + Settings.NodeMargins.x) + Offset + Settings.NodeSize.x,
                                          (Pos.z-1) * (Settings.NodeSize.y + Settings.NodeMargins.y) + Offset + Settings.NodeSize.y / 2);
                }
                return _right;
            }
        }

        /// <summary>
        /// Type of match to the current filter
        /// </summary>
        public FilterManager.FilterMatchType FilterMatch
        {
            get
            {
                return _matchType;
            }
            set
            {
                _matchType = value;
            }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Determine the closest tree by moving along parents and then children until a tree has been found. Returns first tree encountered, or NULL.
        /// </summary>
        /// <returns></returns>
        public Tree ClosestTree()
        {
            // go up through all Parents until we find a parent that is in a Tree
            Queue<Node> parents = new Queue<Node>();
            parents.Enqueue(this);

            while (parents.Count > 0)
            {
                Node current = parents.Dequeue();
                if (current.Tree != null)
                {
                    return current.Tree;
                }

                // otherwise queue up the Parents to be checked
                foreach (Node parent in current.Parents)
                {
                    parents.Enqueue(parent);
                }
            }

            // if that didn't work, try seeing if a child is in a Tree (unlikely, but whateva).
            Queue<Node> children = new Queue<Node>();
            children.Enqueue(this);

            while (children.Count > 0)
            {
                Node current = children.Dequeue();
                if (current.Tree != null)
                {
                    return current.Tree;
                }

                // otherwise queue up the Children to be checked.
                foreach (Node child in current.Children)
                {
                    children.Enqueue(child);
                }
            }

            // finally, if nothing stuck, return null
            return null;
        }

        /// <summary>
        /// Set all prerequisites as parents of this node, and for each parent set this node as a child.
        /// </summary>
        public void CreateLinks()
        {
            // 'vanilla' prerequisites
            if (!Research.prerequisites.NullOrEmpty())
            {
                foreach (ResearchProjectDef prerequisite in Research.prerequisites)
                {
                    if (prerequisite != Research)
                    {
                        var parent = ResearchTree.Forest.FirstOrDefault(node => node.Research == prerequisite);
                        if (parent != null)
                            Parents.Add(parent);
                    }
                }
            }

            foreach (Node parent in Parents)
            {
                parent.Children.Add(this);
            }
        }

        /// <summary>
        /// Prints debug information.
        /// </summary>
        public void Debug()
        {
            StringBuilder text = new StringBuilder();
            text.AppendLine(Research.LabelCap + " (" + Depth + ", " + Genus + ", " + Family + "):");
            text.AppendLine("- Parents");
            foreach (Node parent in Parents)
            {
                text.AppendLine("-- " + parent.Research.LabelCap);
            }
            text.AppendLine("- Children");
            foreach (Node child in Children)
            {
                text.AppendLine("-- " + child.Research.LabelCap);
            }
            text.AppendLine("");
            Log.Message(text.ToString());
        }

        /// <summary>
        /// Adjusts the alpha channel of the color passed in for filtering
        /// </summary>
        /// <param name="col"></param>
        /// <param name="alpha"></param>
        /// <returns></returns>
        private Color AdjustFilterAlpha(Color col)
        {
            return ColorHelper.AdjustAlpha(col, NodeAlpha());
        }

        /// <summary>
        /// Returns an alpha level based on whether or not the node is highlighted in the current filter
        /// </summary>
        /// <returns></returns>
        private float NodeAlpha()
        {
            return (_matchType == FilterManager.FilterMatchType.NO_MATCH ? Settings.FilterNonMatchAlpha : 1.0f);
        }

        /// <summary>
        /// Draw the node, including interactions.
        /// </summary>
        public bool Draw()
        {
            // cop out if off-screen
            Rect screen = new Rect(MainTabWindow_ResearchTree._scrollPosition.x, MainTabWindow_ResearchTree._scrollPosition.y, Screen.width, Screen.height - 35);
            if (Rect.xMin > screen.xMax ||
                Rect.xMax < screen.xMin ||
                Rect.yMin > screen.yMax ||
                Rect.yMax < screen.yMin)
            {
                return false;
            }

            // set color
            GUI.color = !Research.PrerequisitesCompleted ? AdjustFilterAlpha(Tree.GreyedColor) : AdjustFilterAlpha(Tree.MediumColor);

            // mouseover highlights
            if (Mouse.IsOver(Rect))
            {
                // active button
                GUI.DrawTexture(Rect, ResearchTree.ButtonActive);

                // highlight this and all prerequisites if research not completed
                if (!Research.IsFinished)
                {
                    HighlightWithPrereqs();
                }
                else // highlight followups
                {
                    foreach (Node child in Children)
                    {
                        MainTabWindow_ResearchTree.highlightedConnections.Add(new Pair<Node, Node>(this, child));
                        child.Highlight(GenUI.MouseoverColor, false, false);
                    }
                }
            }
            // filter highlights
            else if (_matchType.IsValidMatch())
            {
                GUI.DrawTexture(Rect, ResearchTree.ButtonActive);
                if (Settings.showFilteredLinks)
                {
                    HighlightWithPrereqs();
                } else {
                    Highlight(GenUI.MouseoverColor, false, false);
                }
            }
            // if not moused over, just draw the default button state
            else
            {
                GUI.DrawTexture(Rect, ResearchTree.Button);
            }

            // grey out center to create a progress bar effect, completely greying out research not started.
            bool warnLocked = false, warnPenalty = false;
            if (!Research.IsFinished) {
                Rect progressBarRect = Rect.ContractedBy (2f);
                GUI.color = AdjustFilterAlpha(Tree.GreyedColor);
                progressBarRect.xMin += Research.ProgressPercent * progressBarRect.width;
                GUI.DrawTexture (progressBarRect, BaseContent.WhiteTex);

                warnLocked = IsLocked (Research);
                warnPenalty = Research.techLevel > Faction.OfPlayer.def.techLevel;
            }

            // draw the research label
            GUI.color = AdjustFilterAlpha(Color.white);
            Text.Anchor = TextAnchor.UpperLeft;
            Text.WordWrap = true;
            Text.Font = _largeLabel ? GameFont.Tiny : GameFont.Small;
            if (Settings.debugResearch && Prefs.DevMode)
            {
                Widgets.Label(LabelRect, Research.LabelCap + " (" + Depth + ", " + Genus + ", " + Family + "):");
            }
            else
            {
                Widgets.Label(LabelRect, Research.LabelCap);
            }

            // draw research cost and icon
            Text.Anchor = TextAnchor.UpperRight;
            Text.Font = GameFont.Small;
            if (warnPenalty) {
                GUI.color = AdjustFilterAlpha(Color.yellow);
            }
            Widgets.Label (CostLabelRect, Research.CostApparent.ToStringByStyle (ToStringStyle.Integer));

            GUI.color = AdjustFilterAlpha(Color.white);
            if (warnLocked) {
                GUI.DrawTexture (CostIconRect, WarningIcon);
            } else {
                GUI.DrawTexture (CostIconRect, ResearchIcon);
            }

            Text.WordWrap = true;

            // attach description and further info to a tooltip
            TooltipHandler.TipRegion(Rect, GetResearchTooltipString()); // new TipSignal( GetResearchTooltipString(), Settings.TipID ) );

            // draw unlock icons
            List<Pair<Def, string>> unlocks = Research.GetUnlockDefsAndDescs();
            for (int i = 0; i < unlocks.Count; i++)
            {
                Rect iconRect = new Rect(IconsRect.xMax - (i + 1) * (Settings.IconSize.x + 4f),
                                          IconsRect.yMin + (IconsRect.height - Settings.IconSize.y) / 2f,
                                          Settings.IconSize.x,
                                          Settings.IconSize.y);

                if (iconRect.xMin - Settings.IconSize.x < IconsRect.xMin &&
                    i + 1 < unlocks.Count)
                {
                    // stop the loop if we're about to overflow and have 2 or more unlocks yet to print.
                    iconRect.x = IconsRect.x + 4f;
                    ResearchTree.MoreIcon.DrawFittedIn(iconRect, NodeAlpha());
                    string tip = string.Join("\n", unlocks.GetRange(i, unlocks.Count - i).Select(p => p.Second).ToArray());
                    TooltipHandler.TipRegion(iconRect, tip); // new TipSignal( tip, Settings.TipID, TooltipPriority.Pawn ) );
                    break;
                }

                // draw icon
                unlocks[i].First.DrawColouredIcon(iconRect, NodeAlpha());

                // tooltip
                TooltipHandler.TipRegion(iconRect, unlocks[i].Second); // new TipSignal( unlocks[i].Second, Settings.TipID, TooltipPriority.Pawn ) );

                // reset the color
                GUI.color = Color.white;
            }

            // if clicked and not yet finished, queue up this research and all prereqs.
            if (Widgets.ButtonInvisible(Rect))
            {
                // LMB is queue operations, RMB is info
                if (Event.current.button == 0 && !Research.IsFinished)
                {
                    if (Settings.debugResearch && Prefs.DevMode && Event.current.control)
                    {
                        List<Node> nodesToResearch = GetMissingRequiredRecursive().Concat(new List<Node>(new[] { this })).ToList();
                        foreach (Node n in nodesToResearch)
                        {
                            if (Queue.IsQueued(n))
                                Queue.Dequeue(n);

                            if (!n.Research.IsFinished)
                                Find.ResearchManager.InstantFinish(n.Research, false);
                        }

                        Verse.Sound.SoundStarter.PlayOneShotOnCamera (MessageTypeDefOf.PositiveEvent.sound);
                    } else {

                        if (!Queue.IsQueued (this))
                        {
                            if (warnLocked) {
                                Messages.Message (ResourceBank.String.RequireMissing, MessageTypeDefOf.RejectInput);
                            }

                            // if shift is held, add to queue, otherwise replace queue
                            Queue.EnqueueRange (GetMissingRequiredRecursive ().Concat (new List<Node> (new [] { this })), Event.current.shift);
                        } else {
                            Queue.Dequeue(this);
                        }
                    }

                } else if (Event.current.button == 1) {
                    ResearchPalMod.JumpToHelp (Research);
                }
            }
            return true;
        }

        private void HighlightWithPrereqs()
        {
            List<Node> prereqs = GetMissingRequiredRecursive();
            Highlight(GenUI.MouseoverColor, true, false);
            foreach (Node prerequisite in prereqs)
            {
                prerequisite.Highlight(GenUI.MouseoverColor, true, false);
            }
        }

        private bool IsLocked (ResearchProjectDef research)
        {
            if (research.requiredResearchBuilding != null || !research.requiredResearchFacilities.NullOrEmpty ())
            {
                if (ResearchPalMod.allResearchBenches.NullOrEmpty ())
                {
                    return true;
                }

                foreach (var bench in ResearchPalMod.allResearchBenches) {
                    if (research.CanBeResearchedAt (bench, false))
                    {
                        return false;
                    }
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Get recursive list of all incomplete prerequisites
        /// </summary>
        /// <returns>List<Node> prerequisites</Node></returns>
        public List<Node> GetMissingRequiredRecursive()
        {
            List<Node> parents = new List<Node>(Parents.Where(node => !node.Research.IsFinished));
            List<Node> allParents = new List<Node>(parents);
            foreach (Node current in parents)
            {
                allParents.AddRange (current.GetMissingRequiredRecursive ());
            }
            return allParents.Distinct().ToList();
        }

        /// <summary>
        /// Draw highlights around node, and optionally highlight links to parents/children of this node.
        /// </summary>
        /// <param name="color">color to use</param>
        /// <param name="linkParents">should links to parents be drawn?</param>
        /// <param name="linkChildren">should links to children be drawn?</param>
        public void Highlight(Color color, bool linkParents, bool linkChildren)
        {
            GUI.color = color;
            Widgets.DrawBox(Rect.ContractedBy(-2f), 2);
            GUI.color = Color.white;
            if (linkParents)
            {
                foreach (Node parent in Parents)
                {
                    MainTabWindow_ResearchTree.highlightedConnections.Add(new Pair<Node, Node>(parent, this));
                }
            }
            if (linkChildren)
            {
                foreach (Node child in Children)
                {
                    MainTabWindow_ResearchTree.highlightedConnections.Add(new Pair<Node, Node>(this, child));
                }
            }
        }

        /// <summary>
        /// Recursively determine the depth of this node.
        /// </summary>
        public void SetDepth()
        {
            List<Node> level = new List<Node>();
            level.Add(this);
            while (level.Count > 0 &&
                    level.Any(node => node.Parents.Count > 0))
            {
                // has any parent, increment level.
                Depth++;

                // set level to next batch of distinct Parents, where Parents may not be itself.
                level = level.SelectMany(node => node.Parents).Distinct().Where(node => node != this).ToList();

                // stop infinite recursion with loops of size greater than 2
                if (Depth > 100)
                {
                    Log.Error(Research.LabelCap +
                               " has more than 100 levels of prerequisites. Is the Research Tree defined as a loop?");
                }
            }
        }

        public override string ToString()
        {
            return this.Research.LabelCap + this.Pos;
        }

        private void CreateRects()
        {
            // main rect
            _rect = new Rect(Pos.x * (Settings.NodeSize.x + Settings.NodeMargins.x) + Offset,
                              (Pos.z-1) * (Settings.NodeSize.y + Settings.NodeMargins.y) + Offset,
                              Settings.NodeSize.x,
                              Settings.NodeSize.y);

            // queue rect
            _queueRect = new Rect(_rect.xMax - LabSize / 2f,
                                 _rect.yMin + (_rect.height - LabSize) / 2f,
                                 LabSize,
                                 LabSize);

            // label rect
            _labelRect = new Rect(_rect.xMin + 6f,
                                   _rect.yMin + 3f,
                                   _rect.width * 2f / 3f - 6f,
                                   _rect.height * .5f + 11f);

            // research cost rect
            _costLabelRect = new Rect(_rect.xMin + _rect.width * 2f / 3f,
                                  _rect.yMin + 3f,
                                  _rect.width * 1f / 3f - 16f - 3f,
                                  _rect.height * .5f - 3f);

            // research icon rect
            _costIconRect = new Rect(_costLabelRect.xMax,
                                      _rect.yMin + (_costLabelRect.height - 16f) / 2,
                                      16f,
                                      16f);

            // icon container rect
            _iconsRect = new Rect(_rect.xMin,
                                   _rect.yMin + _rect.height * .5f,
                                   _rect.width,
                                   _rect.height * .5f);

            // see if the label is too big
            _largeLabel = Text.CalcHeight(Research.LabelCap, _labelRect.width) > _labelRect.height;

            // done
            _rectSet = true;
        }

        /// <summary>
        /// Creates text version of research description and additional unlocks/prereqs/etc sections.
        /// </summary>
        /// <returns>string description</returns>
        private string GetResearchTooltipString()
        {
            // start with the description
            var text = new StringBuilder();
            text.AppendLine(Research.description);
            text.AppendLine();

            text.AppendLine(StringExtensions.TitleCase(Research.techLevel.ToStringHuman()));
            text.AppendLine();

            var PlayerTechLevel = Faction.OfPlayer.def.techLevel;
            if (Research.techLevel > PlayerTechLevel) {
                text.AppendLine (ResourceBank.String.ResearchLevels(Research.techLevel, PlayerTechLevel) + " " +
                             ResourceBank.String.ResearchPenalty (Research.CostFactor (PlayerTechLevel)));
                text.AppendLine ();
            }

            if (Research.requiredResearchBuilding != null)
            {
                text.AppendLine(ResourceBank.String.RequireBenchLabel + " " + Research.requiredResearchBuilding.label);
            }
            if (Research.requiredResearchFacilities != null)
            {
                foreach (ThingDef rrf in Research.requiredResearchFacilities)
                {
                    text.AppendLine(ResourceBank.String.RequireFacilityLabel + " " + rrf.label);
                }
            }

            text.AppendLine();

            if (Queue.IsQueued(this))
            {
                text.AppendLine(ResourceBank.String.LClickRemoveFromQueue);
            }
            else
            {
                text.AppendLine(ResourceBank.String.LClickReplaceQueue);
                text.AppendLine(ResourceBank.String.SLClickAddToQueue);
            }

            if (Settings.debugResearch && Prefs.DevMode)
            {
                text.AppendLine(ResourceBank.String.CLClickDebugInstant);
            }

            //To Help System
            if (ResearchPalMod.HasHelpTreeLoaded) {
                text.AppendLine (ResourceBank.String.RClickForDetails);
            }

            if (Prefs.DevMode)
            {
                text.AppendLine();
                text.Append("Position: " + this.Pos.ToString());
                text.Append("Rect: " + this.Rect.ToString());
            }

            return text.ToString();
        }

        #endregion Methods
    }
}