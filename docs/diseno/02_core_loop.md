# 02 — Core loop

**Estado:** borrador
**Decidido en:** #__
**Pendientes:** ver la sección "Pendientes" al final.

## Etapas del juego

| # | Etapa | Tipo | Qué pasa |
|---|---|---|---|
| 1 | Evacuación | Cinemática | Se muestra la evacuación de los humanos y la destrucción de la base por la lluvia de meteoritos. |
| 2 | Despertar | Primer momento jugable | Los gatos despiertan en la **zona segura**. No hay suficiente comida ni agua para sobrevivir, así que necesitan **reparar las zonas cercanas** y **construir módulos de agua y comida**. |

Los módulos de agua y comida están **por definir** en el issue #__.

## Zonas de la base

| Zona | Estado tras la lluvia de meteoritos | En el PMV |
|---|---|---|
| Segura (cueva) | Estable | Sí |
| Media | Dañada, aún reparable | Sí |
| Lejana | Destruida, no reparable | No |

## Estado inicial (etapa 2)

Los gatos empiezan en la zona segura con:

- Oxígeno.
- Poca electricidad.
- Poca comida.
- Materiales de construcción: **metal** y **escombros**, en la cantidad justa para todos los edificios del tutorial (la mayoría se perdió en el cataclismo).

## Tutorial

Orden de pasos:

1. Cinemática (etapa 1).
2. Mover la cámara.
3. Reparar el edificio de **energía** (paneles solares).
4. Reparar el edificio de **producción de alimentos** (plantas y ratones).
5. Reparar el **sistema de extracción de agua del subsuelo**.
6. Trajes.
7. Más pasos por definir.

La reparación de cada edificio es **automática**; después viene la fase de **preparación** (minijuego) para que funcione. La preparación **entra en el PMV** para los edificios de energía, agua y alimentos.

Detalle de edificios, recursos, preparación y trajes en `03_sistemas.md`.

## PMV (entrega del domingo 15)

- Incluye **solo el tutorial** descrito arriba, en las zonas segura y media.
- Incluye la **preparación** de los edificios de energía, agua y alimentos.
- **No incluye:** la zona lejana, la construcción de trajes con fibra, las tierras raras ni la automatización.

## Momentos narrativos (partida completa)

| # | Momento | Qué pasa | Entrega |
|---|---|---|---|
| 1 | Catástrofe | Evacuación y destrucción; control de daños | Cinemática inicial |
| 2 | Reparación | Tutorial: reparar y preparar edificios | Diálogos de la IA |
| 3 | Supervivencia | El mapa se amplía; exploración. En la zona nueva se descubre una **antigua mina de tierras raras** | Diálogos de la IA + mapa |
| 4 | Esperanza | Una tormenta desentierra el **beacon**: la misión es repararlo para comunicarse con la Tierra | Diálogo de la IA + cinemática del beacon |
| 5 | Catástrofe | Al emitir la señal empieza la crisis máxima | — |
| 6 | Apocalipsis | Clímax: todo se vuelve manual y las producciones se alargan | — |
| 7 | Salvación | Los humanos rescatan a los gatos en la ventana de calma de las tormentas | Cinemática final |

El PMV cubre los momentos 1 y 2. La curva de eventos de los momentos 4 a 6 está en `06_eventos_y_ritmo.md`.

El mapa crece de pequeño a grande a medida que se obtiene **información** (post-PMV): en cada zona, al completar un hito (por ejemplo, extraer algo del suelo), la IA aprende más del terreno y se abren nuevas zonas. Los hitos de zona están por definir.

**Victoria:** sobrevivir hasta el rescate, 15 días después del aviso de los humanos (ver `07_lore.md`).

## Pendientes

1. El tutorial original tenía un paso de **construir** un edificio; la lista actual solo tiene **reparaciones**. ¿Algún edificio se construye desde cero, o las reparaciones consumen el metal y los escombros?
2. ¿Qué se enseña de los trajes en el tutorial, si construirlos con fibra no entra en el PMV?
3. Qué módulos de agua y comida existen y cómo funcionan (issue #__).
4. Qué otros pasos lleva el tutorial después de los trajes.
5. Condición de derrota y duración de la partida.
6. Niveles de progresión (idea previa: tres niveles, el primero de recursos) y qué hitos abren cada zona (post-PMV).
7. Eventos aleatorios como presión y ritmo → `06_eventos_y_ritmo.md`.
