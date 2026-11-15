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
| Segura (cueva, con el depósito) | Estable | Sí |
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

Pipeline **acordado** para el PMV. Desde el paso 3, cada edificio sigue el mismo patrón: **misión de la IA → reparación → minijuego**.

| # | Paso | Misión de la IA | Notas |
|---|---|---|---|
| 1 | Cinemática (etapa 1) | — | Evacuación y lluvia de meteoritos |
| 2 | Mover la cámara | — | |
| 3 | **Energía** (paneles solares) | "No tenemos energía: reparen el edificio de energía" | Primero, porque todo lo demás la consume |
| 4 | **Oxígeno** (MOXIE) | "Ahora reparemos el MOXIE para tener una fuente de oxígeno estable" | Minijuego de filtros (ver "Oxígeno" en `03_sistemas.md`) |
| 5 | **Agua** (extracción del subsuelo) | Por redactar | La IA explica que el reciclador de agua quedó dañado y por eso hay que sacar agua del hielo |
| 6 | **Comida: granja de plantas** | Por redactar | Usa la **reserva inicial de abono**, suficiente para montar esta primera granja |
| 7 | **Comida: granja de ratones** | Por redactar | Los desechos de los gatos se guardan en el **depósito** desde el inicio; con los ratones se suman los suyos y el depósito se llena: aparece el último problema |
| 8 | **Desechos** | Por redactar | Procesa los desechos guardados en el depósito, los convierte en **abono** y cierra el ciclo con la granja de plantas |

La reparación de cada edificio es **automática**; después viene la fase de **preparación** (minijuego) para que funcione.

Detalle de edificios, recursos, preparación y trajes en `03_sistemas.md`.

## PMV (entrega del domingo 15)

- Incluye **solo el tutorial** descrito arriba, en las zonas segura y media.
- Incluye la **preparación** (o el mantenimiento, en el caso del MOXIE) de los edificios de energía, oxígeno, agua, granja de plantas, granja de ratones y desechos.
- Incluye el **ciclo de día y noche** y el **hambre individual** de cada gato.
- **No incluye:** la zona lejana, la construcción de trajes con fibra, las tierras raras, la automatización ni el sobretrabajo nocturno.

### Reglas del PMV (propuesta, no acordada)

Para que el tutorial sea seguro y los trajes no se degraden:

1. **Sin eventos destructivos.** No hay tormentas ni otros eventos que rompan trajes o edificios.
2. **Desgaste de trajes desactivado.** La barra de integridad se muestra pero no baja: la tasa de desgaste vale 0 en la configuración del escenario del tutorial.
3. **Paso "Trajes" del tutorial:** la IA explica la barra, la radiación en Marte (dato de RAD, ver `05_datos_nasa.md`) y el umbral del 20 %. Muestra qué pasaría con una **simulación de entrenamiento**, sin consecuencias reales.
4. **Sin caídas ni rescates.** Como los trajes no fallan, los estados Caído y Recuperando y la degradación no aparecen en el PMV; Lógica puede dejarlos para después.
5. **Comida y agua con margen.** Las reservas iniciales alcanzan hasta que funcionen los edificios de alimentos y agua, con margen. La alerta extrema puede aparecer como enseñanza, pero el tutorial no se pierde por hambre o sed.
6. **Primera noche segura.** El tutorial empieza al amanecer y la reparación de energía llega antes de la primera noche; si no, la energía inicial alcanza para esa noche.
7. **Sin derrota (acordado).** El tutorial es lineal y no se puede perder: está guiado hacia el **hito de exploración**, donde el jugador recibe total libertad (post-PMV).

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
2. Aprobar las reglas 1 a 6 del PMV (la 7 ya está acordada).
3. Qué módulos de agua y comida existen y cómo funcionan (issue #__).
4. El paso **Trajes** quedó fuera del pipeline acordado: decidir si va al final del tutorial o pasa a post-PMV (afecta la regla 3).
5. Condición de derrota y duración de la partida.
6. Niveles de progresión (idea previa: tres niveles, el primero de recursos) y qué hitos abren cada zona (post-PMV).
7. Eventos aleatorios como presión y ritmo → `06_eventos_y_ritmo.md`.
