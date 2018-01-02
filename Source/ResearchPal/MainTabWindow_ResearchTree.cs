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
        private static float filterHeight                           = 30f;

        public static List<Pair<Node, Node>> connections            = new List<Pair<Node, Node>>();
        public static List<Pair<Node, Node>> highlightedConnections = new List<Pair<Node, Node>>();
        public static Dictionary<Rect, List<String>> hubTips        = new Dictionary<Rect, List<string>>();
        public static List<Node> nodes                              = new List<Node>();
        public static string FilterPhrase                           = "";
        public static string LastFilterPhrase                       = "";

        public static bool FilterChanged
        {
            get
            {
                return (FilterPhrase != LastFilterPhrase);
            }

        }            

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

            // clear the filter each time the tree is opened
            FilterPhrase = "";
        }

        public override void DoWindowContents( Rect canvas )
        {
            PrepareTreeForDrawing();
            DrawTree( canvas );
        }

        private void PrepareTreeForDrawing()
        {
            // loop through trees
            foreach ( Tree tree in ResearchTree.Trees )
            {
                foreach ( Node node in tree.Trunk.Concat( tree.Leaves ) )
                {
                    nodes.Add( node );

                    foreach ( Node parent in node.Parents )
                    {
                        connections.Add( new Pair<Node, Node>( node, parent ) );
                    }
                }
            }

            // add orphans
            foreach ( Node node in ResearchTree.Orphans.Leaves )
            {
                nodes.Add( node );

                foreach ( Node parent in node.Parents )
                {
                    connections.Add( new Pair<Node, Node>( node, parent ) );
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

            // add an area at the top for filtering
            Rect rectFilter = new Rect(canvas.xMin, canvas.yMin, canvas.width / 6.0f, filterHeight);

            // add a button beside that for clearing the filter phrase
            Rect rectClear = new Rect(rectFilter.xMax + 3f, canvas.yMin + 3f, 24f, 24f);

            // add a text widget with the current filter phrase
            LastFilterPhrase = FilterPhrase;
            FilterPhrase = Widgets.TextField(rectFilter, FilterPhrase);

            // if there's no filter, show a label with the default search keyword
            if (FilterPhrase.NullOrEmpty())
            {
                Widgets.Label(new Rect(rectFilter.x + 6f, rectFilter.y + 3f, rectFilter.width, rectFilter.height), ResourceBank.String.SearchPlaceholder);
            } else if (Widgets.ButtonImage(rectClear, Widgets.CheckboxOffTex)) {
                FilterPhrase = "";                
            }            

            float width = ( maxDepth + 1 ) * ( Settings.NodeSize.x + Settings.NodeMargins.x ); // zero based
            float height = ( totalWidth - 1 ) * (Settings.NodeSize.y + Settings.NodeMargins.y );

            // main view rect
            Rect view = new Rect( 0f, 0f, width, height );
            
            // create the scroll area below the search box (plus a small margin) so it stays on top
            Widgets.BeginScrollView(new Rect(canvas.x,
                                    canvas.y + filterHeight + Settings.NodeMargins.y,
                                    canvas.width,canvas.height - filterHeight - Settings.NodeMargins.y),
                                    ref _scrollPosition, view );
            GUI.BeginGroup( view );

            Text.Anchor = TextAnchor.MiddleCenter;

            // draw regular connections, not done first to better highlight done.
            foreach ( Pair<Node, Node> connection in connections.Where( pair => !pair.Second.Research.IsFinished ) )
            {                
                ResearchTree.DrawLine( connection, connection.First.AdjustFilterAlpha(connection.First.Tree.GreyedColor,0.05f) );
            }

            // draw connections from completed nodes
            if (FilterPhrase.NullOrEmpty())
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
                bool visible = node.Draw();

                if (node.FilterMatch)
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

            // scroll if necessary
            if (FilterChanged)
            {
                if (scrollToNode != null)
                {
                    Rect r = scrollToNode.Rect.ScaledBy(2.0f);
                    _scrollPosition = new Vector2(r.xMin, r.yMin);
                } else if (FilterPhrase == "")
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