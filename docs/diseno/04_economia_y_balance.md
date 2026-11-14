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

## Consumo y hambre

- La comida y el agua se consumen **en tiempo real**, con **estadísticas por gato** (ver `03_sistemas.md`).
- Cada gato tiene su **hambre individual**. Valores de ejemplo:
  - Come cada **3 minutos**.
  - A los **6 minutos** sin comer salta una **alerta de hambre extrema**.
  - A los **7 minutos** sin comer, **muere de hambre**.
- **Orden para comer:** un gato come cuando tiene hambre. Si hay gatos con hambre extrema, comen primero los que tienen más hambre (los más cercanos a morir).
- **Hambre extrema (6–7 minutos):** el gato no trabaja y busca comida. Si estaba trabajando, apenas haya comida deja lo que hace y va a comer.
- El agua además se usa para regar las plantas.
- Reemplaza a la regla del documento antiguo (reporte diario y muerte tras 2 días sin comer), que queda descartada.

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
4. Si la **sed** sigue la misma regla que el hambre, y con qué tiempos.
5. Tiempos reales de hambre (los 3, 6 y 7 minutos son ejemplos).
6. Cuánta salud cuesta el sobretrabajo (post-PMV).
