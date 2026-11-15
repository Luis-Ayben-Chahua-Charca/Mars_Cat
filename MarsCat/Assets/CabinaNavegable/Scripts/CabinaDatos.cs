// GENERADO AUTOMÁTICAMENTE por SCRIPTS_BLENDER/audit/gen_unity_data.py a partir de CABINA_NAVEGABLE.blend
// y de la auditoría de navegación. No editar a mano: regenerar si cambia el modelo.
// Coordenadas de Unity esperadas = (-x, z, -y) de Blender (conversión FBX -Z Forward / Y Up + espejo X de Unity).
using UnityEngine;

public static class CabinaDatos
{
    public struct Mat { public string nombre; public Color color; public float metalico, suavidad; public string albedo, normal, emision; public Color colorEmision; public float intensidadEmision; }
    public struct Luz { public string nombre; public Vector3 posicion; public Color color; public float intensidad, alcance; }
    public struct Punto { public string tramo, objetivo; public Vector3 posicion; }

    public static readonly Mat[] Materiales = {
        new Mat { nombre = "MAT_Cable", color = new Color(0.1517f, 0.1517f, 0.1517f), metalico = 0.0000f, suavidad = 0.3000f, albedo = "", normal = "", emision = "", colorEmision = new Color(0.0000f, 0.0000f, 0.0000f), intensidadEmision = 0.0000f },
        new Mat { nombre = "MAT_Consola_Gris_Oscuro", color = new Color(0.3811f, 0.3959f, 0.4236f), metalico = 0.5000f, suavidad = 0.6000f, albedo = "", normal = "", emision = "", colorEmision = new Color(0.0000f, 0.0000f, 0.0000f), intensidadEmision = 0.0000f },
        new Mat { nombre = "MAT_Cubierta_Metal", color = new Color(1.0000f, 1.0000f, 1.0000f), metalico = 0.5000f, suavidad = 0.5000f, albedo = "T_Cubierta_Panel_Albedo", normal = "T_Cubierta_Panel_Normal", emision = "", colorEmision = new Color(0.0000f, 0.0000f, 0.0000f), intensidadEmision = 0.0000f },
        new Mat { nombre = "MAT_Estructura_Negro", color = new Color(0.1718f, 0.1718f, 0.1897f), metalico = 0.8000f, suavidad = 0.6500f, albedo = "", normal = "", emision = "", colorEmision = new Color(0.0000f, 0.0000f, 0.0000f), intensidadEmision = 0.0000f },
        new Mat { nombre = "MAT_Franjas_Seguridad", color = new Color(1.0000f, 1.0000f, 1.0000f), metalico = 0.0000f, suavidad = 0.4000f, albedo = "T_Franjas_Seguridad_Albedo", normal = "", emision = "", colorEmision = new Color(0.0000f, 0.0000f, 0.0000f), intensidadEmision = 0.0000f },
        new Mat { nombre = "MAT_LED_Ambar_Advertencia", color = new Color(1.0000f, 0.7674f, 0.2478f), metalico = 0.0000f, suavidad = 0.7000f, albedo = "", normal = "", emision = "", colorEmision = new Color(1.0000f, 0.7674f, 0.2478f), intensidadEmision = 8.0000f },
        new Mat { nombre = "MAT_LED_Cian", color = new Color(0.3492f, 0.9309f, 1.0000f), metalico = 0.0000f, suavidad = 0.7000f, albedo = "", normal = "", emision = "", colorEmision = new Color(0.3492f, 0.9309f, 1.0000f), intensidadEmision = 6.0000f },
        new Mat { nombre = "MAT_Pantalla_UI", color = new Color(1.0000f, 1.0000f, 1.0000f), metalico = 0.0000f, suavidad = 0.7500f, albedo = "T_Pantalla_UI_Emision", normal = "", emision = "T_Pantalla_UI_Emision", colorEmision = new Color(1.0000f, 1.0000f, 1.0000f), intensidadEmision = 2.5000f },
        new Mat { nombre = "MAT_Pared_Metal", color = new Color(1.0000f, 1.0000f, 1.0000f), metalico = 0.5500f, suavidad = 0.5800f, albedo = "T_Pared_Metal_Panel_Albedo", normal = "T_Pared_Metal_Panel_Normal", emision = "", colorEmision = new Color(0.0000f, 0.0000f, 0.0000f), intensidadEmision = 0.0000f },
        new Mat { nombre = "MAT_Puerta_Metal", color = new Color(0.8650f, 0.8756f, 0.8911f), metalico = 0.6000f, suavidad = 0.6500f, albedo = "", normal = "", emision = "", colorEmision = new Color(0.0000f, 0.0000f, 0.0000f), intensidadEmision = 0.0000f },
        new Mat { nombre = "MAT_Rejilla_Metal", color = new Color(1.0000f, 1.0000f, 1.0000f), metalico = 0.7000f, suavidad = 0.5500f, albedo = "T_Rejilla_Tecnica_Albedo", normal = "T_Rejilla_Tecnica_Normal", emision = "", colorEmision = new Color(0.0000f, 0.0000f, 0.0000f), intensidadEmision = 0.0000f },
        new Mat { nombre = "MAT_Senal_Amarilla", color = new Color(0.9777f, 0.8650f, 0.2209f), metalico = 0.0000f, suavidad = 0.5000f, albedo = "", normal = "", emision = "", colorEmision = new Color(0.9777f, 0.8650f, 0.2209f), intensidadEmision = 0.1500f },
        new Mat { nombre = "MAT_Senal_Placa", color = new Color(0.1897f, 0.2209f, 0.2717f), metalico = 0.3000f, suavidad = 0.6000f, albedo = "", normal = "", emision = "", colorEmision = new Color(0.0000f, 0.0000f, 0.0000f), intensidadEmision = 0.0000f },
        new Mat { nombre = "MAT_Senales_Atlas", color = new Color(1.0000f, 1.0000f, 1.0000f), metalico = 0.0000f, suavidad = 0.6500f, albedo = "T_Senales_Atlas", normal = "", emision = "T_Senales_Atlas", colorEmision = new Color(1.0000f, 1.0000f, 1.0000f), intensidadEmision = 1.4000f },
        new Mat { nombre = "MAT_Suelo_Placas", color = new Color(1.0000f, 1.0000f, 1.0000f), metalico = 0.4000f, suavidad = 0.4500f, albedo = "T_Suelo_Placas_Albedo", normal = "T_Suelo_Placas_Normal", emision = "", colorEmision = new Color(0.0000f, 0.0000f, 0.0000f), intensidadEmision = 0.0000f },
        new Mat { nombre = "MAT_Tuberia_Metal", color = new Color(0.7014f, 0.7152f, 0.7354f), metalico = 0.8500f, suavidad = 0.7000f, albedo = "", normal = "", emision = "", colorEmision = new Color(0.0000f, 0.0000f, 0.0000f), intensidadEmision = 0.0000f },
    };

