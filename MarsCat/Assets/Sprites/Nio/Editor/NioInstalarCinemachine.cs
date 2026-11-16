// Asistencia de IA: escrito con Claude Code (Anthropic).
using System;
using System.IO;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

/// <summary>
/// Tools > Nio > Instalar Cinemachine: añade com.unity.cinemachine con el Package Manager
/// (versión recomendada para este editor). Antes copia Packages/manifest.json y packages-lock.json en Backups/.
/// Para deshacerlo: Window > Package Manager > Cinemachine > Remove, o restaurar el manifest de la copia.
/// </summary>
public static class NioInstalarCinemachine
{
    static AddRequest peticion;
    static string log;

    [MenuItem("Tools/Nio/Instalar Cinemachine", false, 30)]
    public static void Instalar()
    {
        if (peticion != null && !peticion.IsCompleted) { Debug.Log("[Nio] La instalación de Cinemachine ya está en curso."); return; }
        string raiz = Directory.GetParent(Application.dataPath).FullName;
        string dir = Path.Combine(raiz, "Backups", "NioPlayer_Paquetes_" + DateTime.Now.ToString("yyyyMMdd_HHmmss"));
        Directory.CreateDirectory(dir);
        foreach (string f in new[] { "manifest.json", "packages-lock.json" })
        {
            string src = Path.Combine(raiz, "Packages", f);
            if (File.Exists(src)) File.Copy(src, Path.Combine(dir, f), false);
        }
        log = Path.Combine(raiz, "Logs", "NioPlayer_InstalarCinemachine.txt");
        File.WriteAllText(log, "Instalación de Cinemachine — " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\nCopia del manifiesto: " + dir + "\n");
        peticion = Client.Add("com.unity.cinemachine");
        EditorApplication.update += Progreso;
        Debug.Log("[Nio] Instalando com.unity.cinemachine… (copia del manifiesto en " + dir + ")");
    }

    static void Progreso()
    {
        if (peticion == null || !peticion.IsCompleted) return;
        EditorApplication.update -= Progreso;
        string texto = peticion.Status == StatusCode.Success
            ? "OK: " + peticion.Result.name + " " + peticion.Result.version + " (" + peticion.Result.source + ")"
            : "ERROR: " + (peticion.Error != null ? peticion.Error.message : "desconocido");
        File.AppendAllText(log, texto + "\n");
        if (peticion.Status == StatusCode.Success) Debug.Log("[Nio] Cinemachine instalado: " + peticion.Result.version);
        else Debug.LogError("[Nio] No se pudo instalar Cinemachine: " + texto);
        peticion = null;
    }
}
