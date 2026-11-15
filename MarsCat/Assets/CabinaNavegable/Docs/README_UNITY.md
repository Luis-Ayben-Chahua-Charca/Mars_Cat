# CABINA NAVEGABLE – Guía de integración en Unity (v2, tras la auditoría final)

Escenario modular futurista modelado en Blender 5.1.2 con scripts `bpy`, según el plano adjunto:
cerramiento rectangular, 7 módulos circulares, 6 pasillos, zona técnica lateral derecha y cubierta desmontable.

> **Estado de verificación.**
> - **Verificado automáticamente en Blender** (ver `AUDITORIA_FINAL.md` y `QC_REPORTE.md`): geometría, escala, normales,
>   re-importación de los 3 FBX, recorrido completo con un jugador de radio 0,40 y 0,50 m sobre los colliders re-importados,
>   puertas, cobertura de colliders y apertura de las hojas.
> - **Scripts C#:** compilados con Mono contra *stubs* de la API de Unity (0 errores, 0 avisos). Esto descarta errores de
>   sintaxis y de tipos, pero **no sustituye a compilarlos dentro de Unity**.
> - **No se ha abierto en Unity.** Los ajustes de importación, la orientación de ejes dentro de Unity, el aspecto de los
>   materiales y el comportamiento en ejecución están **pendientes de prueba**. El menú *Cabina › 3* automatiza esa prueba.

---

## 1. Contenido del paquete

| Archivo / carpeta | Descripción |
|---|---|
| `CABINA_NAVEGABLE.blend` | Proyecto editable (Blender **5.1 o posterior**). 16 colecciones, 2 capas de vista (`MODELO_COMPLETO`, `MODELO_INSPECCION`), 16 cámaras de revisión. |
| `CABINA_NAVEGABLE_COMPLETA.fbx` | Modelo visual con cubierta (colecciones 01–15). 39 488 triángulos, 233 mallas. |
| `CABINA_NAVEGABLE_INTERIOR.fbx` | Modelo visual sin cubierta (colecciones 01–14). 32 706 triángulos, 203 mallas. |
| `CABINA_COLLIDERS.fbx` | Solo colisiones (colección 16). 116 mallas, 2 540 triángulos. |
| `TEXTURAS/` | 11 PNG potencia de 2 (512–2048 px, 1,1 MB en total): albedo + normal (OpenGL/Y+), franjas, pantallas, atlas de señales. |
| `UNITY_SCRIPTS/Runtime/` | `CabinaPropiedades`, `CabinaPuertaAutomatica`, `CabinaControladorPrimeraPersona`, `CabinaPruebaRecorrido`, `CabinaDatos` (generado). |
| `UNITY_SCRIPTS/Editor/` | `CabinaImportPostprocessor`, `CabinaColliderUtil`, `CabinaConfigurador` (menú **Cabina**). |
| `RENDERS/` | 17 vistas de revisión (superior, isométricas, frontal, núcleo, 6 módulos, accesos, pasillos, zona técnica), hoja de contactos y mapa del QC. |
| `AUDITORIA/` | Comparativa plano/modelo, mapa del recorrido y resultados en JSON de navegación, optimización, exportación y puertas. |
| `AUDITORIA_FINAL.md` | Informe de la segunda auditoría: pruebas superadas, pendientes, problemas y correcciones. |
| `QC_REPORTE.md` | Control de calidad automático (48/48). |
| `SCRIPTS_BLENDER/` | Scripts que generan el modelo, las texturas, el QC y los renders; `audit/` contiene los de la auditoría. |

## 2. Medidas y decisiones respecto al plano

El plano pide 20 m de ancho, pero el núcleo (8 m) y las dos salas laterales (6 + 6 m) ya suman 20 m sin pasillos ni zona
técnica; la fila central completa mide 23 m. Se optó por **mantener los diámetros y ampliar el ancho a 26 m**
(23 m de fila central + holgura + zona técnica de 2 m). El largo se mantiene en 30 m. Si prefieres respetar los 20 m,
la alternativa es reducir los diámetros (~0,75×) en `build_cabina.py`.

