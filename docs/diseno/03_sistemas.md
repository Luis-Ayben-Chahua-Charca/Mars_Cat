# 03 — Sistemas

**Estado:** borrador
**Decidido en:** #__
**Pendientes:** ver la sección "Pendientes" al final.

Los porcentajes y cantidades de esta página son **ejemplos**: los valores reales deben salir de datos de NASA, de artículos de investigación o, si no hay, de cómo funciona en la Tierra, siempre realistas (ver `README.md` y `05_datos_nasa.md`). Cuando Lógica cree el archivo de balance en `MarsCat/Assets/Data/`, pasan a vivir ahí y esta página solo explica su porqué (ver `README.md`).

## Recursos

| Recurso | Tipo | En el PMV |
|---|---|---|
| Oxígeno | Soporte vital. Se mide como **% de O₂ del aire de la base**, no como tanque (ver "Oxígeno") | Sí |
| Electricidad | Soporte vital | Sí |
| Comida | Soporte vital | Sí |
| Agua | Soporte vital; se guarda en **tanques** propios, no en el depósito | Sí |
| Metal | Material de construcción; insumo de robots y trajes. Se extrae de Marte y se recupera de escombros | Sí |
| Escombros | Material de construcción | Sí |
| Bacterias | Insumo de la preparación del cultivo. **Infinito**: solo se ajusta su valor óptimo en el minijuego | Sí |
| Fibra | Insumo de trajes | No |
| Regolito (tierra) | Base de la mezcla de suelo de la granja de plantas. **Infinito**: está en la superficie marciana | Sí |
| Abono | Insumo de la granja de plantas. **Reserva inicial** suficiente para la primera granja; después sale del reciclador orgánico | Sí |
| Desechos de gatos | Los generan todos los gatos desde el inicio, según lo que comen y beben. Se guardan en el **depósito** hasta que se repara el reciclador orgánico | Sí |
| Desechos de ratones | Los genera la granja de ratones. Se guardan en el **depósito** hasta que se procesan en el reciclador orgánico | Sí |
| Restos de cosecha | Partes no comestibles de la granja de plantas (tallos, raíces). Insumo del reciclador orgánico | Sí |
| Tierras raras | Insumo de chips. Se producen en un edificio propio | No |
| Chips | Se fabrican con tierras raras. Materia prima de robots y de la **construcción** de edificios complejos (post-PMV) | No |
| Robots | Automatizan la preparación. Se fabrican con chips y metal | No |

Los minerales del juego salen siempre de datos de NASA. El **abono** es recurso del pool general (acordado). La **luz** se expresa en %; por confirmar si sale de la electricidad.

## Edificios

| Edificio | Produce | Insumos | En el PMV | Zona |
|---|---|---|---|---|
| Energía (paneles solares) | Electricidad, solo de día; nada durante tormentas | Limpieza con CO₂ comprimido, que cuesta energía (ver "Energía") | Sí: reparar y preparar | Por definir |
| Baterías de emergencia | Guardan electricidad para la noche y emergencias; reparables | — | Por definir | Por definir |
| Reactor KRUSTY (Kilopower) | Electricidad constante, sin depender del clima ni del día | Costo único de construcción (minerales, metal y chips); materiales para correcciones de seguridad | No | Por definir |
| Granja de plantas | Comida; también fibra, algo de oxígeno y restos de cosecha | Regolito, agua, luz, bacterias, abono (la hidroponía se desbloquea post-PMV en el árbol de conocimiento) | Sí: reparar y preparar | Por definir |
| Granja de ratones | Comida (proteína); **desechos de ratones** | Por definir | Sí: reparar y preparar | Por definir |
| Reciclador orgánico | **Abono** | Desechos de gatos y de ratones, restos de cosecha; electricidad (calor) y agua | Sí: reparar y preparar (paso 8) | Por definir |
| Extracción de agua del subsuelo | Agua (hielo bajo la cueva, purificado) | Electricidad (taladro térmico y calentador); bacterias perclorato-reductoras (infinitas) | Sí: reparar y preparar | Segura (propuesta) |
| Reciclador de agua (soporte vital) | Recupera parte del agua consumida; el porcentaje sube con la investigación | Electricidad | No: dañado; en el PMV solo lo menciona la IA | Segura |
| Mina de tierras raras | Tierras raras | Por definir (tiene su propio minijuego) | No | Zona nueva (exploración) |
| MOXIE (industrial) | Oxígeno | Mucha electricidad; filtros de entrada limpios | Sí: reparar y mantener filtros (paso 4) | Por definir |
| Depósito (almacén compartido) | Guarda alimentos, fibra y escombros; lugar de recuperación (ver `04_economia_y_balance.md`) | — | Sí | Segura |
| Taller | Chips, robots y trajes | Tierras raras → chips; chips + metal → robots; fibra + un poco de metal → trajes | No | Por definir |

