# 05 — Datos NASA

**Estado:** borrador
**Decidido en:** #__
**Pendientes:** ver la sección "Pendientes" al final.

Registro de las fuentes científicas que respaldan el juego. Nada de esto se usa en el código hasta tener el dato verificado y su archivo en `MarsCat/Assets/Data/`.

## Ubicación de la base

| Dato | Valor |
|---|---|
| Lugar | **Cráter Vivero**, Marte (nombre oficial de la nomenclatura planetaria) |
| Coordenadas | **48,9710° N, 118,8322° E** |
| Región | Utopia Planitia |
| Fuente | NASA Mars Trek, capa de nomenclatura: https://trek.nasa.gov/mars/ |
| Imágenes | `Art-source/mapas/` |

**Por qué aquí:** el radar SHARAD del orbitador MRO detectó en Utopia Planitia, entre los 39° y 49° de latitud norte, un depósito de hielo enterrado con tanta agua como el lago Superior: 80 a 170 m de espesor, 50–85 % de hielo, bajo una capa de suelo de 1 a 10 m. NASA lo describe como más accesible que la mayoría del hielo marciano. Fuente: NASA/JPL, *Mars Ice Deposit Holds as Much Water as Lake Superior* (2016), https://www.jpl.nasa.gov/news/mars-ice-deposit-holds-as-much-water-as-lake-superior/

**Por verificar:** Vivero (48,97° N) queda en el borde norte de ese rango de latitud. Conviene confirmar en el mapa del estudio (Stuurman et al., 2016, *Geophysical Research Letters*) que el cráter cae dentro del depósito antes de afirmarlo en la presentación.

## APIs

| API | Para qué | Enlace | Estado |
|---|---|---|---|
| DONKI (eventos solares) | Tormentas y llamaradas solares reales | https://ccmc.gsfc.nasa.gov/DONKI/api/ | Por probar |
| DONKI, visualización | Ver eventos recientes | https://ccmc.gsfc.nasa.gov/DONKI/events/ | Referencia |

## Proceso científico de comida, oxígeno y suelo

No es para el jugador: es la base científica de lo que se puede hacer.

1. Hidroponía mientras tanto; se junta desecho orgánico.
2. Cianobacterias fabrican biomasa y O₂.
3. Un biorreactor limpia los percloratos del regolito.
4. Regolito limpio + compost + biomasa = suelo.
5. Micorrizas y rizobios: suelo listo para papa, trigo y soya.

| Organismo | Para qué | Uso en el juego |
|---|---|---|
| Bacterias perclorato-reductoras (*Dechloromonas*, *Azospira*) | Limpian el regolito en un biorreactor; liberan algo de O₂ | Curar suelo |
| Cianobacterias / espirulina | Materia orgánica y O₂ desde CO₂, luz y minerales; la espirulina se come | Oxígeno, comida, suelo |
| Bacterias de compost | Desechos → abono | Curar suelo |
| Rizobios | Fertilizante natural (nitrógeno) en raíces de soya o frijol | Comida |
| Micorrizas | Potencian raíces en suelo curado | Comida |
| Hongos comestibles | Proteína ligera; comen desechos, sin luz | Comida, reciclaje |
| Hongos con melanina | Bioescudo experimental | Radiación (opcional) |

## Cultivos con respaldo

| Alimento | Aporta | Respaldo |
|---|---|---|
| Lechuga roja, mizuna, rábano | Vitaminas | Veggie / VEG-01, ISS |
| Chiles | Vitamina C | Plant Habitat-04, ISS 2021 |
| Tomate cherry enano | Vitaminas | VEG-05, ISS 2022–23 |
| Trigo enano 'Apogee' | Calorías | Ciclo completo en estación espacial |
| Soya | Proteína, aceite | Advanced Astroculture, ISS 2002 |
| Papa | Calorías | "Potatoes on Mars", Centro Internacional de la Papa (Lima) con NASA, 2016–17; no ha volado |
| Espirulina | Proteína y O₂ | Arthrospira / MELiSSA, ESA en la ISS |
| Hongos comestibles | Proteína, reciclaje | Solo diseño conceptual |

## Referencias

