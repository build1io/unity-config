#if UNITY_EDITOR

using System;
using Build1.UnityEGUI;
using Build1.UnityEGUI.Json;
using Build1.UnityEGUI.Types;
using Build1.UnityEGUI.Window;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Build1.UnityConfig.Editor.Json
{
    public sealed class JsonWindow : EGUIWindow
    {
        private string SectionName { get; set; }
        private string Json        { get; set; }

        [SerializeField] private TreeViewState _jsonTreeViewState;
        [NonSerialized]  private JsonTreeView  _jsonTree;

        protected override void OnAwake()
        {
            Padding = 10;
            FocusLost += Close;

            // For debugging.
            // Json = "{" +
            //        "\"support_email\":\"email@gmail.com\"," +
            //        "\"support_email_subject\":\"Support Request #{0}\"," +
            //        "\"support_email_body\":\"Hey developers,\n\n\"," +
            //        "\"privacy_policy_url\":\"https://google.com\"," +
            //        "\"terms_of_use_url\":\"https://google.com\"," +
            //        "\"collection\":[0,1,2,3,4,5,6,1,2,3,4,5,6,1,2,3,4,5,6,1,2,3,4,5,6,1,2,3,4,5,6,1,2,3,4,5,6,1,2,3,4,5,6,1,2,3,4,5,6,1,2,3,4,5,6,1,2,3,4,5,6,1,2,3,4,5,6,1,2,3,4,5,6,1,2,3,4,5,6]," +
            //        "\"multiple_children\":" +
            //        "{" +
            //        "\"name\":\"value\"," +
            //        "\"int\":10" +
            //        "}" +
            //        "}";
        }

        protected override void OnInitialize()
        {
            _jsonTreeViewState ??= new TreeViewState();
            _jsonTree ??= new JsonTreeView(Json, _jsonTreeViewState);
        }

        protected override void OnEGUI()
        {
            EGUI.Horizontally(() =>
            {
                EGUI.Title(SectionName, TitleType.H3);
                EGUI.Space();
                EGUI.Label("* click anywhere outside to close", FontStyle.Italic);
            });
            EGUI.Space(5);

            var titleRect = EGUI.GetLastRect();

            EGUI.Space();
            EGUI.Title("Json:", TitleType.H3, 5);
            EGUI.Space(3);

            var jsonRect = EGUI.GetLastRect();

            EGUI.TextArea(Json.Replace("\n", ""), 150);
            EGUI.Space(1);
            EGUI.Horizontally(() =>
            {
                EGUI.Button("Expand All", 120, EGUI.ButtonHeight01, _jsonTree.ExpandAll);
                EGUI.Button("Collapse All", 120, EGUI.ButtonHeight01, _jsonTree.CollapseAll);
                EGUI.Space();
                EGUI.Button("Copy to Clipboard", 200, EGUI.ButtonHeight01, EGUI.CopyToClipboard, Json);
                EGUI.Button("Close", 100, EGUI.ButtonHeight01, Close);
            });

            const int x = 0;
            var y = titleRect.y + titleRect.height;
            var width = position.width - Padding * 2 - x;
            var height = jsonRect.y - titleRect.y - titleRect.height - Padding * 2;

            _jsonTree.OnEGUI(x, y, width, height);
        }

        /*
         * Static.
         */

        public static void Open(string sectionName, string json)
        {
            var window = EGUIWindow.Open<JsonWindow>("Json Viewer", 800, 900, true, true);
            window.SectionName = sectionName;
            window.Json = json;
        }
    }
}

#endif