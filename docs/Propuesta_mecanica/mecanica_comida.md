# Mecánica: Comida

> **Adaptación acordada (`docs/diseno/03_sistemas.md`, "Cadena alimentaria"):** donde esta propuesta contradiga lo acordado, prevalece `docs/diseno/`.
> - La **granja de plantas sí está en el PMV**, con **papa** como cultivo. Produce **forraje** (alimento de las ratas), **fibra** (para trajes) y restos de cosecha. Los gatos no comen plantas.
> - Las **ratas comen forraje**; la granja de ratones da **raciones** a los gatos.
> - Hambre en tiempo real (alerta a los 8 minutos, muerte a los 10), **15 gatos**, sin moral ni catnip. Caducidad, taurina y plaga quedan para después del PMV.
> - **Fábrica de Suelo (adaptada):** en el PMV, lavar el regolito (perclorato) y mezclar (abono 20 %, rango 0–30 %, y bacterias). Los tanques de cianobacterias no entran; la luz LED (12 h, papa) es post-PMV.
> - **La Redada (PMV):** es la preparación de la granja de ratones. El jugador marca con un clic las ratas sueltas (finitas) y un gato libre las captura. Los gatos con hambre extrema cazan ratas sueltas. Cuando se acaban, la comida escasea de verdad.
> - **Post-PMV:** persecución, plaga, fugas nocturnas y daños a otros edificios. El Criadero queda descartado.

## 1. Descripción General
> **Resumen:** Es el alimento de los gatos. Recurso de urgencia lenta (días, no minutos): el hambre no mata rápido como la falta de aire, pero vuelve lentos a los gatos y arrastra a todas las demás mecánicas. En el MVP no existe ningún cultivo vivo: los gatos sobreviven de las reservas que dejaron los humanos, y el arco del juego es pasar de herederos a ganaderos (ratas) y, después del MVP, a agricultores. Enseña dos ideas STEM centrales: la jerarquía de supervivencia (aire > agua > comida) y la cadena trófica (cultivos → ratas → gatos).

- **Tipo de Recurso:** Tangible (raciones almacenables)
- **Visibilidad:** Barra global de raciones en la interfaz + estado de hambre por gato

---

## 2. Bucle Económico (Inputs y Outputs)

> **Cadena del MVP:** despensa + pellets + reservas almacenadas → ratas → gatos. Nada se planta; todo se hereda, se gestiona o se captura.

### Fuentes de Generación (Cómo se obtiene)
* **Despensa de los humanos:** raciones empaquetadas, finitas; parte quedó destruida por la lluvia de meteoritos (reloj narrativo inicial).
* **Reservas de almacén frío:** verduras ya cosechadas por los humanos (lechuga, tomate). Son comida guardada, no plantas: no se cultivan ni se replantan en el MVP.
* **Pellets de laboratorio:** el alimento de los animales de experimento. Stock finito; es la comida principal de la granja de ratas en el MVP.
* **Suplementos de taurina:** del laboratorio. Los gatos son carnívoros obligados (sin taurina, que solo está en la carne, enferman); estos suplementos son el puente hasta que la granja produzca.
* **Granja de ratas (MVP):** convierte pellets y sobras en proteína felina. Las ratas son omnívoras (por eso son plaga): comen pellets, cáscaras, restos y lo caducado. La IA procesa fuera de cámara y entrega "barras de comida".
* **Hierba gatera seca (catnip):** frasco dejado por los científicos (sabían que tendrían gatos). Consumible de moral, finito, no se cultiva.
* **Post-MVP:** cultivos propios (ver sección 6) en el suelo curado y en la base de hidroponía cercana.
* **Dependencias:** la granja consume pellets/sobras y horas de gato; el minijuego de suelo consume **agua** y **energía**; los cultivos post-MVP consumen agua, energía y **suelo curado** (Minijuego 1). El trigo post-MVP aporta **oxígeno** al soporte vital: 56 g de O₂ por m² al día según la NASA (BVAD, tabla 4.2.8) — coordinar con mecánica Oxígeno.

