// Asistencia de IA: escrito con Claude Code (Anthropic).
using MarsCat.Unity.Minigames;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace MarsCat.EditorTools
{
    /// <summary>
    /// Arma Minijuegos/Minijuego_RescateSolar (Operación Rescate Solar, edificio solar_panels).
    /// Idempotente: reutiliza lo que ya existe y solo enlaza campos vacíos. No guarda la escena.
    /// </summary>
    public static class SolarCleaningBuilder
    {
        private const int Lanes = 3;
        private const float CellSize = 150f;
        private const float CellGap = 40f;
        private const string UndoName = "Crear minijuego Rescate Solar";

        [MenuItem("Tools/Mars Cat/Minijuegos/Rescate Solar")]
        public static void Build()
        {
            MinigameUiKit.Begin(UndoName);

            var (container, root) = MinigameUiKit.MinigameRoot("Minijuego_RescateSolar", "Operación Rescate Solar");

            // Panel solar de fondo (sprite opcional) con los 3 carriles encima.
            MinigameUiKit.Decor(MinigameUiKit.Node(root, "PanelSolar",
                MinigameUiKit.At(MinigameUiKit.C, new Vector2(0f, 20f), new Vector2(3f * CellSize + 4f * CellGap, 330f))), "Panel_solar");

            var cells = new Image[Lanes];
            var flashes = new Image[Lanes];
            var buttons = new Button[Lanes];
            string[] keys = { "Q", "W", "E" };
            for (int i = 0; i < Lanes; i++)
            {
                float x = (i - 1) * (CellSize + CellGap);
                RectTransform lane = MinigameUiKit.Node(root, "Carril_" + (i + 1),
                    MinigameUiKit.At(MinigameUiKit.C, new Vector2(x, 20f), new Vector2(CellSize, 320f)));

                RectTransform cell = MinigameUiKit.Node(lane, "Celda",
                    MinigameUiKit.At(MinigameUiKit.TC, new Vector2(0f, -10f), new Vector2(CellSize - 10f, CellSize - 10f)));
                cells[i] = MinigameUiKit.Decor(cell, "Manchas_tierra");

                RectTransform nozzle = MinigameUiKit.Node(lane, "Boquilla",
                    MinigameUiKit.At(MinigameUiKit.C, new Vector2(0f, -30f), new Vector2(40f, 70f)));
                MinigameUiKit.Panel(nozzle, new Color(0.6f, 0.6f, 0.65f, 1f));
                RectTransform flash = MinigameUiKit.Node(nozzle, "Destello",
                    MinigameUiKit.At(MinigameUiKit.TC, new Vector2(0f, 50f), new Vector2(90f, 90f)));
                flashes[i] = MinigameUiKit.Panel(flash, new Color(0.6f, 0.95f, 1f, 0f));

                RectTransform button = MinigameUiKit.Node(lane, "BotonDisparo",
                    MinigameUiKit.At(MinigameUiKit.BC, new Vector2(0f, 0f), new Vector2(CellSize, 54f)));
                buttons[i] = MinigameUiKit.Button(button, keys[i]).button;
            }

            // Barra de tiempo de la fila.
            RectTransform barRect = MinigameUiKit.Node(root, "BarraTiempoFila",
                MinigameUiKit.At(MinigameUiKit.TC, new Vector2(0f, -78f), new Vector2(560f, 22f)));
            Image rowBar = MinigameUiKit.Bar(barRect, MinigameUiKit.AccentColor);

            // Textos de estado.
            TMP_Text rows = MinigameUiKit.Text(MinigameUiKit.Node(root, "TextoFilas",
                MinigameUiKit.At(MinigameUiKit.TL, new Vector2(30f, -110f), new Vector2(300f, 30f))),
                22f, FontStyles.Bold, TextAlignmentOptions.Left, "Filas restantes: 0");
            TMP_Text efficiency = MinigameUiKit.Text(MinigameUiKit.Node(root, "TextoEficiencia",
                MinigameUiKit.At(MinigameUiKit.TR, new Vector2(-30f, -110f), new Vector2(300f, 30f))),
                22f, FontStyles.Bold, TextAlignmentOptions.Right, "Eficiencia: 0%");
            TMP_Text energy = MinigameUiKit.Text(MinigameUiKit.Node(root, "TextoEnergia",
                MinigameUiKit.At(MinigameUiKit.BL, new Vector2(30f, 50f), new Vector2(360f, 30f))),
                20f, FontStyles.Normal, TextAlignmentOptions.Left, "Electricidad: 0 kWh");
            TMP_Text warning = MinigameUiKit.Text(MinigameUiKit.Node(root, "AvisoSinEnergia",
                MinigameUiKit.At(MinigameUiKit.BC, new Vector2(0f, 90f), new Vector2(560f, 34f))),
                24f, FontStyles.Bold, TextAlignmentOptions.Center, "Sin electricidad", false, new Color(1f, 0.45f, 0.35f, 1f));
            if (warning.gameObject.activeSelf)
            {
                Undo.RecordObject(warning.gameObject, UndoName);
                warning.gameObject.SetActive(false);
            }

            // Panel de resultado (tapa el área de juego; el botón Continuar entrega el resultado).
            RectTransform resultRect = MinigameUiKit.Node(root, "PanelResultado",
                MinigameUiKit.At(MinigameUiKit.C, Vector2.zero, new Vector2(560f, 300f)));
            Image resultBg = MinigameUiKit.Panel(resultRect, new Color(0.05f, 0.05f, 0.08f, 0.97f));
            if (!resultBg.raycastTarget)
            {
                Undo.RecordObject(resultBg, UndoName);
                resultBg.raycastTarget = true;
            }
            TMP_Text result = MinigameUiKit.Text(MinigameUiKit.Node(resultRect, "TextoResultado",
                MinigameUiKit.At(MinigameUiKit.TC, new Vector2(0f, -50f), new Vector2(500f, 80f))),
                30f, FontStyles.Bold, TextAlignmentOptions.Center, "Panel limpio al 0%", true);
            var (continueButton, continueLabel) = MinigameUiKit.Button(MinigameUiKit.Node(resultRect, "BotonContinuar",
                MinigameUiKit.At(MinigameUiKit.BC, new Vector2(0f, 40f), new Vector2(240f, 56f))), "Continuar");
            if (resultRect.gameObject.activeSelf)
            {
                Undo.RecordObject(resultRect.gameObject, UndoName);
                resultRect.gameObject.SetActive(false);
            }

            var view = MinigameUiKit.Add<SolarCleaningView>(container, out _);
            Sprite dirtySprite = MinigameUiKit.FindSprite("Manchas_tierra", false);
            Sprite cleanSprite = MinigameUiKit.FindSprite("Panel_solar", false);
            MinigameUiKit.Wire(view, so =>
            {
                MinigameUiKit.Ref(so, "_root", root.gameObject);
                MinigameUiKit.RefArray(so, "_cells", ToObjects(cells));
                MinigameUiKit.RefArray(so, "_nozzleFlashes", ToObjects(flashes));
                MinigameUiKit.RefArray(so, "_fireButtons", ToObjects(buttons));
                MinigameUiKit.Ref(so, "_rowTimeBar", rowBar);
                MinigameUiKit.Ref(so, "_rowsText", rows);
                MinigameUiKit.Ref(so, "_efficiencyText", efficiency);
                MinigameUiKit.Ref(so, "_energyText", energy);
                MinigameUiKit.Ref(so, "_warningText", warning);
                MinigameUiKit.Ref(so, "_resultPanel", resultRect.gameObject);
                MinigameUiKit.Ref(so, "_resultText", result);
                MinigameUiKit.Ref(so, "_continueButton", continueButton);
                MinigameUiKit.Ref(so, "_continueLabel", continueLabel);
                MinigameUiKit.Ref(so, "_dirtySprite", dirtySprite);
                MinigameUiKit.Ref(so, "_cleanSprite", cleanSprite);
            });
            MinigameUiKit.Register(view, "solar_panels");

            MinigameUiKit.End();
        }

        private static Object[] ToObjects<T>(T[] items) where T : Object
        {
            var list = new Object[items.Length];
            for (int i = 0; i < items.Length; i++) list[i] = items[i];
            return list;
        }
    }
}
