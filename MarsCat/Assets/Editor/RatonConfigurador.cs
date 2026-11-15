using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEditor.U2D.Sprites;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

/// <summary>
/// Tools > Ratón > 1. Configurar ratón enemigo (zona técnica)   → Logs/Raton_Configuracion.txt (copia previa en Backups/)
/// Tools > Ratón > 2. Probar ratón en Play Mode                   → Logs/Raton_PruebaPlayMode.txt
/// </summary>
[InitializeOnLoad]
public static class RatonConfigurador
{
    const string CarpetaRatas = "Assets/Sprites/rats";
    const string CarpetaClips = CarpetaRatas + "/Animations";
    const string CarpetaCtrl = CarpetaRatas + "/Controllers";
    const string RutaControlador = CarpetaCtrl + "/Raton.controller";
    const string RutaNavMesh = CarpetaRatas + "/NavMesh_ZonaTecnica.asset";
    const string RutaSombra = "Assets/Sprites/Nio/Sombra_Nio.png";
    const string NombreRaton = "Raton_Enemigo";
    const string NombreZona = "Zona_Raton_Tecnica";
    const string NombreNavMesh = "NavMesh_ZonaTecnica";
    const string RutaAgentes = "ProjectSettings/NavMeshAreas.asset";
    const string NombreAgente = "Raton";
    const float AgenteRadio = 0.2f, AgenteAltura = 0.45f, AgenteEscalon = 0.12f, AgentePendiente = 40f;
    const string NombrePlayer = "Nio_Player";
    const string NombreCabina = "CabinaPrincipal";
    const string ClavePrueba = "RatonConfigurador.PruebaPendiente";
    const float EscalaRaton = 0.45f;

    struct Paso { public int s, n; public Paso(int s, int n) { this.s = s; this.n = n; } }
    class DefClip { public string nombre, estado; public float fps; public bool bucle; public int min, max; public Paso[] pasos; }

    static readonly DefClip[] Clips =
    {
        new DefClip { nombre = "Raton_Idle", estado = "Idle", fps = 6f, bucle = true, min = 0, max = 3,
            pasos = new[] { new Paso(0, 2), new Paso(1, 1), new Paso(2, 2), new Paso(3, 1), new Paso(2, 1), new Paso(1, 1) } },
        new DefClip { nombre = "Raton_Walk", estado = "Walk", fps = 14f, bucle = true, min = 4, max = 11,
            pasos = new[] { new Paso(4, 1), new Paso(5, 1), new Paso(6, 1), new Paso(7, 1), new Paso(8, 1), new Paso(9, 1), new Paso(10, 1), new Paso(11, 1) } },
        new DefClip { nombre = "Raton_Attack", estado = "Attack", fps = 12f, bucle = false, min = 12, max = 17,
            pasos = new[] { new Paso(12, 1), new Paso(13, 1), new Paso(14, 1), new Paso(15, 1), new Paso(16, 1), new Paso(17, 1) } },
        new DefClip { nombre = "Raton_Damage", estado = "Damage", fps = 10f, bucle = false, min = 18, max = 21,
            pasos = new[] { new Paso(19, 1), new Paso(20, 1), new Paso(21, 1), new Paso(18, 1) } },
        new DefClip { nombre = "Raton_Death", estado = "Death", fps = 10f, bucle = false, min = 19, max = 20,
            pasos = new[] { new Paso(19, 2), new Paso(20, 4) } },
    };
    static readonly int[][] Filas = { new[] { 0, 1, 2, 3 }, new[] { 4, 5, 6, 7, 8, 9, 10, 11 }, new[] { 12, 13, 14, 15, 16, 17 }, new[] { 18, 19, 20, 21 } };

    static string Raiz => Directory.GetParent(Application.dataPath).FullName;
    static string RutaLog(string n) { string d = Path.Combine(Raiz, "Logs"); Directory.CreateDirectory(d); return Path.Combine(d, n); }

    static RatonConfigurador()
    {
        EditorApplication.playModeStateChanged -= AlCambiarModo;
        EditorApplication.playModeStateChanged += AlCambiarModo;
    }

    static int Indice(string nombre)
    {
        if (string.IsNullOrEmpty(nombre)) return -1;
        int i = nombre.LastIndexOf('_');
        return i >= 0 && int.TryParse(nombre.Substring(i + 1), out int v) ? v : -2;
    }

    static string RutaHoja()
    {
        string guid = AssetDatabase.FindAssets("t:Texture2D", new[] { CarpetaRatas }).FirstOrDefault(g => AssetDatabase.GUIDToAssetPath(g).EndsWith(".png"));
        if (guid == null) throw new Exception("No hay ninguna hoja de sprites en " + CarpetaRatas);
        return AssetDatabase.GUIDToAssetPath(guid);
    }

    static Dictionary<int, Sprite> Sprites(string hoja)
    {
        var d = new Dictionary<int, Sprite>();
        foreach (Sprite s in AssetDatabase.LoadAllAssetsAtPath(hoja).OfType<Sprite>()) { int i = Indice(s.name); if (i >= 0) d[i] = s; }
        return d;
    }

    static T Obtener<T>(GameObject go) where T : Component
    {
        T c = go.GetComponent<T>();
        return c != null ? c : Undo.AddComponent<T>(go);
    }