### Sumideros y Consumo (En qué se gasta)
* **Consumo activo:** cada gato come a diario (~250 kcal/sol ≈ 1 rata + suplemento). Escala con gatos despiertos.
* **Alimentar la granja:** cada rata come ~20 g/sol (pellets y sobras en el MVP; grano después). Escala con la población: más ratas = más comida futura, pero más consumo hoy.
* **Caducidad:** las reservas frescas del almacén se echan a perder en pocos soles; las barras procesadas duran más.
* **Plaga:** las ratas sueltas muerden las reservas y los cables (ver Eventos Ambientales).

---

## 3. Almacenamiento y Entropía

- **Límites de Almacenamiento:** la despensa tiene capacidad fija. Lo fresco caduca rápido; procesar en barras extiende la duración pero requiere la granja activa.
- **Pérdida Gradual (Merma/Entropía):** Sí — caducidad de frescos y daño de plaga.
- **Cuellos de Botella:** *early game*: todo lo comestible del arranque se agota o se echa a perder; no hay forma de producir hasta fundar la granja. *Mid game*: equilibrar la población de ratas (deslizador de parejas reproductoras) contra el stock menguante de pellets — la IA avisa: "los pellets alcanzan para X soles; necesitamos cultivos propios". Crecimiento exponencial vs. capacidad de carga: ecología real en una perilla.

---

## 4. Fallos en Cascada (Consecuencias de llegar a 0)

> **¿Qué pasa cuando este recurso se agota?**

1. **Efecto Inmediato:** gatos hambrientos se mueven y trabajan más lento.
2. **Efecto Secundario:** menos velocidad = menos mantenimiento y reparaciones en todas las demás mecánicas.
3. **Espirales de Muerte:** hambre → lentitud → fallos acumulados en energía/oxígeno → colapso general. Más lenta que la espiral del oxígeno: así el jugador aprende la jerarquía de urgencias.
4. **Válvulas de Emergencia:** racionamiento (la IA reduce porciones a costa de moral y velocidad); re-criogenizar a un gato para reducir consumo (decisión dura, coherente con la historia); sacrificar parte de la granja (comida hoy, menos producción mañana).

---

## 5. Respaldo Científico / NASA (Space Apps Challenge)

* **Datos reales de cultivos (NASA BVAD, tablas 4.2.6 a 4.2.8):**

| Cultivo | Comida seca (g/m²/día) | Parte no comestible (g/m²/día) | Oxígeno (g/m²/día) | Agua (kg/m²/día) | Ciclo (días) |
|---|---|---|---|---|---|
| Trigo | 20,00 | 30,00 | 56,00 | 11,79 | 79 |
| Papa | 21,06 | 9,03 | 32,23 | 2,88 | 132 |
| Soya | 4,54 | 6,80 | 13,91 | 2,88 | 97 |
| Lechuga | 6,57 | 0,73 | 7,78 | 1,77 | 28 |
| Tomate | 10,43 | 12,74 | 26,36 | 2,77 | 85 |

Consumo humano de referencia (tabla 3.3.6): 0,835 kg de O₂, 0,998 kg de CO₂ y 0,617 kg de comida seca por persona al día.

