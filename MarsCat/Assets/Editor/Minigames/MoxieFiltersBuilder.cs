// Asistencia de IA: escrito con Claude Code (Anthropic).
using System.Collections.Generic;
using MarsCat.Unity.Minigames;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using K = MarsCat.EditorTools.MinigameUiKit;

namespace MarsCat.EditorTools
{
    /// <summary>
    /// Tools → Mars Cat → Minijuegos → Filtros del MOXIE. Arma Minijuegos/Minijuego_FiltrosMoxie (5 filtros,
    /// barra de eficiencia, botones de limpieza, "Poner en marcha" y la lista para liberar gatos), agrega
    /// MoxieFiltersView, enlaza sus campos y lo registra en el MinigameHost para el edificio "moxie".
    /// Idempotente: lo que ya existe no se toca.
    /// </summary>
    public static class MoxieFiltersBuilder
    {
        private const int FilterIcons = 5;
        private const int WorkerRows = 6;
        private static readonly int[] CleanCounts = { 1, 2, 5 };

        [MenuItem("Tools/Mars Cat/Minijuegos/Filtros del MOXIE")]
        public static void Build()
        {
            K.Begin("Crear minijuego Filtros del MOXIE");

            (RectTransform container, RectTransform root) = K.MinigameRoot("Minijuego_FiltrosMoxie", "Filtros del MOXIE");

            // Filtros
            RectTransform filters = K.Node(root, "Filtros", K.At(K.TC, new Vector2(0f, -70f), new Vector2(700f, 100f)));
            K.Row(filters, 24f, TextAnchor.MiddleCenter, false);
            var icons = new List<Object>();
            for (int i = 0; i < FilterIcons; i++)
            {
                RectTransform icon = K.Node(filters, "Filtro_" + (i + 1), r => r.sizeDelta = new Vector2(90f, 90f));
                icons.Add(K.Panel(icon, new Color(0.45f, 0.32f, 0.22f, 1f)));
            }

            // Eficiencia y estado
            RectTransform bar = K.Node(root, "BarraEficiencia", K.At(K.TC, new Vector2(0f, -180f), new Vector2(600f, 28f)));
            Image fill = K.Bar(bar, K.AccentColor);
            TMP_Text efficiencyText = K.Text(K.Node(root, "TextoEficiencia", K.At(K.TC, new Vector2(0f, -214f), new Vector2(600f, 30f))),
                22f, FontStyles.Normal, TextAlignmentOptions.Center, "Eficiencia");
            TMP_Text status = K.Text(K.Node(root, "TextoEstado", K.At(K.TC, new Vector2(0f, -246f), new Vector2(800f, 30f))),
                20f, FontStyles.Italic, TextAlignmentOptions.Center, "");

            // Poner en marcha
            RectTransform startGroup = K.Node(root, "PonerEnMarcha", K.At(K.TC, new Vector2(0f, -290f), new Vector2(400f, 64f)));
            (Button startButton, TMP_Text startLabel) = K.Button(K.Node(startGroup, "Boton", K.Stretch), "Poner en marcha");

            // Mantenimiento
            RectTransform maintenance = K.Node(root, "Mantenimiento", K.At(K.TC, new Vector2(0f, -290f), new Vector2(700f, 64f)));
            K.Row(maintenance, 20f, TextAnchor.MiddleCenter, false);
            var cleanButtons = new List<Object>();
            var cleanLabels = new List<Object>();
            foreach (int count in CleanCounts)
            {
                RectTransform node = K.Node(maintenance, "Limpiar_" + count, r => r.sizeDelta = new Vector2(210f, 60f));
                (Button button, TMP_Text label) = K.Button(node, "Limpiar " + count);
                cleanButtons.Add(button);
                cleanLabels.Add(label);
            }

            // Elegir de qué edificio sale el gato
            RectTransform freeCats = K.Node(root, "GatosLibres", K.At(K.TC, new Vector2(0f, -366f), new Vector2(900f, 228f)));
            TMP_Text message = K.Text(K.Node(freeCats, "Mensaje", r => K.Set(r, K.TL, K.TR, K.TC, Vector2.zero, new Vector2(0f, 24f))),
                20f, FontStyles.Normal, TextAlignmentOptions.Center, "No hay gatos libres");
            RectTransform rows = K.Node(freeCats, "Filas", r => K.Set(r, K.TL, K.TR, K.TC, new Vector2(0f, -28f), new Vector2(0f, 194f)));
            K.Column(rows, 2f, TextAnchor.UpperCenter, false);
            var rowRoots = new List<GameObject>();
            var rowLabels = new List<TMP_Text>();
            var rowButtons = new List<Button>();
            var rowButtonLabels = new List<TMP_Text>();
            for (int i = 0; i < WorkerRows; i++)
            {
                RectTransform row = K.Node(rows, "Fila_" + (i + 1), r => r.sizeDelta = new Vector2(880f, 30f));
                TMP_Text label = K.Text(K.Node(row, "Nombre", r => K.Margins(r, 10f, 200f, 0f, 0f)),
                    18f, FontStyles.Normal, TextAlignmentOptions.MidlineLeft, "Edificio");
                RectTransform buttonNode = K.Node(row, "Liberar", r => K.Set(r, K.MR, K.MR, K.MR, Vector2.zero, new Vector2(190f, 28f)));
                (Button button, TMP_Text buttonLabel) = K.Button(buttonNode, "Liberar gato");
                if (Mathf.Approximately(buttonLabel.fontSize, 22f)) buttonLabel.fontSize = 18f;
                rowRoots.Add(row.gameObject);
                rowLabels.Add(label);
                rowButtons.Add(button);
                rowButtonLabels.Add(buttonLabel);
            }

            // Vista y enlaces
            var view = K.Add<MoxieFiltersView>(container, out _);
            K.Wire(view, so =>
            {
                K.Ref(so, "_root", root.gameObject);
                K.RefArray(so, "_filterIcons", icons);
                K.Ref(so, "_efficiencyFill", fill);
                K.Ref(so, "_efficiencyText", efficiencyText);
                K.Ref(so, "_statusText", status);
                K.Ref(so, "_startGroup", startGroup.gameObject);
                K.Ref(so, "_startButton", startButton);
                K.Ref(so, "_startLabel", startLabel);
                K.Ref(so, "_maintenanceGroup", maintenance.gameObject);
                K.RefArray(so, "_cleanButtons", cleanButtons);
                K.RefArray(so, "_cleanLabels", cleanLabels);
                K.Ref(so, "_freeCatsGroup", freeCats.gameObject);
                K.Ref(so, "_freeCatsMessage", message);

                SerializedProperty array = so.FindProperty("_workerRows");
                if (array != null && array.arraySize == 0)
                {
                    array.arraySize = WorkerRows;
                    for (int i = 0; i < WorkerRows; i++)
                    {
                        SerializedProperty item = array.GetArrayElementAtIndex(i);
                        item.FindPropertyRelative("Root").objectReferenceValue = rowRoots[i];
                        item.FindPropertyRelative("Label").objectReferenceValue = rowLabels[i];
                        item.FindPropertyRelative("ReleaseButton").objectReferenceValue = rowButtons[i];
                        item.FindPropertyRelative("ReleaseLabel").objectReferenceValue = rowButtonLabels[i];
                    }
                }
            });

            K.Register(view, "moxie");
            K.End();
        }
    }
}