| Elemento | Medida |
|---|---|
| Cerramiento exterior | **26,0 × 30,0 m**, muros de 0,20 m, altura 3,00 m |
| Doble muro del plano | Revestimiento interior de 0,60 m con **bahías** laterales donde encajan las salas laterales |
| Núcleo central (M-00) | Ø 8,0 m exterior / 7,6 m interior. Centro en Blender (−1,1; 0) |
| Salas periféricas (M-01…M-06) | Ø 6,0 m exterior / 5,6 m interior, a 8,5 m del núcleo, cada 60° |
| Pasillos (6) | Ancho libre **2,00 m**, altura 3,0 m, entradas abiertas de 2,0 × 2,6 m |
| Zona técnica | Franja este de 2,0 × 29,6 m tras un muro separador de 0,20 m |
| Cubierta | Paneles de 0,20 m, tapas de módulos de 0,30 m, techos de pasillos de 0,25 m, cúpula en el núcleo |

Posiciones en Blender (m): M-01 Superior izq. (−5,35; 7,36) · M-02 Superior der. (3,15; 7,36) · M-03 Lateral izq. (−9,6; 0) ·
M-04 Lateral der. (7,4; 0) · M-05 Inferior izq. (−5,35; −7,36) · M-06 Inferior der. (3,15; −7,36).

**Flujo de juego** (igual que las zonas de circulación del plano): acceso norte → vestíbulo norte → salas M-01/M-02 →
pasillos → núcleo → salas M-05/M-06 → vestíbulo sur → salida. Las salas laterales M-03/M-04 solo se alcanzan desde el núcleo.
La zona técnica se alcanza desde ambos vestíbulos.

**Límite del mapa:** fuera del cerramiento no hay suelo, igual que en el plano, donde la línea exterior es el «límite del mapa».
Las puertas exteriores (norte, sur y emergencia NE) se abren, pero tres colliders `COL_Limite_Mapa_*` impiden salir del
nivel y caer. Si más adelante quieres un exterior jugable, elimina esos colliders y añade suelo exterior.

## 3. Ejes, escala y pivotes

- Blender: 1 unidad = 1 m, Z arriba, **+Y = norte del plano**, +X = este; origen en el centro de la planta, suelo en Z = 0.
- FBX: Y arriba, metros (`UnitScaleFactor = 100`), exportado con *Apply Transform*, *Forward −Z*, *Up Y*, *Apply Scalings: FBX All*.
- **Unity (esperado, sin probar en Unity):** Unity invierte X al importar, de modo que un punto de Blender (x, y, z) queda en
  **(−x, z, −y)**. Norte = **−Z**, este = −X y la dirección de la flecha del plano (hacia el sur) = **+Z**.
  El núcleo queda en (1,1; 0; 0). `CabinaPruebaRecorrido` comprueba esta orientación al empezar y, si Unity no invirtiera X,
  lo detecta, lo indica en la Consola y corrige sus puntos de paso.
- Toda la geometría estática tiene ubicación, rotación y escala aplicadas (origen en 0,0,0). **Excepción deliberada:** los objetos
  que se programarán (`PUERTA_*`, `INT_*`, `COL_PUERTA_*`, `COL_TRG_*`, `COL_Obs_*` originales) conservan su pivote (base del
  objeto) y un giro solo en el eje vertical, con escala 1.
- Jerarquía de un solo nivel: `GRP_xx_COLECCION → objetos` en los modelos visuales y `CABINA_COLLIDERS → COL_*` en el de
  colisiones. Unity añade el raíz con el nombre del archivo.

## 4. Importación paso a paso

**Requisitos:** Unity 2021.3 LTS o posterior (probado solo a nivel de compilación con stubs). URP o Built-in; para HDRP los
materiales hay que configurarlos a mano.

