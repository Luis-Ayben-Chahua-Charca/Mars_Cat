# 01 — Roles y personas

## Personas

Completar con nombre y usuario de GitHub (se usa para asignar issues).

| Persona | Usuario GitHub | Rol |
|---|---|---|
| Leyni | `____` | Gestión de hitos y repositorio + armado de escenas (arte + código), con apoyo del agente Design Master |
| `____` | `____` | Arte 3D |
| `____` | `____` | Arte 3D |
| `____` | `____` | Unity |
| `____` | `____` | Unity |
| `____` | `____` | Lógica C# |
| `____` | `____` | Lógica C# |
| `____` | `____` | Integración |
| `____` | `____` | Integración |

## Roles y propiedad

Cada rol es **dueño** de una carpeta. Nadie edita archivos de una carpeta ajena; si hace falta un cambio, se pide en la tarjeta del tablero.

| Rol | Hace | Es dueño de | No toca |
|---|---|---|---|
| **Gestión + Escenas** (Leyni) | Hitos, tablero, repo, docs, cierre manual de hitos, tags; arma las escenas principales con el arte y el código | `docs/`, `agents/`, `README.md`, `STRUCTURE.md`, `BITACORA.md`, `AGENTS.md`, config del repo, escenas principales | Contenido de scripts y modelos ajenos |
| **Arte 3D** (2) | Modelos, texturas, exportes `.fbx` | `Art_Source/`, `MarsCat/Assets/Art/` (solo los archivos fuente: `.fbx`, `.png`) | `.meta`, escenas, prefabs, scripts |
| **Unity** (2) | Comportamientos en Unity: `MonoBehaviour`, UI, cámara, input; conecta la lógica pura con el motor | `MarsCat/Assets/Scripts/Unity/`, `MarsCat/Assets/UI/` | Escenas principales, lógica pura |
| **Lógica C#** (2) | Sistemas del juego en C# puro: recursos, economía, eventos, lectura de datos NASA | `MarsCat/Assets/Scripts/Logic/`, `MarsCat/Assets/Data/` | Todo lo que necesite abrir Unity; `.meta` |
| **Integración** (2) | Importan assets, generan y suben los `.meta`, arman prefabs, materiales y configuraciones, ajustan import settings | `MarsCat/Assets/Prefabs/`, `MarsCat/Assets/Config/`, `MarsCat/Assets/Art/Materials/`, **todos los `.meta`**, `ProjectSettings/`, escenas secundarias que se les asignen | Escenas principales (de Leyni), contenido de scripts y modelos ajenos |

## Reparto de escenas

Cada escena tiene un solo dueño. Leyni arma las escenas principales; los integradores preparan los assets, `.meta` y prefabs que ella usa.

| Escena | Dueño |
|---|---|
| `Main.unity` | Leyni |
| `MainMenu.unity` | `____` |
| `Sandbox_<Nombre>.unity` | Cada quien la suya, para pruebas (no se integra) |

## Contrato entre Lógica y Unity

Antes de programar un sistema, Lógica y Unity acuerdan en la tarjeta del issue:

- Nombre de la clase y namespace (`MarsCat.Logic.<Sistema>`).
- Métodos y eventos públicos que Unity va a llamar o escuchar.
- Qué datos recibe (por ejemplo, un `ScriptableObject` de configuración o un `.json`).

Con el contrato escrito, ambos avanzan en paralelo sin esperar al otro.
