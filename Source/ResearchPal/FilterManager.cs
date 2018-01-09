using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using RimWorld;
using Verse;
using System.Text.RegularExpressions;

namespace ResearchPal
{
    [StaticConstructorOnStartup]
    public class FilterManager
    {

        // matching priority is done in order of value
        public enum FilterMatchType
        {
            RESEARCH   = 0,
            UNLOCK     = 1,
            TECH_LEVEL = 2,
            NONE       = 99,
            NO_MATCH   = 100
        }

        #region Fields

        private const float _filterHeight = 24f;
        private const int _commaConcatThreshold = 15;

        private string _filterPhrase = "";
        private string _inputChar;

        private bool _filterDirty = false;
        private bool _resetOnOpen = false;

        private bool _settingFocus = false;
        private bool _forceShowFilter = false;

        private string _filterResultTitle = "";
        private string _filterResultTooltip = "";
        private Dictionary<FilterMatchType, List<string>> _matchResults = new Dictionary<FilterMatchType, List<string>>();

        private bool _rectSet;
        private Rect _rectFilterBtn;
        private Rect _rectFilter;
        private Rect _rectClearBtn;
        private Rect _rectMessage;

        private const string _filterInputName = "FilterInput";
        private static Regex _rxInput = new Regex(@"[\p{L}\p{Nd}\s-_]+");

        public static Texture2D FilterIcon;
        #endregion Fields

        #region Constructors

        static FilterManager()
        {
            FilterIcon = ContentFinder<Texture2D>.Get("UI/Research/magnifier");
        }


        public FilterManager() {}

        #endregion Constructors

        #region Properties

        public float Height
        {
            get
            {
                return _filterHeight;
            }
        }

        public Rect RectFilterBtn
        {
            get
            {
                if (!_rectSet)
                {
                    CreateRects();
                }
                return _rectFilterBtn;
            }
        }

        public Rect RectFilter
        {
            get
            {
                if (!_rectSet)
                {
                    CreateRects();
                }
                return _rectFilter;
            }
        }

        public Rect RectClearBtn
        {
            get
            {
                if (!_rectSet)
                {
                    CreateRects();
                }
                return _rectClearBtn;
            }
        }

        public Rect RectMessage
        {
            get
            {
                if (!_rectSet)
                {
                    CreateRects();
                }
                return _rectMessage;
            }
        }

        public string FilterPhrase
        {
            get
            {
                return _filterPhrase;
            }
            set
            {
                _filterPhrase = value;
            }
        }

        public bool FilterDirty
        {
            get
            {
                return _filterDirty || _resetOnOpen;
            }
        }

        #endregion Properties


        /// <summary>
        /// Process the current event key for valid input and try to refocus on the filter input
        /// </summary>
        public void KeyPress()
        {
            if (_rxInput.IsMatch(Event.current.character.ToString()))
            {
                GUIUtility.keyboardControl = 0; // force a refocus on the input control
                _inputChar = Event.current.character.ToString();
            }
            else if (Event.current.keyCode == KeyCode.Backspace)
            {
                GUIUtility.keyboardControl = 0;
            }
        }

        private void CreateRects()
        {
            // TODO: find the actual width of the tab/window instead of using screen width

            // filter button
            _rectFilterBtn = new Rect(0f, 0f, _filterHeight, _filterHeight);

            // filter input
            _rectFilter = new Rect(_rectFilterBtn.xMax + 6f, 0f, (UI.screenWidth - _rectFilterBtn.width) / 6f, _filterHeight);

            // clear button area
            _rectClearBtn = new Rect(_rectFilter.xMax + 3f, _filterHeight / 4f, _filterHeight / 2f, _filterHeight / 2f);

            // result message area
            _rectMessage = new Rect(_rectClearBtn.xMax + 10f, 0f, (UI.screenWidth - (_rectClearBtn.xMax + 10f)) / 2f, _filterHeight);

            _rectSet = true;
        }

        private bool FilterActive()
        {
            return (!_filterPhrase.NullOrEmpty() || _forceShowFilter);
        }

        private void ClearInput()
        {
            _filterPhrase = "";
            GUIUtility.keyboardControl = 0;
        }

        /// <summary>
        /// Compares a node's research, unlocks and tech-level to the current filter. Updates the node of it's match status.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public FilterMatchType NodeIsMatch(Node node)
        {
            if (!FilterDirty)
                return FilterMatchType.NONE;

            string phrase = _filterPhrase.Trim();
            FilterMatchType ret = FilterMatchType.NONE;
            string retDesc = StringExtensions.TitleCase(node.Research.LabelCap); // default
            if (phrase != "")
            {
                foreach (FilterMatchType tryMatch in Enum.GetValues(typeof(FilterMatchType)))
                {
                    Log.Message("trying match for " + tryMatch.ToString());
                    switch (tryMatch)
                    {
                        case FilterMatchType.RESEARCH:
                            if (node.Research.label.Contains(phrase, StringComparison.InvariantCultureIgnoreCase))
                            {
                                ret = FilterMatchType.RESEARCH;
                            }
                            break;
                        case FilterMatchType.UNLOCK:
                            {
                                List<string> unlockDescs = node.Research.GetUnlockDefsAndDescs()
                                        .Where(unlock => unlock.First.label.Contains(phrase, StringComparison.InvariantCulture))
                                        .Select(unlock => unlock.First.label).ToList();
                                if (unlockDescs.Count > 0)
                                {
                                    retDesc = string.Format("{0} ({1})", retDesc, StringExtensions.TitleCase(string.Join(", ", unlockDescs.ToArray())));
                                    ret = FilterMatchType.UNLOCK;
                                }
                            }
                            break;
                        case FilterMatchType.TECH_LEVEL:
                            {
                                if (node.Research.techLevel.ToStringHuman().Contains(phrase, StringComparison.InvariantCultureIgnoreCase))
                                {
                                    ret = FilterMatchType.TECH_LEVEL;
                                }
                            }
                            break;
                        default:
                            ret = FilterMatchType.NO_MATCH;
                            break;

                    }
                    if (ret != FilterMatchType.NONE)
                        break;
                }
            } else {
                ret = FilterMatchType.NONE;
            }

            // save the result for display later
            if (ret.IsValidMatch())
            {
                if (!_matchResults.ContainsKey(ret))
                {
                    _matchResults.Add(ret, new List<string>());
                }
                _matchResults[ret].Add(retDesc);
            }

            // update the node of the result
            node.FilterMatch = ret;

            return ret;
        }

