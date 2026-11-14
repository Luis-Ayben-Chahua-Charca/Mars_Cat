# 06 — Eventos y ritmo

**Estado:** borrador
**Decidido en:** #__
**Pendientes:** ver la sección "Pendientes" al final.

Los eventos aleatorios presionan al jugador y marcan el ritmo de la partida.

## Catálogo de eventos

| Evento | Efecto |
|---|---|
| Lluvia de meteoritos | Daño físico a la base (el cataclismo inicial; ver `07_lore.md`) |
| Tormenta de arena | Baja la integridad de los trajes (ver `03_sistemas.md`); deja los paneles solares sin producción |
| Tormenta iónica | Degrada edificios; puede reventar trajes |
| Martemoto | Destructor físico |
| PEM | Inhabilita el funcionamiento de los edificios; en el clímax desactiva todas las máquinas |
| Tormenta solar (CME o llamarada, desde DONKI) | Durante toda la partida: sube la radiación; los gatos deben refugiarse y los trajes se desgastan más rápido (efecto real). Con licencia de ficción, en versión **atenuada**: cada edificio tiene un **25 % de probabilidad** de fallar, porque su material se degrada. Post-PMV (en el PMV no hay eventos destructivos). Repararlo cuesta un poco de **metal y fibra**; el costo principal es el **tiempo**: un gato queda ocupado durante la reparación |
| Accidente nuclear (post-PMV) | Si el reactor se usa sin correcciones de seguridad: radio de contaminación más potente que la radiación ambiental |
| Implosión / ruptura por presión | Rompe edificios |
| Radiación permanente | Degrada edificios y trajes de forma continua (no es un evento puntual) |

## Curva de presión

1. **Inicio:** los fenómenos aumentan de forma estable.
2. **Descubrimiento del beacon:** una tormenta desentierra el beacon. Los fenómenos empiezan a crecer poco a poco, pero la situación sigue estable.
3. **Señal del beacon emitida → crisis máxima:** los eventos son más frecuentes y su intensidad sube de forma brusca:
   - Destructores físicos más fuertes (martemotos, tornados).
   - Inhabilitadores (PEM) del funcionamiento de los edificios.
   - Rupturas por presión de los edificios.
   - Pérdida de alimentos por falta de energía.
4. **Clímax (últimos 1–2 minutos):** una **CME extrema** provoca un **PEM** tan fuerte que desactiva todas las máquinas de la base (licencia de ficción acordada) y la producción se vuelve **manual**. La mayoría de las producciones tardan el doble o el triple y algunos recursos ya no se pueden recolectar.
   - Ejemplo ilustrativo: si antes 5 gatos conseguían 20 de oxígeno, ahora hacen falta 20 gatos para los mismos 20. La relación real debe ajustarse con datos.
5. **Resolución:** los humanos rescatan a los gatos en la ventana de calma de las tormentas (ver `07_lore.md`).

## Base científica de las tormentas solares

- **En la Tierra:** una **llamarada** (rayos X) causa apagones de radio en el lado de día. Los cortes de electricidad vienen de las **CME**: al chocar con el campo magnético terrestre generan una tormenta geomagnética que induce corrientes en las líneas eléctricas largas. Así ocurrió el apagón de Quebec de 1989 (unos 6 millones de personas, hasta 9 horas).
- **En Marte:** no hay campo magnético global, así que no hay tormenta geomagnética comparable ni redes eléctricas largas. Las partículas llegan casi sin freno: sube mucho la radiación en la superficie, hay auroras en todo el planeta y la electrónica sufre fallos puntuales. En mayo de 2024, el detector RAD de Curiosity midió el mayor pico desde su aterrizaje (unos 8100 microgray, como 30 radiografías de tórax), la cámara estelar de Mars Odyssey se apagó un rato y los orbitadores de la ESA tuvieron errores de computadora.
- **Para el juego:** los apagones parciales se justifican como fallos y reinicios de la electrónica por partículas. El apagón total del clímax es licencia de ficción.

## Pendientes

1. Cantidad de metal y fibra y tiempo de reparación de un edificio fallado por tormenta solar.
2. Propuesta: para fallos severos, sumar una pequeña cantidad de **tierras raras** al costo, en lugar de chips (los chips exigen la cadena mina → taller).
3. "Tormenta iónica de viento morado": choca con la regla de ser lo más realista posible. Pendiente de revisión (post-PMV).
4. Duración total de una partida, para ubicar el clímax.
5. Si el PMV incluye algún evento.
6. Qué eventos se toman de datos reales (por ejemplo, DONKI para los solares; ver `05_datos_nasa.md`).