    // Luces aproximadas (intensidad/alcance para URP o Built-in; ajustar a gusto).
    public static readonly Luz[] Luces = {
        new Luz { nombre = "LUZ_Advertencia_MID_Exterior", posicion = new Vector3(-5.6695f, 2.4500f, 9.3851f), color = new Color(1.0000f, 0.7354f, 0.2478f), intensidad = 0.8000f, alcance = 2.2000f },
        new Luz { nombre = "LUZ_Advertencia_MII_Exterior", posicion = new Vector3(5.8430f, 2.4500f, 10.5551f), color = new Color(1.0000f, 0.7354f, 0.2478f), intensidad = 0.8000f, alcance = 2.2000f },
        new Luz { nombre = "LUZ_Advertencia_MSD_Exterior", posicion = new Vector3(-3.6430f, 2.4500f, -10.5551f), color = new Color(1.0000f, 0.7354f, 0.2478f), intensidad = 0.8000f, alcance = 2.2000f },
        new Luz { nombre = "LUZ_Advertencia_MSI_Exterior", posicion = new Vector3(7.8695f, 2.4500f, -9.3851f), color = new Color(1.0000f, 0.7354f, 0.2478f), intensidad = 0.8000f, alcance = 2.2000f },
        new Luz { nombre = "LUZ_Advertencia_NE_Emergencia", posicion = new Vector3(-5.9300f, 2.4500f, -13.9500f), color = new Color(1.0000f, 0.7354f, 0.2478f), intensidad = 0.8000f, alcance = 2.2000f },
        new Luz { nombre = "LUZ_Advertencia_N_Principal", posicion = new Vector3(2.9700f, 2.6500f, -13.9500f), color = new Color(1.0000f, 0.7354f, 0.2478f), intensidad = 0.8000f, alcance = 2.2000f },
        new Luz { nombre = "LUZ_Advertencia_S_Salida", posicion = new Vector3(-0.7700f, 2.6500f, 13.9500f), color = new Color(1.0000f, 0.7354f, 0.2478f), intensidad = 0.8000f, alcance = 2.2000f },
        new Luz { nombre = "LUZ_Advertencia_TEC_Norte", posicion = new Vector3(-9.7500f, 2.4500f, -11.9700f), color = new Color(1.0000f, 0.7354f, 0.2478f), intensidad = 0.8000f, alcance = 2.2000f },
        new Luz { nombre = "LUZ_Advertencia_TEC_Sur", posicion = new Vector3(-9.7500f, 2.4500f, 10.0300f), color = new Color(1.0000f, 0.7354f, 0.2478f), intensidad = 0.8000f, alcance = 2.2000f },
        new Luz { nombre = "LUZ_MC_Principal", posicion = new Vector3(1.1000f, 2.5500f, -0.0000f), color = new Color(0.8962f, 0.9547f, 1.0000f), intensidad = 2.6000f, alcance = 7.5000f },
        new Luz { nombre = "LUZ_MID_Principal", posicion = new Vector3(-3.1500f, 2.5500f, 7.3612f), color = new Color(0.8962f, 0.9547f, 1.0000f), intensidad = 2.0000f, alcance = 5.5000f },
        new Luz { nombre = "LUZ_MII_Principal", posicion = new Vector3(5.3500f, 2.5500f, 7.3612f), color = new Color(0.8962f, 0.9547f, 1.0000f), intensidad = 2.0000f, alcance = 5.5000f },
        new Luz { nombre = "LUZ_MLD_Principal", posicion = new Vector3(-7.4000f, 2.5500f, -0.0000f), color = new Color(0.8962f, 0.9547f, 1.0000f), intensidad = 2.0000f, alcance = 5.5000f },
        new Luz { nombre = "LUZ_MLI_Principal", posicion = new Vector3(9.6000f, 2.5500f, -0.0000f), color = new Color(0.8962f, 0.9547f, 1.0000f), intensidad = 2.0000f, alcance = 5.5000f },
        new Luz { nombre = "LUZ_MSD_Principal", posicion = new Vector3(-3.1500f, 2.5500f, -7.3612f), color = new Color(0.8962f, 0.9547f, 1.0000f), intensidad = 2.0000f, alcance = 5.5000f },
        new Luz { nombre = "LUZ_MSI_Principal", posicion = new Vector3(5.3500f, 2.5500f, -7.3612f), color = new Color(0.8962f, 0.9547f, 1.0000f), intensidad = 2.0000f, alcance = 5.5000f },
        new Luz { nombre = "LUZ_PAS_MID", posicion = new Vector3(-1.2850f, 2.6000f, 4.1309f), color = new Color(0.8962f, 0.9547f, 1.0000f), intensidad = 1.2000f, alcance = 3.5000f },
        new Luz { nombre = "LUZ_PAS_MII", posicion = new Vector3(3.4850f, 2.6000f, 4.1309f), color = new Color(0.8962f, 0.9547f, 1.0000f), intensidad = 1.2000f, alcance = 3.5000f },
        new Luz { nombre = "LUZ_PAS_MLD", posicion = new Vector3(-3.6700f, 2.6000f, -0.0000f), color = new Color(0.8962f, 0.9547f, 1.0000f), intensidad = 1.2000f, alcance = 3.5000f },
        new Luz { nombre = "LUZ_PAS_MLI", posicion = new Vector3(5.8700f, 2.6000f, -0.0000f), color = new Color(0.8962f, 0.9547f, 1.0000f), intensidad = 1.2000f, alcance = 3.5000f },
        new Luz { nombre = "LUZ_PAS_MSD", posicion = new Vector3(-1.2850f, 2.6000f, -4.1309f), color = new Color(0.8962f, 0.9547f, 1.0000f), intensidad = 1.2000f, alcance = 3.5000f },
        new Luz { nombre = "LUZ_PAS_MSI", posicion = new Vector3(3.4850f, 2.6000f, -4.1309f), color = new Color(0.8962f, 0.9547f, 1.0000f), intensidad = 1.2000f, alcance = 3.5000f },
        new Luz { nombre = "LUZ_TEC_01", posicion = new Vector3(-11.8000f, 2.3000f, 12.0000f), color = new Color(1.0000f, 0.9640f, 0.9063f), intensidad = 1.3000f, alcance = 4.5000f },
        new Luz { nombre = "LUZ_TEC_02", posicion = new Vector3(-11.8000f, 2.3000f, 6.0000f), color = new Color(1.0000f, 0.9640f, 0.9063f), intensidad = 1.3000f, alcance = 4.5000f },
        new Luz { nombre = "LUZ_TEC_03", posicion = new Vector3(-11.8000f, 2.3000f, -0.0000f), color = new Color(1.0000f, 0.9640f, 0.9063f), intensidad = 1.3000f, alcance = 4.5000f },
        new Luz { nombre = "LUZ_TEC_04", posicion = new Vector3(-11.8000f, 2.3000f, -6.0000f), color = new Color(1.0000f, 0.9640f, 0.9063f), intensidad = 1.3000f, alcance = 4.5000f },
        new Luz { nombre = "LUZ_TEC_05", posicion = new Vector3(-11.8000f, 2.3000f, -12.0000f), color = new Color(1.0000f, 0.9640f, 0.9063f), intensidad = 1.3000f, alcance = 4.5000f },
        new Luz { nombre = "LUZ_Vestibulo_01", posicion = new Vector3(8.5000f, 2.7500f, -12.6000f), color = new Color(0.9309f, 0.9686f, 1.0000f), intensidad = 1.6000f, alcance = 6.5000f },
        new Luz { nombre = "LUZ_Vestibulo_02", posicion = new Vector3(1.1000f, 2.7500f, -12.6000f), color = new Color(0.9309f, 0.9686f, 1.0000f), intensidad = 1.6000f, alcance = 6.5000f },
        new Luz { nombre = "LUZ_Vestibulo_03", posicion = new Vector3(-5.5000f, 2.7500f, -12.6000f), color = new Color(0.9309f, 0.9686f, 1.0000f), intensidad = 1.6000f, alcance = 6.5000f },
        new Luz { nombre = "LUZ_Vestibulo_04", posicion = new Vector3(8.5000f, 2.7500f, 12.6000f), color = new Color(0.9309f, 0.9686f, 1.0000f), intensidad = 1.6000f, alcance = 6.5000f },
        new Luz { nombre = "LUZ_Vestibulo_05", posicion = new Vector3(1.1000f, 2.7500f, 12.6000f), color = new Color(0.9309f, 0.9686f, 1.0000f), intensidad = 1.6000f, alcance = 6.5000f },
        new Luz { nombre = "LUZ_Vestibulo_06", posicion = new Vector3(-5.5000f, 2.7500f, 12.6000f), color = new Color(0.9309f, 0.9686f, 1.0000f), intensidad = 1.6000f, alcance = 6.5000f },
        new Luz { nombre = "LUZ_Vestibulo_07", posicion = new Vector3(10.6000f, 2.7500f, -7.5000f), color = new Color(0.9309f, 0.9686f, 1.0000f), intensidad = 1.6000f, alcance = 6.5000f },
        new Luz { nombre = "LUZ_Vestibulo_08", posicion = new Vector3(10.6000f, 2.7500f, 7.5000f), color = new Color(0.9309f, 0.9686f, 1.0000f), intensidad = 1.6000f, alcance = 6.5000f },
        new Luz { nombre = "LUZ_Vestibulo_09", posicion = new Vector3(-8.6000f, 2.7500f, -9.5000f), color = new Color(0.9309f, 0.9686f, 1.0000f), intensidad = 1.6000f, alcance = 6.5000f },
        new Luz { nombre = "LUZ_Vestibulo_10", posicion = new Vector3(-8.6000f, 2.7500f, 9.5000f), color = new Color(0.9309f, 0.9686f, 1.0000f), intensidad = 1.6000f, alcance = 6.5000f },
    };