1. Crea la carpeta `Assets/CabinaNavegable/`.
2. **Primero los scripts:** copia la carpeta `UNITY_SCRIPTS/` entera (con `Runtime/` y `Editor/`) dentro de
   `Assets/CabinaNavegable/`. Espera a que Unity compile: aparecerá el menú **Cabina** en la barra superior.
3. Copia `CABINA_NAVEGABLE_COMPLETA.fbx`, `CABINA_NAVEGABLE_INTERIOR.fbx`, `CABINA_COLLIDERS.fbx` y la carpeta `TEXTURAS/`
   en `Assets/CabinaNavegable/` (en la **misma carpeta**: las texturas se referencian como `TEXTURAS/…`).
4. Si copiaste los FBX antes que los scripts: selecciona los tres FBX → clic derecho → **Reimport**.
5. Comprueba en el Inspector de cada FBX, pestaña **Model** (el postprocesador lo deja así):

| Ajuste | Valor |
|---|---|
| Scale Factor / Convert Units | 1 / ✓ (*File Scale* debe ser 1) |
| Import Cameras / Import Lights | ✗ / ✗ |
| Normals / Tangents | Import / Calculate Mikktspace |
| Generate Lightmap UVs | ✓ en los modelos visuales, ✗ en colliders |
| Rig / Animation | None / desactivada |
| Materials (solo colliders) | None |

## 5. Materiales

**Automático:** menú **Cabina › 1. Configurar texturas y materiales**. Este menú:
- marca los `*_Normal.png` como *Normal map* (sin sRGB) y el resto como *Default* (sRGB), con *Wrap: Repeat*, mipmaps y aniso 4;
- crea `Materiales/MAT_*.mat` con **URP/Lit** si el proyecto usa URP, o **Standard** en Built-in;
- los asigna a los dos FBX visuales mediante *remap*, sin extraer ni duplicar materiales.

**Manual** (por ejemplo en HDRP), con colores en sRGB y suavidad = 1 − rugosidad de Blender. En los materiales con textura el color base es **blanco**,
porque Unity multiplica el color por la textura. Las UV están a escala de mundo (1 repetición = 2 m en paredes y suelos,
1 m en la rejilla y 4 m en la cubierta), así que no hace falta tocar el *tiling*.

| Material | Texturas | Color base | Metálico | Suavidad | Emisión (HDR, aproximada) |
|---|---|---|---|---|---|
| MAT_Cable | — | #272727 | 0.00 | 0.30 | — |
| MAT_Consola_Gris_Oscuro | — | #61656C | 0.50 | 0.60 | — |
| MAT_Cubierta_Metal | T_Cubierta_Panel_Albedo / T_Cubierta_Panel_Normal | #FFFFFF | 0.50 | 0.50 | — |
| MAT_Estructura_Negro | — | #2C2C30 | 0.80 | 0.65 | — |
| MAT_Franjas_Seguridad | T_Franjas_Seguridad_Albedo | #FFFFFF | 0.00 | 0.40 | — |
| MAT_LED_Ambar_Advertencia | — | #FFC43F | 0.00 | 0.70 | #FFC43F × 4 |
| MAT_LED_Cian | — | #59EDFF | 0.00 | 0.70 | #59EDFF × 3 |
| MAT_Pantalla_UI | T_Pantalla_UI_Emision | #FFFFFF | 0.00 | 0.75 | textura × 1.2 |
| MAT_Pared_Metal | T_Pared_Metal_Panel_Albedo / T_Pared_Metal_Panel_Normal | #FFFFFF | 0.55 | 0.58 | — |
| MAT_Puerta_Metal | — | #DDDFE3 | 0.60 | 0.65 | — |
| MAT_Rejilla_Metal | T_Rejilla_Tecnica_Albedo / T_Rejilla_Tecnica_Normal | #FFFFFF | 0.70 | 0.55 | — |
| MAT_Senal_Amarilla | — | #F9DD38 | 0.00 | 0.50 | #F9DD38 × 0.5 |
| MAT_Senal_Placa | — | #303845 | 0.30 | 0.60 | — |
| MAT_Senales_Atlas | T_Senales_Atlas | #FFFFFF | 0.00 | 0.65 | textura × 0.7 |
| MAT_Suelo_Placas | T_Suelo_Placas_Albedo / T_Suelo_Placas_Normal | #FFFFFF | 0.40 | 0.45 | — |
| MAT_Tuberia_Metal | — | #B3B6BC | 0.85 | 0.70 | — |

