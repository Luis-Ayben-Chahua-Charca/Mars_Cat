# 02 — Reglas de trabajo

Pocas reglas, todas obligatorias. Existen para que 6 personas puedan trabajar en `main` sin ramas ni CI.

## Git

1. **Todo en `main`.** Sin ramas, sin pull requests.
2. **Commits simples y completos:** un commit = una cosa terminada que compila. Nada a medias.
3. **Siempre en este orden:**
   ```
   git add <tu carpeta>
   git commit -m "[area] qué hice #issue"
   git pull
   git push
   ```
   Nunca hacer `git pull` con cambios sin commitear.
4. **Sin rebase.** Cada uno configura una vez `git config pull.rebase false`. Si Git abre un editor con "Merge branch 'main'...", guardar y cerrar.
5. **Prohibido:** `git push --force`, `git reset --hard` sobre commits ya subidos, `git add .` desde la raíz (agregar solo tu carpeta).
6. **Si rompes la compilación**, arreglarla es tu prioridad número uno, antes que cualquier tarjeta.
7. **Arte binario por LFS:** instalar Git LFS **antes** del primer commit de assets (`git lfs install`). El `.gitattributes` ya define qué extensiones van por LFS.

## Mensajes de commit

Formato: `[area] descripción corta #issue`

| Área | Para |
|---|---|
| `arte` | Modelos, texturas, animaciones |
| `logica` | C# puro |
| `unity` | MonoBehaviours, UI, input, cámara |
| `integracion` | Escenas, prefabs, `.meta`, import settings |
| `datos` | Datos NASA |
| `docs` | Documentación |
| `repo` | Configuración del repo o del proyecto |
| `fix` | Arreglo de algo roto |

Ejemplo: `[logica] consumo de oxígeno por colono #8`

> ⚠️ **Nunca** escribir `closes #`, `fixes #`, `resolves #` (ni `close`, `fix`, `resolve`, en cualquier forma). Esas palabras cierran el issue automáticamente. Solo `#8` o `refs #8`.

## Archivos `.meta` (lo más importante en Unity)

- **Solo los integradores** generan y suben `.meta`.
- Arte sube `.fbx` / `.png` **sin** `.meta`. Lógica sube `.cs` **sin** `.meta`.
- El integrador abre Unity, deja que genere los `.meta`, y los sube en un commit `[integracion] meta de <cosa> #issue`.
- **Nunca borrar ni regenerar un `.meta` existente.** Cambia el GUID y rompe todas las referencias en escenas y prefabs.
- Si mueves o renombras un asset, hazlo **dentro de Unity** (no en el explorador de Windows), para que el `.meta` lo acompañe.

Nota para Lógica y Arte: si abren Unity para probar, Unity creará `.meta` locales de sus archivos. **No los suban**; cuando el integrador suba los oficiales, borren los locales sin commitear antes de hacer pull.

## Propiedad de archivos

- **Un script, un dueño.** Dos personas nunca editan el mismo `.cs`.
- **Una escena, un dueño** (ver `01_roles_y_personas.md`).
- Para probar, cada quien usa su `Sandbox_<Nombre>.unity`.
- El dueño de un archivo es quien tiene asignado el issue que lo creó.

## El tablero es la documentación

No se escriben documentos de avance. Cada tarjeta lleva un **checklist** de lo que hay que entregar; quien hace el trabajo solo sube sus archivos con un commit `#n`, y Gestión revisa y marca los checks a mano.

Ejemplo de tarjeta de arte:

```
Edificio: hábitat principal  #21
- [ ] habitat.blend en Art_Source/Blender/
- [ ] SM_Habitat.fbx en MarsCat/Assets/Art/Models/
- [ ] Importado con .meta (Integración)
- [ ] Colocado en Main.unity
```

El artista sube `.blend` y `.fbx` con `[arte] hábitat principal #21` y no escribe nada más. Los checks restantes los marca quien integra.

## Tablero (GitHub Projects)

Columnas (campo **Estado**): `Backlog` → `Por hacer` → `En curso` → `Revisión` → `Hecho`

- El tablero usa un campo propio llamado **Estado**, no el `Status` que trae GitHub. Las automatizaciones de GitHub solo mueven `Status`, así que no tocan las tarjetas.
- **Automatizaciones del Project desactivadas** de todos modos. Nada se mueve ni se cierra solo.
- Máximo **1 tarjeta por persona** en `En curso`.
- Mover a `Revisión` cuando esté subido a `main`. Gestión (o el integrador correspondiente) lo prueba en Unity y lo pasa a `Hecho`.
- Los issues y los hitos se cierran **a mano**.

Etiquetas: `arte`, `logica`, `unity`, `integracion`, `datos-nasa`, `diseno`, `docs`, `repo`, `todos`, `bug`, `P0` (imprescindible para la demo), `P1` (deseable), `P2` (si sobra tiempo).

## Tags

Cuando el juego funciona en un punto importante, Gestión crea un tag:

```
git tag h1-jugable
git push --tags
```

Si algo se rompe cerca de la entrega, se vuelve a la última versión etiquetada para la demo.

## Sincronización

Cada 3–4 horas, 5 minutos: qué cerré, qué tomo, qué me bloquea. Se mueven tarjetas y se sigue.
