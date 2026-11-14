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

La reparación de cada edificio es **automática**; después viene la fase de **preparación** (minijuego) para que funcione. Si la preparación entra en el PMV está por confirmar.

Detalle de edificios, recursos, preparación y trajes en `03_sistemas.md`.

## PMV (entrega del domingo 15)

- Incluye **solo el tutorial** descrito arriba, en las zonas segura y media.
- **No incluye:** la zona lejana ni la construcción de trajes con fibra.

## Pendientes

1. El tutorial original tenía un paso de **construir** un edificio; la lista actual solo tiene **reparaciones**. ¿Algún edificio se construye desde cero, o las reparaciones consumen el metal y los escombros?
2. ¿Qué se enseña de los trajes en el tutorial, si construirlos con fibra no entra en el PMV?
3. Qué módulos de agua y comida existen y cómo funcionan (issue #__).
4. Qué otros pasos lleva el tutorial después de los trajes.
5. Condición de victoria y de derrota.
6. Niveles de progresión (idea previa: tres niveles, el primero de recursos).
7. Eventos aleatorios como presión y ritmo → `06_eventos_y_ritmo.md`.
