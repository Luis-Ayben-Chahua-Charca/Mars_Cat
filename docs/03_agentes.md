# 03 — Registro de agentes

Las reglas y comportamientos de los agentes están en `/AGENTS.md` y en la carpeta `/agents/`. Este documento solo **registra qué instancias existen** y quién responde por cada una.

Al crear una instancia nueva, agregarla aquí **antes** de que escriba en el repo.

| Instancia | Herramienta | Perfil | Humano responsable | Alcance de escritura |
|---|---|---|---|---|
| Gestión | Claude | `agents/gestion.md` | Leyni | `AGENTS.md`, `STRUCTURE.md`, `agents/`, `docs/`, `README.md`, `BITACORA.md`, `.gitignore`, `.gitattributes` |
| Design Master | Claude | `agents/design-master.md` | Leyni | `docs/escenas/` (solo si Leyni lo pide) |
| `____` | Claude Code / Codex | `agents/agent_codigo.md` — Lógica | `____` | Sus archivos en `MarsCat/Assets/Scripts/Logic/` |
| `____` | Claude Code / Codex | `agents/agent_codigo.md` — Lógica | `____` | Sus archivos en `MarsCat/Assets/Scripts/Logic/` |
| `____` | Claude Code / Codex | `agents/agent_codigo.md` — Unity | `____` | Sus archivos en `MarsCat/Assets/Scripts/Unity/` |
| `____` | Claude Code / Codex | `agents/agent_codigo.md` — Unity | `____` | Sus archivos en `MarsCat/Assets/Scripts/Unity/` |

## Coordinación entre instancias

- Las instancias no se hablan directamente: se coordinan a través de **los issues** y del repositorio.
- El avance queda en el commit `#n` y en los checks de la tarjeta. Un comentario corto solo si hay pasos en el Editor o pendientes.
- Las decisiones que afectan a varios roles (contratos Lógica↔Unity, formatos de datos, carpetas) se escriben en el issue o en `docs/` antes de implementarlas.
