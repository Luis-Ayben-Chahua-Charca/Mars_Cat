---
name: work-issue
description: Flujo para trabajar un issue o tarjeta de Mars Cat según el perfil indicado (Código, Design Master o Gestión), con el formato de propuesta de commit del proyecto. Úsalo cuando el humano diga "trabajemos el issue #n", pida la propuesta de commit, los comandos de git add o commit, o qué hay que revisar antes de subir.
argument-hint: "[número del issue o de la tarjeta]"
---

# work-issue — de la tarjeta al commit en Mars Cat

Issue a trabajar: el que indique el humano. Si falta el número, pídelo; **no lo inventes**. Responde en español.

Esta skill **no cambia reglas**: aplica `AGENTS.md`, `STRUCTURE.md`, `CONTRIBUTING.md` y `docs/02_reglas_de_trabajo.md`. Si algo de aquí los contradice, ganan ellos: detente y avisa.

## 1. Antes de empezar

1. Confirma tu **perfil** (`agents/`). Si el humano no lo dijo, pregúntalo antes de escribir nada.
2. Solo lectura: `git status --short` y `git log -1 --oneline`. Se trabaja en `main`, sin ramas.
3. Si hay cambios sin commitear que **no son de esta tarea** (de otra persona, del Editor o de otro commit en curso), no los toques ni los descartes. Avísalo y trabaja en tus archivos.
4. Si en el índice hay archivos ya añadidos de otro grupo, el commit de esta tarea usa rutas (ver §6, punto 5).

## 2. Reconocimiento (solo lectura)

1. Lee el issue o la tarjeta completa y confirma el número con el título.
2. Lee `STRUCTURE.md` (dueño de cada carpeta) y las secciones de `docs/diseno/` que cite el issue.
3. Lee el código o los assets existentes que vas a reutilizar.
4. Señala contradicciones entre documentos o entre un documento y el proyecto. No las resuelvas por tu cuenta.

## 3. Reparto

Clasifica cada parte del issue:

| Marca | Quién | Ejemplo |
|---|---|---|
| 🟢 | El agente | `.cs` de su perfil, pruebas NUnit de Lógica, instrucciones numeradas |
| 🟡 | Leyni en el Editor | Crear carpetas, armar escenas, asignar referencias, import settings, `.meta` |
| 🔴 | Otro rol o Gestión | Arte, decisiones de diseño, cambios en `AGENTS.md`, `STRUCTURE.md`, `agents/` o `docs/` |

Si el issue es sobre todo de otro rol, dilo y propón pasárselo.

## 4. Plan y preguntas

- Plan de 3 a 5 líneas: qué archivos, en qué carpeta y quién es su dueño. Sin ficha larga.
- **Preguntar antes**: lo que se aparta de un documento, cambia el alcance del PMV, fija un valor de balance o un dato NASA, o crea una carpeta o un nombre que no está en `STRUCTURE.md`. Cada pregunta lleva contexto, opciones y tu recomendación. Espera la respuesta.
- **Decidir y reportar**: detalles internos que no cambian ningún documento ni contrato.
- No inventes datos de NASA ni de balance: salen de `MarsCat/Assets/Data/` o del issue.

## 5. Hacer el trabajo

- **Código:** sigue `agents/agent_codigo.md` (Lógica sin `UnityEngine`, Unity como adaptadores, balance en `Data/`). Si no puedes compilar o probar, escribe *"No verificado en Unity"*.
- **Design Master:** instrucciones numeradas con ruta exacta del Editor y un "Comprobar:" al final. No editas archivos de Unity.
- **Gestión:** solo `AGENTS.md`, `STRUCTURE.md`, `agents/`, `docs/`, `README.md`, `BITACORA.md`, `.gitignore`, `.gitattributes` y configuración del repo, con aprobación de Leyni.
- No generes documentación de avance: el tablero es la documentación.
- Nunca `.meta`, `.unity`, `.prefab`, `.asset`, `.mat`, `.controller` ni `ProjectSettings/` a mano. Mover o renombrar assets se hace **dentro del Editor**.

## 6. Propuesta de commit

El agente **no hace push** y el Design Master **no ejecuta Git**: entrega la propuesta y el humano la ejecuta. Otros perfiles pueden preparar el commit solo si el humano lo pide.

Un cambio lógico por commit. Cada commit se entrega con este bloque:

```text
Commit N de M — [area]
Añadir:         <rutas exactas> — <qué cambió>
No añadir:      <ruta> — <por qué> (otro dueño, otro commit, o no es de esta tarea)
Verificar:      git status --short → debe verse <letras y cantidad>
                git diff --staged → sin .meta ajenos ni Library/Temp/Logs/UserSettings
Mensaje:        [area] descripción corta #n   (N caracteres de descripción)
No verificado:  <qué no se probó y por qué>
Luego:          git pull → git push (tuyo)
Quedan:         Commit N+1 de M — [area] <resumen>
```

Debajo del bloque van los comandos, **uno por bloque `bash`**: primero el `git add` y después el `git commit -m`.

Reglas:

1. **Rutas exactas** en `git add`. Nunca `git add .` ni `git add -A`. Para borrados y renombres se listan ambos lados. Un directorio solo se acepta si hoy contiene únicamente lo que va en ese commit.
2. **Mensaje:** `[area] descripción #n`. Áreas: `arte`, `logica`, `unity`, `integracion`, `datos`, `docs`, `repo`, `fix`. La descripción va en minúsculas y mide **menos de 60 caracteres**: mídela antes de entregarla.
3. **Áreas de Integración:** carpetas, `.gitkeep`, `.meta`, escenas, prefabs, materiales y movimientos de assets van en `integracion`. `unity` es solo para MonoBehaviours, UI, input y cámara.
4. **Enlace al issue:** solo `#n` o `refs #n`. Nunca `closes`, `fixes` ni `resolves` (ni variantes).
5. **Índice con archivos de otro grupo:** si `git status --short` muestra archivos añadidos que no son de este commit, el comando es `git commit -m "..." -- <rutas del grupo>`, para que solo entren esas rutas.
6. **Sin rastro de agentes:** ni `Co-Authored-By`, ni firmas, ni enlaces de sesión, ni menciones a una IA, ni en el mensaje ni en los archivos añadidos.
7. **Cambios que reescribe Unity** (por ejemplo `EditorBuildSettings.asset` al renombrar una escena): se listan aparte. Si aparece uno no reconocido, no se añade y se pide el nombre del archivo.
8. **Primer commit de assets binarios:** recordar `git lfs install` y comprobar con `git lfs ls-files`.
9. **El siguiente grupo no se entrega** hasta que el humano diga "siguiente".
10. Orden habitual: `repo` → `docs` → `logica` → `unity` → `integracion` (`.meta`, escenas) → `datos`.

## 7. Cierre

- Pasos para el humano: lo que solo se hace en el Editor, con ruta exacta y qué comprobar.
- Si hay algo que configurar o algo pendiente, un comentario de **2 a 4 líneas** para la tarjeta. Si no, no escribas nada más.
- Los checks de la tarjeta los marca Gestión. No marques nada por el humano.

## Estilo de respuesta

Primero lo que importa: qué está hecho, qué falta y qué debe hacer el humano. Tablas para repartos y listas de archivos, bloques `bash` de un comando cada uno, rutas como enlaces. Sin relleno. Si algo falla o se saltó un paso, dilo con la salida real.
