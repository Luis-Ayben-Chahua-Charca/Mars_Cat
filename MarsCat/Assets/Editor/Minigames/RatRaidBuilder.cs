// Asistencia de IA: escrito con Claude Code (Anthropic).
using System.IO;
using MarsCat.Logic.Data;
using MarsCat.Unity;
using MarsCat.Unity.Minigames;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using static MarsCat.EditorTools.MinigameUiKit;

namespace MarsCat.EditorTools
{
    /// <summary>
    /// Arma La Redada (#60): el panel pequeño "Minijuego_LaRedada" arriba al centro y, aparte, unas ratas sueltas
    /// de prueba para mover a mano. Idempotente y con Undo, como el resto de builders de minijuegos.
    /// </summary>
    public static class RatRaidBuilder
    {
        private const string BalancePath = "Assets/Data/balance_pmv.csv";
        private const string LooseKey = "rat.loose.initial";
        private const string RatsRootName = "RatasSueltas";
        private static readonly Vector2 PanelSize = new Vector2(620f, 270f);

        [MenuItem("Tools/Mars Cat/Minijuegos/La Redada")]
        public static void BuildRaid()
        {
            Begin("Crear minijuego La Redada");

            // Si el contenedor ya existe, el panel no se recoloca (se respetan los ajustes manuales).
            Transform existingGroup = Canvas().Find("Minijuegos");
            bool panelExisted = existingGroup != null && existingGroup.Find("Minijuego_LaRedada") != null;

            (RectTransform container, RectTransform root) = MinigameRoot("Minijuego_LaRedada", "La Redada", PanelSize);
            if (!panelExisted)
                Set(root, TC, TC, TC, new Vector2(0f, -20f), PanelSize);

            TMP_Text instructions = Text(Node(root, "Instrucciones", r => Set(r, TL, TR, TC, new Vector2(0f, -64f), new Vector2(-40f, 50f))),
                18f, FontStyles.Normal, TextAlignmentOptions.Center, "Haz clic en una rata suelta: un gato la llevará a la granja.", wrap: true);
            TMP_Text loose = Text(Node(root, "RatasSueltas", At(TL, new Vector2(20f, -122f), new Vector2(290f, 28f))),
                20f, FontStyles.Normal, TextAlignmentOptions.Left, "Ratas sueltas: 0");
            TMP_Text farm = Text(Node(root, "RatasGranja", At(TR, new Vector2(-20f, -122f), new Vector2(290f, 28f))),
                20f, FontStyles.Normal, TextAlignmentOptions.Right, "Granja: 0 / 0");
            TMP_Text status = Text(Node(root, "Estado", r => Set(r, TL, TR, TC, new Vector2(0f, -156f), new Vector2(-40f, 30f))),
                20f, FontStyles.Italic, TextAlignmentOptions.Center, "", color: AccentColor);
            (Button done, TMP_Text doneLabel) = Button(Node(root, "BotonTerminar", At(BR, new Vector2(-20f, 44f), new Vector2(150f, 40f))), "Terminar");

            var view = Add<RatRaidView>(container, out _);
            Wire(view, so =>
            {
                Ref(so, "_root", root.gameObject);
                Ref(so, "_instructionsText", instructions);
                Ref(so, "_looseText", loose);
                Ref(so, "_farmText", farm);
                Ref(so, "_statusText", status);
                Ref(so, "_doneButton", done);
                Ref(so, "_doneLabel", doneLabel);
                Ref(so, "_camera", Camera.main);
            });
            Register(view, "rat_farm");
            End();
        }

        [MenuItem("Tools/Mars Cat/Minijuegos/Crear ratas sueltas de prueba")]
        public static void BuildTestRats()
        {
            if (!File.Exists(BalancePath))
            {
                Debug.LogError("Ratas sueltas: no existe " + BalancePath + ".");
                return;
            }
            BalanceData balance = BalanceLoader.Parse(File.ReadAllText(BalancePath));
            int count = Mathf.Max(0, Mathf.RoundToInt(balance.Get(LooseKey)));

            Begin("Crear ratas sueltas de prueba");
            GameObject root = GameObject.Find(RatsRootName);
            bool rootNew = root == null;
            if (rootNew)
            {
                root = new GameObject(RatsRootName);
                Undo.RegisterCreatedObjectUndo(root, "Crear ratas sueltas de prueba");
            }

            int created = 0;
            const float radius = 6f;
            for (int i = 0; i < count; i++)
            {
                string name = "Rata_" + i;
                if (root.transform.Find(name) != null) continue;

                float angle = i * Mathf.PI * 2f / Mathf.Max(1, count);
                GameObject rat = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                Undo.RegisterCreatedObjectUndo(rat, "Crear ratas sueltas de prueba");
                rat.name = name;
                Undo.SetTransformParent(rat.transform, root.transform, "Crear ratas sueltas de prueba");
                rat.transform.localPosition = new Vector3(Mathf.Cos(angle) * radius, 0.2f, Mathf.Sin(angle) * radius);
                rat.transform.localScale = new Vector3(0.25f, 0.2f, 0.25f);
                rat.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);

                // Indicador de rata marcada: esfera pequeña encima, sin collider y apagada.
                GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                Undo.RegisterCreatedObjectUndo(marker, "Crear ratas sueltas de prueba");
                marker.name = "Marca";
                Object.DestroyImmediate(marker.GetComponent<Collider>());
                marker.transform.SetParent(rat.transform, false);
                marker.transform.localPosition = new Vector3(-1.6f, 0f, 0f);
                marker.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f) * 1f;
                marker.SetActive(false);

                var view = Undo.AddComponent<LooseRatView>(rat);
                Wire(view, so =>
                {
                    Int(so, "_ratId", i);
                    Ref(so, "_bootstrap", Bootstrap);
                    Ref(so, "_markedIndicator", marker);
                });
                created++;
            }

            Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
            EditorSceneManager.MarkSceneDirty(root.scene);
            Debug.Log($"Ratas sueltas de prueba: {created} creadas de {count} ({LooseKey}); ya existían {count - created}. " +
                      "Muévelas a mano a la estación (la escena no se guardó).");
        }
    }
}