* **Justificación Real:**
Cadena trófica: los gatos son carnívoros obligados — sin proteína animal y taurina enferman — lo que convierte la granja de ratas en la solución biológicamente correcta, y enseña la pirámide alimenticia y la pérdida de energía entre niveles. Las ratas son omnívoras y de ciclo rapidísimo (gestación de 21 días, adultas en 6 semanas): el ganado más veloz posible. Con los datos del BVAD, 1 m² de trigo rinde 20 g de grano seco al día, justo lo que come una rata: **~1 m² de trigo por rata**, así que una granja de 60–100 ratas pide 60–100 m² de cultivo, mientras que el oxígeno de los 5 gatos se cubre con ~7,5 m². Esa diferencia **es** la pérdida de energía de la cadena trófica, en números.
Prioridad del trigo: produce 56 g de O₂ por m² al día, casi el doble que la papa, pero consume 11,79 kg de agua por m² al día, cuatro veces más — más aire y grano a cambio de más agua: la decisión con costo que pide el reto. Además deja 30 g de paja por m² al día (alimento de hongos y compost). La soya rinde solo 4,54 g, lo que justifica dejarla como opcional.
Oxígeno: 835 g por persona al día ÷ 56 g por m² de trigo = **~15 m² por humano**; asumiendo que un gato respira ~1/10 de un humano (estimación propia), **~7,5 m² de trigo cubren el aire de los 5 gatos** — cruce directo con la mecánica de Oxígeno (aplica post-MVP).
Cultivos espaciales reales que respaldan el post-MVP: la lechuga roja de la ISS (Veggie/VEG-01) resultó segura y nutritiva; los chiles de Plant Habitat-04 (2021) se cosecharon y comieron en órbita; la soya completó su ciclo en la ISS (2002); la papa fue cultivada en suelo análogo marciano del desierto peruano de La Joya, dentro de un CubeSat que simulaba la presión, los gases y los ciclos de temperatura de Marte: un experimento del Centro Internacional de la Papa (CIP) y la universidad UTEC, en Lima, con asesoría de NASA Ames — carta peruana del proyecto. Moral: la NASA cultivó zinnias (flores) en la ISS por puro bienestar psicológico; el catnip cumple ese rol para gatos.

* **Uso de Datos/APIs de NASA:** BVAD (tablas de rendimiento de cultivos y consumo por individuo: la fuente de nuestros números). OSDR: estudios VEG-01 (seguridad alimentaria de cultivos en ISS) y OSD-804 / Rodent Research-1 (salud en microgravedad). Kit educativo de producción de comida espacial de la CSA (agencia aliada oficial).

* **Artículos de Referencia:**
  - NASA BVAD — Baseline Values and Assumptions Document. Cifras tomadas de la edición JSC-47804 (2002): https://spacearchitect.org/pubs/NASA-JSC-47804.pdf · Edición 2018: https://ntrs.nasa.gov/citations/20180001338 (verificar que coincidan antes de la entrega).
  - Khodadad et al., "Microbiological and Nutritional Analysis of Lettuce Crops Grown on the ISS", *Frontiers in Plant Science* (vía OSDR).
  - CIP–NASA, proyecto "Potatoes on Mars" (cipotato.org)
  - CSA, "Food production on the Moon and in remote areas" (asc-csa.gc.ca)
  - Para el Minijuego 1: Davila et al. 2013 (percloratos, *Int. J. Astrobiology*); Verseux et al. 2021 (cianobacterias, *Frontiers in Microbiology*); Harris et al. 2021 (rizobios en regolito, *PLOS ONE*).

---

## 6. MVP / Post-MVP

**MVP (todo son reservas; no hay cultivos vivos):**
- Despensa + almacén frío + pellets + taurina + catnip: gestión de reservas que declinan.
- Granja de ratas completa: capturas ambientales → redada → clasificación, alimentación (pellets/sobras) y control de sobrepoblación.
- Minijuego 1 (purificar tierra): produce las primeras bandejas de "suelo vivo" como hito; la siembra llega después.
- Minijuego 2 (la redada).

**Post-MVP:**
- Expansión física: los gatos salen hacia una base cercana que tiene, entre otras cosas, la hidroponía.
- Siembra en suelo curado: **trigo enano** (prioridad: grano para ratas + paja + oxígeno; ~1 m² por rata, así que la granja pide 60–100 m²) y **papa** (calorías, carta CIP; ciclo de 132 días frente a 79 del trigo). **Soya** opcional (con trigo + soya se fabrican "pellets caseros", la dieta real de ratas de laboratorio).
- Sobras de la hidroponía de la otra base como complemento de vitaminas para las ratas.
- Espirulina (tanques), hongos (crecen sobre la paja del trigo y desechos), cultivo de catnip.
- Ciclo cerrado: desechos de la granja → compostera → compost para el Minijuego 1 → tierra para el trigo → trigo para las ratas.
- Comunicación con la Tierra con retraso real (hoja de ruta general del juego).

---

## 7. Eventos Ambientales

> Cosas que pasan solas y perjudican; el jugador decide cuándo y cómo responder.

