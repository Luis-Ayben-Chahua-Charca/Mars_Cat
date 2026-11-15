# 04 — Economía y balance

**Estado:** borrador
**Decidido en:** #__
**Pendientes:** ver la sección "Pendientes" al final.

Los recursos y edificios están en `03_sistemas.md`. Esta página describe cómo fluyen. Las cifras son valores de diseño y pasan a `MarsCat/Assets/Data/` cuando Lógica cree el archivo de balance.

## Ciclo de día y noche

- Hay **día y noche**. Los gatos trabajan de día; de noche descansan.
- De noche la **energía** se consume para **mantener la temperatura** de la base.
- El ciclo de día y noche **entra en el PMV**.
- **Emergencia (post-PMV):** el jugador puede activar **sobretrabajo** nocturno en un edificio. Van primero los gatos **más saludables**; solo si no queda otra opción, los más enfermos. Los **degradados nunca**: morirían.
  - El sobretrabajo cuesta un poco de **salud**, sin importar el traje. Afuera, se suma al daño por radiación si el traje está roto.

## Consumo, hambre y sed

- La comida y el agua se consumen **en tiempo real**, con **estadísticas por gato** (ver `03_sistemas.md`).
- **Hambre y sed siguen la misma lógica**, cada una con sus propios tiempos:
  - El gato come o bebe cada cierto **intervalo**.
  - Pasado un umbral sin comer o beber salta una **alerta extrema**.
  - Pasado otro umbral, **muere**.
- **Orden:** un gato come o bebe cuando lo necesita. Si hay gatos en alerta extrema, van primero los más cercanos a morir.
- **Alerta extrema:** el gato no trabaja y busca comida o agua. Si estaba trabajando, apenas la haya deja lo que hace y va.
- Los tiempos se miden en **fracciones del ciclo de día y noche (sol)**, no en minutos fijos, para que escalen con la duración del ciclo.

### Referencia real (gatos en la Tierra)

Según Cats Protection (Reino Unido):

| Necesidad | Problemas serios | Supervivencia máxima |
|---|---|---|
| Comida (con agua) | A los 2 días sin comer | Hasta unas 2 semanas |
| Agua | Deshidratación en 24 horas | Unos 3–4 días |

El agua es cerca de **4 veces más urgente** que la comida.

### Duración del sol (propuesta recomendada)

- **1 sol = 8 minutos reales:** 6 minutos de día y 2 minutos de noche.
- De noche el tiempo del juego corre **3 veces más rápido**. Así día y noche duran lo mismo en tiempo de juego (como en Marte, cerca del ecuador), pero la noche, en la que los gatos no trabajan, no se vuelve aburrida.
- Referencias de otros juegos: un día de *Don't Starve* dura 8 minutos y uno de *RimWorld*, unos 17 minutos a velocidad normal.
- Con 8 minutos por sol, el tutorial cabe en 1 o 2 soles (8–16 minutos), una duración razonable para una demo.
- El hambre y la sed siguen corriendo de noche: los gatos descansan, pero comen y beben. Sus tiempos se cuentan en **minutos reales** (los de la tabla de abajo); la aceleración nocturna solo afecta al reloj del juego y al gasto de energía de la noche.

### Tiempos de hambre y sed (proporción 2:1 acordada; valores propuestos)

El agua es el doble de urgente que la comida. Entre la alerta y la muerte queda un **margen de reacción** suficiente para que el jugador actúe: 2 minutos para la comida y 1 minuto para el agua.

| Necesidad | Intervalo | Alerta extrema | Muerte | Margen de reacción |
|---|---|---|---|---|
| Comida | 0,5 sol (4 min) | 1 sol (8 min) | 1,25 sol (10 min) | 2 min |
| Agua | 0,25 sol (2 min) | 0,5 sol (4 min) | 0,625 sol (5 min) | 1 min |

Para que sea divertido, la producción de cada edificio reparado debería superar el consumo de la colonia por sol con margen. El jugador debe notar el progreso apenas un edificio funciona.

## Almacenamiento

- **Energía y agua:** almacenamientos independientes, con tope (el agua, en **tanques**).
- **Oxígeno:** no es un tanque, es el **aire de la base**, medido en % de O₂ (ver "Aire y oxígeno"). El tope crece con el árbol tecnológico (en energía, con la investigación nuclear). El tope de energía obliga a repartirla bien, porque la solar es limitada y fluctuante.
- **Energía sin merma:** no se pierde con el tiempo. Hay **baterías de emergencia** reparables.
- **Alimentos, fibra, escombros y desechos:** físicos y visibles, en un **almacén compartido** (un solo edificio: el **depósito**, ver abajo). El jugador debe vigilar qué está acumulando y qué necesita.
- **Exceso:** si el almacén está lleno, la producción se detiene y queda **en cola**; el jugador debe permitir a mano que entre al almacén.

## El depósito

Edificio principal en la zona segura. Es donde sobrevivieron los gatos congelados y donde están al inicio la comida, el agua y los materiales. Los gatos heridos que entran por una esclusa se recuperan ahí.

## Fuentes