## Reparar o construir

- **PMV:** todos los edificios se **reparan**; ninguno se construye desde cero. Reparar no usa chips.
- **Post-PMV:** construir un edificio **nuevo o mejorado** (por ejemplo, hidroponía, reactor KRUSTY o edificios de extracción, una vez investigados) cuesta **chips** además de los materiales base. Así la producción de chips nunca deja de ser necesaria.
- **Regla contra bloqueos:** nada de la cadena que produce chips (la mina de tierras raras y el taller) puede exigir chips para funcionar por primera vez. La primera mina es la **antigua mina** que se descubre y se repara, no se construye.

## Ciclo de un edificio de producción

1. **Reparación:** automática.
2. **Preparación:** minijuego (ver abajo). Lo que pone el jugador se descuenta del pool general.
3. **Funcionamiento:** un **gato libre** se asigna **automáticamente** al puesto y trabaja en el edificio y este produce durante un tiempo.
4. Al terminar ese tiempo, se **vuelve a preparar a mano**.
5. Cuando el jugador tiene **robots**, la preparación se automatiza y deja de hacerse a mano.

Así el jugador aprende primero qué necesita cada edificio, y la automatización evita que se vuelva tedioso.

## Preparación de edificios (minijuego)

Inspirada en los minijuegos de *Among Us*. **Cada edificio tiene su propio minijuego, con su propia mecánica**, elegida para enseñar la ciencia de ese edificio. Lo único común es el **resultado**:

| Resultado | Significado |
|---|---|
| Arranca o no | Si la preparación quedó válida |
| Rendimiento | Multiplicador de producción (por ejemplo, de 80 % a 120 %) |
| Motivo | Si no arranca o rinde poco, qué corregir; la IA lo explica |

| Edificio | Minijuego | Mecánica |
|---|---|---|
| Granja de plantas | Preparación del cultivo | Valor guía y rango por insumo (abajo) |
| Granja de ratones | Por definir | Por definir |
| Reciclador orgánico | "Operación Sopa de Bacterias" | Mantener temperatura y humedad en la zona verde durante el ciclo (ver "Desechos y abono") |
| MOXIE | Filtros de entrada | **Mantenimiento continuo**: los filtros se ensucian solos y se limpian cuando el jugador quiere (ver "Oxígeno") |
| Extracción de agua | "Alquimista Marciano" | Ubicación y profundidad de perforación, secuencia de presión y biorreactor (ver "Agua") |
| Energía | "Operación Rescate Solar" | Ritmo al estilo *Piano Tiles*: disparar CO₂ a las celdas sucias de una banda de paneles (ver "Energía") |

### Granja de plantas: valor guía y rango

- Cada insumo tiene un **valor guía** que da la IA y un **rango de aceptación**.
- **Fuera del rango:** se muestra una **advertencia de rango prohibitivo** que indica qué insumo hay que corregir; el edificio no arranca hasta corregirlo.
- **Dentro del rango:** ciertos valores y **combinaciones** mejoran el **rendimiento**.

#### Valores

El **abono** usa valores de la Tierra (compost en mezclas de cultivo; ver `05_datos_nasa.md`). El resto siguen siendo ejemplos. Pasan a `MarsCat/Assets/Data/` cuando Lógica cree el archivo de balance.

| Insumo | Valor guía | Rango de aceptación | Fuera de rango |
|---|---|---|---|
| Abono (% del volumen de la mezcla de suelo) | 20 % | **0 %–30 %** | Más de 30 %: el exceso de sales **quema** las plantas; no arranca |
| Regolito (tierra) | El resto de la mezcla (80 % con el valor guía) | — | — |
| Agua | 2 L (ejemplo) | Por definir | Por definir |
| Luz | Por definir (%) | Por definir | Por definir |
| Bacterias | Por definir (cantidad) | Por definir | Por definir |

- **Abono en 0 %:** está **dentro** del rango. La granja arranca, pero rinde menos; así la IA enseña para qué sirve el abono antes de que exista el reciclador orgánico.
- **Rango asimétrico:** hacia abajo el abono baja hasta 0 sin bloquear; hacia arriba el borde es la quemadura.


