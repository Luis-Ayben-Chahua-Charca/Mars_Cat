# Auditoría final – CABINA_NAVEGABLE

Fecha: 14-11-2026 · Herramientas: Blender 5.1.2 (bpy, sin interfaz), Python (shapely, numpy, scipy), Mono `mcs` 6.8.
Alcance: segunda auditoría sobre los archivos ya generados, sin reconstruir el proyecto ni eliminar elementos.
Todos los resultados de este informe proceden de pruebas ejecutadas; las pruebas no ejecutadas figuran como **pendientes**.

## Resumen

| Área | Estado |
|---|---|
| Revisión visual (16 renders nuevos + comparativa con el plano) | ✅ Ejecutada. La distribución coincide con el plano; la única diferencia es el ancho (26 m), ya documentada |
| Navegación (recorrido completo, puertas, pasillos, límites) | ✅ 15/15 tramos con radio 0,40 m y 0,50 m, tras corregir colliders |
| Optimización para Unity | ✅ Revisada; 1 problema de UV corregido |
| Exportación (re-importación de los 3 FBX) | ✅ 0 diferencias por objeto con el `.blend` |
| QC original | ✅ 48/48 (criterio del exterior actualizado) |
| Scripts C# | ✅ Compilan contra stubs de la API (0 errores, 0 avisos) · ⏳ compilación real en Unity pendiente |
| Prueba en Unity | ⏳ **No ejecutada**: no hay un editor de Unity disponible en este entorno (ver §2) |

Problemas encontrados: **11** (3 de gravedad alta, 4 media, 2 baja y 2 informativos aceptados). Correcciones realizadas: **8**.
Todas las correcciones se volvieron a verificar con las mismas pruebas.

---

## 1. Pruebas ejecutadas correctamente

### 1.1 Revisión visual
| Prueba | Resultado |
|---|---|
| Hoja de contactos con las 17 vistas | `RENDERS/00_Hoja_de_contactos.png` |
| Vista superior, isométricas con y sin cubierta, interior del núcleo | `RENDERS/01, 02, 03, 05` (Cycles + OpenImageDenoise, 1600–1920 px) |
| Vista desde cada uno de los 6 módulos | `RENDERS/06`–`11` (cámara a 1,65 m mirando al pasillo y al núcleo) |
| Accesos y pasillos | `12` acceso principal con puertas abiertas · `13` vestíbulo norte · `14` pasillos y núcleo sin cubierta · `15` pasillo núcleo→M-04 · `16` zona técnica · `17` vestíbulo sur |
| Coherencia con el plano | `AUDITORIA/00_Comparativa_Plano_vs_Modelo.png`. Coinciden: 7 módulos (núcleo + 2 superiores, 2 laterales, 2 inferiores), 6 pasillos radiales, zona técnica rayada a la derecha, acceso norte con flecha, salida sur, puerta de emergencia NE, bahías laterales del doble muro y zonas de circulación discontinuas |

### 1.2 Navegación
Método: re-importación de `CABINA_COLLIDERS.fbx` (lo mismo que recibe Unity) y cortes horizontales exactos de cada malla a
0,37 / 0,93 / 1,53 / 1,77 m. Sobre ellos, una rejilla de 5 cm donde una celda es transitable si cumple tres condiciones: hay suelo,
la cápsula no toca colliders sólidos y hay al menos 1,90 m libres hasta el techo. El recorrido se calcula con A*, con las puertas abiertas.

| Tramo (r = 0,40 m) | Longitud | Holgura mín. | Puertas | Pasillo |
|---|---|---|---|---|
| Acceso principal → Núcleo | 17,84 m | 0,416 m | MSI_Exterior | MSI |
| Núcleo ↔ M-01 Superior izq. (ida/vuelta) | 6,84 m | 0,416 m | — | MSI |
| Núcleo ↔ M-02 Superior der. | 6,89 m | 0,409 m | — | MSD |
| Núcleo ↔ M-03 Lateral izq. | 9,53 m | 0,405 m | — | MLI |
| Núcleo ↔ M-04 Lateral der. | 9,63 m | 0,405 m | — | MLD |
| Núcleo ↔ M-05 Inferior izq. | 11,76 m | 0,406 m | — | MII |
| Núcleo ↔ M-06 Inferior der. | 11,76 m | 0,406 m | — | MID |
| Núcleo → Zona técnica | 23,71 m | 0,421 m | MSD_Exterior, TEC_Norte | MSD |
| Zona técnica → Salida sur | 28,86 m | 0,425 m | TEC_Sur | — |

