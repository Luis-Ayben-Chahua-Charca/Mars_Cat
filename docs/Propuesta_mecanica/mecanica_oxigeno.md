# Mecánica: Oxígeno

> **Adaptada al diseño en `docs/diseno/03_sistemas.md` (sección "Oxígeno").** Ajustes respecto de esta propuesta:
> - El MOXIE **entra en el PMV** (paso 4 del tutorial).
> - El oxígeno se mide como **% de O₂ del aire de la base** (meta propuesta: 21 %), no como un almacén de 5 kg. El volumen de la base es la reserva y cada edificio suma una fuga.
> - Los filtros se **ensucian solos**; el jugador limpia 1, 2 o los 5 cuando quiere. Cada filtro limpio da el 20 % de la producción. La limpieza ocupa un **gato libre** como tarea puntual; no se controla al gato directamente.
> - Cantidad de gatos: pendiente en el diseño (la propuesta asume 20).
> - Por revisar: "40 g/día en 40 m² de papa" (¿por m²?), el origen de "4 kg por día" y el enlace de la API PDS.
> - Espirulina, fugas por fisuras, consumo nocturno de las plantas y MEDA: post-PMV.

## 1. Descripción General
> **Resumen:** Es la producción de oxígeno para la supervivencia de los gatos, se trata del soporte vital con el que los gatos pueden vivir en el entorno de marte, al implicar directamente la vida de los gatos que son los trabajadores, el juego depende totalmente de este recurso

- **Tipo de Recurso:** Intangible
- **Visibilidad:** Es una barra global que aparece en la interfaz

---

## 2. Bucle Económico (Inputs y Outputs)

### Fuentes de Generación (Cómo se obtiene)
* **MOXIE:** Produce oxígeno en cantidad, se requiere de varias unidades moxie para alcanzar el mínimo para 20 gatos por día equivalente a 2.00 kg la producción por cada unidad MOXIE es de 12 gramos por hora.
* **Cianobacteria:** Como la espirulina se requiere de la plantación y está vinculado con la producción de alimentos respecto a la cantidad es de 1000 litros, respecto a la cantidad producida es de 300 gramos por día.
* **Plantas:** Como el tomate, papa u otros considerado en el área de producción de alimentos, respecto a la cantidad es de 40 gramos por día en una superficie 40 metros cuadrados de plantación de papa.
* **Dependencias:** La producción de oxígeno de MOXIE, depende de la producción de energía, puesto que cada unidad requiere de calentar para conseguir el oxígeno, por otro lado la cianobacteria como las plantas, requieren de agua, tierra, luz, y otras fuentes que permitan la existencia de estas plantas.

### Sumideros y Consumo (En qué se gasta)
* **Mantenimiento Pasivo:** La producción de oxígeno se va en su mayoría en los gatos. Sin embargo, las mismas plantas llegados a ciertas condiciones también consumen oxígeno. 
* **Pérdida masiva:** En caso de eventos del tipo fenómeno natural, al verse afectado puede llegar a romperse el área de producción de oxígeno. Con lo cual se pierde o la eficienta provocada por una reducción de producción o el consuma por una especie de fuga.

---

## 3. Almacenamiento y Entropía

- **Límites de Almacenamiento:** Sí existe un tope máximo para la producción de oxígeno, se guardará en el área de producción de oxígeno el cual es el área de soporte vital. Con lo que el almacen de oxígeno se encuentra en el mismo lugar. Por ello, el almaces debe abastecer para el mínimo de 20 gatos o el número que se indique, para 20 gatos es de 2 kilogramos de oxigeno por día. En base a las métricas planteadas se obtiene 4 kilogramos por día. Considerando que un número viable es de 5 kilogramos de almacenamiento.
- **Pérdida Gradual (Merma/Entropía):** Por algún fenómeno se puede generar fisuras en el almacenamiento de oxígeno por el cual va escapando de forma gradual el oxígeno sin ser precisamente un número tan elevado que afecte considerablemente la barra de oxígeno pero sí tiene un impacto considerable a largo plazo
- **Cuellos de Botella:** La cuestión sería ver que si fallara la energía, las unidades principales de producción de oxígeno también fallarían. Se pierde la fuente principal y entramos en pérdidas importantes

