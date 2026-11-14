# 05 — Datos NASA

**Estado:** borrador
**Decidido en:** #__
**Pendientes:** ver la sección "Pendientes" al final.

Registro de las fuentes científicas que respaldan el juego. Nada de esto se usa en el código hasta tener el dato verificado y su archivo en `MarsCat/Assets/Data/`.

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

## Pendientes

1. Tabla de minerales (mineral, para qué sirve, cuánto hay o dificultad): vacía en el documento original.
2. Probar la API de DONKI y decidir qué eventos se descargan.
3. Datos de cultivo para los valores guía y rangos de la preparación de alimentos (`03_sistemas.md`).
4. Verificar cada referencia en su fuente original antes de citarla en la presentación.
