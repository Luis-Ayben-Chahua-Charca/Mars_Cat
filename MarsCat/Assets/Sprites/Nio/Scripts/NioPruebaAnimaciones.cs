// Asistencia de IA: escrito con Claude Code (Anthropic).
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Prueba automática en Play Mode (menú Tools > Nio > 3). Recorre Idle, Walk, Work y Back,
/// registra el estado del Animator y los sprites que muestra Nio en cada frame, guarda
/// capturas y un informe en Logs/ y sale de Play Mode. Se añade solo durante la prueba.
/// </summary>
[DisallowMultipleComponent]
public class NioPruebaAnimaciones : MonoBehaviour
{
    class Fase
    {
        public string nombre, estado;
        public int min, max, distintosMin;
        public bool trabajar, espaldas, caminar;
        public int frames, enEstado, fueraDeFila, cambios, capturas;
        public float velMax;
        public bool flipSi, flipNo;
        public readonly HashSet<string> otrosEstados = new HashSet<string>();
        public readonly SortedSet<int> vistos = new SortedSet<int>();
        public readonly List<int> secuencia = new List<int>();
    }

    const float Asentar = 0.35f, Duracion = 2.4f, VelPrueba = 0.5f;
    const int Tam = 192, Columnas = 6, FilasCaptura = 4;
    static readonly string[] Estados = { "Idle", "Walk", "Work", "Back" };

    Animator animator;
    SpriteRenderer sr;
    NioAnimacion nio;
    readonly List<Fase> fases = new List<Fase>();
    readonly StringBuilder notas = new StringBuilder();
    Fase actual;
    int fila;
    bool grabando;
    int ultimo = int.MinValue;
    Camera camCaptura;
    RenderTexture rt;
    Texture2D hoja;

    static string Ruta(string n)
    {
        string d = Path.Combine(Directory.GetParent(Application.dataPath).FullName, "Logs");
        Directory.CreateDirectory(d);
        return Path.Combine(d, n);
    }

    IEnumerator Start()
    {
        animator = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        nio = GetComponent<NioAnimacion>();
        if (animator == null || sr == null || nio == null || animator.runtimeAnimatorController == null)
        {
            File.WriteAllText(Ruta("Nio_PruebaPlayMode.txt"), "ERROR: a Nio le falta Animator, SpriteRenderer, NioAnimacion o el controlador.");
            Debug.LogError("[Nio] Prueba cancelada: faltan componentes en Nio.");
            Terminar();
            yield break;
        }

        fases.Add(new Fase { nombre = "Nio_Idle", estado = "Idle", min = 0, max = 3, distintosMin = 4 });
        fases.Add(new Fase { nombre = "Nio_Walk", estado = "Walk", min = 4, max = 9, distintosMin = 6, caminar = true });
        fases.Add(new Fase { nombre = "Nio_Work", estado = "Work", min = 10, max = 13, distintosMin = 4, trabajar = true });
        fases.Add(new Fase { nombre = "Nio_Back", estado = "Back", min = 14, max = 17, distintosMin = 4, espaldas = true });
        fases.Add(new Fase { nombre = "Vuelta a Idle", estado = "Idle", min = 0, max = 3, distintosMin = 2 });

        PrepararCaptura();
        yield return null;

        Camera principal = Camera.main;
        if (principal != null)
        {
            bool dentro = GeometryUtility.TestPlanesAABB(GeometryUtility.CalculateFrustumPlanes(principal), sr.bounds);
            notas.AppendLine("Cámara de juego: " + principal.name + "; Nio dentro de su encuadre: " + (dentro ? "sí" : "NO"));
        }
        else notas.AppendLine("Cámara de juego: no hay Camera.main");

        Vector3 origen = transform.position;
        Vector3 derecha = principal != null ? Vector3.ProjectOnPlane(principal.transform.right, Vector3.up).normalized : Vector3.right;
        float total = Asentar + Duracion;

        for (int i = 0; i < fases.Count; i++)
        {
            Fase f = fases[i];
            actual = f;
            fila = i;
            ultimo = int.MinValue;
            nio.trabajando = f.trabajar;
            nio.deEspaldas = f.espaldas;
            nio.velocidadManual = -1f;
            float t = 0f;
            while (t < total)
            {
                if (f.caminar)
                {
                    float d = t < total * 0.5f ? t : total - t; // ida hacia la derecha y vuelta
                    transform.position = origen + derecha * (VelPrueba * d);
                }
                grabando = t >= Asentar;
                yield return null;
                t += Time.deltaTime;
            }
            grabando = false;
            transform.position = origen;
        }
        actual = null;
        nio.trabajando = false;
        nio.deEspaldas = false;
        Escribir();
        Terminar();
    }

