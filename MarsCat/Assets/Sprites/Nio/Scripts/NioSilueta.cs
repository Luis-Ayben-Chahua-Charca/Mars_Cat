// Asistencia de IA: escrito con Claude Code (Anthropic).
using UnityEngine;

/// <summary>
/// Copia en cada frame el sprite y la orientación de Nio (Visual) a la silueta que se ve a través de los muros.
/// Va en el hijo "Silueta" de Nio_Player/Visual; su material usa el shader Nio/SiluetaOculta.
/// </summary>
[ExecuteAlways]
[DisallowMultipleComponent]
[RequireComponent(typeof(SpriteRenderer))]
public class NioSilueta : MonoBehaviour
{
    public SpriteRenderer origen;
    SpriteRenderer sr;

    void OnEnable()
    {
        sr = GetComponent<SpriteRenderer>();
        if (origen == null && transform.parent != null) origen = transform.parent.GetComponent<SpriteRenderer>();
    }

    void LateUpdate()
    {
        if (origen == null || sr == null) return;
        if (sr.sprite != origen.sprite) sr.sprite = origen.sprite;
        float sx = origen.flipX ? -1f : 1f;
        if (!Mathf.Approximately(transform.localScale.x, sx)) transform.localScale = new Vector3(sx, 1f, 1f);
        bool ver = origen.enabled;
        if (sr.enabled != ver) sr.enabled = ver;
    }
}
