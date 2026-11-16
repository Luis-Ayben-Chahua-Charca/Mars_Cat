# 04 — Economía y balance

**Estado:** borrador
**Decidido en:** #__
**Pendientes:** ver la sección "Pendientes" al final.

Los recursos y edificios están en `03_sistemas.md`. Esta página describe cómo fluyen. Las cifras son valores de diseño y pasan a `MarsCat/Assets/Data/` cuando Lógica cree el archivo de balance.

## Ciclo de día y noche

- Hay **día y noche**. Los gatos trabajan de día; de noche descansan.
- De noche la **energía** se consume para **mantener la temperatura** de la base.
- El ciclo de día y noche **entra en el PMV**.
- **Emergencia (post-PMV):** el jugador puede activar **sobretrabajo** nocturno en un edificio. Van primero los gatos **más saludables**; solo si no queda otra opción, los más enfermos. Los **degradados nunca**: morirían.
  - El sobretrabajo cuesta un poco de **salud**, sin importar el traje. Afuera, se suma al daño por radiación si el traje está roto.

## Consumo, hambre y sed

### Primera tanda de valores (acordado)

| Valor | Cantidad | Fuente |
|---|---|---|
| Necesidad diaria de un gato | ~250 kcal por sol | Tierra: energía en reposo de un gato de 4 kg (70 × peso^0,75 ≈ 200 kcal) × 1,2–1,4 de mantenimiento |
| Ración | 1 ración = 125 kcal; 2 por sol | Derivado |
| Agua por gato | 0,2 L por sol (0,05 L por bebida, 4 por sol) | Tierra: ~50 mL por kg de peso al día (guía veterinaria; por verificar) |
| Oxígeno por gato (para el aire, #55) | ~0,08–0,1 kg por sol | Derivado: 0,835 kg por humano al día (BVAD) escalado por metabolismo (peso^0,75 ≈ 1/10) |
| Paneles | ~6 kWh por sol cada 10 m² | Derivado: irradiancia del 43 % de la Tierra y eficiencia del panel a la latitud de Vivero |
| Comida inicial | ~60 raciones | Diseño: unos 2 soles de margen para 15 gatos (regla 5 del PMV) |
| Agua inicial | ~6 L | Diseño: igual |

Tanques, baterías, calefacción nocturna y costos de reparación quedan como `placeholder` hasta probarlos.

- **Colonia inicial (acordado):** **15 gatos**. Cada uno despierta con un hambre y una sed iniciales algo distintas (aleatorias), para que no coman ni beban todos a la vez.

- La comida y el agua se consumen **en tiempo real**, con **estadísticas por gato** (ver `03_sistemas.md`).
- **Hambre y sed siguen la misma lógica**, cada una con sus propios tiempos:
  - El gato come o bebe cada cierto **intervalo**.
  - Pasado un umbral sin comer o beber salta una **alerta extrema**.
  - Pasado otro umbral, **muere**.
- **Orden:** un gato come o bebe cuando lo necesita. Si hay gatos en alerta extrema, van primero los más cercanos a morir.
- **Alerta extrema:** el gato no trabaja y busca comida o agua. Si estaba trabajando, apenas la haya deja lo que hace y va.
- Los tiempos se miden en **fracciones del ciclo de día y noche (sol)**, no en minutos fijos, para que escalen con la duración del ciclo.

### Referencia real (gatos en la Tierra)

Según Cats Protection (Reino Unido):

| Necesidad | Problemas serios | Supervivencia máxima |
|---|---|---|
| Comida (con agua) | A los 2 días sin comer | Hasta unas 2 semanas |
| Agua | Deshidratación en 24 horas | Unos 3–4 días |

El agua es cerca de **4 veces más urgente** que la comida.

### Duración del sol (acordado)

- **1 sol = 8 minutos reales:** 6 minutos de día y 2 minutos de noche.
- De noche el tiempo del juego corre **3 veces más rápido**. Así día y noche duran lo mismo en tiempo de juego (como en Marte, cerca del ecuador), pero la noche, en la que los gatos no trabajan, no se vuelve aburrida.
- Referencias de otros juegos: un día de *Don't Starve* dura 8 minutos y uno de *RimWorld*, unos 17 minutos a velocidad normal.
- Con 8 minutos por sol, el tutorial cabe en 1 o 2 soles (8–16 minutos), una duración razonable para una demo.
- El hambre y la sed siguen corriendo de noche: los gatos descansan, pero comen y beben. Sus tiempos se cuentan en **minutos reales** (los de la tabla de abajo); la aceleración nocturna solo afecta al reloj del juego y al gasto de energía de la noche.

### Tiempos de hambre y sed (acordado)

El agua es el doble de urgente que la comida. Entre la alerta y la muerte queda un **margen de reacción** suficiente para que el jugador actúe: 2 minutos para la comida y 1 minuto para el agua.

| Necesidad | Intervalo | Alerta extrema | Muerte | Margen de reacción |
|---|---|---|---|---|
| Comida | 0,5 sol (4 min) | 1 sol (8 min) | 1,25 sol (10 min) | 2 min |
| Agua | 0,25 sol (2 min) | 0,5 sol (4 min) | 0,625 sol (5 min) | 1 min |

Para que sea divertido, la producción de cada edificio reparado debería superar el consumo de la colonia por sol con margen. El jugador debe notar el progreso apenas un edificio funciona.

## Almacenamiento

- **Energía y agua:** almacenamientos independientes, con tope (el agua, en **tanques**).
- **PMV (acordado):** cada recurso tiene su **propio tope**. El tope compartido del depósito queda para después del PMV.
- **Oxígeno:** no es un tanque, es el **aire de la base**, medido en % de O₂ (ver "Aire y oxígeno"). El tope crece con el árbol tecnológico (en energía, con la investigación nuclear). El tope de energía obliga a repartirla bien, porque la solar es limitada y fluctuante.
- **Energía sin merma:** no se pierde con el tiempo. Hay **baterías de emergencia** reparables.
- **Raciones, forraje, fibra, escombros y desechos:** físicos y visibles, en un **almacén compartido** (un solo edificio: el **depósito**, ver abajo). El jugador debe vigilar qué está acumulando y qué necesita.
- **Exceso:** si el almacén está lleno, la producción se detiene y queda **en cola**; el jugador debe permitir a mano que entre al almacén.

## El depósito

Edificio principal en la zona segura (la cabina principal, junto con el MOXIE y las esclusas). Es donde sobrevivieron los gatos congelados y donde están al inicio la comida, el agua y los materiales. Los gatos **comen aquí**, y los **trajes iniciales** se guardan aquí hasta que exista el taller (post-PMV). Los gatos heridos que entran por una esclusa se recuperan ahí.

## Fuentes

1. **Reservas iniciales** de materiales.
2. **Escombros** de los edificios destruidos.
3. **Nodos de mineral marciano** cerca, en la cueva o en la superficie. Los minerales salen siempre de datos de NASA. El **regolito** es infinito. El **metal** se extrae de Marte y también se recupera de los escombros.
4. **Edificios:**
   - Granja de plantas (papa): **forraje** para las ratas, **fibra** y algo de oxígeno. Los gatos no comen de aquí: son carnívoros (ver "Cadena alimentaria" en `03_sistemas.md`).
   - Granja de ratones: **raciones de rata** (alimento, proteína) a partir de las ratas capturadas en "La Redada", que comen forraje y se reproducen (ritmo en `03_sistemas.md`, "Ritmo de la granja de ratas"), y **desechos de ratones** (ver "Granja de ratones y raciones" en `03_sistemas.md`).
   - Reciclador orgánico: **abono**, a partir de los desechos de gatos y de ratones y de los restos de cosecha.
   - Energía: electricidad.
   - **Extracción de agua del subsuelo:** agua de hielo purificado (ver "Agua" en `03_sistemas.md`).
   - **Reciclador de agua (post-PMV):** recupera parte del agua consumida; dañado al inicio. Su eficiencia sube por niveles en el árbol tecnológico.
   - **MOXIE** (edificio industrial, PMV): fuente principal de oxígeno; depende de la electricidad y de sus filtros.

Para reparar un edificio caído, los escombros cubren cerca del **90 %** del costo; el **10 %** restante son minerales marcianos.

## Transformación

En un edificio se juntan los insumos y sale el producto; el gato solo lo trabaja. Las cadenas acordadas (por ejemplo, el taller) están en `03_sistemas.md`.

## Sumideros

- **Pasivos (por ciclo):**
  - Oxígeno: según la cantidad de gatos y cuánto trabajan, más una pequeña **fuga por cada edificio** presurizado.
  - Comida: según la cantidad de gatos.
  - **Desechos:** cada gato genera desechos según lo que come y bebe, y la granja de ratones suma los suyos. Se **almacenan en el depósito** y ocupan espacio hasta que aparece el reciclador orgánico y los procesa. Si el depósito se llena de desechos, los gatos pierden salud de forma gradual.
  - Energía: según los edificios activos y, de noche, para mantener la temperatura.
  - La **protección** de los edificios está siempre activa y siempre consume energía.
- **Activos:** la **reparación del beacon** para enviar la alerta a la Tierra.
- **Construcción de edificios nuevos o mejorados (post-PMV):** consume **chips**, lo que mantiene activa la cadena mina → taller (ver `03_sistemas.md`).
- **Mantenimiento del reactor (post-PMV):** las correcciones de seguridad gastan materiales; si se descuidan, hay riesgo de accidente nuclear (ver `03_sistemas.md`).

## Entropía y fallos en cascada

- **Radiación permanente:** degrada edificios y trajes.
- **Edificios:** tienen un **escudo**. Si se pierde, el interior se vuelve peligroso para los gatos (cambios bruscos de temperatura); si no se repara a tiempo, el edificio cae total o parcialmente. Los fenómenos del desierto abren pequeñas **grietas** que dañan la estructura.
- **Sin energía**, cuando se agotan las baterías, los edificios dejan de funcionar y arrastran al resto, en especial la protección contra radiación y el oxígeno. Con el tiempo faltan los demás recursos y los gatos mueren.
- **Prioridad de energía (acordado):** cuando no alcanza, los edificios se apagan por orden de importancia:

| Prioridad | Edificios | Se apaga |
|---|---|---|
| Máxima | MOXIE (oxígeno) y calefacción nocturna | Último |
| Alta | Extracción de agua, taller y depósito | Después de los sin prioridad |
| Sin prioridad | Reciclador orgánico (desechos), granja de ratones y granja de plantas (cosechas) | Primero |

- **Válvula de emergencia de energía:** reparar y reconectar paneles lejanos, a costa de recursos.
- **Sin comida**, hay menos gatos trabajadores.
- Los únicos recursos externos son los nodos de mineral y la energía del sol.

## Gatos heridos

Un gato herido fuera de la base puede ser **auxiliado** a costa de un gato libre; si llega a una esclusa, va al **depósito**. Detalle en `03_sistemas.md`. El **ala médica** es una propuesta fuera del PMV.

## Aire y oxígeno

Modelo propuesto en "Oxígeno" de `03_sistemas.md`: barra de % de O₂ con meta de 21 %, cambio por tick = (producción − consumo − fugas) ÷ volumen de la base. Más edificios dan más **reserva** (el % cambia más lento) pero suman **fugas**; el consumo depende de los gatos.

## Asignación de trabajo

Automática, al estilo de *RimWorld* u *Oxygen Not Included*. Hay dos tipos de trabajo:

| Tipo | Ejemplos | Duración |
|---|---|---|
| **Puesto fijo** | Operar un edificio de producción | Mientras el edificio funcione |
| **Tarea puntual** | Reparar, limpiar filtros del MOXIE, auxiliar a un gato caído, jugar un minijuego de preparación | Hasta terminar la tarea |

- Si hay gatos libres y puestos libres, se asignan solos.
- **Las tareas puntuales tienen prioridad** sobre los puestos fijos. Tienen dos niveles (acordado): **vital** (auxiliar a un gato herido, post-PMV) y **normal** (todas las demás).
- **Sin gatos libres:** la tarea toma un gato de un puesto fijo según la **importancia del edificio**, la misma de la prioridad de energía. El edificio rinde menos mientras falta ese gato: ese es el costo real de la tarea. Acordado:
  - Tarea normal: solo saca gatos de edificios sin prioridad; si no hay, espera.
  - Tarea vital: saca primero de los sin prioridad, después de los de prioridad alta y, como último recurso, de los de prioridad máxima.
- **Liberación manual:** al iniciar la tarea, el jugador puede elegir **de qué edificio** sale el gato en lugar de aceptar el automático. No hay asignación manual por edificio como en *Frostpunk*.
- Si no hay ningún gato que pueda hacerla (todos en alerta extrema, degradados para el exterior o muertos), el botón queda desactivado y la IA lo explica.
- Al terminar la tarea, el gato vuelve a estar libre y el asignador lo devuelve a un puesto.
- La construcción y las tareas se hacen **de día**. De noche no se trabaja, salvo sobretrabajo de emergencia (ver arriba).

## Escalado

- Más edificios consumen más energía y exigen más trabajo y más salidas al exterior, con más exposición y más trajes.
- El **árbol tecnológico** mejora la producción de alimentos, la eficiencia energética y la producción de trajes.
- Recursos intangibles: solo **salud**. No hay moral ni esperanza.

## Segunda tanda de valores (acordado)

Cierra los valores del PMV que estaban en `placeholder`. Fuentes y cálculos en `05_datos_nasa.md` ("Segunda tanda" y "Cuando no hay dato"); el sistema de cada uno, en `03_sistemas.md`. La etiqueta es la que va en la columna `fuente` del CSV (#22).

| Sistema | Valor | Cantidad | Clave del CSV | Fuente |
|---|---|---|---|---|
| Aire | Volumen inicial (refugio sellado) | 25 m³ | `air.initial_volume_m3` | `diseno` |
| Aire | % de O₂ inicial | 0,21 | `air.initial_fraction` | `nasa` (ISS) |
| Aire | Consumo por gato en reposo / trabajando | 0,08 / 0,10 kg de O₂ por sol | `air.cat.rest_kg_per_sol`, `air.cat.work_kg_per_sol` | `derivado` |
| Aire | Densidad del aire | 1,2 kg/m³ | `air.density` | `tierra` |
| Aire | Volumen y fuga que suma cada edificio reparado | 25 m³; 0,1 kg de O₂ por sol | `building.<id>.volume_m3`, `building.<id>.leak_kg_per_sol` | `diseno` (la fuga real de la ISS es mucho menor; licencia por los daños) |
| Aire | % de O₂ de un módulo recién presurizado | 0,16 | `air.new_module_fraction` | `diseno` |
| Aire | Margen en crítico antes de morir | 60 s reales | `air.critical_seconds` | `diseno` (como el agua) |
| MOXIE | Producción con 5 filtros limpios | 2 kg de O₂ por sol | `building.moxie.production_rate_per_sol` | `derivado` (Hecht, 2022) |
| MOXIE | Electricidad | 20 kWh por sol | `building.moxie.energy_rate_per_sol` | `derivado` (~10 kWh por kg) |
| MOXIE | Un filtro se ensucia cada / limpiar un filtro | 360 s de juego (0,5 sol) / 5 s reales | `minigame.moxie.dirty_seconds`, `minigame.moxie.clean_seconds` | `diseno` |
| Energía | Paneles | 60 m² → 36 kWh por sol | `building.solar_panels.production_rate_per_sol` | `derivado` (6 kWh cada 10 m²) |
| Energía | Calefacción nocturna | 10 kWh por noche | `energy.heating.rate_per_sol` | `diseno` (~30 % del día; noche de −90 °C) |
| Energía | Presupuesto | 36 − 20 (MOXIE) − 10 (noche) = 6 kWh por sol para el resto | — | Cuenta de control |
| Agua | Veta fija del PMV | 5 L de hielo, pureza 0,6 → 3 L de agua por lote | `minigame.water.vein.liters`, `minigame.water.vein.purity` | `derivado` (NASA: 50–85 % de hielo) |
| Agua | Energía por lote | 1,5 kWh | `minigame.melt.energy_kwh` | `derivado` (0,3 kWh por litro de hielo) |
| Granja de plantas | Área y forraje | 20 m² → 0,42 kg por sol | `building.plant_farm.production_rate_per_sol` | `nasa` (BVAD, 21 g/m² al día) |
| Granja de plantas | Fibra / restos de cosecha | 0,08 / 0,10 kg por sol | `building.plant_farm.byproduct.fiber_per_sol`, `…crop_residue_per_sol` | `derivado` (0,18 kg no comestibles, repartidos) |
| Fábrica de Suelo | Lote / perclorato inicial / umbral | 0,25 kg / 0,5 % / 0,1 % | `minigame.soil.wash.batch_kg`, `.initial_percent`, `.threshold_percent` | `nasa` (Phoenix) y `tierra` (Oze et al.); lote: `diseno` |
| Fábrica de Suelo | Agua de lavado | Guía 1 L, rango 1–2 L | `minigame.soil.wash.guide`, `.min`, `.max` | `derivado` |
| Fábrica de Suelo | Mezcla: lote y abono | 1 kg; guía 20 %, rango 0–30 % (0,2 kg con la guía) | `minigame.soil.mix.batch_kg`, `.compost.*` | `diseno` / acordado |
| Fábrica de Suelo | Bacterias | Guía 5 %, rango 1–10 % | `minigame.soil.mix.bacteria.*` | `diseno` |
| Abono | Reserva inicial | 1 kg (5 preparaciones) | `resource.compost.initial` | `diseno` |
| Ratas | Desechos con cama | 0,02 kg por rata y por sol | `building.rat_farm.waste_per_rat_per_sol` | `estimacion` |
| Ratas | Captura / caza | 15 s / 30 s reales | `rat.capture_seconds`, `rat.hunt_seconds` | `diseno` |
| Gatos | Desechos | 0,018 kg por comida (36 g por día) | `cat.waste_per_meal` | `estimacion` (confianza baja) |
| Reciclador | Lote y rendimiento | 1 kg de mezcla → 0,6 kg de abono | `minigame.compost.batch_kg`, `minigame.compost.yield_ratio` | `tierra` |
| Reciclador | Duración del ciclo | 60 s reales | `minigame.compost.cycle_seconds` | `diseno` (en la Tierra, 30–45 días; licencia) |
| Reciclador | Tope de desechos / salud con el depósito lleno | 3 kg / −5 % de salud por sol | `resource.cat_waste.capacity`, `cat.waste_full_health_per_sol` | `diseno` |
| Edificios | Duración de un ciclo de funcionamiento | 720 s de juego (1 sol) | `building.<id>.run_seconds` | `diseno` |
| Edificios | Reparación | 9 escombros + 1 metal; 10 s reales | `building.<id>.repair_debris_cost`, `…repair_metal_cost`, `…repair_seconds` | `diseno` (90 % / 10 %, acordado) |
| Reservas | Escombros / metal iniciales | 60 / 6 | `resource.debris.initial`, `resource.metal.initial` | `diseno` (alcanza para todo el tutorial) |
| Topes | Agua / electricidad / raciones / forraje | 10 L / 20 kWh / 100 / 2 kg | `resource.<id>.capacity` | `diseno` |

**Controles de cuentas:**

- **Aire:** 25 m³ × 1,2 × 0,05 = 1,5 kg de O₂ entre 21 % y 16 %, lo que consumen los 15 gatos en ~1 sol. El MOXIE da 2 kg: cubre a los gatos y las fugas de los primeros módulos.
- **Agua:** 6 L iniciales y 3 L por sol de consumo; el extractor da 3 L por ciclo de 1 sol. Cada preparación de la granja gasta 1 L más: el agua queda justa a propósito.
- **Comida:** 60 raciones iniciales (2 soles); la granja de ratas llega a ~28 raciones por sol (ritmo acordado).

## Pendientes

1. Topes de almacenamiento y tamaño del lote (el documento antiguo menciona 15 unidades por lote de plantas como ejemplo).
2. Consumo por gato y por edificio.
3. Tope compartido del depósito y cola de exceso: post-PMV (en el PMV, tope por recurso; acordado).
5. Producción por sol de cada edificio frente al consumo de la colonia.
6. Cuánta salud cuesta el sobretrabajo (post-PMV).