`MAT_Estructura_Negro` también se usa en los **cantos de la cubierta** (corrección de la auditoría).
`MAT_Collider_Debug` existe solo en el `.blend` para ver los colliders y no se importa en Unity.

**Luces:** el FBX no lleva luces. *Cabina › 2* crea 37 luces puntuales (`LUCES_CABINA`) en las posiciones de las luces del
`.blend`, con intensidad y alcance aproximados, más luz ambiente plana. Ajústalas a tu pipeline. Para iluminación horneada,
marca la arquitectura como *Contribute GI* (el postprocesador ya lo hace) y usa los UV de lightmap generados.

## 6. Colliders

`CABINA_COLLIDERS.fbx` se coloca en el mismo origen que el modelo visual y coincide con él exactamente.
El postprocesador convierte cada `COL_*` en un collider y elimina su `MeshRenderer`:

| Prefijo | Cantidad | Collider de Unity | Uso |
|---|---|---|---|
| `COL_Suelo` | 1 | BoxCollider | Suelo continuo (z = 0) |
| `COL_Muro_Exterior_*` | 16 | BoxCollider | Muros perimetrales, bahías y muro de la zona técnica |
| `COL_<M>_Pared_Circular` | 7 | MeshCollider (no convexo, 168–180 tri) | Muros de los 7 módulos con sus vanos |
| `COL_PAS_*_Pared_A/B` | 12 | 4 BoxCollider + 8 MeshCollider (cajas giradas) | Paredes de los pasillos |
| `COL_Obs_*` | 19 | 18 BoxCollider + 1 MeshCollider (consola cilíndrica del núcleo) | Consolas, terminales, armarios, panel eléctrico |
| `COL_Obs_Aud_*` | 39 | 17 BoxCollider + 22 MeshCollider (cajas giradas de 12 tri) | **Nuevos:** marcos, pantallas, paneles y nervios que sobresalían del muro |
| `COL_Limite_Mapa_*` | 3 | BoxCollider | **Nuevos:** límite del mapa delante de las puertas exteriores |
| `COL_PUERTA_*` | 9 | BoxCollider (lo activa/desactiva la puerta) | Bloquea el paso con la puerta cerrada |
| `COL_TRG_PUERTA_*` | 9 | BoxCollider **Is Trigger** | Detección: abre la puerta desde 1,54 m a cada lado |
| `COL_Cubierta_Opcional` | 1 | BoxCollider | Techo a 3,0 m; desactívalo si usas cámara cenital con el modelo INTERIOR |

En total hay 78 cajas alineadas que se convierten en `BoxCollider` (más baratos) y 38 `MeshCollider` no convexos. El mayor tiene
180 triángulos. No marques ningún MeshCollider como convexo: los muros circulares con vanos no lo son.

**Sin el postprocesador:** añade a mano un `MeshCollider` a cada `COL_*` (y un `BoxCollider` *Is Trigger* a los `COL_TRG_*`), y
desactiva sus `MeshRenderer`. Para una prueba rápida sin puertas, desactiva los `COL_PUERTA_*`.

## 7. Player Controller

