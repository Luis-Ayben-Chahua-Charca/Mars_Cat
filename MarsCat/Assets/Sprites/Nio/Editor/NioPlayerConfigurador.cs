using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Unity.Cinemachine;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Tools > Nio > 5. Configurar Nio_Player y cámara   (copia de seguridad en Backups/, informe en Logs/NioPlayer_Configuracion.txt)
/// Tools > Nio > 6. Probar Nio_Player en Play Mode   (informe en Logs/NioPlayer_PruebaPlayMode.txt)
/// </summary>
[InitializeOnLoad]
public static class NioPlayerConfigurador
{
    const string NombrePlayer = "Nio_Player";
    const string NombreVisual = "Visual";
    const string NombreTarget = "CameraTarget";
    const string NombreSombra = "Sombra";
    const string NombreReferencia = "MovementReference";
    const string NombreCabina = "CabinaPrincipal";
    const string NombreCamara = "Camara_Vista_General";
    const string NombreCMNio = "CM_Nio_Isometrica";
    const string NombreCMGeneral = "CM_Vista_General";
    const string RutaControlador = "Assets/Sprites/Nio/Controllers/Nio.controller";
    const string RutaSombra = "Assets/Sprites/Nio/Sombra_Nio.png";
    const string RutaMaterialSilueta = "Assets/Sprites/Nio/Materials/Nio_Silueta.mat";
    const string ClavePrueba = "NioPlayerConfigurador.PruebaPendiente";

    // Personaje
    const float AltoCC = 0.62f, RadioCC = 0.22f, PielCC = 0.02f, EscalonCC = 0.15f, PendienteCC = 45f;
    const float AlturaTarget = 0.32f;
    // Cámara de seguimiento (misma inclinación y orientación que la cámara original)
    const float DistanciaCamara = 10f, FovSeguimiento = 35f, PantallaY = 0.1f, ZonaMuerta = 0.06f, Amortiguacion = 0.35f;

    static string Raiz => Directory.GetParent(Application.dataPath).FullName;

    static string RutaLog(string n)
    {
        string d = Path.Combine(Raiz, "Logs");
        Directory.CreateDirectory(d);
        return Path.Combine(d, n);
    }

    static NioPlayerConfigurador()
    {
        EditorApplication.playModeStateChanged -= AlCambiarModo;
        EditorApplication.playModeStateChanged += AlCambiarModo;
    }

    static string Ruta(Transform t)
    {
        string r = t.name;
        while (t.parent != null) { t = t.parent; r = t.name + "/" + r; }
        return r;
    }

    static T Obtener<T>(GameObject go, out bool nuevo) where T : Component
    {
        T c = go.GetComponent<T>();
        nuevo = c == null;
        if (nuevo) c = Undo.AddComponent<T>(go);
        return c;
    }

    static Transform Hijo(Transform padre, string nombre, out bool nuevo)
    {
        Transform t = padre.Find(nombre);
        nuevo = t == null;
        if (nuevo)
        {
            var go = new GameObject(nombre);
            Undo.RegisterCreatedObjectUndo(go, "Crear " + nombre);
            Undo.SetTransformParent(go.transform, padre, "Crear " + nombre);
            t = go.transform;
            t.localPosition = Vector3.zero;
            t.localRotation = Quaternion.identity;
            t.localScale = Vector3.one;
        }
        return t;
    }

    static bool SueloEn(Vector3 p, out float y, Transform ignorar = null)
    {
        y = 0f;
        float mejor = float.MaxValue;
        bool hay = false;
        foreach (RaycastHit h in Physics.RaycastAll(p + Vector3.up * 2.5f, Vector3.down, 4f, ~0, QueryTriggerInteraction.Ignore))
        {
            if (ignorar != null && h.collider.transform.IsChildOf(ignorar)) continue;
            if (h.normal.y < 0.9f || h.point.y > 0.4f || h.point.y < -0.4f) continue;
            if (h.distance < mejor) { mejor = h.distance; y = h.point.y; hay = true; }
        }
        return hay;
    }

