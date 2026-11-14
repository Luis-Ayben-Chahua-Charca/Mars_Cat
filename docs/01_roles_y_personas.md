# 01 — Roles y personas

## Personas

Completar con nombre y usuario de GitHub (se usa para asignar issues).

| Persona | Usuario GitHub | Rol |
|---|---|---|
| Leyni | `____` | Integración Unity + Gestión (hitos, repo, tablero) |
| `____` | `____` | Integración Unity |
| `____` | `____` | Arte 3D |
| `____` | `____` | Arte 3D |
| `____` | `____` | Lógica C# |
| `____` | `____` | Lógica C# |

Equipo de **6 personas**: 2 de Arte, 2 de Lógica C# y 2 de Integración Unity.

## Roles y propiedad

Cada rol es **dueño** de una carpeta. Nadie edita archivos de una carpeta ajena; si hace falta un cambio, se pide en la tarjeta del tablero. Entre dos personas del mismo rol, el dueño de un archivo es quien tiene asignada la tarjeta que lo creó.

| Rol | Hace | Es dueño de | No toca |
|---|---|---|---|
| **Arte 3D** (2) | Modelos, texturas, exportes `.fbx`, animación en OpenToonz | `Art_Source/`, archivos fuente en `MarsCat/Assets/Art/` (`.fbx`, `.png`) | `.meta`, escenas, prefabs, scripts |
| **Lógica C#** (2) | Sistemas del juego en C# puro: recursos, economía, eventos, lectura de datos NASA | `MarsCat/Assets/Scripts/Logic/`, `MarsCat/Assets/Data/` | Todo lo que necesite abrir Unity; `.meta` |
| **Integración Unity** (2, incluye a Leyni) | MonoBehaviours, UI, cámara, input; importan assets, generan y suben los `.meta`, arman prefabs, materiales, configuraciones y escenas | `MarsCat/Assets/Scripts/Unity/`, `Prefabs/`, `Config/`, `Art/Materials/`, `Scenes/`, **todos los `.meta`**, `ProjectSettings/` | Contenido de `.blend`/`.fbx` y de scripts de Lógica |
| **Gestión** (Leyni) | Hitos, tablero, repo, cierre manual de tarjetas e hitos, tags | `docs/`, `agents/`, `README.md`, `STRUCTURE.md`, `CONTRIBUTING.md`, `AGENTS.md`, `BITACORA.md`, config del repo | — |

## Reparto de escenas

Cada escena tiene un solo dueño.

| Escena | Dueño |
|---|---|
| `Main.unity` | Leyni |
| `MainMenu.unity` | `____` (Integración Unity) |
| `Sandbox_<Nombre>.unity` | Cada quien la suya, para pruebas (no se integra) |

## Contrato entre Lógica e Integración Unity

Antes de programar un sistema, Lógica e Integración Unity acuerdan en la tarjeta del issue:

- Nombre de la clase y namespace (`MarsCat.Logic.<Sistema>`).
- Métodos y eventos públicos que Unity va a llamar o escuchar.
- Qué datos recibe (por ejemplo, un `ScriptableObject` de configuración o un `.json`).

Con el contrato escrito, ambos avanzan en paralelo sin esperar al otro.
