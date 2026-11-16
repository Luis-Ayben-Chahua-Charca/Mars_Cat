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
    /// Tools → Mars Cat → Minijuegos → Sopa de Bacterias: arma Minijuegos/Minijuego_SopaBacterias con las dos fases
    /// (Fase_Mezcla y Fase_Sopa, hijos de Root), agrega CompostView, enlaza sus campos y lo registra en el host
    /// para "organic_recycler". Idempotente: lo que ya existe no se toca.
    /// </summary>
    public static class CompostBuilder
    {
        private const float RowWidth = 920f;
        private static readonly Color DarkTrack = new Color(1f, 1f, 1f, 0.1f);
        private static readonly Color DangerColor = new Color(0.95f, 0.25f, 0.2f, 0.9f);
        private static readonly Color NeedleColor = Color.white;

        [MenuItem("Tools/Mars Cat/Minijuegos/Sopa de Bacterias")]
        public static void Build()
        {
            Begin("Crear minijuego Sopa de Bacterias");
            (RectTransform container, RectTransform root) = MinigameRoot("Minijuego_SopaBacterias", "Operación Sopa de Bacterias");

            // ---- Fase 1: mezcla ----
            RectTransform mix = Node(root, "Fase_Mezcla", r => Margins(r, 40f, 40f, 80f, 56f));
            var sliders = new Object[3];
            var labels = new Object[3];
            var amounts = new Object[3];
            string[] names = { "Desechos de gatos", "Cama de ratas", "Restos de cosecha" };
            for (int i = 0; i < 3; i++)
            {
                RectTransform row = Node(mix, "Insumo_" + (i + 1), At(TL, new Vector2(0f, -i * 70f), new Vector2(RowWidth, 60f)));
                labels[i] = Text(Node(row, "Etiqueta", r => Set(r, TL, TL, TL, Vector2.zero, new Vector2(340f, 60f))),
                    22f, FontStyles.Normal, TextAlignmentOptions.MidlineLeft, names[i]);
                sliders[i] = Slider(Node(row, "Slider", r => Set(r, TL, TL, TL, new Vector2(360f, -15f), new Vector2(400f, 30f))), 0f, 1f, 0f);
                amounts[i] = Text(Node(row, "Cantidad", r => Set(r, TR, TR, TR, Vector2.zero, new Vector2(150f, 60f))),
                    22f, FontStyles.Normal, TextAlignmentOptions.MidlineRight, "0");
            }
            TMP_Text ratio = Text(Node(mix, "Relacion", At(TL, new Vector2(0f, -230f), new Vector2(RowWidth, 40f))),
                28f, FontStyles.Bold, TextAlignmentOptions.MidlineLeft, "Relación C:N");
            TMP_Text range = Text(Node(mix, "Rango", At(TL, new Vector2(0f, -275f), new Vector2(RowWidth, 32f))),
                20f, FontStyles.Normal, TextAlignmentOptions.MidlineLeft, "Meta");
            TMP_Text total = Text(Node(mix, "Total", At(TL, new Vector2(0f, -312f), new Vector2(RowWidth, 32f))),
                20f, FontStyles.Normal, TextAlignmentOptions.MidlineLeft, "Total");
            TMP_Text mixMessage = Text(Node(mix, "Mensaje", At(TL, new Vector2(0f, -355f), new Vector2(RowWidth, 70f))),
                22f, FontStyles.Italic, TextAlignmentOptions.TopLeft, "", wrap: true, color: AccentColor);
            (Button confirm, TMP_Text confirmLabel) = Button(Node(mix, "BotonCargar", At(BC, new Vector2(0f, 6f), new Vector2(300f, 56f))), "Cargar mezcla");

            // ---- Fase 2: sopa ----
            RectTransform soup = Node(root, "Fase_Sopa", r => Margins(r, 40f, 40f, 80f, 56f));
            TMP_Text tempText = Text(Node(soup, "TextoTemperatura", At(TL, Vector2.zero, new Vector2(RowWidth, 34f))),
                24f, FontStyles.Bold, TextAlignmentOptions.MidlineLeft, "Temperatura");
            RectTransform tempTrack = Node(soup, "PistaTemperatura", At(TL, new Vector2(0f, -44f), new Vector2(RowWidth, 36f)));
            Panel(Node(tempTrack, "Fondo", Stretch), DarkTrack);
            RectTransform tempGreen = Node(tempTrack, "ZonaVerde", Stretch);
            Panel(tempGreen, GreenZoneColor);
            RectTransform tempOverheat = Mark(tempTrack, "MarcaSobrecalentamiento", DangerColor);
            RectTransform tempNeedle = Needle(tempTrack);

            TMP_Text humidityText = Text(Node(soup, "TextoHumedad", At(TL, new Vector2(0f, -100f), new Vector2(RowWidth, 34f))),
                24f, FontStyles.Bold, TextAlignmentOptions.MidlineLeft, "Humedad");
            RectTransform humidityTrack = Node(soup, "PistaHumedad", At(TL, new Vector2(0f, -144f), new Vector2(RowWidth, 36f)));
            Panel(Node(humidityTrack, "Fondo", Stretch), DarkTrack);
            RectTransform humidityGreen = Node(humidityTrack, "ZonaVerde", Stretch);
            Panel(humidityGreen, GreenZoneColor);
            RectTransform humidityDry = Mark(humidityTrack, "MarcaSeca", DangerColor);
            RectTransform humidityDrown = Mark(humidityTrack, "MarcaAhogo", DangerColor);
            RectTransform humidityNeedle = Needle(humidityTrack);

            TMP_Text heatLabel = Text(Node(soup, "TextoCalor", At(TL, new Vector2(0f, -205f), new Vector2(300f, 32f))),
                22f, FontStyles.Bold, TextAlignmentOptions.MidlineLeft, "Calor");
            Image heatFill = Bar(Node(soup, "BarraCalor", At(TL, new Vector2(0f, -243f), new Vector2(420f, 24f))), ButtonColor);
            TMP_Text heatKeys = Text(Node(soup, "TeclasCalor", At(TL, new Vector2(440f, -239f), new Vector2(240f, 32f))),
                20f, FontStyles.Bold, TextAlignmentOptions.MidlineLeft, "Q / E", color: AccentColor);
            Image lamp = Panel(Node(soup, "LuzAspersor", At(TL, new Vector2(0f, -287f), new Vector2(28f, 28f))), Color.white);
            TMP_Text sprinklerText = Text(Node(soup, "TextoAspersor", At(TL, new Vector2(40f, -285f), new Vector2(380f, 32f))),
                22f, FontStyles.Bold, TextAlignmentOptions.MidlineLeft, "Aspersor");
            TMP_Text sprinklerKeys = Text(Node(soup, "TeclasAspersor", At(TL, new Vector2(440f, -285f), new Vector2(240f, 32f))),
                20f, FontStyles.Bold, TextAlignmentOptions.MidlineLeft, "Espacio", color: AccentColor);
            TMP_Text warning = Text(Node(soup, "AvisoIA", At(TR, new Vector2(0f, -205f), new Vector2(230f, 110f))),
                22f, FontStyles.Bold, TextAlignmentOptions.TopRight, "", wrap: true, color: AccentColor);

            TMP_Text timeText = Text(Node(soup, "TextoTiempo", At(TL, new Vector2(0f, -345f), new Vector2(RowWidth, 30f))),
                20f, FontStyles.Normal, TextAlignmentOptions.MidlineLeft, "Tiempo en verde");
            Image timeBar = Bar(Node(soup, "BarraTiempo", At(TL, new Vector2(0f, -380f), new Vector2(RowWidth, 24f))), GreenZoneColor);
            TMP_Text yieldText = Text(Node(soup, "TextoRendimiento", At(TL, new Vector2(0f, -412f), new Vector2(RowWidth, 30f))),
                22f, FontStyles.Bold, TextAlignmentOptions.MidlineLeft, "Rendimiento previsto");

            RectTransform resultGroup = Node(soup, "Resultado", Stretch);
            Image resultBackground = Panel(resultGroup, new Color(0.07f, 0.06f, 0.09f, 0.97f));
            TMP_Text resultText = Text(Node(resultGroup, "TextoResultado", At(C, new Vector2(0f, 60f), new Vector2(760f, 160f))),
                30f, FontStyles.Bold, TextAlignmentOptions.Center, "", wrap: true);
            (Button continueButton, TMP_Text continueLabel) = Button(Node(resultGroup, "BotonContinuar", At(C, new Vector2(0f, -80f), new Vector2(300f, 56f))), "Continuar");
            if (resultBackground != null) resultGroup.gameObject.SetActive(false);

            TMP_Text title = root.Find("Titulo") != null ? root.Find("Titulo").GetComponent<TMP_Text>() : null;

            // ---- Vista y enlaces ----
            var view = Add<CompostView>(container, out _);
            Wire(view, so =>
            {
                Ref(so, "_root", root.gameObject);
                Ref(so, "_mixPhase", mix.gameObject);
                Ref(so, "_soupPhase", soup.gameObject);
                Ref(so, "_titleText", title);
                RefArray(so, "_inputSliders", sliders);
                RefArray(so, "_inputLabels", labels);
                RefArray(so, "_inputAmountTexts", amounts);
                Ref(so, "_ratioText", ratio);
                Ref(so, "_rangeText", range);
                Ref(so, "_totalText", total);
                Ref(so, "_mixMessageText", mixMessage);
                Ref(so, "_confirmButton", confirm);
                Ref(so, "_confirmLabel", confirmLabel);
                Ref(so, "_tempText", tempText);
                Ref(so, "_tempGreenZone", tempGreen);
                Ref(so, "_tempOverheatMark", tempOverheat);
                Ref(so, "_tempNeedle", tempNeedle);
                Ref(so, "_humidityText", humidityText);
                Ref(so, "_humidityGreenZone", humidityGreen);
                Ref(so, "_humidityDryMark", humidityDry);
                Ref(so, "_humidityDrownMark", humidityDrown);
                Ref(so, "_humidityNeedle", humidityNeedle);
                Ref(so, "_heatFill", heatFill);
                Ref(so, "_heatText", heatLabel);
                Ref(so, "_heatKeysText", heatKeys);
                Ref(so, "_sprinklerLamp", lamp);
                Ref(so, "_sprinklerText", sprinklerText);
                Ref(so, "_sprinklerKeysText", sprinklerKeys);
                Ref(so, "_warningText", warning);
                Ref(so, "_timeBar", timeBar);
                Ref(so, "_timeText", timeText);
                Ref(so, "_yieldText", yieldText);
                Ref(so, "_resultGroup", resultGroup.gameObject);
                Ref(so, "_resultText", resultText);
                Ref(so, "_continueButton", continueButton);
                Ref(so, "_continueLabel", continueLabel);
            });

            // Las dos fases arrancan ocultas hasta que la vista abra el panel.
            soup.gameObject.SetActive(false);
            Register(view, "organic_recycler");
            End();
        }

        // Marca vertical fina que el script coloca a lo largo de la pista.
        private static RectTransform Mark(RectTransform track, string name, Color color)
        {
            RectTransform mark = Node(track, name, r => Set(r, new Vector2(0f, 0f), new Vector2(0f, 1f), C, Vector2.zero, new Vector2(4f, 0f)));
            Panel(mark, color);
            return mark;
        }

        // Aguja: sobresale de la pista por arriba y por abajo.
        private static RectTransform Needle(RectTransform track)
        {
            RectTransform needle = Node(track, "Aguja", r => Set(r, new Vector2(0f, 0f), new Vector2(0f, 1f), C, Vector2.zero, new Vector2(8f, 14f)));
            Panel(needle, NeedleColor);
            return needle;
        }
    }
}
