// Se ejecuta al importar/reimportar cualquier FBX cuyo nombre contenga "cabina".
//  - Ajustes del importador: escala 1, unidades del archivo (metros), sin cámaras/luces/animación, normales importadas,
//    tangentes MikkTSpace, UV de lightmap solo en los modelos visuales, sin materiales en el FBX de colliders.
//  - CABINA_COLLIDERS.fbx: COL_* -> collider según CabinaColliderUtil y se eliminan MeshRenderer/MeshFilter.
//  - Modelos visuales: marca como estática toda la arquitectura salvo PUERTA_* e INT_* (objetos interactivos).
//  - Copia las propiedades personalizadas de Blender a un componente CabinaPropiedades.
using System.Linq;
using UnityEditor;
using UnityEngine;

public class CabinaImportPostprocessor : AssetPostprocessor
{
    bool EsCabina { get { return assetPath.ToLowerInvariant().Contains("cabina") && assetPath.ToLowerInvariant().EndsWith(".fbx"); } }
    bool EsColliders { get { return assetPath.ToLowerInvariant().Contains("collider"); } }

    void OnPreprocessModel()
    {
        if (!EsCabina) return;
        var mi = (ModelImporter)assetImporter;
        mi.globalScale = 1f;
        mi.useFileUnits = true;            // "Convert Units": el FBX está en metros
        mi.importCameras = false;
        mi.importLights = false;
        mi.importAnimation = false;
        mi.animationType = ModelImporterAnimationType.None;
        mi.importNormals = ModelImporterNormals.Import;
        mi.importTangents = ModelImporterTangents.CalculateMikk;
        mi.generateSecondaryUV = !EsColliders;
        if (EsColliders) mi.materialImportMode = ModelImporterMaterialImportMode.None;
    }

    void OnPostprocessModel(GameObject root)
    {
        if (!EsCabina) return;
        foreach (var t in root.GetComponentsInChildren<Transform>(true))
        {
            string n = t.name;
            if (EsColliders && n.StartsWith("COL_"))
            {
                var mf = t.GetComponent<MeshFilter>();
                var mr = t.GetComponent<MeshRenderer>();
                CabinaColliderUtil.Configurar(t.gameObject, mf != null ? mf.sharedMesh : null);
                if (mr != null) Object.DestroyImmediate(mr, true);
                if (mf != null) Object.DestroyImmediate(mf, true);
                bool dinamico = n.StartsWith("COL_PUERTA_") || n.StartsWith("COL_TRG_");
                if (!dinamico) GameObjectUtility.SetStaticEditorFlags(t.gameObject, StaticEditorFlags.BatchingStatic);
            }
            else if (!EsColliders && t != root.transform)
            {
                bool interactivo = n.StartsWith("PUERTA_") || n.StartsWith("INT_");
                if (!interactivo)
                    GameObjectUtility.SetStaticEditorFlags(t.gameObject,
                        StaticEditorFlags.ContributeGI | StaticEditorFlags.BatchingStatic |
                        StaticEditorFlags.OccluderStatic | StaticEditorFlags.OccludeeStatic |
                        StaticEditorFlags.ReflectionProbeStatic);
            }
        }
    }

    void OnPostprocessGameObjectWithUserProperties(GameObject go, string[] nombres, object[] valores)
    {
        if (!EsCabina) return;
        var p = go.GetComponent<CabinaPropiedades>();
        if (p == null) p = go.AddComponent<CabinaPropiedades>();   // no usar ?? con objetos de Unity
        p.claves = nombres;
        p.valores = valores.Select(v => v == null ? "" : System.Convert.ToString(v, System.Globalization.CultureInfo.InvariantCulture)).ToArray();
    }
}