| Prueba | Resultado |
|---|---|
| Ingresar por el acceso principal, llegar al núcleo, ir a los 6 módulos, volver al núcleo, ir a la zona técnica y salir por el sur | ✅ 15/15 tramos (183 m) con **r = 0,40 m**, y ✅ 15/15 con **r = 0,50 m** (radio por defecto del CharacterController de Unity) |
| Los 6 pasillos se atraviesan en ambos sentidos | ✅ MSI, MSD, MLI, MLD, MII, MID |
| Cruce de puerta a puerta (A* entre puntos a 0,8 m a cada lado, puertas abiertas) | ✅ 6/6 puertas interiores (4 de salas + 2 técnicas), con r = 0,40 y 0,50 m |
| Los triggers cubren la aproximación por los dos lados (cápsula + *skin width* tocando la puerta cerrada) | ✅ 9/9 puertas; detección desde 1,54 m a cada lado |
| Sellado con las puertas cerradas | ✅ Vestíbulo norte, vestíbulo sur, zona técnica y núcleo+salas quedan separados |
| Núcleo → 6 salas con las puertas cerradas (los pasillos no tienen puertas) | ✅ 6/6 |
| Celdas transitables fuera del cerramiento / islas no conectadas | ✅ 0 / 0 m² (475 m² transitables y conectados) |
| Límite en las puertas exteriores (posición máxima alcanzable) | ✅ 14,575 m (antes del límite, situado a 15,0 m) en N_Principal, S_Salida y NE_Emergencia |
| Techo y escalones | ✅ 3,00 m libres en todo el recorrido; suelo plano (marcas de 6–8 mm, sin escalones) |
| Cobertura visual ↔ colliders | ✅ Tras la corrección, ninguna pieza visual queda sin collider en la zona transitable |
| Apertura de hojas (dónde queda la hoja abierta) | ✅ Puertas rectas: 99,4–99,5 % dentro del muro · salas (muro curvo, modo giro): 95,4 % |

### 1.3 Optimización para Unity
| Aspecto | Resultado |
|---|---|
| Polígonos | 39 488 tri (COMPLETA) · 32 706 (INTERIOR) · 2 540 (colliders) · 20 830 vértices visuales. El objeto más pesado es `MC_Pared_Circular` (2 574 tri) |
| Materiales | 16 en los modelos visuales, todos con nodos exportables (Principled BSDF + Image Texture + Normal Map), sin nodos procedurales · 369 submallas |
| Texturas | 11 PNG, todos potencia de 2 (512–2048), 1,1 MB en total; normal maps en *Non-Color*; todas existen y se referencian con rutas relativas |
| UV | 1 mapa UV por malla, ninguna malla sin UV. Densidad de texel: paredes ~513 px/m, suelo ~510, cubierta ~244, rejilla 512–1024 |
| Normales | Ninguna invertida (volumen con signo, tras re-importar). 39 mallas con normales personalizadas (Bevel + Weighted Normal) |
| Escala y orientación | 1 u = 1 m; FBX Y-up; ida y vuelta exacta (diferencia de cajas envolventes < 1 mm por objeto) |
| Jerarquía | 1 nivel (`GRP_*` → objetos); 15 / 14 / 1 raíces en COMPLETA / INTERIOR / COLLIDERS |
| Colliders | 116 mallas, máximo 180 tri (muros circulares), 78 cajas → BoxCollider y 38 MeshCollider no convexos. Ninguno supera 255 tri |
| Compatibilidad de materiales | Base color, metálico, suavidad, normal map y emisión se trasladan a URP/Lit o Standard (`CabinaDatos.cs`) |

### 1.4 Prueba de exportación (re-importación en Blender)
| Comprobación | COMPLETA | INTERIOR | COLLIDERS |
|---|---|---|---|
| Objetos que faltan o sobran | 0 / 0 | 0 / 0 | 0 / 0 |
| Mallas · triángulos | 233 · 39 488 | 203 · 32 706 | 116 · 2 540 |
| Geometría (triángulos por objeto) | idéntica | idéntica | idéntica |
| Caja envolvente por objeto (escala y posición) | < 1 mm | < 1 mm | < 1 mm |
| Materiales por objeto | idénticos | idénticos | — (debug) |
| Texturas enlazadas y presentes | 7 materiales con textura, 0 faltan | 6, 0 faltan | — |
| Jerarquía (padre de cada objeto) | idéntica | idéntica | idéntica |
| Propiedades personalizadas (tipo, puerta_id, recorrido…) | idénticas | idénticas | idénticas |
| Separación de cubierta | 30 `CUB_` | **0** `CUB_` | 0 |
| Objetos de colisión | 0 `COL_` | 0 `COL_` | 116 `COL_` |
| Cámaras y luces | 0 | 0 | 0 |
| Normales invertidas | 0 | 0 | 0 |

