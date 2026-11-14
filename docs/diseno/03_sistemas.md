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
| Agua | Soporte vital | Sí |
| Metal | Material de construcción; insumo de robots y trajes. Se extrae de Marte y se recupera de escombros | Sí |
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
| Depósito (almacén compartido) | Guarda alimentos, fibra y escombros; lugar de recuperación (ver `04_economia_y_balance.md`) | — | Sí | Segura |
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

## Pendientes

1. Si el **abono** es recurso del pool general, y de dónde sale la **luz**.
2. Si la **hidroponía** entra en el PMV como alternativa al cultivo en regolito.
3. Valores reales de la preparación (NASA, artículo o funcionamiento en la Tierra).
4. Insumos y minijuego de la **mina de tierras raras** (post-PMV).
5. Confirmar la cadena **tierras raras → chips → robots (chips + metal)** en el taller.
6. Cuánto dura el funcionamiento de cada edificio antes de volver a prepararlo.
7. Si con el rango prohibitivo los insumos ya puestos se descuentan o no.
8. Preparación de energía y de extracción de agua.
9. Costo en metal y escombros de cada reparación.
10. Cuánto produce y consume cada edificio, y cuánto consume cada gato.
11. En qué zona está cada edificio.
12. Ritmo al que baja la salud con el traje al 20 %, y duración del estado Recuperando.
13. Cuánto tarda la reparación de un traje en el taller, y si en el PMV (sin taller) los trajes se reparan de otra forma.
14. "Buscando comida" se agregó a los estados por la regla de hambre: confirmar.
15. Si el MOXIE entra en el PMV.
16. Contrato de cada sistema para Lógica (clase y namespace `MarsCat.Logic.<Sistema>`), que se acuerda en el issue.