* **Ratas salvajes (pre-redada):** desde el inicio hay ratas sueltas (escaparon durante la evacuación) mordiendo las reservas y los empaques de la despensa. El gato las ve, al principio no logra atraparlas; captura algunas (~3) de forma ambiental durante otras misiones. Preparan la redada sin tutorial explícito.
* **Rata fugada de la granja (post-redada):** una rata escapa sin que la IA lo note; se descubren los estragos (reservas mordidas, un cable dañado de otra mecánica). Sigue haciendo daño mientras no sea capturada. Nace de la gestión del jugador, no de una misión incompleta.
* **Lote echado a perder:** reservas frescas mal gestionadas caducan; la IA lo señala y explica por qué.
* **Gancho para otras mecánicas:** las ratas pueden morder cables de energía o sensores de oxígeno — cada mecánica define su "evento de daño por rata" (coordinar en equipo).

---

## 8. Minijuegos relacionados

### Minijuego 1: La Fábrica de Suelo (Purificar la tierra)

**Objetivo:** convertir tierra marciana tóxica en suelo vivo cultivable operando tres estaciones.

**Mecánica Jugable (2D, recolección + estaciones simples estilo Among Us):**
* **Recolección (interior):** el gato busca y lleva a cada estación: regolito del stock que los humanos ya tenían almacenado (ellos hacían este proceso), agua del tanque, compost de la compostera y el vial de cianobacterias del laboratorio.
* **Estación 1 — Lavadora:** dosificar agua sobre el regolito mirando el medidor de perclorato en vivo. Poca agua deja veneno; mucha desperdicia. Las bacterias del tanque limpian el agua para reusar (~90 %).
* **Estación 2 — Tanques verdes:** mantener felices a las cianobacterias con tres perillas (luz, CO₂, temperatura); fabrican la materia orgánica.
* **Estación 3 — Mezcladora:** armar la receta (~85 % regolito limpio, ~10 % compost, ~5 % biomasa) e inocular al final.
* **Sin fallo duro y sin calificaciones:** la IA ejecuta cada estación cuando el jugador entra al rango aceptable, guiando ("un poco más", "¡uy, casi te pasas!"). La eficiencia se premia con más tierra por lote; hacerlo regular rinde menos, pero hay un piso: nunca se pierde el lote.
* **Tiempo:** el lote corre en segundo plano y avisa al terminar. Gag: la IA anuncia "listo, ahora esperamos unas semanas" y el gato no entiende — la lección de escalas de tiempo, disfrazada.
* **El valor educativo está en lo que la IA va diciendo durante la acción**, no en el minijuego en sí.

**Contexto STEM:** el suelo de Marte es tóxico (percloratos) pero curable; la vida —bacterias, cianobacterias, compost— es la máquina que fabrica suelo, igual que ocurrió en la Tierra. Proceso propuesto a partir de estudios parciales reales (ver sección 5); nadie lo ha integrado completo todavía, y así se declara.

### Minijuego 2: La Redada (Operación Ganado)

**Objetivo:** capturar la plaga de ratas y fundar la granja.

**Mecánica Jugable (persecución 2D + gestión):**
* **Detonante:** al terminar el primer lote de suelo, la IA cruza datos y descubre la magnitud de la plaga: hay que capturarlas todas.
* **Captura:** destreza felina del jugador; la IA ayuda de forma parametrizada (no controlable). Sensores/radar marcan a las restantes para evitar la búsqueda frustrante de "la última rata". Se capturan todas las marcadas.
* **Presión:** mientras dura la redada, las ratas siguen comiendo reservas y pueden dañar cables. UNA métrica visible para el niño: "reservas perdidas".
* **Tono:** solo captura en pantalla. La IA pide no comérselas: "vivas valen más — serán tu granja". La conversión a barras de comida ocurre fuera de cámara, a cargo de la IA.
* **Granja:** el escáner del laboratorio clasifica machos y hembras (la IA explica por qué importan más hembras para criar); alimentarlas cuesta pellets y sobras (~20 g/rata/sol); un deslizador de parejas reproductoras controla la sobrepoblación.
* **Licencia declarada:** las ratas crecen acelerado "porque eran experimentos" (misma tecnología que los gatos). La licencia es pequeña: las ratas reales ya gestan en 21 días y maduran en 6 semanas — el ganado más rápido que existe.

