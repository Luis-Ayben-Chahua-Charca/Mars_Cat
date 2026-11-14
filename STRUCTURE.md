# STRUCTURE.md — Estructura del repositorio

Dónde va cada cosa, quién es su dueño y cómo se nombra. Si un archivo no encaja en ninguna carpeta, se pregunta en el issue antes de crear una nueva.

## Raíz del repositorio

```
Mars_Cat/
├── AGENTS.md              Comportamiento principal de agentes (Gestión)
├── CLAUDE.md              Importa AGENTS.md para Claude Code (Gestión)
├── STRUCTURE.md           Este archivo (Gestión)
├── README.md              Presentación e inicio rápido (Gestión)
├── CONTRIBUTING.md        Reglas de push y mensaje de commit (Gestión)
├── BITACORA.md            Registro de hitos cerrados (Gestión)
├── .gitignore             (Gestión) — ignora _tablero/, scripts locales de gestión
├── .gitattributes         LFS y merge de YAML (Gestión)
├── agents/                Perfiles de agentes (Gestión)
├── docs/                  Documentación mínima del equipo (Gestión)
├── Art_Source/            Archivos fuente de arte, fuera de Unity (Arte)
│   ├── Blender/           .blend
│   └── OpenToonz/         Proyectos .tnz y fuentes
└── MarsCat/               Proyecto Unity 6000.6.7f1
    ├── Assets/            (ver abajo)
    ├── Packages/          Integración — no tocar sin pedido
    └── ProjectSettings/   Integración — no tocar sin pedido
```

## `MarsCat/Assets/`

| Carpeta | Contenido | Dueño |
|---|---|---|
| `Art/Models/` | `.fbx` exportados de Blender | Arte 3D (archivo), Integración (`.meta`, import settings) |
| `Art/Textures/` | `.png` de texturas | Arte 3D (archivo), Integración (`.meta`) |
| `Art/2D/` | Secuencias y sprites de OpenToonz | Arte (archivo), Integración (`.meta`) |
| `Art/Materials/` | `.mat` | Integración |
| `Audio/` | `.wav`, `.ogg` | Quien se asigne en el issue |
| `Data/` | Datos NASA y de balance (`.json`, `.csv`) | Lógica |
| `Scripts/Logic/` | C# puro, namespace `MarsCat.Logic.*` | Lógica |
| `Scripts/Logic/Tests/` | Pruebas NUnit de la lógica | Lógica |
| `Scripts/Unity/` | MonoBehaviours, namespace `MarsCat.Unity.*` | Unity |
| `Scripts/Unity/UI/` | Scripts de interfaz | Unity |
| `Config/` | ScriptableObjects `.asset` de configuración | Integración |
| `Prefabs/` | `.prefab` | Integración |
| `Scenes/` | Escenas del juego (`Main.unity`, `MainMenu.unity`) | Leyni / Integración (una escena, un dueño) |
| `Scenes/Sandbox/` | `Sandbox_<Nombre>.unity` de pruebas personales | Cada persona la suya; no se integran |
| `UI/` | Sprites, fuentes y elementos visuales de interfaz | Arte / Integración |

**Todos los `.meta` son de Integración**, sin importar la carpeta.

## Convenciones de nombres

| Qué | Convención | Ejemplo |
|---|---|---|
| Clases y archivos `.cs` | PascalCase, archivo = clase | `OxygenSystem.cs` |
| Namespaces | `MarsCat.Logic.<Sistema>` o `MarsCat.Unity.<Área>` | `MarsCat.Logic.Resources` |
| Métodos y propiedades | PascalCase | `Consume()`, `CurrentAmount` |
| Campos privados | `_camelCase` | `_capacity` |
| Modelos | `SM_<Nombre>` (estático), `SK_<Nombre>` (con esqueleto) | `SM_Habitat.fbx` |
| Texturas | `T_<Nombre>_<Tipo>` (`D` color, `N` normal, `R` rugosidad) | `T_Habitat_D.png` |
| Materiales | `M_<Nombre>` | `M_Habitat.mat` |
| Prefabs | `P_<Nombre>` | `P_Habitat.prefab` |
| ScriptableObjects | `CFG_<Nombre>` | `CFG_Economy.asset` |
| Datos | `snake_case` | `mars_temperature.csv` |
| Escenas de prueba | `Sandbox_<Nombre>` | `Sandbox_Leyni.unity` |

Sin espacios, tildes ni ñ en nombres de archivos.
