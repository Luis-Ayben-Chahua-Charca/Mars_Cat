// Asistencia de IA: escrito con Claude Code (Anthropic).
using MarsCat.Unity.Minigames;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static MarsCat.EditorTools.MinigameUiKit;

namespace MarsCat.EditorTools
{
    /// <summary>
    /// Tools → Mars Cat → Minijuegos → Camara de derretido: arma Minijuegos/Minijuego_CamaraDerretido (dial con aguja
    /// y zona verde, círculo de progreso, acelerar, avisos y panel de resultado), enlaza la vista y la registra en el
    /// MinigameHost para "water_extractor". Idempotente; sprites opcionales (si faltan, formas lisas).
    /// </summary>
    public static class MeltingChamberBuilder
    {
        private const string UndoName = "Crear minijuego Camara de derretido";
        private static readonly Color DialColor = new Color(1f, 1f, 1f, 0.14f);
        private static readonly Color NeedleColor = new Color(1f, 0.55f, 0.2f, 1f);
        private static readonly Color RingColor = new Color(1f, 1f, 1f, 0.12f);

        [MenuItem("Tools/Mars Cat/Minijuegos/Camara de derretido")]
        public static void Build()
        {
            Begin(UndoName);

            var (container, root) = MinigameRoot("Minijuego_CamaraDerretido", "Cámara de derretido");

            TMP_Text phase = Text(Node(root, "Fase", r => Set(r, TL, TR, TC, new Vector2(0f, -70f), new Vector2(-40f, 40f))),
                26f, FontStyles.Bold, TextAlignmentOptions.Center, "Fase");

            // Dial (media circunferencia superior de un círculo): fondo, zona verde y aguja.
            RectTransform dial = Node(root, "Dial", At(C, new Vector2(-230f, -20f), new Vector2(380f, 380f)));
            Radial(Node(dial, "Fondo", Stretch), "melt_dial", DialColor, Image.FillMethod.Radial180);
            Image zone = Radial(Node(dial, "ZonaVerde", Stretch), "melt_green_zone", GreenZoneColor, Image.FillMethod.Radial180);
            RectTransform needle = Node(dial, "Aguja", r => Set(r, C, C, new Vector2(0.5f, 0f), Vector2.zero, new Vector2(10f, 170f)));
            Image needleImage = Decor(needle, "melt_needle");
            if (needleImage.sprite == null) needleImage.color = NeedleColor;
            Decor(Node(dial, "Pivote", At(C, Vector2.zero, new Vector2(28f, 28f))), "melt_pivot");

            TMP_Text hint = Text(Node(root, "Controles", At(C, new Vector2(-230f, -60f), new Vector2(440f, 50f))),
                18f, FontStyles.Normal, TextAlignmentOptions.Center, "A / D", wrap: true);

            // Acelerar.
            var accel = Button(Node(root, "BotonAcelerar", At(C, new Vector2(-230f, -135f), new Vector2(300f, 50f))), "Acelerar");
            TMP_Text accelText = Text(Node(root, "TextoAcelerar", At(C, new Vector2(-230f, -190f), new Vector2(440f, 30f))),
                18f, FontStyles.Normal, TextAlignmentOptions.Center, "");

            // Progreso.
            RectTransform progress = Node(root, "Progreso", At(C, new Vector2(230f, 20f), new Vector2(240f, 240f)));
            Radial(Node(progress, "Fondo", Stretch), "melt_progress_bg", RingColor, Image.FillMethod.Radial360, 1f);
            Image fill = Radial(Node(progress, "Relleno", Stretch), "melt_progress", AccentColor, Image.FillMethod.Radial360, 0.4f);

            TMP_Text message = Text(Node(root, "Mensaje", At(C, new Vector2(0f, -250f), new Vector2(900f, 36f))),
                22f, FontStyles.Bold, TextAlignmentOptions.Center, "");

            // Panel de resultado (oculto hasta que termina la sesión).
            RectTransform resultRect = Node(root, "Resultado", Stretch);
            Image resultBg = Panel(resultRect, new Color(0.05f, 0.04f, 0.07f, 0.96f));
            if (!resultBg.raycastTarget)
            {
                Undo.RecordObject(resultBg, UndoName);
                resultBg.raycastTarget = true;
            }
            TMP_Text resultText = Text(Node(resultRect, "Texto", At(C, new Vector2(0f, 60f), new Vector2(800f, 120f))),
                30f, FontStyles.Bold, TextAlignmentOptions.Center, "Resultado", wrap: true);
            var cont = Button(Node(resultRect, "BotonContinuar", At(C, new Vector2(0f, -80f), new Vector2(260f, 56f))), "Continuar");
            var retry = Button(Node(resultRect, "BotonReintentar", At(C, new Vector2(-150f, -80f), new Vector2(260f, 56f))), "Reintentar");
            var exit = Button(Node(resultRect, "BotonSalir", At(C, new Vector2(150f, -80f), new Vector2(260f, 56f))), "Salir");
            resultRect.gameObject.SetActive(false);

            var view = Add<MeltingChamberView>(container, out _);
            Wire(view, so =>
            {
                Ref(so, "_root", root.gameObject);
                Ref(so, "_phaseText", phase);
                Ref(so, "_hintText", hint);
                Ref(so, "_accelText", accelText);
                Ref(so, "_messageText", message);
                Ref(so, "_needle", needle);
                Ref(so, "_greenZone", zone);
                Ref(so, "_progressFill", fill);
                Ref(so, "_accelButton", accel.button);
                Ref(so, "_accelButtonLabel", accel.label);
                Ref(so, "_resultPanel", resultRect.gameObject);
                Ref(so, "_resultText", resultText);
                Ref(so, "_continueButton", cont.button);
                Ref(so, "_continueLabel", cont.label);
                Ref(so, "_retryButton", retry.button);
                Ref(so, "_retryLabel", retry.label);
                Ref(so, "_exitButton", exit.button);
                Ref(so, "_exitLabel", exit.label);
            });
            Register(view, "water_extractor");

            End();
        }

        // Image Filled radial: sprite opcional por nombre; si falta, el círculo (Knob) integrado de Unity.
        private static Image Radial(RectTransform rect, string spriteName, Color color, Image.FillMethod method, float amount = 1f)
        {
            Image image = Add<Image>(rect, out bool isNew);
            if (!isNew) return image;
            Sprite sprite = FindSprite(spriteName);
            bool custom = sprite != null;
            image.sprite = custom ? sprite : AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
            image.color = custom ? Color.white : color;
            image.raycastTarget = false;
            image.type = Image.Type.Filled;
            image.fillMethod = method;
            image.fillOrigin = method == Image.FillMethod.Radial180 ? (int)Image.Origin180.Bottom : (int)Image.Origin360.Top;
            image.fillClockwise = true;
            image.fillAmount = amount;
            return image;
        }
    }
}