        public void DrawFilterControls(Rect canvas)
        {
            GUI.BeginGroup(canvas);

            string oldPhrase = _filterPhrase;
            if (!_inputChar.NullOrEmpty())
            {
                _filterPhrase += _inputChar;
                _inputChar = "";
            }

            // check the toggle button
            if (Widgets.ButtonImage(RectFilterBtn, FilterIcon))
            {
                // flip the toggle
                _forceShowFilter = !(_forceShowFilter || !_filterPhrase.NullOrEmpty());
                if (!_forceShowFilter)
                {
                    ClearInput();
                }
            }

            if (FilterActive())
            {
                if (Widgets.ButtonImage(RectClearBtn, Widgets.CheckboxOffTex))
                {
                    ClearInput();
                } else {
                    // add a text widget with the current filter phrase
                    GUI.SetNextControlName(_filterInputName);

                    // focus the filter input field immediately if we're not already focused
                    if (GUI.GetNameOfFocusedControl() != _filterInputName)
                    {
                        _settingFocus = true;
                        GUI.FocusControl(_filterInputName);
                    } else {
                        // if the focus was just set, then the automatic behaviour is to select all the text
                        // we don't want that, so immediately deselect the text, and move the cursor to the end
                        if (_settingFocus)
                        {
                            TextEditor te = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);
                            te.SelectNone();
                            te.MoveTextEnd();
                            _settingFocus = false;
                        }
                    }

                    _filterPhrase = Widgets.TextField(RectFilter, _filterPhrase);
                }
            } else if (GUI.GetNameOfFocusedControl() == _filterInputName) {
                GUIUtility.keyboardControl = 0;
            }

            _filterDirty = (oldPhrase != _filterPhrase);
            if (_filterDirty)
            {
                _matchResults.Clear();
            }
            GUI.EndGroup();
        }

        private void BuildFilterResultMessages()
        {
            _filterResultTitle = ResourceBank.String.FilterResults(_matchResults.Sum(k => k.Value.Count));

            var tt = new StringBuilder();
            foreach (KeyValuePair<FilterMatchType, List<string>> info in _matchResults.OrderBy(k => k.Key))
            {
                tt.AppendLine(info.Key.ToFriendlyString());
                string indent = "";
                if (info.Value.Count() <= _commaConcatThreshold)
                {
                    indent = "  ";
                }
                tt.Append(indent + string.Join(info.Value.Count() <= _commaConcatThreshold ? Environment.NewLine + indent : ", ", info.Value.ToArray()));
                tt.AppendLine();
                tt.AppendLine();
            }
            _filterResultTooltip = tt.ToString();
        }

        public void DrawFilterResults(Rect canvas)
        {
            // rebuild the message/tooltip if necessary
            if (FilterDirty)
            {
                BuildFilterResultMessages();
            }

            GUI.BeginGroup(canvas);
            GUI.color = Color.white;
            if (FilterActive())
            {
                Widgets.Label(RectMessage, _filterResultTitle);
                if (!_filterResultTooltip.NullOrEmpty())
                {
                    TooltipHandler.TipRegion(_rectMessage, _filterResultTooltip);
                }
            }
            GUI.EndGroup();
            _resetOnOpen = false;
        }

        public void Reset()
        {
            ClearInput();
            _forceShowFilter = false;
            _resetOnOpen = true;
            _filterResultTitle = "";
            _filterResultTooltip = "";
        }

    }

    public static class FilterMatchExtension
    {
        public static string ToFriendlyString(this FilterManager.FilterMatchType fType)
        {
            switch (fType)
            {
                case FilterManager.FilterMatchType.RESEARCH:
                    return ResourceBank.String.FilterTitleResearch;
                case FilterManager.FilterMatchType.UNLOCK:
                    return ResourceBank.String.FilterTitleUnlocks;
                case FilterManager.FilterMatchType.TECH_LEVEL:
                    return ResourceBank.String.FilterTitleTechLevel;
                default:
                    return "";
            }
        }

        public static bool IsValidMatch(this FilterManager.FilterMatchType fType)
        {
            return (fType != FilterManager.FilterMatchType.NO_MATCH && fType != FilterManager.FilterMatchType.NONE);
        }
    }

}