- Davila et al. (2013), *Perchlorate on Mars: a chemical hazard and a resource for humans*, International Journal of Astrobiology.
- Verseux et al. (2021), *A low-pressure, N₂/CO₂ atmosphere is suitable for cyanobacterium-based life-support systems on Mars*, Frontiers in Microbiology.
- Poughon et al. (2020), *Limnospira indica PCC8005 growth in photobioreactor*, Life Sciences in Space Research. Versión divulgativa: nota de la ESA "Green smoothies in space".
- Harris et al. (2021), trébol y rizobios en regolito marciano simulado, PLOS ONE.
- Shunk, Gomez y Averesch (2020), *Radiotrophic Fungi can Attenuate Ionizing Radiation aboard the ISS*. **Preprint de bioRxiv sin revisión por pares**: citarlo como tal.
- OSD-804, NASA Rodent Research 1: pérdida ósea en ratones tras 37 días en microgravedad. https://osdr.nasa.gov/bio/repo/data/studies/OSD-804 — posible uso para la mecánica de salud (caminadora T2).

## Energía

Desde `docs/Propuesta_mecanica/mecanica_energia.md`. Verificar cada dato y enlace antes de usarlo.

- **Irradiancia solar en Marte:** cerca del 43 % de la de la Tierra.
- **Limpieza de paneles por ISRU:** CO₂ atmosférico comprimido en lugar de agua; el agua líquida no es estable en la superficie por la baja presión (hierve y se congela). Base del minijuego "Operación Rescate Solar".
- **Polvo:** se mide como profundidad óptica (τ). Las tormentas globales pueden bloquear la luz durante semanas; la de 2018 terminó la misión del rover Opportunity.
- **Kilopower / KRUSTY** (Kilopower Reactor Using Stirling Technology): reactor de fisión de NASA de 1 a 10 kW eléctricos, con tubos de calor de sodio y motores Stirling. https://www.nasa.gov/directorates/stmd/technology-demonstration-missions-missions/kilopower/
- **Opportunity y la tormenta de polvo:** https://mars.nasa.gov/resources/21495/opportunity-rover-and-the-martian-dust-storm/ (por verificar).
- **DONKI:** eyecciones de masa coronal (CME) y llamaradas solares para alimentar eventos (ver `06_eventos_y_ritmo.md`).

## Tormentas solares

- NASA / phys.org (junio 2024): la tormenta solar del 20 de mayo de 2024 en Marte. Curiosity RAD midió unos 8100 microgray (como 30 radiografías de tórax), el mayor pico desde su aterrizaje; MAVEN vio auroras globales; la cámara estelar de Mars Odyssey se apagó y se recuperó en menos de una hora. https://phys.org/news/2024-06-nasa-mars-epic-solar-storm.html
- ESA: errores de computadora en Mars Express y TGO durante la misma tormenta. https://www.marsdaily.com/reports/Solar_storm_supercharges_Mars_atmosphere_and_disrupts_ESA_orbiters_999.html
- Scientific American: tormenta geomagnética de marzo de 1989 y apagón de Quebec. https://www.scientificamerican.com/article/geomagnetic-storm-march-13-1989-extreme-space-weather/

## Agua

Desde `docs/Propuesta_mecanica/mecanica_agua.md`. Verificar cada dato y enlace antes de usarlo.

- **Reciclaje en la ISS (ECLSS):** el sistema de soporte vital recupera cerca del **98 %** del agua (procesadores de orina y de agua). Base del reciclador.
- **Presión y agua líquida:** la presión media de Marte es de unos **6 milibares**, cerca del punto triple del agua: el agua líquida no es estable en la superficie (hierve y se congela). Base de la cámara presurizada y de no limpiar paneles con agua.
- **Hielo subterráneo:** el proyecto *Subsurface Water Ice Mapping* (SWIM) de NASA mapea hielo bajo el regolito en latitudes medias, con datos de radar como SHARAD (Mars Reconnaissance Orbiter). Base del extractor y de la vista de radar del paso 1; las profundidades de las vetas podrían tomarse de sus mapas. Por verificar.
- **Percloratos:** la misión Phoenix (2008) los encontró en el suelo; son tóxicos. Al derretir hielo con polvo de regolito, se disuelven en el agua. Base del biorreactor (ver bacterias perclorato-reductoras arriba).
- **Taladro térmico:** proyecto *RedWater* (Honeybee Robotics con NASA) para extraer agua de depósitos de hielo. Por verificar.

## Oxígeno y aire