## Energía

Propuesta del equipo en `docs/Propuesta_mecanica/mecanica_energia.md`, adaptada aquí.

- **Barra global** en la interfaz. Recurso intangible: no ocupa el depósito.
- **Paneles solares:** fuente inicial. Producen poco y de forma fluctuante: solo de día y nada durante tormentas. Necesitan mejoras.
- **Reactor KRUSTY:** fuente de *late game*, alto en el árbol de conocimiento. Produce mucho y de forma estable, sin depender del clima ni del ciclo de día y noche.
  - **Costo único** de construcción; no consume combustible de forma continua.
  - Tiene una **barra de integridad** propia que baja con el uso. Se mantiene con **correcciones de seguridad**, que cuestan materiales.
  - Si se usa demasiado tiempo sin correcciones, ocurre un **accidente nuclear**: aparece un **radio de contaminación**, más potente que la radiación ambiental, donde los trajes se desgastan más rápido.
- **Polvo:** se acumula sobre los paneles y baja su producción; hay que limpiarlos.
- **Limpieza (ISRU):** con **CO₂ marciano comprimido** y un **cepillo antiestático**. Comprimir el gas cuesta **energía** de las baterías. No se usa agua: el agua líquida no es estable en la superficie de Marte por la baja presión.
- **Cuello de botella:** al inicio, reparar la energía primero y después priorizar qué edificios reciben energía.

### Minijuego de preparación de energía: "Operación Rescate Solar" (acordado)

Inspirado en *Piano Tiles*. El gato, como "conserje espacial", limpia el polvo de los paneles con disparos de **CO₂ comprimido**. Tarea puntual: ocupa un gato libre.

- **Escena:** una **banda de paneles** con **3 carriles** que avanza fila por fila hacia una **plataforma con 3 boquillas** (1, 2, 3). Algunas celdas traen polvo.
- **Controles:** **Q / W / E** disparan por la boquilla 1, 2 o 3. La animación del gato presiona el botón correspondiente.
- **Ventana:** cada fila queda frente a las boquillas durante **1 segundo**; lo que no se limpia en ese tiempo pasa sucio.
- **Disparos sin límite, con costo:** cada disparo gasta **electricidad** de las baterías, acierte o no. Disparar a celdas limpias o *spamear* los tres carriles desperdicia energía: ese es el castigo. El tiempo también cuenta, pero pesa menos.
- **Eficiencia** = celdas sucias limpiadas ÷ total de celdas sucias. Es el rendimiento del resultado común de preparación.
- **Balance:** el costo por disparo se ajusta con la producción de electricidad de los paneles en `Data/`. Debe costar menos de lo que gana un panel limpio, y disparar siempre a los tres carriles debe costar más de lo que se gana.
- **Enseña:** por qué el agua líquida no sirve en Marte y el concepto de **ISRU** (usar el CO₂ del propio planeta, a cambio de energía).
- **Post-PMV (opcional):** suciedad pesada que pide 2 disparos; banda más rápida durante tormentas de polvo; cepillo antiestático como acción gratuita pero lenta.

#### Arte de referencia (`Art-source/2D/`)

| Archivo | Uso |
|---|---|
| `Panel_solar.png` | Textura de la banda (3 columnas, se repite en vertical; se desplaza por UV) |
| `Plataforma.png` | Plataforma con las 3 boquillas numeradas |
| `gato_apuntando.png` | Animación del gato presionando 1, 2 o 3 |
| `disparo_co2.png` | Secuencia del disparo de CO₂ |
| `Manchas_tierra.png` | 6 variantes de polvo sobre las celdas |

**Observaciones pendientes del arte:**
- `Manchas_tierra.png` tiene fondo oscuro con brillo pegado; hace falta exportarlo con canal alfa.
- `disparo_co2.png` tiene bordes cian y blancos que quedaron al quitar el fondo.
- La plataforma está en vista 3/4 y el panel en vista cenital; probar cómo se ven juntos.

### Minijuego post-PMV: "Forja KRUSTY"

Recolección de minerales y metal, ensamblaje del reactor (núcleo → tubos de sodio → motor Stirling) y regulación de válvulas para mantener el motor en la zona verde. Enseña la idea de calor → movimiento → electricidad.

### Propuesta de lógica de rendimiento del cultivo (no acordada)

