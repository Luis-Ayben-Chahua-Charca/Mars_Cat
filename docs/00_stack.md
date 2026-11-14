# 00 — Stack tecnológico

| Herramienta | Versión | Uso | Quién |
|---|---|---|---|
| Unity | **6000.6.7f1** (exacta, todos la misma) | Motor del juego, escenas, integración | Unity, Integradores |
| C# | La incluida en Unity 6 | Lógica del juego | Lógica, Unity |
| IDE | Visual Studio 2022 / Rider / VS Code + extensión Unity | Edición de scripts | Lógica, Unity |
| Blender | La que tenga el equipo de arte (anotar aquí: `____`) | Modelado 3D, exporte a `.fbx` | Arte 3D |
| OpenToonz | Anotar aquí: `____` | Animación 2D / secuencias de imágenes | Arte |
| Git + Git LFS | Últimas estables | Control de versiones | Todos |
| GitHub | — | Repo, issues, Projects (Kanban), milestones | Todos |
| Datos NASA | Ver informe de datos del proyecto | Fuente de datos del juego (requisito del concurso) | Lógica, Integradores |

## Configuración obligatoria del proyecto Unity

Se hace **una sola vez**, por un integrador, al crear el proyecto (hito H0):

- Unity Hub → New project → Location: `D:\dev\Mars_Cat` → Project name: `MarsCat`
  (queda en `Mars_Cat/MarsCat/`).
- **Edit → Project Settings → Editor**
  - Version Control → Mode: **Visible Meta Files**
  - Asset Serialization → Mode: **Force Text**
- Primer commit del proyecto: `[repo] proyecto Unity base #<issue>`.

## Formatos de intercambio

| De | A | Formato | Carpeta destino |
|---|---|---|---|
| Blender | Unity | `.fbx` (+ texturas `.png`) | `MarsCat/Assets/Art/Models/` |
| OpenToonz | Unity | Secuencia `.png` o sprite sheet | `MarsCat/Assets/Art/2D/` |
| Lógica C# | Unity | `.cs` (clases sin `MonoBehaviour`) | `MarsCat/Assets/Scripts/Logic/` |
| Datos NASA | Unity | `.json` / `.csv` | `MarsCat/Assets/Data/` |

Los `.blend` fuente van en `Art_Source/` en la raíz del repo (fuera de `Assets/`, para que Unity no intente importarlos).

## Merge inteligente de escenas (opcional, recomendado para integradores)

Agregar a la configuración global de Git (ajustar la ruta si Unity está en otro lugar):

```
git config --global merge.unityyamlmerge.name "Unity SmartMerge"
git config --global merge.unityyamlmerge.driver "'C:/Program Files/Unity/Hub/Editor/6000.6.7f1/Editor/Data/Tools/UnityYAMLMerge.exe' merge -p %O %B %A %A"
git config --global merge.unityyamlmerge.recursive binary
```

Si alguien no lo configura, Git usa el merge normal; no se rompe nada.