**Contexto STEM:** cadena trófica (las plantas alimentan a las ratas, las ratas a los gatos, y en cada nivel se pierde energía), ganadería vs. caza, omnivoría de las ratas (por eso son plaga), crecimiento exponencial de poblaciones y capacidad de carga.

---

**Nota de rigor:** los rendimientos de cultivos (comida, paja, oxígeno y agua por m² al día) son datos reales de la NASA (BVAD JSC-47804, tablas 4.2.6 a 4.2.8; la columna de días de ciclo conviene verificarla en el PDF). Los consumos de gatos (~250 kcal/sol) y ratas (~20 g/sol), el tamaño de la colonia (60–100) y el factor "1 gato = 1/10 de humano" son estimaciones de diseño propias — presentarlas como tales ante el jurado.

---

## 9. Glosario

### Términos científicos
* **Sol:** un día marciano (24,6 horas terrestres). En el juego, 1 sol = 8 minutos reales.
* **Regolito:** el "suelo" de Marte — roca volcánica molida, sin materia orgánica. No es tierra fértil.
* **Perclorato:** sal tóxica presente en el regolito marciano (~0,5–1 %). Hay que retirarla antes de cultivar.
* **Carnívoro obligado:** animal que solo puede vivir de carne. Los gatos lo son: sin proteína animal y taurina, enferman.
* **Taurina:** nutriente esencial para los gatos que solo existe en tejido animal (o como suplemento sintético).
* **Omnívoro:** come de todo. Las ratas lo son — por eso sobreviven como plaga y sirven de ganado.
* **Cadena trófica:** quién se come a quién, y la energía que se pierde en cada nivel (cultivos → ratas → gatos).
* **Capacidad de carga:** máximo de población que un sistema puede alimentar de forma sostenible.
* **Cianobacterias:** microbios que hacen fotosíntesis: con CO₂, luz, agua y minerales crean materia orgánica y oxígeno. La espirulina es una cianobacteria comestible.
* **Compost:** desechos orgánicos descompuestos por microbios hasta volverse abono.
* **Biomasa:** materia orgánica producida por seres vivos (aquí, la de las cianobacterias para la mezcla de suelo).
* **Hidroponía:** cultivo sin tierra, con raíces en agua con nutrientes. Es como cultivan la lechuga en la ISS.
* **Micorrizas / rizobios:** hongos de raíz y bacterias fijadoras de nitrógeno que se inoculan al suelo nuevo (post-MVP).
* **Pellets:** alimento balanceado de animales de laboratorio (básicamente grano + proteína de soya).
* **BVAD:** documento de la NASA con los valores de consumo por persona y rendimiento de cultivos (oxígeno, agua, comida, biomasa por m²). Fuente de nuestros números reales; usamos la edición JSC-47804 (2002).
* **OSDR:** repositorio abierto de la NASA de biología espacial. De ahí citamos VEG-01 (lechuga ISS) y Rodent Research-1 (salud ósea).
* **CIP:** Centro Internacional de la Papa, en Lima. Probó papas en condiciones marcianas simuladas con la NASA.
* **CSA:** agencia espacial de Canadá, aliada oficial del reto; usamos su kit educativo de comida espacial.

### Convenciones del equipo (vocabulario interno del juego)
* **La IA:** la inteligencia artificial de la base; guía, narra la ciencia y ejecuta lo que los gatos no pueden.
* **Reservas:** toda la comida heredada de los humanos (despensa, almacén frío, pellets). Declinan y no se reponen.
* **Barras de comida:** ración procesada por la IA a partir de las ratas, fuera de cámara.
* **Suelo vivo / tierra curada:** el producto del Minijuego 1 — regolito limpio + compost + biomasa, listo para sembrar.
* **Capturas ambientales:** ratas atrapadas fuera de minijuegos, mientras el gato hace otras misiones (~3 antes de la redada).
* **La Redada:** el Minijuego 2 — misión de capturar a toda la plaga marcada y fundar la granja.
* **Eventos ambientales:** cosas que pasan solas y perjudican (rata fugada, lote caducado); el jugador decide cuándo responder.
* **Deslizador de parejas:** control de la granja que fija cuántas ratas se reproducen — más futuro vs. más consumo hoy.
* **MVP:** la versión mínima jugable que se entrega en la hackatón. **Post-MVP:** lo diseñado para después (expansión a la base de hidroponía, siembra, espirulina...).
* **Licencia declarada:** elemento de ficción admitido abiertamente (gatos inteligentes, criogenia, ratas aceleradas), separado de la capa de ciencia real.

