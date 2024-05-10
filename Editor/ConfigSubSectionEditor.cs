#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using Build1.UnityEGUI;
using Build1.UnityEGUI.Types;
using UnityEngine;

namespace Build1.UnityConfig.Editor
{
    public abstract class ConfigSubSectionEditor
    {
        public virtual string SubSectionName { get; private set; }
        public         object DataObject     { get; private set; }

        public virtual void SetData(object dto, string name)
        {
            DataObject = dto;
            SubSectionName = name;
        }

        public abstract void   OnEGUI();
        public abstract string OnValidate();
    }

    public abstract class ConfigSubSectionEditor<T> : ConfigSubSectionEditor
    {
        protected T Data { get; private set; }

        private int                                      _foldsIndex;
        private Dictionary<object, ConfigSubSectionEditor> _subSectionsEditors;

        public override void SetData(object dto, string name)
        {
            base.SetData(dto, name ?? typeof(T).Name);

            try
            {
                Data = (T)dto;
            }
            catch
            {
                Debug.LogError($"Wrong section name for {GetType().FullName}. Cast type failed: {dto.GetType().FullName} != {typeof(T).FullName}");
            }
        }

        /*
         * Rendering.
         */

        public override void OnEGUI()
        {
            _foldsIndex = 0;

            OnEGUI(Data);
        }

        protected abstract void OnEGUI(T dto);

        /*
         * Sub Sections.
         */

        protected void RenderSubSection<S, D>(D dto) where S : ConfigSubSectionEditor<D>
        {
            RenderSubSection<S, D>(dto, null);
        }

        protected void RenderSubSection<S, D>(D dto, string name) where S : ConfigSubSectionEditor<D>
        {
            _subSectionsEditors ??= new Dictionary<object, ConfigSubSectionEditor>();

            if (!_subSectionsEditors.TryGetValue(dto, out var subSectionEditor))
            {
                subSectionEditor = (S)Activator.CreateInstance(typeof(S));
                subSectionEditor.SetData(dto, name);

                _subSectionsEditors[typeof(S)] = subSectionEditor;
            }

            EGUI.GetFoldInfo(this, out var folds);
            EGUI.Panel(10, () =>
            {
                var folded = folds[_foldsIndex];

                EGUI.Horizontally(() =>
                {
                    EGUI.Foldout(subSectionEditor.SubSectionName, FoldoutType.Bold, ref folded);

                    if (!folded)
                    {
                        EGUI.Button("...", EGUI.Size(30, EGUI.ButtonHeight04)).OnClick(() => { ConfigSubSectionWindow.Open(subSectionEditor); });
                    }
                });

                folds[_foldsIndex] = folded;

                if (folded)
                {
                    EGUI.Space(10);

                    subSectionEditor.OnEGUI();
                }
            });

            _foldsIndex++;
        }

        /*
         * Validation.
         */

        public override string OnValidate()
        {
            if (_subSectionsEditors != null)
            {
                foreach (var subSectionEditor in _subSectionsEditors.Values)
                {
                    var result = subSectionEditor.OnValidate();
                    if (result != null)
                        return result;
                }
            }

            return OnValidate(Data);
        }

        protected abstract string OnValidate(T dto);
    }
}

#endif