### 1.5 Scripts C# y preparación de Unity
| Prueba | Resultado |
|---|---|
| Compilación con Mono `mcs` contra stubs de UnityEngine/UnityEditor/InputSystem (`SCRIPTS_BLENDER/audit/csharp_check/`) | ✅ 0 errores, 0 avisos con 3 configuraciones: Input Manager clásico, Input System nuevo y ambos |
| Sentido de giro de las puertas de muro curvo calculado en ejes de Unity frente a la posición verificada en Blender | ✅ Coincide con 2 mm de diferencia |
| Datos de Unity generados del `.blend` (materiales, luces, puntos de paso) | ✅ `CabinaDatos.cs`: 16 materiales, 37 luces, 46 puntos de paso |
| QC original re-ejecutado sobre los archivos finales | ✅ 48/48 (`QC_REPORTE.md`) |

## 2. Pruebas pendientes

| Prueba | Motivo | Cómo hacerla |
|---|---|---|
| Compilar los scripts en el editor de Unity | No hay un editor de Unity en este entorno. El equipo conectado es Windows, pero su shell remota es una VM Linux sin Unity. Los stubs solo reproducen firmas | Copiar `UNITY_SCRIPTS/` a `Assets/` (README §4) |
| Importación real (ejes −x/z/−y, escala, propiedades en `CabinaPropiedades`) | Sin Unity | README §4. La prueba automática comprueba los ejes al empezar |
| Materiales URP/Standard (aspecto, emisión, normal maps) | Sin Unity | Menú *Cabina › 1* |
| Iluminación (37 luces aproximadas, lightmaps) | Sin Unity | Menú *Cabina › 2*; hornear si se desea |
| Colliders, triggers y puertas en tiempo de ejecución (eventos `OnTriggerEnter` con CharacterController, giro de las puertas de las salas) | Sin Unity | README §9, lista manual |
| Recorrido completo con la física de Unity | Sin Unity | Menú *Cabina › 3* → `CabinaPruebaRecorrido_resultado.txt` (esperado: 15/15) |
| Rendimiento en Unity (FPS, draw calls, batching) | Sin Unity | Profiler / Frame Debugger |
| Prueba de juego con jugador humano | Sin Unity | WASD + ratón (README §8) |
| HDRP | No cubierto por el configurador | Materiales manuales (README §5) |

## 3. Problemas encontrados

| # | Gravedad | Problema | Evidencia |
|---|---|---|---|
| P1 | Alta | Los **marcos de las 4 puertas exteriores de las salas** sobresalían 0,246 m sin collider: el jugador los atravesaba | Cobertura visual↔collider: `MARCO_M*_Exterior` 0,078 m² |
| P2 | Alta | Las **puertas exteriores (norte, sur, NE) daban al vacío**: fuera no hay suelo, así que con la puerta abierta el jugador podía salir del mapa y caer | Sin límite: el exterior era alcanzable en el QC 2D anterior |
| P3 | Alta (visual) | Las **hojas de las puertas de las salas** (planas sobre muro curvo) quedaban **un 91 % fuera del muro** al abrirse en línea recta, sobresaliendo hasta ~1 m hacia el vestíbulo y sin collider | `door_open_audit.py`: 8,7 % ocultas |
| P4 | Media | **Cantos de la cubierta** (generados por Solidify) con **UV degeneradas** en un material con normal map: 1 644 triángulos y 137,7 m². En Unity producen tangentes inválidas (manchas o sombreado roto) | Análisis de UV por material |
| P5 | Media | Pantallas del núcleo (0,095 m), paneles de acceso (0,117 m), marcos de pasillo (0,09 m) y nervios (0,08 m) sobresalían de sus colliders | Cobertura visual↔collider |
| P6 | Media | El README anterior indicaba un **color base gris en materiales con textura**: en Unity multiplicaría la textura y la oscurecería | Revisión de la tabla de materiales frente a los nodos |
| P7 | Media | No existían **Player Controller**, lógica de puertas, configuración de materiales ni luces en Unity (el FBX no lleva luces, así que la escena quedaría oscura) | Revisión de `UNITY_SCRIPTS/` |
| P8 | Baja | El postprocesador convertía **todos** los colliders en MeshCollider (también 78 cajas) y el FBX de colliders generaba un material | Revisión del C# |
| P9 | Baja | El QC anterior consideraba **alcanzable el exterior** porque solo comprobaba en 2D sin suelo; el criterio era incorrecto | `qc_cabina.py` |
| P10 | Info (aceptado) | Las consolas y terminales tienen colliders hasta 0,10 m mayores que su proyección visual; la franja de los nervios suma 0,08 m. Tiene un efecto mínimo y evita enganches | Cobertura collider↔visual |
| P11 | Info (aceptado) | Los dorsos de pantallas y señales tienen UV degeneradas, pero sin normal map y pegados a la pared, así que no se ven | Análisis de UV |

