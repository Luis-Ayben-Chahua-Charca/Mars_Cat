# Mecánica: Comida

## 1. Descripción General
> **Resumen:** Es el alimento de los gatos. Recurso de urgencia lenta (días, no minutos): el hambre no mata rápido como la falta de aire, pero vuelve lentos a los gatos y arrastra a todas las demás mecánicas. Enseña dos ideas STEM centrales: la jerarquía de supervivencia (aire > agua > comida) y la cadena trófica (cultivos → ratas → gatos).

- **Tipo de Recurso:** Tangible (raciones almacenables)
- **Visibilidad:** Barra global de raciones en la interfaz + estado de hambre por gato

---

## 2. Bucle Económico (Inputs y Outputs)

### Fuentes de Generación (Cómo se obtiene)
* **Despensa de los humanos:** raciones empaquetadas, finitas; parte quedó destruida por la lluvia de meteoritos (reloj narrativo inicial). Incluye alimento de laboratorio para animales y suplementos de taurina.
* **Invernadero rescatado:** cultivos de los humanos que sobrevivieron semanas con sistemas automáticos a media energía. La tarea no es plantar: es rescatar (regar, reconectar luces LED, cosechar lo vivo).
* **Granja de ratas:** convierte cultivos en proteína. Los gatos son carnívoros obligados (necesitan taurina, que solo está en la carne): la granja no es decoración, es la única fuente sostenible de comida felina. La IA procesa fuera de cámara y entrega "barras de comida".
* **Hierba gatera seca (catnip):** frasco dejado por los científicos (sabían que tendrían gatos). Consumible de moral, finito, no se cultiva en el MVP.
* **Dependencias:** el invernadero consume **agua** (riego) y **energía** (LED y calor), y aporta **oxígeno** al sistema de soporte vital (ver nota en sección 5; coordinar con mecánica Oxígeno). El trigo y la papa futuros dependen del **suelo curado** (Minijuego 1).

### Sumideros y Consumo (En qué se gasta)
* **Consumo activo:** cada gato come a diario (~250 kcal/sol ≈ 1 rata + suplemento). Escala con gatos despiertos.
* **Alimentar la granja:** cada rata come ~20 g/sol de material vegetal. Escala con la población: más ratas = más comida futura, pero más consumo hoy.
* **Caducidad:** lo fresco se echa a perder en pocos soles; las barras procesadas duran más.
* **Plaga:** las ratas sueltas comen reservas e invernadero (ver Eventos Ambientales).

---

## 3. Almacenamiento y Entropía

- **Límites de Almacenamiento:** la despensa tiene capacidad fija. Lo fresco caduca rápido; procesar en barras extiende la duración pero requiere la granja activa.
- **Pérdida Gradual (Merma/Entropía):** Sí — caducidad de frescos y daño de plaga.
- **Cuellos de Botella:** *early game*: la despensa se agota y obliga a rescatar el invernadero ya. *Mid game*: equilibrar la población de ratas (deslizador de parejas reproductoras) contra el consumo de cultivos. Crecimiento exponencial vs. capacidad de carga: ecología real en una perilla.

---

## 4. Fallos en Cascada (Consecuencias de llegar a 0)

> **¿Qué pasa cuando este recurso se agota?**

1. **Efecto Inmediato:** gatos hambrientos se mueven y trabajan más lento.
2. **Efecto Secundario:** menos velocidad = menos mantenimiento y reparaciones en todas las demás mecánicas.
3. **Espirales de Muerte:** hambre → lentitud → fallos acumulados en energía/oxígeno → colapso general. Más lenta que la espiral del oxígeno: así el jugador aprende la jerarquía de urgencias.
4. **Válvulas de Emergencia:** racionamiento (la IA reduce porciones a costa de moral y velocidad); re-criogenizar a un gato para reducir consumo (decisión dura, coherente con la historia); sacrificar parte de la granja (comida hoy, menos producción mañana).

---

