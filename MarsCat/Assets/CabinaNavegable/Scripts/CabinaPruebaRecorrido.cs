// Asistencia de IA: escrito con Claude Code (Anthropic).
// Prueba automática de navegación EN UNITY (física real de Unity + puertas con su script).
// Recorre los waypoints calculados en la auditoría (CabinaDatos.Recorrido):
// acceso principal -> núcleo -> M-01..M-06 (volviendo al núcleo) -> zona técnica -> salida sur.
// Falla si el jugador se atasca, cae o sale del cerramiento. Muestra el progreso en pantalla y escribe
// el resultado en la Consola y en <proyecto>/CabinaPruebaRecorrido_resultado.txt.
// Añádelo al JUGADOR (menú Cabina > 3. Ejecutar prueba automática de recorrido) y pulsa Play.
using System.Collections;
using System.IO;
using System.Text;
using UnityEngine;

[RequireComponent(typeof(CabinaControladorPrimeraPersona))]
public class CabinaPruebaRecorrido : MonoBehaviour
{
    public float velocidad = 3.0f;
    public float radioLlegada = 0.3f;
    [Tooltip("Segundos sin acercarse al siguiente punto para considerar que el jugador está atascado.")]
    public float tiempoAtasco = 4f;

    CabinaControladorPrimeraPersona ctrl;
    CharacterController cc;
    readonly StringBuilder log = new StringBuilder();
    string estado = "Preparando…";
    int alcanzados, objetivosTotales;
    bool espejoX;

    void Registrar(string s) { log.AppendLine(s); Debug.Log("[CabinaPrueba] " + s); estado = s; }

    Vector3 Pos(Vector3 p) { return espejoX ? new Vector3(-p.x, p.y, p.z) : p; }

    IEnumerator Start()
    {
        ctrl = GetComponent<CabinaControladorPrimeraPersona>();
        cc = GetComponent<CharacterController>();
        ctrl.controlManual = false;
        yield return null;

        // 1) Comprobación de ejes: el núcleo debe estar en (1.1, y, 0) si Unity espeja X al importar el FBX.
        var nucleo = GameObject.Find("MC_Pared_Circular");
        if (nucleo != null && nucleo.GetComponent<Renderer>() != null)
        {
            Vector3 c = nucleo.GetComponent<Renderer>().bounds.center; c.y = 0f;
            Vector3 e = CabinaDatos.CentroNucleoEsperado;
            if (Vector3.Distance(c, e) < 0.25f) Registrar("Ejes OK: núcleo en " + c.ToString("F2"));
            else if (Vector3.Distance(c, new Vector3(-e.x, 0f, e.z)) < 0.25f)
            { espejoX = true; Registrar("AVISO: X sin espejar (núcleo en " + c.ToString("F2") + "). Se corrigen los waypoints."); }
            else Registrar("AVISO: núcleo en posición inesperada " + c.ToString("F2") + " (esperado " + e.ToString("F2") + ").");
        }
        else Registrar("AVISO: no se encontró MC_Pared_Circular; no se comprueban los ejes.");

        // 2) Colocar al jugador en el acceso principal
        cc.enabled = false;
        transform.position = Pos(CabinaDatos.InicioRecorrido) + Vector3.up * 0.05f;
        cc.enabled = true;
        yield return new WaitForSeconds(0.3f);

        foreach (var p in CabinaDatos.Recorrido) if (!string.IsNullOrEmpty(p.objetivo)) objetivosTotales++;
        float t0 = Time.time, distancia = 0f;
        Vector3 anterior = transform.position;
        bool ok = true;

        for (int i = 0; i < CabinaDatos.Recorrido.Length && ok; i++)
        {
            var wp = CabinaDatos.Recorrido[i];
            Vector3 destino = Pos(wp.posicion);
            float mejor = float.MaxValue, tMejor = Time.time;
            while (true)
            {
                Vector3 d = destino - transform.position; d.y = 0f;
                float dist = d.magnitude;
                if (dist < radioLlegada) break;
                if (dist < mejor - 0.05f) { mejor = dist; tMejor = Time.time; }
                if (Time.time - tMejor > tiempoAtasco)
                { ok = false; Registrar("FALLO: atascado en " + transform.position.ToString("F2") + " (tramo «" + wp.tramo + "», punto " + i + ")"); break; }
                if (transform.position.y < -1f) { ok = false; Registrar("FALLO: el jugador cayó en " + transform.position.ToString("F2")); break; }
                if (Mathf.Abs(transform.position.x) > 13.2f || Mathf.Abs(transform.position.z) > 15.2f)
                { ok = false; Registrar("FALLO: el jugador salió del cerramiento en " + transform.position.ToString("F2")); break; }
                Vector3 dir = d / dist;
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir, Vector3.up), 8f * Time.deltaTime);
                ctrl.MoverAutomatico(dir * velocidad);
                distancia += Vector3.Distance(anterior, transform.position); anterior = transform.position;
                yield return null;
            }
            if (ok && !string.IsNullOrEmpty(wp.objetivo))
            {
                alcanzados++;
                Registrar(string.Format("OK  {0}/{1}  {2}  (t = {3:F1} s)", alcanzados, objetivosTotales, wp.objetivo, Time.time - t0));
            }
        }
        ctrl.MoverAutomatico(Vector3.zero);

        var sb = new StringBuilder();
        sb.AppendLine(ok ? "RESULTADO: RECORRIDO COMPLETADO" : "RESULTADO: RECORRIDO FALLIDO");
        sb.AppendLine(string.Format("Objetivos alcanzados: {0}/{1} · tiempo {2:F1} s · distancia {3:F1} m", alcanzados, objetivosTotales, Time.time - t0, distancia));
        var puertas = new System.Collections.Generic.List<CabinaPuertaAutomatica>();
        foreach (var r in gameObject.scene.GetRootGameObjects()) puertas.AddRange(r.GetComponentsInChildren<CabinaPuertaAutomatica>(true));
        foreach (var p in puertas) sb.AppendLine(string.Format("Puerta {0}: aperturas completas = {1}", p.puertaId, p.Aperturas));
        Registrar(sb.ToString());
        try
        {
            string ruta = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "CabinaPruebaRecorrido_resultado.txt"));
            File.WriteAllText(ruta, log.ToString(), Encoding.UTF8);
            Debug.Log("[CabinaPrueba] Resultado guardado en " + ruta);
        }
        catch (System.Exception ex) { Debug.LogWarning("[CabinaPrueba] No se pudo guardar el resultado: " + ex.Message); }
        ctrl.controlManual = true;
    }

    void OnGUI()
    {
        GUI.Box(new Rect(10, 10, 620, 56), "");
        GUI.Label(new Rect(20, 16, 600, 22), "Prueba automática de recorrido · objetivos " + alcanzados + "/" + objetivosTotales);
        GUI.Label(new Rect(20, 38, 600, 22), estado.Split('\n')[0]);
    }
}
