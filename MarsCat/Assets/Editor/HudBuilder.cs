// Asistencia de IA: escrito con Claude Code (Anthropic).
using System;
using System.Collections.Generic;
using MarsCat.Logic.Resources;
using MarsCat.Unity;
using MarsCat.Unity.UI;
using MarsCat.Unity.View;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace MarsCat.EditorTools
{
    /// <summary>
    /// Tools → Mars Cat → Crear HUD: arma en la escena abierta el Canvas del HUD de Main con la jerarquía
    /// acordada con Design Master (09_interfaz.md) y conecta los scripts de MarsCat.Unity.UI.
    /// Idempotente: busca cada objeto por nombre bajo su padre y, si ya existe, no toca su posición, tamaño ni estilo.
    /// Solo crea lo que falta, completa sprites vacíos y enlaza los campos que estén en null.
    /// Todo va a un solo paso de Undo; la escena queda marcada como modificada, sin guardarse.
    /// Tools → Mars Cat → Marcar texturas de UI como Sprite: importa como Sprite (2D and UI) las texturas de Assets/UI/.
    /// </summary>
    public static class HudBuilder
    {
        private const string SpriteFolder = "Assets/UI";
        private const string UndoName = "Crear HUD";

        private static readonly Vector2 ReferenceResolution = new Vector2(1920f, 1080f);
        private static readonly Color TextColor = new Color32(0xF2, 0xEC, 0xFF, 0xFF);
        private static readonly Color StateColor = new Color32(0x22, 0xE5, 0xF0, 0xFF);
        private static readonly Color MissingSpriteColor = new Color(1f, 1f, 1f, 0.25f);
        private static readonly Color MapColor = new Color(0f, 0f, 0f, 0.4f);

        private static readonly List<string> Created = new List<string>();
        private static readonly List<string> Reused = new List<string>();
        private static readonly SortedSet<string> MissingSprites = new SortedSet<string>();
        private static GameBootstrap _bootstrap;
        private static SelectionController _selection;

        [MenuItem("Tools/Mars Cat/Crear HUD")]
        public static void Build()
        {
            Undo.IncrementCurrentGroup();
            Undo.SetCurrentGroupName(UndoName);
            int group = Undo.GetCurrentGroup();
            Created.Clear();
            Reused.Clear();
            MissingSprites.Clear();

            _bootstrap = Object.FindAnyObjectByType<GameBootstrap>(FindObjectsInactive.Include);
            _selection = Object.FindAnyObjectByType<SelectionController>(FindObjectsInactive.Include);
            if (_bootstrap == null)
                Debug.LogWarning("Crear HUD: no hay GameBootstrap en la escena; los campos Bootstrap quedan vacíos.");
            if (_selection == null)
                Debug.LogWarning("Crear HUD: no hay SelectionController en la escena; DetailPanel.Selection queda vacío.");
            if (TMP_Settings.instance == null || TMP_Settings.defaultFontAsset == null)
                Debug.LogWarning("Crear HUD: faltan los TMP Essential Resources (Window → TextMeshPro → Import TMP Essential Resources). Los textos no se verán.");

            RectTransform canvas = BuildCanvas();
            EnsureEventSystem();

            RectTransform top = Node(canvas, "HUD_Arriba", r => Set(r, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), Vector2.zero, new Vector2(0f, 160f)));
            BuildAiCorner(top);
            BuildResources(top);
            BuildClock(top);
            BuildMaterials(top);
            BuildMissions(top);
            BuildPauseButton(top);

            RectTransform bottom = Node(canvas, "HUD_Abajo", Stretch);
            BuildMinimap(bottom);
            BuildDetailPanel(canvas);

            Undo.CollapseUndoOperations(group);
            EditorSceneManager.MarkSceneDirty(canvas.gameObject.scene);
            Selection.activeGameObject = canvas.gameObject;

            Debug.Log("Crear HUD: listo (la escena no se guardó).\n" +
                      $"Creados ({Created.Count}): {string.Join(", ", Created)}\n" +
                      $"Reutilizados ({Reused.Count}): {string.Join(", ", Reused)}\n" +
                      $"Sprites que faltan en {SpriteFolder}/ ({MissingSprites.Count}): {string.Join(", ", MissingSprites)}");
        }

        [MenuItem("Tools/Mars Cat/Marcar texturas de UI como Sprite")]
        public static void MarkUiTexturesAsSprites()
        {
            if (!AssetDatabase.IsValidFolder(SpriteFolder))
            {
                Debug.LogWarning($"No existe la carpeta {SpriteFolder}.");
                return;
            }
            int changed = 0;
            foreach (string guid in AssetDatabase.FindAssets("t:Texture2D", new[] { SpriteFolder }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (!(AssetImporter.GetAtPath(path) is TextureImporter importer)) continue;
                if (importer.textureType == TextureImporterType.Sprite) continue;
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.alphaIsTransparency = true;
                importer.SaveAndReimport();
                changed++;
            }
            Debug.Log($"Texturas de {SpriteFolder} marcadas como Sprite (2D and UI): {changed}.");
        }

        // ---------------- Canvas y eventos ----------------

        private static RectTransform BuildCanvas()
        {
            Canvas canvas = null;
            foreach (GameObject root in EditorSceneManager.GetActiveScene().GetRootGameObjects())
            {
                if (root.name == "Canvas" && root.GetComponent<Canvas>() != null) { canvas = root.GetComponent<Canvas>(); break; }
                if (canvas == null && root.GetComponent<Canvas>() != null) canvas = root.GetComponent<Canvas>();
            }

            if (canvas != null)
            {
                Reused.Add(canvas.name);
                return (RectTransform)canvas.transform;
            }

            var go = new GameObject("Canvas", typeof(RectTransform));
            Undo.RegisterCreatedObjectUndo(go, UndoName);
            int uiLayer = LayerMask.NameToLayer("UI");
            if (uiLayer >= 0) go.layer = uiLayer;
            canvas = Undo.AddComponent<Canvas>(go);
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = Undo.AddComponent<CanvasScaler>(go);
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = ReferenceResolution;
            scaler.matchWidthOrHeight = 0.5f;
            Undo.AddComponent<GraphicRaycaster>(go);
            Created.Add("Canvas");
            return (RectTransform)go.transform;
        }

        private static void EnsureEventSystem()
        {
            var eventSystem = Object.FindAnyObjectByType<EventSystem>(FindObjectsInactive.Include);
            GameObject go;
            if (eventSystem != null)
            {
                go = eventSystem.gameObject;
                Reused.Add(go.name);
            }
            else
            {
                go = new GameObject("EventSystem");
                Undo.RegisterCreatedObjectUndo(go, UndoName);
                Undo.AddComponent<EventSystem>(go);
                Created.Add("EventSystem");
            }
            var legacy = go.GetComponent<StandaloneInputModule>();
            if (legacy != null)
            {
                Undo.DestroyObjectImmediate(legacy);
                Debug.Log("Crear HUD: StandaloneInputModule reemplazado por InputSystemUIInputModule.");
            }
            if (go.GetComponent<InputSystemUIInputModule>() == null)
                Undo.AddComponent<InputSystemUIInputModule>(go);
        }

        // ---------------- Arriba ----------------

        private static void BuildAiCorner(RectTransform top)
        {
            RectTransform root = Node(top, "EsquinaIA", r => Set(r, TL, TL, TL, new Vector2(20f, -20f), new Vector2(520f, 140f)));
            CanvasGroup group = Add<CanvasGroup>(root, out _);

            RectTransform frame = Node(root, "Marco", r => Set(r, TL, TL, TL, Vector2.zero, new Vector2(120f, 120f)));
            Decor(frame, "12_marco_ia");
            RectTransform portrait = Node(frame, "Retrato", r => Set(r, C, C, C, Vector2.zero, new Vector2(100f, 100f)));
            Image portraitImage = Decor(portrait, "01_robot_ia", preserveAspect: true);

            TMP_Text name = Text(Node(root, "Nombre", r => Set(r, TL, TL, TL, new Vector2(132f, -4f), new Vector2(380f, 28f))),
                22f, FontStyles.Bold, TextAlignmentOptions.TopLeft, "IA");
            RectTransform balloon = Node(root, "Globo", r => Set(r, TL, TL, TL, new Vector2(132f, -36f), new Vector2(380f, 90f)));
            Decor(balloon, "09_panel_detalle", sliced: true);
            TMP_Text speech = Text(Node(balloon, "Texto", r => Margins(r, 12f, 12f, 12f, 12f)),
                18f, FontStyles.Normal, TextAlignmentOptions.TopLeft, "", wrap: true, overflow: TextOverflowModes.Truncate);

            AiCorner ai = Add<AiCorner>(root, out bool isNew);
            Wire(ai, so =>
            {
                Ref(so, "_bootstrap", _bootstrap);
                Ref(so, "_portrait", portraitImage);
                Ref(so, "_name", name);
                Ref(so, "_speech", speech);
                Ref(so, "_group", group);
                if (isNew) Float(so, "_charsPerSecond", 45f);
            });
        }

        private static void BuildResources(RectTransform top)
        {
            RectTransform root = Node(top, "Recursos", r => Set(r, TL, TL, TL, new Vector2(560f, -30f), new Vector2(360f, 64f)));
            Row(root, 24f, TextAnchor.MiddleLeft, false);
            ResourceItem(root, "Comida", ResourceId.Food, "02_comida");
            ResourceItem(root, "Agua", ResourceId.Water, "03_agua");
        }

        private static void BuildClock(RectTransform top)
        {
            RectTransform root = Node(top, "Reloj", r => Set(r, TC, TC, TC, new Vector2(0f, -10f), new Vector2(360f, 180f)));
            Decor(Node(root, "Marco", Stretch), "04_reloj_dia_noche", preserveAspect: true);
            RectTransform needle = Node(root, "Aguja", r => Set(r, BC, BC, BC, new Vector2(0f, 10f), new Vector2(32f, 140f)));
            Decor(needle, "04_reloj_aguja");
            TMP_Text sol = Text(Node(root, "Sol", r => Set(r, BC, BC, BC, new Vector2(0f, 12f), new Vector2(200f, 32f))),
                24f, FontStyles.Bold, TextAlignmentOptions.Center, "Sol 1");
            TMP_Text phase = Text(Node(root, "Fase", r => Set(r, BC, BC, TC, new Vector2(0f, -4f), new Vector2(200f, 24f))),
                16f, FontStyles.Normal, TextAlignmentOptions.Center, "Día");

            HudClock clock = Add<HudClock>(root, out _);
            Wire(clock, so =>
            {
                Ref(so, "_bootstrap", _bootstrap);
                Ref(so, "_solLabel", sol);
                Ref(so, "_phaseLabel", phase);
                Ref(so, "_needle", needle);
            });
        }

        private static void BuildMaterials(RectTransform top)
        {
            RectTransform root = Node(top, "Materiales", r => Set(r, TC, TC, TL, new Vector2(200f, -30f), new Vector2(480f, 64f)));
            Row(root, 24f, TextAnchor.MiddleLeft, false);
            ResourceItem(root, "Metal", ResourceId.Metal, "11_metal");
            ResourceItem(root, "Escombros", ResourceId.Debris, "11_escombros");
            ResourceItem(root, "Abono", ResourceId.Compost, "11_abono");
        }

        private static void BuildMissions(RectTransform top)
        {
            RectTransform root = Node(top, "Misiones", r => Set(r, TR, TR, TR, new Vector2(-20f, -20f), new Vector2(380f, 260f)));
            Decor(Node(root, "Marco", Stretch), "05_tableta_misiones");
            TMP_Text title = Text(Node(root, "Titulo", r => Set(r, TL, TR, TC, new Vector2(0f, -24f), new Vector2(-48f, 32f))),
                22f, FontStyles.Bold, TextAlignmentOptions.Center, "Misiones");
            TMP_Text mission = Text(Node(root, "Mision", r => Margins(r, 24f, 24f, 64f, 24f)),
                18f, FontStyles.Normal, TextAlignmentOptions.TopLeft, "", wrap: true, overflow: TextOverflowModes.Truncate);

            HudMissions missions = Add<HudMissions>(root, out _);
            Wire(missions, so =>
            {
                Ref(so, "_bootstrap", _bootstrap);
                Ref(so, "_title", title);
                Ref(so, "_currentMission", mission);
            });
        }

        private static void BuildPauseButton(RectTransform top)
        {
            RectTransform root = Node(top, "BotonPausa", r => Set(r, TC, TC, TC, new Vector2(0f, -200f), new Vector2(72f, 72f)));
            Image image = ButtonImage(root, "10_boton_pausa", fallbackToDefault: false);
            Button button = Add<Button>(root, out bool isNew);
            if (isNew) button.targetGraphic = image;

            HudPauseButton pause = Add<HudPauseButton>(root, out _);
            Wire(pause, so =>
            {
                Ref(so, "_bootstrap", _bootstrap);
                Ref(so, "_button", button);
            });
        }

        // ---------------- Abajo ----------------

        private static void BuildMinimap(RectTransform bottom)
        {
            RectTransform root = Node(bottom, "Minimapa", r => Set(r, BL, BL, C, new Vector2(190f, 190f), new Vector2(260f, 260f)));

            RectTransform map = Node(root, "Mapa", Stretch);
            RawImage mapImage = Add<RawImage>(map, out bool newMap);
            if (newMap)
            {
                mapImage.color = MapColor;
                mapImage.raycastTarget = false;
            }
            Decor(Node(root, "Aro", Stretch), "06_aro_minimapa");

            Arc(root, "ArcoOxigeno", HudArcBar.Source.Oxygen, true, "07_arco_oxigeno_relleno", "07_arco_oxigeno_marco");
            Arc(root, "ArcoEnergia", HudArcBar.Source.Electricity, false, "08_arco_energia_relleno", "08_arco_energia_marco");
        }

        private static void Arc(RectTransform minimap, string name, HudArcBar.Source source, bool left, string fillSprite, string frameSprite)
        {
            Vector2 pivot = left ? new Vector2(1f, 0f) : new Vector2(0f, 0f);
            RectTransform root = Node(minimap, name, r => Set(r, C, C, pivot, Vector2.zero, new Vector2(190f, 190f)));

            RectTransform fillRect = Node(root, "Relleno", Stretch);
            Image fill = Decor(fillRect, fillSprite, configure: image =>
            {
                image.type = Image.Type.Filled;
                image.fillMethod = Image.FillMethod.Radial90;
                image.fillOrigin = left ? (int)Image.Origin90.BottomRight : (int)Image.Origin90.BottomLeft;
                image.fillAmount = 1f;
            });
            RectTransform frame = Node(root, "Marco", Stretch);
            Decor(frame, frameSprite);
            // El marco se dibuja encima del relleno.
            if (frame.GetSiblingIndex() < fillRect.GetSiblingIndex())
            {
                Undo.SetSiblingIndex(frame, fillRect.GetSiblingIndex(), UndoName);
            }

            Vector2 labelCorner = left ? TL : TR;
            TMP_Text percent = Text(Node(root, "Porcentaje", r => Set(r, labelCorner, labelCorner, labelCorner, new Vector2(0f, 24f), new Vector2(90f, 24f))),
                18f, FontStyles.Bold, left ? TextAlignmentOptions.Left : TextAlignmentOptions.Right, "100%");

            HudArcBar bar = Add<HudArcBar>(root, out bool isNew);
            Wire(bar, so =>
            {
                Ref(so, "_bootstrap", _bootstrap);
                if (isNew) EnumValue(so, "_source", (int)source);
                Ref(so, "_fill", fill);
                Ref(so, "_label", percent);
            });
        }

        private static void BuildDetailPanel(RectTransform canvas)
        {
            RectTransform container = Node(canvas, "DetailPanel", r => Set(r, BR, BR, BR, new Vector2(-20f, 20f), new Vector2(460f, 340f)));
            RectTransform root = Node(container, "Root", Stretch);
            Image background = Decor(root, "09_panel_detalle", sliced: true);
            if (!background.raycastTarget)
            {
                Undo.RecordObject(background, UndoName);
                background.raycastTarget = true;   // bloquea los clics hacia la escena
            }

            TMP_Text title = Text(Node(root, "Titulo", r => Set(r, TL, TR, TC, new Vector2(0f, -20f), new Vector2(-40f, 34f))),
                26f, FontStyles.Bold, TextAlignmentOptions.Left, "Edificio");
            TMP_Text state = Text(Node(root, "Estado", r => Set(r, TL, TR, TC, new Vector2(0f, -58f), new Vector2(-40f, 26f))),
                20f, FontStyles.Normal, TextAlignmentOptions.Left, "Estado", color: StateColor);
            TMP_Text details = Text(Node(root, "Detalles", r => Margins(r, 20f, 20f, 92f, 84f)),
                16f, FontStyles.Normal, TextAlignmentOptions.TopLeft, "", wrap: true, overflow: TextOverflowModes.Truncate);

            RectTransform buttons = Node(root, "Botones", r => Set(r, BL, new Vector2(1f, 0f), BC, new Vector2(0f, 20f), new Vector2(-40f, 48f)));
            Row(buttons, 16f, TextAnchor.MiddleCenter, true);
            (Button repair, TMP_Text repairLabel) = PanelButton(buttons, "Reparar");
            (Button prepare, TMP_Text prepareLabel) = PanelButton(buttons, "Preparar");

            DetailPanel panel = Add<DetailPanel>(container, out bool isNew);
            Wire(panel, so =>
            {
                Ref(so, "_bootstrap", _bootstrap);
                Ref(so, "_selection", _selection);
                Ref(so, "_root", root.gameObject);
                Ref(so, "_title", title);
                Ref(so, "_state", state);
                Ref(so, "_details", details);
                Ref(so, "_repairButton", repair);
                Ref(so, "_repairLabel", repairLabel);
                Ref(so, "_prepareButton", prepare);
                Ref(so, "_prepareLabel", prepareLabel);
                if (isNew) Bool(so, "_debugAutoPrepare", true);
            });
        }

        // ---------------- Piezas ----------------

        private static void ResourceItem(RectTransform parent, string name, ResourceId resource, string sprite)
        {
            RectTransform item = Node(parent, name, r => r.sizeDelta = new Vector2(150f, 64f));
            Row(item, 8f, TextAnchor.MiddleLeft, false);
            Decor(Node(item, "Icono", r => r.sizeDelta = new Vector2(56f, 56f)), sprite, preserveAspect: true);
            TMP_Text value = Text(Node(item, "Valor", r => r.sizeDelta = new Vector2(86f, 56f)),
                28f, FontStyles.Bold, TextAlignmentOptions.Left, "0");

            HudResourceLabel label = Add<HudResourceLabel>(item, out bool isNew);
            Wire(label, so =>
            {
                Ref(so, "_bootstrap", _bootstrap);
                if (isNew)
                {
                    EnumValue(so, "_resource", (int)resource);
                    Bool(so, "_showCapacity", false);
                }
                Ref(so, "_value", value);
            });
        }

        private static (Button, TMP_Text) PanelButton(RectTransform parent, string name)
        {
            RectTransform rect = Node(parent, name, r => r.sizeDelta = new Vector2(180f, 48f));
            Image image = ButtonImage(rect, "10_boton", fallbackToDefault: true);
            Button button = Add<Button>(rect, out bool isNew);
            if (isNew) button.targetGraphic = image;
            TMP_Text label = Text(Node(rect, "Texto", Stretch), 20f, FontStyles.Bold, TextAlignmentOptions.Center, name);
            return (button, label);
        }

        private static Image ButtonImage(RectTransform rect, string spriteName, bool fallbackToDefault)
        {
            Image image = Add<Image>(rect, out bool isNew);
            if (isNew) image.raycastTarget = true;
            if (image.sprite == null)
            {
                Sprite sprite = FindSprite(spriteName);
                if (sprite == null && fallbackToDefault)
                    sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
                if (sprite != null)
                {
                    Undo.RecordObject(image, UndoName);
                    image.sprite = sprite;
                    image.color = Color.white;
                    image.type = sprite.border != Vector4.zero ? Image.Type.Sliced : Image.Type.Simple;
                }
                else if (isNew)
                {
                    image.color = MissingSpriteColor;
                }
            }
            return image;
        }

        /// <summary>Image decorativa (raycast apagado). Si el sprite no existe: blanco al 25 %; se completa al volver a ejecutar.</summary>
        private static Image Decor(RectTransform rect, string spriteName, bool preserveAspect = false, bool sliced = false,
            Action<Image> configure = null)
        {
            Image image = Add<Image>(rect, out bool isNew);
            if (isNew)
            {
                image.raycastTarget = false;
                image.preserveAspect = preserveAspect;
                configure?.Invoke(image);
            }
            if (image.sprite == null)
            {
                Sprite sprite = FindSprite(spriteName);
                Undo.RecordObject(image, UndoName);
                if (sprite != null)
                {
                    image.sprite = sprite;
                    image.color = Color.white;
                    if (sliced && image.type != Image.Type.Filled)
                        image.type = sprite.border != Vector4.zero ? Image.Type.Sliced : Image.Type.Simple;
                }
                else
                {
                    image.color = MissingSpriteColor;
                }
            }
            return image;
        }

        /// <summary>TMP decorativo. El estilo solo se aplica al crearlo, para no pisar ajustes hechos a mano.</summary>
        private static TMP_Text Text(RectTransform rect, float size, FontStyles style, TextAlignmentOptions alignment,
            string initial, bool wrap = false, TextOverflowModes overflow = TextOverflowModes.Ellipsis, Color? color = null)
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
                text.overflowMode = overflow;
                text.text = initial;
            }
            return text;
        }

        private static void Row(RectTransform rect, float spacing, TextAnchor alignment, bool controlAndExpand)
        {
            HorizontalLayoutGroup layout = Add<HorizontalLayoutGroup>(rect, out bool isNew);
            if (!isNew) return;
            layout.spacing = spacing;
            layout.padding = new RectOffset(0, 0, 0, 0);
            layout.childAlignment = alignment;
            layout.childControlWidth = controlAndExpand;
            layout.childControlHeight = controlAndExpand;
            layout.childForceExpandWidth = controlAndExpand;
            layout.childForceExpandHeight = false;
        }

        // ---------------- Utilidades ----------------

        private static readonly Vector2 TL = new Vector2(0f, 1f);
        private static readonly Vector2 TC = new Vector2(0.5f, 1f);
        private static readonly Vector2 TR = new Vector2(1f, 1f);
        private static readonly Vector2 C = new Vector2(0.5f, 0.5f);
        private static readonly Vector2 BL = new Vector2(0f, 0f);
        private static readonly Vector2 BC = new Vector2(0.5f, 0f);
        private static readonly Vector2 BR = new Vector2(1f, 0f);

        /// <summary>Hijo por nombre; si no existe lo crea y le aplica la distribución. Si existe, no lo toca.</summary>
        private static RectTransform Node(RectTransform parent, string name, Action<RectTransform> layoutWhenCreated)
        {
            string path = PathOf(parent) + "/" + name;
            Transform child = parent.Find(name);
            if (child != null)
            {
                Reused.Add(path);
                if (child is RectTransform existing) return existing;
                Debug.LogWarning($"Crear HUD: {path} existe pero no tiene RectTransform.");
            }

            var go = new GameObject(name, typeof(RectTransform));
            Undo.RegisterCreatedObjectUndo(go, UndoName);
            Undo.SetTransformParent(go.transform, parent, UndoName);
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

        private static void Set(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 position, Vector2 size)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = pivot;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }

        private static void Stretch(RectTransform rect) => Margins(rect, 0f, 0f, 0f, 0f);

        private static void Margins(RectTransform rect, float left, float right, float top, float bottom)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = C;
            rect.offsetMin = new Vector2(left, bottom);
            rect.offsetMax = new Vector2(-right, -top);
        }

        private static string PathOf(Transform t) => t.parent == null ? t.name : PathOf(t.parent) + "/" + t.name;

        private static T Add<T>(Component owner, out bool isNew) where T : Component
        {
            T component = owner.GetComponent<T>();
            isNew = component == null;
            return isNew ? Undo.AddComponent<T>(owner.gameObject) : component;
        }

        // Nombres reales de los archivos de arte (Art-source/UI → Assets/UI) para cada nombre de la especificación.
        // Se prueba primero el nombre de la especificación y después sus alias, en orden.
        private static readonly Dictionary<string, string[]> SpriteAliases = new Dictionary<string, string[]>
        {
            { "02_comida", new[] { "02_icono_comida" } },
            { "04_reloj_dia_noche", new[] { "04_reloj_de_dia_noche" } },
            { "04_reloj_aguja", new[] { "04_aguja_reloj" } },
            { "08_arco_energia_relleno", new[] { "07_arco_electricidad_relleno", "08_arco_electricidad_relleno" } },
            { "08_arco_energia_marco", new[] { "08_arco_electricidad_marco", "07_arco_electricidad_marco" } },
            { "09_panel_detalle", new[] { "09_panel_detalles" } },
            { "10_boton_pausa", new[] { "boton_pausa" } },
            { "11_metal", new[] { "10_metal" } },
            { "11_escombros", new[] { "10_escombros" } },
            { "11_abono", new[] { "10_abono" } },
            { "12_marco_ia", new[] { "marco_ia" } },
        };

        private static Sprite FindSprite(string name)
        {
            if (string.IsNullOrEmpty(name)) return null;
            Sprite sprite = FindSpriteExact(name);
            if (sprite == null && SpriteAliases.TryGetValue(name, out string[] aliases))
            {
                foreach (string alias in aliases)
                {
                    sprite = FindSpriteExact(alias);
                    if (sprite != null) break;
                }
            }
            if (sprite == null) MissingSprites.Add(name);
            return sprite;
        }

        private static Sprite FindSpriteExact(string name)
        {
            if (!AssetDatabase.IsValidFolder(SpriteFolder)) return null;
            foreach (string guid in AssetDatabase.FindAssets(name + " t:Sprite", new[] { SpriteFolder }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (System.IO.Path.GetFileNameWithoutExtension(path) != name) continue;
                Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                if (sprite != null) return sprite;
            }
            return null;
        }

        private static void Wire(Component component, Action<SerializedObject> assign)
        {
            var so = new SerializedObject(component);
            assign(so);
            so.ApplyModifiedProperties();   // queda en el mismo paso de Undo
        }

        /// <summary>Enlaza el campo solo si está vacío, para no pisar lo que se haya asignado a mano.</summary>
        private static void Ref(SerializedObject so, string field, Object value)
        {
            SerializedProperty property = Find(so, field);
            if (property == null || property.objectReferenceValue != null || value == null) return;
            property.objectReferenceValue = value;
        }

        private static void EnumValue(SerializedObject so, string field, int value)
        {
            SerializedProperty property = Find(so, field);
            if (property != null) property.enumValueIndex = value;
        }

        private static void Bool(SerializedObject so, string field, bool value)
        {
            SerializedProperty property = Find(so, field);
            if (property != null) property.boolValue = value;
        }

        private static void Float(SerializedObject so, string field, float value)
        {
            SerializedProperty property = Find(so, field);
            if (property != null) property.floatValue = value;
        }

        private static SerializedProperty Find(SerializedObject so, string field)
        {
            SerializedProperty property = so.FindProperty(field);
            if (property == null)
                Debug.LogWarning($"Crear HUD: {so.targetObject.GetType().Name} no tiene el campo {field}.");
            return property;
        }
    }
}
