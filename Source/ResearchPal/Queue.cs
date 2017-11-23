using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using RimWorld;
using Verse;

// ReSharper disable PossibleNullReferenceException
// reflection is dangerous - deal with it. Fluffy.

namespace ResearchPal
{
    [StaticConstructorOnStartup]
    public class Queue : GameComponent
    {
        #region Fields

        internal static readonly Texture2D CircleFill;
        private static readonly List<Node> _queue = new List<Node>();
        private static List<ResearchProjectDef> _saveableQueue;

        #endregion Fields

        #region Methods

        static Queue()
        {
            CircleFill = ContentFinder<Texture2D>.Get("UI/Research/circle-fill");
        }

        public Queue (Game game) { }

        public Queue () { }

        public static void Dequeue(Node node)
        {
            _queue.Remove(node);
            List<Node> followUps = _queue.Where(n => n.GetMissingRequiredRecursive().Contains(node)).ToList();
            foreach (Node followUp in followUps)
            {
                _queue.Remove(followUp);
            }
        }

        public static void DrawLabels()
        {
            int i = 1;
            foreach (Node node in _queue)
            {
                // draw coloured tag
                GUI.color = node.Tree.MediumColor;
                GUI.DrawTexture(node.QueueRect, CircleFill);

                // if this is not first in line, grey out centre of tag
                if (i > 1)
                {
                    GUI.color = node.Tree.GreyedColor;
                    GUI.DrawTexture(node.QueueRect.ContractedBy(2f), CircleFill);
                }

                // draw queue number
                GUI.color = Color.white;
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(node.QueueRect, i++.ToString());
                Text.Anchor = TextAnchor.UpperLeft;
            }
        }

        public static void Enqueue(Node node, bool add)
        {
            // if we're not adding, clear the current queue and current research project
            if (!add)
            {
                _queue.Clear();
                Find.ResearchManager.currentProj = null;
            }

            // add to the queue if not already in it
            if (!_queue.Contains(node))
            {
                _queue.Add(node);
            }

            // try set the first research in the queue to be the current project.
            Node next = _queue.First();
            Find.ResearchManager.currentProj = next?.Research; // null if next is null.
        }

        public static void EnqueueRange(IEnumerable<Node> nodes, bool add)
        {
            // clear current Queue if not adding
            if (!add)
            {
                _queue.Clear();
                Find.ResearchManager.currentProj = null;
            }

            // sorting by depth ensures prereqs are met - cost is just a bonus thingy.
            foreach (Node node in nodes.OrderBy(node => node.Depth).ThenBy(node => node.Research.CostApparent))
            {
                Enqueue(node, true);
            }
            Node first = _queue.First();
            Find.ResearchManager.currentProj = first?.Research;
        }

        public static bool IsQueued(Node node)
        {
            return _queue.Contains(node);
        }

        /// <summary>
        /// Removes and returns the first node in the queue.
        /// </summary>
        /// <returns></returns>
        public static Node Pop()
        {
            if (_queue != null && _queue.Count > 0)
            {
                Node node = _queue[0];
                _queue.RemoveAt(0);
                return node;
            }
            return null;
        }

        public override void ExposeData()
        {
            // store research defs as these are the defining elements
            if (Scribe.mode == LoadSaveMode.Saving)
            {
                _saveableQueue = _queue.Select(node => node.Research).ToList();
            }

            Scribe_Collections.Look(ref _saveableQueue, "Queue", LookMode.Def);

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                // initialize the tree if not initialized
                if (!ResearchTree.Initialized)
                    ResearchTree.Initialize();

                // initialize the queue
                foreach (ResearchProjectDef research in _saveableQueue)
                {
                    // find a node that matches the research - or null if none found
                    Node node = ResearchTree.Forest.FirstOrDefault(n => n.Research == research);

                    // enqueue the node
                    if (node != null)
                    {
                        Enqueue(node, true);
                    }
                }
            }
        }

        /// <summary>
        /// Override for Verse.ResearchMananager.MakeProgress
        ///
        /// Changes default pop-up when research is complete to an inbox message, and starts the next research in the queue - if available.
        /// </summary>
        /// <param name="amount"></param>
        public static ResearchProjectDef Next (ResearchProjectDef last)
        {
            Pop ();

            string label = ResourceBank.String.ResearchFinished (last.LabelCap);
            string text = label + "\n\n" + last.DescriptionDiscovered;
            LetterDef letter;
            ResearchProjectDef next;

            if (_queue.Count > 0) {
                next = _queue.First ().Research;
                text += "\n\nNext in queue: " + next.LabelCap;
                letter = LetterDefOf.PositiveEvent;
            } else {
                next = null;
                text += "\n\nNext in queue: none";
                letter = LetterDefOf.NegativeEvent;
            }

            Find.LetterStack.ReceiveLetter (label, text, letter);

            return next;
        }

        #endregion Methods
    }
}