# Mecánica: Agua

> **Adaptada al diseño en `docs/diseno/03_sistemas.md` (sección "Agua").** Ajustes respecto de esta propuesta:
> - El extractor del subsuelo **es parte del PMV** (paso 5 del tutorial), no del *mid game*. El reciclador quedó dañado por los meteoritos: por eso al despertar falta agua.
> - La cueva es la **zona segura**: el hielo está debajo y el gato trabaja dentro de la base. No se controla al gato directamente; el minijuego es un panel de estación con mecánica propia: elegir **dónde y qué tan hondo** perforar según el radar, la secuencia de presión y el biorreactor.
> - Los percloratos vienen del polvo de regolito mezclado con el hielo; el hielo de los glaciares en sí es casi puro.
> - En el PMV la comida sale de la **producción de alimentos** con regolito; la hidroponía es post-PMV.
> - Tanques llenos: la extracción se detiene (cola), no se pierde agua.
> - Sin agua rige la regla de sed de `04_economia_y_balance.md` (alerta extrema y muerte), no la reducción de velocidad.
> - Los trajes se reparan solos en el taller; la falta de agua no los afecta directamente.

## 1. Descripción General
> **Resumen:** El agua es el pilar de la supervivencia biológica. No solo sirve para mantener hidratados a los gatos, sino que es el insumo principal de las granjas hidropónicas que generan comida y oxígeno. 

- **Tipo de Recurso:** Físico / Tangible
- **Visibilidad:** Barra global en la interfaz y tanques de almacenamiento visuales en la base.

---

## 2. Bucle Económico (Inputs y Outputs)

### Fuentes de Generación (Cómo se obtiene)
* **Fase 1 (Early Game - Circuito Cerrado):** Al inicio, el agua **no se extrae, se recicla**. La colonia depende totalmente de los purificadores del Sistema de Soporte Vital que recuperan la humedad de la respiración de los gatos, el sudor y la transpiración de las plantas. Es un juego de suma cero (el agua total no crece, solo da vueltas).
* **Fase 2 (Mid Game - Minería de Hielo):** Los gatos deben explorar la **Cueva de refugio** para extraer bloques de hielo permafrost. Requiere energía para los taladros térmicos y para mantener el entorno presurizado al derretirlo.

### Sumideros y Consumo (En qué se gasta)
* **Consumo Activo (Soporte Vital):** Los gatos necesitan beber cada ciclo.
* **Mantenimiento Agrícola:** Las granjas hidropónicas consumen agua continuamente para que crezcan cultivos como la papa, soya, y vegetales de hoja.
* **Dependencias y Cuellos de Botella:** El hielo marciano extraído en Mid/Late game está contaminado con **percloratos** (sales tóxicas). Antes de entrar a los tanques, debe pasar por un biorreactor donde bacterias perclorato-reductoras lo purifican.

*(Nota científica estricta: ¡El agua NUNCA se usa para limpiar paneles solares ni exteriores, ya que se congelaría y evaporaría al instante por la baja presión atmosférica!)*

---

## 3. Almacenamiento y Entropía

- **Límites de Almacenamiento:** Requiere construir tanques físicos presurizados (Módulo de Agua). Si los tanques se llenan, el exceso de hielo extraído se sublima (se pierde) al no poder almacenarse en estado líquido/sólido controlado.
- **Pérdida Gradual (Merma/Entropía):** El sistema de reciclaje (Early Game) no es 100% perfecto. Hay una pérdida milimétrica cada ciclo que obliga al jugador a avanzar rápido hacia el Mid Game (la cueva) antes de que la colonia se seque por completo.
- **Vulnerabilidad a Crisis (Implosión de presión):** Si un evento de viento o meteorito fisura el módulo de agua, la caída de presión hace que el agua líquida hierva de inmediato, perdiendo gran parte del suministro de golpe.

---

## 4. Fallos en Cascada (Consecuencias de llegar a 0)

> **¿Qué pasa cuando este recurso se agota?**