---

## 4. Fallos en Cascada (Consecuencias de llegar a 0)

> **¿Qué pasa cuando este recurso se agota?**

1. **Efecto Inmediato:** Es practicamente la muerte de los gatos que son los personajes jugables.

---

## 5. Respaldo Científico / NASA (Space Apps Challenge)
* **Justificación Real:** La mecánica de oxígeno se fundamenta en investigaciones científicas sobre la producción de oxígeno mediante recursos locales de Marte (In-Situ Resource Utilization, ISRU) y sistemas biológicos de soporte vital:
    * MOXIE: La NASA demostró que es posible extraer oxígeno de la atmósfera marciana, compuesta principalmente por dióxido de carbono (CO₂), mediante electrólisis de óxido sólido a aproximadamente 800 °C. El dispositivo experimental alcanzó una producción máxima de 12 gramos de oxígeno por hora. Esto justifica que el videojuego utilice unidades MOXIE dependientes de energía eléctrica, con riesgos asociados a interrupciones energéticas y fallos técnicos.

    * Cianobacterias: Investigaciones del programa MELiSSA de la Agencia Espacial Europea estudian el uso de Limnospira indica (espirulina) para consumir CO₂ y producir oxígeno y biomasa mediante fotosíntesis. En experimentos controlados se han observado tasas de 0,10 a 0,45 mmol de O₂ por litro y hora. Por ello, una instalación de 1.000 litros puede utilizar una producción aproximada de 300 gramos diarios como escenario favorable, siempre que se mantengan las condiciones necesarias de iluminación, temperatura y nutrientes.

    * Plantas: Investigaciones sobre sistemas bioregenerativos de soporte vital identifican a cultivos como la papa, tomate, soya y lechuga como posibles productores de oxígeno. Su rendimiento depende de la superficie cultivada, iluminación, temperatura y fase de desarrollo. La producción de oxígeno puede incorporarse como beneficio adicional de los invernaderos, considerando también la respiración vegetal y los ciclos de cultivo.

    * Consumo de los gatos: Estudios fisiológicos sobre gatos adultos han registrado un consumo aproximado de 8,4 mL de O₂ por kilogramo de peso corporal por minuto en reposo. Para el videojuego se establece una población inicial de 20 gatos adultos, con un consumo estimado de 1,60 kg de oxígeno al día y una meta de producción de 2,00 kg diarios que incorpora un margen de seguridad del 25 %.

    * Condiciones ambientales de Marte: Las tormentas de polvo pueden reducir la radiación solar que llega a la superficie, afectando los sistemas de generación eléctrica y, de manera indirecta, la producción de oxígeno. Esta relación permite justificar eventos climáticos que disminuyan la eficiencia de los sistemas de soporte vital.