Desde `docs/Propuesta_mecanica/mecanica_oxigeno.md` e investigación propia. Verificar cada dato y enlace antes de usarlo.

- **Aire de la ISS:** imita el de la Tierra a nivel del mar (21 % O₂, 78 % N₂, 14,7 psia). NASA-STD-3001 fija una normoxia de 145–155 mmHg de presión parcial de O₂. https://www.nasa.gov/wp-content/uploads/2023/07/eclss-technical-brief-ochmo.pdf
- **Hábitats para Luna y Marte:** NASA propuso una "atmósfera de exploración" de 8,2 psia y 34 % O₂: menos presión y más porcentaje para facilitar las caminatas. Para el juego se usa la mezcla terrestre (21 %), más fácil de entender. https://ntrs.nasa.gov/citations/20240003211
- **Umbrales:** bajo 19,5 % el aire es deficiente y sobre 22 % está enriquecido (OSHA); bajo 16 % bajan la coordinación y la agudeza; bajo 10 % puede haber pérdida de conciencia; bajo 6 %, muerte (NIOSH). Base de las franjas del aire en `03_sistemas.md`.
  - https://www.osha.gov/laws-regs/standardinterpretations/2007-04-02-0
  - https://webarchive.library.unt.edu/eot2008/20090109085115/http:/www.cdc.gov/niosh/docs/2005-100/chapter5.html
- **Consumo metabólico:** una persona consume entre 0,49 y 1,25 kg de O₂ al día (promedio 0,84 kg), según su actividad. https://ntrs.nasa.gov/api/citations/20120015343/downloads/20120015343.pdf
- **Consumo de un gato:** 8,4 mL de O₂ por kg por minuto en reposo (Dhindsa, Hoversland y Metcalfe, 1979). Para un gato de unos 4–4,6 kg da unos 70–80 g al día, cerca de una décima parte de una persona. Anotar el peso supuesto.
- **Volumen como reserva:** la ISS usa su volumen y el rango permitido de presiones parciales como colchón; pueden pasar días entre una producción de oxígeno y la siguiente. https://ntrs.nasa.gov/archive/nasa/casi.ntrs.nasa.gov/20150010425.pdf
- **Fugas:** NASA modela la fuga de aire en kg por día y por módulo; la ISS tiene una pérdida de diseño de unos 0,195 kg de aire al día. Base de la fuga por edificio.
  - https://ntrs.nasa.gov/api/citations/20230013896/downloads/Logistics_BeyondLEO-ASCEND2023-Pres.pdf
  - https://newspaceeconomy.ca/2025/09/03/what-cargo-is-needed-to-sustain-astronauts-during-space-exploration/
- **Filtros:** en la ISS la tripulación aspira cada semana las entradas de los filtros HEPA de los conductos de aire. Base del minijuego de filtros. https://ntrs.nasa.gov/api/citations/20180008634/downloads/20180008634.pdf
- **MOXIE:** la meta era 6 g de O₂ por hora con 98 % de pureza; llegó a 12 g por hora y produjo 122 g en total (2021–2023). Escalar a kilogramos por hora pediría unos 25 kW. Base de la dependencia del MOXIE de la energía.
  - https://www.freethink.com/space/oxygen-on-mars-isru
  - https://www.therobotreport.com/?p=563715
- **Espirulina:** 0,10–0,45 mmol de O₂ por litro y hora (programa MELiSSA). 1000 L dan unos 77–346 g por día (post-PMV).
- **Por revisar en la propuesta:** "40 g/día en 40 m² de papa" parece estar en g por m² por día; revisar la fuente (Frontiers in Astronomy and Space Sciences, 2022). El enlace de la API PDS (`pds.mcp.nasa.gov`) parece mal escrito.
- **MEDA (Perseverance):** la presión en Jezero cambia con las estaciones. Podría hacer variar el rendimiento del MOXIE con datos reales (post-PMV, H2).

## Suelo y abono

- **Regolito con materia orgánica:** Wamelink et al. (2019) cultivaron nueve de diez especies en simulante de suelo marciano de NASA mezclado con materia orgánica; el contenido óptimo de materia orgánica aún no se conoce. https://doi.org/10.1515/opag-2019-0051
- **Valores de la Tierra para el abono** (regla de `README.md`): en mezclas de cultivo, el compost suele ser del 20 al 33 % del volumen, y no conviene usar demasiado porque el exceso de sales solubles quema las raíces. Base del valor guía (20 %) y del límite de quemadura (30 %).
  - https://extension.usu.edu/yardandgarden/research/using-compost-in-utah-gardens.pdf
  - https://ncagr.gov/compost-guidelines/download