---

---

## 10. Fuentes y datasets

### 10.1 Datos que alimentan números del juego

| Fuente | Qué sacamos | Dónde se usa | Enlace |
|---|---|---|---|
| NASA BVAD, JSC-47804 (2002), tablas 4.2.6 a 4.2.8 y 3.3.6 | Rendimiento de trigo, papa, soya, lechuga y tomate (comida, paja, oxígeno y agua por m² al día; ciclo en días) y consumo humano de referencia (0,835 kg O₂/día) | Tabla de la sección 5; balance de oxígeno, agua y alimento de la granja | https://spacearchitect.org/pubs/NASA-JSC-47804.pdf |
| NASA BVAD, edición 2018 (verificar que coincida) | Misma fuente, versión más reciente | Verificación | https://ntrs.nasa.gov/citations/20180001338 |
| NASA OSDR — OSD-804 / Rodent Research-1 | Pérdida de hueso en microgravedad: se debilitan los huesos que cargan peso, no la columna | Diálogos de la IA sobre salud y gravedad | https://osdr.nasa.gov/bio/repo/data/studies/OSD-804 |

### 10.2 Fuentes que respaldan la ciencia del diseño

| Fuente | Qué respalda | Enlace |
|---|---|---|
| Khodadad et al. (2020), *Frontiers in Plant Science* | La lechuga roja cultivada en la ISS (VEG-01) resultó segura y nutritiva | https://www.frontiersin.org/articles/10.3389/fpls.2020.00199/pdf |
| Davila et al. (2013), *International Journal of Astrobiology* | Percloratos en el regolito marciano: peligro y recurso; limpieza biológica (Minijuego 1) | https://cambridge.org/core/journals/international-journal-of-astrobiology/article/perchlorate-on-mars-a-chemical-hazard-and-a-resource-for-humans/E4906FAD7F45A9AE8212B9198C6FD4AB |
| Verseux et al. (2021), *Frontiers in Microbiology* | Cianobacterias creciendo con atmósfera tipo Marte y regolito simulado (Minijuego 1) | https://frontiersin.org/articles/10.3389/fmicb.2021.611798/pdf |
| Harris et al. (2021), *PLOS ONE* | Rizobios fijando nitrógeno en regolito marciano simulado (suelo post-MVP) | https://pmc.ncbi.nlm.nih.gov/articles/PMC8480890 |
| Centro Internacional de la Papa (CIP), Lima, 2017 | Papa cultivada en suelo análogo marciano del desierto de La Joya (Perú), en un CubeSat que simulaba presión, gases y ciclos de temperatura de Marte. Lo hizo el CIP con la universidad UTEC de Lima, **con asesoría de NASA Ames** | https://cipotato.org/blog/indicators-show-potatoes-can-grow-mars/ |
| CSA (Agencia Espacial Canadiense), kit educativo | Producción de comida fuera de la Tierra, material para público joven (agencia aliada oficial del reto) | https://www.asc-csa.gc.ca/eng/youth-educators/toolkits/food-production/ |
| Cosecha de chiles en la ISS, Plant Habitat-04 (2021) | El cultivo más complejo cosechado y comido en órbita | Verificado solo en prensa; buscar la página oficial de NASA antes de citarlo |

### 10.3 Qué NO viene de estas fuentes
Consumo de los gatos (~250 kcal/sol), de las ratas (~20 g/sol), tamaño de la colonia (60–100), receta del suelo (85/10/5) y el factor "1 gato = 1/10 de humano": son estimaciones de diseño propias, construidas a partir de los datos de arriba y de biología básica.