    void LateUpdate()
    {
        if (!grabando || actual == null) return;
        Fase f = actual;
        f.frames++;
        string est = Estado();
        if (est == f.estado) f.enEstado++; else f.otrosEstados.Add(est);
        int idx = Indice(sr.sprite);
        f.vistos.Add(idx);
        if (idx < f.min || idx > f.max) f.fueraDeFila++;
        f.velMax = Mathf.Max(f.velMax, nio.Velocidad);
        if (sr.flipX) f.flipSi = true; else f.flipNo = true;
        if (idx != ultimo)
        {
            if (ultimo != int.MinValue) f.cambios++;
            if (f.secuencia.Count < 40) f.secuencia.Add(idx);
            ultimo = idx;
            if (fila < FilasCaptura && f.capturas < Columnas)
            {
                Capturar(fila, f.capturas);
                f.capturas++;
            }
        }
    }

    string Estado()
    {
        AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);
        foreach (string e in Estados) if (info.IsName(e)) return e;
        return "desconocido";
    }

    static int Indice(Sprite s)
    {
        if (s == null) return -1;
        int i = s.name.LastIndexOf('_');
        return i >= 0 && int.TryParse(s.name.Substring(i + 1), out int v) ? v : -2;
    }

    void PrepararCaptura()
    {
        var go = new GameObject("NioPrueba_CamaraCaptura");
        camCaptura = go.AddComponent<Camera>();
        camCaptura.enabled = false;
        camCaptura.fieldOfView = 36f;
        camCaptura.nearClipPlane = 0.05f;
        camCaptura.farClipPlane = 40f;
        camCaptura.clearFlags = CameraClearFlags.SolidColor;
        camCaptura.backgroundColor = new Color(0.15f, 0.16f, 0.19f, 1f);
        rt = new RenderTexture(Tam, Tam, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
        hoja = new Texture2D(Columnas * Tam, FilasCaptura * Tam, TextureFormat.RGB24, false);
        var px = new Color32[hoja.width * hoja.height];
        for (int i = 0; i < px.Length; i++) px[i] = new Color32(30, 30, 34, 255);
        hoja.SetPixels32(px);
    }

    void Capturar(int r, int c)
    {
        Camera principal = Camera.main;
        Vector3 fwd = principal != null ? Vector3.ProjectOnPlane(principal.transform.forward, Vector3.up).normalized : transform.forward;
        Vector3 centro = sr.bounds.center;
        camCaptura.transform.SetPositionAndRotation(centro - fwd * 1.3f + Vector3.up * 0.05f, Quaternion.LookRotation(fwd, Vector3.up));
        try
        {
            var req = new RenderPipeline.StandardRequest { destination = rt };
            if (RenderPipeline.SupportsRenderRequest(camCaptura, req)) RenderPipeline.SubmitRenderRequest(camCaptura, req);
            else { camCaptura.targetTexture = rt; camCaptura.Render(); camCaptura.targetTexture = null; }
            RenderTexture prev = RenderTexture.active;
            RenderTexture.active = rt;
            hoja.ReadPixels(new Rect(0, 0, Tam, Tam), c * Tam, (FilasCaptura - 1 - r) * Tam);
            RenderTexture.active = prev;
        }
        catch (System.Exception e)
        {
            notas.AppendLine("Captura no disponible: " + e.Message);
        }
    }

    void Escribir()
    {
        var sb = new StringBuilder();
        sb.AppendLine("PRUEBA DE ANIMACIONES DE NIO EN PLAY MODE — " + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        sb.AppendLine("Unity " + Application.unityVersion + " | escena " + gameObject.scene.path + " | objeto '" + name + "' | controlador '" + animator.runtimeAnimatorController.name + "'");
        sb.AppendLine("Clips del controlador: " + string.Join(", ", animator.runtimeAnimatorController.animationClips.Distinct()
            .Select(c => c.name + " (" + c.length.ToString("0.###") + " s, " + c.frameRate + " fps, bucle " + (c.isLooping ? "sí" : "no") + ")")));
        sb.Append(notas);
        sb.AppendLine("Método: cada fase dura " + (Asentar + Duracion).ToString("0.##") + " s; se registran los últimos " + Duracion.ToString("0.##") +
                      " s frame a frame (estado del Animator + sprite del SpriteRenderer). En Walk, Nio se desplaza " + VelPrueba + " m/s a la derecha y vuelve.");
        sb.AppendLine();

        int ok = 0;
        foreach (Fase f in fases)
        {
            bool estadoOk = f.frames > 0 && f.enEstado == f.frames;
            bool filaOk = f.fueraDeFila == 0;
            bool variedad = f.vistos.Count >= f.distintosMin;
            bool cambia = f.cambios >= 2;
            bool flipOk = !f.caminar || (f.flipSi && f.flipNo);
            bool velOk = !f.caminar || f.velMax > 0.1f;
            bool pasa = estadoOk && filaOk && variedad && cambia && flipOk && velOk;
            if (pasa) ok++;
            string linea = "[" + (pasa ? "OK" : "FALLO") + "] " + f.nombre + ": estado '" + f.estado + "' en " + f.enEstado + "/" + f.frames + " frames" +
                           (f.otrosEstados.Count > 0 ? " (otros: " + string.Join(",", f.otrosEstados) + ")" : "") +
                           "; sprites vistos [" + string.Join(",", f.vistos) + "] (" + f.vistos.Count + " distintos, mínimo " + f.distintosMin + ")" +
                           "; fuera de su fila: " + f.fueraDeFila + "; cambios de sprite: " + f.cambios;
            if (f.caminar)
                linea += "; velocidad máx " + f.velMax.ToString("0.00") + " m/s; mira a la derecha: " + (f.flipNo ? "sí" : "no") + ", a la izquierda (flipX): " + (f.flipSi ? "sí" : "no");
            sb.AppendLine(linea);
            sb.AppendLine("      secuencia: " + string.Join(" > ", f.secuencia));
        }
        sb.AppendLine();
        sb.AppendLine("Capturas: Logs/Nio_PruebaPlayMode_capturas.png (filas Idle, Walk, Work, Back; un fotograma por cada cambio de sprite).");
        sb.AppendLine("RESULTADO: " + ok + "/" + fases.Count + " fases superadas");
        File.WriteAllText(Ruta("Nio_PruebaPlayMode.txt"), sb.ToString());
        hoja.Apply();
        File.WriteAllBytes(Ruta("Nio_PruebaPlayMode_capturas.png"), hoja.EncodeToPNG());
        if (ok == fases.Count) Debug.Log("[Nio] Prueba en Play Mode: " + ok + "/" + fases.Count + " fases OK. Informe: Logs/Nio_PruebaPlayMode.txt");
        else Debug.LogWarning("[Nio] Prueba en Play Mode: " + ok + "/" + fases.Count + " fases OK. Revisa Logs/Nio_PruebaPlayMode.txt");
    }

    void Terminar()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    void OnDestroy()
    {
        if (rt != null) rt.Release();
    }
}