## Desechos y reciclaje orgánico

Base del reciclador orgánico (`03_sistemas.md`, "Desechos y abono"). Verificar antes de usar.

- **MELiSSA (ESA), compartimento I:** recibe los desechos de la misión (heces, orina, restos de comida y partes no comestibles de las plantas) y trabaja a 55 °C por bioseguridad; a esa temperatura se inactivan la mayoría de los patógenos humanos. https://www.esa.int/articles/compartment-i-the-liquefying-compartment · https://www.melissafoundation.org/page/waste-treatment
- **Sanitización (EPA, 40 CFR 503):** en recipiente cerrado, 55 °C o más durante al menos 3 días. Base del borde inferior de la zona verde. https://www.law.cornell.edu/cfr/text/40/appendix-B_to_part_503
- **Temperatura máxima:** los microorganismos termófilos crecen mejor entre 55 y 75 °C; cerca de 70 °C o con poco oxígeno, el proceso se resiente. Base del sobrecalentamiento. https://www.pew.org/-/media/legacy/uploadedfiles/phg/content_level_pages/issue_briefs/psppewpspcompostingmanurenarrativev42pdf.pdf
- **Humedad:** entre 40 y 65 %, ideal 50–60 %; sobre 60 % falta oxígeno y el proceso se vuelve anaerobio; bajo 40 % se frena mucho. https://extension.oregonstate.edu/animals-livestock/beef/disposal-animal-mortality-byproducts
- **Desechos de gato:** tienen unas 2,5 veces el nitrógeno del estiércol de vaca y pueden traer *Toxoplasma* y parásitos; por eso necesitan compostaje caliente. Las temperaturas citadas para *Toxoplasma* varían entre fuentes: por verificar. https://www.gardeningknowhow.com/composting/manurescat-feces-in-compost.htm
- **Protección planetaria:** el COSPAR (Comité de Investigación Espacial) tiene principios y guías para misiones humanas a Marte, que NASA y ESA siguen. Buscan evitar dos contaminaciones: la **directa** (llevar microbios de la Tierra a Marte, que arruinaría la búsqueda de vida) y la **inversa** (traer algo de Marte a la Tierra). Una tripulación es una fuente continua de microbios, y NASA pide desarrollar formas de **eliminar los desechos biológicos minimizando el impacto** en el ambiente marciano. No es una prohibición literal: son guías, y los requisitos detallados para misiones tripuladas todavía se están definiendo.
  - https://ntrs.nasa.gov/api/citations/20250010949/downloads/ACR25%20-%20WP%20-%20Planetary%20Protection.pdf
  - https://nodis3.gsfc.nasa.gov/OPD_docs/NID_8715-129_.pdf
  - https://cosparhq.cnes.fr/assets/uploads/2024/07/PP-Policy_SRT_220-July-2024.pdf
- **Micelio (post-PMV):** NASA estudia hongos alimentados con residuos para fabricar materiales de construcción. Por verificar la fuente exacta.

## Comida: papa, ratas y gatos

| Dato | Valor | Fuente |
|---|---|---|
| Papa: biomasa comestible seca | 21,06 g/m² al día | NASA BVAD, JSC-47804 (2002), tablas 4.2.6–4.2.8 |
| Papa: biomasa no comestible | 9,03 g/m² al día | Igual |
| Papa: oxígeno | 32,23 g/m² al día | Igual |
| Papa: agua | 2,88 kg/m² al día | Igual |
| Papa: ciclo | 132 días (verificar en el PDF) | Igual |
| Rata de laboratorio: alimento | ~20 g al día | Estimación del equipo (`mecanica_comida.md`) |
| Gato: energía diaria | ~250 kcal | Fórmula veterinaria de energía en reposo × mantenimiento |

- BVAD: https://spacearchitect.org/pubs/NASA-JSC-47804.pdf (edición 2018 para verificar: https://ntrs.nasa.gov/citations/20180001338).
- "Potatoes on Mars", Centro Internacional de la Papa (Lima) con la UTEC y asesoría de NASA Ames: papa cultivada en suelo análogo marciano. https://cipotato.org/blog/indicators-show-potatoes-can-grow-mars/

