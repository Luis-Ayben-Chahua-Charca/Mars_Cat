// Asistencia de IA: escrito con Claude Code (Anthropic).
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEditor.U2D.Sprites;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

/// <summary>
/// Herramientas de Nio (menú Tools > Nio):
///  1. Diagnosticar (solo lectura)         -> Logs/Nio_Diagnostico.txt
///  2. Configurar animaciones y escena     -> copia de seguridad en Backups/, Logs/Nio_Configuracion.txt
///  3. Probar animaciones en Play Mode     -> Logs/Nio_PruebaPlayMode.txt y capturas
/// </summary>
[InitializeOnLoad]
public static class NioConfigurador
{
    const string Carpeta = "Assets/Sprites/Nio";
    const string RutaHoja = Carpeta + "/Hoja_de_sprites_del_astronauta_Nio-removebg-preview.png";
    const string RutaControlador = Carpeta + "/Controllers/Nio.controller";
    const string NombreNio = "Nio";
    const string NombreGrupo = "Nio_Pruebas_Anteriores (desactivado)";
    const string NombreCabina = "CabinaPrincipal";
    const string NombreCamara = "Camara_Vista_General";
    const string ClavePrueba = "NioConfigurador.PruebaPendiente";
    const float Escala = 0.6f;
    static readonly Vector3 CentroNucleo = new Vector3(1.1f, 0f, 0f);
    static readonly Vector3 PuntoNucleoRecorrido = new Vector3(1.125f, 0f, -2.625f);
    static readonly string[] Protegidos = { "SUE_Suelo_Zona_Tecnica", "SUE_Suelo_General", "GRP_01_ESTRUCTURA_EXTERIOR", "Modules", "Connectors" };

    struct Paso
    {
        public int s, n;
        public Paso(int s, int n) { this.s = s; this.n = n; }
    }

    class DefClip
    {
        public string ruta, estado;
        public float fps;
        public int min, max;
        public Paso[] pasos;
    }

    // Secuencias de sprites (índice del sprite, nº de frames que se mantiene) usando solo la fila de cada animación.
    static readonly DefClip[] Clips =
    {
        new DefClip { ruta = Carpeta + "/Animations/Nio_idlle.anim", estado = "Idle", fps = 12f, min = 0, max = 3,
            pasos = new[] { new Paso(0, 6), new Paso(1, 4), new Paso(0, 4), new Paso(3, 6), new Paso(2, 2), new Paso(0, 2) } },
        new DefClip { ruta = Carpeta + "/Animations/Nio_Walk.anim", estado = "Walk", fps = 10f, min = 4, max = 9,
            pasos = new[] { new Paso(4, 1), new Paso(5, 1), new Paso(6, 1), new Paso(7, 1), new Paso(8, 1), new Paso(9, 1) } },
        new DefClip { ruta = Carpeta + "/Animations/Nio_work.anim", estado = "Work", fps = 8f, min = 10, max = 13,
            pasos = new[] { new Paso(10, 1), new Paso(13, 1), new Paso(10, 1), new Paso(13, 1), new Paso(10, 1), new Paso(13, 1), new Paso(11, 3), new Paso(12, 3) } },
        new DefClip { ruta = Carpeta + "/Animations/Nio_Back.anim", estado = "Back", fps = 6f, min = 14, max = 17,
            pasos = new[] { new Paso(14, 1), new Paso(15, 1), new Paso(16, 1), new Paso(17, 1), new Paso(16, 1), new Paso(15, 1) } },
    };

    static readonly int[][] Filas = { new[] { 0, 1, 2, 3 }, new[] { 4, 5, 6, 7, 8, 9 }, new[] { 10, 11, 12, 13 }, new[] { 14, 15, 16, 17 } };

    static string Raiz => Directory.GetParent(Application.dataPath).FullName;

    static string RutaLog(string n)
    {
        string d = Path.Combine(Raiz, "Logs");
        Directory.CreateDirectory(d);
        return Path.Combine(d, n);
    }

    static NioConfigurador()
    {
        EditorApplication.playModeStateChanged -= AlCambiarModo;
        EditorApplication.playModeStateChanged += AlCambiarModo;
    }

    public static int Indice(string nombre)
    {
        if (string.IsNullOrEmpty(nombre)) return -1;
        int i = nombre.LastIndexOf('_');
        return i >= 0 && int.TryParse(nombre.Substring(i + 1), out int v) ? v : -2;
    }

    static Dictionary<int, Sprite> CargarSprites()
    {
        var d = new Dictionary<int, Sprite>();
        foreach (Sprite s in AssetDatabase.LoadAllAssetsAtPath(RutaHoja).OfType<Sprite>())
        {
            int i = Indice(s.name);
            if (i >= 0) d[i] = s;
        }
        return d;
    }

    static string Ruta(Transform t)
    {
        string r = t.name;
        while (t.parent != null) { t = t.parent; r = t.name + "/" + r; }
        return r;
    }

    // Referencia serializada no vacía cuyo objeto ya no existe ("Missing" en el Inspector).
    // Se lee por reflexión para no depender de la API (instanceID / EntityId) de cada versión.
    static bool ReferenciaRota(SerializedProperty p)
    {
        if (p.objectReferenceValue != null) return false;
        var pi = typeof(SerializedProperty).GetProperty("objectReferenceInstanceIDValue");
        if (pi != null)
        {
            object v = pi.GetValue(p);
            if (v is int i) return i != 0;
        }
        var pe = typeof(SerializedProperty).GetProperty("objectReferenceEntityIdValue");
        if (pe != null)
        {
            object v = pe.GetValue(p);
            return v != null && !v.Equals(Activator.CreateInstance(v.GetType()));
        }
        return false;
    }

    static string DescRef(UnityEngine.Object o, string prop, out bool rota)
    {
        rota = false;
        SerializedProperty p = new SerializedObject(o).FindProperty(prop);
        if (p == null) return "?";
        if (p.objectReferenceValue != null)
        {
            string ruta = AssetDatabase.GetAssetPath(p.objectReferenceValue);
            return p.objectReferenceValue.name + (string.IsNullOrEmpty(ruta) ? "" : " (" + ruta + ")");
        }
        rota = ReferenciaRota(p);
        return rota ? "FALTA: referencia rota (el archivo ya no existe)" : "ninguno";
    }