- **Tolerancia por lado:** cada insumo puede tener una tolerancia distinta por debajo y por encima del valor guía. Abono: 20 puntos hacia abajo (0 % da `s = 0`) y 10 hacia arriba (30 % da `s = 0`).

- **Puntaje por insumo:** `s = 1 − |valor − guía| / tolerancia`. Vale 1 en el valor guía y 0 en el borde del rango.
- **Rendimiento base:** `rendimiento = 0,8 + 0,4 × promedio(s)`. Va de 80 % (todo en el borde) a 120 % (todo en el valor guía).
- **Combinaciones:** una tabla de sinergias por edificio. Si dos insumos de la sinergia tienen `s ≥ 0,8`, suman un bono fijo (por ejemplo +10 %). Para el PMV se puede dejar sin combinaciones.
- **Automatización (post-PMV):** el robot pone todos los insumos en su valor guía; el resultado es fijo y rápido. Hacerlo a mano permite buscar combinaciones y superar ese rendimiento.
- **Datos reales:** los valores guía y rangos deberían salir de datos de NASA sobre cultivo en el espacio y registrarse en `05_datos_nasa.md`. No se inventan.

Para Lógica: un **resultado común** (`PreparationResult`: arranca, rendimiento, motivo) y una clase de preparación **por minijuego**, cada una con su configuración en `Data/`. El ciclo del edificio solo lee el resultado, sin saber qué minijuego lo produjo. El contrato se acuerda en el issue.

## Oxígeno

Propuesta del equipo en `docs/Propuesta_mecanica/mecanica_oxigeno.md`, adaptada aquí. **Acordado:** el MOXIE entra en el PMV (paso 4 del tutorial) con el minijuego de filtros; la limpieza ocupa un **gato libre** como tarea puntual. El modelo del aire es **propuesta**.

### El aire de la base

- La barra muestra el **% de O₂ del aire de la base**, como en un hábitat real, donde se controla la mezcla de gases y no un tanque. Meta propuesta: **21 %**, como el aire de la Tierra.

| Franja | % O₂ | Efecto |
|---|---|---|
| Verde | 19,5–22 % | Normal |
| Amarillo | 16–19,5 % | La IA avisa; los gatos trabajan más lento |
| Rojo | 10–16 % | Los gatos dejan de trabajar |
| Crítico | Menos de 10 % | Si dura más que el margen de reacción, los gatos mueren |

Por encima de 22 % el aire está **enriquecido** y sube el riesgo de incendio (post-PMV, si se quiere usar). Umbrales con fuente en `05_datos_nasa.md`.

- **Cambio por tick:** (producción del MOXIE + aporte de las plantas − consumo de los gatos − fugas) ÷ volumen de la base.
- **El consumo es de los gatos**, según cuántos son y cuánto trabajan, no del tamaño de la base.
- **El volumen es la reserva:** cuanto más grande la base, más lento cambia el %.
- **Cada edificio reparado suma volumen:** al presurizarlo, el % baja un poco una sola vez y la base gana reserva, pero también una **fuga** pequeña permanente. Así, una base más grande pide más oxígeno de forma realista.

### MOXIE y sus filtros

- **Stash de 5 filtros** de entrada. Se **ensucian solos** con el tiempo (post-PMV, más rápido con tormentas de polvo).
- **Producción:** cada filtro limpio aporta el 20 % de la capacidad del MOXIE. Con los 5 tapados no produce y el % de O₂ baja de forma continua.
- El MOXIE gasta electricidad igual aunque tenga filtros sucios: limpiarlos aprovecha mejor la energía.
- **Limpieza:** el jugador limpia **1, 2 o los 5** cuando quiera; se puede hacer parcial. Solo cuesta **tiempo**: es una **tarea puntual** que ocupa un **gato libre** mientras dura (ver "Asignación de trabajo" en `04_economia_y_balance.md`). Sin gatos disponibles no se puede limpiar.
- **No sigue el ciclo preparar → funcionar → volver a preparar:** es mantenimiento continuo. Su resultado común (rendimiento) es la fracción de filtros limpios.
- **Enseña:** ISRU (oxígeno a partir del CO₂ marciano), que producir oxígeno cuesta mucha energía y que el mantenimiento es parte de la vida en una estación (la tripulación de la ISS aspira sus filtros cada semana).

## Agua

Propuesta del equipo en `docs/Propuesta_mecanica/mecanica_agua.md`, adaptada aquí. **Acordado:** la extracción entra en el PMV y el reciclaje es post-PMV. El resto de la sección sigue en propuesta.