Otras observaciones: el ancho es de 26 m frente a los 20 m del plano (decisión documentada en el README §2). Las 6 vistas de
módulo son casi idénticas porque los módulos son copias giradas con el mismo mobiliario; se distinguen por su señalización.

## 4. Correcciones realizadas

| # | Corrección | Archivos afectados | Verificación |
|---|---|---|---|
| C1 (P1, P5) | **39 colliders de caja** `COL_Obs_Aud_*`: una caja por jamba en los marcos (sin cerrar el vano) y una franja junto al muro en pantallas, paneles y nervios | `.blend` (colección 16), `CABINA_COLLIDERS.fbx` | Cobertura: 0 piezas sin collider · recorrido 15/15 con r 0,40 y 0,50 m · cruce de las 6 puertas interiores OK |
| C2 (P2) | **3 límites de mapa** `COL_Limite_Mapa_*` (0,2 m, pegados a la cara exterior del muro delante de N_Principal, S_Salida y NE_Emergencia) | `.blend`, `CABINA_COLLIDERS.fbx` | Posición máxima alcanzable: 14,575 m; QC «exterior no alcanzable» OK |
| C3 (P3) | Modo **GirarSobreEje** en `CabinaPuertaAutomatica`: las 4 puertas de las salas giran ~31,4° alrededor del centro de su sala (centros generados en `CabinaDatos.PuertasGiratorias`) | `UNITY_SCRIPTS/` | En Blender, la hoja queda un 95,4 % oculta en el muro; el sentido calculado en Unity coincide con el de Blender (2 mm) |
| C4 (P4) | Los cantos del Solidify de las 22 piezas de cubierta usan `MAT_Estructura_Negro` (`material_offset_rim`), sin aplicar el modificador | `.blend`, `CABINA_NAVEGABLE_COMPLETA.fbx` | 0 caras con UV degenerada en materiales con normal map · re-importación idéntica |
| C5 (P6) | Tabla de materiales regenerada desde el `.blend` (blanco en los materiales con textura) y configuración automática URP/Standard | `README_UNITY.md`, `CabinaDatos.cs`, `CabinaConfigurador.cs` | Compilación OK; valores leídos de los nodos |
| C6 (P7) | Nuevos scripts: `CabinaControladorPrimeraPersona` (Input System nuevo y clásico), `CabinaPuertaAutomatica`, `CabinaPruebaRecorrido` (prueba automática en Unity) y menú *Cabina* (materiales, escena con jugador, puertas y 37 luces, prueba) | `UNITY_SCRIPTS/` | Compilación contra stubs OK (3 configuraciones) |
| C7 (P8) | `CabinaColliderUtil`: BoxCollider para cajas alineadas y MeshCollider para el resto; FBX de colliders sin materiales; propiedades con cultura invariante | `UNITY_SCRIPTS/Editor/` | Compilación OK; clasificación 78 / 38 coherente con el análisis |
| C8 (P9) | QC actualizado: los umbrales deben ser alcanzables y el exterior no | `SCRIPTS_BLENDER/qc_cabina.py`, `QC_REPORTE.md` | 48/48 |

Además, se añadieron 12 cámaras `CAM_AUD_*` al `.blend` (en la colección raíz; se mantienen exactamente las 16 colecciones).

**Incidencia en la propia herramienta de auditoría, corregida:** la primera versión del corte horizontal aplicaba la
regla par-impar a toda la malla, de modo que dos piezas solapadas (hoja + tira LED) se anulaban. Se cambió para resolver cada
pieza conexa por separado y se repitieron la navegación, la cobertura y la apertura de puertas. Los resultados de este informe
son los de la versión corregida.

## 5. Archivos de evidencia

| Archivo | Contenido |
|---|---|
| `AUDITORIA/00_Comparativa_Plano_vs_Modelo.png` | Plano y vista superior lado a lado |
| `AUDITORIA/mapa_navegacion_auditoria.png` | Espacio transitable, colliders, puertas, triggers y recorrido A* por tramos |
| `AUDITORIA/nav_resultados.json` | Tramos, holguras, puertas, triggers, sellado y cobertura |
| `AUDITORIA/nav_resultados_ANTES_correccion.json` | Mismo análisis antes de corregir los colliders (incluye P1 y P5) |
| `AUDITORIA/opt_export_resultados.json` | Optimización y comparación objeto por objeto de los 3 FBX |
| `AUDITORIA/puertas_apertura.json` | Posición de cada hoja abierta (lineal frente a giro) |
| `AUDITORIA/nav_waypoints_unity.json` | Puntos de paso en coordenadas de Blender y de Unity |
| `QC_REPORTE.md` / `RENDERS/QC_Mapa_Navegacion.png` | QC automático (48/48) |
| `SCRIPTS_BLENDER/audit/` | Todos los scripts de esta auditoría, reproducibles |