* **Uso de Datos/APIs de NASA:** MEDA (Mars Environmental Dynamics Analyzer).

    * MEDA es un instrumento del rover Perseverance que registra condiciones atmosféricas y ambientales reales de Marte, incluyendo temperatura, presión atmosférica, radiación solar, polvo, humedad y viento. Los datos históricos de MEDA, disponibles en el Planetary Data System (PDS) de NASA, pueden utilizarse para generar condiciones ambientales dentro del videojuego.

    * Aplicaciones propuestas:
        * Presión atmosférica: Utilizar mediciones de presión para representar variaciones ambientales que puedan modificar el rendimiento de extracción de CO₂ de los sistemas MOXIE.

        * Radiación solar y polvo: Simular disminuciones de producción energética durante episodios de acumulación de polvo o tormentas, afectando indirectamente a MOXIE y a los sistemas biológicos que dependan de iluminación artificial.

        * Temperatura ambiental: Utilizar las variaciones térmicas marcianas para determinar la demanda energética de los sistemas de regulación térmica de invernaderos y fotobiorreactores.

        * Eventos climáticos: Emplear registros históricos para construir escenarios de tormentas de polvo, periodos de baja radiación o condiciones extremas que obliguen al jugador a recurrir al oxígeno almacenado.

    * Acceso a datos cientificos:
        * NASA PDS — Datos de MEDA: archivos científicos históricos, incluyendo mediciones calibradas en formato CSV. https://pds-atmospheres.nmsu.edu/data_and_services/atmospheres_data/PERSEVERANCE/meda.html

        * NASA PDS Search API: servicio REST que permite buscar productos y metadatos científicos de NASA. https://pds.mcp.nasa.gov/api/search/1/products

        * Documentación de la API PDS: instrucciones para realizar consultas e identificar conjuntos de datos. https://nasa-pds.github.io/pds-api/guides/search.html

    * Consideracion tecnica: Los registros de MEDA corresponden a observaciones reales realizadas en el cráter Jezero. Su utilización en el videojuego consistirá en reproducir o transformar condiciones históricas en escenarios simulados. No se considerarán predicciones meteorológicas en tiempo real. La API PDS facilita el descubrimiento de productos científicos, mientras que los archivos MEDA deben descargarse y procesarse para alimentar la simulación.

* **Artículos de Referencia:**
    * NASA (2023). NASA's Oxygen-Generating Experiment MOXIE Completes Mars Mission. Artículo oficial de NASA Justifica la viabilidad experimental de producir oxígeno a partir de la atmósfera marciana.
    * Hoffman et al. (2022). Mars Oxygen ISRU Experiment (MOXIE)—Preparing for human Mars exploration. Science Advances, 8(35). DOI: 10.1126/sciadv.abp8636 Fundamenta el funcionamiento electroquímico de MOXIE y las posibilidades de ampliar su tecnología.
    * ARTHROSPIRA-C (2025). ARTHROSPIRA-C space flight experiment: Validation of biomass and oxygen production bioprocesses in a space bioreactor prior to upload to space. Acta Astronautica. Consultar publicación científica. Sustenta la producción de oxígeno mediante cianobacterias y su dependencia de la iluminación y las condiciones del cultivo.
    * A Plant Characterization Unit for Closed Life Support (2022). Hardware and Control Design for Atmospheric Systems. Frontiers in Astronomy and Space Sciences. DOI: 10.3389/fspas.2022.820752 Proporciona valores de producción de oxígeno por superficie cultivada para lechuga, soya, tomate y papa.
    * Dhindsa, Hoversland y Metcalfe (1979). Resting hemodynamics and oxygen transport in unanesthetized adult cats. Respiration Physiology, 38(2), 105–114. DOI: 10.1016/0034-5687(79)90030-6. Permite establecer una referencia fisiológica para el consumo de oxígeno de los gatos adultos.
    * Rodriguez-Manfredi et al. (2021). The Mars Environmental Dynamics Analyzer, MEDA. A Suite of Environmental Sensors for the Mars 2020 Mission. Space Science Reviews, 217, 48. DOI: 10.1007/s11214-021-00816-9. Fundamenta la utilización de datos meteorológicos reales de Marte para construir los escenarios ambientales del videojuego.
    * NASA (2022). 10 Things: Dust in the Wind (on Mars and Well Beyond). Artículo de NASA Science. Justifica la incorporación de tormentas de polvo como eventos capaces de afectar la disponibilidad de energía solar y el funcionamiento de la colonia.

### Propuesta 3: Minijuego de MOXIE (Limpieza de Filtros de CO₂)

**Nombre del Minijuego:** Operación MOXIE: Rescate de Oxígeno

**Objetivo:** Limpiar los 5 filtros de entrada de CO₂ de una unidad MOXIE para restaurar su eficiencia de producción de oxígeno del 50 % al 100 %, garantizando el suministro de este recurso vital para los 20 gatos de la colonia marciana.