- **Recurso físico.** Barra global en la interfaz. Se guarda en **tanques** con tope propio, como el oxígeno; no ocupa el depósito.
- **Fuentes:**
  1. **Reciclador (soporte vital), post-PMV:** recupera parte del agua que consumen gatos y cultivos, como el sistema de la ISS. La lluvia de meteoritos lo dejó dañado; por eso, al despertar, el agua no alcanza (etapa 2 de `02_core_loop.md`).
     - **En el PMV** no es un edificio: solo la IA lo menciona para explicar por qué falta agua y para presentar el dato del 98 % de la ISS (ver `05_datos_nasa.md`).
     - **Post-PMV** se repara, y su **eficiencia de reciclaje sube por niveles** de investigación en el árbol tecnológico. Reciclar cuesta menos energía que perforar vetas profundas, así que el jugador elige entre las dos fuentes.
  2. **Extracción de agua del subsuelo, PMV:** el edificio del tutorial (paso 5). Saca **hielo** de debajo de la cueva, lo derrite en una cámara presurizada y lo limpia de **percloratos** en un biorreactor antes de mandarlo a los tanques.
- **Consumo:** la sed de cada gato (`04_economia_y_balance.md`) y la preparación de la **granja de plantas**. La hidroponía es post-PMV.
- **Exceso:** con los tanques llenos, la extracción se detiene y queda en cola, igual que el depósito. El agua no se pierde.
- **Sin agua:** rige la regla de sed (alerta extrema, busca agua, muere). La granja de plantas no arranca porque el insumo agua queda fuera de rango.
- **Post-PMV:** racionamiento (cortar el agua a los cultivos para que beban los gatos) y fisura del tanque por el evento **ruptura por presión**, que hace hervir y perder parte del agua (`06_eventos_y_ritmo.md`).

### Preparación del extractor: "Alquimista Marciano"

Panel de estación: el jugador **no controla al gato**, que se asigna solo al puesto. La mecánica es propia y gira en torno a **dónde y qué tan hondo perforar**.

**Paso 1 — Sondeo y perforación**

- Una **vista en corte del subsuelo** bajo la cueva muestra el resultado del radar: vetas de hielo a distintas **profundidades**, con distinta **pureza** (cuánto polvo de regolito, y por tanto percloratos, tienen) y **volumen**.
- El jugador **coloca** el taladro y elige la **profundidad**:
  - **Somero:** barato y rápido, pero hielo más sucio y con menos volumen.
  - **Profundo:** hielo más puro y abundante, pero cuesta más electricidad y tiempo.
  - **Fallar la veta** (quedarse corto o pasarse) no da agua: hay que reubicar.
- Mientras baja, mantiene la **potencia** del taladro térmico en la zona verde: si se calienta de más, el hielo **sublima** y se pierde parte del lote.
- **Cada veta se agota.** En la siguiente preparación hay que elegir otra, así el minijuego no se repite igual.

**Paso 2 — Cámara de derretido** (acordado)

Inspirado en las acciones con tiempo de *Mi Vecino Infernal 2*: cada fase tiene un **círculo de progreso** que se llena mientras el jugador mantiene la aguja en su zona.

1. **Presurizar:** un **manómetro** en el centro de la pantalla. El jugador mantiene la aguja dentro de la zona verde mientras se llena el círculo. Si falla, la presurización **se reinicia** (no se pierde hielo, solo tiempo).
2. **Calentar:** después, un **termómetro** en el centro. Igual: mantener la aguja en verde hasta llenar el círculo. Si falla, el agua **hierve** y **se pierde el lote**.

- **Círculo de progreso (valores de referencia, pendientes de balance):** arranca en **40 %**. Con la aguja en el centro se llena hasta 100 % en **5 segundos**. Si la aguja sale del centro, el círculo **baja**; si llega a **0 %**, la fase falla.
- El orden es fijo (primero presión, después calor), y el castigo es más duro en la segunda fase: el jugador aprende que calentar sin presión hace hervir el agua.
- **Botón acelerar:** reduce el tiempo a la **mitad** (2,5 segundos), pero gasta **más electricidad** y hace las agujas **más inestables** (oscilan más). Es un desafío opcional: más rápido, más caro y más difícil.

**Paso 3 — Biorreactor**

