// Puerta automática deslizante. Va en el objeto trigger COL_TRG_PUERTA_<id> (BoxCollider con Is Trigger).
// La configura automáticamente el menú Cabina > 2. Montar escena de prueba.
// - Se abre cuando un CharacterController / Rigidbody / objeto con tag "Player" entra en el trigger.
// - Mientras haya alguien dentro permanece abierta; se cierra tras "retardoCierre" segundos.
// - El collider de bloqueo (COL_PUERTA_<id>) se desactiva en cuanto empieza a abrirse y se reactiva al cerrarse del todo.
// - Modo Deslizar: cada hoja se desplaza "desplazamientos[i]" (muros rectos: la hoja entra en el hueco del muro).
// - Modo GirarSobreEje: las hojas giran "anguloGrados" alrededor de "centroGiro" (puertas en muro curvo de las salas:
//   la hoja recorre el arco del muro en lugar de salir en línea recta hacia el vestíbulo).
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class CabinaPuertaAutomatica : MonoBehaviour
{
    public string puertaId;
    [Tooltip("Hojas de la puerta (objetos PUERTA_*).")]
    public Transform[] hojas = new Transform[0];
    [Tooltip("Desplazamiento en coordenadas de mundo de cada hoja cuando la puerta está totalmente abierta.")]
    public Vector3[] desplazamientos = new Vector3[0];
    public enum Modo { Deslizar, GirarSobreEje }
    public Modo modo = Modo.Deslizar;
    [Tooltip("Modo GirarSobreEje: punto (mundo) del eje vertical de giro = centro de la sala.")]
    public Vector3 centroGiro;
    [Tooltip("Modo GirarSobreEje: ángulo de apertura en grados (con signo).")]
    public float anguloGrados;
    [Tooltip("Collider que bloquea el paso con la puerta cerrada (COL_PUERTA_<id>).")]
    public Collider colliderBloqueo;
    [Tooltip("Si está marcada, la puerta no se abre.")]
    public bool bloqueada = false;
    [Tooltip("Invierte el sentido de apertura si una hoja se abre hacia el lado equivocado.")]
    public bool invertirSentido = false;
    public float velocidad = 4f;          // m/s de las hojas
    public float retardoCierre = 0.6f;    // s
    [Tooltip("Opcional: renderer de la baliza INT_LuzAdvertencia_<id> (se activa mientras la puerta se mueve).")]
    public Renderer indicador;

    public int Aperturas { get; private set; }
    public float Apertura01 { get { return apertura; } }
    public bool Abierta { get { return apertura >= 0.999f; } }

    Vector3[] cerradas;
    Quaternion[] rotCerradas;
    float apertura, recorrido = 1f, tSalida;
    bool contadaApertura;
    readonly HashSet<Collider> ocupantes = new HashSet<Collider>();

    void Awake()
    {
        cerradas = new Vector3[hojas.Length];
        rotCerradas = new Quaternion[hojas.Length];
        for (int i = 0; i < hojas.Length; i++)
        {
            cerradas[i] = hojas[i] != null ? hojas[i].position : Vector3.zero;
            rotCerradas[i] = hojas[i] != null ? hojas[i].rotation : Quaternion.identity;
            if (i < desplazamientos.Length) recorrido = Mathf.Max(recorrido, desplazamientos[i].magnitude);
        }
        if (modo == Modo.GirarSobreEje && hojas.Length > 0 && hojas[0] != null)
        {
            Vector3 r = hojas[0].position - centroGiro; r.y = 0f;
            recorrido = Mathf.Max(0.05f, Mathf.Abs(anguloGrados) * Mathf.Deg2Rad * r.magnitude);   // longitud del arco
        }
        var c = GetComponent<Collider>();
        if (c != null) c.isTrigger = true;
        var mr = GetComponent<MeshRenderer>();
        if (mr != null) mr.enabled = false;
    }

    static bool EsActor(Collider c)
    {
        return c is CharacterController || c.attachedRigidbody != null || c.CompareTag("Player");
    }

    void OnTriggerEnter(Collider c) { if (EsActor(c)) ocupantes.Add(c); }

    void OnTriggerExit(Collider c)
    {
        if (ocupantes.Remove(c)) tSalida = Time.time;
    }

    void Update()
    {
        ocupantes.RemoveWhere(c => c == null || !c.enabled || !c.gameObject.activeInHierarchy);
        bool abrir = !bloqueada && ocupantes.Count > 0;
        float objetivo = abrir ? 1f : (Time.time - tSalida > retardoCierre ? 0f : apertura);
        apertura = Mathf.MoveTowards(apertura, objetivo, Time.deltaTime * velocidad / Mathf.Max(0.05f, recorrido));

        float s = invertirSentido ? -1f : 1f;
        if (modo == Modo.GirarSobreEje)
        {
            Quaternion q = Quaternion.AngleAxis(anguloGrados * apertura * s, Vector3.up);
            for (int i = 0; i < hojas.Length; i++)
                if (hojas[i] != null)
                {
                    hojas[i].position = centroGiro + q * (cerradas[i] - centroGiro);
                    hojas[i].rotation = q * rotCerradas[i];
                }
        }
        else
        {
            for (int i = 0; i < hojas.Length; i++)
                if (hojas[i] != null && i < desplazamientos.Length)
                    hojas[i].position = cerradas[i] + desplazamientos[i] * (apertura * s);
        }

        if (colliderBloqueo != null) colliderBloqueo.enabled = apertura <= 0.001f && !abrir;
        if (indicador != null)
            indicador.enabled = (apertura > 0.001f && apertura < 0.999f) || (bloqueada && (Time.time * 2f) % 1f > 0.5f);

        if (apertura >= 0.999f && !contadaApertura) { Aperturas++; contadaApertura = true; }
        if (apertura <= 0.001f) contadaApertura = false;
    }
}
