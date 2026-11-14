# Mecánica: Energía

## 1. Descripción General
> **Resumen:** Es la generación de energía eléctrica y es la que permite a todas las máquinas funcionar, por lo tanto su producción es tranversal para que el resto de recursos puedan funcionar.

- **Tipo de Recurso:** Intangible 
- **Visibilidad:** Es una barra global en la interfaz

---

## 2. Bucle Económico (Inputs y Outputs)

### Fuentes de Generación (Cómo se obtiene)
* **Paneles solares:** Producen electricidad durante el dia, es una cantidad solo suficiente para lo basico, necesita hacer mejoras.
* **Reactor Nuclear KRUSTY:** Produce mucha energía y de forma estable, pero es necesario avanzar mucho en el árbol de conocimientos. Tiene un **costo único de construcción** (minerales, metal y chips); no consume combustible de forma continua.
* **Dependencias:** Los paneles necesitan energía para comprimir CO2 atmosférico y usarlo en mantenimiento y limpieza. El reactor necesita **correcciones de seguridad** periódicas, que cuestan materiales (ver sección 7).

### Sumideros y Consumo (En qué se gasta)
* **Consumo Activo:** Cuesta energía comprimir el gas CO2 marciano para mantener los paneles solares limpios. El reactor consume materiales solo en sus correcciones de seguridad.
* **Penalización por Población:** El consumo escala a medida que se crean mas edificios, el panel solar no genera demasiado y es fluctuante en los ciclos de dia, y de plano inactivo durante una tormenta, Por otro lado el reactor es mucho mas estable, el balance se da a que esta muy arriba en el arbol de conocimiento.

---

## 3. Almacenamiento y Entropía

- **Límites de Almacenamiento:** Tiene un tope máximo pero que es expansible a medida que se investiga la energia nuclear. La idea es que se tiene que comportar como una especie de techo que obligue al jugador a distribuir correctamente los edificios que usa ya que la energía solar es límitada y fluctuante. En caso de emergencia pueden activarse baterias de emergencia que pueden repararse.
- **Pérdida Gradual (Merma/Entropía):** La energía almacenada no se pierde. El reactor sí tiene una **barra de integridad** propia que baja con el uso (ver sección 7).
- **Cuellos de Botella:** Es gestionar la poca energia que tienes early y mid game, primero es reparar la energia para conseguir y luego administrar el recurso para darle prioridad a ciertas cosas y dejar otras de lado. 

---

## 4. Fallos en Cascada (Consecuencias de llegar a 0)

> **¿Qué pasa cuando este recurso se agota?**

1. **Efecto Inmediato:** Los edificios dejan de funcionar luego de agotarse las baterias
2. **Efecto Secundario:** Otros recursos dejan de producirse y por lo tanto lleva a que el personaje muera
3. **Espirales de Muerte:** Todos los recursos dejan de producirse asi que dado cierto tiempo empieza a haber escazes de otros recursos y eso lleva  la muerte
4. **Válvulas de Emergencia:** Pueden reparar paneles lejanos pero necesitan emplear recursos para la reparación y reconexion de tales paneles
5. **Accidente nuclear:** si el reactor se usa demasiado tiempo sin correcciones de seguridad, se produce un accidente (ver sección 7).

---