- La purificación ocurre **dentro del edificio**, en un tanque propio, después de derretir el hielo. No se hace bajo tierra: las bacterias necesitan temperatura y condiciones controladas, el resultado debe poder medirse, y meter bacterias en el subsuelo contaminaría Marte (protección planetaria). Orden: extraer → derretir en la cámara → purificar en el biorreactor.

- La **dosis de bacterias** perclorato-reductoras depende de la pureza de la veta: hielo sucio necesita más bacterias y más tiempo.
- El jugador mantiene la **temperatura** tibia mientras el agua pasa de púrpura o marrón a azul.
- Si se queda corto, el agua sigue tóxica y no entra a los tanques.

**Rendimiento:** volumen de la veta × pureza lograda × (1 − pérdidas por sublimación en la perforación). Si el agua hierve en el paso 2 se pierde el lote entero. Una veta profunda bien trabajada rinde más, pero gasta más energía: el jugador decide según cuánta electricidad tenga.

#### Arte de referencia (`Art-source/2D/`)

| Archivo | Uso |
|---|---|
| `Soil_Cutaway.png` | Vista en corte con 4 estratos: cada estrato es un rango de profundidad; las vetas van encima |
| `Drill_Box_Cable.png` | Carrete, cable y cabezal: el cable estirado marca la profundidad; los LEDs, la potencia; la luz naranja, el sobrecalentamiento |
| `Ice_Contamination.png` | 6 bloques de hielo, de pequeño y sucio a grande y limpio: pureza y volumen de cada veta |
| `Thermometer_Heating.png` | Termómetro con zonas azul, verde y roja para la fase de calentar |

**Archivos nuevos que faltan** (en `Art-source/2D/`, con canal alfa):

| Archivo | Contenido |
|---|---|
| `Pressure_Gauge.png` | Esfera del manómetro con la zona verde al centro |
| `Pressure_Gauge_Needle.png` | Aguja del manómetro, separada para poder rotarla |
| `Progress_Ring.png` | Círculo de progreso: anillo vacío y anillo lleno, por separado |
| `Speed_Button.png` | Botón acelerar en tres estados: normal, presionado y activo |
| `Bioreactor_Tank.png` | Tanque del biorreactor con el agua en tres estados: púrpura o marrón, intermedio y azul |
| `Bacteria_Dose_Icon.png` | Ícono de la dosis de bacterias |

**Observaciones pendientes del arte:** el taladro, el hielo y el termómetro tienen fondo oscuro con brillo pegado; exportarlos con canal alfa. Verificar el alfa de `Soil_Cutaway.png`.

- **Energía:** el taladro y el calentador gastan electricidad al preparar. Refuerza la regla del tutorial de reparar primero la energía (paso 3) antes que el agua (paso 5).
- **Enseña:** que el hielo marciano se busca con radar y está a distintas profundidades, por qué el agua líquida no es estable a la presión de Marte, qué son los percloratos y la **biorremediación**.

## Desechos y abono

Propuesta del equipo en `docs/Propuesta_mecanica/mecanica-desechos.md`, adaptada aquí. **Acordado**, incluido el minijuego; los controles son propuesta.

- **Edificio: reciclador orgánico.** Módulo sellado que convierte desechos en **abono** con bacterias de compost. Cierra el ciclo: comida → gatos y ratones → desechos → abono → granja de plantas. Es la misma idea del sistema MELiSSA de la ESA (ver `05_datos_nasa.md`).
- **Insumos:** desechos de gatos, desechos de ratones y restos de cosecha de la granja de plantas. Las bacterias son infinitas.
- **Dependencias:** **electricidad** (paneles y baterías) para mantener el calor, porque el frío de Marte enfría el módulo, y un poco de **agua** para la humedad de la colonia bacteriana.
- **Desechos de gatos:** cada gato los genera según lo que come y bebe. Se guardan en el **depósito** desde el inicio y ocupan espacio.
- **Exceso:** si el depósito se llena de desechos, los gatos **pierden salud** de forma gradual hasta que se procesen. Solo salud: no hay moral.
- **Sin abono:** la granja de plantas arranca igual, pero rinde menos (abono en 0 %, ver "Granja de plantas").
- **Por qué no se tira afuera:** las agencias buscan que los desechos de una misión tripulada no contaminen Marte (protección planetaria, ver `05_datos_nasa.md`).
- **Post-PMV:** ampliar los tanques cuando crece la población; pérdida del lote si un PEM o una tormenta solar corta la energía (las bacterias se vuelven a sembrar); cultivo de **hongos**, que dejan **micelio y fibra** para trajes y para reparar fisuras.

