# 03 — Sistemas

**Estado:** borrador
**Decidido en:** #__
**Pendientes:** ver la sección "Pendientes" al final.

Los porcentajes y cantidades de esta página son **ejemplos**: los valores reales deben salir de datos de NASA, de artículos de investigación o, si no hay, de cómo funciona en la Tierra, siempre realistas (ver `README.md` y `05_datos_nasa.md`). Cuando Lógica cree el archivo de balance en `MarsCat/Assets/Data/`, pasan a vivir ahí y esta página solo explica su porqué (ver `README.md`).

## Recursos

| Recurso | Tipo | En el PMV |
|---|---|---|
| Oxígeno | Soporte vital | Sí |
| Electricidad | Soporte vital | Sí |
| Comida | Soporte vital | Sí |
| Agua | Soporte vital; se guarda en **tanques** propios, no en el depósito | Sí |
| Metal | Material de construcción; insumo de robots y trajes. Se extrae de Marte y se recupera de escombros | Sí |
| Escombros | Material de construcción | Sí |
| Bacterias | Insumo de la preparación del cultivo. **Infinito**: solo se ajusta su valor óptimo en el minijuego | Sí |
| Fibra | Insumo de trajes | No |
| Regolito (tierra) | Tierra del minijuego de alimentos. **Infinito**: está en la superficie marciana | Sí |
| Tierras raras | Insumo de chips. Se producen en un edificio propio | No |
| Chips | Se fabrican con tierras raras. Materia prima de robots y de la **construcción** de edificios complejos (post-PMV) | No |
| Robots | Automatizan la preparación. Se fabrican con chips y metal | No |

Los minerales del juego salen siempre de datos de NASA. Insumo del cultivo aún sin confirmar como recurso del pool general: **abono**. La **luz** se expresa en %; por confirmar si sale de la electricidad.

## Edificios

| Edificio | Produce | Insumos | En el PMV | Zona |
|---|---|---|---|---|
| Energía (paneles solares) | Electricidad, solo de día; nada durante tormentas | Limpieza con CO₂ comprimido, que cuesta energía (ver "Energía") | Sí: reparar y preparar | Por definir |
| Baterías de emergencia | Guardan electricidad para la noche y emergencias; reparables | — | Por definir | Por definir |
| Reactor KRUSTY (Kilopower) | Electricidad constante, sin depender del clima ni del día | Costo único de construcción (minerales, metal y chips); materiales para correcciones de seguridad | No | Por definir |
| Producción de alimentos (plantas y ratones) | Comida; también fibra y oxígeno | Regolito, agua, luz, bacterias, abono (la hidroponía se desbloquea post-PMV en el árbol de conocimiento) | Sí: reparar y preparar | Por definir |
| Extracción de agua del subsuelo | Agua (hielo bajo la cueva, purificado) | Electricidad (taladro térmico y calentador); bacterias perclorato-reductoras (infinitas) | Sí: reparar y preparar | Segura (propuesta) |
| Reciclador de agua (soporte vital) | Recupera parte del agua consumida | Electricidad | Por definir (dañado al inicio) | Segura |
| Mina de tierras raras | Tierras raras | Por definir (tiene su propio minijuego) | No | Zona nueva (exploración) |
| MOXIE (industrial) | Oxígeno neto | Por definir | No | Por definir |
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
| Producción de alimentos | Preparación del cultivo | Valor guía y rango por insumo (abajo) |
| Extracción de agua | "Alquimista Marciano" | Ubicación y profundidad de perforación, secuencia de presión y biorreactor (ver "Agua") |
| Energía | "Operación Rescate Solar" | Acción: limpiar polvo gastando batería (ver "Energía") |

### Producción de alimentos: valor guía y rango

- Cada insumo tiene un **valor guía** que da la IA y un **rango de aceptación**.
- **Fuera del rango:** se muestra una **advertencia de rango prohibitivo** que indica qué insumo hay que corregir; el edificio no arranca hasta corregirlo.
- **Dentro del rango:** ciertos valores y **combinaciones** mejoran el **rendimiento**.

#### Ejemplo

Valores de ejemplo, no reales.

| Insumo | Valor guía | Rango de aceptación |
|---|---|---|
| Regolito (tierra) | 20 % | 15 %–25 % |
| Agua | 2 L | Por definir |
| Luz | Por definir (%) | Por definir |
| Bacterias | Por definir (cantidad) | Por definir |
| Abono | Por definir (kg) | Por definir |


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

### Minijuego candidato de preparación de energía: "Operación Rescate Solar"

- El gato, como "conserje espacial", limpia el polvo de los paneles antes de que caiga la noche, con ráfagas de CO₂ comprimido y el cepillo antiestático.
- Una barra de "luz solar capturada" muestra el efecto.
- **Balance:** cada ráfaga gasta energía de las baterías. Mantener el disparador apretado desperdicia energía y la base puede quedarse corta para la noche; conviene usar ráfagas cortas y el cepillo en seco.
- Ráfagas de viento traen más polvo y obligan a ser rápido.
- Es un minijuego de **acción**, distinto del modelo de valor guía y rango del cultivo.
- **Enseña:** por qué el agua líquida no sirve en Marte, y el concepto de **ISRU** (usar recursos del propio planeta, como el CO₂, a cambio de energía).
- Si es la preparación de energía del PMV está **pendiente de revisión**.

### Minijuego post-PMV: "Forja KRUSTY"

Recolección de minerales y metal, ensamblaje del reactor (núcleo → tubos de sodio → motor Stirling) y regulación de válvulas para mantener el motor en la zona verde. Enseña la idea de calor → movimiento → electricidad.

### Propuesta de lógica de rendimiento del cultivo (no acordada)