    // ------------------------------------------------------------------ 5. CONFIGURAR
    [MenuItem("Tools/Nio/5. Configurar Nio_Player y cámara", false, 40)]
    public static void Configurar()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) { EditorUtility.DisplayDialog("Nio", "Sal de Play Mode antes de configurar.", "OK"); return; }
        Scene escena = SceneManager.GetActiveScene();
        if (!escena.path.EndsWith("CabinaPrincipal.unity")) { EditorUtility.DisplayDialog("Nio", "Abre la escena CabinaPrincipal.", "OK"); return; }
        string marca = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        if (!EditorUtility.DisplayDialog("Configurar Nio_Player",
                "Copia en Backups/NioPlayer_" + marca + ".\nNio pasa a Nio_Player con CharacterController, NioMovement y cámara Cinemachine.\nNo se toca la cabina.",
                "Configurar", "Cancelar"))
            return;

        var log = new StringBuilder();
        log.AppendLine("CONFIGURACIÓN DE NIO_PLAYER — " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " | Unity " + Application.unityVersion);
        try
        {
            CopiaSeguridad(Path.Combine(Raiz, "Backups", "NioPlayer_" + marca), escena.path, log);
            AnimatorController ctrl = ActualizarControlador(log);
            Sprite sombra = CrearSombra(log);
            Material silueta = CrearMaterialSilueta(log);
            GameObject player = PrepararPersonaje(ctrl, sombra, silueta, log);
            PrepararCamara(player, log);
            EditorSceneManager.MarkSceneDirty(escena);
            if (!EditorSceneManager.SaveScene(escena)) throw new Exception("No se pudo guardar la escena.");
            log.AppendLine("Escena guardada: " + escena.path);
        }
        catch (Exception e)
        {
            log.AppendLine("ERROR: " + e);
            File.WriteAllText(RutaLog("NioPlayer_Configuracion.txt"), log.ToString());
            Debug.LogError("[Nio] Error configurando Nio_Player: " + e.Message);
            return;
        }
        Verificar(log);
    }

    static void CopiaSeguridad(string dir, string rutaEscena, StringBuilder log)
    {
        int n = 0;
        foreach (string r in new[] { rutaEscena, RutaControlador, RutaControlador + ".meta" })
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

    static AnimatorStateTransition Tr(AnimatorState a, AnimatorState b)
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

    static AnimatorController ActualizarControlador(StringBuilder log)
    {
        var ctrl = AssetDatabase.LoadAssetAtPath<AnimatorController>(RutaControlador);
        if (ctrl == null) throw new Exception("No existe " + RutaControlador + " (ejecuta antes Tools > Nio > 2).");
        foreach (string p in new[] { "Speed", "IsWorking", "IsBack" })
            if (!ctrl.parameters.Any(x => x.name == p)) throw new Exception("Al controlador le falta el parámetro " + p);
        AnimatorStateMachine sm = ctrl.layers[0].stateMachine;
        AnimatorState S(string n) => sm.states.Select(c => c.state).First(s => s.name == n);
        AnimatorState idle = S("Idle"), walk = S("Walk"), work = S("Work"), back = S("Back");
        foreach (AnimatorState st in new[] { idle, walk, work, back })
            foreach (AnimatorStateTransition t in st.transitions.ToArray()) st.RemoveTransition(t);

        var t1 = Tr(idle, walk); t1.AddCondition(AnimatorConditionMode.Greater, 0.1f, "Speed"); t1.AddCondition(AnimatorConditionMode.IfNot, 0f, "IsBack");
        Tr(idle, back).AddCondition(AnimatorConditionMode.If, 0f, "IsBack");
        Tr(idle, work).AddCondition(AnimatorConditionMode.If, 0f, "IsWorking");
        Tr(walk, back).AddCondition(AnimatorConditionMode.If, 0f, "IsBack");
        Tr(walk, idle).AddCondition(AnimatorConditionMode.Less, 0.1f, "Speed");
        var t6 = Tr(back, walk); t6.AddCondition(AnimatorConditionMode.IfNot, 0f, "IsBack"); t6.AddCondition(AnimatorConditionMode.Greater, 0.1f, "Speed");
        Tr(back, idle).AddCondition(AnimatorConditionMode.IfNot, 0f, "IsBack");
        Tr(work, idle).AddCondition(AnimatorConditionMode.IfNot, 0f, "IsWorking");
        EditorUtility.SetDirty(ctrl);
        AssetDatabase.SaveAssetIfDirty(ctrl);
        log.AppendLine("Nio.controller: mismos estados, clips y parámetros (Speed, IsWorking, IsBack); 8 transiciones sin exit time: " +
                       "Idle→Walk (Speed>0.1 y no IsBack), Idle→Back, Idle→Work, Walk→Back, Walk→Idle (Speed<0.1), Back→Walk (no IsBack y Speed>0.1), Back→Idle, Work→Idle");
        return ctrl;
    }

    static Sprite CrearSombra(StringBuilder log)
    {
        string full = Path.Combine(Raiz, RutaSombra);
        if (!File.Exists(full))
        {
            const int n = 64;
            var tex = new Texture2D(n, n, TextureFormat.RGBA32, false);
            for (int y = 0; y < n; y++)
                for (int x = 0; x < n; x++)
                {
                    float r = new Vector2(x + 0.5f - n / 2f, y + 0.5f - n / 2f).magnitude / (n / 2f);
                    float a = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((1f - r) / 0.5f));
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
                }
            File.WriteAllBytes(full, tex.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(tex);
            AssetDatabase.ImportAsset(RutaSombra);
            var imp = (TextureImporter)AssetImporter.GetAtPath(RutaSombra);
            imp.textureType = TextureImporterType.Sprite;
            imp.spriteImportMode = SpriteImportMode.Single;
            imp.spritePixelsPerUnit = n;
            imp.alphaIsTransparency = true;
            imp.mipmapEnabled = false;
            imp.wrapMode = TextureWrapMode.Clamp;
            imp.SaveAndReimport();
            log.AppendLine("Creada la textura de sombra circular " + RutaSombra + " (64×64, degradado; no es un sprite de Nio)");
        }
        return AssetDatabase.LoadAssetAtPath<Sprite>(RutaSombra);
    }

    static Material CrearMaterialSilueta(StringBuilder log)
    {
        var mat = AssetDatabase.LoadAssetAtPath<Material>(RutaMaterialSilueta);
        if (mat != null) return mat;
        Shader sh = Shader.Find("Nio/SiluetaOculta");
        if (sh == null) throw new Exception("No se encontró el shader Nio/SiluetaOculta");
        if (!AssetDatabase.IsValidFolder("Assets/Sprites/Nio/Materials")) AssetDatabase.CreateFolder("Assets/Sprites/Nio", "Materials");
        mat = new Material(sh) { name = "Nio_Silueta" };
        mat.SetColor("_Color", new Color(0.55f, 0.85f, 1f, 0.6f));
        AssetDatabase.CreateAsset(mat, RutaMaterialSilueta);
        AssetDatabase.SaveAssetIfDirty(mat);
        log.AppendLine("Creado el material " + RutaMaterialSilueta + " (shader Nio/SiluetaOculta: dibuja a Nio solo donde un muro lo tapa)");
        return mat;
    }

    static GameObject PrepararPersonaje(AnimatorController ctrl, Sprite sombraSprite, Material matSilueta, StringBuilder log)
    {
        GameObject[] raices = SceneManager.GetActiveScene().GetRootGameObjects();
        GameObject player = raices.FirstOrDefault(g => g.name == NombrePlayer);
        Transform visual;
        Physics.SyncTransforms();
        if (player == null)
        {
            GameObject nio = raices.FirstOrDefault(g => g.name == "Nio");
            if (nio == null) throw new Exception("No existe el objeto 'Nio' en la raíz de la escena.");
            Vector3 pos = nio.transform.position;
            if (SueloEn(pos, out float ySuelo)) pos.y = ySuelo;
            player = new GameObject(NombrePlayer);
            Undo.RegisterCreatedObjectUndo(player, "Crear Nio_Player");
            player.transform.SetPositionAndRotation(pos, Quaternion.identity);
            Undo.SetTransformParent(nio.transform, player.transform, "Nio → Visual");
            Undo.RecordObject(nio, "Nio → Visual");
            nio.name = NombreVisual;
            nio.transform.localPosition = Vector3.zero;
            visual = nio.transform;
            log.AppendLine("Creado Nio_Player en " + pos.ToString("0.000") + "; el objeto 'Nio' (SpriteRenderer, Animator, NioAnimacion) pasa a ser su hijo 'Visual' sin duplicar componentes");
        }
        else
        {
            visual = player.transform.Find(NombreVisual);
            if (visual == null) throw new Exception("Nio_Player no tiene el hijo 'Visual'.");
            log.AppendLine("Nio_Player ya existía: se actualiza su configuración");
        }
        GameObjectUtility.SetStaticEditorFlags(player, 0);

        var cc = Obtener<CharacterController>(player, out bool _);
        cc.height = AltoCC;
        cc.radius = RadioCC;
        cc.skinWidth = PielCC;
        cc.center = new Vector3(0f, AltoCC * 0.5f + PielCC, 0f);
        cc.stepOffset = EscalonCC;
        cc.slopeLimit = PendienteCC;
        cc.minMoveDistance = 0f;

        Transform target = Hijo(player.transform, NombreTarget, out bool _);
        target.localPosition = new Vector3(0f, AlturaTarget, 0f);

        Transform sombra = Hijo(player.transform, NombreSombra, out bool _);
        var srSombra = Obtener<SpriteRenderer>(sombra.gameObject, out bool _);
        var srVisual = visual.GetComponent<SpriteRenderer>();
        srSombra.sprite = sombraSprite;
        srSombra.color = new Color(0f, 0f, 0f, 0.35f);
        if (srVisual != null) srSombra.sharedMaterial = srVisual.sharedMaterial;
        srSombra.sortingLayerName = "Default";
        srSombra.sortingOrder = -1;
        srSombra.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        sombra.localPosition = new Vector3(0f, 0.012f, 0f);
        sombra.localRotation = Quaternion.Euler(90f, 0f, 0f);
        sombra.localScale = new Vector3(0.42f, 0.42f, 1f);

        Transform sil = Hijo(visual, "Silueta", out bool _);
        sil.localPosition = Vector3.zero;
        sil.localRotation = Quaternion.identity;
        var srSil = Obtener<SpriteRenderer>(sil.gameObject, out bool _);
        srSil.sharedMaterial = matSilueta;
        if (srVisual != null) srSil.sprite = srVisual.sprite;
        srSil.sortingLayerName = "Default";
        srSil.sortingOrder = 1;
        srSil.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        var ns = Obtener<NioSilueta>(sil.gameObject, out bool _);
        ns.origen = srVisual;
        GameObjectUtility.SetStaticEditorFlags(sil.gameObject, 0);

        GameObject refGo = raices.FirstOrDefault(g => g.name == NombreReferencia);
        if (refGo == null)
        {
            refGo = new GameObject(NombreReferencia);
            Undo.RegisterCreatedObjectUndo(refGo, "Crear MovementReference");
        }
        // Norte del plano = -Z de Unity, este = -X  →  rotación Y 180°.
        refGo.transform.SetPositionAndRotation(Vector3.zero, Quaternion.Euler(0f, 180f, 0f));
        refGo.transform.localScale = Vector3.one;

        var mov = Obtener<NioMovement>(player, out bool movNuevo);
        mov.movementReference = refGo.transform;
        mov.visual = visual;
        mov.animacion = visual.GetComponent<NioAnimacion>();
        mov.cameraTarget = target;
        mov.sombra = sombra;
        if (movNuevo) mov.controlActivo = false;

        var an = visual.GetComponent<Animator>();
        if (an != null && an.runtimeAnimatorController != ctrl) an.runtimeAnimatorController = ctrl;
        if (mov.animacion != null)
        {
            Undo.RecordObject(mov.animacion, "Suavizado de Nio");
            mov.animacion.suavizado = 20f; // Idle en ~0,3 s al soltar las teclas
            mov.animacion.velocidadManual = -1f;
            EditorUtility.SetDirty(mov.animacion);
        }
        EditorUtility.SetDirty(player);
        log.AppendLine("Nio_Player: CharacterController (alto " + AltoCC + " m, radio " + RadioCC + " m, centro y=" + cc.center.y.ToString("0.###") + ", piel " + PielCC +
                       ", escalón " + EscalonCC + " m, pendiente " + PendienteCC + "°), NioMovement (velocidad " + mov.velocidad + " m/s, correr " + mov.velocidadCorrer +
                       " m/s, control desactivado al inicio), CameraTarget a " + AlturaTarget + " m, Sombra circular de 0,42 m; Visual/Silueta (se ve a través de los muros solo cuando algo tapa a Nio)");
        log.AppendLine("MovementReference en la raíz, rotación Y 180°: W = norte del plano (-Z), D = este (-X)");
        return player;
    }

    static void PrepararCamara(GameObject player, StringBuilder log)
    {
        GameObject[] raices = SceneManager.GetActiveScene().GetRootGameObjects();
        Camera cam = raices.SelectMany(r => r.GetComponentsInChildren<Camera>(true)).FirstOrDefault(c => c.name == NombreCamara);
        if (cam == null) throw new Exception("No se encontró " + NombreCamara);
        Transform grupo = cam.transform.parent != null ? cam.transform.parent : cam.transform;

        // Vista original conservada como cámara virtual de prioridad baja (se captura antes de añadir el Brain).
        Transform general = Hijo(grupo, NombreCMGeneral, out bool generalNueva);
        if (generalNueva) general.SetPositionAndRotation(cam.transform.position, cam.transform.rotation);
        var cmGeneral = Obtener<CinemachineCamera>(general.gameObject, out bool _);
        LensSettings lensG = LensSettings.Default;
        lensG.FieldOfView = 45f;
        lensG.NearClipPlane = cam.nearClipPlane;
        lensG.FarClipPlane = cam.farClipPlane;
        cmGeneral.Lens = lensG;
        cmGeneral.Priority = 0;

        Transform target = player.transform.Find(NombreTarget);
        Transform seguir = Hijo(grupo, NombreCMNio, out bool _);
        seguir.rotation = general.rotation; // misma inclinación y orientación que la vista original
        var cmNio = Obtener<CinemachineCamera>(seguir.gameObject, out bool _);
        cmNio.Follow = target;
        cmNio.LookAt = null;
        LensSettings lensN = LensSettings.Default;
        lensN.FieldOfView = FovSeguimiento;
        lensN.NearClipPlane = 0.1f;
        lensN.FarClipPlane = 200f;
        cmNio.Lens = lensN;
        cmNio.Priority = 10;

        var composer = Obtener<CinemachinePositionComposer>(seguir.gameObject, out bool _);
        composer.CameraDistance = DistanciaCamara;
        composer.Damping = new Vector3(Amortiguacion, Amortiguacion, Amortiguacion);
        composer.Composition.ScreenPosition = new Vector2(0f, PantallaY);
        composer.Composition.DeadZone.Enabled = true;
        composer.Composition.DeadZone.Size = new Vector2(ZonaMuerta, ZonaMuerta);
        composer.Composition.HardLimits.Enabled = true;
        composer.Composition.HardLimits.Size = new Vector2(0.6f, 0.6f);
        composer.CenterOnActivate = true;

        var decollider = Obtener<CinemachineDecollider>(seguir.gameObject, out bool _);
        decollider.CameraRadius = 0.3f;
        decollider.Decollision.Enabled = true;
        decollider.Decollision.ObstacleLayers = 1; // capa Default (muros, suelo, mobiliario)
        decollider.Decollision.Damping = 0.3f;
        decollider.TerrainResolution.Enabled = false;

        var brain = Obtener<CinemachineBrain>(cam.gameObject, out bool _);
        brain.DefaultBlend = new CinemachineBlendDefinition(CinemachineBlendDefinition.Styles.EaseInOut, 1f);

        Vector3 e = general.eulerAngles;
        log.AppendLine("Cámara: se reutiliza " + NombreCamara + " (única cámara real) con CinemachineBrain. " + NombreCMNio + " (prioridad 10) sigue a CameraTarget con " +
                       "Position Composer: distancia " + DistanciaCamara + " m, FOV " + FovSeguimiento + "°, rotación fija (" + e.x.ToString("0.0") + "°, " + e.y.ToString("0.0") +
                       "°) igual que la vista original, Nio un " + (PantallaY * 100) + " % por debajo del centro, zona muerta " + ZonaMuerta + ", amortiguación " + Amortiguacion +
                       " s; Decollider (radio 0,3 m, capa Default) para que la cámara no entre en la geometría. " + NombreCMGeneral + " (prioridad 0) guarda la vista general original.");
    }

    // ------------------------------------------------------------------ VERIFICACIÓN (editor)
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
        C(!EditorUtility.scriptCompilationFailed, "Sin errores de compilación");
        GameObject[] raices = SceneManager.GetActiveScene().GetRootGameObjects();
        GameObject player = raices.FirstOrDefault(g => g.name == NombrePlayer);
        C(player != null && player.transform.parent == null, "Nio_Player independiente en la raíz de la escena");
        if (player == null) { Escribir(log, ok, total); return; }
        Transform visual = player.transform.Find(NombreVisual);
        C(raices.All(g => g.name != "Nio"), "Ya no queda un 'Nio' suelto en la raíz (pasó a ser Nio_Player/Visual)");
        C(player.GetComponents<CharacterController>().Length == 1 && player.GetComponents<NioMovement>().Length == 1, "Nio_Player: 1 CharacterController y 1 NioMovement");
        C(visual != null && visual.GetComponent<SpriteRenderer>() != null && visual.GetComponent<Animator>() != null, "Visual: SpriteRenderer y Animator");
        C(player.GetComponentsInChildren<Animator>(true).Length == 1, "Un único Animator en la jerarquía de Nio_Player");
        GameObject cabina = raices.FirstOrDefault(g => g.name == NombreCabina);
        C(cabina != null && cabina.GetComponentsInChildren<Animator>(true).Length == 0, "Ningún Animator en objetos de la cabina (SUE_Suelo_Zona_Tecnica, etc.)");

        var mov = player.GetComponent<NioMovement>();
        C(mov.movementReference != null && mov.visual == visual && mov.animacion != null && mov.cameraTarget != null && mov.sombra != null,
          "NioMovement con referencias asignadas (MovementReference, Visual, NioAnimacion, CameraTarget, Sombra)");
        Vector3 f = mov.movementReference != null ? mov.movementReference.forward : Vector3.zero;
        Vector3 r = mov.movementReference != null ? mov.movementReference.right : Vector3.zero;
        C(Vector3.Angle(f, Vector3.back) < 0.5f && Vector3.Angle(r, Vector3.left) < 0.5f, "MovementReference: Forward = " + f.ToString("0.00") + " (norte del plano), Right = " + r.ToString("0.00") + " (este)");
        C(!mov.controlActivo, "Control desactivado al iniciar (se activa con C)");

        var cc = player.GetComponent<CharacterController>();
        Physics.SyncTransforms();
        bool hay = SueloEn(player.transform.position, out float ySuelo, player.transform);
        C(hay && Mathf.Abs(player.transform.position.y - ySuelo) < 0.01f, "Posición inicial " + player.transform.position.ToString("0.000") + " sobre el suelo (y suelo " + ySuelo.ToString("0.000") + ")");
        Vector3 c = player.transform.TransformPoint(cc.center);
        float mitad = Mathf.Max(0f, cc.height * 0.5f - cc.radius);
        var solapes = Physics.OverlapCapsule(c + Vector3.up * mitad, c - Vector3.up * mitad, cc.radius, ~0, QueryTriggerInteraction.Ignore)
            .Where(x => !x.transform.IsChildOf(player.transform)).Select(x => x.name).ToArray();
        C(solapes.Length == 0, "La cápsula inicial no se solapa con muros ni mobiliario" + (solapes.Length > 0 ? ": " + string.Join(", ", solapes) : ""));
        var srv = visual.GetComponent<SpriteRenderer>();
        log.AppendLine("        Nio mide " + srv.bounds.size.y.ToString("0.00") + " m (puertas 2,43–2,63 m, consolas 1,38 m); cápsula " + cc.height + " × Ø" + (cc.radius * 2) + " m");

        var ctrl = AssetDatabase.LoadAssetAtPath<AnimatorController>(RutaControlador);
        AnimatorStateMachine sm = ctrl.layers[0].stateMachine;
        int tr = sm.states.Sum(s => s.state.transitions.Length);
        C(sm.states.Length == 4 && sm.defaultState.name == "Idle" && tr == 8 && ctrl.parameters.Length == 3 &&
          sm.states.All(s => s.state.transitions.All(t => !t.hasExitTime && t.duration == 0f)),
          "Nio.controller: 4 estados, inicial Idle, 3 parámetros, " + tr + " transiciones sin exit time");

        Camera cam = raices.SelectMany(x => x.GetComponentsInChildren<Camera>(true)).ToArray() is Camera[] cams && cams.Length == 1 ? cams[0] : null;
        C(cam != null && cam.name == NombreCamara, "Una sola cámara real en la escena (" + (cam != null ? cam.name : "?") + ")");
        var brain = cam != null ? cam.GetComponent<CinemachineBrain>() : null;
        var vcams = raices.SelectMany(x => x.GetComponentsInChildren<CinemachineCamera>(true)).ToArray();
        var cmNio = vcams.FirstOrDefault(v => v.name == NombreCMNio);
        C(brain != null && cmNio != null && cmNio.Follow == mov.cameraTarget && cmNio.Priority.Value > vcams.Where(v => v != cmNio).Select(v => v.Priority.Value).DefaultIfEmpty(-1).Max(),
          "CinemachineBrain en la cámara y " + NombreCMNio + " (prioridad más alta) siguiendo a CameraTarget");
        C(cmNio != null && cmNio.GetComponent<CinemachinePositionComposer>() != null && cmNio.GetComponent<CinemachineDecollider>() != null,
          "Seguimiento suavizado (Position Composer) y anticolisión (Decollider)");

        Transform silT = visual.Find("Silueta");
        var silSr = silT != null ? silT.GetComponent<SpriteRenderer>() : null;
        C(silSr != null && silT.GetComponent<NioSilueta>() != null && silSr.sharedMaterial != null && silSr.sharedMaterial.shader.name == "Nio/SiluetaOculta" &&
          silSr.sharedMaterial.shader.isSupported && silT.GetComponents<Animator>().Length == 0,
          "Silueta a través de muros: shader Nio/SiluetaOculta compilado y soportado, sin Animator propio");
        if (cmNio != null)
        {
            float pct = VisibilidadRuta(cmNio.transform.rotation, player.transform, out int muestras);
            log.AppendLine("        Visibilidad directa estimada desde la cámara de seguimiento en el recorrido completo: " + pct.ToString("0.0") + " % de " + muestras +
                           " puntos; el resto del tiempo se ve la silueta");
        }
        Escribir(log, ok, total);
        Selection.activeGameObject = player;
    }

    static float VisibilidadRuta(Quaternion rot, Transform ignorar, out int muestras)
    {
        float h = DistanciaCamara * Mathf.Tan(FovSeguimiento * 0.5f * Mathf.Deg2Rad);
        Vector3 desplaz = -(rot * Vector3.forward) * DistanciaCamara + (rot * Vector3.up) * (2f * h * PantallaY);
        var puntos = new List<Vector3> { CabinaDatos.InicioRecorrido };
        puntos.AddRange(CabinaDatos.Recorrido.Select(q => q.posicion));
        muestras = 0;
        int visibles = 0;
        for (int i = 0; i + 1 < puntos.Count; i++)
        {
            float d = Vector3.Distance(puntos[i], puntos[i + 1]);
            int n = Mathf.Max(1, Mathf.CeilToInt(d / 0.3f));
            for (int k = 0; k < n; k++)
            {
                Vector3 p = Vector3.Lerp(puntos[i], puntos[i + 1], k / (float)n);
                Vector3 camPos = p + Vector3.up * AlturaTarget + desplaz;
                muestras++;
                if (VisibleDesde(camPos, p + Vector3.up * 0.32f, ignorar) || VisibleDesde(camPos, p + Vector3.up * 0.58f, ignorar)) visibles++;
            }
        }
        return 100f * visibles / Mathf.Max(1, muestras);
    }

    [MenuItem("Tools/Nio/7. Visibilidad según la inclinación de la cámara (solo lectura)", false, 42)]
    public static void AnalizarInclinaciones()
    {
        GameObject player = GameObject.Find(NombrePlayer);
        var cmNio = SceneManager.GetActiveScene().GetRootGameObjects().SelectMany(x => x.GetComponentsInChildren<CinemachineCamera>(true)).FirstOrDefault(v => v.name == NombreCMNio);
        if (player == null || cmNio == null) { EditorUtility.DisplayDialog("Nio", "Ejecuta antes Tools > Nio > 5.", "OK"); return; }
        Physics.SyncTransforms();
        float yaw = cmNio.transform.eulerAngles.y, actual = cmNio.transform.eulerAngles.x;
        var sb = new StringBuilder();
        sb.AppendLine("VISIBILIDAD DIRECTA DE NIO SEGÚN LA INCLINACIÓN (solo lectura) — " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        sb.AppendLine("Recorrido completo de la auditoría (núcleo, 6 módulos, pasillos y accesos), cámara a " + DistanciaCamara + " m, orientación " + yaw.ToString("0.0") + "°, FOV " + FovSeguimiento + "°.");
        sb.AppendLine("Se cuenta como visible si no hay muro ni mueble entre la cámara y el centro o la cabeza de Nio.");
        foreach (float pitch in new[] { actual, 45f, 50f, 55f, 60f, 70f })
        {
            float pct = VisibilidadRuta(Quaternion.Euler(pitch, yaw, 0f), player.transform, out int m);
            sb.AppendLine("  inclinación " + pitch.ToString("0.0") + "°" + (Mathf.Approximately(pitch, actual) ? " (actual, igual que la vista original)" : "") + ": " + pct.ToString("0.0") + " % de " + m + " puntos");
        }
        File.WriteAllText(RutaLog("NioPlayer_Visibilidad.txt"), sb.ToString());
        Debug.Log("[Nio] Análisis de visibilidad escrito en Logs/NioPlayer_Visibilidad.txt");
    }

    public static bool VisibleDesde(Vector3 origen, Vector3 destino, Transform ignorar)
    {
        Vector3 d = destino - origen;
        foreach (RaycastHit h in Physics.RaycastAll(origen, d.normalized, d.magnitude - 0.03f, ~0, QueryTriggerInteraction.Ignore))
        {
            if (ignorar != null && h.collider.transform.IsChildOf(ignorar)) continue;
            if (h.collider.bounds.min.y > 2.5f) continue; // cubierta oculta
            return false;
        }
        return true;
    }

    static void Escribir(StringBuilder log, int ok, int total)
    {
        log.AppendLine();
        log.AppendLine("RESULTADO: " + ok + "/" + total + " comprobaciones superadas");
        File.WriteAllText(RutaLog("NioPlayer_Configuracion.txt"), log.ToString());
        if (ok == total) Debug.Log("[Nio] Nio_Player configurado: " + ok + "/" + total + " comprobaciones OK. Informe: Logs/NioPlayer_Configuracion.txt");
        else Debug.LogWarning("[Nio] Nio_Player configurado: " + ok + "/" + total + " comprobaciones OK. Revisa Logs/NioPlayer_Configuracion.txt");
    }

    // ------------------------------------------------------------------ 6. PRUEBA EN PLAY MODE
    [MenuItem("Tools/Nio/6. Probar Nio_Player en Play Mode", false, 41)]
    public static void Probar()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        if (GameObject.Find(NombrePlayer) == null) { EditorUtility.DisplayDialog("Nio", "No existe Nio_Player. Ejecuta antes Tools > Nio > 5.", "OK"); return; }
        if (SceneManager.GetActiveScene().isDirty && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        SessionState.SetBool(ClavePrueba, true);
        EditorApplication.isPlaying = true;
    }

    static void AlCambiarModo(PlayModeStateChange cambio)
    {
        if (cambio != PlayModeStateChange.EnteredPlayMode || !SessionState.GetBool(ClavePrueba, false)) return;
        SessionState.SetBool(ClavePrueba, false);
        GameObject p = GameObject.Find(NombrePlayer);
        if (p == null) { Debug.LogError("[Nio] No se encontró Nio_Player en Play Mode."); EditorApplication.isPlaying = false; return; }
        if (p.GetComponent<NioPlayerPrueba>() == null) p.AddComponent<NioPlayerPrueba>();
    }
}