### Minijuego: "Operación Sopa de Bacterias" (acordado)

Tarea puntual: ocupa un gato libre. Sigue el ciclo del edificio (preparar → procesar un tiempo → volver a preparar).

- **Panel:** dos agujas, **temperatura** y **humedad**, cada una con su zona verde. El frío de Marte baja la temperatura de forma constante.
- **Controles:** una perilla de **calor** (gasta electricidad) y un **aspersor** (gasta agua del pool, la misma que beben los gatos).
- **Zona verde (valores reales):** temperatura **55–65 °C**, que además elimina los patógenos de los desechos de gato; humedad **50–60 %**.
- **Fuera de la zona:**
  - Más de unos 70 °C: se sobrecalienta y las bacterias mueren; se pierde el lote.
  - Menos de 55 °C: el proceso se frena y el abono no queda sanitizado.
  - Humedad sobre 65 %: falta aire y las bacterias se "ahogan"; además se gasta agua de más.
  - Humedad bajo 40 %: el proceso casi se detiene.
- **Objetivo:** mantener las dos agujas en verde durante el tiempo del ciclo. El **rendimiento** depende del tiempo que estuvieron en verde.
- **Enseña:** economía circular (la materia orgánica no desaparece), que las bacterias necesitan rangos de temperatura y agua como cualquier ser vivo, y por qué las agencias tratan los desechos como un recurso.

#### Controles (propuesta)

| Control | Acción | Mouse | Teclado |
|---|---|---|---|
| Perilla de calor | Nivel continuo 0–100 %: se deja en una posición y se corrige | Arrastrar arriba o abajo sobre la perilla; rueda del mouse encima | Q / E |
| Aspersor | Rocía solo mientras se mantiene presionado | Mantener clic en el botón | Mantener Espacio |

- La aguja de temperatura tiene **inercia**: se acerca de a poco al nivel de la perilla, así que hay que anticiparse.
- Zona verde pintada en cada aguja; aviso corto de la IA cerca de los bordes; teclas visibles junto a cada control.
- **Esc** cierra el minijuego; el ciclo sigue con el último nivel de calor.
- **Opcional post-PMV:** el calor reseca la mezcla (la humedad baja más rápido con más calor).

## Salud y trajes espaciales

### Barras

- Cada gato tiene dos barras: **traje** (integridad) y **salud**.
- El traje protege contra la **radiación** y mantiene la **presión atmosférica**. En la realidad, el control de presión es un sí o no; el **umbral del 20 %** es referencial y sirve para unir radiación y presión en una sola mecánica.
- La **radiación permanente** y el tiempo de exposición desgastan el traje.
- Los edificios también tienen **escudo** (ver `04_economia_y_balance.md`).

### Dos formas de caer

| Caso | Qué pasa |
|---|---|
| **Desgaste gradual** | El traje llega al 20 % sin que lo rompa un evento. La **salud** empieza a bajar; si baja del **50 %**, el gato se pone mal y **cae**. Da más margen para reaccionar. |
| **Ruptura por evento** | Un evento (arena u otro destructivo) baja el traje por debajo del 20 %. Se tira la probabilidad: **50 % muerte en el acto**, **50 % degradación** y el gato cae. |

### Gato caído

1. Camina **5 segundos** hacia la base y cae.
2. Si llega a una **esclusa**, vive y va al **depósito**.
3. Si cae afuera, el jugador puede **auxiliarlo** con un botón. Cuesta un **gato libre**; si no hay gatos libres, no se puede.
4. Si no es rescatado en **1 minuto**, muere.

Un gato que sobrevive a la ruptura del traje, o que cayó por desgaste gradual y durante el rescate llegó al **25 % de salud**, queda **degradado**: está demasiado mal para soportar el exterior y **no vuelve a salir**, aunque esté libre. Puede seguir trabajando dentro de la base. El **ala médica** es una propuesta fuera del PMV.

### Estadísticas por gato

| Estadística | Notas |
|---|---|
| Hambre | Individual, en tiempo real (ver `04_economia_y_balance.md`) |
| Sed | Individual, en tiempo real |
| Salud | Baja por traje al 20 %, sobretrabajo y hambre |
| Traje (integridad) | Cada traje es un recurso con su propia integridad |
| Estado | Ver tabla de estados |
| Degradado | Sí o no, permanente |

### Estados del gato (aprobado)

Para la máquina de estados de Lógica:

| Estado | Cuenta como libre | Descripción |
|---|---|---|
| Libre | Sí | Disponible para cualquier puesto permitido |
| Trabajando (interior) | No | En un puesto dentro de la base |
| Trabajando (exterior) | No | En un puesto fuera; con traje |
| Caído | No | Herido afuera; corre el minuto de rescate |
| Recuperando | No | En el depósito tras entrar por la esclusa o ser auxiliado; dura un tiempo y luego pasa a Libre |
| Descansando | No | De noche; solo sale de este estado por sobretrabajo de emergencia (post-PMV) |
| Buscando comida | No | Hambre extrema (6–7 minutos sin comer): deja de trabajar y va a comer |
| Muerto | — | Fuera del juego |

Además del estado, un **atributo permanente** `Degradado` (sí o no). El asignador automático nunca envía a un gato degradado a puestos exteriores ni a sobretrabajo nocturno. Para el sobretrabajo ordena a los gatos de más a menos salud. Para comer, ordena de más a menos hambre. Así no hace falta un estado falso de "trabajo" para retenerlo: el estado `Recuperando` cubre la pausa temporal y el atributo cubre la restricción permanente.

### Fabricación

- Los trajes se fabrican en el **taller** con **fibra y un poco de metal**. No se construyen en el PMV.
- Cada traje tiene su propia integridad. Se **reparan automáticamente con el tiempo** en el taller.
- **En el PMV los trajes no se degradan** (ver reglas del PMV en `02_core_loop.md`), así que no hace falta repararlos.

## Pendientes

1. De dónde sale la **luz** (el abono ya es recurso del pool: acordado).
2. **Hidroponía:** se investiga más tarde en el árbol de conocimiento (post-PMV). Falta definir su mecánica.
3. Valores reales de la preparación (NASA, artículo o funcionamiento en la Tierra).
4. Insumos y minijuego de la **mina de tierras raras** (post-PMV).
5. Confirmar la cadena **tierras raras → chips → robots (chips + metal)** en el taller.
6. Cuántos chips cuesta cada edificio nuevo o mejorado, y qué edificios de extracción los piden.
7. Cuánto dura el funcionamiento de cada edificio antes de volver a prepararlo.
8. Si con el rango prohibitivo los insumos ya puestos se descuentan o no.
9. **Agua** (paso 2 acordado): balance del círculo (arranque en 40 %, 5 s, velocidad de bajada) si resulta muy difícil; costo del botón acelerar; arte faltante. Para el PMV: cuántas vetas hay y con qué profundidad, pureza y volumen; si van los tres pasos; cuánta electricidad gasta perforar por metro.
19. **Reciclador de agua (post-PMV):** porcentaje de recuperación por nivel de investigación, costo de reparación y consumo de electricidad.
20. Tope de los tanques de agua.
21. **Granja de ratones:** insumos, minijuego, cuánta comida y cuántos desechos produce.
22. **Reciclador orgánico:** confirmar los controles; desechos por gato y por ratón por sol; restos por cosecha; cuánto abono rinde cada lote; duración del ciclo; cuánta salud se pierde con el depósito lleno de desechos.
23. **Aire:** aprobar la meta de 21 % y las franjas; volumen y fuga de cada edificio; consumo por gato (en reposo y trabajando); margen de reacción bajo 10 %; cuánto aportan las plantas.
10. **Operación Rescate Solar** (acordada): costo por disparo y velocidad de la banda según la producción de los paneles en `Data/`; las baterías iniciales deben alcanzar para la limpieza y para la primera noche (regla 6 del PMV); observaciones pendientes del arte.
11. Costo en metal y escombros de cada reparación.
12. Cuánto produce y consume cada edificio, y cuánto consume cada gato.
13. En qué zona está cada edificio.
14. Ritmo al que baja la salud con el traje al 20 %, duración del estado Recuperando y tiempo de reparación de un traje en el taller (post-PMV).
15. "Buscando comida" se agregó a los estados por la regla de hambre: confirmar.
16. **MOXIE** (entra en el PMV, acordado): cada cuánto se ensucia un filtro, cuánto dura limpiarlo, producción con los 5 limpios y consumo de electricidad.
17. Ritmo de desgaste de la integridad del reactor, costo de las correcciones de seguridad y tamaño del radio de contaminación (post-PMV).
18. Contrato de cada sistema para Lógica (clase y namespace `MarsCat.Logic.<Sistema>`), que se acuerda en el issue.