**Mecánica Jugable (Limpieza y Mantenimiento):**
* **El Problema:** La pantalla muestra una unidad MOXIE cuyos 5 filtros de entrada de CO₂ se encuentran obstruidos por el polvo marciano. Una barra visual de "Eficiencia de Producción de Oxígeno" indica que la máquina está funcionando únicamente al 50 % de su capacidad máxima. Esto significa que, en lugar de producir 12 gramos de oxígeno por hora, solamente genera 6 gramos por hora, reduciendo el suministro disponible para la colonia.
* **La Acción:** El jugador controla a un gato trabajador especializado en mantenimiento del soporte vital. Al interactuar con MOXIE, se abre un panel que muestra los 5 filtros cubiertos de polvo rojo. El gato dispone de un cepillo y una herramienta de aspiración para retirar el polvo acumulado. Utilizando el ratón o joystick, el jugador debe realizar movimientos de limpieza sobre cada filtro hasta eliminar completamente la suciedad. Cada filtro presenta una barra individual de limpieza que permite visualizar el progreso.
* **La Recuperación de Eficiencia:** Cada filtro completamente limpiado aumenta la eficiencia de MOXIE en un 10 %. La barra global de eficiencia se actualiza inmediatamente, mostrando cómo la intervención del gato mejora la producción de oxígeno:
  1. **0 filtros limpios:** 50 % de eficiencia (6 g O₂/h).
  2. **1 filtro limpio:** 60 % de eficiencia (7,2 g O₂/h).
  3. **2 filtros limpios:** 70 % de eficiencia (8,4 g O₂/h).
  4. **3 filtros limpios:** 80 % de eficiencia (9,6 g O₂/h).
  5. **4 filtros limpios:** 90 % de eficiencia (10,8 g O₂/h).
  6. **5 filtros limpios:** 100 % de eficiencia (12 g O₂/h).
* **El Balance de Recursos:** Mientras los filtros permanezcan sucios, MOXIE continúa consumiendo electricidad, pero produce menos oxígeno. Esto obliga al jugador a realizar el mantenimiento para aprovechar mejor la energía disponible. El oxígeno producido se incorpora al almacenamiento global del área de soporte vital, cuya capacidad inicial es de 5 kilogramos.
* **El Mantenimiento Parcial:** El jugador puede abandonar el minijuego antes de terminar la limpieza. En ese caso, MOXIE conserva la eficiencia correspondiente a los filtros completamente limpiados. Los filtros restantes permanecen sucios y podrán limpiarse en una siguiente intervención.
* **El Clima:** Las tormentas de polvo marcianas pueden acelerar la acumulación de suciedad en los filtros, provocando una disminución gradual de la eficiencia. Esto obliga al jugador a supervisar periódicamente las unidades MOXIE y decidir cuándo realizar su mantenimiento.
* **La Victoria:** El minijuego se completa exitosamente cuando los 5 filtros están completamente limpios y la unidad MOXIE recupera el 100 % de su eficiencia. La barra de producción alcanza su valor máximo y el sistema confirma que la generación de oxígeno ha sido restaurada.

**Contexto STEM (Ciencia para Niños):**

Este minijuego enseña de manera visual que producir oxígeno en Marte requiere tecnología, energía y mantenimiento constante. MOXIE es un dispositivo real desarrollado por la NASA que logró extraer oxígeno del dióxido de carbono presente en la atmósfera marciana mediante un proceso llamado electrólisis de óxido sólido.

Los niños descubren que el polvo marciano puede representar un problema para los equipos tecnológicos y que mantener limpios los sistemas de entrada de aire ayuda a proteger su funcionamiento. También aprenden que una máquina puede seguir consumiendo electricidad incluso cuando trabaja con menor eficiencia, por lo que realizar mantenimiento preventivo permite aprovechar mejor los recursos limitados de una colonia espacial.

La mecánica de los 5 filtros y el incremento del 10 % de eficiencia por cada filtro limpiado es una simplificación diseñada para el videojuego, inspirada en la necesidad real de proteger los sistemas tecnológicos del polvo marciano. De esta manera, los jugadores comprenden la relación entre **mantenimiento, eficiencia energética y supervivencia**, descubriendo que pequeñas acciones pueden tener consecuencias importantes para toda una colonia.