    // ------------------------------------------------------------------ 1. DIAGNÓSTICO
    [MenuItem("Tools/Nio/1. Diagnosticar (solo lectura)", false, 1)]
    public static void Diagnosticar()
    {
        var sb = new StringBuilder();
        var problemas = new List<string>();
        Scene escena = SceneManager.GetActiveScene();
        sb.AppendLine("DIAGNÓSTICO DE NIO (solo lectura) — " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        sb.AppendLine("Unity " + Application.unityVersion + " | escena activa: " + escena.path + (escena.isDirty ? " (con cambios sin guardar)" : ""));
        sb.AppendLine("Errores de compilación de scripts: " + (EditorUtility.scriptCompilationFailed ? "SÍ" : "no"));
        sb.AppendLine();

        var imp = AssetImporter.GetAtPath(RutaHoja) as TextureImporter;
        var sprites = CargarSprites();
        sb.AppendLine("[Hoja de sprites] " + RutaHoja);
        if (imp != null)
            sb.AppendLine("  tipo " + imp.textureType + ", modo " + imp.spriteImportMode + ", PPU " + imp.spritePixelsPerUnit + ", filtro " + imp.filterMode + ", sprites " + sprites.Count);
        foreach (int[] f in Filas)
            sb.AppendLine("  fila " + f[0] + "-" + f[f.Length - 1] + ": " + string.Join("  ", f.Select(i => sprites.ContainsKey(i)
                ? i + " (" + sprites[i].rect.width + "x" + sprites[i].rect.height + " px, base y=" + sprites[i].rect.y + ", pivote " +
                  (sprites[i].pivot.x / sprites[i].rect.width).ToString("0.00") + "," + (sprites[i].pivot.y / sprites[i].rect.height).ToString("0.00") + ")"
                : i + " (FALTA)")));

        sb.AppendLine();
        sb.AppendLine("[Animation Clips en " + Carpeta + "]");
        foreach (string guid in AssetDatabase.FindAssets("t:AnimationClip", new[] { Carpeta }))
        {
            string ruta = AssetDatabase.GUIDToAssetPath(guid);
            var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(ruta);
            if (clip == null) continue;
            sb.AppendLine("  " + ruta + ": " + clip.length.ToString("0.###") + " s, " + clip.frameRate + " fps, bucle " + (clip.isLooping ? "sí" : "no"));
            foreach (EditorCurveBinding b in AnimationUtility.GetObjectReferenceCurveBindings(clip))
            {
                ObjectReferenceKeyframe[] k = AnimationUtility.GetObjectReferenceCurve(clip, b);
                sb.AppendLine("    curva '" + b.path + "' " + b.type.Name + "." + b.propertyName + ": " +
                              string.Join("  ", k.Select(x => x.time.ToString("0.###") + "s→" + (x.value != null ? Indice(x.value.name).ToString() : "NULO"))));
                var tiempos = k.Select(x => x.time).ToArray();
                var pasos = tiempos.Zip(tiempos.Skip(1), (a, c) => Mathf.Round((c - a) * clip.frameRate)).Distinct().ToArray();
                if (pasos.Length > 1) sb.AppendLine("      aviso: espaciado irregular entre keyframes (" + string.Join(",", pasos) + " frames)");
            }
            foreach (EditorCurveBinding b in AnimationUtility.GetCurveBindings(clip))
                sb.AppendLine("    curva float '" + b.path + "' " + b.type.Name + "." + b.propertyName);
        }

        sb.AppendLine();
        sb.AppendLine("[Animator Controllers en " + Carpeta + "]");
        int controladores = 0;
        foreach (string guid in AssetDatabase.FindAssets("t:AnimatorController", new[] { Carpeta }))
        {
            string ruta = AssetDatabase.GUIDToAssetPath(guid);
            var c = AssetDatabase.LoadAssetAtPath<AnimatorController>(ruta);
            if (c == null) { sb.AppendLine("  " + ruta + ": no se pudo cargar"); continue; }
            controladores++;
            sb.AppendLine("  " + ruta + ": parámetros [" + string.Join(", ", c.parameters.Select(p => p.name + ":" + p.type)) + "]");
            foreach (AnimatorControllerLayer l in c.layers)
            {
                AnimatorStateMachine sm = l.stateMachine;
                int transiciones = sm.states.Sum(s => s.state.transitions.Length) + sm.anyStateTransitions.Length;
                sb.AppendLine("    capa " + l.name + ": " + sm.states.Length + " estado(s), " + transiciones + " transición(es), inicial: " + (sm.defaultState != null ? sm.defaultState.name : "ninguno"));
                foreach (ChildAnimatorState cs in sm.states)
                {
                    string mot = DescRef(cs.state, "m_Motion", out bool rota);
                    if (rota) problemas.Add("El controlador " + Path.GetFileName(ruta) + " tiene el estado '" + cs.state.name + "' con un clip que ya no existe.");
                    sb.AppendLine("      estado '" + cs.state.name + "' → " + mot);
                }
                if (sm.states.Length == 1 && transiciones == 0)
                    problemas.Add("El controlador " + Path.GetFileName(ruta) + " solo tiene 1 estado y ninguna transición: no puede cambiar de animación.");
            }
        }
        if (controladores > 1) problemas.Add("Hay " + controladores + " Animator Controllers en " + Carpeta + " (uno por clip, creados al arrastrar sprites/clips a la escena) en lugar de uno con los 4 estados.");

        sb.AppendLine();
        sb.AppendLine("[Escena: Animator, Animation y SpriteRenderer]");
        foreach (GameObject raiz in escena.GetRootGameObjects())
        {
            foreach (Animator an in raiz.GetComponentsInChildren<Animator>(true))
            {
                string ctrl = DescRef(an, "m_Controller", out bool rota);
                bool anadido = PrefabUtility.IsAddedComponentOverride(an);
                bool tieneSprite = an.GetComponent<SpriteRenderer>() != null;
                sb.AppendLine("  Animator en " + Ruta(an.transform) + ": controlador " + ctrl + (tieneSprite ? "" : "; SIN SpriteRenderer") + (anadido ? "; añadido sobre el prefab de la cabina" : ""));
                if (rota) problemas.Add("El Animator de '" + Ruta(an.transform) + "' apunta a un controlador que ya no existe.");
                if (raiz.name == NombreCabina) problemas.Add("Hay un Animator añadido a un objeto de la cabina: " + Ruta(an.transform) + " (controlador: " + ctrl + ").");
            }
            foreach (Animation la in raiz.GetComponentsInChildren<Animation>(true))
            {
                sb.AppendLine("  Animation (legacy) en " + Ruta(la.transform));
                if (raiz.name == NombreCabina) problemas.Add("Hay un componente Animation (legacy) añadido a la cabina: " + Ruta(la.transform) + ".");
            }
            foreach (SpriteRenderer s in raiz.GetComponentsInChildren<SpriteRenderer>(true))
            {
                string sp = DescRef(s, "m_Sprite", out bool rota);
                sb.AppendLine("  SpriteRenderer en " + Ruta(s.transform) + ": sprite " + sp + ", activo " + s.gameObject.activeInHierarchy +
                              ", posición " + s.transform.position.ToString("0.00") + ", capa '" + s.sortingLayerName + "' orden " + s.sortingOrder);
                if (rota) problemas.Add("El SpriteRenderer de '" + Ruta(s.transform) + "' apunta a un sprite que ya no existe (no se dibuja nada).");
            }
        }
        var nioSprites = escena.GetRootGameObjects().Where(g => g.GetComponent<SpriteRenderer>() != null && g.GetComponent<Animator>() != null).ToList();
        if (nioSprites.Count > 1) problemas.Add("Hay " + nioSprites.Count + " objetos-sprite de Nio sueltos en la escena, cada uno con su propio controlador de un solo estado.");

        sb.AppendLine();
        sb.AppendLine("[CONCLUSIONES]");
        if (problemas.Count == 0) sb.AppendLine("  No se encontraron problemas.");
        foreach (string p in problemas.Distinct()) sb.AppendLine("  - " + p);
        sb.AppendLine("  - Recordatorio: en modo edición el Animator no se reproduce; las animaciones solo avanzan en Play Mode (o en la ventana Animation con Preview).");

        File.WriteAllText(RutaLog("Nio_Diagnostico.txt"), sb.ToString());
        Debug.Log("[Nio] Diagnóstico escrito en Logs/Nio_Diagnostico.txt (" + problemas.Distinct().Count() + " problemas).");
    }

    // ------------------------------------------------------------------ 2. CONFIGURACIÓN
    [MenuItem("Tools/Nio/2. Configurar animaciones y escena", false, 2)]
    public static void Configurar()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            EditorUtility.DisplayDialog("Nio", "Sal de Play Mode antes de configurar.", "OK");
            return;
        }
        Scene escena = SceneManager.GetActiveScene();
        if (!escena.path.EndsWith("CabinaPrincipal.unity"))
        {
            EditorUtility.DisplayDialog("Nio", "Abre la escena CabinaPrincipal antes de configurar a Nio.", "OK");
            return;
        }
        if (escena.GetRootGameObjects().Any(g => g.name == "Nio_Player"))
        {
            EditorUtility.DisplayDialog("Nio", "Nio ya está convertido en Nio_Player (personaje controlable). Usa Tools > Nio > 5 para reconfigurarlo.", "OK");
            return;
        }
        string marca = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string dirBackup = Path.Combine(Raiz, "Backups", "Nio_" + marca);
        if (!EditorUtility.DisplayDialog("Configurar Nio",
                "Primero se copiarán los archivos afectados en Backups/Nio_" + marca + ". Después:\n\n" +
                "• Pivotes de la hoja en la línea de suelo de cada fila (el PNG no se toca)\n" +
                "• Keyframes de Nio_idlle, Nio_Walk, Nio_work y Nio_Back\n" +
                "• Controllers/Nio.controller con Idle, Walk, Work y Back\n" +
                "• Objeto independiente 'Nio' en la escena\n" +
                "• Se quitan los Animator/Animation añadidos por error a la cabina\n" +
                "• Los Nio de prueba anteriores quedan desactivados (no se borran)\n\n" +
                "No se borra ningún archivo.", "Configurar", "Cancelar"))
            return;

