#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using Build1.UnityConfig.Editor.Export;
using Build1.UnityConfig.Editor.Json;
using Build1.UnityConfig.Utils;
using Build1.UnityEGUI;
using Build1.UnityEGUI.Components.Label;
using Build1.UnityEGUI.Components.Title;
using Build1.UnityEGUI.Results;
using UnityEditor;
using UnityEngine;

namespace Build1.UnityConfig.Editor.Config.States
{
    internal sealed class ConfigState : ConfigEditorState
    {
        private static Vector2 _scrollPosition = new(0, 1);

        private readonly Dictionary<string, ConfigSectionEditor> _sections;

        public ConfigState(ConfigEditorModel model) : base(model)
        {
            _sections = UnityConfig.Instance.Sections;
        }

        public override void Draw()
        {
            var backClicked = false;
            var deleteConfig = false;

            EGUI.Horizontally(() =>
            {
                EGUI.Button("Back", EGUI.Height(EGUI.ButtonHeight01)).Clicked(out backClicked);
                EGUI.Enabled(model.SelectedConfigCanBeDeleted, () =>
                {
                    EGUI.Button("Delete", EGUI.Size(130, EGUI.ButtonHeight01)).Clicked(out deleteConfig);
                });
            });

            EGUI.Space(6);

            if (!model.ConfigSelected)
                return;

            var configViewClicked = false;
            var configCopyClicked = false;
            var configCopyMinClicked = false;
            var configCompressedCopyClicked = false;
            var exportClicked = false;

            var sectionSaveClicked = false;
            var sectionRevertClicked = false;
            var sectionViewClicked = false;

            EGUI.Line(Color.gray);
            EGUI.Space(2);
            EGUI.Horizontally(() =>
            {
                EGUI.Title(model.SelectedConfigName, TitleType.H3, EGUI.OffsetX(5), EGUI.StretchedWidth(), EGUI.StretchedHeight(), EGUI.TextAnchor(TextAnchor.MiddleLeft));

                switch (model.Settings.Mode)
                {
                    case ConfigMode.Default:
                        EGUI.Button("Copy Json Comp.", EGUI.Size(130, EGUI.DropDownHeight01)).Clicked(out configCompressedCopyClicked);
                        EGUI.Button("Copy Json Min.", EGUI.Size(130, EGUI.DropDownHeight01)).Clicked(out configCopyMinClicked);
                        
                        break;
                    case ConfigMode.Decomposed:
                        EGUI.Button("Export", EGUI.Size(130, EGUI.DropDownHeight01)).Clicked(out exportClicked);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                
                EGUI.Button("Copy Json", EGUI.Size(130, EGUI.DropDownHeight01)).Clicked(out configCopyClicked);
                EGUI.Button("View Json", EGUI.Size(130, EGUI.DropDownHeight01)).Clicked(out configViewClicked);
            });

            EGUI.Space(6);
            EGUI.Line(Color.gray);
            EGUI.Space(5);

            var configSections = model.SelectedConfigSections;
            var canBeSaved = model.SelectedConfigCanBeSaved;
            var modified = model.CheckSelectedSectionModified();

            if (backClicked)
            {
                if (canBeSaved && modified)
                    backClicked = AskSaveChanges();

                if (backClicked)
                    model.Reset();
                
                return;
            }

            if (deleteConfig)
            {
                if (EGUI.Alert(Application.productName, $"Are you sure you want to delete config {model.SelectedConfigName}?", "Delete", "Cancel"))
                    model.RemoveConfig(model.SelectedConfigName);
                return;
            }

            EGUI.Horizontally(() =>
            {
                EGUI.DropDown(configSections, model.SelectedConfigSectionIndex, EGUI.DropDownHeight01, newIndex =>
                {
                    var select = true;
                    
                    if (modified)
                        select = AskSaveChanges();
                    
                    if (select)
                        model.SelectSection(newIndex);
                });

                EGUI.Enabled(canBeSaved && modified, () =>
                {
                    EGUI.Button("Save", EGUI.Size(130, EGUI.DropDownHeight01))
                        .Clicked(out sectionSaveClicked);
                });
                
                // Firebase config can be edited locally and reverted, but can't be saved.
                EGUI.Enabled(modified, () =>
                {
                    EGUI.Button("Revert", EGUI.Size(130, EGUI.DropDownHeight01))
                        .Clicked(out sectionRevertClicked);
                });

                EGUI.Button("View Json", EGUI.Size(130, EGUI.DropDownHeight01))
                    .Clicked(out sectionViewClicked);
            });

            if (!canBeSaved)
            {
                EGUI.Space(5);
                EGUI.MessageBox("Firebase config can't be updated from Editor. Copy Json and update it manually.", MessageType.Warning);
            }

            EGUI.Space(5);
            EGUI.Line(Color.gray);

            EGUI.Space(10);

            var sectionName = model.SelectedConfigSectionName;
            if (canBeSaved && modified)
                sectionName += "*";

            var section = GetSection(model.SelectedConfigSectionName);

            EGUI.Title(sectionName, TitleType.H3, EGUI.OffsetX(5), EGUI.StretchedWidth(), EGUI.TextAnchor(TextAnchor.MiddleLeft));
            EGUI.Space(10);
            EGUI.Scroll(ref _scrollPosition, () =>
            {
                if (section == null)
                {
                    EGUI.Label("Section GUI not implemented.", LabelType.Error, EGUI.StretchedWidth(), EGUI.TextAnchor(TextAnchor.MiddleCenter));
                }
                else
                {
                    section.OnEGUI(model.SelectedConfigSection);
                }
            });

            EGUI.Space();

            if (configViewClicked)
                JsonViewer.Open(model.SelectedConfigName, model.SelectedConfig.ToJson(false));

            if (configCopyClicked)
                EGUI.CopyToClipboard(model.SelectedConfig.ToJson(true));
            
            if (configCopyMinClicked)
                EGUI.CopyToClipboard(model.SelectedConfig.ToJson(false));

            if (configCompressedCopyClicked)
                EGUI.CopyToClipboard(model.SelectedConfig.ToJson(false).Compress());

            if (exportClicked)
                ExportWindow.Open(model);

            if (sectionSaveClicked && section != null)
            {
                var validationErrorMessage = section.OnValidate(model.SelectedConfigSection);
                if (validationErrorMessage != null)
                {
                    EGUI.LogError($"Config: Validation error: \"{validationErrorMessage}\"");
                }
                else
                {
                    var proceed = true;
                    var json = model.SelectedConfig.ToJson(false);
                    if (json.Contains("с") || json.Contains("С"))
                        proceed = EditorUtility.DisplayDialog("Warning", "Config JSON contains cyrillic C symbol.\nThat's probably a typo.", "Proceed", "Abort");

                    if (proceed)
                    {
                        model.SaveSection();
                        EGUI.Log("Config: Saved");    
                    }
                }
            }

            if (sectionRevertClicked)
                model.RevertSection();

            if (sectionViewClicked)
                JsonViewer.Open(model.SelectedConfigName + "." + model.SelectedConfigSectionName, model.SelectedConfigSection.ToJson(false));
        }

        public override void Reset()
        {
            foreach (var section in _sections.Values)
                section.OnReset();
        }

        private bool AskSaveChanges()
        {
            return EGUI.Alert(Application.productName,
                              $"Save changes to {model.SelectedConfigSectionName} section?",
                              "Save", "Cancel", "Revert", result =>
                              {
                                  switch (result)
                                  {
                                      case AlertResult.Confirm:
                                          model.SaveSection();
                                          return true;
                                      case AlertResult.Discard:
                                          model.RevertSection();
                                          return true;
                                      case AlertResult.Cancel: 
                                          return false;
                                      default:
                                          throw new ArgumentOutOfRangeException(nameof(result), result, null);
                                  }
                              });
        }

        private ConfigSectionEditor GetSection(string name)
        {
            return _sections.TryGetValue(name, out var section) ? section : null;
        }
    }
}

#endif