**Automático:** menú **Cabina › 2. Montar escena de prueba**. Crea y guarda `CABINA_Prueba.unity` con lo siguiente:
- modelo visual COMPLETA (o INTERIOR si es el único presente) y colliders en (0, 0, 0);
- puertas configuradas: un `CabinaPuertaAutomatica` en cada `COL_TRG_PUERTA_*`, enlazado a sus hojas, a su collider de bloqueo
  y a su baliza;
- luces y luz ambiente;
- **JUGADOR** a 1,4 m dentro del acceso principal, mirando hacia el interior. Lleva un `CharacterController` (altura 1,8,
  radio 0,4, centro y = 0,9, *Step Offset* 0,3, *Slope Limit* 45°, *Skin Width* 0,04), el componente
  `CabinaControladorPrimeraPersona` y una cámara hija a 1,65 m (tag *MainCamera*, plano cercano 0,05, FOV 70).

**Manual:** crea un GameObject `JUGADOR` (tag *Player*) con esos mismos ajustes de `CharacterController` y añade
`CabinaControladorPrimeraPersona`. Añade una cámara hija a 1,65 m y arrástrala al campo *Camara*. Colócalo en Unity (1,1; 0,05; −13,5),
mirando hacia +Z.

**Entrada:** el controlador funciona con el **Input System nuevo** (`ENABLE_INPUT_SYSTEM`, por defecto en Unity 6) y con el
**Input Manager clásico**. No necesita configurar acciones.

## 8. Navegación en primera persona

| Acción | Control |
|---|---|
| Moverse | WASD / flechas |
| Mirar | Ratón (clic en la vista de juego para capturarlo; Esc para liberarlo) |
| Correr / saltar | Shift / Espacio |

Parámetros en el Inspector: `velocidad` 3,5 m/s, `velocidadCorrer` 6 m/s, `alturaSalto` 1 m, `gravedad` −19,6, `sensibilidadRaton`.

Márgenes medidos sobre los colliders re-importados: pasillos de 2,00 m, puertas de salas de 1,6 m, puertas técnicas de 1,2 m,
emergencia de 1,4 m y acceso/salida de 3,0 m. Hay 3,0 m libres de techo en todo el nivel y no hay escalones (las marcas del suelo
miden 6–8 mm). Todo el recorrido funciona con radios de cápsula de **0,40 m y 0,50 m** (el valor por defecto de Unity).
Si quieres NPC, el paquete *AI Navigation* puede hornear un NavMesh a partir de los colliders (radio de agente 0,3–0,5 m).

## 9. Puertas y accesos: cómo probarlos

Funcionamiento de `CabinaPuertaAutomatica`:
- se abre cuando un `CharacterController`, un `Rigidbody` o un objeto con tag *Player* entra en el trigger;
- permanece abierta mientras haya alguien dentro y se cierra 0,6 s después de que salga;
- su collider de bloqueo se desactiva al empezar a abrirse y se reactiva al cerrarse del todo;
- `bloqueada` la mantiene cerrada; `invertirSentido` sirve si una hoja se abre hacia el lado equivocado.

| Puerta | Ancho libre | Modo de apertura | Verificación en Blender |
|---|---|---|---|
| N_Principal / S_Salida (doble hoja) | 3,0 × 2,6 m | Deslizar 1,47 m por hoja | Hoja 99,5 % oculta en el muro |
| NE_Emergencia | 1,4 × 2,4 m | Deslizar 1,39 m | 99,4 % oculta |
| TEC_Norte / TEC_Sur | 1,2 × 2,4 m | Deslizar 1,19 m | 99,4 % oculta |
| MSI / MSD / MII / MID _Exterior (muro curvo) | 1,6 × 2,4 m | **Girar ~31,4°** alrededor del centro de la sala | 95,4 % oculta (en línea recta quedaría un 91 % fuera del muro) |