## 5. Respaldo Científico / NASA (Space Apps Challenge)
* **Justificación Real:**
Cultivos en el espacio: la lechuga roja cultivada en la ISS (experimento Veggie/VEG-01) resultó segura y nutritiva; los chiles de Plant Habitat-04 (2021) fueron el cultivo más complejo cosechado en órbita. Cadena trófica: los gatos son carnívoros obligados — sin proteína animal y taurina enferman — lo que justifica la granja de ratas como solución biológicamente correcta, y enseña la pirámide alimenticia y su pérdida de energía entre niveles. La papa fue probada en condiciones marcianas simuladas por el Centro Internacional de la Papa (CIP, Lima) junto a la NASA — proyecto peruano. Moral: la NASA cultivó zinnias (flores) en la ISS por puro bienestar psicológico de la tripulación; el catnip cumple ese rol para gatos. Oxígeno: según los valores de referencia de la NASA (BVAD), ~25 m² de cultivo intensivo cubren el O₂ de un humano; 5 gatos respiran como medio humano, así que ~12 m² de invernadero activo pueden cubrir el aire de toda la tripulación — cruce directo con la mecánica de Oxígeno.
* **Uso de Datos/APIs de NASA:** OSDR (Open Science Data Repository): estudios VEG-01 (seguridad alimentaria de cultivos ISS) y OSD-804 / Rodent Research-1 (salud ósea, justifica ejercicio y gravedad en la salud de la tripulación). BVAD para consumos por individuo. Kit educativo de producción de comida espacial de la CSA (agencia aliada oficial).
* **Artículos de Referencia:**
  - Khodadad et al., "Microbiological and Nutritional Analysis of Lettuce Crops Grown on the ISS", *Frontiers in Plant Science* (vía OSDR).
  - NASA BVAD — Baseline Values and Assumptions Document (https://ntrs.nasa.gov/citations/20180001338)
  - CIP–NASA, proyecto "Potatoes on Mars" (cipotato.org)
  - CSA, "Food production on the Moon and in remote areas" (asc-csa.gc.ca)
  - Para el Minijuego 1: Davila et al. 2013 (percloratos, *Int. J. Astrobiology*); Verseux et al. 2021 (cianobacterias, *Frontiers in Microbiology*); Harris et al. 2021 (rizobios en regolito, *PLOS ONE*).

---

## 6. MVP / Post-MVP

**MVP:**
- Despensa finita + rescate del invernadero vecino (regar, reconectar LED, cosechar).
- Granja de ratas completa: capturas ambientales → redada → clasificación, alimentación y control de sobrepoblación.
- Catnip seco como consumible de moral.
- Minijuego 1 (purificar tierra): produce las primeras bandejas de "suelo vivo" como hito.
- Minijuego 2 (la redada).

**Post-MVP:**
- Siembra en suelo curado: trigo enano y papa (calorías para la granja).
- Hidroponía nueva (zona a desbloquear), espirulina (tanques), hongos (reciclan desechos).
- Recolección de regolito exterior (gato con traje o robot).
- Cultivo de catnip.
- Comunicación con la Tierra con retraso real (hoja de ruta general del juego).

---

## 7. Eventos Ambientales

> Cosas que pasan solas y perjudican; el jugador decide cuándo y cómo responder.

* **Ratas salvajes (pre-redada):** desde el inicio del juego hay ratas sueltas (escaparon durante la evacuación) dañando reservas e invernadero. El gato las ve, al principio no logra atraparlas; captura algunas (~3) de forma ambiental durante otras misiones. Preparan la redada sin tutorial explícito.
* **Rata fugada de la granja (post-redada):** una rata escapa sin que la IA lo note; se descubren los estragos (reservas mordidas, un cable dañado de otra mecánica). Sigue haciendo daño mientras no sea capturada. Nace de la gestión del jugador, no de una misión incompleta.
* **Lote echado a perder:** comida fresca no cosechada o mal almacenada caduca; la IA lo señala y explica por qué.
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
* **Sin fallo duro:** la IA ejecuta cada estación cuando el jugador entra al rango aceptable, guiando ("un poco más", "¡uy, casi te pasas!"). La eficiencia se premia con más tierra por lote, nunca con calificaciones.
* **Tiempo:** el lote corre en segundo plano y avisa al terminar. Gag: la IA anuncia "listo, ahora esperamos unas semanas" y el gato no entiende — la lección de escalas de tiempo, disfrazada.
* **El valor educativo está en lo que la IA va diciendo durante la acción**, no en el minijuego en sí.

**Contexto STEM:** el suelo de Marte es tóxico (percloratos) pero curable; la vida —bacterias, cianobacterias, compost— es la máquina que fabrica suelo, igual que ocurrió en la Tierra. Proceso propuesto a partir de estudios parciales reales (ver sección 5); nadie lo ha integrado completo todavía, y así se declara.

### Minijuego 2: La Redada (Operación Ganado)

**Objetivo:** capturar la plaga de ratas y fundar la granja.

**Mecánica Jugable (persecución 2D + gestión):**
* **Detonante:** al terminar el primer lote de suelo, la IA cruza datos y descubre la magnitud de la plaga: hay que capturarlas todas.
* **Captura:** destreza felina del jugador; la IA ayuda de forma parametrizada (no controlable). Sensores/radar marcan a las restantes para evitar la búsqueda frustrante de "la última rata".
* **Presión:** mientras dura la redada, las ratas siguen comiendo reservas y pueden dañar cables. UNA métrica visible para el niño: "reservas perdidas".
* **Tono:** solo captura en pantalla. La IA pide no comérselas: "vivas valen más — serán tu granja". La conversión a barras de comida ocurre fuera de cámara, a cargo de la IA.
* **Granja:** el escáner del laboratorio clasifica machos y hembras (la IA explica por qué importan más hembras para criar); alimentarlas cuesta cultivos (~20 g/rata/sol); un deslizador de parejas reproductoras controla la sobrepoblación.
* **Licencia declarada:** las ratas crecen acelerado "porque eran experimentos" (misma tecnología que los gatos). La licencia es pequeña: las ratas reales ya gestan en 21 días y maduran en 6 semanas — el ganado más rápido que existe.

**Contexto STEM:** cadena trófica (las plantas alimentan a las ratas, las ratas a los gatos, y en cada nivel se pierde energía), ganadería vs. caza, crecimiento exponencial de poblaciones y capacidad de carga.

---

**Nota de rigor:** los números de consumo, colonia de ratas (60–100 para 5 gatos) y metros cuadrados de cultivo son estimaciones de diseño basadas en BVAD y biología básica, no especificaciones probadas — presentarlas como tales ante el jurado.

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
* **BVAD:** documento de la NASA con los valores de consumo por persona (oxígeno, agua, comida). Fuente de nuestros números.
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