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

- **Oxígeno y energía:** almacenamientos independientes, con tope. El tope crece con el árbol tecnológico.
- **Alimentos, fibra y escombros:** físicos y visibles, en un **almacén compartido** (un solo edificio: el **depósito**, ver abajo). El jugador debe vigilar qué está acumulando y qué necesita.
- **Exceso:** si el almacén está lleno, la producción se detiene y queda **en cola**; el jugador debe permitir a mano que entre al almacén.

## El depósito

Edificio principal en la zona segura. Es donde sobrevivieron los gatos congelados y donde están al inicio la comida, el agua y los materiales. Los gatos heridos que entran por una esclusa se recuperan ahí.

## Fuentes

1. **Reservas iniciales** de materiales.
2. **Escombros** de los edificios destruidos.
3. **Nodos de mineral marciano** cerca, en la cueva o en la superficie. Los minerales salen siempre de datos de NASA. El **regolito** es infinito. El **metal** se extrae de Marte y también se recupera de los escombros.
4. **Edificios:**
   - Plantas: alimento, **fibra** y oxígeno.
   - Energía: electricidad.
   - **MOXIE** (edificio industrial): producción neta de oxígeno.

Para reparar un edificio caído, los escombros cubren cerca del **90 %** del costo; el **10 %** restante son minerales marcianos.

## Transformación

En un edificio se juntan los insumos y sale el producto; el gato solo lo trabaja. Las cadenas acordadas (por ejemplo, el taller) están en `03_sistemas.md`.

## Sumideros

- **Pasivos (por ciclo):**
  - Oxígeno y comida: según la cantidad de gatos.
  - Energía: según los edificios activos y, de noche, para mantener la temperatura.
  - La **protección** de los edificios está siempre activa y siempre consume energía.
- **Activos:** la **reparación del beacon** para enviar la alerta a la Tierra.

## Entropía y fallos en cascada

- **Radiación permanente:** degrada edificios y trajes.
- **Edificios:** tienen un **escudo**. Si se pierde, el interior se vuelve peligroso para los gatos (cambios bruscos de temperatura); si no se repara a tiempo, el edificio cae total o parcialmente. Los fenómenos del desierto abren pequeñas **grietas** que dañan la estructura.
- **Sin energía**, un edificio no puede trabajar y arrastra al resto, en especial la protección contra radiación y el oxígeno.
- **Sin comida**, hay menos gatos trabajadores.
- Los únicos recursos externos son los nodos de mineral y la energía del sol.

## Gatos heridos

Un gato herido fuera de la base puede ser **auxiliado** a costa de un gato libre; si llega a una esclusa, va al **depósito**. Detalle en `03_sistemas.md`. El **ala médica** es una propuesta fuera del PMV.

## Asignación de trabajo

- **Automática:** si hay gatos libres y puestos libres, se asignan solos.
- La construcción se hace **de día**, si hay gatos libres.
- De noche no se trabaja, salvo sobretrabajo de emergencia (ver arriba).

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
