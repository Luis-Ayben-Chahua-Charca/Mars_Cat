// Asistencia de IA: escrito con Claude Code (Anthropic).
using System;
using System.Collections.Generic;
using MarsCat.Unity;
using MarsCat.Unity.Minigames;
using MarsCat.Unity.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace MarsCat.EditorTools
{
    /// <summary>
    /// Utilidades compartidas por los builders de minijuegos (Tools → Mars Cat → Minijuegos → ...).
    /// Mismas reglas que HudBuilder: idempotente (busca por nombre bajo el padre y, si existe, no toca su distribución
    /// ni su estilo), enlaza campos solo si están vacíos, todo en un paso de Undo, marca la escena sin guardarla.
    /// Uso típico:
    ///   MinigameUiKit.Begin("Crear minijuego X");
    ///   var (container, root) = MinigameUiKit.MinigameRoot("Minijuego_X", "Título");
    ///   ... Node / Text / Button / Bar ...
    ///   var view = MinigameUiKit.Add&lt;MiVista&gt;(container, out bool isNew);
    ///   MinigameUiKit.Wire(view, so => { MinigameUiKit.Ref(so, "_root", root.gameObject); ... });
    ///   MinigameUiKit.Register(view, "solar_panels");
    ///   MinigameUiKit.End();
    /// </summary>
    public static class MinigameUiKit
    {
        public const string SpriteFolder = "Assets/UI";

        public static readonly Color TextColor = new Color32(0xF2, 0xEC, 0xFF, 0xFF);
        public static readonly Color AccentColor = new Color32(0x22, 0xE5, 0xF0, 0xFF);
        public static readonly Color PanelColor = new Color(0.07f, 0.06f, 0.09f, 0.92f);
        public static readonly Color ButtonColor = new Color(0.85f, 0.45f, 0.2f, 1f);
        public static readonly Color GreenZoneColor = new Color(0.2f, 0.85f, 0.35f, 0.55f);
        public static readonly Color MissingSpriteColor = new Color(1f, 1f, 1f, 0.25f);
        public static readonly Vector2 DefaultPanelSize = new Vector2(1000f, 640f);

        public static readonly Vector2 TL = new Vector2(0f, 1f);
        public static readonly Vector2 TC = new Vector2(0.5f, 1f);
        public static readonly Vector2 TR = new Vector2(1f, 1f);
        public static readonly Vector2 ML = new Vector2(0f, 0.5f);
        public static readonly Vector2 C = new Vector2(0.5f, 0.5f);
        public static readonly Vector2 MR = new Vector2(1f, 0.5f);
        public static readonly Vector2 BL = new Vector2(0f, 0f);
        public static readonly Vector2 BC = new Vector2(0.5f, 0f);
        public static readonly Vector2 BR = new Vector2(1f, 0f);

        private static readonly List<string> Created = new List<string>();
        private static readonly List<string> Reused = new List<string>();
        private static readonly SortedSet<string> MissingSprites = new SortedSet<string>();
        private static string _undoName = "Crear minijuego";
        private static int _group;
        private static RectTransform _canvas;

        public static GameBootstrap Bootstrap { get; private set; }

        // ---------------- Operación ----------------

        public static void Begin(string undoName)
        {
            _undoName = undoName;
            Undo.IncrementCurrentGroup();
            Undo.SetCurrentGroupName(undoName);
            _group = Undo.GetCurrentGroup();
            Created.Clear();
            Reused.Clear();
            MissingSprites.Clear();
            _canvas = null;
            Bootstrap = Object.FindAnyObjectByType<GameBootstrap>(FindObjectsInactive.Include);
            if (Bootstrap == null) Debug.LogWarning(undoName + ": no hay GameBootstrap en la escena.");
            if (TMP_Settings.instance == null || TMP_Settings.defaultFontAsset == null)
                Debug.LogWarning(undoName + ": faltan los TMP Essential Resources (Window → TextMeshPro → Import TMP Essential Resources).");
        }

        public static void End()
        {
            Undo.CollapseUndoOperations(_group);
            RectTransform canvas = Canvas();
            EditorSceneManager.MarkSceneDirty(canvas.gameObject.scene);
            Debug.Log(_undoName + ": listo (la escena no se guardó).\n" +
                      $"Creados ({Created.Count}): {string.Join(", ", Created)}\n" +
                      $"Reutilizados ({Reused.Count}): {string.Join(", ", Reused)}\n" +
                      $"Sprites que faltan en {SpriteFolder}/ ({MissingSprites.Count}): {string.Join(", ", MissingSprites)}");
        }

        /// <summary>El Canvas del HUD (raíz "Canvas", o cualquier Canvas raíz). Lo crea si no existe.</summary>
        public static RectTransform Canvas()
        {
            if (_canvas != null) return _canvas;
            Canvas found = null;
            foreach (GameObject root in EditorSceneManager.GetActiveScene().GetRootGameObjects())
            {
                var canvas = root.GetComponent<Canvas>();
                if (canvas == null) continue;
                if (root.name == "Canvas") { found = canvas; break; }
                if (found == null) found = canvas;
            }
            if (found == null)
            {
                var go = new GameObject("Canvas", typeof(RectTransform));
                Undo.RegisterCreatedObjectUndo(go, _undoName);
                found = Undo.AddComponent<Canvas>(go);
                found.renderMode = RenderMode.ScreenSpaceOverlay;
                var scaler = Undo.AddComponent<CanvasScaler>(go);
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920f, 1080f);
                scaler.matchWidthOrHeight = 0.5f;
                Undo.AddComponent<GraphicRaycaster>(go);
                Created.Add("Canvas");
            }
            _canvas = (RectTransform)found.transform;
            return _canvas;
        }

        /// <summary>
        /// Contenedor "Minijuegos/&lt;name&gt;" (lleva el script del minijuego, siempre activo) con un hijo "Root": panel
        /// centrado que el script muestra y oculta, con fondo que bloquea los clics y un título arriba.
        /// </summary>
        public static (RectTransform container, RectTransform root) MinigameRoot(string name, string title, Vector2? size = null)
        {
            RectTransform group = Node(Canvas(), "Minijuegos", Stretch);
            group.SetAsLastSibling();
            RectTransform container = Node(group, name, Stretch);
            RectTransform root = Node(container, "Root", r => Set(r, C, C, C, Vector2.zero, size ?? DefaultPanelSize));
            Image background = Panel(root, PanelColor);
            if (!background.raycastTarget)
            {
                Undo.RecordObject(background, _undoName);
                background.raycastTarget = true;
            }
            Text(Node(root, "Titulo", r => Set(r, TL, TR, TC, new Vector2(0f, -18f), new Vector2(-40f, 44f))),
                32f, FontStyles.Bold, TextAlignmentOptions.Center, title);
            Text(Node(root, "AyudaTeclas", r => Set(r, BL, BR, BC, new Vector2(0f, 12f), new Vector2(-40f, 28f))),
                16f, FontStyles.Normal, TextAlignmentOptions.Center, "Esc: salir");
            return (container, root);
        }

        /// <summary>Registra el minijuego en el MinigameHost de la escena (lo crea en "Minijuegos" si no existe).</summary>
        public static void Register(MinigameView view, string buildingId)
        {
            var host = Object.FindAnyObjectByType<MinigameHost>(FindObjectsInactive.Include);
            if (host == null)
            {
                RectTransform group = Node(Canvas(), "Minijuegos", Stretch);
                host = Add<MinigameHost>(group, out _);
            }
            Wire(host, so =>
            {
                Ref(so, "_bootstrap", Bootstrap);
                Ref(so, "_detailPanel", Object.FindAnyObjectByType<DetailPanel>(FindObjectsInactive.Include));
                Ref(so, "_aiCorner", Object.FindAnyObjectByType<AiCorner>(FindObjectsInactive.Include));
                SerializedProperty entries = so.FindProperty("_entries");
                for (int i = 0; i < entries.arraySize; i++)
                {
                    SerializedProperty entry = entries.GetArrayElementAtIndex(i);
                    if (entry.FindPropertyRelative("BuildingId").stringValue == buildingId)
                    {
                        SerializedProperty existing = entry.FindPropertyRelative("View");
                        if (existing.objectReferenceValue == null) existing.objectReferenceValue = view;
                        return;
                    }
                }
                entries.arraySize++;
                SerializedProperty added = entries.GetArrayElementAtIndex(entries.arraySize - 1);
                added.FindPropertyRelative("BuildingId").stringValue = buildingId;
                added.FindPropertyRelative("View").objectReferenceValue = view;
            });
        }

        // ---------------- Nodos y distribución ----------------

        /// <summary>Hijo por nombre; si no existe lo crea y le aplica la distribución. Si existe, no lo toca.</summary>
        public static RectTransform Node(RectTransform parent, string name, Action<RectTransform> layoutWhenCreated)
        {
            string path = PathOf(parent) + "/" + name;
            Transform child = parent.Find(name);
            if (child is RectTransform existing)
            {
                Reused.Add(path);
                return existing;
            }

            var go = new GameObject(name, typeof(RectTransform));
            Undo.RegisterCreatedObjectUndo(go, _undoName);
            Undo.SetTransformParent(go.transform, parent, _undoName);
            go.layer = parent.gameObject.layer;
            var rect = (RectTransform)go.transform;
            rect.localScale = Vector3.one;
            rect.localRotation = Quaternion.identity;
            rect.anchorMin = rect.anchorMax = rect.pivot = C;
            rect.anchoredPosition = Vector2.zero;
            layoutWhenCreated?.Invoke(rect);
            Created.Add(path);
            return rect;
        }

        public static void Set(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 position, Vector2 size)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = pivot;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }

        /// <summary>Atajo: ancla y pivote iguales.</summary>
        public static Action<RectTransform> At(Vector2 anchor, Vector2 position, Vector2 size) =>
            r => Set(r, anchor, anchor, anchor, position, size);

        public static void Stretch(RectTransform rect) => Margins(rect, 0f, 0f, 0f, 0f);

        public static void Margins(RectTransform rect, float left, float right, float top, float bottom)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = C;
            rect.offsetMin = new Vector2(left, bottom);
            rect.offsetMax = new Vector2(-right, -top);
        }

        public static void Row(RectTransform rect, float spacing, TextAnchor alignment, bool controlAndExpand)
        {
            HorizontalLayoutGroup layout = Add<HorizontalLayoutGroup>(rect, out bool isNew);
            if (!isNew) return;
            layout.spacing = spacing;
            layout.childAlignment = alignment;
            layout.childControlWidth = controlAndExpand;
            layout.childControlHeight = controlAndExpand;
            layout.childForceExpandWidth = controlAndExpand;
            layout.childForceExpandHeight = false;
        }

        public static void Column(RectTransform rect, float spacing, TextAnchor alignment, bool controlAndExpand)
        {
            VerticalLayoutGroup layout = Add<VerticalLayoutGroup>(rect, out bool isNew);
            if (!isNew) return;
            layout.spacing = spacing;
            layout.childAlignment = alignment;
            layout.childControlWidth = controlAndExpand;
            layout.childControlHeight = controlAndExpand;
            layout.childForceExpandWidth = controlAndExpand;
            layout.childForceExpandHeight = false;
        }

        // ---------------- Elementos ----------------

        /// <summary>Image de color liso, decorativa (raycast apagado).</summary>
        public static Image Panel(RectTransform rect, Color color)
        {
            Image image = Add<Image>(rect, out bool isNew);
            if (isNew)
            {
                image.raycastTarget = false;
                image.color = color;
            }
            return image;
        }

        /// <summary>Image decorativa con sprite de Assets/UI (si falta: blanco al 25 %).</summary>
        public static Image Decor(RectTransform rect, string spriteName, bool preserveAspect = false)
        {
            Image image = Add<Image>(rect, out bool isNew);
            if (isNew)
            {
                image.raycastTarget = false;
                image.preserveAspect = preserveAspect;
            }
            if (image.sprite == null && !string.IsNullOrEmpty(spriteName))
            {
                Sprite sprite = FindSprite(spriteName);
                Undo.RecordObject(image, _undoName);
                if (sprite != null)
                {
                    image.sprite = sprite;
                    image.color = Color.white;
                }
                else
                {
                    image.color = MissingSpriteColor;
                }
            }
            return image;
        }

        /// <summary>Barra horizontal: fondo + relleno Filled (Horizontal) que el script controla con fillAmount.</summary>
        public static Image Bar(RectTransform rect, Color fillColor)
        {
            Panel(Node(rect, "Fondo", Stretch), new Color(1f, 1f, 1f, 0.12f));
            RectTransform fillRect = Node(rect, "Relleno", Stretch);
            Image fill = Add<Image>(fillRect, out bool isNew);
            if (isNew)
            {
                fill.raycastTarget = false;
                fill.color = fillColor;
                fill.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
                fill.type = Image.Type.Filled;
                fill.fillMethod = Image.FillMethod.Horizontal;
                fill.fillOrigin = (int)Image.OriginHorizontal.Left;
                fill.fillAmount = 0.5f;
            }
            return fill;
        }

        /// <summary>TMP decorativo. El estilo solo se aplica al crearlo.</summary>
        public static TMP_Text Text(RectTransform rect, float size, FontStyles style, TextAlignmentOptions alignment,
            string initial, bool wrap = false, Color? color = null)
        {
            TextMeshProUGUI text = Add<TextMeshProUGUI>(rect, out bool isNew);
            if (isNew)
            {
                text.raycastTarget = false;
                text.fontSize = size;
                text.enableAutoSizing = false;
                text.fontStyle = style;
                text.alignment = alignment;
                text.color = color ?? TextColor;
                text.textWrappingMode = wrap ? TextWrappingModes.Normal : TextWrappingModes.NoWrap;
                text.overflowMode = wrap ? TextOverflowModes.Truncate : TextOverflowModes.Ellipsis;
                text.text = initial;
            }
            return text;
        }

        /// <summary>Botón con fondo y texto hijo "Texto". Devuelve el botón y su texto.</summary>
        public static (Button button, TMP_Text label) Button(RectTransform rect, string caption, string spriteName = "10_boton")
        {
            Image image = Add<Image>(rect, out bool newImage);
            if (newImage)
            {
                image.raycastTarget = true;
                Sprite sprite = FindSprite(spriteName, reportMissing: false)
                                ?? AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
                image.sprite = sprite;
                image.type = sprite != null && sprite.border != Vector4.zero ? Image.Type.Sliced : Image.Type.Simple;
                image.color = sprite != null && sprite.name == "UISprite" ? ButtonColor : Color.white;
            }
            Button button = Add<Button>(rect, out bool newButton);
            if (newButton) button.targetGraphic = image;
            TMP_Text label = Text(Node(rect, "Texto", Stretch), 22f, FontStyles.Bold, TextAlignmentOptions.Center, caption);
            return (button, label);
        }

        /// <summary>Slider horizontal estándar de uGUI (fondo, relleno y manija), de min a max.</summary>
        public static Slider Slider(RectTransform rect, float min, float max, float value)
        {
            Slider slider = Add<Slider>(rect, out bool isNew);
            RectTransform background = Node(rect, "Fondo", r => Margins(r, 0f, 0f, 8f, 8f));
            Panel(background, new Color(1f, 1f, 1f, 0.15f));
            RectTransform fillArea = Node(rect, "Area", r => Margins(r, 8f, 8f, 8f, 8f));
            RectTransform fill = Node(fillArea, "Relleno", Stretch);
            Image fillImage = Panel(fill, AccentColor);
            RectTransform handleArea = Node(rect, "ZonaManija", r => Margins(r, 10f, 10f, 0f, 0f));
            RectTransform handle = Node(handleArea, "Manija", r => Set(r, new Vector2(0f, 0f), new Vector2(0f, 1f), C, Vector2.zero, new Vector2(20f, 0f)));
            Image handleImage = Add<Image>(handle, out bool newHandle);
            if (newHandle)
            {
                handleImage.color = Color.white;
                handleImage.raycastTarget = true;
            }
            if (isNew)
            {
                slider.fillRect = fill;
                slider.handleRect = handle;
                slider.targetGraphic = handleImage;
                slider.direction = UnityEngine.UI.Slider.Direction.LeftToRight;
                slider.minValue = min;
                slider.maxValue = max;
                slider.value = value;
                fillImage.raycastTarget = false;
            }
            return slider;
        }

        // ---------------- Componentes y campos ----------------

        public static T Add<T>(Component owner, out bool isNew) where T : Component
        {
            T component = owner.GetComponent<T>();
            isNew = component == null;
            return isNew ? Undo.AddComponent<T>(owner.gameObject) : component;
        }

        public static void Wire(Object target, Action<SerializedObject> assign)
        {
            var so = new SerializedObject(target);
            assign(so);
            so.ApplyModifiedProperties();
        }

        /// <summary>Enlaza el campo solo si está vacío.</summary>
        public static void Ref(SerializedObject so, string field, Object value)
        {
            SerializedProperty property = Find(so, field);
            if (property == null || property.objectReferenceValue != null || value == null) return;
            property.objectReferenceValue = value;
        }

        /// <summary>Enlaza un arreglo de referencias solo si está vacío.</summary>
        public static void RefArray(SerializedObject so, string field, IList<Object> values)
        {
            SerializedProperty property = Find(so, field);
            if (property == null || !property.isArray || property.arraySize > 0) return;
            property.arraySize = values.Count;
            for (int i = 0; i < values.Count; i++)
                property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
        }

        public static void Float(SerializedObject so, string field, float value)
        {
            SerializedProperty property = Find(so, field);
            if (property != null) property.floatValue = value;
        }

        public static void Int(SerializedObject so, string field, int value)
        {
            SerializedProperty property = Find(so, field);
            if (property != null) property.intValue = value;
        }

        public static void Bool(SerializedObject so, string field, bool value)
        {
            SerializedProperty property = Find(so, field);
            if (property != null) property.boolValue = value;
        }

        public static void String(SerializedObject so, string field, string value)
        {
            SerializedProperty property = Find(so, field);
            if (property != null && string.IsNullOrEmpty(property.stringValue)) property.stringValue = value;
        }

        private static SerializedProperty Find(SerializedObject so, string field)
        {
            SerializedProperty property = so.FindProperty(field);
            if (property == null)
                Debug.LogWarning($"{_undoName}: {so.targetObject.GetType().Name} no tiene el campo {field}.");
            return property;
        }

        public static Sprite FindSprite(string name, bool reportMissing = true)
        {
            if (string.IsNullOrEmpty(name)) return null;
            if (AssetDatabase.IsValidFolder(SpriteFolder))
            {
                foreach (string guid in AssetDatabase.FindAssets(name + " t:Sprite", new[] { SpriteFolder }))
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    if (System.IO.Path.GetFileNameWithoutExtension(path) != name) continue;
                    Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                    if (sprite != null) return sprite;
                }
            }
            if (reportMissing) MissingSprites.Add(name);
            return null;
        }

        private static string PathOf(Transform t) => t.parent == null ? t.name : PathOf(t.parent) + "/" + t.name;
    }
}
