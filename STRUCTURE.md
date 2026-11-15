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
├── .claude/               Configuración compartida de Claude Code (Gestión)
│   └── skills/work-issue/ Skill /work-issue: flujo por issue y propuesta de commit
├── agents/                Perfiles de agentes (Gestión)
├── docs/                  Documentación del equipo (Gestión)
│   ├── 00–05_*.md         Proceso: stack, roles, reglas, agentes, hitos, plan de implementación
│   ├── diseno/            Diseño del juego (Gestión; contenido acordado en issues)
│   ├── Propuesta_mecanica/ Investigación y propuestas por mecánica, insumo para diseno/ (cualquier integrante)
│   └── escenas/           Notas de armado de escenas (Design Master, solo si Leyni lo pide)
├── Art_Source/            Archivos fuente de arte, fuera de Unity (Arte)
│   ├── Blender/           .blend
│   └── OpenToonz/         Proyectos .tnz y fuentes
└── MarsCat/               Proyecto Unity 6000.6.7f1
    ├── Assets/            (ver abajo)
    ├── Packages/          Integración — no tocar sin pedido
    └── ProjectSettings/   Integración — no tocar sin pedido
```

## `docs/diseno/`

Los mantiene Gestión. Índice y estado de cada documento en `docs/diseno/README.md`. Los números de balance no se copian aquí: viven en `MarsCat/Assets/Data/`.

| Archivo | Contenido | Lo consume |
|---|---|---|
| `README.md` | Índice, reglas y estado de cada documento | Todos |
| `01_vision.md` | Pitch, público, pilares, qué aprende el jugador, alcance de la demo | Todos |
| `02_core_loop.md` | Etapas, zonas de la base, tutorial, alcance del PMV | Lógica, Integración Unity |
| `03_sistemas.md` | Recursos, edificios, trajes, sus reglas y namespaces `MarsCat.Logic.*` | Lógica, Integración Unity |
| `04_economia_y_balance.md` | Reglas de economía y el porqué de cada valor de `Data/` | Lógica |
| `05_datos_nasa.md` | Registro de datasets NASA y su uso | Lógica |
| `06_eventos_y_ritmo.md` | Catálogo de eventos y curva de presión | Lógica, Integración Unity |
| `07_lore.md` | Premisa, personajes, textos del juego | Arte, Integración Unity |
| `08_direccion_arte.md` | Paleta, escala, referencias y lista de assets | Arte, Integración |

## `docs/Propuesta_mecanica/`

Investigación y propuestas de mecánicas que hace cualquier integrante. No son decisiones: Gestión las adapta a `docs/diseno/`, donde lo ya acordado prevalece ante contradicciones.

| Archivo | Contenido |
|---|---|
| `mecanica_<nombre>.md` | Una mecánica por archivo (por ejemplo, `mecanica_energia.md`) |

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
| `Scripts/Unity/` | MonoBehaviours, namespace `MarsCat.Unity.*` | Integración Unity |
| `Scripts/Unity/UI/` | Scripts de interfaz | Integración Unity |
| `Config/` | ScriptableObjects `.asset` de configuración | Integración |
| `Prefabs/` | `.prefab` | Integración |
| `Scenes/` | Escenas del juego (`Main.unity`, `MainMenu.unity`) | Integración Unity (una escena, un dueño) |
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
| Documentos | `NN_snake_case.md` | `03_sistemas.md` |

Sin espacios, tildes ni ñ en nombres de archivos.
