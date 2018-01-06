using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using RimWorld;
using Verse;

namespace ResearchPal
{
    public class MainTabWindow_ResearchTree : MainTabWindow
    {
        internal static Vector2 _scrollPosition                     = Vector2.zero;

        public static List<Pair<Node, Node>> connections            = new List<Pair<Node, Node>>();
        public static List<Pair<Node, Node>> highlightedConnections = new List<Pair<Node, Node>>();
        public static Dictionary<Rect, List<String>> hubTips        = new Dictionary<Rect, List<string>>();
        public static List<Node> nodes                              = new List<Node>();
        public static FilterManager filterManager                   = new FilterManager();
        
        public override Vector2 RequestedTabSize {
            get {
                return new Vector2 (UI.screenWidth, UI.screenHeight);
            }
        }

        public virtual float TabButtonBarPercent
        {
            get
            {
                return 0f;
            }
        }

        public override void PreOpen ()
        {
            base.PreOpen ();

            if (Settings.shouldPause) {
                this.forcePause = Settings.shouldPause;
            }

            if (Settings.shouldReset)
            {
                filterManager.Reset();
            }            

        }

        public override void WindowOnGUI()
        {
            base.WindowOnGUI();
            filterManager.CheckPressedKey();            
        }

        public override void DoWindowContents( Rect canvas )
        {
            filterManager.DrawFilterControls(canvas);

            PrepareTreeForDrawing();
            DrawTree( canvas );

            filterManager.DrawFilterResults(canvas);
        }
               
        private void PrepareTreeForDrawing()
        {
            
            // loop through trees
            foreach ( Tree tree in ResearchTree.Trees )
            {
                PrepareNodes(tree.Trunk.Concat(tree.Leaves));
            }

            // add orphans
            PrepareNodes(ResearchTree.Orphans.Leaves);
        }

        private void PrepareNodes(IEnumerable<Node> nodeList)
        {
            foreach (Node node in nodeList)
            {
                nodes.Add(node);
                filterManager.NodeIsMatch(node);

                foreach (Node parent in node.Parents)
                {
                    connections.Add(new Pair<Node, Node>(node, parent));
                }
            }
        }

        public void DrawTree( Rect canvas )
        {
            // get total size of Research Tree
            int maxDepth = 0, totalWidth = 0;

            if ( ResearchTree.Trees.Any() )
            {
                maxDepth = ResearchTree.Trees.Max( tree => tree.MaxDepth );
                totalWidth = ResearchTree.Trees.Sum( tree => tree.Width );
            }

            maxDepth = Math.Max( maxDepth, ResearchTree.Orphans.MaxDepth );
            totalWidth += ResearchTree.Orphans.Width;
                   
            float width = ( maxDepth + 1 ) * ( Settings.NodeSize.x + Settings.NodeMargins.x ); // zero based
            float height = ( totalWidth - 1 ) * (Settings.NodeSize.y + Settings.NodeMargins.y );

            // main view rect
            Rect view = new Rect( 0f, 0f, width, height );
            
            // create the scroll area below the search box (plus a small margin) so it stays on top
            Widgets.BeginScrollView(new Rect(canvas.x,
                                    canvas.y + filterManager.Height + Settings.NodeMargins.y,
                                    canvas.width,canvas.height - filterManager.Height - Settings.NodeMargins.y),
                                    ref _scrollPosition, view );
            GUI.BeginGroup( view );            
            Text.Anchor = TextAnchor.MiddleCenter;

            // draw regular connections, not done first to better highlight done.
            foreach ( Pair<Node, Node> connection in connections.Where( pair => !pair.Second.Research.IsFinished ) )
            {                
                ResearchTree.DrawLine( connection, connection.First.AdjustFilterAlpha(connection.First.Tree.GreyedColor,0.05f) );
            }

            // draw connections from completed nodes
            if (filterManager.FilterPhrase.NullOrEmpty())
            {
                foreach (Pair<Node, Node> connection in connections.Where(pair => pair.Second.Research.IsFinished))
                {
                    ResearchTree.DrawLine(connection, connection.First.AdjustFilterAlpha(connection.First.Tree.MediumColor, 0.05f));
                }
            }            
            connections.Clear();

            // draw highlight connections on top
            foreach (Pair<Node, Node> connection in highlightedConnections)
            {
                ResearchTree.DrawLine(connection, GenUI.MouseoverColor, true);
            }
            highlightedConnections.Clear();

            // draw nodes on top of lines
            bool reqScroll = true;
            Node scrollToNode = null;
            foreach ( Node node in nodes )
            {
                // draw the node
                bool visible = node.Draw();

                // ensure that at least one matching node is visible, prioritize highest on the screen            
                if (filterManager.FilterDirty)
                {                    
                    if (node.FilterMatch.IsValidMatch())
                    {
                        if (!reqScroll)
                            continue;

                        // this node is a match and is currently visible, we don't need to scroll
                        if (visible)
                        {
                            reqScroll = false;
                            scrollToNode = null;
                        } else {
                            // this node is a match, but isn't visible. if it's the highest node then we'll scroll to it
                            if (scrollToNode == null || node.Pos.z < scrollToNode.Pos.z)
                            {
                                scrollToNode = node;
                            }
                        }
                    }
                }
            }
            
            if (filterManager.FilterDirty)
            {
                // scroll to a matching node if necessary
                if (scrollToNode != null)
                {
                    // scale the focus area to ensure it all fits on the screen
                    Rect r = scrollToNode.Rect.ScaledBy(2.0f);
                    _scrollPosition = new Vector2(r.xMin, r.yMin);
                } else if (filterManager.FilterPhrase == "")
                {
                    _scrollPosition = Vector2.zero;
                }
            }            
            nodes.Clear();

            // register hub tooltips
            foreach ( KeyValuePair<Rect, List<string>> pair in hubTips )
            {
                string text = string.Join( "\n", pair.Value.ToArray() );
                TooltipHandler.TipRegion( pair.Key, text );
            }
            hubTips.Clear();

            // draw Queue labels
            Queue.DrawLabels();

            // reset anchor
            Text.Anchor = TextAnchor.UpperLeft;

            GUI.EndGroup();
            Widgets.EndScrollView();
        }
    }
}