1. **Efecto Inmediato:** Las plantas de las granjas hidropónicas se marchitan y detienen su producción de comida y oxígeno.
2. **Efecto Secundario:** El icono de deshidratación aparece en los gatos. Su velocidad de movimiento se reduce drásticamente (todo tarda el doble).
3. **Espirales de Muerte:** Sin agua, no hay comida ni O2. Los gatos no pueden reparar los trajes ni salir a limpiar paneles. La colonia completa colapsa rápidamente.
4. **Válvulas de Emergencia:** Racionamiento. El jugador puede decidir cortar el suministro de agua a los hidropónicos (sacrificando comida futura) para darles unos días extra de bebida a los gatos mientras reparan los sistemas.

---

## 5. Respaldo Científico / NASA (Space Apps Challenge)

* **Soporte Vital y Reciclaje (ECLSS):** Basado en el *Environmental Control and Life Support System* de la Estación Espacial Internacional (ISS), que recicla hasta el 98% del agua a través del Procesador de Orina (UPA) y el Procesador de Agua (WPA). En el juego, esto explica por qué al inicio no necesitan minar.
* **Agua en Marte y Presión Atmosférica:** En Marte, la presión atmosférica promedio es de 6 milibares (0.6% de la Tierra). En estas condiciones (Límite de Armstrong y Punto Triple del agua), el agua líquida es inestable: hierve y se congela casi simultáneamente. Esto justifica que la minería de hielo deba derretirse en interiores presurizados.
* **Hielo Subterráneo:** La NASA confirma la presencia de vastos glaciares subterráneos de agua pura bajo el regolito en latitudes medias (proyecto *Subsurface Water Ice Mapping* - SWIM). Esto avala el uso de cuevas de refugio para la extracción temprana.
* **Toxicidad y Percloratos:** La misión Phoenix (2008) descubrió altas concentraciones de percloratos en el suelo. Es tóxico para la tiroides. El uso de **Bacterias Perclorato-Reductoras** (como *Dechloromonas*) en biorreactores es una solución real en estudio por biólogos espaciales para hacer el agua potable y cultivar suelo fértil, y es una mecánica de "refinamiento" perfecta para el juego.

---

## 6. Minijuegos relacionados

### Propuesta: Minijuego de Extracción y Purificación (El Alquimista Marciano)

**Objetivo:** Minar hielo sucio de la Cueva de refugio, derretirlo sin romper la presión, y cultivar las bacterias necesarias para purificar el veneno (percloratos) antes de que la base se quede sin reservas.

**Mecánica Jugable (Puzzle de Refinamiento):**
* **Fase 1 (Cueva - Extracción):** El jugador controla a un gato con un traje reforzado dentro de la cueva. Debe usar un taladro térmico (referencia al proyecto *RedWater* de la NASA) para cortar bloques de "hielo azul sucio". Si el taladro se calienta de más, derrite el hielo al vacío y el agua se evapora (se pierde).
* **Fase 2 (Esclusa - Presurización):** Los bloques se llevan a un procesador. El jugador debe mantener el equilibrio de las válvulas de presión y temperatura. Si calienta el hielo antes de presurizar la cámara, el agua hervirá violentamente.
* **Fase 3 (Biorreactor - Limpieza):** El agua líquida obtenida es de color púrpura/marrón (tóxica). El jugador debe agregar nutrientes a una colonia de **Bacterias Perclorato-Reductoras**. Aparece un minijuego de ritmo o sincronización celular: el jugador debe mantener la temperatura cálida del agua para que las bacterias "coman" el perclorato. A medida que lo hacen, el agua se vuelve azul y cristalina, lista para enviarse a los tanques.

**Contexto STEM (Ciencia para Niños):**
Les enseña que el agua en otros planetas no es abrir un grifo y beber. Aprenderán el ciclo de los estados del agua de forma extrema (cómo el hielo puede convertirse en gas sin pasar por líquido si no hay presión atmosférica). También descubrirán la microbiología: ¡que existen bacterias "buenas" que pueden comer toxinas y limpiar el medio ambiente (biorremediación) para ayudar a los astronautas a sobrevivir!