    static Transform Hijo(Transform padre, string nombre)
    {
        Transform t = padre.Find(nombre);
        if (t != null) return t;
        var go = new GameObject(nombre);
        Undo.RegisterCreatedObjectUndo(go, "Crear " + nombre);
        Undo.SetTransformParent(go.transform, padre, "Crear " + nombre);
        go.transform.localPosition = Vector3.zero;
        go.transform.localRotation = Quaternion.identity;
        go.transform.localScale = Vector3.one;
        return go.transform;
    }

    static GameObject Raiz_(string nombre) => SceneManager.GetActiveScene().GetRootGameObjects().FirstOrDefault(g => g.name == nombre);

    // ------------------------------------------------------------------ 1. CONFIGURAR
    [MenuItem("Tools/Ratón/1. Configurar ratón enemigo (zona técnica)", false, 1)]
    public static void Configurar()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) { EditorUtility.DisplayDialog("Ratón", "Sal de Play Mode antes de configurar.", "OK"); return; }
        Scene escena = SceneManager.GetActiveScene();
        if (!escena.path.EndsWith("CabinaPrincipal.unity")) { EditorUtility.DisplayDialog("Ratón", "Abre la escena CabinaPrincipal.", "OK"); return; }
        if (Raiz_(NombrePlayer) == null) { EditorUtility.DisplayDialog("Ratón", "No existe Nio_Player (Tools > Nio > 5).", "OK"); return; }
        string marca = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        if (!EditorUtility.DisplayDialog("Configurar ratón", "Copia en Backups/Raton_" + marca + ".\nSe añade el ratón en la zona técnica, su malla de navegación y la vida de Nio.\nNo se toca la cabina.", "Configurar", "Cancelar"))
            return;

        var log = new StringBuilder();
        log.AppendLine("CONFIGURACIÓN DEL RATÓN ENEMIGO — " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " | Unity " + Application.unityVersion);
        try
        {
            string hoja = RutaHoja();
            CopiaSeguridad(Path.Combine(Raiz, "Backups", "Raton_" + marca), new[] { escena.path, hoja + ".meta", RutaControlador, RutaNavMesh, RutaAgentes }, log);
            AjustarPivotes(hoja, log);
            Dictionary<int, Sprite> sp = Sprites(hoja);
            for (int i = 0; i < 22; i++) if (!sp.ContainsKey(i)) throw new Exception("Falta el sprite " + i + " en " + hoja);
            var clips = CrearClips(sp, log);
            AnimatorController ctrl = CrearControlador(clips, log);
            GameObject player = Raiz_(NombrePlayer);
            PrepararNio(player, log);
            DoorSecurityController zona = PrepararZona(player, log);
            NavMeshSurface surface = PrepararNavMesh(zona, log);
            PrepararRaton(zona, surface, ctrl, sp, player, log);
            EditorSceneManager.MarkSceneDirty(escena);
            if (!EditorSceneManager.SaveScene(escena)) throw new Exception("No se pudo guardar la escena.");
            log.AppendLine("Escena guardada: " + escena.path);
        }
        catch (Exception e)
        {
            log.AppendLine("ERROR: " + e);
            File.WriteAllText(RutaLog("Raton_Configuracion.txt"), log.ToString());
            Debug.LogError("[Ratón] Error en la configuración: " + e.Message);
            return;
        }
        Verificar(log);
    }

    static void CopiaSeguridad(string dir, string[] rutas, StringBuilder log)
    {
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

    static void AjustarPivotes(string hoja, StringBuilder log)
    {
        var importer = (TextureImporter)AssetImporter.GetAtPath(hoja);
        var f = new SpriteDataProviderFactories();
        f.Init();
        ISpriteEditorDataProvider dp = f.GetSpriteEditorDataProviderFromObject(importer);
        dp.InitSpriteEditorDataProvider();
        SpriteRect[] rects = dp.GetSpriteRects();
        var por = new Dictionary<int, SpriteRect>();
        foreach (SpriteRect r in rects) por[Indice(r.name)] = r;
        foreach (int[] fila in Filas)
        {
            if (fila.Any(i => !por.ContainsKey(i))) throw new Exception("La hoja del ratón no tiene los recortes esperados");
            float suelo = fila.Min(i => por[i].rect.y);
            foreach (int i in fila)
            {
                por[i].alignment = SpriteAlignment.Custom;
                por[i].pivot = new Vector2(0.5f, (suelo - por[i].rect.y) / por[i].rect.height);
            }
        }
        dp.SetSpriteRects(rects);
        dp.Apply();
        importer.SaveAndReimport();
        log.AppendLine("Hoja del ratón: pivotes en el centro horizontal y en la línea de suelo de cada fila (el PNG no se toca)");
    }

    static Dictionary<string, AnimationClip> CrearClips(Dictionary<int, Sprite> sp, StringBuilder log)
    {
        if (!AssetDatabase.IsValidFolder(CarpetaClips)) AssetDatabase.CreateFolder(CarpetaRatas, "Animations");
        var res = new Dictionary<string, AnimationClip>();
        EditorCurveBinding b = EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite");
        foreach (DefClip d in Clips)
        {
            string ruta = CarpetaClips + "/" + d.nombre + ".anim";
            var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(ruta);
            if (clip == null) { clip = new AnimationClip { name = d.nombre }; AssetDatabase.CreateAsset(clip, ruta); }
            var keys = new List<ObjectReferenceKeyframe>();
            int fr = 0;
            foreach (Paso p in d.pasos) { keys.Add(new ObjectReferenceKeyframe { time = fr / d.fps, value = sp[p.s] }); fr += p.n; }
            float fin = fr / d.fps;
            Paso ult = d.pasos[d.pasos.Length - 1];
            if (ult.n > 1) keys.Add(new ObjectReferenceKeyframe { time = (fr - 1) / d.fps, value = sp[ult.s] });
            clip.frameRate = d.fps;
            AnimationUtility.SetObjectReferenceCurve(clip, b, keys.ToArray());
            if (Mathf.Abs(clip.length - fin) > 0.5f / d.fps)
            {
                keys.Add(new ObjectReferenceKeyframe { time = fin, value = sp[ult.s] });
                AnimationUtility.SetObjectReferenceCurve(clip, b, keys.ToArray());
            }
            AnimationClipSettings st = AnimationUtility.GetAnimationClipSettings(clip);
            st.loopTime = d.bucle;
            AnimationUtility.SetAnimationClipSettings(clip, st);
            EditorUtility.SetDirty(clip);
            AssetDatabase.SaveAssetIfDirty(clip);
            res[d.estado] = clip;
            log.AppendLine("Clip " + d.nombre + ": " + d.fps + " fps, " + clip.length.ToString("0.###") + " s, " + (d.bucle ? "en bucle" : "una vez") + ", sprites " +
                           string.Join(" ", d.pasos.Select(p => p.s + (p.n > 1 ? "×" + p.n : ""))));
        }
        return res;
    }

    static AnimatorStateTransition Tr(AnimatorState a, AnimatorState b, bool exitTime = false)
    {
        AnimatorStateTransition t = a.AddTransition(b);
        t.hasExitTime = exitTime;
        t.exitTime = exitTime ? 1f : 0f;
        t.hasFixedDuration = true;
        t.duration = 0f;
        t.offset = 0f;
        t.canTransitionToSelf = false;
        return t;
    }

    static AnimatorController CrearControlador(Dictionary<string, AnimationClip> clips, StringBuilder log)
    {
        if (!AssetDatabase.IsValidFolder(CarpetaCtrl)) AssetDatabase.CreateFolder(CarpetaRatas, "Controllers");
        var ctrl = AssetDatabase.LoadAssetAtPath<AnimatorController>(RutaControlador);
        if (ctrl == null) ctrl = AnimatorController.CreateAnimatorControllerAtPath(RutaControlador);
        else
        {
            foreach (AnimatorControllerParameter p in ctrl.parameters.ToArray()) ctrl.RemoveParameter(p);
            AnimatorStateMachine v = ctrl.layers[0].stateMachine;
            foreach (ChildAnimatorState cs in v.states.ToArray()) v.RemoveState(cs.state);
            foreach (AnimatorStateTransition t in v.anyStateTransitions.ToArray()) v.RemoveAnyStateTransition(t);
        }
        ctrl.AddParameter("Speed", AnimatorControllerParameterType.Float);
        ctrl.AddParameter("Attack", AnimatorControllerParameterType.Trigger);
        ctrl.AddParameter("Hurt", AnimatorControllerParameterType.Trigger);
        ctrl.AddParameter("IsDead", AnimatorControllerParameterType.Bool);
        AnimatorStateMachine sm = ctrl.layers[0].stateMachine;
        AnimatorState idle = sm.AddState("Idle", new Vector3(300, 100, 0));
        AnimatorState walk = sm.AddState("Walk", new Vector3(560, 0, 0));
        AnimatorState attack = sm.AddState("Attack", new Vector3(560, 200, 0));
        AnimatorState damage = sm.AddState("Damage", new Vector3(300, 300, 0));
        AnimatorState death = sm.AddState("Death", new Vector3(60, 300, 0));
        idle.motion = clips["Idle"]; walk.motion = clips["Walk"]; attack.motion = clips["Attack"]; damage.motion = clips["Damage"]; death.motion = clips["Death"];
        sm.defaultState = idle;

        Tr(idle, walk).AddCondition(AnimatorConditionMode.Greater, 0.1f, "Speed");
        Tr(walk, idle).AddCondition(AnimatorConditionMode.Less, 0.1f, "Speed");
        Tr(idle, attack).AddCondition(AnimatorConditionMode.If, 0f, "Attack");
        Tr(walk, attack).AddCondition(AnimatorConditionMode.If, 0f, "Attack");
        Tr(attack, idle, true);
        Tr(damage, idle, true);
        AnimatorStateTransition aDamage = sm.AddAnyStateTransition(damage);
        aDamage.hasExitTime = false; aDamage.duration = 0f; aDamage.hasFixedDuration = true; aDamage.canTransitionToSelf = true;
        aDamage.AddCondition(AnimatorConditionMode.If, 0f, "Hurt");
        aDamage.AddCondition(AnimatorConditionMode.IfNot, 0f, "IsDead");
        AnimatorStateTransition aDeath = sm.AddAnyStateTransition(death);
        aDeath.hasExitTime = false; aDeath.duration = 0f; aDeath.hasFixedDuration = true; aDeath.canTransitionToSelf = false;
        aDeath.AddCondition(AnimatorConditionMode.If, 0f, "IsDead");
        EditorUtility.SetDirty(ctrl);
        AssetDatabase.SaveAssetIfDirty(ctrl);
        log.AppendLine("Raton.controller: Idle (inicial), Walk, Attack, Damage, Death; parámetros Speed (Float), Attack (Trigger), Hurt (Trigger), IsDead (Bool); " +
                       "Idle↔Walk por Speed 0,1; Idle/Walk→Attack con Attack; Attack→Idle y Damage→Idle al terminar el clip; Any State→Damage con Hurt (si no IsDead); Any State→Death con IsDead");
        return ctrl;
    }

    static void PrepararNio(GameObject player, StringBuilder log)
    {
        var salud = Obtener<Salud>(player);
        salud.vidaMaxima = 5;
        salud.invulnerabilidad = 1f;
        var resp = Obtener<NioRespuestaDanio>(player);
        resp.movimiento = player.GetComponent<NioMovement>();
        Transform visual = player.transform.Find("Visual");
        resp.sprite = visual != null ? visual.GetComponent<SpriteRenderer>() : null;
        EditorUtility.SetDirty(player);
        log.AppendLine("Nio_Player: + Salud (5 puntos, 1 s de invulnerabilidad tras cada golpe) y NioRespuestaDanio (parpadeo, empuje y reaparición). NioMovement no se modifica.");
    }

    static DoorSecurityController PrepararZona(GameObject player, StringBuilder log)
    {
        GameObject cabina = Raiz_(NombreCabina);
        Renderer suelo = cabina.GetComponentsInChildren<Renderer>(true).FirstOrDefault(r => r.name == "SUE_Suelo_Zona_Tecnica");
        if (suelo == null) throw new Exception("No se encontró SUE_Suelo_Zona_Tecnica");
        Bounds b = suelo.bounds;
        CabinaPuertaAutomatica[] puertas = cabina.GetComponentsInChildren<CabinaPuertaAutomatica>(true).Where(p => p.puertaId == "TEC_Norte" || p.puertaId == "TEC_Sur").ToArray();
        if (puertas.Length != 2) throw new Exception("No se encontraron las puertas TEC_Norte y TEC_Sur");

        GameObject z = Raiz_(NombreZona);
        if (z == null) { z = new GameObject(NombreZona); Undo.RegisterCreatedObjectUndo(z, "Crear zona"); }
        z.transform.SetPositionAndRotation(new Vector3(b.center.x, 0f, b.center.z), Quaternion.identity);
        var seg = Obtener<DoorSecurityController>(z);
        seg.jugador = player.transform;
        seg.puertas = puertas;
        seg.centroZona = new Vector3(b.center.x, 1.5f, b.center.z);
        seg.tamanoZona = new Vector3(b.size.x, 3f, b.size.z);
        EditorUtility.SetDirty(seg);
        log.AppendLine("Zona del ratón: " + NombreZona + " con DoorSecurityController; zona = suelo SUE_Suelo_Zona_Tecnica (centro " + seg.centroZona.ToString("0.00") +
                       ", " + b.size.x.ToString("0.00") + " × " + b.size.z.ToString("0.00") + " m); puertas " + string.Join(" y ", puertas.Select(p => p.puertaId)));
        return seg;
    }

    // Tipo de agente propio del ratón (radio 0,2 m, altura 0,45 m). Con el agente Humanoid por defecto (radio 0,5 m, altura 2 m)
    // el pasillo técnico queda casi sin malla: cualquier conducto o mueble por debajo de 2 m lo corta.
    static int AgenteRaton(StringBuilder log)
    {
        UnityEngine.Object ajustes = AssetDatabase.LoadAllAssetsAtPath(RutaAgentes).FirstOrDefault();
        if (ajustes == null) throw new Exception("No se encontró " + RutaAgentes);
        var so = new SerializedObject(ajustes);
        SerializedProperty lista = so.FindProperty("m_Settings"), nombres = so.FindProperty("m_SettingNames");
        if (lista == null || nombres == null) throw new Exception("Formato inesperado de " + RutaAgentes);
        int indice = -1;
        for (int i = 0; i < nombres.arraySize; i++) if (nombres.GetArrayElementAtIndex(i).stringValue == NombreAgente) indice = i;
        bool creado = false;
        if (indice < 0)
        {
            NavMeshBuildSettings nuevo = NavMesh.CreateSettings();
            so.Update();
            lista = so.FindProperty("m_Settings"); nombres = so.FindProperty("m_SettingNames");
            for (int i = 0; i < lista.arraySize; i++) if (lista.GetArrayElementAtIndex(i).FindPropertyRelative("agentTypeID").intValue == nuevo.agentTypeID) indice = i;
            if (indice < 0) throw new Exception("NavMesh.CreateSettings no añadió el tipo de agente");
            while (nombres.arraySize <= indice) nombres.InsertArrayElementAtIndex(nombres.arraySize);
            nombres.GetArrayElementAtIndex(indice).stringValue = NombreAgente;
            creado = true;
        }
        SerializedProperty a = lista.GetArrayElementAtIndex(indice);
        a.FindPropertyRelative("agentRadius").floatValue = AgenteRadio;
        a.FindPropertyRelative("agentHeight").floatValue = AgenteAltura;
        a.FindPropertyRelative("agentClimb").floatValue = AgenteEscalon;
        a.FindPropertyRelative("agentSlope").floatValue = AgentePendiente;
        so.ApplyModifiedPropertiesWithoutUndo();
        AssetDatabase.SaveAssetIfDirty(ajustes);
        int id = a.FindPropertyRelative("agentTypeID").intValue;
        NavMeshBuildSettings comprobado = NavMesh.GetSettingsByID(id);
        if (Mathf.Abs(comprobado.agentRadius - AgenteRadio) > 0.001f || Mathf.Abs(comprobado.agentHeight - AgenteAltura) > 0.001f)
            throw new Exception("El tipo de agente " + NombreAgente + " no tomó los valores (radio " + comprobado.agentRadius + ", altura " + comprobado.agentHeight + ")");
        log.AppendLine("Tipo de agente de navegación '" + NombreAgente + "' " + (creado ? "creado" : "actualizado") + " en " + RutaAgentes + " (id " + id + "): radio " + AgenteRadio +
                       " m, altura " + AgenteAltura + " m, escalón " + AgenteEscalon + " m, pendiente " + AgentePendiente + "°. Humanoid no se toca.");
        return id;
    }

    static NavMeshSurface PrepararNavMesh(DoorSecurityController zona, StringBuilder log)
    {
        int agente = AgenteRaton(log);
        GameObject go = Raiz_(NombreNavMesh);
        if (go == null) { go = new GameObject(NombreNavMesh); Undo.RegisterCreatedObjectUndo(go, "Crear NavMesh"); }
        go.transform.SetPositionAndRotation(new Vector3(zona.centroZona.x, 0f, zona.centroZona.z), Quaternion.identity);
        var s = Obtener<NavMeshSurface>(go);
        s.agentTypeID = agente;
        s.collectObjects = CollectObjects.Volume;
        s.center = new Vector3(0f, 1f, 0f);
        s.size = new Vector3(zona.tamanoZona.x, 3f, zona.tamanoZona.z + 0.6f); // en X no sobresale del suelo de la zona: nada de malla al otro lado de la pared
        s.useGeometry = NavMeshCollectGeometry.PhysicsColliders;
        s.layerMask = ~0;
        GameObject rataPrevia = Raiz_(NombreRaton);
        bool estabaActiva = rataPrevia != null && rataPrevia.activeSelf;
        if (rataPrevia != null) rataPrevia.SetActive(false); // que el propio ratón no se cuele en la malla
        Physics.SyncTransforms();
        s.RemoveData();
        s.navMeshData = null;
        // Misma recogida que NavMeshSurface (colliders físicos dentro del volumen), pero sin triggers (zonas de detección de las puertas)
        // ni el CharacterController de Nio: no son obstáculos reales.
        var fuentes = new List<NavMeshBuildSource>();
        Bounds mundo = new Bounds(go.transform.position + s.center, s.size);
        UnityEngine.AI.NavMeshBuilder.CollectSources(mundo, s.layerMask, s.useGeometry, s.defaultArea, new List<NavMeshBuildMarkup>(), fuentes);
        int triggers = fuentes.RemoveAll(f => f.component is Collider c && (c.isTrigger || c is CharacterController));
        NavMeshData datos = UnityEngine.AI.NavMeshBuilder.BuildNavMeshData(NavMesh.GetSettingsByID(agente), fuentes, new Bounds(s.center, s.size), go.transform.position, go.transform.rotation);
        if (rataPrevia != null) rataPrevia.SetActive(estabaActiva);
        if (datos == null) throw new Exception("No se pudo generar la malla de navegación");
        datos.name = NombreNavMesh;
        if (AssetDatabase.LoadAssetAtPath<NavMeshData>(RutaNavMesh) != null) AssetDatabase.DeleteAsset(RutaNavMesh); // asset generado por esta herramienta
        AssetDatabase.CreateAsset(datos, RutaNavMesh);
        s.navMeshData = datos;
        s.AddData();
        EditorUtility.SetDirty(s);
        NavMeshTriangulation tri = NavMesh.CalculateTriangulation();
        float area = 0f;
        for (int i = 0; i + 2 < tri.indices.Length; i += 3)
            area += Vector3.Cross(tri.vertices[tri.indices[i + 1]] - tri.vertices[tri.indices[i]], tri.vertices[tri.indices[i + 2]] - tri.vertices[tri.indices[i]]).magnitude * 0.5f;
        superficieMalla = area;
        log.AppendLine("Malla de navegación: " + NombreNavMesh + " (NavMeshSurface, agente " + NombreAgente + ", volumen de la zona técnica, " + fuentes.Count + " colliders físicos; " + triggers +
                       " triggers/CharacterController descartados) guardada en " + RutaNavMesh + "; superficie transitable " + area.ToString("0.0") + " m². Las puertas cerradas quedan como obstáculos.");
        return s;
    }

    static float superficieMalla;

    static void PrepararRaton(DoorSecurityController zona, NavMeshSurface surface, AnimatorController ctrl, Dictionary<int, Sprite> sp, GameObject player, StringBuilder log)
    {
        Vector3 c = zona.centroZona;
        float largo = zona.tamanoZona.z * 0.5f;
        Vector3 Muestra(Vector3 p)
        {
            if (!NavMesh.SamplePosition(new Vector3(p.x, 0f, p.z), out NavMeshHit h, 1.5f, NavMesh.AllAreas)) throw new Exception("No hay malla de navegación cerca de " + p);
            return h.position;
        }
        Vector3 inicio = Muestra(new Vector3(c.x, 0f, c.z + 5f));
        Vector3 pa = Muestra(new Vector3(c.x, 0f, c.z + largo - 4f));
        Vector3 pb = Muestra(new Vector3(c.x, 0f, c.z - largo + 4f));

        GameObject rata = Raiz_(NombreRaton);
        bool nueva = rata == null;
        if (nueva) { rata = new GameObject(NombreRaton); Undo.RegisterCreatedObjectUndo(rata, "Crear ratón"); }
        rata.transform.SetPositionAndRotation(inicio, Quaternion.identity);
        GameObjectUtility.SetStaticEditorFlags(rata, 0);

        var agente = Obtener<NavMeshAgent>(rata);
        agente.agentTypeID = surface.agentTypeID;
        agente.radius = 0.2f;
        agente.height = 0.4f;
        agente.baseOffset = 0f;
        agente.speed = 1.2f;
        agente.acceleration = 20f;
        agente.angularSpeed = 720f;
        agente.stoppingDistance = 0.1f;
        agente.autoBraking = true;
        agente.updateRotation = false;
        agente.obstacleAvoidanceType = ObstacleAvoidanceType.LowQualityObstacleAvoidance;

        var cuerpo = Obtener<CapsuleCollider>(rata);
        cuerpo.radius = 0.17f;
        cuerpo.height = 0.34f;
        cuerpo.center = new Vector3(0f, 0.17f, 0f);
        cuerpo.isTrigger = false;

        var salud = Obtener<Salud>(rata);
        salud.vidaMaxima = 3;
        salud.invulnerabilidad = 0.3f;

        Transform visual = Hijo(rata.transform, "Visual");
        visual.localScale = Vector3.one * EscalaRaton;
        var sr = Obtener<SpriteRenderer>(visual.gameObject);
        sr.sprite = sp[0];
        var srNio = player.transform.Find("Visual")?.GetComponent<SpriteRenderer>();
        if (srNio != null) sr.sharedMaterial = srNio.sharedMaterial;
        sr.sortingLayerName = "Default";
        sr.sortingOrder = 0;
        sr.spriteSortPoint = SpriteSortPoint.Pivot;
        sr.color = Color.white;
        var an = Obtener<Animator>(visual.gameObject);
        an.runtimeAnimatorController = ctrl;
        an.applyRootMotion = false;
        an.cullingMode = AnimatorCullingMode.AlwaysAnimate;

        Transform sombra = Hijo(rata.transform, "Sombra");
        var srS = Obtener<SpriteRenderer>(sombra.gameObject);
        srS.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(RutaSombra);
        srS.color = new Color(0f, 0f, 0f, 0.35f);
        if (srNio != null) srS.sharedMaterial = srNio.sharedMaterial;
        srS.sortingLayerName = "Default";
        srS.sortingOrder = -1;
        sombra.localPosition = new Vector3(0f, 0.011f, 0f);
        sombra.localRotation = Quaternion.Euler(90f, 0f, 0f);
        sombra.localScale = new Vector3(0.36f, 0.36f, 1f);

        var animacion = Obtener<RatEnemyAnimation>(visual.gameObject);
        animacion.agente = agente;
        animacion.sombra = sombra;

        var combate = Obtener<RatEnemyCombat>(rata);
        combate.animacion = animacion;
        combate.danio = 1;
        combate.distanciaAtaque = 0.75f;
        combate.alcanceImpacto = 0.95f;
        combate.cooldown = 1.2f;
        combate.retardoImpacto = 0.17f;
        combate.duracionAtaque = 0.5f;

        GameObject zonaGo = zona.gameObject;
        Transform pts = Hijo(zonaGo.transform, "PuntosPatrulla");
        Transform p1 = Hijo(pts, "Patrulla_1"); p1.position = pa;
        Transform p2 = Hijo(pts, "Patrulla_2"); p2.position = pb;

        var ia = Obtener<RatEnemyAI>(rata);
        ia.jugador = player.transform;
        ia.seguridad = zona;
        ia.combate = combate;
        ia.puntosPatrulla = new[] { p1, p2 };

        var ctrlRaton = Obtener<RatEnemyController>(rata);
        ctrlRaton.ia = ia;
        ctrlRaton.combate = combate;
        ctrlRaton.animacion = animacion;
        ctrlRaton.cuerpo = cuerpo;

        EditorUtility.SetDirty(rata);
        log.AppendLine((nueva ? "Creado " : "Actualizado ") + NombreRaton + " en " + inicio.ToString("0.00") + ": NavMeshAgent (radio 0,2 m), CapsuleCollider (Ø0,34 m), Salud (3 puntos), " +
                       "RatEnemyController, RatEnemyAI (detección 6 m, persecución 2,6 m/s, patrulla 1,2 m/s entre 2 puntos), RatEnemyCombat (1 de daño, 0,75 m, 1,2 s de espera); " +
                       "Visual (SpriteRenderer + Animator Raton.controller + RatEnemyAnimation, escala " + EscalaRaton + ") y Sombra");
    }

    // ------------------------------------------------------------------ VERIFICACIÓN
    static void Verificar(StringBuilder log)
    {
        int ok = 0, total = 0;
        void C(bool cond, string texto) { total++; if (cond) ok++; log.AppendLine((cond ? "[OK]    " : "[FALLO] ") + texto); }
        log.AppendLine();
        log.AppendLine("== VERIFICACIÓN EN EL EDITOR ==");
        C(!EditorUtility.scriptCompilationFailed, "Sin errores de compilación");
        GameObject rata = Raiz_(NombreRaton), zonaGo = Raiz_(NombreZona), player = Raiz_(NombrePlayer), cabina = Raiz_(NombreCabina);
        C(rata != null && zonaGo != null && player != null, "Raton_Enemigo, Zona_Raton_Tecnica y Nio_Player en la escena");
        if (rata == null || zonaGo == null || player == null) { Escribir(log, ok, total); return; }
        var ia = rata.GetComponent<RatEnemyAI>();
        var combate = rata.GetComponent<RatEnemyCombat>();
        var ctrlR = rata.GetComponent<RatEnemyController>();
        var anim = rata.GetComponentInChildren<RatEnemyAnimation>();
        var seg = zonaGo.GetComponent<DoorSecurityController>();
        C(ia != null && combate != null && ctrlR != null && anim != null && rata.GetComponent<Salud>() != null && rata.GetComponent<NavMeshAgent>() != null,
          "Ratón: Salud, NavMeshAgent, RatEnemyController, RatEnemyAI, RatEnemyCombat y RatEnemyAnimation");
        C(ia.jugador == player.transform && ia.seguridad == seg && ia.combate == combate && ia.puntosPatrulla.All(p => p != null) && ctrlR.ia == ia && ctrlR.combate == combate &&
          ctrlR.animacion == anim && combate.animacion == anim && anim.agente != null && anim.sombra != null,
          "Referencias del ratón asignadas (jugador, seguridad, combate, animación, puntos de patrulla)");
        C(rata.GetComponentsInChildren<Animator>(true).Length == 1 && player.GetComponentsInChildren<Animator>(true).Length == 1,
          "Un único Animator en el ratón y otro en Nio (controladores independientes)");
        C(cabina.GetComponentsInChildren<Animator>(true).Length == 0 && cabina.GetComponentsInChildren<RatEnemyAI>(true).Length == 0,
          "Nada añadido a los objetos de la cabina");
        C(player.GetComponent<Salud>() != null && player.GetComponent<NioRespuestaDanio>() != null && player.GetComponent<NioMovement>() != null,
          "Nio_Player: Salud y NioRespuestaDanio añadidos; NioMovement intacto");
        C(seg.puertas.Length == 2 && seg.puertas.All(p => p != null) && seg.jugador == player.transform && !seg.Contiene(player.transform.position),
          "DoorSecurityController con las puertas TEC_Norte y TEC_Sur; Nio empieza fuera de la zona");

        var ctrl = AssetDatabase.LoadAssetAtPath<AnimatorController>(RutaControlador);
        AnimatorStateMachine sm = ctrl.layers[0].stateMachine;
        string[] estados = sm.states.Select(s => s.state.name).OrderBy(n => n).ToArray();
        C(estados.SequenceEqual(new[] { "Attack", "Damage", "Death", "Idle", "Walk" }) && sm.defaultState.name == "Idle" && ctrl.parameters.Length == 4 &&
          sm.states.Sum(s => s.state.transitions.Length) == 6 && sm.anyStateTransitions.Length == 2,
          "Raton.controller: 5 estados (inicial Idle), 4 parámetros, 6 transiciones + 2 desde Any State");

        // Clips muestreados frame a frame sobre el propio ratón (AnimationMode restaura el sprite al terminar).
        var srR = anim.GetComponent<SpriteRenderer>();
        AnimationMode.StartAnimationMode();
        try
        {
            foreach (DefClip d in Clips)
            {
                var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(CarpetaClips + "/" + d.nombre + ".anim");
                var esperado = new List<int>();
                foreach (Paso p in d.pasos) for (int k = 0; k < p.n; k++) esperado.Add(p.s);
                var obtenido = new List<int>();
                for (int i = 0; i < esperado.Count; i++)
                {
                    AnimationMode.BeginSampling();
                    AnimationMode.SampleAnimationClip(anim.gameObject, clip, (i + 0.5f) / d.fps);
                    AnimationMode.EndSampling();
                    obtenido.Add(srR.sprite != null ? Indice(srR.sprite.name) : -1);
                }
                C(esperado.SequenceEqual(obtenido) && clip.isLooping == d.bucle && Mathf.Abs(clip.length - esperado.Count / d.fps) < 0.5f / d.fps,
                  "Clip " + d.nombre + ": cambia el sprite frame a frame = " + string.Join(" ", obtenido) + "; " + clip.length.ToString("0.##") + " s, " + (d.bucle ? "bucle" : "una vez"));
            }
        }
        finally { AnimationMode.StopAnimationMode(); }

        // Malla de navegación: el ratón queda confinado en la zona técnica.
        Physics.SyncTransforms();
        var agente = rata.GetComponent<NavMeshAgent>();
        bool enMalla = NavMesh.SamplePosition(rata.transform.position, out NavMeshHit hr, 0.3f, NavMesh.AllAreas);
        C(enMalla, "El ratón empieza sobre la malla de navegación");
        var estadoPuntos = new List<string>();
        bool patrullaOk = true;
        foreach (Transform p in ia.puntosPatrulla)
        {
            var path = new NavMeshPath();
            bool okP = NavMesh.CalculatePath(hr.position, p.position, NavMesh.AllAreas, path) && path.status == NavMeshPathStatus.PathComplete;
            patrullaOk &= okP;
            estadoPuntos.Add(p.name + " " + p.position.ToString("0.0") + " → " + path.status);
        }
        C(patrullaOk, "Los dos puntos de patrulla son alcanzables desde la posición inicial (" + string.Join("; ", estadoPuntos) + ")");
        var rutaLarga = new NavMeshPath();
        Vector3 extremoA = new Vector3(seg.centroZona.x, 0f, seg.centroZona.z - seg.tamanoZona.z * 0.5f + 1f), extremoB = new Vector3(seg.centroZona.x, 0f, seg.centroZona.z + seg.tamanoZona.z * 0.5f - 1f);
        bool ea = NavMesh.SamplePosition(extremoA, out NavMeshHit ha, 1f, NavMesh.AllAreas), eb = NavMesh.SamplePosition(extremoB, out NavMeshHit hb, 1f, NavMesh.AllAreas);
        bool pasillo = ea && eb && NavMesh.CalculatePath(ha.position, hb.position, NavMesh.AllAreas, rutaLarga) && rutaLarga.status == NavMeshPathStatus.PathComplete;
        C(pasillo && superficieMalla > seg.tamanoZona.x * seg.tamanoZona.z * 0.3f, "La malla recorre el pasillo técnico de punta a punta (" + superficieMalla.ToString("0.0") + " m² transitables)");
        int puertasSellos = 0;
        foreach (CabinaPuertaAutomatica p in seg.puertas)
        {
            Bounds b = p.colliderBloqueo != null ? p.colliderBloqueo.bounds : new Bounds(p.transform.position, Vector3.one);
            Vector3 normal = b.size.x < b.size.z ? Vector3.right : Vector3.forward;
            Vector3 fuera = Mathf.Sign(Vector3.Dot(b.center - seg.centroZona, normal)) * normal;
            Vector3 exterior = new Vector3(b.center.x, 0f, b.center.z) + fuera * 0.8f;
            bool hayMallaEnPuerta = NavMesh.SamplePosition(new Vector3(b.center.x, 0f, b.center.z), out _, 0.2f, NavMesh.AllAreas);
            bool hayMallaFuera = NavMesh.SamplePosition(exterior, out _, 0.4f, NavMesh.AllAreas);
            var path = new NavMeshPath();
            bool camino = NavMesh.CalculatePath(hr.position, exterior, NavMesh.AllAreas, path) && path.status == NavMeshPathStatus.PathComplete;
            if (!hayMallaEnPuerta && !hayMallaFuera && !camino) puertasSellos++;
            log.AppendLine("        Puerta " + p.puertaId + ": malla en el hueco " + (hayMallaEnPuerta ? "sí" : "no") + ", al otro lado " + (hayMallaFuera ? "sí" : "no") + ", ruta hasta fuera " + (camino ? "sí" : "no"));
        }
        C(puertasSellos == seg.puertas.Length, "El ratón no tiene ruta para atravesar ninguna de las dos puertas (queda confinado en la zona técnica)");
        Escribir(log, ok, total);
        Selection.activeGameObject = rata;
    }

    static void Escribir(StringBuilder log, int ok, int total)
    {
        log.AppendLine();
        log.AppendLine("RESULTADO: " + ok + "/" + total + " comprobaciones superadas");
        File.WriteAllText(RutaLog("Raton_Configuracion.txt"), log.ToString());
        if (ok == total) Debug.Log("[Ratón] Configuración terminada: " + ok + "/" + total + " comprobaciones OK. Informe: Logs/Raton_Configuracion.txt");
        else Debug.LogWarning("[Ratón] Configuración terminada: " + ok + "/" + total + " comprobaciones OK. Revisa Logs/Raton_Configuracion.txt");
    }

    // ------------------------------------------------------------------ 2. PRUEBA
    [MenuItem("Tools/Ratón/2. Probar ratón en Play Mode", false, 2)]
    public static void Probar()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        if (Raiz_(NombreRaton) == null) { EditorUtility.DisplayDialog("Ratón", "Ejecuta antes Tools > Ratón > 1.", "OK"); return; }
        if (SceneManager.GetActiveScene().isDirty && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        SessionState.SetBool(ClavePrueba, true);
        EditorApplication.isPlaying = true;
    }

    static void AlCambiarModo(PlayModeStateChange cambio)
    {
        if (cambio != PlayModeStateChange.EnteredPlayMode || !SessionState.GetBool(ClavePrueba, false)) return;
        SessionState.SetBool(ClavePrueba, false);
        var go = new GameObject("RatonPrueba_Temporal");
        go.AddComponent<RatonPrueba>();
    }
}