## 5. Respaldo Científico / NASA (Space Apps Challenge)
* **Justificación Real:** 
Energía Solar y Polvo Marciano: La irradiancia solar en Marte es aproximadamente el 43% de la terrestre, lo que hace que los paneles generen menos energía por metro cuadrado. El factor más crítico es el polvo en suspensión, medido científicamente como Profundidad Óptica (Dust Optical Depth o $\tau$). La acumulación física de regolito exige mantenimiento continuo, y las tormentas de polvo globales pueden bloquear la luz durante semanas, fenómeno real que inhabilitó permanentemente al rover Opportunity de la NASA en 2018. Esto justifica mecánicamente que el planeta sea el antagonista y la fluctuación temprana del recurso. Además, el mantenimiento se basa en el principio ISRU (In-Situ Resource Utilization) usando CO2 atmosférico comprimido, ya que el agua líquida no es estable en la superficie marciana debido a su baja presión atmosférica (herviría y se congelaría al instante).  
Transición Nuclear (KRUSTY): El acrónimo corresponde a Kilopower Reactor Using Stirling Technology, un experimento real de la NASA diseñado para suministrar energía de fisión segura y continua (1 a 10 kilovatios) para colonias en la Luna y Marte. Funciona mediante tubos de calor de sodio líquido y motores Stirling. En el juego, esto justifica una fuente de energía masiva que ignora el clima marciano y el ciclo día/noche, ideal para sostener la demanda crítica de hidropónicos y soporte vital en el "late game".Uso de Datos/APIs de NASA:API de DONKI (Space Weather Database): Se integrará la API de DONKI (https://ccmc.gsfc.nasa.gov/DONKI/api/) para alimentar los eventos de riesgo de la colonia.   

* **Uso de Datos/APIs de NASA:** Los datos de Eyecciones de Masa Coronal (CMEs) y Llamaradas Solares (Solar Flares) de DONKI alimentan las **tormentas solares** del juego:
  - **Efecto real:** sube la radiación en la superficie; los gatos deben refugiarse y los trajes se desgastan más rápido. En Marte no hay campo magnético global, así que las partículas llegan casi sin freno; en mayo de 2024 el detector RAD de Curiosity midió el mayor pico desde su aterrizaje y algunos equipos tuvieron fallos de electrónica.
  - **Licencia de ficción (versión atenuada):** durante una tormenta solar, cada edificio tiene un **25 % de probabilidad de fallar** porque su material se degrada. Repararlo cuesta un poco de **metal y fibra**, y sobre todo **tiempo** (un gato queda ocupado). Es post-PMV: en el PMV no hay eventos destructivos.
  - **Clímax (licencia de ficción):** una **CME extrema** provoca un PEM tan fuerte que apaga **todas** las máquinas de la base, incluido el reactor, y la producción pasa a ser manual.
  - **Diferencia con la Tierra:** en la Tierra los apagones eléctricos los causan las CME al chocar con el campo magnético e inducir corrientes en líneas largas (apagón de Quebec, 1989); las llamaradas solas causan apagones de radio.  
* **Artículos de Referencia:** 
NASA Kilopower: NASA's Kilopower Reactor Successfully Completes Ground Testing. Documentación oficial sobre la tecnología KRUSTY para sustentar el árbol de conocimientos de la colonia. ([https://www.nasa.gov/directorates/stmd/technology-demonstration-missions-missions/kilopower/](https://www.nasa.gov/directorates/stmd/technology-demonstration-missions-missions/kilopower/))
Space Weather (DONKI): Documentación técnica de la API para modelar el impacto de la actividad solar en redes eléctricas marcianas. ([https://ccmc.gsfc.nasa.gov/DONKI/](https://ccmc.gsfc.nasa.gov/DONKI/))   
The Mars Exploration Rover Dust Storms: Reportes del Jet Propulsion Laboratory (JPL) sobre cómo el polvo marciano degrada las celdas solares, utilizado para justificar la curva de entropía y el sistema de limpieza. ([https://mars.nasa.gov/resources/21495/opportunity-rover-and-the-martian-dust-storm/](https://mars.nasa.gov/resources/21495/opportunity-rover-and-the-martian-dust-storm/))

## 6. Minijuegos relacionados

### Propuesta 1: Minijuego de Paneles Solares (El Conserje Espacial)

**Nombre del Minijuego:** Operación: Rescate Solar

**Estado:** pendiente de revisión si es la preparación del edificio de energía en el PMV. Si entra, las baterías iniciales deben alcanzar para la limpieza y para la primera noche.

**Objetivo:** Gestionar la energía de las baterías de la colonia para disparar CO2 comprimido, limpiar el polvo marciano de los paneles y restaurar el flujo de electricidad antes de que caiga la noche.

**Mecánica Jugable (Acción y Gestión):**
* **El Problema:** La pantalla muestra los paneles solares cubiertos gradualmente por una gruesa capa de polvo rojo. Una barra visual de "Luz Solar Capturada" está casi vacía, advirtiendo que la energía está bajando.
* **La Acción:** El jugador controla al gato en su rol de conserje espacial. Está equipado con un tanque soplador de CO2 marciano comprimido y un cepillo antiestático. El jugador debe usar el ratón o el joystick para lanzar ráfagas de aire y "barrer" el polvo de las celdas solares.   
* **El Balance de Recursos:** Aquí entra la gestión. Comprimir el gas marciano cuesta unidades de energía de las reservas de la colonia. Si el jugador simplemente mantiene presionado el disparador, desperdiciará los valiosos vatios y la base podría no tener suficiente electricidad para pasar la noche. Debe usar ráfagas cortas y precisas combinadas con el cepillo en seco.
* **El Clima:** Ocasionalmente, ráfagas de viento traen más polvo, obligando al jugador a ser rápido y eficiente.

**Contexto STEM (Ciencia para Niños):**
Este minijuego enseña de forma visual que los paneles solares no producen energía por arte de magia, sino que necesitan una línea de visión directa y limpia hacia el Sol. Los niños aprenden dos lecciones vitales de ciencia espacial: primero, que el agua líquida no es estable en la superficie de Marte debido a la baja presión (herviría e instantáneamente se congelaría) y segundo, el concepto de ISRU (Utilización de Recursos In-Situ), que consiste en usar recursos del propio planeta (como el abundante CO2 atmosférico) para resolver problemas locales, entendiendo que transformar esos recursos exige un sacrificio energético.
---

### Propuesta 2: Minijuego del Generador KRUSTY (Transición Nuclear)

**Nombre del Minijuego:** Forja KRUSTY: El Corazón Caliente

**Objetivo:** Explorar el entorno para recolectar minerales raros y ensamblar las piezas de un reactor para transformar el calor en electricidad constante.

**Mecánica Jugable (Recolección y Rompecabezas Lógico):**
* **Fase 1: Recolección (Scavenging):** El jugador debe enviar a sus gatos a extraer minerales raros de la cueva de refugio y recuperar metales resistentes de los escombros de las edificaciones previas, además de reunir los **chips** que pide la construcción. Cada bloque de mineral recolectado se deposita en una interfaz de fabricación.   
* **Fase 2: Ensamblaje:** El minijuego cambia a una vista de plano (blueprint). El jugador tiene que construir el reactor encajando tres partes principales que ha fabricado con los recursos recolectados:
  1. **El Núcleo de Uranio (La Estufa):** Genera calor intenso (se ilumina de rojo).
  2. **Los Tubos de Sodio (Las Tuberías):** Transportan el calor (el jugador debe conectar tuberías desde el núcleo hacia el motor para que fluya una animación de líquido rojo/naranja).
  3. **El Motor Stirling (La Rueda):** Recibe el calor y comienza a girar vigorosamente, emitiendo rayos amarillos (energía).
* **Fase 3: Sincronización:** Para que la energía llegue a la base, el jugador debe regular válvulas para que el motor no se sobrecaliente, manteniendo un indicador en la zona "Verde". Una vez estabilizado, el cable principal se ilumina y la colonia entera se enciende, mientras se mantengan las correcciones de seguridad (sección 7).

**Contexto STEM (Ciencia para Niños):**
La energía nuclear suena intimidante, pero este minijuego la "mastica" reduciéndola a un concepto térmico simple que un niño puede entender: Calor → Movimiento → Electricidad. Les enseña que los materiales raros extraídos de la tierra de Marte se usan para crear un fuego que nunca se apaga (el núcleo), el cual calienta un motor especial (Stirling) que, al girar, le da a la base una independencia total de las tormentas de polvo y los ciclos de día y noche. También enseña que una fuente poderosa exige mantenimiento y responsabilidad.

---

## 7. Riesgo: accidente nuclear (post-PMV)

- El reactor tiene una **barra de integridad** propia que baja con el uso.
- Se mantiene con **correcciones de seguridad**, que cuestan materiales y devuelven integridad.
- Si el reactor se usa **demasiado tiempo sin correcciones**, ocurre un **accidente nuclear**.
- El accidente crea un **radio de contaminación**, más potente que la radiación ambiental: dentro de él, los **trajes se desgastan más rápido**.
- Por definir: ritmo de desgaste de la integridad, costo de las correcciones y tamaño del radio.