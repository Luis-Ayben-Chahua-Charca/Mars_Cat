// Asistencia de IA: escrito con Claude Code (Anthropic).
using System;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Apertura y cierre del domo de una cápsula. Solo mueve las piezas indicadas (transformaciones locales respecto a
/// su posición cerrada); la base no se toca.
///
/// Dos modos:
///  - Con Animator asignado: activa el bool "parametroAbierto" y los Animation Clips del controlador hacen el movimiento.
///  - Sin Animator: interpola cada pieza desde su pose cerrada a "rotacionAbierta"/"desplazamientoAbierto" con una curva
///    suave, en "duracion" segundos (cada pieza puede empezar con retraso para aperturas escalonadas).
///
/// Integración prevista (cuando Capsula.blend esté disponible): exportar desde Blender con el domo en un objeto
/// independiente y el origen en su bisagra; en Unity, añadir este componente a la raíz de la cápsula y arrastrar el domo
/// a "piezas". Botones de prueba en el menú contextual (⋮) o llamando a Abrir() / Cerrar() desde un evento.
/// </summary>
[DisallowMultipleComponent]
public class CapsuleDomeController : MonoBehaviour
{
    [Serializable]
    public class Pieza
    {
        public Transform transform;
        [Tooltip("Giro local (grados) en la posición abierta, respecto a la pose cerrada.")]
        public Vector3 rotacionAbierta = new Vector3(-100f, 0f, 0f);
        [Tooltip("Desplazamiento local (m) en la posición abierta.")]
        public Vector3 desplazamientoAbierto;
        [Tooltip("Fracción del recorrido total antes de que esta pieza empiece a moverse.")]
        [Range(0f, 0.9f)] public float retraso;
    }

    public Pieza[] piezas = new Pieza[0];
    [Min(0.05f)] public float duracion = 1.6f;
    public AnimationCurve curva = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    public bool abiertoAlIniciar;

    [Header("Modo Animator (opcional)")]
    public Animator animator;
    public string parametroAbierto = "Abierto";

    [Header("Eventos")]
    public UnityEvent alTerminarApertura = new UnityEvent();
    public UnityEvent alTerminarCierre = new UnityEvent();
    public event Action<bool> AlTerminar;

    /// <summary>0 = cerrado, 1 = abierto.</summary>
    public float Progreso { get; private set; }
    public bool Abierto => Progreso >= 1f;
    public bool Cerrado => Progreso <= 0f;
    public bool EnMovimiento => !Mathf.Approximately(Progreso, objetivo);

    Quaternion[] rotCerrada;
    Vector3[] posCerrada;
    float objetivo;

    void Awake()
    {
        rotCerrada = new Quaternion[piezas.Length];
        posCerrada = new Vector3[piezas.Length];
        for (int i = 0; i < piezas.Length; i++)
            if (piezas[i]?.transform != null)
            {
                rotCerrada[i] = piezas[i].transform.localRotation;
                posCerrada[i] = piezas[i].transform.localPosition;
            }
        Progreso = objetivo = abiertoAlIniciar ? 1f : 0f;
        Aplicar(Progreso);
        if (animator != null) animator.SetBool(parametroAbierto, abiertoAlIniciar);
    }

    [ContextMenu("Prueba: abrir")]
    public void Abrir() => Fijar(true);

    [ContextMenu("Prueba: cerrar")]
    public void Cerrar() => Fijar(false);

    public void Alternar() => Fijar(objetivo < 0.5f);

    void Fijar(bool abrir)
    {
        objetivo = abrir ? 1f : 0f;
        if (animator != null) animator.SetBool(parametroAbierto, abrir);
    }

    void Update()
    {
        if (animator != null || Mathf.Approximately(Progreso, objetivo)) return;
        Progreso = Mathf.MoveTowards(Progreso, objetivo, Time.deltaTime / duracion);
        Aplicar(Progreso);
        if (Mathf.Approximately(Progreso, objetivo))
        {
            bool abierto = objetivo > 0.5f;
            AlTerminar?.Invoke(abierto);
            if (abierto) alTerminarApertura?.Invoke(); else alTerminarCierre?.Invoke();
        }
    }

    void Aplicar(float t)
    {
        if (rotCerrada == null) return;
        for (int i = 0; i < piezas.Length; i++)
        {
            Pieza p = piezas[i];
            if (p?.transform == null) continue;
            float tp = Mathf.Clamp01((t - p.retraso) / Mathf.Max(0.0001f, 1f - p.retraso));
            float k = curva.Evaluate(tp);
            p.transform.localRotation = rotCerrada[i] * Quaternion.Euler(p.rotacionAbierta * k);
            p.transform.localPosition = posCerrada[i] + p.desplazamientoAbierto * k;
        }
    }
}
