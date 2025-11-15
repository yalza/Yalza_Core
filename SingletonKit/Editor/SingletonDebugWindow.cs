#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Yalza_Core.SingletonKit.Runtime;
using Yalza_Core.SingletonKit.Runtime.Core;

namespace Yalza_Core.SingletonKit.Editor
{
    public class SingletonDebugWindow : EditorWindow
    {
        private Vector2 _scroll;
        private string _search = "";
        private bool _autoRefresh = true;
        private double _lastRepaintTime;
        private const double RepaintInterval = 0.5; // giây

        [MenuItem("Yalza Core/Debug/Singletons Window", priority = 1000)]
        public static void Open()
        {
            var window = GetWindow<SingletonDebugWindow>("Singletons");
            window.minSize = new Vector2(350, 200);
        }

        private void OnEnable()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            EditorApplication.update += OnEditorUpdate;
        }

        private void OnDisable()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.update -= OnEditorUpdate;
        }

        private void OnEditorUpdate()
        {
            if (!_autoRefresh) return;

            if (EditorApplication.timeSinceStartup - _lastRepaintTime > RepaintInterval)
            {
                _lastRepaintTime = EditorApplication.timeSinceStartup;
                Repaint();
            }
        }

        private void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            Repaint();
        }

        private void OnGUI()
        {
            DrawToolbar();
            EditorGUILayout.Space(4);

            DrawSearchBar();
            EditorGUILayout.Space(4);

            DrawSingletonList();
        }

        private void DrawToolbar()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                GUILayout.Label(EditorApplication.isPlaying ? "Play Mode" : "Edit Mode", EditorStyles.toolbarButton);

                GUILayout.FlexibleSpace();

                _autoRefresh = GUILayout.Toggle(
                    _autoRefresh,
                    "Auto Refresh",
                    EditorStyles.toolbarButton,
                    GUILayout.Width(100)
                );

                if (GUILayout.Button("Refresh", EditorStyles.toolbarButton, GUILayout.Width(70)))
                {
                    Repaint();
                }
            }
        }

        private void DrawSearchBar()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label("Search", GUILayout.Width(50));
                _search = EditorGUILayout.TextField(_search);
            }
        }

        private void DrawSingletonList()
        {
            IReadOnlyCollection<object> instances = SingletonRegistry.Instances;

            if (instances == null || instances.Count == 0)
            {
                EditorGUILayout.HelpBox("No singleton instances registered.", MessageType.Info);
                return;
            }

            var filtered = instances
                .Where(i => i != null)
                .Where(i =>
                {
                    if (string.IsNullOrEmpty(_search)) return true;
                    var t = i.GetType();
                    return t.Name.IndexOf(_search, StringComparison.OrdinalIgnoreCase) >= 0
                           || t.FullName.IndexOf(_search, StringComparison.OrdinalIgnoreCase) >= 0;
                })
                .OrderBy(i => i.GetType().Name)
                .ToList();

            if (filtered.Count == 0)
            {
                EditorGUILayout.HelpBox("No singleton matches the search filter.", MessageType.Info);
                return;
            }

            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            foreach (var instance in filtered)
            {
                DrawSingletonItem(instance);
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawSingletonItem(object instance)
        {
            var type = instance.GetType();

            using (new EditorGUILayout.VerticalScope("box"))
            {
                // Header
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Label(type.Name, EditorStyles.boldLabel);

                    GUILayout.FlexibleSpace();

                    // Category tag
                    var cat = GetCategory(instance);
                    var catColor = GetCategoryColor(cat);
                    using (new ColorScope(catColor))
                    {
                        GUILayout.Label($"[{cat}]", EditorStyles.miniBoldLabel);
                    }

                    // Buttons cho UnityEngine.Object
                    if (instance is UnityEngine.Object unityObj)
                    {
                        if (GUILayout.Button("Ping", GUILayout.Width(50)))
                        {
                            EditorGUIUtility.PingObject(unityObj);
                        }

                        if (GUILayout.Button("Select", GUILayout.Width(60)))
                        {
                            Selection.activeObject = unityObj;
                        }
                    }
                }

                EditorGUI.indentLevel++;

                EditorGUILayout.LabelField("Type", type.FullName);

                // Thông tin chi tiết theo interface
                if (instance is IMonoSingleton monoSingleton)
                {
                    EditorGUILayout.LabelField("Initialized", monoSingleton.IsInitialized.ToString());

                    if (instance is MonoBehaviour mb && mb.gameObject != null)
                    {
                        EditorGUILayout.LabelField("GameObject", mb.gameObject.name);
                        EditorGUILayout.LabelField("Scene", mb.gameObject.scene.name);
                    }
                }
                else if (instance is INonMonoSingleton nonMono)
                {
                    EditorGUILayout.LabelField("Initialized", nonMono.IsInitialized.ToString());
                }

                EditorGUI.indentLevel--;
            }
        }

        private string GetCategory(object instance)
        {
            if (instance is MonoBehaviour)
            {
                // Thử đoán loại theo base type
                var t = instance.GetType();
                while (t != null)
                {
                    if (t.IsGenericType &&
                        t.GetGenericTypeDefinition() == typeof(MonoSingleton<>))
                        return "MonoSingleton";

                    if (t.IsGenericType &&
                        t.GetGenericTypeDefinition() == typeof(MonoSingletonInScene<>))
                        return "MonoSingletonInScene";

                    t = t.BaseType;
                }

                return "MonoBehaviour Singleton";
            }

            if (instance is INonMonoSingleton)
                return "NonMonoSingleton";

            return "Other";
        }

        private Color GetCategoryColor(string category)
        {
            switch (category)
            {
                case "MonoSingleton": return new Color(0.3f, 0.8f, 1f);
                case "MonoSingletonInScene": return new Color(0.4f, 1f, 0.4f);
                case "NonMonoSingleton": return new Color(1f, 0.8f, 0.3f);
                default: return new Color(0.8f, 0.8f, 0.8f);
            }
        }

        /// <summary>
        /// Helper đổi tạm GUI.color, auto restore khi dispose.
        /// </summary>
        private readonly struct ColorScope : IDisposable
        {
            private readonly Color _oldColor;

            public ColorScope(Color newColor)
            {
                _oldColor = GUI.color;
                GUI.color = newColor;
            }

            public void Dispose()
            {
                GUI.color = _oldColor;
            }
        }
    }
}
#endif
