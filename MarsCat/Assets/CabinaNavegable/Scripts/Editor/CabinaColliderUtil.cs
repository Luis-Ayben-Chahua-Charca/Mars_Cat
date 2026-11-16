// Asistencia de IA: escrito con Claude Code (Anthropic).
// Reglas comunes para convertir las mallas COL_* de CABINA_COLLIDERS.fbx en colliders de Unity.
//  - COL_TRG_*            -> BoxCollider (Is Trigger)            [zona de detección de puertas]
//  - COL_PUERTA_*         -> BoxCollider                         [bloqueo con la puerta cerrada; lo gestiona la puerta]
//  - caja alineada local  -> BoxCollider                         [muros rectos, obstáculos, límites de mapa]
//  - resto                -> MeshCollider no convexo             [muros circulares, cajas giradas]
using UnityEngine;

public static class CabinaColliderUtil
{
    public static bool EsCajaLocal(Mesh m)
    {
        try
        {
            var v = m.vertices;
            if (v.Length == 0 || m.triangles.Length != 36) return false;
            Bounds b = m.bounds;
            const float tol = 1e-3f;
            foreach (var p in v)
            {
                if (Mathf.Abs(p.x - b.min.x) > tol && Mathf.Abs(p.x - b.max.x) > tol) return false;
                if (Mathf.Abs(p.y - b.min.y) > tol && Mathf.Abs(p.y - b.max.y) > tol) return false;
                if (Mathf.Abs(p.z - b.min.z) > tol && Mathf.Abs(p.z - b.max.z) > tol) return false;
            }
            return true;
        }
        catch (System.Exception)
        {
            return false;   // malla no legible: se usará MeshCollider
        }
    }

    /// <summary>Añade el collider adecuado (si aún no tiene) y devuelve el collider resultante.</summary>
    public static Collider Configurar(GameObject go, Mesh mesh)
    {
        var existente = go.GetComponent<Collider>();
        if (existente != null || mesh == null) return existente;
        string n = go.name;
        bool trigger = n.StartsWith("COL_TRG_");
        if (trigger || n.StartsWith("COL_PUERTA_") || EsCajaLocal(mesh))
        {
            var bc = go.AddComponent<BoxCollider>();
            bc.center = mesh.bounds.center;
            bc.size = mesh.bounds.size;
            bc.isTrigger = trigger;
            return bc;
        }
        var mc = go.AddComponent<MeshCollider>();
        mc.sharedMesh = mesh;
        mc.convex = false;
        return mc;
    }
}
