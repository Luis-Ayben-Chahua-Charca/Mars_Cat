# agents/ — Perfiles de comportamiento

El comportamiento principal está en `/AGENTS.md`. Los archivos de esta carpeta **agregan** reglas por perfil; nunca contradicen ni relajan las de `AGENTS.md`.

| Archivo | Perfil |
|---|---|
| `agent_codigo.md` | Agentes que solo escriben código (Lógica y Unity) |
| `design-master.md` | Asistente de Leyni para armar escenas; no escribe código |
| `gestion.md` | Gestión de hitos, tablero, repo y documentación |
| `herramientas.md` | Notas para Claude Code y ChatGPT Codex |

## Cómo iniciar una sesión (cualquier herramienta)

Primer mensaje recomendado al agente:

```
Lee AGENTS.md y STRUCTURE.md.
Tu perfil: agents/agent_codigo.md, sección <Lógica | Unity>.
Soy <nombre>, dueño de <carpeta o archivos>.
Trabajamos en el issue #<n>: <título>.
```

Para el Design Master y Gestión basta con indicar el perfil y el issue o escena.

## Agregar un perfil nuevo

Lo crea el perfil Gestión, con aprobación de Leyni: un archivo nuevo aquí, una fila en la tabla de perfiles de `/AGENTS.md` y el registro en `docs/03_agentes.md`.
