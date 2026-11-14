# Herramientas: Claude Code y ChatGPT Codex

Los integrantes pueden usar **Claude Code** o **ChatGPT Codex** en su máquina. Ambos siguen `AGENTS.md`; este archivo solo agrega lo propio de cada herramienta.

## Cómo cargan las instrucciones

| Herramienta | Lee automáticamente | Nota |
|---|---|---|
| Codex | `AGENTS.md` | Debe abrirse en la raíz del repo (`Mars_Cat/`) |
| Claude Code | `CLAUDE.md` (que importa `AGENTS.md`) | Debe abrirse en la raíz del repo (`Mars_Cat/`) |

Ninguna carga el perfil sola: **en el primer mensaje indica tu perfil** (ver plantilla en `agents/README.md`).

## Configuración recomendada para ambas

- Abrir siempre desde la raíz del repo, nunca desde dentro de `MarsCat/`.
- Usar el modo en que la herramienta **pide aprobación antes de ejecutar comandos**, y no aprobar:
  - `git push`, `git rebase`, `git reset`, `git checkout -- .`, `git clean`, cualquier `--force`
  - comandos que borren o muevan archivos fuera de tu carpeta
  - abrir o ejecutar Unity
- Revisar el diff antes de cada commit (`git diff --staged`). Si aparece un `.meta`, una escena o un archivo de otra persona, quitarlo del commit.
- Si la herramienta propone "arreglar" archivos fuera de tu carpeta para que compile, detente y avisa en el issue al dueño.

## Si tu Unity está abierto mientras el agente trabaja

Unity detecta los `.cs` nuevos y genera `.meta` locales. Es normal; **no los subas**. Los oficiales los sube el integrador.

## Al terminar

Haz tú el commit con `#n` y el push siguiendo `CONTRIBUTING.md`. Si el agente te dio un comentario para la tarjeta, pégalo; si no, no hace falta escribir nada más.