## Suelo, papa, ratas y compost (minijuegos de comida y desechos)

| Dato | Valor | Fuente |
|---|---|---|
| Perclorato en el suelo de Marte | 0,4–0,6 % de la masa, medido en la solución al mojar la muestra (es muy soluble) | Hecht et al. (2009), *Science* 325:64–67, sonda Phoenix |
| Limpieza microbiana del perclorato | Microbios que lo degradan y suman materia orgánica, probados en el simulante MGS-1; el lavado necesita agua escasa y el calor mucha energía | Proyecto de A. Delgado (Arizona State University), financiado por NSF y NASA, 2024 (en curso, sin resultados publicados) |
| Papa y fotoperíodo | Con 12 h de luz el rendimiento de tubérculo sube con más CO₂ y más luz; con luz continua no, y baja el índice de cosecha | Wheeler, Fitzpatrick y Tibbitts (2019), *Frontiers in Plant Science* 10:1632 |
| Relación C:N para compostar | Empezar cerca de 30:1. Viruta y aserrín: 300–700:1; estiércol de vaca: 20:1; de caballo: 25:1 | Cornell Composting, *Composting Ingredients* |
| Sanitización del compost | Recipiente cerrado: 55 °C o más durante 3 días | EPA, 40 CFR 503, apéndice B (procesos que reducen patógenos) |
| Toxoplasma | Los ooquistes esporulados dejan de ser infectivos tras 1 minuto a 60 °C; a 45 °C sobreviven 1 día | Dubey (1998), citado por Food Standards Australia New Zealand (2014) |
| Ratas de laboratorio | Temperatura 20–26 °C; humedad 30–70 %; espacio de 187 cm² (hasta 300 g) a 387 cm² (hasta 500 g) por rata | *Guide for the Care and Use of Laboratory Animals*, 8.ª ed. (National Academies, 2011), tablas 3.1 y 3.2 |

Enlaces:
- Hecht et al. (2009): https://experts.arizona.edu/en/publications/detection-of-perchlorate-and-the-soluble-chemistry-of-martian-soi/
- Microbios y perclorato (ASU): https://www.notebookcheck.net/Toxic-Mars-soil-could-potentially-get-a-microbial-makeover-for-future-farming.903805.0.html
- Wheeler et al. (2019): https://www.frontiersin.org/journals/plant-science/articles/10.3389/fpls.2019.01632/
- Cornell: https://www.compost.css.cornell.edu/Factsheets/FS2.html
- EPA 40 CFR 503, apéndice B: https://www.law.cornell.edu/cfr/text/40/appendix-B_to_part_503
- FSANZ, Toxoplasma gondii (2014): https://www.foodstandards.gov.au/sites/default/files/publications/Documents/Toxoplasma%20gondii%20-%20jan%202014.pdf
- Guide, 8.ª ed.: https://www.nationalacademies.org/read/12910/chapter/4

**Por verificar:** la relación C:N de los desechos de gato y de los restos de papa no aparece en la tabla de Cornell; hay que buscarla antes de cargar el balance.

## Referencias no NASA (valores de la Tierra)

- Cats Protection (Reino Unido), *How long can cats go without food?*: un gato adulto sano sobrevive hasta unas 2 semanas sin comida (con agua), con problemas serios desde los 2 días; sin agua, unos 3–4 días, con deshidratación en 24 horas. https://www.cats.org.uk/cats-blog/how-long-can-cats-go-without-food — usado en `04_economia_y_balance.md` para la urgencia relativa del hambre y la sed.

## Pendientes

1. Tabla de minerales marcianos con fuente NASA, incluido el regolito (mineral, para qué sirve, cuánto hay o dificultad): vacía en el documento original.
2. Probar la API de DONKI y decidir qué eventos se descargan.
3. Datos de cultivo para los valores guía y rangos de la preparación de alimentos (`03_sistemas.md`). Los valores actuales son solo ejemplos; si no hay dato de NASA, se busca en artículos de investigación y, si tampoco hay, se toma de cómo funciona en la Tierra, siempre realista.
4. Verificar cada referencia en su fuente original antes de citarla en la presentación.