        var log = new StringBuilder();
        log.AppendLine("CONFIGURACIÓN DE NIO — " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " | Unity " + Application.unityVersion);
        try
        {
            if (escena.isDirty) log.AppendLine("Aviso: la escena tenía cambios sin guardar; se guardan junto con esta configuración.");
            CopiaSeguridad(dirBackup, escena.path, log);
            AjustarPivotes(log);
            Dictionary<int, Sprite> sprites = CargarSprites();
            for (int i = 0; i < 18; i++) if (!sprites.ContainsKey(i)) throw new Exception("Falta el sprite " + i + " en la hoja.");
            var clips = new Dictionary<string, AnimationClip>();
            foreach (DefClip d in Clips)
            {
                var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(d.ruta);
                if (clip == null) throw new Exception("No existe " + d.ruta);
                AplicarClip(clip, d, sprites, log);
                clips[d.estado] = clip;
            }
            foreach (AnimationClip c in clips.Values) AssetDatabase.SaveAssetIfDirty(c);
            AnimatorController ctrl = CrearControlador(clips, log);
            PrepararEscena(ctrl, sprites, log);
            EditorSceneManager.MarkSceneDirty(escena);
            if (!EditorSceneManager.SaveScene(escena)) throw new Exception("No se pudo guardar la escena.");
            log.AppendLine("Escena guardada: " + escena.path);
        }
        catch (Exception e)
        {
            log.AppendLine("ERROR: " + e);
            File.WriteAllText(RutaLog("Nio_Configuracion.txt"), log.ToString());
            Debug.LogError("[Nio] Error en la configuración: " + e.Message + " (Logs/Nio_Configuracion.txt)");
            return;
        }
        Verificar(log);
    }

    static void CopiaSeguridad(string dir, string rutaEscena, StringBuilder log)
    {
        var rutas = new List<string> { rutaEscena, RutaHoja + ".meta" };
        foreach (DefClip d in Clips) { rutas.Add(d.ruta); rutas.Add(d.ruta + ".meta"); }
        rutas.Add(RutaControlador);
        rutas.Add(RutaControlador + ".meta");
        int n = 0;
        foreach (string r in rutas)
        {
            string src = Path.Combine(Raiz, r);
            if (!File.Exists(src)) continue;
            string dst = Path.Combine(dir, r);
            Directory.CreateDirectory(Path.GetDirectoryName(dst));
            File.Copy(src, dst, false);
            n++;
        }
        log.AppendLine("Copia de seguridad: " + n + " archivos en " + dir);
    }

    static void AjustarPivotes(StringBuilder log)
    {
        var importer = AssetImporter.GetAtPath(RutaHoja) as TextureImporter;
        if (importer == null) throw new Exception("No se encontró el importador de " + RutaHoja);
        var fabrica = new SpriteDataProviderFactories();
        fabrica.Init();
        ISpriteEditorDataProvider dp = fabrica.GetSpriteEditorDataProviderFromObject(importer);
        dp.InitSpriteEditorDataProvider();
        SpriteRect[] rects = dp.GetSpriteRects();
        var porIndice = new Dictionary<int, SpriteRect>();
        foreach (SpriteRect r in rects) porIndice[Indice(r.name)] = r;
        foreach (int[] fila in Filas)
        {
            if (fila.Any(i => !porIndice.ContainsKey(i))) throw new Exception("La hoja no tiene los sprites esperados de la fila " + fila[0]);
            float suelo = fila.Min(i => porIndice[i].rect.y);
            foreach (int i in fila)
            {
                SpriteRect r = porIndice[i];
                r.alignment = SpriteAlignment.Custom;
                r.pivot = new Vector2(0.5f, (suelo - r.rect.y) / r.rect.height);
            }
            log.AppendLine("Pivotes fila " + fila[0] + "-" + fila[fila.Length - 1] + ": centro horizontal y línea de suelo común y=" + suelo + " px (antes: centro del recorte)");
        }
        dp.SetSpriteRects(rects);
        dp.Apply();
        importer.SaveAndReimport();
    }

    static void AplicarClip(AnimationClip clip, DefClip d, Dictionary<int, Sprite> sp, StringBuilder log)
    {
        EditorCurveBinding binding = EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite");
        var keys = new List<ObjectReferenceKeyframe>();
        int f = 0;
        foreach (Paso p in d.pasos)
        {
            keys.Add(new ObjectReferenceKeyframe { time = f / d.fps, value = sp[p.s] });
            f += p.n;
        }
        int total = f;
        float fin = total / d.fps;
        Paso ultimo = d.pasos[d.pasos.Length - 1];
        if (ultimo.n > 1) keys.Add(new ObjectReferenceKeyframe { time = (total - 1) / d.fps, value = sp[ultimo.s] });
        clip.frameRate = d.fps;
        AnimationUtility.SetObjectReferenceCurve(clip, binding, keys.ToArray());
        if (Mathf.Abs(clip.length - fin) > 0.5f / d.fps)
        {
            keys.Add(new ObjectReferenceKeyframe { time = fin, value = sp[ultimo.s] });
            AnimationUtility.SetObjectReferenceCurve(clip, binding, keys.ToArray());
        }
        AnimationClipSettings s = AnimationUtility.GetAnimationClipSettings(clip);
        s.loopTime = true;
        s.loopBlend = false;
        AnimationUtility.SetAnimationClipSettings(clip, s);
        EditorUtility.SetDirty(clip);
        log.AppendLine("Clip " + clip.name + " (" + d.estado + "): " + d.fps + " fps, " + clip.length.ToString("0.###") + " s en bucle, secuencia " +
                       string.Join(" ", d.pasos.Select(p => p.s + (p.n > 1 ? "×" + p.n : ""))));
    }

    static AnimatorStateTransition Transicion(AnimatorState a, AnimatorState b)
    {
        AnimatorStateTransition t = a.AddTransition(b);
        t.hasExitTime = false;
        t.exitTime = 0f;
        t.hasFixedDuration = true;
        t.duration = 0f;
        t.offset = 0f;
        t.canTransitionToSelf = false;
        return t;
    }

    static AnimatorController CrearControlador(Dictionary<string, AnimationClip> clips, StringBuilder log)
    {
        var ctrl = AssetDatabase.LoadAssetAtPath<AnimatorController>(RutaControlador);
        if (ctrl == null)
        {
            ctrl = AnimatorController.CreateAnimatorControllerAtPath(RutaControlador);
            log.AppendLine("Creado " + RutaControlador);
        }
        else
        {
            foreach (AnimatorControllerParameter p in ctrl.parameters.ToArray()) ctrl.RemoveParameter(p);
            AnimatorStateMachine viejo = ctrl.layers[0].stateMachine;
            foreach (ChildAnimatorState cs in viejo.states.ToArray()) viejo.RemoveState(cs.state);
            foreach (AnimatorStateTransition t in viejo.anyStateTransitions.ToArray()) viejo.RemoveAnyStateTransition(t);
            log.AppendLine("Reconstruido " + RutaControlador + " (mismo archivo)");
        }
        ctrl.AddParameter("Speed", AnimatorControllerParameterType.Float);
        ctrl.AddParameter("IsWorking", AnimatorControllerParameterType.Bool);
        ctrl.AddParameter("IsBack", AnimatorControllerParameterType.Bool);

        AnimatorStateMachine sm = ctrl.layers[0].stateMachine;
        AnimatorState idle = sm.AddState("Idle", new Vector3(300, 120, 0));
        AnimatorState walk = sm.AddState("Walk", new Vector3(600, 0, 0));
        AnimatorState work = sm.AddState("Work", new Vector3(600, 120, 0));
        AnimatorState back = sm.AddState("Back", new Vector3(600, 240, 0));
        idle.motion = clips["Idle"];
        walk.motion = clips["Walk"];
        work.motion = clips["Work"];
        back.motion = clips["Back"];
        sm.defaultState = idle;

        Transicion(idle, walk).AddCondition(AnimatorConditionMode.Greater, 0.1f, "Speed");
        Transicion(walk, idle).AddCondition(AnimatorConditionMode.Less, 0.1f, "Speed");
        Transicion(idle, work).AddCondition(AnimatorConditionMode.If, 0f, "IsWorking");
        Transicion(work, idle).AddCondition(AnimatorConditionMode.IfNot, 0f, "IsWorking");
        Transicion(idle, back).AddCondition(AnimatorConditionMode.If, 0f, "IsBack");
        Transicion(back, idle).AddCondition(AnimatorConditionMode.IfNot, 0f, "IsBack");

        EditorUtility.SetDirty(ctrl);
        AssetDatabase.SaveAssetIfDirty(ctrl);
        log.AppendLine("Controlador: estados Idle (inicial), Walk, Work, Back; parámetros Speed (Float), IsWorking (Bool), IsBack (Bool); " +
                       "6 transiciones sin exit time ni mezcla: Idle→Walk Speed>0.1, Walk→Idle Speed<0.1, Idle↔Work IsWorking, Idle↔Back IsBack");
        return ctrl;
    }

    static Camera BuscarCamara()
    {
        foreach (GameObject r in SceneManager.GetActiveScene().GetRootGameObjects())
            foreach (Camera c in r.GetComponentsInChildren<Camera>(true))
                if (c.name == NombreCamara) return c;
        return Camera.main;
    }

    static void PrepararEscena(AnimatorController ctrl, Dictionary<int, Sprite> sprites, StringBuilder log)
    {
        Scene escena = SceneManager.GetActiveScene();
        GameObject[] raices = escena.GetRootGameObjects();

        Material mat = null;
        foreach (GameObject r in raices)
            foreach (SpriteRenderer s in r.GetComponentsInChildren<SpriteRenderer>(true))
                if (mat == null && s.sharedMaterial != null) mat = s.sharedMaterial;

        // 1) Componentes de animación añadidos por error a objetos de la cabina.
        GameObject cabina = raices.FirstOrDefault(g => g.name == NombreCabina);
        if (cabina != null)
        {
            var comps = new List<Component>();
            comps.AddRange(cabina.GetComponentsInChildren<Animator>(true));
            comps.AddRange(cabina.GetComponentsInChildren<Animation>(true));
            foreach (Component c in comps)
            {
                string desc = Ruta(c.transform) + " (" + c.GetType().Name + ")";
                if (PrefabUtility.IsAddedComponentOverride(c))
                {
                    Undo.DestroyObjectImmediate(c);
                    log.AppendLine("Quitado de la cabina (componente añadido sobre el prefab): " + desc);
                }
                else log.AppendLine("No se toca (forma parte del prefab original): " + desc);
            }
        }

        // 2) Nio de prueba anteriores: se agrupan y se desactivan, sin borrarlos.
        List<GameObject> antiguos = raices.Where(g => g.name.StartsWith("Hoja") && g.GetComponent<SpriteRenderer>() != null).ToList();
        if (antiguos.Count > 0)
        {
            GameObject grupo = raices.FirstOrDefault(g => g.name == NombreGrupo);
            if (grupo == null)
            {
                grupo = new GameObject(NombreGrupo);
                Undo.RegisterCreatedObjectUndo(grupo, "Grupo Nio anteriores");
            }
            foreach (GameObject g in antiguos)
            {
                Undo.SetTransformParent(g.transform, grupo.transform, "Agrupar Nio anteriores");
                log.AppendLine("Desactivado (no borrado): " + g.name);
            }
            grupo.SetActive(false);
        }

        // 3) Objeto Nio independiente.
        GameObject nio = raices.FirstOrDefault(g => g.name == NombreNio);
        if (nio == null)
        {
            nio = new GameObject(NombreNio);
            Undo.RegisterCreatedObjectUndo(nio, "Crear Nio");
            log.AppendLine("Creado objeto independiente 'Nio' en la raíz de la escena");
        }
        else log.AppendLine("Reutilizado el objeto 'Nio' existente");

        SpriteRenderer sr = nio.GetComponent<SpriteRenderer>();
        if (sr == null) sr = nio.AddComponent<SpriteRenderer>();
        sr.sprite = sprites[0];
        if (mat != null) sr.sharedMaterial = mat;
        sr.drawMode = SpriteDrawMode.Simple;
        sr.color = Color.white;
        sr.flipX = false;
        sr.flipY = false;
        sr.sortingLayerName = "Default";
        sr.sortingOrder = 0;
        sr.spriteSortPoint = SpriteSortPoint.Pivot;

        Animator an = nio.GetComponent<Animator>();
        if (an == null) an = nio.AddComponent<Animator>();
        an.runtimeAnimatorController = ctrl;
        an.applyRootMotion = false;
        an.cullingMode = AnimatorCullingMode.AlwaysAnimate;
        an.updateMode = AnimatorUpdateMode.Normal;

        if (nio.GetComponent<NioAnimacion>() == null) nio.AddComponent<NioAnimacion>();
        GameObjectUtility.SetStaticEditorFlags(nio, (StaticEditorFlags)0);

        nio.transform.localScale = Vector3.one * Escala;
        Camera cam = BuscarCamara();
        nio.transform.position = BuscarPosicion(cam, log);
        if (cam != null)
        {
            Vector3 f = Vector3.ProjectOnPlane(cam.transform.forward, Vector3.up);
            if (f.sqrMagnitude > 1e-6f) nio.transform.rotation = Quaternion.LookRotation(f.normalized, Vector3.up);
        }
        EditorUtility.SetDirty(nio);
        log.AppendLine("Nio: SpriteRenderer (sprite 0, material " + (sr.sharedMaterial != null ? sr.sharedMaterial.name : "por defecto") +
                       ", capa Default, orden 0), Animator (Nio.controller, sin root motion), NioAnimacion; escala " + Escala +
                       "; posición " + nio.transform.position.ToString("0.000") + "; orientado hacia " + (cam != null ? cam.name : "la cámara"));
    }

    static bool Suelo(Vector3 p, out Vector3 s)
    {
        s = p;
        if (Physics.Raycast(p + Vector3.up * 2.5f, Vector3.down, out RaycastHit hit, 4f, ~0, QueryTriggerInteraction.Ignore) &&
            hit.normal.y > 0.9f && hit.point.y > -0.3f && hit.point.y < 0.4f)
        {
            s = hit.point;
            return true;
        }
        return false;
    }

    static bool Ocupado(Vector3 s) =>
        Physics.CheckCapsule(s + Vector3.up * 0.3f, s + Vector3.up * 0.5f, 0.25f, ~0, QueryTriggerInteraction.Ignore);

    static bool Bloqueado(Vector3 a, Vector3 b)
    {
        Vector3 d = b - a;
        foreach (RaycastHit h in Physics.RaycastAll(a, d.normalized, d.magnitude - 0.02f, ~0, QueryTriggerInteraction.Ignore))
        {
            if (h.collider.bounds.min.y > 2.8f) continue; // piezas de cubierta/techo (la cubierta está oculta)
            return true;
        }
        return false;
    }

    static bool Visible(Camera cam, Vector3 s)
    {
        foreach (float h in new[] { 0.15f, 0.35f, 0.6f })
        {
            Vector3 t = s + Vector3.up * h;
            Vector3 vp = cam.WorldToViewportPoint(t);
            if (vp.z <= 0f || vp.x < 0.02f || vp.x > 0.98f || vp.y < 0.02f || vp.y > 0.98f) return false;
            if (Bloqueado(cam.transform.position, t)) return false;
        }
        return true;
    }

    static Vector3 BuscarPosicion(Camera cam, StringBuilder log)
    {
        Physics.SyncTransforms();
        Vector3 haciaCam = cam != null ? Vector3.ProjectOnPlane(cam.transform.position - CentroNucleo, Vector3.up).normalized : Vector3.forward;
        Vector3 fwd = cam != null ? Vector3.ProjectOnPlane(cam.transform.forward, Vector3.up).normalized : Vector3.forward;
        Vector3 der = Vector3.Cross(Vector3.up, fwd).normalized;
        float[] radios = { 2.0f, 1.6f, 2.4f, 1.2f, 2.8f, 0.8f };
        float[] angulos = { 0f, 20f, -20f, 40f, -40f, 60f, -60f, 90f, -90f, 120f, -120f, 180f };
        int probados = 0;
        foreach (float r in radios)
            foreach (float a in angulos)
            {
                probados++;
                Vector3 p = CentroNucleo + Quaternion.Euler(0f, a, 0f) * haciaCam * r;
                if (!Suelo(p, out Vector3 s)) continue;
                if (Ocupado(s) || Ocupado(s + der * 0.375f) || Ocupado(s + der * 0.75f)) continue;
                if (Physics.Raycast(s + Vector3.up * 0.32f, -fwd, 1.4f, ~0, QueryTriggerInteraction.Ignore)) continue;
                if (cam != null && !Visible(cam, s)) continue;
                log.AppendLine("Posición de Nio: suelo del núcleo central a " + r + " m del centro (giro " + a + "° respecto a la cámara), " +
                               "libre de obstáculos, con 0,75 m libres a su derecha y visible desde " + (cam != null ? cam.name : "-") + " (" + probados + " puntos probados)");
                return s;
            }
        log.AppendLine("AVISO: ningún punto del núcleo cumplió todas las condiciones; se usa el punto del recorrido del núcleo " + PuntoNucleoRecorrido);
        return Suelo(PuntoNucleoRecorrido, out Vector3 s2) ? s2 : PuntoNucleoRecorrido;
    }

    // ------------------------------------------------------------------ VERIFICACIÓN (modo edición)
    static void Verificar(StringBuilder log)
    {
        int ok = 0, total = 0;
        void C(bool cond, string texto)
        {
            total++;
            if (cond) ok++;
            log.AppendLine((cond ? "[OK]    " : "[FALLO] ") + texto);
        }

        log.AppendLine();
        log.AppendLine("== VERIFICACIÓN EN EL EDITOR ==");
        C(!EditorUtility.scriptCompilationFailed, "Sin errores de compilación de scripts");

        Dictionary<int, Sprite> sprites = CargarSprites();
        C(sprites.Count == 18 && Enumerable.Range(0, 18).All(sprites.ContainsKey), "Hoja original con sus 18 sprites (0-17), PNG sin modificar");
        foreach (int[] fila in Filas)
        {
            if (!fila.All(sprites.ContainsKey)) continue;
            float[] suelos = fila.Select(i => sprites[i].rect.y + sprites[i].pivot.y).ToArray();
            C(suelos.Max() - suelos.Min() < 0.01f, "Fila " + fila[0] + "-" + fila[fila.Length - 1] + ": todos los sprites apoyan en la misma línea de suelo (y=" + suelos[0].ToString("0.##") + " px)");
        }

        // Muestreo frame a frame con AnimationMode (el mismo sistema que la vista previa de la ventana Animation)
        // sobre el propio Nio; al terminar, AnimationMode restaura el sprite original.
        GameObject nioMuestra = SceneManager.GetActiveScene().GetRootGameObjects().FirstOrDefault(g => g.name == NombreNio);
        SpriteRenderer msr = nioMuestra != null ? nioMuestra.GetComponent<SpriteRenderer>() : null;
        if (msr == null) C(false, "Objeto 'Nio' con SpriteRenderer para muestrear los clips");
        else
        {
            AnimationMode.StartAnimationMode();
            try
            {
                foreach (DefClip d in Clips)
                {
                    var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(d.ruta);
                    if (clip == null) { C(false, "Existe " + d.ruta); continue; }
                    var esperado = new List<int>();
                    foreach (Paso p in d.pasos) for (int k = 0; k < p.n; k++) esperado.Add(p.s);
                    var obtenido = new List<int>();
                    for (int i = 0; i < esperado.Count; i++)
                    {
                        AnimationMode.BeginSampling();
                        AnimationMode.SampleAnimationClip(nioMuestra, clip, (i + 0.5f) / d.fps);
                        AnimationMode.EndSampling();
                        obtenido.Add(msr.sprite != null ? Indice(msr.sprite.name) : -1);
                    }
                    EditorCurveBinding[] b = AnimationUtility.GetObjectReferenceCurveBindings(clip);
                    bool bindingOk = b.Length == 1 && b[0].path == "" && b[0].type == typeof(SpriteRenderer) && b[0].propertyName == "m_Sprite" &&
                                     AnimationUtility.GetCurveBindings(clip).Length == 0;
                    bool enFila = obtenido.All(x => x >= d.min && x <= d.max);
                    bool largo = Mathf.Abs(clip.length - esperado.Count / d.fps) < 0.5f / d.fps;
                    C(bindingOk && enFila && esperado.SequenceEqual(obtenido) && clip.isLooping && largo,
                      "Clip " + clip.name + ": cambia SpriteRenderer.m_Sprite; muestreado frame a frame = " + string.Join(" ", obtenido) +
                      " (solo sprites " + d.min + "-" + d.max + "); " + clip.length.ToString("0.###") + " s a " + d.fps + " fps; bucle " + (clip.isLooping ? "sí" : "no"));
                }
            }
            finally
            {
                AnimationMode.StopAnimationMode();
            }
        }

        var ctrl = AssetDatabase.LoadAssetAtPath<AnimatorController>(RutaControlador);
        C(ctrl != null, "Existe " + RutaControlador);
        if (ctrl != null)
        {
            AnimatorStateMachine sm = ctrl.layers[0].stateMachine;
            string[] nombres = sm.states.Select(s => s.state.name).OrderBy(n => n).ToArray();
            C(ctrl.layers.Length == 1 && nombres.SequenceEqual(new[] { "Back", "Idle", "Walk", "Work" }), "Controlador con 1 capa y estados " + string.Join(", ", nombres));
            C(sm.defaultState != null && sm.defaultState.name == "Idle", "Estado inicial: Idle");
            bool paramsOk = ctrl.parameters.Length == 3 &&
                            ctrl.parameters.Any(p => p.name == "Speed" && p.type == AnimatorControllerParameterType.Float) &&
                            ctrl.parameters.Any(p => p.name == "IsWorking" && p.type == AnimatorControllerParameterType.Bool) &&
                            ctrl.parameters.Any(p => p.name == "IsBack" && p.type == AnimatorControllerParameterType.Bool);
            C(paramsOk, "Parámetros Speed (Float), IsWorking (Bool), IsBack (Bool)");
            foreach (DefClip d in Clips)
            {
                AnimatorState st = sm.states.Select(s => s.state).FirstOrDefault(s => s.name == d.estado);
                C(st != null && st.motion == AssetDatabase.LoadAssetAtPath<AnimationClip>(d.ruta), "Estado " + d.estado + " → " + Path.GetFileName(d.ruta));
            }
            int tr = sm.states.Sum(s => s.state.transitions.Length);
            bool limpias = sm.states.All(s => s.state.transitions.All(t => !t.hasExitTime && t.duration == 0f && t.conditions.Length == 1));
            C(tr == 6 && limpias && sm.anyStateTransitions.Length == 0, tr + " transiciones (Idle↔Walk, Idle↔Work, Idle↔Back) sin exit time ni mezcla; AnyState: " + sm.anyStateTransitions.Length);
        }

        Scene escena = SceneManager.GetActiveScene();
        GameObject[] raices = escena.GetRootGameObjects();
        GameObject nio = raices.FirstOrDefault(g => g.name == NombreNio);
        C(nio != null && nio.transform.parent == null, "Objeto 'Nio' independiente en la raíz de la escena");
        if (nio != null)
        {
            C(nio.GetComponents<SpriteRenderer>().Length == 1 && nio.GetComponents<Animator>().Length == 1 && nio.GetComponents<NioAnimacion>().Length == 1,
              "Nio tiene 1 SpriteRenderer, 1 Animator y 1 NioAnimacion (sin duplicados)");
            Animator an = nio.GetComponent<Animator>();
            C(an != null && ctrl != null && an.runtimeAnimatorController == ctrl && !an.applyRootMotion, "Animator de Nio usa Nio.controller (sin root motion)");
            SpriteRenderer nsr = nio.GetComponent<SpriteRenderer>();
            C(nsr != null && nsr.sprite != null && Indice(nsr.sprite.name) == 0 && nsr.sharedMaterial != null,
              "SpriteRenderer: sprite inicial " + (nsr != null && nsr.sprite != null ? nsr.sprite.name : "ninguno") + ", material " +
              (nsr != null && nsr.sharedMaterial != null ? nsr.sharedMaterial.name : "ninguno") + ", capa '" + (nsr != null ? nsr.sortingLayerName : "") + "' orden " + (nsr != null ? nsr.sortingOrder : 0));
            if (nsr != null)
                log.AppendLine("        posición " + nio.transform.position.ToString("0.000") + ", rotación Y " + nio.transform.eulerAngles.y.ToString("0.#") +
                               "°, escala " + nio.transform.localScale.x + " → alto en Idle " + nsr.bounds.size.y.ToString("0.00") + " m");
        }

        GameObject cabina = raices.FirstOrDefault(g => g.name == NombreCabina);
        if (cabina != null)
        {
            int n = cabina.GetComponentsInChildren<Animator>(true).Length + cabina.GetComponentsInChildren<Animation>(true).Length;
            C(n == 0, "La cabina no tiene Animator ni Animation (" + string.Join(", ", Protegidos) + " incluidos)");
            if (PrefabUtility.IsOutermostPrefabInstanceRoot(cabina))
                C(PrefabUtility.GetAddedComponents(cabina).All(a => !(a.instanceComponent is Animator) && !(a.instanceComponent is Animation)),
                  "Sin componentes de animación añadidos sobre el prefab de la cabina");
        }
        GameObject grupo = raices.FirstOrDefault(g => g.name == NombreGrupo);
        int antiguosActivos = raices.Count(g => g.name.StartsWith("Hoja") && g.activeInHierarchy);
        C(antiguosActivos == 0 && (grupo == null || !grupo.activeSelf),
          "Nio de prueba anteriores desactivados, no borrados" + (grupo != null ? " (" + grupo.transform.childCount + " en '" + NombreGrupo + "')" : ""));

        Camera cam = BuscarCamara();
        if (nio != null && cam != null)
        {
            Physics.SyncTransforms();
            SpriteRenderer nsr = nio.GetComponent<SpriteRenderer>();
            bool enc = GeometryUtility.TestPlanesAABB(GeometryUtility.CalculateFrustumPlanes(cam), nsr.bounds);
            C(enc && Visible(cam, nio.transform.position), "Nio visible desde " + cam.name + " (dentro del encuadre y sin muros delante)");
            Renderizar(cam, nio, log);
        }
        else C(false, "Cámara de juego encontrada");

        log.AppendLine();
        log.AppendLine("RESULTADO: " + ok + "/" + total + " comprobaciones superadas");
        File.WriteAllText(RutaLog("Nio_Configuracion.txt"), log.ToString());
        if (ok == total) Debug.Log("[Nio] Configuración terminada: " + ok + "/" + total + " comprobaciones OK. Informe: Logs/Nio_Configuracion.txt");
        else Debug.LogWarning("[Nio] Configuración terminada: " + ok + "/" + total + " comprobaciones OK. Revisa Logs/Nio_Configuracion.txt");

        if (nio != null)
        {
            Selection.activeGameObject = nio;
            if (SceneView.lastActiveSceneView != null)
                SceneView.lastActiveSceneView.Frame(new Bounds(nio.transform.position + Vector3.up * 0.35f, Vector3.one * 3f), false);
        }
    }

    static void Renderizar(Camera cam, GameObject nio, StringBuilder log)
    {
        const int w = 1600, h = 900;
        var rt = new RenderTexture(w, h, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
        try
        {
            cam.aspect = (float)w / h;
            var req = new RenderPipeline.StandardRequest { destination = rt };
            if (RenderPipeline.SupportsRenderRequest(cam, req)) RenderPipeline.SubmitRenderRequest(cam, req);
            else
            {
                RenderTexture prevT = cam.targetTexture;
                cam.targetTexture = rt;
                cam.Render();
                cam.targetTexture = prevT;
            }
            RenderTexture prev = RenderTexture.active;
            RenderTexture.active = rt;
            var tex = new Texture2D(w, h, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, w, h), 0, 0);
            tex.Apply();
            RenderTexture.active = prev;
            File.WriteAllBytes(RutaLog("Nio_VistaCamaraJuego.png"), tex.EncodeToPNG());

            Vector3 vp = cam.WorldToViewportPoint(nio.GetComponent<SpriteRenderer>().bounds.center);
            int cx = Mathf.RoundToInt(vp.x * w), cy = Mathf.RoundToInt(vp.y * h);
            const int m = 70, z = 4;
            var zoom = new Texture2D(2 * m * z, 2 * m * z, TextureFormat.RGB24, false);
            for (int y = 0; y < 2 * m * z; y++)
                for (int x = 0; x < 2 * m * z; x++)
                    zoom.SetPixel(x, y, tex.GetPixel(Mathf.Clamp(cx - m + x / z, 0, w - 1), Mathf.Clamp(cy - m + y / z, 0, h - 1)));
            zoom.Apply();
            File.WriteAllBytes(RutaLog("Nio_VistaCamaraJuego_zoom.png"), zoom.EncodeToPNG());
            log.AppendLine("        Render de " + cam.name + ": Logs/Nio_VistaCamaraJuego.png (Nio en el píxel " + cx + "," + (h - cy) +
                           ") y ampliación ×4: Logs/Nio_VistaCamaraJuego_zoom.png");
            UnityEngine.Object.DestroyImmediate(tex);
            UnityEngine.Object.DestroyImmediate(zoom);
        }
        catch (Exception e)
        {
            log.AppendLine("        No se pudo renderizar la cámara: " + e.Message);
        }
        finally
        {
            cam.ResetAspect();
            rt.Release();
            UnityEngine.Object.DestroyImmediate(rt);
        }
    }

    // ------------------------------------------------------------------ 3. PRUEBA EN PLAY MODE
    [MenuItem("Tools/Nio/3. Probar animaciones en Play Mode", false, 3)]
    public static void ProbarEnPlay()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        if (GameObject.Find("Nio_Player") != null)
        {
            EditorUtility.DisplayDialog("Nio", "Nio ya es Nio_Player: usa Tools > Nio > 6, que prueba también las animaciones con movimiento real.", "OK");
            return;
        }
        if (GameObject.Find(NombreNio) == null)
        {
            EditorUtility.DisplayDialog("Nio", "No hay un objeto 'Nio' activo en la escena. Ejecuta antes Tools > Nio > 2.", "OK");
            return;
        }
        if (SceneManager.GetActiveScene().isDirty && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        SessionState.SetBool(ClavePrueba, true);
        EditorApplication.isPlaying = true;
    }

    static void AlCambiarModo(PlayModeStateChange cambio)
    {
        if (cambio != PlayModeStateChange.EnteredPlayMode || !SessionState.GetBool(ClavePrueba, false)) return;
        SessionState.SetBool(ClavePrueba, false);
        GameObject nio = GameObject.Find(NombreNio);
        if (nio == null)
        {
            Debug.LogError("[Nio] No se encontró el objeto 'Nio' en Play Mode.");
            EditorApplication.isPlaying = false;
            return;
        }
        if (nio.GetComponent<NioPruebaAnimaciones>() == null) nio.AddComponent<NioPruebaAnimaciones>();
    }
}