    // Recorrido de la auditoría (A* sobre los colliders re-importados, radio 0.4 m).
    public static readonly Vector3 InicioRecorrido = new Vector3(1.1250f, 0.0000f, -14.5750f);
    public static readonly Punto[] Recorrido = {
        new Punto { tramo = "Acceso principal (umbral) -> Núcleo central", objetivo = "", posicion = new Vector3(6.6250f, 0.0000f, -10.6750f) },
        new Punto { tramo = "Acceso principal (umbral) -> Núcleo central", objetivo = "", posicion = new Vector3(6.6750f, 0.0000f, -10.3750f) },
        new Punto { tramo = "Acceso principal (umbral) -> Núcleo central", objetivo = "", posicion = new Vector3(6.2750f, 0.0000f, -9.6250f) },
        new Punto { tramo = "Acceso principal (umbral) -> Núcleo central", objetivo = "", posicion = new Vector3(2.3250f, 0.0000f, -3.2250f) },
        new Punto { tramo = "Acceso principal (umbral) -> Núcleo central", objetivo = "Núcleo central", posicion = new Vector3(1.1250f, 0.0000f, -2.6250f) },
        new Punto { tramo = "Núcleo central -> M-01 Superior izq.", objetivo = "", posicion = new Vector3(2.3750f, 0.0000f, -3.2750f) },
        new Punto { tramo = "Núcleo central -> M-01 Superior izq.", objetivo = "M-01 Superior izq.", posicion = new Vector3(5.3250f, 0.0000f, -7.3750f) },
        new Punto { tramo = "M-01 Superior izq. -> Núcleo central", objetivo = "", posicion = new Vector3(2.2250f, 0.0000f, -3.1250f) },
        new Punto { tramo = "M-01 Superior izq. -> Núcleo central", objetivo = "Núcleo central", posicion = new Vector3(1.1250f, 0.0000f, -2.6250f) },
        new Punto { tramo = "Núcleo central -> M-02 Superior der.", objetivo = "", posicion = new Vector3(-0.1750f, 0.0000f, -3.2750f) },
        new Punto { tramo = "Núcleo central -> M-02 Superior der.", objetivo = "M-02 Superior der.", posicion = new Vector3(-3.1250f, 0.0000f, -7.3750f) },
        new Punto { tramo = "M-02 Superior der. -> Núcleo central", objetivo = "", posicion = new Vector3(-0.0250f, 0.0000f, -3.1250f) },
        new Punto { tramo = "M-02 Superior der. -> Núcleo central", objetivo = "Núcleo central", posicion = new Vector3(1.1250f, 0.0000f, -2.6250f) },
        new Punto { tramo = "Núcleo central -> M-03 Lateral izq.", objetivo = "", posicion = new Vector3(4.5250f, 0.0000f, -0.5750f) },
        new Punto { tramo = "Núcleo central -> M-03 Lateral izq.", objetivo = "M-03 Lateral izq.", posicion = new Vector3(9.5750f, 0.0000f, -0.0250f) },
        new Punto { tramo = "M-03 Lateral izq. -> Núcleo central", objetivo = "", posicion = new Vector3(4.4750f, 0.0000f, -0.5750f) },
        new Punto { tramo = "M-03 Lateral izq. -> Núcleo central", objetivo = "Núcleo central", posicion = new Vector3(1.1250f, 0.0000f, -2.6250f) },
        new Punto { tramo = "Núcleo central -> M-04 Lateral der.", objetivo = "", posicion = new Vector3(-2.3250f, 0.0000f, -0.5750f) },
        new Punto { tramo = "Núcleo central -> M-04 Lateral der.", objetivo = "M-04 Lateral der.", posicion = new Vector3(-7.4250f, 0.0000f, -0.0250f) },
        new Punto { tramo = "M-04 Lateral der. -> Núcleo central", objetivo = "", posicion = new Vector3(-2.2750f, 0.0000f, -0.5750f) },
        new Punto { tramo = "M-04 Lateral der. -> Núcleo central", objetivo = "Núcleo central", posicion = new Vector3(1.1250f, 0.0000f, -2.6250f) },
        new Punto { tramo = "Núcleo central -> M-05 Inferior izq.", objetivo = "", posicion = new Vector3(2.5750f, 0.0000f, -0.7750f) },
        new Punto { tramo = "Núcleo central -> M-05 Inferior izq.", objetivo = "", posicion = new Vector3(3.3750f, 0.0000f, 2.9750f) },
        new Punto { tramo = "Núcleo central -> M-05 Inferior izq.", objetivo = "M-05 Inferior izq.", posicion = new Vector3(5.3750f, 0.0000f, 7.3750f) },
        new Punto { tramo = "M-05 Inferior izq. -> Núcleo central", objetivo = "", posicion = new Vector3(3.3250f, 0.0000f, 2.7750f) },
        new Punto { tramo = "M-05 Inferior izq. -> Núcleo central", objetivo = "", posicion = new Vector3(2.6250f, 0.0000f, -0.6250f) },
        new Punto { tramo = "M-05 Inferior izq. -> Núcleo central", objetivo = "", posicion = new Vector3(2.4750f, 0.0000f, -0.9250f) },
        new Punto { tramo = "M-05 Inferior izq. -> Núcleo central", objetivo = "Núcleo central", posicion = new Vector3(1.1250f, 0.0000f, -2.6250f) },
        new Punto { tramo = "Núcleo central -> M-06 Inferior der.", objetivo = "", posicion = new Vector3(-0.3750f, 0.0000f, -0.7750f) },
        new Punto { tramo = "Núcleo central -> M-06 Inferior der.", objetivo = "", posicion = new Vector3(-1.2250f, 0.0000f, 3.2250f) },
        new Punto { tramo = "Núcleo central -> M-06 Inferior der.", objetivo = "M-06 Inferior der.", posicion = new Vector3(-3.1250f, 0.0000f, 7.3750f) },
        new Punto { tramo = "M-06 Inferior der. -> Núcleo central", objetivo = "", posicion = new Vector3(-1.1250f, 0.0000f, 2.7750f) },
        new Punto { tramo = "M-06 Inferior der. -> Núcleo central", objetivo = "", posicion = new Vector3(-0.4250f, 0.0000f, -0.6250f) },
        new Punto { tramo = "M-06 Inferior der. -> Núcleo central", objetivo = "", posicion = new Vector3(-0.2250f, 0.0000f, -0.9750f) },
        new Punto { tramo = "M-06 Inferior der. -> Núcleo central", objetivo = "Núcleo central", posicion = new Vector3(1.1250f, 0.0000f, -2.6250f) },
        new Punto { tramo = "Núcleo central -> Zona técnica", objetivo = "", posicion = new Vector3(-0.1750f, 0.0000f, -3.2750f) },
        new Punto { tramo = "Núcleo central -> Zona técnica", objetivo = "", posicion = new Vector3(-5.1750f, 0.0000f, -10.0750f) },
        new Punto { tramo = "Núcleo central -> Zona técnica", objetivo = "", posicion = new Vector3(-9.9750f, 0.0000f, -10.8250f) },
        new Punto { tramo = "Núcleo central -> Zona técnica", objetivo = "", posicion = new Vector3(-10.9750f, 0.0000f, -10.8250f) },
        new Punto { tramo = "Núcleo central -> Zona técnica", objetivo = "", posicion = new Vector3(-11.2250f, 0.0000f, -10.5750f) },
        new Punto { tramo = "Núcleo central -> Zona técnica", objetivo = "Zona técnica", posicion = new Vector3(-11.7750f, 0.0000f, -4.0250f) },
        new Punto { tramo = "Zona técnica -> Salida sur (umbral)", objetivo = "", posicion = new Vector3(-11.2250f, 0.0000f, 10.5750f) },
        new Punto { tramo = "Zona técnica -> Salida sur (umbral)", objetivo = "", posicion = new Vector3(-10.9750f, 0.0000f, 10.8250f) },
        new Punto { tramo = "Zona técnica -> Salida sur (umbral)", objetivo = "", posicion = new Vector3(-1.8250f, 0.0000f, 12.9250f) },
        new Punto { tramo = "Zona técnica -> Salida sur (umbral)", objetivo = "", posicion = new Vector3(-0.0750f, 0.0000f, 13.8750f) },
        new Punto { tramo = "Zona técnica -> Salida sur (umbral)", objetivo = "Salida sur (umbral)", posicion = new Vector3(1.1250f, 0.0000f, 14.5750f) },
    };

    // Puertas en muro curvo: se abren girando alrededor del centro de su sala (verificado en Blender: la hoja
    // queda un 95 % oculta en el muro; con deslizamiento recto quedaría un 91 % fuera, sobresaliendo al vestíbulo).
    public struct Giro { public string puertaId; public Vector3 centro; }
    public static readonly Giro[] PuertasGiratorias = {
        new Giro { puertaId = "MID_Exterior", centro = new Vector3(-3.1500f, 0.0000f, 7.3610f) },
        new Giro { puertaId = "MII_Exterior", centro = new Vector3(5.3500f, 0.0000f, 7.3610f) },
        new Giro { puertaId = "MSD_Exterior", centro = new Vector3(-3.1500f, 0.0000f, -7.3610f) },
        new Giro { puertaId = "MSI_Exterior", centro = new Vector3(5.3500f, 0.0000f, -7.3610f) },
    };

    // Centro del núcleo en Unity (para comprobar la orientación de ejes al importar).
    public static readonly Vector3 CentroNucleoEsperado = new Vector3(1.1f, 0f, 0f);
}
