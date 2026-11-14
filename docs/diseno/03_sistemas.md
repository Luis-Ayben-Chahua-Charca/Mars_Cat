# 03 — Sistemas

**Estado:** borrador
**Decidido en:** #__
**Pendientes:** ver la sección "Pendientes" al final.

Los porcentajes y cantidades de esta página son **ejemplos**: los valores reales deben salir de datos de NASA o de artículos de investigación (ver `05_datos_nasa.md`). Cuando Lógica cree el archivo de balance en `MarsCat/Assets/Data/`, pasan a vivir ahí y esta página solo explica su porqué (ver `README.md`).

## Recursos

| Recurso | Tipo | En el PMV |
|---|---|---|
| Oxígeno | Soporte vital | Sí |
| Electricidad | Soporte vital | Sí |
| Comida | Soporte vital | Sí |
| Agua | Soporte vital | Sí |
| Metal | Material de construcción; insumo de robots | Sí |
| Escombros | Material de construcción | Sí |
| Bacterias | Insumo de la preparación del cultivo. **Infinito**: solo se ajusta su valor óptimo en el minijuego | Sí |
| Fibra | Insumo de trajes | No |
| Regolito (tierra) | Tierra del minijuego de alimentos. **Infinito**: está en la superficie marciana | Sí |
| Tierras raras | Insumo de chips. Se producen en un edificio propio | No |
| Chips | Intermediario: se fabrican con tierras raras | No |
| Robots | Automatizan la preparación. Se fabrican con chips y metal | No |

Los minerales del juego salen siempre de datos de NASA. Insumo del cultivo aún sin confirmar como recurso del pool general: **abono**. La **luz** se expresa en %; por confirmar si sale de la electricidad.

## Edificios

| Edificio | Produce | Insumos | En el PMV | Zona |
|---|---|---|---|---|
| Energía (paneles solares) | Electricidad | Por definir | Sí: reparar y preparar | Por definir |
| Producción de alimentos (plantas y ratones) | Comida; también fibra y oxígeno | Regolito, agua, luz, bacterias, abono (alternativa: hidroponía, por definir si entra en el PMV) | Sí: reparar y preparar | Por definir |
| Extracción de agua del subsuelo | Agua | Por definir | Sí: reparar y preparar | Por definir |
| Mina de tierras raras | Tierras raras | Por definir (tiene su propio minijuego) | No | Zona nueva (exploración) |
| MOXIE (industrial) | Oxígeno neto | Por definir | No | Por definir |
| Almacén compartido | Guarda alimentos, fibra y escombros (ver `04_economia_y_balance.md`) | — | Por definir | Por definir |
| Taller | Chips, robots y trajes | Tierras raras → chips; chips + metal → robots; fibra + un poco de metal → trajes | No | Por definir |

## Ciclo de un edificio de producción

1. **Reparación:** automática.
2. **Preparación:** minijuego (ver abajo). Lo que pone el jugador se descuenta del pool general.
3. **Funcionamiento:** un **gato libre** se asigna **automáticamente** al puesto y trabaja en el edificio y este produce durante un tiempo.
4. Al terminar ese tiempo, se **vuelve a preparar a mano**.
5. Cuando el jugador tiene **robots**, la preparación se automatiza y deja de hacerse a mano.

Así el jugador aprende primero qué necesita cada edificio, y la automatización evita que se vuelva tedioso.

## Preparación de edificios (minijuego)

Inspirada en los minijuegos de *Among Us*.

- Cada insumo tiene un **valor guía** que da la IA y un **rango de aceptación**.
- **Fuera del rango:** se muestra una **advertencia de rango prohibitivo** que indica qué insumo hay que corregir; el edificio no arranca hasta corregirlo.
- **Dentro del rango:** ciertos valores y **combinaciones** mejoran el **rendimiento**.

### Ejemplo: producción de alimentos

Valores de ejemplo, no reales.

| Insumo | Valor guía | Rango de aceptación |
|---|---|---|
| Regolito (tierra) | 20 % | 15 %–25 % |
| Agua | 2 L | Por definir |
| Luz | Por definir (%) | Por definir |
| Bacterias | Por definir (cantidad) | Por definir |
| Abono | Por definir (kg) | Por definir |

