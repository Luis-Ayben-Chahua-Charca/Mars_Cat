// Componente de datos: guarda las propiedades personalizadas exportadas desde Blender
// (tipo, puerta_id, recorrido_m, apertura_dir_unity, senal_id, ...). Lo rellena CabinaImportPostprocessor al importar.
using System.Globalization;
using UnityEngine;

[DisallowMultipleComponent]
public class CabinaPropiedades : MonoBehaviour
{
    public string[] claves = new string[0];
    public string[] valores = new string[0];

    public string Obtener(string clave)
    {
        for (int i = 0; i < claves.Length && i < valores.Length; i++)
            if (claves[i] == clave) return valores[i];
        return null;
    }

    public float ObtenerFloat(string clave, float porDefecto = 0f)
    {
        var s = Obtener(clave);
        float v;
        return !string.IsNullOrEmpty(s) && float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out v) ? v : porDefecto;
    }

    public Vector3 ObtenerVector(string clave)
    {
        var s = Obtener(clave);
        if (string.IsNullOrEmpty(s)) return Vector3.zero;
        var p = s.Split(',');
        if (p.Length < 3) return Vector3.zero;
        float x, y, z;
        var ci = CultureInfo.InvariantCulture;
        if (float.TryParse(p[0], NumberStyles.Float, ci, out x) && float.TryParse(p[1], NumberStyles.Float, ci, out y) &&
            float.TryParse(p[2], NumberStyles.Float, ci, out z))
            return new Vector3(x, y, z);
        return Vector3.zero;
    }
}
