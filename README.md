# Mars Cat — NASA Space Apps Challenge 2026

Juego de gestión de una colonia en Marte, construido con datos reales de NASA.

## Estructura del repositorio

```
Mars_Cat/
├── README.md            ← este archivo
├── STRUCTURE.md         ← dónde va cada cosa, dueños y convenciones de nombres
├── CONTRIBUTING.md      ← reglas para hacer push y formato del commit
├── AGENTS.md            ← comportamiento principal de agentes de IA (Codex lo lee solo)
├── CLAUDE.md            ← importa AGENTS.md para Claude Code
├── BITACORA.md          ← registro mínimo: una línea por hito cerrado
├── .gitignore / .gitattributes
├── agents/              ← perfiles: código, design master, gestión, herramientas
├── docs/
│   ├── 00_stack.md            ← herramientas y versiones
│   ├── 01_roles_y_personas.md ← quién hace qué
│   ├── 02_reglas_de_trabajo.md← Git, commits, .meta, escenas, tablero
│   ├── 03_agentes.md          ← registro de instancias de IA
│   ├── 04_hitos.md            ← H0–H4 y criterios de cierre
│   └── diseno/                ← diseño del juego: visión, loop, sistemas, economía, lore…
├── Art_Source/          ← .blend y proyectos de OpenToonz
└── MarsCat/             ← proyecto Unity (se crea con Unity Hub)
```

Detalle completo en [`STRUCTURE.md`](STRUCTURE.md).

## Si usas Claude Code o Codex

Abre la herramienta en la raíz del repo e indícale tu perfil en el primer mensaje (plantilla en [`agents/README.md`](agents/README.md)).

## Inicio rápido (cada integrante, una sola vez)

1. Instalar **Git** y **Git LFS**, luego: `git lfs install`
2. Clonar el repo.
3. Configurar pull sin rebase: `git config pull.rebase false`
4. Instalar **Unity 6000.6.7f1** desde Unity Hub (misma versión exacta para todos).
5. Leer `docs/02_reglas_de_trabajo.md` (5 minutos, evita horas de conflictos).

## Flujo diario

```
terminar algo completo que compile
git add <tu carpeta>
git commit -m "[area] qué hice #issue"
git pull
git push
```

Ver detalles en [`docs/02_reglas_de_trabajo.md`](docs/02_reglas_de_trabajo.md).
