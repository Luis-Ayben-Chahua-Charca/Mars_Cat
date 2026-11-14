# AGENTS.md — Comportamiento principal de los agentes

Este archivo manda sobre cualquier otro perfil de agente. Lo leen automáticamente **Codex** (AGENTS.md) y **Claude Code** (vía `CLAUDE.md`). Si eres otra herramienta o instancia, léelo igual antes de hacer nada.

## Proyecto

**Mars Cat** — juego de gestión de una colonia en Marte para el NASA Space Apps Challenge 2026. Debe usar datos reales de NASA.

- Motor: **Unity 6000.6.7f1** (proyecto en `MarsCat/`)
- Código: C#
- Arte: Blender (3D) y OpenToonz (2D/animación)
- Repo: GitHub, todo en `main`, sin CI

## Orden de lectura al iniciar una sesión

1. Este archivo.
2. `STRUCTURE.md` — dónde va cada cosa y quién es dueño.
3. **Tu perfil** en `agents/` (ver tabla abajo). Si el humano no te dijo cuál, **pregúntale antes de escribir nada**.
4. `docs/02_reglas_de_trabajo.md` — Git, commits, `.meta`.
5. El issue en el que vas a trabajar (el humano te da el número).

## Perfiles

| Perfil | Archivo | Quién lo usa | Escribe código |
|---|---|---|---|
| Código — Lógica | `agents/agent_codigo.md` (sección Lógica) | Integrantes de Lógica C# | Sí, C# puro |
| Código — Unity | `agents/agent_codigo.md` (sección Unity) | Integrantes de Unity | Sí, MonoBehaviours / UI |
| Design Master | `agents/design-master.md` | Leyni (armado de escenas) | **No** |
| Gestión | `agents/gestion.md` | Leyni (hitos, repo, docs) | **No** (solo docs y config del repo) |

Notas específicas de Claude Code y Codex: `agents/herramientas.md`.

## Reglas universales (todos los perfiles)

1. **Alcance:** solo escribes en las carpetas que tu perfil y el humano te permiten. Si algo necesario cae fuera, lo propones; no lo haces.
2. **Propiedad:** un script, un dueño; una escena, un dueño. Nunca edites archivos de otra persona.
3. **`.meta`:** nunca crear, borrar, renombrar ni regenerar archivos `.meta`.
4. **Unity YAML:** nunca editar a mano `.unity`, `.prefab`, `.asset`, `.mat`, `.controller` ni nada en `ProjectSettings/`.
5. **Git:**
   - Todo en `main`. Sin ramas, sin rebase, sin `--force`, sin `reset --hard`, sin reescribir historia.
   - **El agente no hace `git push`.** Puede preparar el commit si el humano lo pide; el humano revisa y hace push.
   - Mensaje de commit: `[area] descripción corta #issue`.
   - **Nunca** escribir `closes`, `fixes`, `resolves` (ni variantes) seguido de `#n`. Solo `#n` o `refs #n`.
   - Nunca `git add .` ni `git add -A`: solo los archivos de tu tarea.
6. **No ejecutar Unity** (ni en batch mode) ni instalar paquetes en el proyecto sin pedido explícito.
7. **No inventar datos de NASA.** Si un dato no está en `MarsCat/Assets/Data/` o en el issue, se pregunta.
8. **No cambiar reglas ni docs.** `AGENTS.md`, `STRUCTURE.md`, `agents/` y `docs/` solo los modifica el perfil Gestión con aprobación de Leyni.
9. **Ante la duda, pregunta.** Es más barato que deshacer un error en `main`.
10. **No generes documentación.** El tablero es la documentación: el avance se registra con el commit `#n` y los checks de la tarjeta. Al terminar, solo si hay algo que configurar en el Editor o algo pendiente, dale al humano un comentario de 2–4 líneas para la tarjeta.

## Idioma

Responde y documenta en español. Identificadores de código en inglés (clases, métodos, variables); comentarios en español.
