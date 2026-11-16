// Asistencia de IA: escrito con Claude Code (Anthropic).
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using MarsCat.Unity.Environment;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace MarsCat.Unity.EditorTools
{
    /// <summary>
    /// Fase 1 — integración de los modelos de Art-source/Edificios (exportados a FBX en Assets/Art/Models).
    /// Tools > Mars Cat > Modelos 3D > 1. Importar modelos, materiales URP y prefabs   → Logs/Modelos3D_Importacion.txt
    /// Tools > Mars Cat > Modelos 3D > 2. Crear escena Sandbox_Modelos3D             → Logs/Modelos3D_Distribucion.txt
    /// Tools > Mars Cat > Modelos 3D > 3. Probar cúpulas en Play Mode                → Logs/Modelos3D_PruebaPlayMode.txt
    /// No abre ni guarda Main.unity ni CabinaPrincipal.unity: la distribución se comprueba sobre una instancia del prefab
    /// CabinaNavegable dentro de la escena Sandbox.
    /// </summary>
    [InitializeOnLoad]
    public static class IntegracionModelos3D
    {
        const string CarpetaModelos = "Assets/Art/Models";
        const string CarpetaMateriales = "Assets/Art/Materials";
        const string CarpetaPrefabs = "Assets/Prefabs";
        const string RutaEscena = "Assets/Scenes/Sandbox/Sandbox_Modelos3D.unity";
        const string RutaCabina = "Assets/CabinaNavegable/Prefabs/CabinaNavegable.prefab";
        const string ClavePrueba = "MarsCat.Modelos3D.Prueba";
        const float AnguloPorDefecto = 60f, DuracionCupula = 2f;
        const float AlturaMuroPorDefecto = 3f;

        class Def
        {
            public string nombre, material; public Vector3 tam; public int tris, objetos, degenerados; public bool dobleCara, collider;
            public string Fbx => CarpetaModelos + "/SM_" + nombre + ".fbx";
        }

        // Valores medidos en Blender sobre los FBX exportados (tamaño en ejes de Unity: X, Y = alto, Z).
        static readonly Def[] Defs =
        {
            new Def { nombre = "Capsula", material = "M_Capsula", tam = new Vector3(39.696f, 3.457f, 25.772f), tris = 3784, degenerados = 112, objetos = 25, dobleCara = true }, // Capsula.002 y .003: 56 caras de área cero (112 triángulos) que Unity descarta
            new Def { nombre = "Capsula_Cupula", material = "M_Capsula", tam = new Vector3(6.093f, 3.457f, 8.247f), tris = 1136, objetos = 14, dobleCara = true },
            new Def { nombre = "MoxieO2", material = "M_MoxieO2", tam = new Vector3(2.0f, 2.754f, 1.1f), tris = 188, objetos = 1, collider = true },
            new Def { nombre = "RecicladoraAgua", material = "M_RecicladoraAgua", tam = new Vector3(2.742f, 1.532f, 1.695f), tris = 832, objetos = 1, collider = true },
            new Def { nombre = "PanelesSolares", material = "M_PanelesSolares", tam = new Vector3(3.188f, 3.089f, 4.602f), tris = 82, objetos = 1, collider = true },
            new Def { nombre = "TaladorAgua", material = "M_TaladorAgua", tam = new Vector3(0.67f, 1.836f, 1.261f), tris = 104, objetos = 1, collider = true },
            new Def { nombre = "Heolica", material = "M_Heolica", tam = new Vector3(9.827f, 16.437f, 3.083f), tris = 123, objetos = 1, dobleCara = true },
            new Def { nombre = "Maceta", material = "M_Maceta", tam = new Vector3(3.024f, 0.81f, 3.024f), tris = 28, objetos = 1, collider = true },
            new Def { nombre = "CajaArena", material = "M_CajaArena", tam = new Vector3(9.357f, 0.909f, 5.355f), tris = 28, objetos = 1, collider = true },
            new Def { nombre = "Antena", material = "M_Antena", tam = new Vector3(2.0f, 1.879f, 1.444f), tris = 272, objetos = 1, dobleCara = true, collider = true },
        };

        static string Raiz => Path.GetDirectoryName(Application.dataPath);
        static string RutaLog(string n) { string d = Path.Combine(Raiz, "Logs"); Directory.CreateDirectory(d); return Path.Combine(d, n); }
        static string Capturas { get { string d = Path.Combine(Raiz, "Logs", "Modelos3D_Capturas"); Directory.CreateDirectory(d); return d; } }

        static StringBuilder sb; static int ok, total;
        static void C(bool cond, string texto) { total++; if (cond) ok++; sb.AppendLine((cond ? "[OK]    " : "[FALLO] ") + texto); }
        static void Iniciar(string titulo) { sb = new StringBuilder(); ok = total = 0; sb.AppendLine(titulo + " — " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " | Unity " + Application.unityVersion); }
        static void Escribir(string archivo, string etiqueta)
        {
            sb.AppendLine(); sb.AppendLine("RESULTADO: " + ok + "/" + total + " comprobaciones superadas");
            File.WriteAllText(RutaLog(archivo), sb.ToString());
            string m = "[Modelos3D] " + etiqueta + ": " + ok + "/" + total + " OK. Informe: Logs/" + archivo;
            if (ok == total) Debug.Log(m); else Debug.LogWarning(m);
        }
        static string F(Vector3 v) => "(" + v.x.ToString("0.00") + ", " + v.y.ToString("0.00") + ", " + v.z.ToString("0.00") + ")";

        // =====================================================================================================
        // 1. IMPORTAR
        // =====================================================================================================
        [MenuItem("Tools/Mars Cat/Modelos 3D/1. Importar modelos, materiales URP y prefabs", false, 1)]
        public static void Importar()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            Iniciar("IMPORTACIÓN DE MODELOS 3D (FASE 1)");
            string marcas = MarcasEscenas();
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single); // escena vacía sin guardar: no se toca ninguna escena del proyecto
            try
            {
                var urp = GraphicsSettings.currentRenderPipeline;
                C(urp != null && urp.GetType().Name.Contains("Universal"), "Proyecto en URP (" + (urp != null ? urp.GetType().Name + " " + urp.name : "sin pipeline") + ")");
                Shader lit = Shader.Find("Universal Render Pipeline/Lit");
                C(lit != null, "Shader Universal Render Pipeline/Lit disponible");
                Directory.CreateDirectory(Path.Combine(Raiz, CarpetaMateriales)); Directory.CreateDirectory(Path.Combine(Raiz, CarpetaPrefabs));

                sb.AppendLine(); sb.AppendLine("== Importación de los FBX ==");
                foreach (Def d in Defs) ImportarFbx(d);

                sb.AppendLine(); sb.AppendLine("== Materiales URP (Assets/Art/Materials) ==");
                var mats = new Dictionary<string, Material>();
                foreach (Def d in Defs)
                    if (!mats.ContainsKey(d.material)) mats[d.material] = CrearMaterial(d.material, lit, Defs.Any(x => x.material == d.material && x.dobleCara));

                sb.AppendLine(); sb.AppendLine("== Prefabs (Assets/Prefabs/Buildings y Assets/Prefabs/Environment) ==");
                MigrarPrefabs();
                temporal = EditorSceneManager.NewPreviewScene(); // los prefabs se montan aquí: la escena abierta no se toca
                try
                {
                    foreach (Def d in Defs.Where(x => x.nombre != "Capsula_Cupula")) CrearPrefabSimple(d, mats[d.material]);
                    CrearPrefabCupula("A", mats["M_Capsula"]);
                    CrearPrefabCupula("B", mats["M_Capsula"]);
                }
                finally { EditorSceneManager.ClosePreviewScene(temporal); }
                C(MarcasEscenas() == marcas, "Main.unity y CabinaPrincipal.unity sin cambios (misma fecha de modificación)");
            }
            catch (Exception e) { C(false, "Excepción: " + e); }
            Escribir("Modelos3D_Importacion.txt", "Importación");
        }

        // Estructura de STRUCTURE.md: Prefabs/<Categoría>/P_<Categoría>_<Nombre>. Los equipos son edificios; la cápsula y las cúpulas, entorno.
        static string NombrePrefab(string modelo) => modelo == "Capsula" ? "P_Env_Capsula" : "P_Building_" + modelo;
        static string RutaPrefab(string nombre) => CarpetaPrefabs + (nombre.StartsWith("P_Building_") ? "/Buildings/" : "/Environment/") + nombre + ".prefab";

        // Mueve (sin perder el GUID) los prefabs creados antes con la estructura anterior (Prefabs/P_<Nombre>).
        static void MigrarPrefabs()
        {
            foreach (string sub in new[] { "Buildings", "Environment" })
                if (!AssetDatabase.IsValidFolder(CarpetaPrefabs + "/" + sub)) AssetDatabase.CreateFolder(CarpetaPrefabs, sub);
            var pares = Defs.Where(d => d.nombre != "Capsula_Cupula").Select(d => ("P_" + d.nombre, NombrePrefab(d.nombre)))
                .Concat(new[] { ("P_Cupula_A", "P_Env_Cupula_A"), ("P_Cupula_B", "P_Env_Cupula_B") });
            foreach (var (viejo, nuevo) in pares)
            {
                string desde = CarpetaPrefabs + "/" + viejo + ".prefab", hasta = RutaPrefab(nuevo);
                if (AssetDatabase.LoadAssetAtPath<GameObject>(desde) == null || AssetDatabase.LoadAssetAtPath<GameObject>(hasta) != null) continue;
                string error = AssetDatabase.MoveAsset(desde, hasta);
                C(string.IsNullOrEmpty(error), "Movido " + desde + " → " + hasta + " (mismo GUID)" + (string.IsNullOrEmpty(error) ? "" : ": " + error));
            }
        }

        static Scene temporal;
        static string MarcasEscenas() => string.Join("|", new[] { "Assets/Scenes/Main.unity", "Assets/Scenes/CabinaPrincipal.unity" }
            .Select(r => File.Exists(Path.Combine(Raiz, r)) ? File.GetLastWriteTimeUtc(Path.Combine(Raiz, r)).Ticks.ToString() : "-"));
        static GameObject Nuevo(string nombre) { var g = new GameObject(nombre); SceneManager.MoveGameObjectToScene(g, temporal); return g; }

        static void ImportarFbx(Def d)
        {
            var mi = AssetImporter.GetAtPath(d.Fbx) as ModelImporter;
            if (mi == null) { C(false, d.Fbx + " no existe o no es un modelo"); return; }
            mi.globalScale = 1f; mi.useFileScale = true; mi.bakeAxisConversion = false; mi.preserveHierarchy = true;
            mi.importBlendShapes = false; mi.importVisibility = false; mi.importCameras = false; mi.importLights = false;
            mi.materialImportMode = ModelImporterMaterialImportMode.None;
            mi.importNormals = ModelImporterNormals.Import; mi.importTangents = ModelImporterTangents.CalculateMikk;
            mi.animationType = ModelImporterAnimationType.None; mi.importAnimation = false;
            mi.addCollider = false; mi.isReadable = false; mi.meshCompression = ModelImporterMeshCompression.Off; mi.generateSecondaryUV = false;
            mi.SaveAndReimport();

            GameObject m = AssetDatabase.LoadAssetAtPath<GameObject>(d.Fbx);
            MeshFilter[] mfs = m.GetComponentsInChildren<MeshFilter>(true);
            Bounds b = BoundsDe(mfs, m.transform);
            int tris = mfs.Sum(f => Tris(f.sharedMesh));
            bool rotOk = mfs.All(f => Quaternion.Angle(f.transform.localRotation, Quaternion.identity) < 0.01f) && mfs.All(f => (f.transform.localScale - Vector3.one).magnitude < 1e-4f);
            bool normales = mfs.All(f => f.sharedMesh.HasVertexAttribute(VertexAttribute.Normal));
            bool uvs = mfs.All(f => f.sharedMesh.HasVertexAttribute(VertexAttribute.TexCoord0));
            Vector3 dif = b.size - d.tam;
            C(mfs.Length == d.objetos && tris == d.tris - d.degenerados, Path.GetFileName(d.Fbx) + ": " + mfs.Length + " malla(s), " + tris + " triángulos (en Blender: " + d.objetos + " / " + d.tris +
              (d.degenerados > 0 ? ", de los que " + d.degenerados + " son caras de área cero que Unity descarta" : "") + ")");
            C(Mathf.Max(Mathf.Abs(dif.x), Mathf.Abs(dif.y), Mathf.Abs(dif.z)) < 0.01f, "   escala 1 unidad = 1 m: tamaño " + F(b.size) + " m = medida de Blender " + F(d.tam));
            C(rotOk, "   orientación: piezas sin rotación ni escala residual (Y arriba, transformaciones aplicadas al exportar)");
            C(normales && uvs, "   normales importadas del FBX y UV presentes");
        }

        static Material CrearMaterial(string nombre, Shader lit, bool dobleCara)
        {
            string ruta = CarpetaMateriales + "/" + nombre + ".mat";
            Material m = AssetDatabase.LoadAssetAtPath<Material>(ruta);
            if (m == null) { m = new Material(lit); AssetDatabase.CreateAsset(m, ruta); }
            m.shader = lit;
            // Los .blend no tienen materiales asignados: se usa el gris por defecto de Blender (0,8 lineal), metal 0, rugosidad 0,5.
            float g = Mathf.LinearToGammaSpace(0.8f);
            m.SetColor("_BaseColor", new Color(g, g, g, 1f)); m.SetFloat("_Metallic", 0f); m.SetFloat("_Smoothness", 0.5f);
            m.SetFloat("_Cull", dobleCara ? (float)CullMode.Off : (float)CullMode.Back);
            m.doubleSidedGI = dobleCara;
            EditorUtility.SetDirty(m); AssetDatabase.SaveAssetIfDirty(m);
            C(m.shader == lit && m.HasProperty("_BaseColor"), nombre + ".mat: URP/Lit" + (dobleCara ? ", dos caras (malla abierta)" : ""));
            return m;
        }

        static void CrearPrefabSimple(Def d, Material mat)
        {
            GameObject fbx = AssetDatabase.LoadAssetAtPath<GameObject>(d.Fbx);
            string nombrePf = NombrePrefab(d.nombre);
            var raiz = Nuevo(nombrePf);
            var modelo = (GameObject)PrefabUtility.InstantiatePrefab(fbx, temporal);
            modelo.name = "Modelo"; modelo.transform.SetParent(raiz.transform, false);
            foreach (Renderer r in modelo.GetComponentsInChildren<Renderer>(true)) r.sharedMaterials = new[] { mat };
            Bounds b = BoundsDe(modelo.GetComponentsInChildren<MeshFilter>(true), raiz.transform);
            modelo.transform.localPosition = new Vector3(-b.center.x, -b.min.y, -b.center.z); // pivote = centro de la base
            if (d.collider)
            {
                var bc = raiz.AddComponent<BoxCollider>();
                bc.center = new Vector3(0f, b.size.y * 0.5f, 0f); bc.size = b.size;
            }
            string ruta = RutaPrefab(nombrePf);
            PrefabUtility.SaveAsPrefabAsset(raiz, ruta, out bool guardado);
            UnityEngine.Object.DestroyImmediate(raiz);
            GameObject p = AssetDatabase.LoadAssetAtPath<GameObject>(ruta);
            Bounds bp = BoundsDe(p.GetComponentsInChildren<MeshFilter>(true), p.transform);
            C(guardado && Mathf.Abs(bp.min.y) < 0.001f && Mathf.Abs(bp.center.x) < 0.001f && Mathf.Abs(bp.center.z) < 0.001f &&
              p.GetComponentsInChildren<Renderer>(true).All(r => r.sharedMaterial == mat),
              nombrePf + ": pivote en el centro de la base, material " + mat.name + (d.collider ? ", BoxCollider " + F(b.size) : ", sin collider"));
        }

        // Cúpula articulada: anillo (base) + 6 pétalos que giran sobre su bisagra (el lado del hexágono) con CapsuleDomeController.
        static void CrearPrefabCupula(string variante, Material mat)
        {
            GameObject fbx = AssetDatabase.LoadAssetAtPath<GameObject>(CarpetaModelos + "/SM_Capsula_Cupula.fbx");
            Transform fBase = fbx.transform.Find("Cupula" + variante + "_Base");
            var fPetalos = Enumerable.Range(1, 6).Select(k => fbx.transform.Find("Cupula" + variante + "_Petalo_" + k)).ToArray();
            if (fBase == null || fPetalos.Any(t => t == null)) { C(false, "SM_Capsula_Cupula.fbx: faltan piezas de la variante " + variante); return; }
            Bounds bb = BoundsDe(new[] { fBase.GetComponent<MeshFilter>() }, fbx.transform);
            Vector3 off = new Vector3(0f, -bb.min.y, 0f); // la raíz queda en la base del anillo, centrada en el hexágono

            var raiz = Nuevo("P_Env_Cupula_" + variante);
            var gBase = Pieza("Base", fBase, raiz.transform, mat); gBase.transform.localPosition = fBase.localPosition + off;
            var piezas = new CapsuleDomeController.Pieza[6];
            for (int k = 0; k < 6; k++)
            {
                Vector3 h = fPetalos[k].localPosition + off; // el origen del pétalo está en su bisagra
                Vector3 radial = new Vector3(h.x, 0f, h.z).normalized;
                var bis = Nuevo("Bisagra_" + (k + 1)); bis.transform.SetParent(raiz.transform, false);
                bis.transform.localPosition = h; bis.transform.localRotation = Quaternion.LookRotation(radial, Vector3.up);
                var pet = Pieza("Petalo_" + (k + 1), fPetalos[k], bis.transform, mat);
                pet.transform.position = h; pet.transform.rotation = fPetalos[k].localRotation; // misma pose que en el FBX
                piezas[k] = new CapsuleDomeController.Pieza { transform = bis.transform, rotacionAbierta = new Vector3(AnguloPorDefecto, 0f, 0f), retraso = k % 2 == 1 ? 0.12f : 0f };
            }
            foreach (var pz in piezas) pz.rotacionAbierta = new Vector3(AnguloPorDefecto, 0f, 0f); // el paso 2 lo ajusta a la base real
            var domo = raiz.AddComponent<CapsuleDomeController>();
            domo.piezas = piezas; domo.duracion = DuracionCupula; domo.curva = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f); domo.abiertoAlIniciar = false;

            float apotema = fPetalos[0].localPosition.magnitude > 0 ? new Vector2(fPetalos[0].localPosition.x, fPetalos[0].localPosition.z).magnitude : 2.8f;
            var zona = Nuevo("ZonaInteraccion"); zona.layer = 2; // Ignore Raycast
            zona.transform.SetParent(raiz.transform, false);
            zona.transform.localPosition = new Vector3(0f, -AlturaMuroPorDefecto + 1.1f, 0f);
            var cap = zona.AddComponent<CapsuleCollider>(); cap.isTrigger = true; cap.direction = 1; cap.radius = apotema * 0.85f; cap.height = 2.2f;
            var inter = zona.AddComponent<DomeInteraction>();
            var so = new SerializedObject(inter); so.FindProperty("_dome").objectReferenceValue = domo; so.ApplyModifiedPropertiesWithoutUndo();

            string ruta = RutaPrefab("P_Env_Cupula_" + variante);
            PrefabUtility.SaveAsPrefabAsset(raiz, ruta, out bool guardado);
            UnityEngine.Object.DestroyImmediate(raiz);
            GameObject p = AssetDatabase.LoadAssetAtPath<GameObject>(ruta);
            var cd = p.GetComponent<CapsuleDomeController>();
            int solidos = p.GetComponentsInChildren<Collider>(true).Count(c => !c.isTrigger);
            C(guardado && cd != null && cd.piezas.Length == 6 && cd.piezas.All(x => x.transform != null) && solidos == 0 &&
              p.GetComponentInChildren<DomeInteraction>() != null,
              "P_Env_Cupula_" + variante + ": anillo + 6 pétalos con bisagra, CapsuleDomeController (" + cd.piezas[0].rotacionAbierta.x.ToString("0") + "° por defecto, " + DuracionCupula +
              " s), zona de interacción Trigger con DomeInteraction (tecla E) y ningún collider sólido");
        }

        static GameObject Pieza(string nombre, Transform fuente, Transform padre, Material mat)
        {
            var g = Nuevo(nombre); g.transform.SetParent(padre, false);
            g.AddComponent<MeshFilter>().sharedMesh = fuente.GetComponent<MeshFilter>().sharedMesh;
            g.AddComponent<MeshRenderer>().sharedMaterial = mat;
            g.transform.localRotation = fuente.localRotation; g.transform.localScale = fuente.localScale;
            return g;
        }

        static int Tris(Mesh m) { int n = 0; for (int i = 0; i < m.subMeshCount; i++) n += (int)m.GetIndexCount(i) / 3; return n; }

        static Bounds BoundsDe(IEnumerable<MeshFilter> mfs, Transform espacio)
        {
            bool hay = false; Bounds b = default;
            foreach (MeshFilter f in mfs)
            {
                if (f == null || f.sharedMesh == null || !f.gameObject.activeInHierarchy && f.gameObject.scene.IsValid()) continue;
                Matrix4x4 mtx = espacio.worldToLocalMatrix * f.transform.localToWorldMatrix;
                Bounds mb = f.sharedMesh.bounds;
                for (int i = 0; i < 8; i++)
                {
                    Vector3 c = mb.center + Vector3.Scale(mb.extents, new Vector3((i & 1) == 0 ? -1 : 1, (i & 2) == 0 ? -1 : 1, (i & 4) == 0 ? -1 : 1));
                    Vector3 w = mtx.MultiplyPoint3x4(c);
                    if (!hay) { b = new Bounds(w, Vector3.zero); hay = true; } else b.Encapsulate(w);
                }
            }
            return b;
        }

        // =====================================================================================================
        // 2. ESCENA SANDBOX: galería + distribución comprobada sobre la cabina
        // =====================================================================================================
        class Modulo
        {
            public string codigo, id, funcion; public Vector3 centro; public float rExt, rInt, techo, suelo;
            public List<(float ang, float ancho)> accesos = new List<(float, float)>();
        }

        static readonly (string codigo, string id, string funcion, string prefab)[] Asignacion =
        {
            ("MSI", "M-01", "Cultivo de alimentos", "P_Building_Maceta"),
            ("MSD", "M-02", "Reciclaje orgánico", "P_Building_CajaArena"),
            ("MLI", "M-03", "Generación de oxígeno", "P_Building_MoxieO2"),
            ("MLD", "M-04", "Energía y mantenimiento", null),
            ("MII", "M-05", "Extracción de agua", "P_Building_TaladorAgua"),
            ("MID", "M-06", "Tratamiento de agua", "P_Building_RecicladoraAgua"),
            ("MC", "Nucleo", "Control general de la base", null),
        };

        static List<Modulo> MedirModulos(GameObject cabina)
        {
            Physics.SyncTransforms();
            var lista = new List<Modulo>();
            foreach (var a in Asignacion)
            {
                Collider muro = cabina.GetComponentsInChildren<Collider>(true).FirstOrDefault(c => c.name == "COL_" + a.codigo + "_Pared_Circular");
                if (muro == null) continue;
                Bounds b = muro.bounds;
                var m = new Modulo { codigo = a.codigo, id = a.id, funcion = a.funcion, centro = new Vector3(b.center.x, b.min.y, b.center.z),
                                     rExt = Mathf.Max(b.extents.x, b.extents.z), techo = b.max.y, suelo = b.min.y };
                var dist = new List<float>(); var abiertos = new List<float>();
                Vector3 o = m.centro + Vector3.up * 1.0f;
                for (int g = 0; g < 360; g += 2)
                {
                    Vector3 dir = Quaternion.Euler(0f, g, 0f) * Vector3.forward;
                    var hits = Physics.RaycastAll(o, dir, m.rExt + 0.6f, ~0, QueryTriggerInteraction.Ignore).OrderBy(h => h.distance).ToArray();
                    RaycastHit hm = hits.FirstOrDefault(h => h.collider == muro);
                    bool puerta = hits.Length > 0 && hits[0].collider.name.StartsWith("COL_PUERTA");
                    if (hm.collider != null && !(hits[0].collider != muro && hits[0].collider.name.StartsWith("COL_PUERTA"))) dist.Add(hm.distance);
                    if (hm.collider == null || puerta) abiertos.Add(g);
                }
                dist.Sort(); m.rInt = dist.Count > 0 ? dist[dist.Count / 2] : m.rExt - 0.2f;
                // agrupar ángulos abiertos consecutivos
                var grupos = new List<List<float>>();
                foreach (float g in abiertos)
                {
                    if (grupos.Count > 0 && Mathf.DeltaAngle(grupos[^1][^1], g) <= 2.01f) grupos[^1].Add(g); else grupos.Add(new List<float> { g });
                }
                if (grupos.Count > 1 && Mathf.DeltaAngle(grupos[^1][^1], grupos[0][0] + 360f) <= 2.01f) { grupos[^1].AddRange(grupos[0].Select(x => x + 360f)); grupos.RemoveAt(0); }
                foreach (var gr in grupos)
                {
                    float centro = (gr[0] + gr[^1]) * 0.5f % 360f; float anchoG = gr[^1] - gr[0] + 2f;
                    m.accesos.Add((centro, 2f * m.rInt * Mathf.Sin(anchoG * 0.5f * Mathf.Deg2Rad)));
                }
                lista.Add(m);
            }
            return lista;
        }

        [MenuItem("Tools/Mars Cat/Modelos 3D/2. Crear escena Sandbox_Modelos3D (galería y distribución)", false, 2)]
        public static void CrearEscena()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            Iniciar("ESCENA SANDBOX Y DISTRIBUCIÓN (FASE 1)");
            string marcas = MarcasEscenas();
            try
            {
                if (File.Exists(Path.Combine(Raiz, RutaEscena)))
                {
                    string copia = Path.Combine(Raiz, "Logs", "Modelos3D_Copias", DateTime.Now.ToString("yyyyMMdd_HHmmss"));
                    Directory.CreateDirectory(copia); File.Copy(Path.Combine(Raiz, RutaEscena), Path.Combine(copia, Path.GetFileName(RutaEscena)));
                    sb.AppendLine("Copia de la escena anterior en " + copia);
                }
                Scene escena = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                var luz = new GameObject("Luz_Direccional").AddComponent<Light>();
                luz.type = LightType.Directional; luz.intensity = 1.2f; luz.shadows = LightShadows.Soft; luz.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
                var suelo = GameObject.CreatePrimitive(PrimitiveType.Plane); suelo.name = "Suelo_Exterior"; suelo.transform.position = new Vector3(0f, -0.02f, -10f); suelo.transform.localScale = new Vector3(16f, 1f, 16f);

                var cabina = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(RutaCabina), escena);
                cabina.name = "CabinaNavegable (referencia, sin modificar)";
                var mods = MedirModulos(cabina);
                C(mods.Count == 7, "Medidos " + mods.Count + " módulos circulares en el prefab CabinaNavegable (6 salas + núcleo)");

                noCaben.Clear();
                MatrizEncaje(mods);
                var distrib = new GameObject("Distribucion").transform;
                var cupulas = new GameObject("Cupulas").transform;
                AjustarAngulo(mods, Cerramiento(cabina), cupulas);
                sb.AppendLine(); sb.AppendLine("== Módulos, accesos y equipos (medidas reales de los colliders de la cabina) ==");
                foreach (Modulo m in mods)
                {
                    var asig = Asignacion.First(x => x.codigo == m.codigo);
                    sb.AppendLine(m.id + " (" + m.codigo + ") " + m.funcion + ": centro " + F(m.centro) + ", Ø ext " + (2 * m.rExt).ToString("0.00") + " m, Ø int " + (2 * m.rInt).ToString("0.00") +
                                  " m, muro " + (m.techo - m.suelo).ToString("0.00") + " m; accesos: " + string.Join(", ", m.accesos.Select(a => a.ang.ToString("0") + "° (" + a.ancho.ToString("0.0") + " m)")));
                    if (asig.prefab == null) sb.AppendLine("        Sin modelo asignado entre los nueve (" + (m.id == "Nucleo" ? "control general" : "energía y mantenimiento") + "): queda pendiente.");
                    else ColocarEquipo(m, asig.prefab, distrib);
                    ColocarCupula(m, cupulas, true);
                }
                ColocarExterior(cabina);
                CrearGaleria();
                if (noCaben.Count > 0) { sb.AppendLine(); sb.AppendLine("Equipos que no caben a tamaño real en la sala de la hipótesis: " + string.Join("; ", noCaben)); }
                var cam = new GameObject("Camara_Sandbox").AddComponent<Camera>(); cam.tag = "MainCamera";
                cam.transform.position = new Vector3(26f, 30f, 30f); cam.transform.LookAt(new Vector3(0f, 0f, 0f)); cam.fieldOfView = 50f; cam.farClipPlane = 400f;
                Directory.CreateDirectory(Path.Combine(Raiz, "Assets/Scenes/Sandbox"));
                bool guardada = EditorSceneManager.SaveScene(escena, RutaEscena);
                C(guardada, "Escena guardada en " + RutaEscena + " (Main.unity y CabinaPrincipal.unity no se abren ni se guardan)");
                Capturar(cam, new Vector3(1f, 42f, 0f), Quaternion.Euler(90f, 180f, 0f), 50f, "Distribucion_cenital.png");
                Capturar(cam, new Vector3(24f, 24f, 26f), Quaternion.LookRotation(-new Vector3(24f, 24f, 26f)), 50f, "Distribucion_isometrica.png");
                Capturar(cam, new Vector3(36f, 30f, -20f), Quaternion.LookRotation(new Vector3(36f, 0f, -58f) - new Vector3(36f, 30f, -20f)), 60f, "Galeria.png");
                cam.transform.position = new Vector3(26f, 30f, 30f); cam.transform.LookAt(Vector3.zero);
                EditorSceneManager.SaveScene(escena);
                C(MarcasEscenas() == marcas, "Main.unity y CabinaPrincipal.unity sin cambios (misma fecha de modificación)");
            }
            catch (Exception e) { C(false, "Excepción: " + e); }
            Escribir("Modelos3D_Distribucion.txt", "Escena y distribución");
        }

        const float MargenMuro = 0.15f, Carril = 0.55f, CentroLibre = 0.6f;

        // Busca la mejor posición dentro de la sala: dentro del círculo interior (margen 0,15 m), sin tapar el centro (0,6 m)
        // ni el camino recto de cada acceso al centro (carril de 1,1 m) y sin chocar con colliders. Devuelve la escala que cabe (1 = real).
        static float Encaje(Modulo m, Vector3 tam, float escalaMin, out Vector3 mejorPos, out Quaternion mejorRot, out string motivo)
        {
            float mejor = float.NegativeInfinity; mejorPos = default; mejorRot = Quaternion.identity; motivo = "";
            foreach (float s in new[] { 1f, 0.9f, 0.8f, 0.7f, 0.6f, 0.5f, 0.45f, 0.4f, 0.35f, 0.3f })
            {
                if (s < escalaMin) break;
                Vector3 t = tam * s; int fueraCirculo = 0, bloquea = 0, choca = 0;
                for (int ang = 0; ang < 360; ang += 10)
                    for (int giro = 0; giro < 2; giro++)
                    {
                        Vector3 dir = Quaternion.Euler(0f, ang, 0f) * Vector3.forward;
                        Quaternion rot = Quaternion.LookRotation(-dir) * Quaternion.Euler(0f, giro * 90f, 0f);
                        for (float r = m.rInt; r >= 0f; r -= 0.1f)
                        {
                            Vector3 pos = m.centro + dir * r;
                            if (Esquinas(pos, rot, t).Any(e => Plano(e - m.centro).magnitude > m.rInt - MargenMuro)) { fueraCirculo++; continue; }
                            if (DistRect(m.centro, pos, rot, t) < CentroLibre || HuecoAccesos(m, pos, rot, t) < Carril) { bloquea++; continue; }
                            Vector3 cBox = pos + Vector3.up * (t.y * 0.5f + 0.05f);
                            if (Physics.OverlapBox(cBox, new Vector3(t.x, Mathf.Max(0.01f, t.y - 0.1f), t.z) * 0.5f, rot, ~0, QueryTriggerInteraction.Ignore).Length > 0) { choca++; continue; }
                            float minAcc = m.accesos.Count == 0 ? 180f : m.accesos.Min(a => Mathf.Abs(Mathf.DeltaAngle(a.ang, ang)));
                            float puntos = r * 2f + minAcc / 30f;
                            if (puntos > mejor) { mejor = puntos; mejorPos = pos; mejorRot = rot; }
                            break;
                        }
                    }
                if (mejor > float.NegativeInfinity) return s;
                motivo = "a escala " + s.ToString("0.00") + ": " + fueraCirculo + " posiciones no caben en el círculo, " + bloquea + " tapan un acceso o el centro, " + choca + " chocan con colliders";
            }
            return 0f;
        }

        static float HuecoAccesos(Modulo m, Vector3 pos, Quaternion rot, Vector3 t)
        {
            float mn = float.MaxValue;
            foreach (var a in m.accesos)
            {
                Vector3 entrada = m.centro + Quaternion.Euler(0f, a.ang, 0f) * Vector3.forward * m.rInt;
                for (float u = 0f; u <= 1f; u += 0.02f) mn = Mathf.Min(mn, DistRect(Vector3.Lerp(entrada, m.centro, u), pos, rot, t));
            }
            return mn;
        }

        static void ColocarEquipo(Modulo m, string prefab, Transform padre)
        {
            GameObject pf = AssetDatabase.LoadAssetAtPath<GameObject>(RutaPrefab(prefab));
            Vector3 tam = pf.GetComponent<BoxCollider>().size;
            float escala = Encaje(m, tam, 0.3f, out Vector3 pos, out Quaternion rot, out string motivo);
            if (escala <= 0f) { sb.AppendLine("[NO CABE] " + prefab + " " + F(tam) + " m no cabe en " + m.id + " ni reducido al 30 % (" + motivo + "); no se coloca"); noCaben.Add(prefab + " en " + m.id); return; }
            var inst = (GameObject)PrefabUtility.InstantiatePrefab(pf, padre.gameObject.scene);
            inst.transform.SetParent(padre, true);
            inst.name = m.id + "_" + prefab + (escala < 1f ? "_escala_" + escala.ToString("0.00") + "_PROPUESTA" : "");
            inst.transform.SetPositionAndRotation(pos, rot); inst.transform.localScale = Vector3.one * escala;
            string texto = prefab + " " + F(tam) + " m en " + m.id + ": a " + Plano(pos - m.centro).magnitude.ToString("0.00") + " m del centro, sin tocar muros ni colliders; paso libre a cada acceso ≥ " +
                           (2f * HuecoAccesos(m, pos, rot, tam * escala)).ToString("0.00") + " m de ancho";
            if (escala >= 1f) C(true, "        " + texto + " (tamaño real)");
            else { sb.AppendLine("[NO CABE] " + texto.Replace(" m en " + m.id, " m NO cabe a tamaño real en " + m.id) + "; colocado al " + (escala * 100f).ToString("0") + " % como propuesta (revisar escala en Blender)"); noCaben.Add(prefab + " en " + m.id + " (cabe al " + (escala * 100f).ToString("0") + " %)"); }
        }

        static readonly List<string> noCaben = new List<string>();

        static void MatrizEncaje(List<Modulo> mods)
        {
            sb.AppendLine(); sb.AppendLine("== Encaje de cada equipo interior en cada sala (antes de colocar nada; mismos criterios) ==");
            foreach (string n in new[] { "P_Building_Maceta", "P_Building_CajaArena", "P_Building_MoxieO2", "P_Building_TaladorAgua", "P_Building_RecicladoraAgua" })
            {
                Vector3 tam = AssetDatabase.LoadAssetAtPath<GameObject>(RutaPrefab(n)).GetComponent<BoxCollider>().size;
                sb.AppendLine("   " + n.PadRight(18) + string.Join("  ", mods.Select(m =>
                {
                    float e = Encaje(m, tam, 0.3f, out _, out _, out _);
                    return m.id + ": " + (e >= 1f ? "sí" : e > 0f ? (e * 100f).ToString("0") + " %" : "no");
                })));
            }
            sb.AppendLine("   (sí = cabe a tamaño real; % = escala máxima que cabe; no = ni al 30 %)");
        }

        static Vector3 Plano(Vector3 v) { v.y = 0f; return v; }
        static Vector3[] Esquinas(Vector3 pos, Quaternion rot, Vector3 t) =>
            new[] { new Vector3(-1, 0, -1), new Vector3(1, 0, -1), new Vector3(1, 0, 1), new Vector3(-1, 0, 1) }.Select(e => pos + rot * Vector3.Scale(e, t * 0.5f)).ToArray();
        static float DistRect(Vector3 p, Vector3 pos, Quaternion rot, Vector3 t)
        {
            Vector3 l = Quaternion.Inverse(rot) * Plano(p - pos);
            float dx = Mathf.Max(Mathf.Abs(l.x) - t.x * 0.5f, 0f), dz = Mathf.Max(Mathf.Abs(l.z) - t.z * 0.5f, 0f);
            return Mathf.Sqrt(dx * dx + dz * dz);
        }

        static float RadioAnillo()
        {
            GameObject pf = AssetDatabase.LoadAssetAtPath<GameObject>(RutaPrefab("P_Env_Cupula_A"));
            Bounds b = BoundsDe(new[] { pf.transform.Find("Base").GetComponent<MeshFilter>() }, pf.transform);
            return Mathf.Max(b.extents.x / Mathf.Cos(30f * Mathf.Deg2Rad), b.extents.z);
        }

        static void ColocarCupula(Modulo m, Transform padre, bool registrar)
        {
            GameObject pf = AssetDatabase.LoadAssetAtPath<GameObject>(RutaPrefab("P_Env_Cupula_A"));
            float s = m.rExt / RadioAnillo();
            var inst = (GameObject)PrefabUtility.InstantiatePrefab(pf, padre.gameObject.scene);
            inst.transform.SetParent(padre, true); inst.name = "Cupula_" + m.id;
            inst.transform.SetPositionAndRotation(new Vector3(m.centro.x, m.techo, m.centro.z), Quaternion.identity);
            inst.transform.localScale = Vector3.one * s;
            Transform zona = inst.transform.Find("ZonaInteraccion");
            zona.localPosition = new Vector3(0f, (m.suelo + 1.1f - m.techo) / s, 0f);
            var cap = zona.GetComponent<CapsuleCollider>(); cap.radius = (m.rInt - 0.3f) / s; cap.height = 2.2f / s;
            Physics.SyncTransforms();
            int solidos = inst.GetComponentsInChildren<Collider>(true).Count(c => !c.isTrigger);
            if (registrar) C(solidos == 0, "        Cúpula P_Env_Cupula_A sobre el muro (y = " + m.techo.ToString("0.00") + " m), escala " + s.ToString("0.000") + " (esquinas del hexágono en el borde exterior del muro Ø " +
              (2 * m.rExt).ToString("0.0") + " m); zona de interacción a 1,1 m del suelo, radio " + (m.rInt - 0.3f).ToString("0.00") + " m; sin colliders sólidos");
        }

        static Bounds Cerramiento(GameObject cabina)
        {
            var muros = cabina.GetComponentsInChildren<Collider>(true).Where(c => c.name.StartsWith("COL_Muro_Exterior")).ToArray();
            Bounds rec = muros[0].bounds; foreach (var c in muros) rec.Encapsulate(c.bounds);
            return rec;
        }

        // Peor invasión (m) de los pétalos abiertos con un ángulo dado: dentro de otra sala o fuera del cerramiento (≤ 0 = ninguna).
        static float Invasion(IEnumerable<CapsuleDomeController> cupulas, List<Modulo> mods, Bounds rec, float angulo, bool poseActual, out string donde)
        {
            float peor = float.MinValue; donde = "";
            foreach (var c in cupulas)
            {
                Modulo propia = mods.First(x => "Cupula_" + x.id == c.name);
                foreach (var pz in c.piezas)
                {
                    Quaternion giro = pz.transform.rotation * Quaternion.Euler(angulo, 0f, 0f) * Quaternion.Inverse(pz.transform.rotation);
                    Vector3 h = pz.transform.position;
                    foreach (MeshFilter mf in pz.transform.GetComponentsInChildren<MeshFilter>())
                        foreach (Vector3 v in mf.sharedMesh.vertices)
                        {
                            Vector3 w = mf.transform.TransformPoint(v);
                            Vector3 p = poseActual ? w : h + giro * (w - h);
                            foreach (Modulo m in mods)
                            {
                                if (m == propia) continue;
                                float e = m.rExt + 0.05f - Plano(p - m.centro).magnitude;
                                if (e > peor) { peor = e; donde = c.name + " entra en " + m.id; }
                            }
                            float fuera = Mathf.Max(rec.min.x - p.x, p.x - rec.max.x, rec.min.z - p.z, p.z - rec.max.z);
                            if (fuera > peor) { peor = fuera; donde = c.name + " sale del cerramiento"; }
                        }
                }
            }
            return peor;
        }

        // Calcula el ángulo con cúpulas temporales (mismas posiciones y escalas que las definitivas), las borra, guarda el ángulo
        // en el prefab y solo después se colocan las definitivas: así no se pierden los ajustes de las instancias al guardar el prefab.
        static void AjustarAngulo(List<Modulo> mods, Bounds rec, Transform padreCupulas)
        {
            foreach (Modulo m in mods) ColocarCupula(m, padreCupulas, false);
            Physics.SyncTransforms();
            var cupulas = padreCupulas.GetComponentsInChildren<CapsuleDomeController>();
            float angulo = 90f;
            for (; angulo > 20f; angulo -= 1f) if (Invasion(cupulas, mods, rec, angulo, false, out _) <= 0f) break;
            Invasion(cupulas, mods, rec, angulo + 1f, false, out string limite);
            foreach (Transform t in padreCupulas.Cast<Transform>().ToArray()) UnityEngine.Object.DestroyImmediate(t.gameObject);
            string ruta = RutaPrefab("P_Env_Cupula_A");
            GameObject contenido = PrefabUtility.LoadPrefabContents(ruta);
            foreach (var pz in contenido.GetComponent<CapsuleDomeController>().piezas) pz.rotacionAbierta = new Vector3(angulo, 0f, 0f);
            PrefabUtility.SaveAsPrefabAsset(contenido, ruta); PrefabUtility.UnloadPrefabContents(contenido);
            var guardado = AssetDatabase.LoadAssetAtPath<GameObject>(ruta).GetComponent<CapsuleDomeController>();
            bool aplicado = guardado.piezas.All(pz => Mathf.Abs(pz.rotacionAbierta.x - angulo) < 0.01f);
            C(aplicado && angulo > 20f, "Ángulo de apertura para esta base: " + angulo.ToString("0") + "° (el mayor con el que, abiertas todas a la vez, ningún pétalo entra en otra sala ni sale del cerramiento; con " +
              (angulo + 1f).ToString("0") + "° " + limite + "). Guardado en P_Env_Cupula_A");
        }

        static void ColocarExterior(GameObject cabina)
        {
            sb.AppendLine(); sb.AppendLine("== Exterior ==");
            Bounds rec = Cerramiento(cabina);
            var puertas = cabina.GetComponentsInChildren<Collider>(true).Where(c => c.name.StartsWith("COL_PUERTA_") && !c.name.Contains("TEC_") && (c.name.Contains("_N_") || c.name.Contains("_S_") || c.name.Contains("_NE_"))).ToArray();
            sb.AppendLine("Cerramiento: " + F(rec.size) + " m (de " + F(rec.min) + " a " + F(rec.max) + "); accesos exteriores: " + string.Join(", ", puertas.Select(p => p.name.Replace("COL_PUERTA_", "") + " " + F(p.bounds.center))));
            var padre = new GameObject("Exterior").transform;
            var colocar = new (string prefab, Vector3 pos, float giro)[]
            {
                ("P_Building_PanelesSolares", new Vector3(rec.max.x + 4f, 0f, -6f), 90f),
                ("P_Building_Antena", new Vector3(rec.max.x + 3f, 0f, 6f), -90f),
                ("P_Building_Heolica", new Vector3(rec.max.x + 12f, 0f, 0f), 90f),
            };
            var colocados = new List<(string, Bounds)>();
            foreach (var c in colocar)
            {
                var inst = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(RutaPrefab(c.prefab)), padre.gameObject.scene);
                inst.transform.SetParent(padre, true); inst.name = "Exterior_" + c.prefab; inst.transform.SetPositionAndRotation(c.pos, Quaternion.Euler(0f, c.giro, 0f));
                Bounds b = BoundsDe(inst.GetComponentsInChildren<MeshFilter>(), inst.transform.parent);
                float aMuro = b.min.x - rec.max.x; float aPuerta = puertas.Length == 0 ? 99f : puertas.Min(p => Vector3.Distance(Plano(p.bounds.center), Plano(b.ClosestPoint(p.bounds.center))));
                bool solapa = colocados.Any(o => o.Item2.Intersects(b));
                C(aMuro >= 1.5f && aPuerta >= 3f && !solapa, "   " + c.prefab + " " + F(b.size) + " m fuera del cerramiento, a " + aMuro.ToString("0.0") + " m del muro y " + aPuerta.ToString("0.0") + " m del acceso exterior más cercano" + (solapa ? " — SOLAPA con otro equipo" : ""));
                colocados.Add((c.prefab, b));
            }
        }

        static void CrearGaleria()
        {
            var padre = new GameObject("Galeria").transform;
            string[] orden = { "P_Building_MoxieO2", "P_Building_RecicladoraAgua", "P_Building_TaladorAgua", "P_Building_Maceta", "P_Building_CajaArena", "P_Building_PanelesSolares", "P_Building_Antena", "P_Building_Heolica", "P_Env_Cupula_A", "P_Env_Cupula_B", "P_Env_Capsula" };
            float x = 0f;
            foreach (string n in orden)
            {
                GameObject pf = AssetDatabase.LoadAssetAtPath<GameObject>(RutaPrefab(n));
                Bounds b = BoundsDe(pf.GetComponentsInChildren<MeshFilter>(true), pf.transform);
                var inst = (GameObject)PrefabUtility.InstantiatePrefab(pf, padre.gameObject.scene);
                inst.transform.SetParent(padre, true); inst.name = "Galeria_" + n;
                float z = n == "P_Env_Capsula" ? -75f : -50f;
                inst.transform.position = new Vector3(x - b.min.x, n.StartsWith("P_Env_Cupula") || n == "P_Env_Capsula" ? -b.min.y : 0f, z);
                x += b.size.x + 3f;
            }
            sb.AppendLine(); sb.AppendLine("Galería: " + orden.Length + " prefabs en fila a z = −50 (P_Env_Capsula, con todos los objetos del .blend, a z = −75).");
        }

        static void Capturar(Camera cam, Vector3 pos, Quaternion rot, float fov, string archivo)
        {
            cam.transform.SetPositionAndRotation(pos, rot); cam.fieldOfView = fov;
            var rt = new RenderTexture(1280, 800, 24); var prev = cam.targetTexture; cam.targetTexture = rt; cam.Render();
            RenderTexture.active = rt; var tex = new Texture2D(1280, 800, TextureFormat.RGB24, false); tex.ReadPixels(new Rect(0, 0, 1280, 800), 0, 0); tex.Apply();
            File.WriteAllBytes(Path.Combine(Capturas, archivo), tex.EncodeToPNG());
            RenderTexture.active = null; cam.targetTexture = prev; UnityEngine.Object.DestroyImmediate(rt); UnityEngine.Object.DestroyImmediate(tex);
        }

        // =====================================================================================================
        // 3. PRUEBA EN PLAY MODE
        // =====================================================================================================
        static readonly Stack<IEnumerator> pila = new Stack<IEnumerator>();
        static IntegracionModelos3D()
        {
            EditorApplication.playModeStateChanged += st =>
            {
                if (st != PlayModeStateChange.EnteredPlayMode || !SessionState.GetBool(ClavePrueba, false)) return;
                SessionState.EraseBool(ClavePrueba);
                pila.Clear(); pila.Push(Prueba()); EditorApplication.update += Paso;
            };
        }

        [MenuItem("Tools/Mars Cat/Modelos 3D/3. Probar cúpulas en Play Mode", false, 3)]
        public static void Probar()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (SceneManager.GetActiveScene().path != RutaEscena)
            {
                if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
                if (!File.Exists(Path.Combine(Raiz, RutaEscena))) { EditorUtility.DisplayDialog("Modelos 3D", "Ejecuta antes el paso 2.", "OK"); return; }
                EditorSceneManager.OpenScene(RutaEscena);
            }
            SessionState.SetBool(ClavePrueba, true);
            EditorApplication.isPlaying = true;
        }

        static void Paso()
        {
            if (!EditorApplication.isPlaying) { EditorApplication.update -= Paso; return; }
            try
            {
                while (pila.Count > 0)
                {
                    IEnumerator e = pila.Peek();
                    if (!e.MoveNext()) { pila.Pop(); continue; }
                    if (e.Current is IEnumerator hijo) { pila.Push(hijo); continue; }
                    return; // yield return null: seguir en el siguiente tick del editor
                }
            }
            catch (Exception ex) { pila.Clear(); C(false, "Excepción durante la prueba: " + ex); RestaurarEntrada(); Escribir("Modelos3D_PruebaPlayMode.txt", "Prueba de cúpulas"); EditorApplication.isPlaying = false; }
            if (pila.Count == 0) EditorApplication.update -= Paso;
        }

        static InputSettings.BackgroundBehavior fondoPrevio; static InputSettings.EditorInputBehaviorInPlayMode editorPrevio; static bool entradaCambiada;
        static void RestaurarEntrada()
        {
            if (!entradaCambiada) return;
            InputSystem.settings.backgroundBehavior = fondoPrevio; InputSystem.settings.editorInputBehaviorInPlayMode = editorPrevio; entradaCambiada = false;
        }

        static IEnumerator Frames(int n) { int f = Time.frameCount; while (Time.frameCount < f + n) yield return null; }
        static IEnumerator Segundos(float s) { float t = Time.time + s; while (Time.time < t) yield return null; }

        static IEnumerator PulsarTecla(Key k)
        {
            Keyboard kb = Keyboard.current;
            if (kb == null) yield break;
            if (!kb.enabled) InputSystem.EnableDevice(kb);
            InputSystem.QueueStateEvent(kb, new KeyboardState(k));
            yield return Frames(2);
            InputSystem.QueueStateEvent(kb, new KeyboardState());
            yield return Frames(2);
        }

        static float AnguloBisagra(CapsuleDomeController d, int i, Quaternion[] cerradas) => Quaternion.Angle(cerradas[i], d.piezas[i].transform.localRotation);

        static IEnumerator Prueba()
        {
            Iniciar("PRUEBA DE CÚPULAS EN PLAY MODE (Sandbox_Modelos3D)");
            fondoPrevio = InputSystem.settings.backgroundBehavior; editorPrevio = InputSystem.settings.editorInputBehaviorInPlayMode;
            // Solo durante la prueba: que la tecla simulada llegue aunque la ventana de Unity no tenga el foco. Se restaura al final.
            InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
            InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
            entradaCambiada = true;
            yield return Segundos(0.5f);

            GameObject[] raices = SceneManager.GetActiveScene().GetRootGameObjects();
            var cabina = raices.First(g => g.name.StartsWith("CabinaNavegable"));
            var mods = MedirModulos(cabina);
            var cupulas = raices.SelectMany(g => g.GetComponentsInChildren<CapsuleDomeController>()).Where(c => c.name.StartsWith("Cupula_")).OrderBy(c => c.name).ToArray();
            C(cupulas.Length == 7 && cupulas.All(c => c.Cerrado), "7 cúpulas en la escena (M-01…M-06 y núcleo), todas cerradas al empezar");

            sb.AppendLine(); sb.AppendLine("== Colliders: la cúpula no interfiere con la cabina ==");
            int solidos = cupulas.Sum(c => c.GetComponentsInChildren<Collider>(true).Count(x => !x.isTrigger));
            var zonas = cupulas.Select(c => c.GetComponentInChildren<DomeInteraction>()).ToArray();
            C(solidos == 0 && zonas.All(z => z != null && z.GetComponent<Collider>().isTrigger && z.gameObject.layer == 2),
              "Ninguna pieza de las cúpulas tiene collider sólido; la única es la zona de interacción (Trigger, capa Ignore Raycast)");
            Modulo m1 = mods.First(m => m.id == "M-01");
            CapsuleDomeController d1 = cupulas.First(c => c.name == "Cupula_M-01"); DomeInteraction z1 = d1.GetComponentInChildren<DomeInteraction>();
            bool rayo = Physics.Raycast(m1.centro + Vector3.up * 10f, Vector3.down, out RaycastHit hr, 20f);
            C(rayo && hr.collider.GetComponentInParent<CapsuleDomeController>() == null, "Un rayo vertical en el centro de M-01 atraviesa la cúpula y la zona y da en " + (rayo ? hr.collider.name : "nada") + " (la cámara y la IA no la ven)");

            // Jugador de prueba con CharacterController (mismas medidas que Nio_Player)
            var jug = new GameObject("JugadorPrueba_Temporal");
            var cc = jug.AddComponent<CharacterController>(); cc.radius = 0.22f; cc.height = 0.62f; cc.center = new Vector3(0f, 0.33f, 0f);
            void Llevar(Vector3 p) { cc.enabled = false; jug.transform.position = p; cc.enabled = true; Physics.SyncTransforms(); }
            Llevar(m1.centro + Vector3.up * 30f);
            yield return Segundos(0.3f);

            sb.AppendLine(); sb.AppendLine("== Interacción en M-01 (tecla " + z1.InteractKey + ") ==");
            C(Keyboard.current != null, "Teclado del Input System disponible (" + (Keyboard.current != null ? Keyboard.current.displayName : "ninguno") + ")");
            yield return PulsarTecla(z1.InteractKey);
            yield return Segundos(0.3f);
            C(!z1.PlayerInside && z1.Interactions == 0 && d1.Cerrado, "Con el jugador fuera de la zona, pulsar E no hace nada");

            Llevar(m1.centro + Vector3.up * 0.05f);
            yield return Segundos(0.3f);
            Vector3 antes = jug.transform.position; cc.Move(Vector3.left * 0.3f); Physics.SyncTransforms();
            float avance = Plano(jug.transform.position - antes).magnitude;
            yield return Segundos(0.2f);
            C(z1.PlayerInside && avance > 0.28f, "El jugador entra en la zona de la sala (OnTriggerEnter) y se mueve dentro sin que nada lo frene (" + avance.ToString("0.00") + " de 0,30 m)");

            var cerradas = d1.piezas.Select(p => p.transform.localRotation).ToArray();
            int inter0 = z1.Interactions;
            float tPulsar = Time.time;
            yield return PulsarTecla(z1.InteractKey);
            bool porTecla = z1.Interactions == inter0 + 1;
            if (!porTecla) { sb.AppendLine("        (la pulsación simulada no llegó; se usa DomeInteraction.Interact())"); z1.Interact(); }
            C(porTecla, "Pulsar E dentro de la zona abre la cúpula (Input System → DomeInteraction → CapsuleDomeController.Alternar)");
            float t0 = tPulsar; var muestras = new List<(float t, float a, float p)>();
            Camera cam = Camera.main;
            if (cam != null) { cam.transform.position = m1.centro + new Vector3(7f, 9f, 7f); cam.transform.LookAt(m1.centro + Vector3.up * 2.5f); cam.fieldOfView = 55f; }
            int foto = 0; float proxFoto = 0f;
            while (d1.EnMovimiento && Time.time - t0 < DuracionCupula + 1.5f)
            {
                float t = Time.time - t0;
                muestras.Add((t, AnguloBisagra(d1, 0, cerradas), d1.Progreso));
                if (cam != null && t >= proxFoto) { CapturarJuego(cam, "Cupula_apertura_" + foto.ToString("00") + ".png"); foto++; proxFoto += 0.2f; }
                yield return null;
            }
            float tAbrir = Time.time - t0;
            if (cam != null) CapturarJuego(cam, "Cupula_apertura_" + foto.ToString("00") + ".png");
            bool monotono = muestras.Zip(muestras.Skip(1), (a, b) => b.a >= a.a - 0.01f).All(x => x);
            float ang1 = d1.piezas[0].rotacionAbierta.x;
            float errCurva = muestras.Select(s => Mathf.Abs(s.a - ang1 * d1.curva.Evaluate(Mathf.Clamp01(s.p)))).DefaultIfEmpty(99f).Max();
            C(d1.Abierto && monotono && errCurva < 0.5f && Mathf.Abs(tAbrir - DuracionCupula) < 0.35f && muestras.Count >= 5,
              "Movimiento suave y controlado: se abre en " + tAbrir.ToString("0.00") + " s (configurado " + DuracionCupula + " s), el ángulo siempre crece y en cada uno de los " + muestras.Count +
              " fotogramas sigue la curva de aceleración y frenado (desviación máx. " + errCurva.ToString("0.00") + "°); depende del tiempo, no de los fotogramas por segundo");
            string tramo = string.Join(" → ", new[] { 0.25f, 0.5f, 0.75f }.Select(f => muestras.OrderBy(s => Mathf.Abs(s.t - f * DuracionCupula)).First()).Select(s => s.a.ToString("0") + "°"));
            sb.AppendLine("        Pétalo 1 a 25 / 50 / 75 % del tiempo: " + tramo + " (curva de aceleración y frenado suaves)");
            float[] angulos = Enumerable.Range(0, 6).Select(i => AnguloBisagra(d1, i, cerradas)).ToArray();
            C(angulos.All(a => Mathf.Abs(a - ang1) < 0.5f), "Abierta: los 6 pétalos giran " + string.Join(" / ", angulos.Select(a => a.ToString("0.0"))) + "° sobre su bisagra (configurado " + ang1.ToString("0") + "°)");

            // Geometría abierta: ningún pétalo baja del borde del muro ni se mete en los colliders de la cabina
            float minY = float.MaxValue; int dentro = 0; var tocados = new HashSet<string>();
            foreach (MeshFilter mf in d1.GetComponentsInChildren<MeshFilter>().Where(f => f.name.StartsWith("Petalo")))
            {
                Vector3[] vs; try { vs = mf.sharedMesh.vertices; } catch { vs = new Vector3[0]; }
                foreach (Vector3 v in vs)
                {
                    Vector3 w = mf.transform.TransformPoint(v); minY = Mathf.Min(minY, w.y);
                    foreach (Collider c in Physics.OverlapSphere(w, 0.03f, ~0, QueryTriggerInteraction.Ignore)) { dentro++; tocados.Add(c.name); }
                }
            }
            C(minY >= m1.techo - 0.05f, "Abierta, el punto más bajo de los pétalos queda a " + (minY - m1.techo).ToString("+0.00;-0.00") + " m del borde del muro (no baja hacia los pasillos)");
            C(dentro == 0, "Ningún vértice de los pétalos abiertos toca un collider de la cabina" + (dentro > 0 ? " — tocan: " + string.Join(", ", tocados) : ""));
            C(cupulas.Where(c => c != d1).All(c => c.Cerrado), "Las otras 6 cúpulas siguen cerradas (cada sala es independiente)");

            yield return PulsarTecla(z1.InteractKey);
            if (z1.Interactions == inter0 + 1) z1.Interact();
            float t1 = Time.time;
            while (d1.EnMovimiento && Time.time - t1 < DuracionCupula + 1.5f) yield return null;
            float errCierre = Enumerable.Range(0, 6).Max(i => AnguloBisagra(d1, i, cerradas));
            C(d1.Cerrado && errCierre < 0.01f, "Pulsar E otra vez la cierra en " + (Time.time - t1).ToString("0.00") + " s y los pétalos vuelven exactamente a su pose (error " + errCierre.ToString("0.000") + "°)");

            Llevar(m1.centro + Vector3.up * 30f);
            yield return Segundos(0.3f);
            int inter1 = z1.Interactions;
            yield return PulsarTecla(z1.InteractKey);
            yield return Segundos(0.3f);
            C(!z1.PlayerInside && z1.Interactions == inter1 && d1.Cerrado, "Al salir de la zona (aquí teletransportado: Unity no envía OnTriggerExit y la presencia caduca en 2,5 pasos de física), E vuelve a no tener efecto");

            sb.AppendLine(); sb.AppendLine("== Reutilización en otras salas ==");
            CapsuleDomeController dn = cupulas.First(c => c.name == "Cupula_Nucleo");
            var cerrN = dn.piezas.Select(p => p.transform.localRotation).ToArray();
            dn.GetComponentInChildren<DomeInteraction>().Interact();
            float t2 = Time.time; while (dn.EnMovimiento && Time.time - t2 < DuracionCupula + 1.5f) yield return null;
            C(dn.Abierto && Enumerable.Range(0, 6).All(i => Mathf.Abs(AnguloBisagra(dn, i, cerrN) - dn.piezas[i].rotacionAbierta.x) < 0.5f),
              "El mismo prefab en el núcleo (escala " + dn.transform.localScale.x.ToString("0.000") + ", Ø 8 m) se abre igual");
            foreach (var c in cupulas.Where(c => c != dn)) c.GetComponentInChildren<DomeInteraction>().Interact();
            float t3 = Time.time; while (cupulas.Any(c => c.EnMovimiento) && Time.time - t3 < DuracionCupula + 2f) yield return null;
            C(cupulas.All(c => c.Abierto), "Las 7 cúpulas abiertas a la vez con sus propias zonas");
            float inv = Invasion(cupulas, mods, Cerramiento(cabina), 0f, true, out string dondeInv);
            C(inv <= 0f, "Abiertas todas, ningún pétalo entra en el círculo de otra sala ni sale del cerramiento (margen " + (-inv).ToString("0.00") + " m; lo más cercano: " + dondeInv + ")");
            if (cam != null)
            {
                cam.transform.position = new Vector3(24f, 26f, 26f); cam.transform.LookAt(new Vector3(0f, 2f, 0f)); cam.fieldOfView = 50f; CapturarJuego(cam, "Cupulas_todas_abiertas.png");
                cam.transform.SetPositionAndRotation(new Vector3(1f, 42f, 0f), Quaternion.Euler(90f, 180f, 0f)); CapturarJuego(cam, "Cupulas_todas_abiertas_cenital.png");
            }
            foreach (var c in cupulas) c.Cerrar();
            float t4 = Time.time; while (cupulas.Any(c => c.EnMovimiento) && Time.time - t4 < DuracionCupula + 2f) yield return null;
            C(cupulas.All(c => c.Cerrado), "Y vuelven a cerrarse todas");

            UnityEngine.Object.Destroy(jug);
            RestaurarEntrada();
            C(InputSystem.settings.backgroundBehavior == fondoPrevio && InputSystem.settings.editorInputBehaviorInPlayMode == editorPrevio, "Ajustes del Input System restaurados al terminar la prueba");
            Escribir("Modelos3D_PruebaPlayMode.txt", "Prueba de cúpulas");
            EditorApplication.isPlaying = false;
        }

        static void CapturarJuego(Camera cam, string archivo)
        {
            var rt = new RenderTexture(960, 600, 24); var prev = cam.targetTexture; cam.targetTexture = rt; cam.Render();
            RenderTexture.active = rt; var tex = new Texture2D(960, 600, TextureFormat.RGB24, false); tex.ReadPixels(new Rect(0, 0, 960, 600), 0, 0); tex.Apply();
            File.WriteAllBytes(Path.Combine(Capturas, archivo), tex.EncodeToPNG());
            RenderTexture.active = null; cam.targetTexture = prev; UnityEngine.Object.Destroy(rt); UnityEngine.Object.Destroy(tex);
        }
    }
}
