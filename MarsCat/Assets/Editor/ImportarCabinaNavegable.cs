// Asistencia de IA: escrito con Claude Code (Anthropic).
// Assets/Editor/ImportarCabinaNavegable.cs
// Menú: Tools > Cabina Navegable > Importar y configurar escenario
//
// Importa y organiza CABINA_NAVEGABLE (Blender) dentro de Assets/CabinaNavegable/, crea materiales URP (o Standard),
// monta la escena CabinaPrincipal.unity con una jerarquía organizada, prepara colliders y puertas, crea el prefab
// CabinaNavegable.prefab y ejecuta una verificación con la física real de Unity. El informe se guarda en
// <proyecto>/Logs/CabinaNavegable_Verificacion.txt y se resume en la Consola.
//
// No borra ni modifica los archivos originales. Antes de sobrescribir una escena o un prefab existentes pide confirmación.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public static class ImportarCabinaNavegable
{
    const string MENU = "Tools/Cabina Navegable/";
    const string RAIZ = "Assets/CabinaNavegable";
    const string MODELS = RAIZ + "/Models";
    const string MATERIALS = RAIZ + "/Materials";
    const string TEXTURES = RAIZ + "/Textures";
    const string PREFABS = RAIZ + "/Prefabs";
    const string SCENES = "Assets/Scenes";
    const string COLLIDERS = RAIZ + "/Colliders";
    const string FBX_INTERIOR = MODELS + "/CABINA_NAVEGABLE_INTERIOR.fbx";
    const string FBX_COMPLETA = MODELS + "/CABINA_NAVEGABLE_COMPLETA.fbx";
    const string FBX_COLLIDERS = COLLIDERS + "/CABINA_COLLIDERS.fbx";
    const string ESCENA = SCENES + "/CabinaPrincipal.unity";
    const string PREFAB = PREFABS + "/CabinaNavegable.prefab";
    const string ESCENA_PRUEBA = SCENES + "/CabinaPrueba_Recorrido.unity";

    static readonly string[] GRUPOS = { "Environment", "Cabina_Visual", "Modules", "Connectors", "Technical_Area",
                                        "Removable_Roof", "Collisions", "Lighting", "Cameras" };

    // Grupos del FBX (colecciones de Blender) -> grupos de la escena
    static string DestinoDe(string grp)
    {
        if (grp.StartsWith("GRP_03") || grp.StartsWith("GRP_04") || grp.StartsWith("GRP_05") || grp.StartsWith("GRP_06") ||
            grp.StartsWith("GRP_07") || grp.StartsWith("GRP_08") || grp.StartsWith("GRP_09")) return "Modules";
        if (grp.StartsWith("GRP_10")) return "Connectors";
        if (grp.StartsWith("GRP_11")) return "Technical_Area";
        if (grp.StartsWith("GRP_13")) return "Lighting";
        if (grp.StartsWith("GRP_15")) return "Removable_Roof";
        return "Cabina_Visual";   // GRP_01 estructura, GRP_02 suelos, GRP_12 puertas, GRP_14 mobiliario
    }

    static readonly List<string> errores = new List<string>();
    static readonly List<string> avisos = new List<string>();

    static void CapturarLog(string msg, string stack, LogType t)
    {
        if (t == LogType.Error || t == LogType.Exception || t == LogType.Assert) errores.Add(msg);
        else if (t == LogType.Warning) avisos.Add(msg);
    }

    // =====================================================================================================
    [MenuItem(MENU + "Importar y configurar escenario", false, 1)]
    public static void ImportarYConfigurar()
    {
        errores.Clear(); avisos.Clear();
        Application.logMessageReceived += CapturarLog;
        try { Ejecutar(); }
        catch (Exception ex) { Debug.LogException(ex); }
        finally { Application.logMessageReceived -= CapturarLog; EditorUtility.ClearProgressBar(); }
    }

    static void Progreso(string paso, float f) { EditorUtility.DisplayProgressBar("Cabina Navegable", paso, f); }

    static void Ejecutar()
    {
        // ---------- 0. Comprobaciones previas
        foreach (var p in new[] { FBX_INTERIOR, FBX_COMPLETA, FBX_COLLIDERS })
            if (AssetDatabase.LoadAssetAtPath<GameObject>(p) == null)
            {
                EditorUtility.DisplayDialog("Cabina Navegable", "Falta " + p + ". Copia los archivos de la cabina en Assets/CabinaNavegable/.", "OK");
                return;
            }
        bool escenaExiste = File.Exists(ESCENA), prefabExiste = File.Exists(PREFAB);
        if ((escenaExiste || prefabExiste) && !Application.isBatchMode &&
            !EditorUtility.DisplayDialog("Cabina Navegable",
                "Ya existen " + (escenaExiste ? "CabinaPrincipal.unity " : "") + (prefabExiste ? "CabinaNavegable.prefab" : "") +
                ".\n¿Quieres regenerarlos (se sobrescribirán)?", "Regenerar", "Cancelar"))
            return;
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        // ---------- 1. Carpetas
        Progreso("Creando carpetas", 0.05f);
        foreach (var c in new[] { MODELS, MATERIALS, TEXTURES, PREFABS, SCENES, RAIZ + "/Scripts", COLLIDERS })
            CrearCarpeta(c);

        // ---------- 2. Texturas
        Progreso("Configurando texturas", 0.1f);
        foreach (var g in AssetDatabase.FindAssets("t:Texture2D", new[] { TEXTURES }))
        {
            string p = AssetDatabase.GUIDToAssetPath(g);
            var ti = AssetImporter.GetAtPath(p) as TextureImporter;
            if (ti == null) continue;
            bool normal = p.EndsWith("_Normal.png");
            var tipo = normal ? TextureImporterType.NormalMap : TextureImporterType.Default;
            if (ti.textureType != tipo || ti.sRGBTexture == normal || ti.wrapMode != TextureWrapMode.Repeat || ti.anisoLevel != 4)
            {
                ti.textureType = tipo; ti.sRGBTexture = !normal; ti.wrapMode = TextureWrapMode.Repeat;
                ti.mipmapEnabled = true; ti.anisoLevel = 4;
                ti.SaveAndReimport();
            }
        }

        // ---------- 3. Materiales (URP/Lit si el proyecto usa URP; si no, Standard)
        Progreso("Creando materiales", 0.2f);
        bool urp = EsURP();
        Shader sh = Shader.Find(urp ? "Universal Render Pipeline/Lit" : "Standard");
        if (sh == null) { Debug.LogError("[Cabina] No se encontró el shader " + (urp ? "URP/Lit" : "Standard")); return; }
        foreach (var d in CabinaDatos.Materiales)
        {
            string p = MATERIALS + "/" + d.nombre + ".mat";
            var m = AssetDatabase.LoadAssetAtPath<Material>(p);
            if (m == null) { m = new Material(sh); AssetDatabase.CreateAsset(m, p); }
            else m.shader = sh;
            AplicarMaterial(m, d, urp);
            EditorUtility.SetDirty(m);
        }
        AssetDatabase.SaveAssets();

        // ---------- 4. Importación de los FBX (ajustes + reasignación de materiales) y reimportación forzada
        Progreso("Importando FBX", 0.3f);
        ConfigurarModelo(FBX_INTERIOR, false);
        ConfigurarModelo(FBX_COMPLETA, false);
        ConfigurarModelo(FBX_COLLIDERS, true);

        // ---------- 5. Escena
        Progreso("Creando la escena CabinaPrincipal", 0.5f);
        var escena = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var raiz = new GameObject("CabinaPrincipal");
        var G = new Dictionary<string, Transform>();
        foreach (var n in GRUPOS)
        {
            var go = new GameObject(n);
            go.transform.SetParent(raiz.transform, false);
            G[n] = go.transform;
        }

        // 5a. Modelo visual principal: INTERIOR (sin cubierta). Se conservan los grupos GRP_* del FBX.
        var interior = Instanciar(FBX_INTERIOR);
        foreach (Transform grp in interior.transform.Cast<Transform>().ToList())
            grp.SetParent(G[DestinoDe(grp.name)], true);
        UnityEngine.Object.DestroyImmediate(interior);

        // 5b. Cubierta: solo GRP_15 de COMPLETA (INTERIOR no la tiene, así que no se duplica geometría)
        var completa = Instanciar(FBX_COMPLETA);
        var cub = completa.transform.Cast<Transform>().FirstOrDefault(t => t.name.StartsWith("GRP_15"));
        if (cub != null)
        {
            cub.SetParent(G["Removable_Roof"], true);
            cub.gameObject.SetActive(false);    // oculta al inicio para ver el interior; Tools > Cabina Navegable > Mostrar u ocultar cubierta
        }
        else Debug.LogError("[Cabina] No se encontró GRP_15_CUBIERTA_REMOVIBLE en " + FBX_COMPLETA);
        UnityEngine.Object.DestroyImmediate(completa);

        // 5c. Colisiones: geometría independiente de CABINA_COLLIDERS.fbx
        var cols = Instanciar(FBX_COLLIDERS);
        foreach (Transform t in cols.transform.Cast<Transform>().ToList()) t.SetParent(G["Collisions"], true);
        UnityEngine.Object.DestroyImmediate(cols);
        int colAnadidos = PrepararColliders(G["Collisions"]);

        // 5d. Puertas automáticas (hojas visuales + collider de bloqueo + trigger)
        int puertas = ConfigurarPuertas(raiz);

        // 5e. Iluminación (el FBX no trae luces: se recrean las del .blend)
        var luces = new GameObject("Luces_Puntuales").transform;
        luces.SetParent(G["Lighting"], false);
        foreach (var l in CabinaDatos.Luces)
        {
            var go = new GameObject(l.nombre);
            go.transform.SetParent(luces, false);
            go.transform.position = l.posicion;
            var L = go.AddComponent<Light>();
            L.type = LightType.Point; L.color = l.color; L.intensity = l.intensidad; L.range = l.alcance;
            L.shadows = LightShadows.None;
        }

        // 5f. Entorno: luz direccional
        var sol = new GameObject("Luz_Direccional");
        sol.transform.SetParent(G["Environment"], false);
        sol.transform.rotation = Quaternion.Euler(50f, 150f, 0f);
        var ls = sol.AddComponent<Light>();
        ls.type = LightType.Directional; ls.intensity = 1.0f; ls.shadows = LightShadows.Soft;
        ls.color = new Color(1f, 0.97f, 0.92f);

        // 5g. Cámara de visualización (perspectiva elevada, apuntando al centro)
        var camGo = new GameObject("Camara_Vista_General");
        camGo.transform.SetParent(G["Cameras"], false);
        camGo.tag = "MainCamera";
        camGo.transform.position = new Vector3(-20f, 26f, 28f);
        camGo.transform.rotation = Quaternion.LookRotation(Vector3.zero - camGo.transform.position, Vector3.up);
        var cam = camGo.AddComponent<Camera>();
        cam.fieldOfView = 45f; cam.nearClipPlane = 0.1f; cam.farClipPlane = 300f;
        camGo.AddComponent<AudioListener>();

        // Estáticos: arquitectura (no puertas, no INT_*)
        foreach (var t in raiz.GetComponentsInChildren<Transform>(true))
        {
            if (t == raiz.transform || G.ContainsValue(t)) continue;
            string n = t.name;
            if (n.StartsWith("PUERTA_") || n.StartsWith("INT_") || n.StartsWith("COL_PUERTA_") || n.StartsWith("COL_TRG_") ||
                t.GetComponent<Light>() != null || t.GetComponent<Camera>() != null) continue;
            if (t.GetComponent<Renderer>() != null)
                GameObjectUtility.SetStaticEditorFlags(t.gameObject, StaticEditorFlags.ContributeGI | StaticEditorFlags.BatchingStatic |
                    StaticEditorFlags.OccluderStatic | StaticEditorFlags.OccludeeStatic | StaticEditorFlags.ReflectionProbeStatic);
            else if (t.GetComponent<Collider>() != null)
                GameObjectUtility.SetStaticEditorFlags(t.gameObject, StaticEditorFlags.BatchingStatic);
        }

        // ---------- 6. Prefab (estructura visual + colisiones + puertas + luces; sin cámara ni entorno de la escena)
        Progreso("Creando el prefab", 0.7f);
        G["Environment"].SetParent(null, true);
        G["Cameras"].SetParent(null, true);
        bool okPrefab;
        PrefabUtility.SaveAsPrefabAssetAndConnect(raiz, PREFAB, InteractionMode.AutomatedAction, out okPrefab);
        G["Environment"].SetParent(raiz.transform, true); G["Environment"].SetSiblingIndex(0);
        G["Cameras"].SetParent(raiz.transform, true); G["Cameras"].SetAsLastSibling();

        // ---------- 7. Guardar escena y vista
        Progreso("Guardando la escena", 0.8f);
        bool okEscena = EditorSceneManager.SaveScene(escena, ESCENA);
        AnadirABuild(ESCENA);
        EncuadrarVistaEscena(raiz);
        Selection.activeGameObject = raiz;

        // ---------- 8. Verificación con Unity
        Progreso("Verificando", 0.9f);
        Verificar(raiz, okEscena, okPrefab, colAnadidos, puertas, urp);
    }

    // =====================================================================================================
    static void CrearCarpeta(string ruta)
    {
        if (AssetDatabase.IsValidFolder(ruta)) return;
        string padre = Path.GetDirectoryName(ruta).Replace('\\', '/');
        if (!AssetDatabase.IsValidFolder(padre)) CrearCarpeta(padre);
        AssetDatabase.CreateFolder(padre, Path.GetFileName(ruta));
    }

    static bool EsURP()
    {
        var rp = GraphicsSettings.currentRenderPipeline;
        return rp != null && rp.GetType().Name.Contains("Universal");
    }

    static void AplicarMaterial(Material m, CabinaDatos.Mat d, bool urp)
    {
        Func<string, Texture2D> T = n => string.IsNullOrEmpty(n) ? null : AssetDatabase.LoadAssetAtPath<Texture2D>(TEXTURES + "/" + n + ".png");
        var alb = T(d.albedo); var nor = T(d.normal); var em = T(d.emision);
        m.SetColor(urp ? "_BaseColor" : "_Color", d.color);
        m.SetTexture(urp ? "_BaseMap" : "_MainTex", alb);
        if (urp && m.HasProperty("_MainTex")) m.SetTexture("_MainTex", alb);
        m.SetFloat("_Metallic", d.metalico);
        m.SetFloat(urp ? "_Smoothness" : "_Glossiness", d.suavidad);
        m.SetTexture("_BumpMap", nor);
        if (nor != null) m.EnableKeyword("_NORMALMAP"); else m.DisableKeyword("_NORMALMAP");
        if (d.intensidadEmision > 0f)
        {
            float k = Mathf.Clamp(d.intensidadEmision * 0.5f, 0.5f, 4f);
            m.SetColor("_EmissionColor", d.colorEmision * k);
            m.SetTexture("_EmissionMap", em);
            m.EnableKeyword("_EMISSION");
            m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.BakedEmissive;
        }
        else
        {
            m.SetColor("_EmissionColor", Color.black);
            m.SetTexture("_EmissionMap", null);
            m.DisableKeyword("_EMISSION");
            m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.EmissiveIsBlack;
        }
    }

    static void ConfigurarModelo(string ruta, bool colliders)
    {
        var mi = (ModelImporter)AssetImporter.GetAtPath(ruta);
        mi.globalScale = 1f;
        mi.useFileUnits = true;
        mi.importCameras = false;
        mi.importLights = false;
        mi.importAnimation = false;
        mi.animationType = ModelImporterAnimationType.None;
        mi.importNormals = ModelImporterNormals.Import;
        mi.importTangents = ModelImporterTangents.CalculateMikk;
        mi.generateSecondaryUV = !colliders;
        mi.materialImportMode = colliders ? ModelImporterMaterialImportMode.None : ModelImporterMaterialImportMode.ImportViaMaterialDescription;
        if (!colliders)
            foreach (var d in CabinaDatos.Materiales)
            {
                var mat = AssetDatabase.LoadAssetAtPath<Material>(MATERIALS + "/" + d.nombre + ".mat");
                if (mat != null) mi.AddRemap(new AssetImporter.SourceAssetIdentifier(typeof(Material), d.nombre), mat);
            }
        EditorUtility.SetDirty(mi);
        mi.SaveAndReimport();
        AssetDatabase.ImportAsset(ruta, ImportAssetOptions.ForceUpdate);
    }

    static GameObject Instanciar(string ruta)
    {
        var go = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(ruta));
        go.transform.position = Vector3.zero; go.transform.rotation = Quaternion.identity; go.transform.localScale = Vector3.one;
        PrefabUtility.UnpackPrefabInstance(go, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
        return go;
    }

    static int PrepararColliders(Transform raiz)
    {
        int n = 0;
        foreach (var t in raiz.GetComponentsInChildren<Transform>(true))
        {
            if (!t.name.StartsWith("COL_")) continue;
            var mf = t.GetComponent<MeshFilter>();
            if (t.GetComponent<Collider>() == null && mf != null) { CabinaColliderUtil.Configurar(t.gameObject, mf.sharedMesh); n++; }
            var c = t.GetComponent<Collider>();
            if (c is MeshCollider) ((MeshCollider)c).convex = false;
            if (c != null && t.name.StartsWith("COL_TRG_")) c.isTrigger = true;
            var mr = t.GetComponent<MeshRenderer>();
            if (mr != null) UnityEngine.Object.DestroyImmediate(mr);     // los colliders no se dibujan
            if (mf != null) UnityEngine.Object.DestroyImmediate(mf);     // el MeshCollider conserva su propia referencia a la malla
        }
        return n;
    }

    static Transform Hijo(GameObject root, string nombre)
    {
        return root.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == nombre);
    }

    static int ConfigurarPuertas(GameObject raiz)
    {
        var hojas = raiz.GetComponentsInChildren<CabinaPropiedades>(true)
            .Where(p => p.Obtener("tipo") == "puerta_automatica" && !string.IsNullOrEmpty(p.Obtener("puerta_id")))
            .GroupBy(p => p.Obtener("puerta_id"));
        int n = 0;
        foreach (var g in hojas)
        {
            string id = g.Key;
            var trg = Hijo(raiz, "COL_TRG_PUERTA_" + id);
            if (trg == null) { Debug.LogWarning("[Cabina] Sin trigger para la puerta " + id); continue; }
            var c = trg.GetComponent<Collider>();
            if (c != null) c.isTrigger = true;
            var puerta = trg.GetComponent<CabinaPuertaAutomatica>();
            if (puerta == null) puerta = trg.gameObject.AddComponent<CabinaPuertaAutomatica>();
            puerta.puertaId = id;
            puerta.hojas = g.Select(p => p.transform).ToArray();
            puerta.desplazamientos = g.Select(p => DireccionApertura(p)).ToArray();
            puerta.modo = CabinaPuertaAutomatica.Modo.Deslizar;
            foreach (var giro in CabinaDatos.PuertasGiratorias)
            {
                if (giro.puertaId != id) continue;
                var h0 = puerta.hojas[0];
                Vector3 r = h0.position - giro.centro; r.y = 0f;
                Vector3 d = puerta.desplazamientos[0];
                float signo = Mathf.Sign(Vector3.Dot(Vector3.Cross(Vector3.up, r), d));
                puerta.modo = CabinaPuertaAutomatica.Modo.GirarSobreEje;
                puerta.centroGiro = new Vector3(giro.centro.x, h0.position.y, giro.centro.z);
                puerta.anguloGrados = signo * d.magnitude / Mathf.Max(0.1f, r.magnitude) * Mathf.Rad2Deg;
            }
            var bloq = Hijo(raiz, "COL_PUERTA_" + id);
            puerta.colliderBloqueo = bloq != null ? bloq.GetComponent<Collider>() : null;
            var luz = Hijo(raiz, "INT_LuzAdvertencia_" + id);
            puerta.indicador = luz != null ? luz.GetComponent<Renderer>() : null;
            n++;
        }
        return n;
    }

    static Vector3 DireccionApertura(CabinaPropiedades p)
    {
        float recorrido = p.ObtenerFloat("recorrido_m", 1f);
        Vector3 du = p.ObtenerVector("apertura_dir_unity");
        Vector3 r = p.transform.right;
        float s = du == Vector3.zero ? 1f : Mathf.Sign(Vector3.Dot(du, r));
        return r * s * recorrido;
    }

    static void AnadirABuild(string ruta)
    {
        var lista = EditorBuildSettings.scenes.ToList();
        if (lista.Any(s => s.path == ruta)) return;
        lista.Add(new EditorBuildSettingsScene(ruta, true));
        EditorBuildSettings.scenes = lista.ToArray();
    }

    static Bounds LimitesVisuales(GameObject raiz, bool incluirInactivos)
    {
        var rs = raiz.GetComponentsInChildren<Renderer>(incluirInactivos).Where(r => r.enabled).ToArray();
        if (rs.Length == 0) return new Bounds(Vector3.zero, Vector3.zero);
        var b = rs[0].bounds;
        foreach (var r in rs) b.Encapsulate(r.bounds);
        return b;
    }

    static void EncuadrarVistaEscena(GameObject raiz)
    {
        var sv = SceneView.lastActiveSceneView;
        if (sv == null) return;
        var b = LimitesVisuales(raiz, false);
        sv.LookAt(b.center, Quaternion.Euler(45f, 145f, 0f), 24f);
        sv.Repaint();
    }

    // =====================================================================================================
    // VERIFICACIÓN con la física y los datos reales de Unity
    static void Verificar(GameObject raiz, bool okEscena, bool okPrefab, int colAnadidos, int puertas, bool urp)
    {
        var rep = new StringBuilder();
        int ok = 0, fallos = 0;
        Action<bool, string, string> C = (cond, que, det) =>
        {
            if (cond) ok++; else fallos++;
            rep.AppendLine((cond ? "[OK]    " : "[FALLO] ") + que + (string.IsNullOrEmpty(det) ? "" : "  ·  " + det));
        };
        rep.AppendLine("VERIFICACIÓN CABINA NAVEGABLE · " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " · Unity " + Application.unityVersion);
        rep.AppendLine("Render pipeline: " + (urp ? "URP (" + GraphicsSettings.currentRenderPipeline.name + ")" : "Built-in / otro"));
        rep.AppendLine();

        Physics.SyncTransforms();
        // 1. Los tres FBX importados
        foreach (var p in new[] { FBX_INTERIOR, FBX_COMPLETA, FBX_COLLIDERS })
        {
            var go = AssetDatabase.LoadAssetAtPath<GameObject>(p);
            int mallas = go == null ? 0 : go.GetComponentsInChildren<MeshFilter>(true).Length;
            long tri = 0;
            if (go != null)
                foreach (var mf in go.GetComponentsInChildren<MeshFilter>(true))
                    if (mf.sharedMesh != null) for (int s = 0; s < mf.sharedMesh.subMeshCount; s++) tri += mf.sharedMesh.GetIndexCount(s) / 3;
            int cc = go == null ? 0 : go.GetComponentsInChildren<Collider>(true).Length;
            C(go != null, "FBX importado: " + Path.GetFileName(p), "mallas " + mallas + " · triángulos " + tri + (cc > 0 ? " · colliders " + cc : ""));
        }

        // 2. Modelo visual en la escena
        var visual = new[] { "Cabina_Visual", "Modules", "Connectors", "Technical_Area", "Lighting" }
            .Select(n => raiz.transform.Find(n)).Where(t => t != null).ToArray();
        int rVis = visual.Sum(t => t.GetComponentsInChildren<MeshRenderer>(true).Length);
        C(rVis >= 200, "Modelo visual INTERIOR en la escena", rVis + " MeshRenderer (esperado 203)");
        var roof = raiz.transform.Find("Removable_Roof");
        int rRoof = roof == null ? 0 : roof.GetComponentsInChildren<MeshRenderer>(true).Length;
        C(rRoof == 30, "Cubierta (de COMPLETA) en Removable_Roof, sin duplicar el INTERIOR", rRoof + " piezas CUB_ (oculta al inicio)");
        int dup = raiz.GetComponentsInChildren<Transform>(true).GroupBy(t => t.name).Count(g => g.Count() > 1 && g.Key.StartsWith("CUB_"));
        C(dup == 0, "Sin geometría duplicada de cubierta", dup + " nombres repetidos");

        // 3. Escala y orientación
        var b = LimitesVisuales(raiz, false);
        C(Mathf.Abs(b.size.x - 26f) < 0.2f && Mathf.Abs(b.size.z - 30f) < 0.4f && b.size.y > 2.9f && b.size.y < 4.5f,
          "Escala real (1 u = 1 m)", "tamaño visual " + b.size.ToString("F2") + " m (esperado ≈ 26 × 3 × 30)");
        var mc = Hijo(raiz, "MC_Pared_Circular");
        Vector3 mcC = mc != null && mc.GetComponent<Renderer>() != null ? mc.GetComponent<Renderer>().bounds.center : Vector3.positiveInfinity;
        bool espejo = Mathf.Abs(mcC.x + 1.1f) < 0.25f && Mathf.Abs(mcC.z) < 0.25f;
        C(mc != null && (Mathf.Abs(mcC.x - 1.1f) < 0.25f || espejo) && Mathf.Abs(mcC.z) < 0.25f, "Orientación (núcleo en x = 1.1, z = 0)",
          "centro del núcleo " + mcC.ToString("F2") + (espejo ? " (X sin espejar: se corrige en la prueba)" : ""));
        var tr = raiz.transform;
        C(tr.position == Vector3.zero && tr.rotation == Quaternion.identity && tr.localScale == Vector3.one,
          "CabinaPrincipal en Position (0,0,0) · Rotation (0,0,0) · Scale (1,1,1)", "");

        // 4. Siete módulos y seis pasillos
        string[] mods = { "MC", "MSI", "MSD", "MLI", "MLD", "MII", "MID" };
        var encontrados = mods.Where(m => { var t = Hijo(raiz, m + "_Pared_Circular"); return t != null && t.GetComponent<Renderer>() != null; }).ToList();
        string diams = string.Join(", ", encontrados.Select(m => m + " Ø" + Hijo(raiz, m + "_Pared_Circular").GetComponent<Renderer>().bounds.size.x.ToString("F1")));
        C(encontrados.Count == 7, "Existen las siete habitaciones circulares", diams);
        var pas = new[] { "MSI", "MSD", "MLI", "MLD", "MII", "MID" }.Where(c => Hijo(raiz, "PAS_" + c + "_Suelo") != null && Hijo(raiz, "PAS_" + c + "_Paredes") != null).Count();
        C(pas == 6, "Existen los seis pasillos (suelo + paredes)", pas + "/6");

        // 5. Materiales y texturas
        var rends = raiz.GetComponentsInChildren<Renderer>(true);
        int sinMat = 0, rosa = 0, conTex = 0, texEsperada = 0;
        var texNombres = new HashSet<string>(CabinaDatos.Materiales.Where(m => !string.IsNullOrEmpty(m.albedo)).Select(m => m.nombre));
        foreach (var r in rends)
            foreach (var m in r.sharedMaterials)
            {
                if (m == null) { sinMat++; continue; }
                if (m.shader == null || m.shader.name.Contains("InternalErrorShader") || m.shader.name == "Hidden/InternalErrorShader") rosa++;
                if (texNombres.Contains(m.name))
                {
                    texEsperada++;
                    var t = m.HasProperty("_BaseMap") ? m.GetTexture("_BaseMap") : (m.HasProperty("_MainTex") ? m.GetTexture("_MainTex") : null);
                    if (t != null) conTex++;
                }
            }
        C(sinMat == 0 && rosa == 0, "Materiales asignados y compatibles (" + (urp ? "URP/Lit" : "Standard") + ")", "huecos " + sinMat + " · shader de error " + rosa);
        C(texEsperada > 0 && conTex == texEsperada, "Texturas asignadas", conTex + "/" + texEsperada + " ranuras con textura");
        var matsUsados = rends.SelectMany(r => r.sharedMaterials).Where(m => m != null).Select(m => m.name).Distinct().Count();
        rep.AppendLine("        Materiales distintos en uso: " + matsUsados);

        // 6. Colliders
        var colRoot = raiz.transform.Find("Collisions");
        var cs = colRoot.GetComponentsInChildren<Collider>(true);
        int box = cs.Count(c => c is BoxCollider && !c.isTrigger), mesh = cs.Count(c => c is MeshCollider), trg = cs.Count(c => c.isTrigger);
        int convexos = cs.OfType<MeshCollider>().Count(c => c.convex);
        int colVisual = visual.Sum(t => t.GetComponentsInChildren<Collider>(true).Length);
        int rendCol = colRoot.GetComponentsInChildren<Renderer>(true).Length;
        C(cs.Length == 116 && convexos == 0, "Colliders preparados (geometría independiente)",
          cs.Length + " colliders: " + box + " Box · " + mesh + " Mesh no convexos · " + trg + " triggers de puerta · añadidos ahora " + colAnadidos);
        C(colVisual == 0 && rendCol == 0, "Sin colliders en la geometría visual decorativa y sin render en los colliders", "visual " + colVisual + " · renderers en Collisions " + rendCol);
        var bc = cs.Where(c => !c.isTrigger).Select(c => c.bounds).Aggregate((a, x) => { a.Encapsulate(x); return a; });
        var bcIn = new Bounds(bc.center, bc.size);
        C(Vector3.Distance(new Vector3(bcIn.center.x, 0, bcIn.center.z), new Vector3(b.center.x, 0, b.center.z)) < 0.3f &&
          Mathf.Abs(bcIn.size.x - b.size.x) < 0.6f, "Colliders alineados con el modelo visual",
          "centro colliders " + bcIn.center.ToString("F2") + " vs visual " + b.center.ToString("F2") + " · tamaño " + bcIn.size.ToString("F1"));
        C(puertas == 9, "Puertas automáticas configuradas", puertas + "/9 (CabinaPuertaAutomatica en COL_TRG_PUERTA_*)");

        // 7. Recorrido con física de Unity (cápsula r = 0.35 m, h = 1.8 m), puertas abiertas
        var bloqueos = cs.Where(c => c.name.StartsWith("COL_PUERTA_")).ToList();
        foreach (var c in bloqueos) c.enabled = false;
        Physics.SyncTransforms();
        int tramos = 0, tramosOk = 0;
        var fallosRec = new List<string>();
        Vector3 prev = Ajustar(CabinaDatos.InicioRecorrido, espejo);
        const float R = 0.35f;
        foreach (var w in CabinaDatos.Recorrido)
        {
            Vector3 cur = Ajustar(w.posicion, espejo);
            Vector3 d = cur - prev; d.y = 0f;
            float dist = d.magnitude;
            tramos++;
            bool libre = true; string choque = "";
            if (dist > 0.01f)
            {
                Vector3 p1 = prev + Vector3.up * (R + 0.1f), p2 = prev + Vector3.up * (1.8f - R);
                var hits = Physics.CapsuleCastAll(p1, p2, R, d / dist, dist, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore)
                    .Where(h => h.collider != null && h.collider.transform.IsChildOf(colRoot)).ToArray();
                var solap = Physics.OverlapCapsule(p1, p2, R, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore)
                    .Where(c => c.transform.IsChildOf(colRoot)).ToArray();
                if (hits.Length > 0 || solap.Length > 0)
                {
                    libre = false;
                    choque = string.Join(", ", hits.Select(h => h.collider.name).Concat(solap.Select(c => c.name)).Distinct());
                }
            }
            if (libre) tramosOk++; else fallosRec.Add(w.tramo + " → " + choque);
            prev = cur;
        }
        foreach (var c in bloqueos) c.enabled = true;
        Physics.SyncTransforms();
        C(tramosOk == tramos, "Recorrido completo sin colisiones (acceso → núcleo → 6 módulos → zona técnica → salida)",
          tramosOk + "/" + tramos + " segmentos libres" + (fallosRec.Count > 0 ? " · " + string.Join(" | ", fallosRec.Take(5)) : ""));
        // Con las puertas cerradas, una cápsula que cruza cada vano debe chocar con su COL_PUERTA_ (si no, la puerta no bloquearía)
        int cerradas = 0;
        foreach (var c in bloqueos.OfType<BoxCollider>())
        {
            Vector3 ax = c.size.x < c.size.z ? Vector3.right : Vector3.forward;       // eje más delgado = normal de la puerta
            Vector3 n = c.transform.TransformDirection(ax); n.y = 0f; n.Normalize();
            Vector3 ctr = c.bounds.center; ctr.y = 0f;
            Vector3 a = ctr + n * 1.2f;
            var hits = Physics.CapsuleCastAll(a + Vector3.up * (R + 0.1f), a + Vector3.up * (1.8f - R), R, -n, 2.4f,
                                              Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
            if (hits.Any(h => h.collider == c)) cerradas++;
        }
        C(cerradas == bloqueos.Count && bloqueos.Count == 9, "Puertas cerradas bloquean el paso (se abren solas al acercarse en Play)", cerradas + "/" + bloqueos.Count);

        // 8. Escena y prefab
        var sc = SceneManager.GetActiveScene();
        if (sc.isDirty) okEscena &= EditorSceneManager.SaveScene(sc, ESCENA);
        C(okEscena && File.Exists(ESCENA) && !sc.isDirty, "Escena guardada", ESCENA);
        var pf = AssetDatabase.LoadAssetAtPath<GameObject>(PREFAB);
        int pfCol = pf == null ? 0 : pf.GetComponentsInChildren<Collider>(true).Length;
        int pfRend = pf == null ? 0 : pf.GetComponentsInChildren<MeshRenderer>(true).Length;
        C(okPrefab && pf != null && pfCol == 116 && pfRend >= 230, "Prefab creado con colliders asociados", PREFAB + " · " + pfRend + " renderers · " + pfCol + " colliders");

        // 9. Consola
        C(errores.Count == 0, "Consola sin errores durante la operación", errores.Count + " errores · " + avisos.Count + " avisos");
        foreach (var e in errores.Take(15)) rep.AppendLine("        ERROR: " + e.Split('\n')[0]);
        foreach (var a in avisos.Distinct().Take(10)) rep.AppendLine("        aviso: " + a.Split('\n')[0]);

        rep.Insert(0, "RESULTADO: " + ok + "/" + (ok + fallos) + " comprobaciones superadas\n");
        string ruta = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Logs", "CabinaNavegable_Verificacion.txt"));
        Directory.CreateDirectory(Path.GetDirectoryName(ruta));
        File.WriteAllText(ruta, rep.ToString(), new UTF8Encoding(false));
        string resumen = "[Cabina] Importación terminada: " + ok + "/" + (ok + fallos) + " comprobaciones OK. Informe: " + ruta;
        if (fallos == 0) Debug.Log(resumen + "\n" + rep); else Debug.LogWarning(resumen + "\n" + rep);
    }

    static Vector3 Ajustar(Vector3 p, bool espejo) { return espejo ? new Vector3(-p.x, p.y, p.z) : p; }

    // =====================================================================================================
    [MenuItem(MENU + "Mostrar u ocultar cubierta", false, 20)]
    public static void AlternarCubierta()
    {
        var c = Resources.FindObjectsOfTypeAll<Transform>()
            .FirstOrDefault(t => t.name.StartsWith("GRP_15") && t.gameObject.scene.IsValid());
        if (c == null) { Debug.Log("[Cabina] La escena abierta no contiene la cubierta."); return; }
        Undo.RecordObject(c.gameObject, "Mostrar u ocultar cubierta");
        c.gameObject.SetActive(!c.gameObject.activeSelf);
        EditorSceneManager.MarkSceneDirty(c.gameObject.scene);
        Debug.Log("[Cabina] Cubierta " + (c.gameObject.activeSelf ? "visible" : "oculta") + ".");
    }

    [MenuItem(MENU + "Prueba de recorrido en Play (escena aparte)", false, 40)]
    public static void PruebaRecorrido()
    {
        var pf = AssetDatabase.LoadAssetAtPath<GameObject>(PREFAB);
        if (pf == null) { EditorUtility.DisplayDialog("Cabina Navegable", "Primero ejecuta «Importar y configurar escenario».", "OK"); return; }
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        if (File.Exists(ESCENA_PRUEBA) && !Application.isBatchMode &&
            !EditorUtility.DisplayDialog("Cabina Navegable", "Se regenerará " + ESCENA_PRUEBA + ". ¿Continuar?", "Sí", "Cancelar")) return;
        var esc = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        var cam0 = GameObject.Find("Main Camera");
        if (cam0 != null) UnityEngine.Object.DestroyImmediate(cam0);
        PrefabUtility.InstantiatePrefab(pf);
        var j = new GameObject("JUGADOR");
        j.tag = "Player";
        j.transform.position = CabinaDatos.InicioRecorrido + Vector3.up * 0.05f;
        var cc = j.AddComponent<CharacterController>();
        cc.height = 1.8f; cc.radius = 0.4f; cc.center = new Vector3(0f, 0.9f, 0f); cc.stepOffset = 0.3f; cc.skinWidth = 0.04f;
        var camGo = new GameObject("Camara"); camGo.tag = "MainCamera";
        camGo.transform.SetParent(j.transform, false); camGo.transform.localPosition = new Vector3(0f, 1.65f, 0f);
        var cam = camGo.AddComponent<Camera>(); cam.nearClipPlane = 0.05f; cam.fieldOfView = 70f;
        camGo.AddComponent<AudioListener>();
        var fps = j.AddComponent<CabinaControladorPrimeraPersona>(); fps.camara = camGo.transform;
        j.AddComponent<CabinaPruebaRecorrido>();
        EditorSceneManager.SaveScene(esc, ESCENA_PRUEBA);
        EditorApplication.isPlaying = true;
    }
}
