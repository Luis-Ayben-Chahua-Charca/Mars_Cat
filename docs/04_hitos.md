# 04 — Hitos

Los hitos se definen por **lo que debe ser jugable** y se cierran **manualmente**, después de probar la build. Al cerrar un hito: tag + una línea en `BITACORA.md`.

| Hito | Criterio de cierre | Tag |
|---|---|---|
| **H0 — Base** | Documentación de stack, roles, reglas y agentes en el repo; proyecto Unity 6000.6.7f1 creado y configurado (Visible Meta Files, Force Text); todos clonaron, abrieron el proyecto e hicieron un commit de prueba | `h0-base` |
| **H1 — Tutorial jugable** | Se puede jugar el tutorial de principio a fin (ver `docs/diseno/02_core_loop.md`), aunque sea con cubos | `h1-jugable` |
| **H2 — Datos NASA** | Al menos un sistema del juego usa datos reales de NASA (requisito del concurso) | `h2-datos` |
| **H3 — Contenido y arte** | Modelos y animaciones reemplazan los placeholders | `h3-contenido` |
| **H4 — Demo congelada** | Build estable, video y presentación. Desde aquí solo se corrigen bugs | `h4-demo` |

## H0 — Tareas

| # | Tarea | Rol | Prioridad |
|---|---|---|---|
| 1 | Subir documentación base (README, STRUCTURE, CONTRIBUTING, AGENTS, agents/, docs/, BITACORA) | Gestión | P0 |
| 2 | Subir `.gitignore` y `.gitattributes` | Gestión | P0 |
| 3 | Terminar el tablero: vista Board agrupada por Estado y automatizaciones desactivadas (etiquetas, milestones e issues se crean con un script local) | Gestión | P0 |
| 4 | Completar la tabla de personas y usuarios de GitHub | Gestión | P0 |
| 5 | Todos: instalar Git LFS y Unity 6000.6.7f1, `git config pull.rebase false` | Todos | P0 |
| 6 | Crear el proyecto Unity en `MarsCat/` y configurar Visible Meta Files + Force Text | Integración | P0 |
| 7 | Crear estructura de carpetas en `Assets/` (ver abajo) con sus `.meta` | Integración | P0 |
| 8 | Crear `Main.unity` y una `Sandbox_<Nombre>.unity` por persona | Integración | P1 |
| 9 | Todos: clonar, abrir el proyecto y hacer su primer commit con `#n` (Gestión marca el check de cada uno) | Todos | P0 |
| 10 | Completar versiones de Blender y OpenToonz en `00_stack.md` | Arte | P1 |
| 11 | Registrar las instancias de IA en `03_agentes.md` | Gestión | P1 |
| 12 | Configurar UnityYAMLMerge (opcional) | Integración | P2 |
| 13 | Decidir core loop, recursos y condiciones de victoria/derrota (se anotan en `docs/diseno/02_core_loop.md`) | Todos (decisión de equipo) | P0 — bloquea H1 |

## Estructura de carpetas en `Assets/`

La tarea 7 crea las carpetas definidas en [`STRUCTURE.md`](../STRUCTURE.md).
