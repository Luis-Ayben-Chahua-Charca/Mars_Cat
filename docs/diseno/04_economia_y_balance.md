# 04 — Economía y balance

**Estado:** borrador
**Decidido en:** #__
**Pendientes:** ver la sección "Pendientes" al final.

Los recursos y edificios están en `03_sistemas.md`. Esta página describe cómo fluyen. Las cifras son valores de diseño y pasan a `MarsCat/Assets/Data/` cuando Lógica cree el archivo de balance.

## Ciclo diario

- La economía se mide **por días**.
- Al terminar el día se emite un **reporte**: si todos los gatos comieron y bebieron.
- Un gato que no comió recibe un **símbolo de hambre**. Si no come **2 días seguidos**, muere.
- El agua sigue el mismo reporte y además se usa para regar las plantas.

## Almacenamiento

- **Oxígeno y energía:** almacenamientos independientes, con tope. El tope crece con el árbol tecnológico.
- **Alimentos, fibra y escombros:** físicos y visibles, en un **almacén compartido** (un solo edificio). El jugador debe vigilar qué está acumulando y qué necesita.
- **Exceso:** si el almacén está lleno, la producción se detiene y queda **en cola**; el jugador debe permitir a mano que entre al almacén.

## Fuentes

1. **Reservas iniciales** de materiales.
2. **Escombros** de los edificios destruidos.
3. **Nodos de mineral marciano** cerca, en la cueva o en la superficie.
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
  - Energía: según los edificios activos.
  - La **protección** de los edificios está siempre activa y siempre consume energía.
- **Activos:** la **reparación del beacon** para enviar la alerta a la Tierra.

## Entropía y fallos en cascada

- **Radiación permanente:** degrada edificios y trajes.
- **Edificios:** tienen un **escudo**. Si se pierde, el interior se vuelve peligroso para los gatos (cambios bruscos de temperatura); si no se repara a tiempo, el edificio cae total o parcialmente. Los fenómenos del desierto abren pequeñas **grietas** que dañan la estructura.
- **Sin energía**, un edificio no puede trabajar y arrastra al resto, en especial la protección contra radiación y el oxígeno.
- **Sin comida**, hay menos gatos trabajadores.
- Los únicos recursos externos son los nodos de mineral y la energía del sol.

## Válvula de emergencia

Un gato que pasó el umbral de exposición puede **internarse** (sigue consumiendo, pero no trabaja) o dejarse morir para que no consuma recursos.

## Escalado

- Más edificios consumen más energía y exigen más trabajo y más salidas al exterior, con más exposición y más trajes.
- El **árbol tecnológico** mejora la producción de alimentos, la eficiencia energética y la producción de trajes.
- Recursos intangibles: solo **salud**. No hay moral ni esperanza.

## Pendientes

1. ¿Los "minerales marcianos" son el mismo recurso que el **metal** de `03_sistemas.md`?
2. Si la válvula de emergencia (internar o dejar morir) es una decisión del jugador **después** de la degradación al 50 %, o la reemplaza.
3. Topes de almacenamiento y tamaño del lote (el documento antiguo menciona 15 unidades por lote de plantas como ejemplo).
4. Consumo por gato y por edificio.
5. Si el almacén compartido entra en el PMV.
