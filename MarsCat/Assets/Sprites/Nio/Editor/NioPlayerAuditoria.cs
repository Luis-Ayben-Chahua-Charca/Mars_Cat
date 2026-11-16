// Asistencia de IA: escrito con Claude Code (Anthropic).
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Tools > Nio > 4. Auditoría para personaje controlable (solo lectura): no modifica nada.
/// Escribe Logs/NioPlayer_Auditoria.txt.
/// </summary>
public static class NioPlayerAuditoria
{
    static string RutaLog(string n)
    {
        string d = Path.Combine(Directory.GetParent(Application.dataPath).FullName, "Logs");
        Directory.CreateDirectory(d);
        return Path.Combine(d, n);
    }

    static string Ruta(Transform t)
    {
        string r = t.name;
        while (t.parent != null) { t = t.parent; r = t.name + "/" + r; }
        return r;
    }

    static string B(Bounds b) => "centro " + b.center.ToString("0.00") + " tamaño " + b.size.ToString("0.00") + " (y " + b.min.y.ToString("0.00") + "…" + b.max.y.ToString("0.00") + ")";

    [MenuItem("Tools/Nio/4. Auditoría para personaje controlable (solo lectura)", false, 20)]
    public static void Auditar()
    {
        var sb = new StringBuilder();
        Scene escena = SceneManager.GetActiveScene();
        GameObject[] raices = escena.GetRootGameObjects();
        sb.AppendLine("AUDITORÍA PARA NIO CONTROLABLE (solo lectura) — " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        sb.AppendLine("Unity " + Application.unityVersion + " | escena " + escena.path + (escena.isDirty ? " (con cambios sin guardar)" : "") +
                      " | errores de compilación: " + (EditorUtility.scriptCompilationFailed ? "SÍ" : "no"));
        sb.AppendLine("Raíces de la escena: " + string.Join(", ", raices.Select(r => r.name + (r.activeSelf ? "" : " (inactivo)"))));

        // Paquetes y entrada
        sb.AppendLine();
        sb.AppendLine("[Paquetes y entrada]");
        var paquetes = UnityEditor.PackageManager.PackageInfo.GetAllRegisteredPackages();
        foreach (string id in new[] { "com.unity.cinemachine", "com.unity.inputsystem", "com.unity.render-pipelines.universal", "com.unity.ai.navigation" })
        {
            var p = paquetes.FirstOrDefault(x => x.name == id);
            sb.AppendLine("  " + id + ": " + (p != null ? p.version : "NO instalado"));
        }
        bool cm3 = Type.GetType("Unity.Cinemachine.CinemachineCamera, Unity.Cinemachine") != null;
        bool cm2 = Type.GetType("Cinemachine.CinemachineVirtualCamera, Cinemachine") != null;
        sb.AppendLine("  Tipos de Cinemachine cargados: " + (cm3 ? "Cinemachine 3 (Unity.Cinemachine)" : cm2 ? "Cinemachine 2" : "ninguno"));
        UnityEngine.Object ps = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset").FirstOrDefault();
        int manejo = ps != null ? new SerializedObject(ps).FindProperty("activeInputHandler").intValue : -1;
        sb.AppendLine("  Active Input Handling: " + (manejo == 0 ? "Input Manager (antiguo)" : manejo == 1 ? "Input System Package (nuevo)" : manejo == 2 ? "Ambos" : "?"));
#if ENABLE_INPUT_SYSTEM
        sb.AppendLine("  ENABLE_INPUT_SYSTEM definido: sí");
#else
        sb.AppendLine("  ENABLE_INPUT_SYSTEM definido: no");
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
        sb.AppendLine("  ENABLE_LEGACY_INPUT_MANAGER definido: sí");
#else
        sb.AppendLine("  ENABLE_LEGACY_INPUT_MANAGER definido: no");
#endif

        // Nio
        sb.AppendLine();
        sb.AppendLine("[Nio en la jerarquía]");
        foreach (GameObject r in raices)
            foreach (Animator an in r.GetComponentsInChildren<Animator>(true))
            {
                var comps = an.GetComponents<Component>().Where(c => c != null).Select(c => c.GetType().Name);
                sb.AppendLine("  " + Ruta(an.transform) + (an.gameObject.activeInHierarchy ? "" : " (inactivo)") + ": [" + string.Join(", ", comps) + "]" +
                              ", controlador " + (an.runtimeAnimatorController != null ? an.runtimeAnimatorController.name : "ninguno") +
                              ", pos " + an.transform.position.ToString("0.000") + ", escala " + an.transform.lossyScale.x.ToString("0.##"));
                var sr = an.GetComponent<SpriteRenderer>();
                if (sr != null && sr.sprite != null)
                    sb.AppendLine("      sprite " + sr.sprite.name + " " + sr.sprite.rect.width + "x" + sr.sprite.rect.height + " px, PPU " + sr.sprite.pixelsPerUnit +
                                  ", pivote " + (sr.sprite.pivot.x / sr.sprite.rect.width).ToString("0.00") + "," + (sr.sprite.pivot.y / sr.sprite.rect.height).ToString("0.00") +
                                  ", material " + (sr.sharedMaterial ? sr.sharedMaterial.name : "-") + ", alto en mundo " + sr.bounds.size.y.ToString("0.00") + " m" +
                                  ", sombras " + sr.shadowCastingMode + ", capa " + sr.sortingLayerName + "/" + sr.sortingOrder);
                if (an.GetComponent<CharacterController>() != null || an.GetComponentInParent<CharacterController>() != null) sb.AppendLine("      ya tiene CharacterController");
            }
        foreach (string guid in AssetDatabase.FindAssets("t:AnimatorController", new[] { "Assets/Sprites/Nio/Controllers" }))
        {
            var c = AssetDatabase.LoadAssetAtPath<AnimatorController>(AssetDatabase.GUIDToAssetPath(guid));
            if (c == null) continue;
            AnimatorStateMachine sm = c.layers[0].stateMachine;
            sb.AppendLine("  Controlador " + AssetDatabase.GetAssetPath(c) + ": estados " + string.Join(", ", sm.states.Select(s => s.state.name + "→" + (s.state.motion ? s.state.motion.name : "-"))) +
                          "; inicial " + (sm.defaultState ? sm.defaultState.name : "-") + "; parámetros " + string.Join(", ", c.parameters.Select(p => p.name + ":" + p.type)));
            foreach (ChildAnimatorState s in sm.states)
                foreach (AnimatorStateTransition t in s.state.transitions)
                    sb.AppendLine("      " + s.state.name + " → " + (t.destinationState ? t.destinationState.name : "?") + " si " +
                                  string.Join(" y ", t.conditions.Select(k => k.parameter + " " + k.mode + (k.mode == AnimatorConditionMode.Greater || k.mode == AnimatorConditionMode.Less ? " " + k.threshold : ""))));
        }
        foreach (string guid in AssetDatabase.FindAssets("t:AnimationClip", new[] { "Assets/Sprites/Nio/Animations" }))
        {
            var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(AssetDatabase.GUIDToAssetPath(guid));
            var k = AnimationUtility.GetObjectReferenceCurveBindings(clip).SelectMany(b => AnimationUtility.GetObjectReferenceCurve(clip, b)).Select(x => x.value ? x.value.name.Substring(x.value.name.LastIndexOf('_') + 1) : "-");
            sb.AppendLine("  Clip " + clip.name + ": " + clip.length.ToString("0.##") + " s, sprites " + string.Join(" ", k));
        }

        // Cámaras
        sb.AppendLine();
        sb.AppendLine("[Cámaras]");
        foreach (GameObject r in raices)
            foreach (Camera c in r.GetComponentsInChildren<Camera>(true))
            {
                Vector3 e = c.transform.eulerAngles;
                sb.AppendLine("  " + Ruta(c.transform) + ": tag " + c.tag + ", pos " + c.transform.position.ToString("0.0") + ", rot (" + e.x.ToString("0.0") + ", " + e.y.ToString("0.0") + ", " + e.z.ToString("0.0") + ")" +
                              ", " + (c.orthographic ? "ortográfica " + c.orthographicSize : "perspectiva FOV " + c.fieldOfView) + ", near/far " + c.nearClipPlane + "/" + c.farClipPlane +
                              ", componentes [" + string.Join(", ", c.GetComponents<Component>().Where(x => x != null).Select(x => x.GetType().Name)) + "]");
            }

        // Escenario y colliders
        sb.AppendLine();
        sb.AppendLine("[Escenario]");
        GameObject cabina = raices.FirstOrDefault(g => g.name == "CabinaPrincipal");
        if (cabina != null)
        {
            Renderer[] rs = cabina.GetComponentsInChildren<Renderer>(false);
            if (rs.Length > 0)
            {
                Bounds b = rs[0].bounds;
                foreach (Renderer x in rs) b.Encapsulate(x.bounds);
                sb.AppendLine("  Renderers visibles: " + rs.Length + "; límites " + B(b));
            }
            sb.AppendLine("  Transform raíz: pos " + cabina.transform.position + ", rot " + cabina.transform.eulerAngles + ", escala " + cabina.transform.localScale);

            Collider[] cols = cabina.GetComponentsInChildren<Collider>(true);
            sb.AppendLine("  Colliders: " + cols.Length + " (" + string.Join(", ", cols.GroupBy(c => c.GetType().Name + (c.isTrigger ? " trigger" : "")).Select(g => g.Count() + " " + g.Key)) + ")");
            var suelos = cols.Where(c => !c.isTrigger && c.bounds.size.y < 0.35f && c.bounds.max.y < 0.3f).ToList();
            var muros = cols.Where(c => !c.isTrigger && c.bounds.size.y >= 2.0f).ToList();
            var techo = cols.Where(c => !c.isTrigger && c.bounds.min.y > 2.5f).ToList();
            var resto = cols.Where(c => !c.isTrigger && !suelos.Contains(c) && !muros.Contains(c) && !techo.Contains(c)).ToList();
            sb.AppendLine("  Suelo (" + suelos.Count + "): cara superior entre y=" + (suelos.Count > 0 ? suelos.Min(c => c.bounds.max.y).ToString("0.000") + " y " + suelos.Max(c => c.bounds.max.y).ToString("0.000") : "-"));
            foreach (Collider c in suelos.Take(12)) sb.AppendLine("      " + c.name + " [" + c.GetType().Name + "] " + B(c.bounds));
            sb.AppendLine("  Muros y paramentos ≥2 m (" + muros.Count + "): alturas " + (muros.Count > 0 ? muros.Min(c => c.bounds.max.y).ToString("0.00") + "…" + muros.Max(c => c.bounds.max.y).ToString("0.00") + " m" : "-"));
            foreach (Collider c in muros.Take(8)) sb.AppendLine("      " + c.name + " [" + c.GetType().Name + (c is MeshCollider mc ? (mc.convex ? " convexo" : " no convexo") : "") + "] " + B(c.bounds));
            sb.AppendLine("  Cubierta/techo (min y > 2.5 m): " + techo.Count);
            sb.AppendLine("  Otros (mobiliario, equipos, umbrales…) (" + resto.Count + "):");
            foreach (Collider c in resto.OrderBy(c => c.name)) sb.AppendLine("      " + Ruta(c.transform) + " [" + c.GetType().Name + "] alto " + c.bounds.max.y.ToString("0.00") + " m, " + B(c.bounds));
            var triggers = cols.Where(c => c.isTrigger).ToList();
            sb.AppendLine("  Triggers (" + triggers.Count + "): " + string.Join(", ", triggers.Select(c => c.name)));

            sb.AppendLine();
            sb.AppendLine("[Puertas]");
            foreach (CabinaPuertaAutomatica p in cabina.GetComponentsInChildren<CabinaPuertaAutomatica>(true))
                sb.AppendLine("  " + p.puertaId + " (" + Ruta(p.transform) + "): modo " + p.modo + ", bloqueada " + p.bloqueada +
                              ", bloqueo " + (p.colliderBloqueo ? B(p.colliderBloqueo.bounds) : "-"));

            sb.AppendLine();
            sb.AppendLine("[Referencias de escala]");
            foreach (Transform t in cabina.GetComponentsInChildren<Transform>(true).Where(tr => tr.name.StartsWith("GRP_14") || tr.name.StartsWith("GRP_12")))
                foreach (Transform h in t)
                {
                    Renderer[] hr = h.GetComponentsInChildren<Renderer>(true);
                    if (hr.Length == 0) continue;
                    Bounds hb = hr[0].bounds;
                    foreach (Renderer x in hr) hb.Encapsulate(x.bounds);
                    sb.AppendLine("  " + h.name + ": alto " + hb.size.y.ToString("0.00") + " m, ancho " + Mathf.Max(hb.size.x, hb.size.z).ToString("0.00") + " m");
                }
        }

        sb.AppendLine();
        sb.AppendLine("[Iluminación]");
        foreach (GameObject r in raices)
            foreach (Light l in r.GetComponentsInChildren<Light>(true).Where(l => l.type == LightType.Directional))
                sb.AppendLine("  " + Ruta(l.transform) + ": direccional, sombras " + l.shadows + ", rot " + l.transform.eulerAngles.ToString("0.0"));
        sb.AppendLine("  Pipeline: " + (UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline ? UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline.name : "Built-in"));

        File.WriteAllText(RutaLog("NioPlayer_Auditoria.txt"), sb.ToString());
        Debug.Log("[Nio] Auditoría para personaje controlable escrita en Logs/NioPlayer_Auditoria.txt");
    }
}