**Prueba manual (lista de comprobación):**
1. Pulsa Play y camina hacia la puerta de M-01 desde el vestíbulo: debe abrirse antes de llegar y cerrarse al alejarte.
2. Atraviesa las 4 puertas de las salas, las 2 técnicas y entra en los 6 pasillos (no tienen puertas).
3. Acércate a la puerta principal desde dentro: se abre, pero el límite del mapa no te deja salir.
4. Comprueba que en las puertas de las salas la hoja recorre el muro curvo sin asomar al vestíbulo.
5. Marca `bloqueada` en una puerta y comprueba que no deja pasar.

**Prueba automática:** menú **Cabina › 3. Ejecutar prueba automática de recorrido**. Añade `CabinaPruebaRecorrido` al
JUGADOR y entra en Play. El jugador sigue 46 puntos de paso (183 m) calculados en la auditoría: acceso principal → núcleo →
M-01…M-06, volviendo cada vez al núcleo → zona técnica → salida sur. La prueba falla si se atasca 4 s, cae o sale del
cerramiento. El resultado se muestra en pantalla y en la Consola, y se guarda en `<proyecto>/CabinaPruebaRecorrido_resultado.txt`
(incluye las aperturas de cada puerta). Resultado esperado: «RECORRIDO COMPLETADO · Objetivos alcanzados: 15/15».

## 10. Organización y nomenclatura

| Colección | Prefijos | Contenido |
|---|---|---|
| 01_ESTRUCTURA_EXTERIOR | `EXT_` | Muro perimetral, revestimiento N/S con bahías, nervios (Array), tuberías, rejillas (Array) |
| 02_SUELOS | `SUE_` | Suelo general, rejilla técnica, marcas de circulación (Array), flechas de acceso/salida |
| 03_MODULO_CENTRAL | `MC_` | Muro circular con 6 vanos, suelo, anillos LED, 6 pantallas |
| 04–09_MODULO_* | `MSI_ MSD_ MLI_ MLD_ MII_ MID_` | Muro circular, suelo, anillo LED y pantalla mural de cada sala |
| 10_PASILLOS | `PAS_` | Paredes, suelo y LED de los 6 pasillos |
| 11_ZONA_TECNICA | `TEC_`, `INT_Panel_*` | Muro separador, armarios, cableado, tuberías, ventilación, consola, cuadro y paneles de mantenimiento |
| 12_PUERTAS_Y_ACCESOS | `PUERTA_ MARCO_ ACC_Umbral_ INT_PanelAcceso_ INT_LuzAdvertencia_ INT_Punto_` | Hojas, marcos, umbrales, paneles de acceso y balizas |
| 13_ILUMINACION | `LUZ_ LED_` | Luces de Blender (no se exportan) y LED del zócalo |
| 14_MOBILIARIO | `INT_Consola_ INT_Terminal_Info_ INT_Senal_ INT_Holograma_` | Consolas, terminales, señales y holograma |
| 15_CUBIERTA_REMOVIBLE | `CUB_` | Paneles exteriores, de zona técnica, tapas y lucernarios, cúpula y techos de pasillos |
| 16_COLLIDERS_UNITY | `COL_` | 116 mallas de colisión bajo el empty `CABINA_COLLIDERS` |

**Visualización en Blender:** selector de capa de vista `MODELO_COMPLETO` (cabina cerrada) o `MODELO_INSPECCION` (cubierta
excluida). La colección 16 está excluida en ambas; actívala desde el Outliner. Las cámaras `CAM_AUD_*` reproducen las vistas
de `RENDERS/`. **En Unity:** *Cabina › Mostrar u ocultar cubierta*.

## 11. Objetos interactivos y propiedades

Todos son objetos separados con propiedades personalizadas exportadas al FBX. El postprocesador las copia a `CabinaPropiedades`.

