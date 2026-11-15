# Mecánica: Manejo de Desechos Orgánicos (Biorreactor)

## 1. Descripción General
> **Resumen:** En el espacio no existe la "basura". Todo desecho biológico de la tripulación y de las granjas es un recurso secuestrado que debe reintroducirse en el sistema de circuito cerrado para sostener la vida y las construcciones.

- **Tipo de Recurso:** Tangible (Biomasa de desecho / Abono / Fibra / Micelio).
- **Visibilidad:** Indicadores de acumulación en la interfaz del módulo del Biorreactor y en el almacén físico compartido.

---

## 2. Bucle Económico (Inputs y Outputs)

### Fuentes de Generación (Inputs)
* **Desechos Fisiológicos:** Generados de forma constante e inevitable por los gatos a medida que consumen alimentos y agua de la reserva.
* **Biomasa Agrícola:** Restos no comestibles resultantes de las cosechas en las granjas hidropónicas (como las raíces del trigo enano o los tallos sobrantes de la soya y las papas). 

### Sumideros y Beneficios (Outputs)
* **MVP - Abono (Curar el Suelo):** La biomasa es procesada por un cultivo de **bacterias de compost**. Se gasta aplicando el abono a las granjas para acelerar el crecimiento de cultivos vitales o para ayudar a las *bacterias perclorato-reductoras* a purificar el tóxico regolito marciano extraído.
* **Post-MVP - Micelio y Fibra:** Los desechos se derivan a cuartos oscuros para cultivar **hongos comestibles** . El proceso deja como residuo una gruesa red de micelio y fibra resistente que se extrae para fabricar trajes de seguridad o reparar las fisuras de los edificios .
* **Dependencias:** El Biorreactor es un módulo sellado. Consume **Energía** para mantener la temperatura (evitando que el clima exterior lo congele) y pequeñas cantidades de **Agua** para mantener viva a la colonia bacteriana.

---

## 3. Almacenamiento y Entropía

- **Límites de Almacenamiento:** Los desechos no procesados ocupan espacio físico en el almacén unificado . Si hay exceso, se bloquea el almacenamiento  y la basura comienza a acumularse en los pasillos de la colonia.
- **Pérdida Gradual / Entropía:** El abono resultante no caduca rápidamente, pero *el proceso* es frágil. Si ocurre un fallo eléctrico por una llamarada solar (PEM)  y el biorreactor se queda sin energía térmica, las bacterias mueren por congelamiento y se pierde todo el ciclo de compostaje.
- **Cuellos de Botella:** La velocidad a la que las bacterias procesan la biomasa es lenta y fija. A mayor población, se generan desechos más rápido de lo que se procesan, obligando al jugador a invertir recursos en ampliar los tanques del biorreactor.

---

## 4. Fallos en Cascada (Consecuencias de la inacción)

> **¿Qué pasa si los desechos se acumulan o no se genera abono?**

1. **Efecto Inmediato:** El almacenamiento físico colapsa y se desactiva la recolección automática . Los habitáculos se llenan de residuos orgánicos.
2. **Efecto Secundario (Salud):** El entorno insalubre actúa de forma parecida a la exposición externa: golpea directamente la moral y la salud de los obreros felinos, haciéndolos lentos o incapacitándolos .
3. **Espirales de Muerte:** Sin abono, el suelo marciano pierde fertilidad . Las cosechas de las granjas hidropónicas se retrasan, lo que inevitablemente lleva a que los gatos pasen 2 días sin comer y empiecen a morir por inanición .

---

## 5. Respaldo Científico / NASA (Space Apps Challenge)
* **Justificación Real:** 
**Sistemas ECLSS (Closed-Loop Life Support Systems):** Agencias como la ESA (con su Proyecto MELiSSA) y la NASA estudian compartimentos de biorreactores donde bacterias específicas descomponen heces, orina y biomasa vegetal. Este proceso es obligatorio porque en Marte está prohibido expulsar basura a la superficie (Protocolos de Protección Planetaria) y el clima extremo destruiría los nutrientes .
**Economía Circular y Micelio:** La transición hacia el uso de biomateriales se respalda en las investigaciones recientes de la NASA (Astro-mycology), que exploran el uso de hongos alimentados con residuos para cultivar bloques de construcción, aislamiento de radiación y tejidos.
* **Referencias Científicas:** 
**Proyecto MELiSSA (Micro-Ecological Life Support System Alternative):** El marco de referencia oficial de la Agencia Espacial Europea para el reciclaje total de materia orgánica en misiones lunares y marcianas .

---

## 6. Minijuego relacionado

### Propuesta: El Biorreactor (Operación: Sopa de Bacterias)

**Objetivo:** Regular la temperatura y humedad de la cámara del Biorreactor para que la colonia de bacterias sobreviva al ciclo completo de descomposición y genere el abono.

**Mecánica Jugable (Gestión y Equilibrio Térmico):**
* **El Problema:** Tienes una cámara sellada llena de desechos. En la pantalla ves el estado de salud microscópico de las "Bacterias de Compost" . El despiadado frío de Marte (el antagonista exterior) intenta constantemente enfriar las paredes del tanque, bajando la temperatura a niveles letales.
* **La Acción:** El jugador debe usar perillas interactivas en la consola del Biorreactor para inyectar calor (gastando Energía del reactor o las baterías) y rociar aspersores (gastando Agua). El objetivo es mantener dos agujas indicadoras (Temperatura y Humedad) dentro de una estrecha "Zona Verde" durante un tiempo limitado.
* **El Peligro de Excederse:** Si el jugador inyecta demasiada energía de golpe, el biorreactor hierve y mata a la colonia bacteriana. Si usa demasiada agua, las ahoga y, peor aún, agota las limitadas reservas hídricas que los gatos necesitan para beber.

**Contexto STEM (Ciencia para Niños):**
Este minijuego transforma la asquerosa idea de "gestionar basura" en un laboratorio de biología. Los niños aprenden la base de la Economía Circular: la materia orgánica no desaparece mágicamente al tirarla al tacho. Aprenden que las bacterias son seres vivos microscópicos que, al igual que los astronautas felinos, necesitan rangos muy específicos de temperatura y agua para sobrevivir y hacer su trabajo. Así comprenden por qué las agencias espaciales tratan el excremento y los restos de plantas como un tesoro químico indispensable para sobrevivir lejos de la Tierra.