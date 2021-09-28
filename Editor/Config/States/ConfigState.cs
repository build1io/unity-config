#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using Build1.UnityConfig.Editor.Config.Sections;
using Build1.UnityConfig.Editor.Json;
using Build1.UnityEGUI;
using Build1.UnityEGUI.Results;
using Build1.UnityEGUI.Types;
using UnityEditor;
using UnityEngine;

namespace Build1.UnityConfig.Editor.Config.States
{
    public sealed class ConfigState : State
    {
        private static Vector2 _scrollPosition = new Vector2(0, 1);

        private readonly Dictionary<string, Section> _sections;

        public ConfigState(ConfigModel model) : base(model)
        {
            _sections = UnityConfig.Sections;
        }

        public override void Draw()
        {
            var backClicked = false;
            var deleteConfig = false;

            EGUI.Horizontally(() =>
            {
                EGUI.Button("Back", out backClicked, EGUI.ButtonHeight01);
                EGUI.Enabled(model.SelectedConfigCanBeDeleted, () => { EGUI.Button("Delete", out deleteConfig, 130, EGUI.ButtonHeight01); });
            });

            EGUI.Space(6);

            if (!model.ConfigSelected)
                return;

            var configViewClicked = false;
            var configCopyClicked = false;

            var sectionSaveClicked = false;
            var sectionRevertClicked = false;
            var sectionViewClicked = false;

            EGUI.Line(Color.gray);
            EGUI.Space(2);
            EGUI.Horizontally(() =>
            {
                EGUI.Title(model.SelectedConfigName, TitleType.H3, 5, true, TextAnchor.MiddleLeft);
                EGUI.Button("Copy Json", out configCopyClicked, 130, EGUI.ButtonHeight01);
                EGUI.Button("View Json", out configViewClicked, 130, EGUI.ButtonHeight01);
            });

            EGUI.Space(6);
            EGUI.Line(Color.gray);
            EGUI.Space(5);

            var configSections = model.SelectedConfigSections;
            var canBeSaved = model.SelectedConfigCanBeSaved;
            var modified = model.CheckSelectedSectionModified();

            if (backClicked)
            {
                if (modified)
                    backClicked = AskSaveChanges();
                if (backClicked)
                    model.Reset();
                return;
            }

            if (deleteConfig)
            {
                if (EGUI.Alert(Application.identifier, $"Are you sure you want to delete config {model.SelectedConfigName}?", "Delete", "Cancel"))
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
                    EGUI.Button("Save", out sectionSaveClicked, 130, EGUI.DropDownHeight01);
                });

                EGUI.Enabled(modified, () =>
                {
                    EGUI.Button("Revert", out sectionRevertClicked, 130, EGUI.DropDownHeight01);
                });

                EGUI.Button("View Json", out sectionViewClicked, 130, EGUI.DropDownHeight01);
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

            EGUI.Title(sectionName, TitleType.H3, 5, true, TextAnchor.MiddleLeft);
            EGUI.Space(10);
            EGUI.Scroll(ref _scrollPosition, () =>
            {
                if (section == null)
                {
                    EGUI.Label("Section GUI not implemented.", LabelType.Error, true, TextAnchor.MiddleCenter);
                }
                else
                {
                    section.OnEGUI(model.SelectedConfigSection);
                }
            });

            EGUI.Space();

            if (configViewClicked)
                JsonWindow.Open(model.SelectedConfigName, model.SelectedConfig.ToJson(false));

            if (configCopyClicked)
                EGUI.CopyToClipboard(model.SelectedConfig.ToJson(false));

            if (sectionSaveClicked && section != null)
            {
                var validationErrorMessage = section.OnValidate(model.SelectedConfigSection);
                if (validationErrorMessage == null)
                {
                    model.SaveSection();
                    EGUI.Log("Config: Saved");
                }
                else
                {
                    EGUI.LogError($"Config: Not saved. Error: \"{validationErrorMessage}\"");
                }
            }

            if (sectionRevertClicked)
                model.RevertSection();

            if (sectionViewClicked)
                JsonWindow.Open(model.SelectedConfigName + "." + model.SelectedConfigSectionName, model.SelectedConfigSection.ToJson(false));
        }

        private bool AskSaveChanges()
        {
            return EGUI.Alert(Application.identifier,
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

        private Section GetSection(string name)
        {
            return _sections.TryGetValue(name, out var section) ? section : null;
        }
    }
}

#endif