| Objetos | Cant. | Propiedades principales |
|---|---|---|
| `PUERTA_<id>` / `PUERTA_<id>_Hoja_Izq/_Der` | 11 hojas (9 puertas) | `tipo=puerta_automatica`, `puerta_id`, `recorrido_m`, `ancho_libre_m`, `alto_libre_m`, `apertura_dir_unity`, `apertura_dir_blender` |
| `INT_PanelAcceso_<puerta>` | 9 | `tipo=panel_acceso`, `puerta_id` |
| `INT_LuzAdvertencia_<puerta>` | 9 | `tipo=luz_advertencia`, `puerta_id` |
| `INT_Consola_*` | 8 | `tipo=consola`, `modulo`, `funcion` |
| `INT_Terminal_Info_*` | 8 | `tipo=terminal_informacion` |
| `INT_Senal_*` | 15 | `tipo=senal_navegacion`, `senal_id`, `texto` |
| `INT_Panel_Electrico_Principal`, `INT_Panel_Mantenimiento_N/S` | 3 | `tipo=panel_electrico / panel_mantenimiento` |
| `INT_Holograma_MC` | 1 | `tipo=holograma`, `animable` |
| `INT_Punto_*` (empties) | 28 | `tipo=punto_interaccion`, `objetivo` (posición del jugador para interactuar) |

## 12. Rendimiento

| Métrica | Valor |
|---|---|
| Triángulos | 39 488 (COMPLETA) · 32 706 (INTERIOR) · 2 540 (colliders) |
| Vértices (visual) | 20 830 |
| Materiales | 16 en los modelos visuales; 369 submallas en total, que se reducen con *Static Batching* o el *SRP Batcher* de URP |
| Texturas | 11 PNG potencia de 2, 1,1 MB en disco |
| Objetos más pesados | `MC_Pared_Circular` (2 574 tri) y muros de las salas (≈1 450 tri) |

Es un escenario ligero, adecuado incluso para equipos modestos. No se ha medido en Unity (pendiente).

## 13. Regenerar o reexportar

- **Exportación FBX desde el `.blend`:** *Selected Objects*, *Object Types: Empty + Mesh*, *Apply Modifiers*,
  *Apply Scalings: FBX All*, *Forward −Z*, *Up Y*, *Apply Unit*, *Use Space Transform*, *Apply Transform*, *Smoothing: Face*,
  *Triangulate Faces*, *Custom Properties*, *Path Mode: Relative*, sin *Leaf Bones* ni animación
  (`SCRIPTS_BLENDER/audit/audit_export.py` lo hace por código).
- **Scripts:** `build_cabina.py` (modelo), `textures.py`, `qc_cabina.py` (QC), `render_views.py`.
  En `audit/`: `nav_audit.py` (navegación), `opt_export_audit.py` (optimización y re-importación), `door_open_audit.py`,
  `fix_colliders.py`, `fix_roof_rims.py`, `gen_unity_data.py` (regenera `CabinaDatos.cs`), `render_audit.py`.
  En `csharp_check/`: stubs y orden de compilación con Mono.
- **Modificadores** activos y editables en el `.blend`: Bevel + Weighted Normal (muros y marcos), Solidify (cubierta, con
  material de canto) y Array (nervios, marcas, armarios, rejillas). El FBX los lleva aplicados. Los Boolean de los vanos se
  aplicaron al construir.
- Si cambias la geometría, vuelve a ejecutar `nav_audit.py` y `gen_unity_data.py` para actualizar los puntos de paso de la
  prueba automática.

## 14. Limitaciones conocidas

- **Unity no se ha ejecutado:** importación, materiales, luces, puertas, controlador y prueba automática están pendientes de
  verificar en el editor (ver `AUDITORIA_FINAL.md`, sección 2).
- La intensidad de las luces y de la emisión en Unity es una aproximación a la escena de Blender.
- Las texturas son procedurales y sencillas (sin mapas de rugosidad o metalicidad por píxel).
- Los dorsos de pantallas y señales (pegados a la pared, sin normal map) tienen UV degeneradas. No se ven.
- El `.blend` se guardó con Blender 5.1.2; versiones anteriores pueden no abrirlo.