- **Puntaje por insumo:** `s = 1 − |valor − guía| / tolerancia`. Vale 1 en el valor guía y 0 en el borde del rango.
- **Rendimiento base:** `rendimiento = 0,8 + 0,4 × promedio(s)`. Va de 80 % (todo en el borde) a 120 % (todo en el valor guía).
- **Combinaciones:** una tabla de sinergias por edificio. Si dos insumos de la sinergia tienen `s ≥ 0,8`, suman un bono fijo (por ejemplo +10 %). Para el PMV se puede dejar sin combinaciones.
- **Automatización (post-PMV):** el robot pone todos los insumos en su valor guía; el resultado es fijo y rápido. Hacerlo a mano permite buscar combinaciones y superar ese rendimiento.
- **Datos reales:** los valores guía y rangos deberían salir de datos de NASA sobre cultivo en el espacio y registrarse en `05_datos_nasa.md`. No se inventan.

Para Lógica: un **resultado común** (`PreparationResult`: arranca, rendimiento, motivo) y una clase de preparación **por minijuego**, cada una con su configuración en `Data/`. El ciclo del edificio solo lee el resultado, sin saber qué minijuego lo produjo. El contrato se acuerda en el issue.

## Agua

Propuesta del equipo en `docs/Propuesta_mecanica/mecanica_agua.md`, adaptada aquí. **Propuesta, no acordada.**

- **Recurso físico.** Barra global en la interfaz. Se guarda en **tanques** con tope propio, como el oxígeno; no ocupa el depósito.
- **Fuentes:**
  1. **Reciclador (soporte vital):** recupera parte del agua que consumen gatos y cultivos, como el sistema de la ISS. La lluvia de meteoritos lo dejó dañado; por eso, al despertar, el agua no alcanza (etapa 2 de `02_core_loop.md`).
  2. **Extracción de agua del subsuelo:** el edificio del tutorial (paso 5). Saca **hielo** de debajo de la cueva, lo derrite en una cámara presurizada y lo limpia de **percloratos** en un biorreactor antes de mandarlo a los tanques.
- **Consumo:** la sed de cada gato (`04_economia_y_balance.md`) y la preparación de la **producción de alimentos**. La hidroponía es post-PMV.
- **Exceso:** con los tanques llenos, la extracción se detiene y queda en cola, igual que el depósito. El agua no se pierde.
- **Sin agua:** rige la regla de sed (alerta extrema, busca agua, muere). La producción de alimentos no arranca porque el insumo agua queda fuera de rango.
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

**Paso 2 — Cámara de derretido**

- Secuencia corta: primero **presurizar**, después **calentar**.
- Si calienta antes de presurizar, el agua **hierve**: se pierde el lote y la IA explica por qué.

**Paso 3 — Biorreactor**

- La **dosis de bacterias** perclorato-reductoras depende de la pureza de la veta: hielo sucio necesita más bacterias y más tiempo.
- El jugador mantiene la **temperatura** tibia mientras el agua pasa de púrpura o marrón a azul.
- Si se queda corto, el agua sigue tóxica y no entra a los tanques.

**Rendimiento:** volumen de la veta × pureza lograda × (1 − pérdidas por sublimación o ebullición). Una veta profunda bien trabajada rinde más, pero gasta más energía: el jugador decide según cuánta electricidad tenga.

- **Energía:** el taladro y el calentador gastan electricidad al preparar. Refuerza la regla del tutorial de reparar primero la energía (paso 3) antes que el agua (paso 5).
- **Enseña:** que el hielo marciano se busca con radar y está a distintas profundidades, por qué el agua líquida no es estable a la presión de Marte, qué son los percloratos y la **biorremediación**.

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

1. Si el **abono** es recurso del pool general, y de dónde sale la **luz**.
2. **Hidroponía:** se investiga más tarde en el árbol de conocimiento (post-PMV). Falta definir su mecánica.
3. Valores reales de la preparación (NASA, artículo o funcionamiento en la Tierra).
4. Insumos y minijuego de la **mina de tierras raras** (post-PMV).
5. Confirmar la cadena **tierras raras → chips → robots (chips + metal)** en el taller.
6. Cuántos chips cuesta cada edificio nuevo o mejorado, y qué edificios de extracción los piden.
7. Cuánto dura el funcionamiento de cada edificio antes de volver a prepararlo.
8. Si con el rango prohibitivo los insumos ya puestos se descuentan o no.
9. Aprobar la propuesta de **Agua** y su preparación "Alquimista Marciano" (arriba). Para el PMV: cuántas vetas hay y con qué profundidad, pureza y volumen; si van los tres pasos; cuánta electricidad gasta perforar por metro.
19. **Reciclador de agua:** si entra en el PMV (reparable o solo como explicación de la IA) y qué porcentaje recupera.
20. Tope de los tanques de agua.
10. Si "Operación Rescate Solar" es la preparación de energía del PMV (pendiente de revisión). Si entra, las baterías iniciales deben alcanzar para la limpieza y para la primera noche (regla 6 del PMV).
11. Costo en metal y escombros de cada reparación.
12. Cuánto produce y consume cada edificio, y cuánto consume cada gato.
13. En qué zona está cada edificio.
14. Ritmo al que baja la salud con el traje al 20 %, duración del estado Recuperando y tiempo de reparación de un traje en el taller (post-PMV).
15. "Buscando comida" se agregó a los estados por la regla de hambre: confirmar.
16. Si el MOXIE entra en el PMV.
17. Ritmo de desgaste de la integridad del reactor, costo de las correcciones de seguridad y tamaño del radio de contaminación (post-PMV).
18. Contrato de cada sistema para Lógica (clase y namespace `MarsCat.Logic.<Sistema>`), que se acuerda en el issue.