1. **Reservas iniciales** de materiales.
2. **Escombros** de los edificios destruidos.
3. **Nodos de mineral marciano** cerca, en la cueva o en la superficie. Los minerales salen siempre de datos de NASA. El **regolito** es infinito. El **metal** se extrae de Marte y también se recupera de los escombros.
4. **Edificios:**
   - Granja de plantas: alimento, **fibra** y algo de oxígeno.
   - Granja de ratones: alimento (proteína) y **desechos de ratones**.
   - Edificio de desechos: **abono**, a partir de los desechos de gatos y de ratones.
   - Energía: electricidad.
   - **Extracción de agua del subsuelo:** agua de hielo purificado (ver "Agua" en `03_sistemas.md`).
   - **Reciclador de agua (post-PMV):** recupera parte del agua consumida; dañado al inicio. Su eficiencia sube por niveles en el árbol tecnológico.
   - **MOXIE** (edificio industrial, PMV): fuente principal de oxígeno; depende de la electricidad y de sus filtros.

Para reparar un edificio caído, los escombros cubren cerca del **90 %** del costo; el **10 %** restante son minerales marcianos.

## Transformación

En un edificio se juntan los insumos y sale el producto; el gato solo lo trabaja. Las cadenas acordadas (por ejemplo, el taller) están en `03_sistemas.md`.

## Sumideros

- **Pasivos (por ciclo):**
  - Oxígeno: según la cantidad de gatos y cuánto trabajan, más una pequeña **fuga por cada edificio** presurizado.
  - Comida: según la cantidad de gatos.
  - **Desechos:** cada gato genera desechos por sol, y la granja de ratones suma los suyos. Se **almacenan en el depósito** y ocupan espacio hasta que aparece el edificio de desechos y los procesa.
  - Energía: según los edificios activos y, de noche, para mantener la temperatura.
  - La **protección** de los edificios está siempre activa y siempre consume energía.
- **Activos:** la **reparación del beacon** para enviar la alerta a la Tierra.
- **Construcción de edificios nuevos o mejorados (post-PMV):** consume **chips**, lo que mantiene activa la cadena mina → taller (ver `03_sistemas.md`).
- **Mantenimiento del reactor (post-PMV):** las correcciones de seguridad gastan materiales; si se descuidan, hay riesgo de accidente nuclear (ver `03_sistemas.md`).

## Entropía y fallos en cascada

- **Radiación permanente:** degrada edificios y trajes.
- **Edificios:** tienen un **escudo**. Si se pierde, el interior se vuelve peligroso para los gatos (cambios bruscos de temperatura); si no se repara a tiempo, el edificio cae total o parcialmente. Los fenómenos del desierto abren pequeñas **grietas** que dañan la estructura.
- **Sin energía**, cuando se agotan las baterías, los edificios dejan de funcionar y arrastran al resto, en especial la protección contra radiación y el oxígeno. Con el tiempo faltan los demás recursos y los gatos mueren.
- **Válvula de emergencia de energía:** reparar y reconectar paneles lejanos, a costa de recursos.
- **Sin comida**, hay menos gatos trabajadores.
- Los únicos recursos externos son los nodos de mineral y la energía del sol.

## Gatos heridos

Un gato herido fuera de la base puede ser **auxiliado** a costa de un gato libre; si llega a una esclusa, va al **depósito**. Detalle en `03_sistemas.md`. El **ala médica** es una propuesta fuera del PMV.

## Aire y oxígeno

Modelo propuesto en "Oxígeno" de `03_sistemas.md`: barra de % de O₂ con meta de 21 %, cambio por tick = (producción − consumo − fugas) ÷ volumen de la base. Más edificios dan más **reserva** (el % cambia más lento) pero suman **fugas**; el consumo depende de los gatos.

## Asignación de trabajo

Automática, al estilo de *RimWorld* u *Oxygen Not Included*. Hay dos tipos de trabajo:

| Tipo | Ejemplos | Duración |
|---|---|---|
| **Puesto fijo** | Operar un edificio de producción | Mientras el edificio funcione |
| **Tarea puntual** | Reparar, limpiar filtros del MOXIE, auxiliar a un gato caído, jugar un minijuego de preparación | Hasta terminar la tarea |

- Si hay gatos libres y puestos libres, se asignan solos.
- **Las tareas puntuales tienen prioridad** sobre los puestos fijos.
- **Sin gatos libres:** la tarea toma un gato de un puesto fijo según un **orden de prioridad de edificios** (en `Data/`; primero sale el del edificio menos crítico). El edificio rinde menos mientras falta ese gato: ese es el costo real de la tarea.
- **Liberación manual:** al iniciar la tarea, el jugador puede elegir **de qué edificio** sale el gato en lugar de aceptar el automático. No hay asignación manual por edificio como en *Frostpunk*.
- Si no hay ningún gato que pueda hacerla (todos en alerta extrema, degradados para el exterior o muertos), el botón queda desactivado y la IA lo explica.
- Al terminar la tarea, el gato vuelve a estar libre y el asignador lo devuelve a un puesto.
- La construcción y las tareas se hacen **de día**. De noche no se trabaja, salvo sobretrabajo de emergencia (ver arriba).

## Escalado

- Más edificios consumen más energía y exigen más trabajo y más salidas al exterior, con más exposición y más trajes.
- El **árbol tecnológico** mejora la producción de alimentos, la eficiencia energética y la producción de trajes.
- Recursos intangibles: solo **salud**. No hay moral ni esperanza.

## Pendientes

1. Topes de almacenamiento y tamaño del lote (el documento antiguo menciona 15 unidades por lote de plantas como ejemplo).
2. Consumo por gato y por edificio.
3. Si el tope del depósito y la cola de exceso entran en el PMV (el depósito en sí está en la zona segura).
4. Aprobar la duración del sol (8 minutos, noche acelerada 3 veces) y los tiempos de hambre y sed.
5. Producción por sol de cada edificio frente al consumo de la colonia.
6. Cuánta salud cuesta el sobretrabajo (post-PMV).
