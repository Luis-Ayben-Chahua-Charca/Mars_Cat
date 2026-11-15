# QC – Cabina navegable

Resultado global: **TODAS LAS COMPROBACIONES SUPERADAS** (48/48)

Triángulos (modificadores aplicados): COMPLETA 39,488 · INTERIOR 32,706

| Sección | Comprobación | Estado | Detalle |
|---|---|---|---|
| Blend | El proyecto .blend se abre sin errores | OK | CABINA_NAVEGABLE.blend |
| Blend | Colecciones 01–16 presentes y en orden | OK | 01_ESTRUCTURA_EXTERIOR, 02_SUELOS, 03_MODULO_CENTRAL, 04_MODULO_SUPERIOR_IZQUIERDO, 05_MODULO_SUPERIOR_DERECHO, 06_MODULO_LATERAL_IZQUIERDO, 07_MODULO_LATERAL_DERECHO, 08_MODULO_INFERIOR_IZQUIERDO, 09_MODULO_INFERIOR_DERECHO, 10_PASILLOS, 11_ZONA_TECNICA, 12_PUERTAS_Y_ACCESOS, 13_ILUMINACION, 14_MOBILIARIO, 15_CUBIERTA_REMOVIBLE, 16_COLLIDERS_UNITY |
| Blend | Texturas externas encontradas (rutas relativas //TEXTURAS/) | OK | 11 imágenes; faltan: [] |
| Blend | Escala: 1 unidad = 1 m (sistema métrico) | OK |  |
| Distribución | Exactamente 7 módulos circulares | OK | MC_Pared_Circular, MID_Pared_Circular, MII_Pared_Circular, MLD_Pared_Circular, MLI_Pared_Circular, MSD_Pared_Circular, MSI_Pared_Circular |
| Distribución | Diámetro MC_Pared_Circular | OK | 8.000 x 8.000 m (esperado 8.0) |
| Distribución | Diámetro MID_Pared_Circular | OK | 6.000 x 6.000 m (esperado 6.0) |
| Distribución | Diámetro MII_Pared_Circular | OK | 6.000 x 6.000 m (esperado 6.0) |
| Distribución | Diámetro MLD_Pared_Circular | OK | 6.000 x 6.000 m (esperado 6.0) |
| Distribución | Diámetro MLI_Pared_Circular | OK | 6.000 x 6.000 m (esperado 6.0) |
| Distribución | Diámetro MSD_Pared_Circular | OK | 6.000 x 6.000 m (esperado 6.0) |
| Distribución | Diámetro MSI_Pared_Circular | OK | 6.000 x 6.000 m (esperado 6.0) |
| Pasillos | Ancho libre PAS_MSI | OK | 2.000 m; tramo s=3.81…5.75 m (núcleo r=4.0 → sala r=3.0 a 8.5 m) |
| Pasillos | Ancho libre PAS_MSD | OK | 2.000 m; tramo s=3.81…5.75 m (núcleo r=4.0 → sala r=3.0 a 8.5 m) |
| Pasillos | Ancho libre PAS_MLI | OK | 2.000 m; tramo s=3.81…5.75 m (núcleo r=4.0 → sala r=3.0 a 8.5 m) |
| Pasillos | Ancho libre PAS_MLD | OK | 2.000 m; tramo s=3.81…5.75 m (núcleo r=4.0 → sala r=3.0 a 8.5 m) |
| Pasillos | Ancho libre PAS_MII | OK | 2.000 m; tramo s=3.81…5.75 m (núcleo r=4.0 → sala r=3.0 a 8.5 m) |
| Pasillos | Ancho libre PAS_MID | OK | 2.000 m; tramo s=3.81…5.75 m (núcleo r=4.0 → sala r=3.0 a 8.5 m) |
| Pasillos | Los 6 pasillos conectan físicamente núcleo y salas (54 rayos núcleo→centro de sala, sin impacto) | OK | 0 impactos en 54 rayos (offsets ±0.9 m, alturas 0.3/1.0/1.8 m) |
| Cerramiento | Sin aberturas accidentales (rayos desde 23 puntos interiores, modelo completo, puertas cerradas) | OK | 41400 rayos, 0 escapes |
| Mallas | Mallas cerradas (sin aristas no-manifold) | OK | 349 mallas; abiertas: [] |
| Mallas | Sin normales invertidas (volumen con signo > 0 por isla) | OK | invertidas: [] |
| Mallas | Sin caras duplicadas | OK | [] |
| Mallas | Sin caras de área cero | OK | [] |
| Mallas | Sin objetos duplicados superpuestos | OK | [] |
| Mallas | Todas las caras tienen material asignado (sin huecos de material) | OK | [] |
| Transformaciones | Escala aplicada (1,1,1) en todos los objetos | OK | [] |
| Transformaciones | Ubicación/rotación aplicadas en geometría estática; solo objetos interactivos conservan pivote (giro en Z) | OK | 126 objetos con pivote propio (INT_/PUERTA_/COL_PUERTA_/COL_TRG_/COL_Obs_); incorrectos: [] |
| Cubierta | Cubierta 100 % en 15_CUBIERTA_REMOVIBLE, sin objetos compartidos ni hijos ajenos | OK | 30 piezas de cubierta |
| Cubierta | Capas de vista: MODELO_COMPLETO (cubierta visible) y MODELO_INSPECCION (solo cubierta excluida) | OK | {"MODELO_COMPLETO": ["16_COLLIDERS_UNITY"], "MODELO_INSPECCION": ["15_CUBIERTA_REMOVIBLE", "16_COLLIDERS_UNITY"]} |
| FBX | Cabecera FBX: eje vertical Y+ (Unity), frontal Z, coordenadas X; versión y unidades | OK | FBX 7400; {'UpAxis': 1, 'UpAxisSign': 1, 'FrontAxis': 2, 'FrontAxisSign': 1, 'CoordAxis': 0, 'CoordAxisSign': 1, 'UnitScaleFactor': 100.0} |
| FBX | Orientación: norte del plano (Blender +Y) = −Z en el espacio FBX/Unity | OK | PUERTA_N_Principal_Hoja_Izq Lcl Translation (FBX) = (-1.86, 0.0, -14.9) → (-1.86, 0.0, -14.9) m |
| FBX | CABINA_NAVEGABLE_COMPLETA.fbx: re-importado, mismos objetos que la colección de origen | OK | 276 objetos (233 mallas), 39,488 triángulos; faltan [] sobran [] |
| FBX | CABINA_NAVEGABLE_COMPLETA.fbx: sin cámaras ni luces | OK | [] |
| FBX | CABINA_NAVEGABLE_COMPLETA.fbx: escala real y eje vertical tras ida y vuelta | OK | EXT_Muro_Perimetral: 26.00 × 30.00 × 3.00 m (X × Y × Z); extensión total 26.00 × 30.06 × 4.20 m |
| FBX | COMPLETA: materiales y texturas referenciadas (relativas a TEXTURAS/) | OK | 16 materiales, 11 texturas; faltan [] |
| FBX | Propiedades personalizadas exportadas (p. ej. tipo/puerta_id en puertas) | OK | {'tipo': 'puerta_automatica', 'puerta_id': 'N_Principal', 'eje_apertura_local': 'X', 'sentido_apertura': -1, 'apertura_dir_blender': '-1.000,-0.000,0.000', 'apertura_dir_unity': '1.000,0.000,0.000', 'recorrido_m': 1.47, 'ancho_libre_m': 3.0, 'alto_libre_m': 2.6} |
| FBX | Objetos interactivos presentes y separados | OK | 92 objetos INT_/PUERTA_ |
| FBX | CABINA_NAVEGABLE_INTERIOR.fbx: re-importado, mismos objetos que la colección de origen | OK | 245 objetos (203 mallas), 32,706 triángulos; faltan [] sobran [] |
| FBX | CABINA_NAVEGABLE_INTERIOR.fbx: sin cámaras ni luces | OK | [] |
| FBX | CABINA_NAVEGABLE_INTERIOR.fbx: escala real y eje vertical tras ida y vuelta | OK | EXT_Muro_Perimetral: 26.00 × 30.00 × 3.00 m (X × Y × Z); extensión total 26.00 × 30.06 × 3.20 m |
| FBX | CABINA_COLLIDERS.fbx: re-importado, mismos objetos que la colección de origen | OK | 117 objetos (116 mallas), 2,540 triángulos; faltan [] sobran [] |
| FBX | CABINA_COLLIDERS.fbx: sin cámaras ni luces | OK | [] |
| FBX | CABINA_COLLIDERS.fbx: escala real y eje vertical tras ida y vuelta | OK | COL_Suelo: 26.00 × 30.00 × 0.20 m (X × Y × Z); extensión total 26.00 × 33.00 × 3.40 m |
| Navegación | Puertas abiertas: todo el nivel alcanzable desde el acceso norte (jugador r=0.3 m) | OK | 18 puntos de control alcanzados: salas, pasillos, núcleo, zona técnica, umbrales de acceso y salida |
| Navegación | Puertas abiertas: el exterior (sin suelo) no es alcanzable (COL_Limite_Mapa_*) | OK | Exterior norte, sur y NE bloqueados |
| Navegación | Puertas cerradas: el vestíbulo no conecta con exterior, zona técnica ni núcleo (cerramiento estanco) | OK | sin fugas |
| Navegación | Puertas cerradas: desde el núcleo se alcanzan los 6 pasillos y las 6 salas (colliders no bloquean accesos) | OK | 12/12 espacios alcanzados |