La preparación de energía y de extracción de agua está por definir.

### Propuesta de lógica de rendimiento (no acordada)

- **Puntaje por insumo:** `s = 1 − |valor − guía| / tolerancia`. Vale 1 en el valor guía y 0 en el borde del rango.
- **Rendimiento base:** `rendimiento = 0,8 + 0,4 × promedio(s)`. Va de 80 % (todo en el borde) a 120 % (todo en el valor guía).
- **Combinaciones:** una tabla de sinergias por edificio. Si dos insumos de la sinergia tienen `s ≥ 0,8`, suman un bono fijo (por ejemplo +10 %). Para el PMV se puede dejar sin combinaciones.
- **Automatización (post-PMV):** el robot pone todos los insumos en su valor guía; el resultado es fijo y rápido. Hacerlo a mano permite buscar combinaciones y superar ese rendimiento.
- **Datos reales:** los valores guía y rangos deberían salir de datos de NASA sobre cultivo en el espacio y registrarse en `05_datos_nasa.md`. No se inventan.

Para Lógica, esto se traduce en una clase de preparación por edificio que recibe los insumos y la configuración (guía, tolerancia y sinergias desde `Data/`) y devuelve si arranca, con qué rendimiento o qué insumo está fuera de rango. El contrato se acuerda en el issue.

## Salud y trajes espaciales

- Cada gato tiene **dos barras**: la del **traje** (escudo) y la de **salud**. Al agotarse el traje, entra en juego la salud, que se consume rápido y tiene umbrales que definen el estado del gato.
- Los edificios también tienen **escudo** (ver `04_economia_y_balance.md`).
- Los trajes protegen a los gatos contra la **radiación** cuando salen de la base.
- Tienen una **barra de integridad**.
- **Umbral del 20 %:** representa el soporte vital de **presión atmosférica**. Cuando la integridad llega al 20 %, el traje comienza a **dañar la salud** del gato.
- Los **eventos de arena** u otros eventos destructivos bajan la integridad por debajo del 20 %.
- Si la integridad baja del 20 % por un evento, primero se tira la **probabilidad (50 %)**:
  - **Muerte.**
  - **Degradación:** por el daño de radiación, el gato **no puede volver a salir** de la base.
- **Gato herido:**
  1. Camina **5 segundos** hacia la base y cae.
  2. Si llega a entrar a una **esclusa**, vive y va al **depósito**.
  3. Si cae afuera, el jugador puede **auxiliarlo** con un botón. Cuesta un **gato libre**; si no hay gatos libres, no se puede.
  4. Si no es rescatado en **1 minuto**, muere.
- **Ala médica:** propuesta fuera del PMV.
- Se fabrican en el **taller** con **fibra y un poco de metal**. No se construyen en el PMV.
- La **radiación permanente** degrada los trajes con el tiempo de exposición.

## Pendientes

1. Si el **abono** es recurso del pool general, y de dónde sale la **luz**.
2. Si la **hidroponía** entra en el PMV como alternativa al cultivo en regolito.
3. Valores reales de la preparación (fuente NASA o artículo); qué hacer si no hay referencia publicada.
4. Qué es el **depósito** al que va el gato herido: ¿dónde se queda, consume recursos, puede trabajar dentro?
5. Si la secuencia del gato herido aplica solo al caso de **degradación** o también antes de la tirada de probabilidad.
6. Insumos y minijuego de la **mina de tierras raras** (post-PMV).
7. Confirmar la cadena **tierras raras → chips → robots (chips + metal)** en el taller.
8. Cuánto dura el funcionamiento de cada edificio antes de volver a prepararlo.
9. Si con el rango prohibitivo los insumos ya puestos se descuentan o no.
10. Preparación de energía y de extracción de agua.
11. Costo en metal y escombros de cada reparación.
12. Cuánto produce y consume cada edificio, y cuánto consume cada gato.
13. En qué zona está cada edificio.
14. Si el daño a la salud por el traje empieza **al llegar** al 20 % o **por debajo**, y a qué ritmo.
15. Si los trajes se pueden reparar.
16. Si el MOXIE y el almacén compartido entran en el PMV.
17. Contrato de cada sistema para Lógica (clase y namespace `MarsCat.Logic.<Sistema>`), que se acuerda en el issue.
