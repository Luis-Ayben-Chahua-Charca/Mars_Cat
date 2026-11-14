# 03 — Sistemas

**Estado:** borrador
**Decidido en:** #__
**Pendientes:** ver la sección "Pendientes" al final.

Los porcentajes y cantidades de esta página son valores de diseño o de ejemplo. Cuando Lógica cree el archivo de balance en `MarsCat/Assets/Data/`, pasan a vivir ahí y esta página solo explica su porqué (ver `README.md`).

## Recursos

| Recurso | Tipo | En el PMV |
|---|---|---|
| Oxígeno | Soporte vital | Sí |
| Electricidad | Soporte vital | Sí |
| Comida | Soporte vital | Sí |
| Agua | Soporte vital | Sí |
| Metal | Material de construcción | Sí |
| Escombros | Material de construcción | Sí |
| Fibra | Material para fabricar trajes | No |
| Tierras raras | Automatiza la preparación de edificios (chips) | Por confirmar |

Insumos que aparecen en la preparación del cultivo y aún no están en la tabla (por confirmar si son recursos del pool general): **tierra**, **bacterias**, **abono**. La **luz** se expresa en %; por confirmar si sale de la electricidad.

## Edificios del tutorial

| Edificio | Produce | Acción en el tutorial | Zona |
|---|---|---|---|
| Energía (paneles solares) | Electricidad | Reparar y preparar | Por definir |
| Producción de alimentos (plantas y ratones) | Comida | Reparar y preparar | Por definir |
| Extracción de agua del subsuelo | Agua | Reparar y preparar | Por definir |

## Preparación de edificios

Inspirada en los minijuegos de *Among Us*.

1. **Reparación:** automática.
2. **Preparación:** el jugador ajusta los insumos que el edificio necesita para funcionar. Lo que pone se **descuenta del pool general** de recursos.
3. Cada insumo tiene un **valor guía** que da la IA y un **rango de aceptación**. Fuera del rango, el edificio no funciona.
4. Dentro del rango, ciertos valores y **combinaciones** mejoran el **rendimiento**.
5. **Automatización con chips** (tierras raras): el jugador aprende a hacer la preparación a mano, y luego puede automatizarla para que no se vuelva tediosa.

### Ejemplo: zona de cultivo

| Insumo | Valor guía | Rango de aceptación |
|---|---|---|
| Tierra | 20 % | 15 %–25 % |
| Agua | 2 L | Por definir |
| Luz | Por definir (%) | Por definir |
| Bacterias | Por definir (cantidad) | Por definir |
| Abono | Por definir (kg) | Por definir |

Los demás edificios tendrán su propia preparación, por definir.

### Propuesta de lógica de rendimiento (no acordada)

- **Puntaje por insumo:** `s = 1 − |valor − guía| / tolerancia`. Vale 1 en el valor guía y 0 en el borde del rango. Fuera del rango, el edificio no arranca.
- **Rendimiento base:** `rendimiento = 0,8 + 0,4 × promedio(s)`. Va de 80 % (todo en el borde) a 120 % (todo en el valor guía).
- **Combinaciones:** una tabla de sinergias por edificio. Si dos insumos de la sinergia tienen `s ≥ 0,8`, suman un bono fijo (por ejemplo +10 %). Para el PMV se puede dejar sin combinaciones.
- **Automatización:** el chip pone todos los insumos en su valor guía, así que el resultado es fijo y rápido. Hacerlo a mano permite buscar combinaciones y superar ese rendimiento.
- **Datos reales:** los valores guía y rangos deberían salir de datos de NASA sobre cultivo en el espacio y registrarse en `05_datos_nasa.md`. No se inventan.

Para Lógica, esto se traduce en una clase de preparación por edificio que recibe los insumos y la configuración (guía, tolerancia y sinergias desde `Data/`) y devuelve si arranca y con qué rendimiento. El contrato se acuerda en el issue.

## Trajes espaciales

- Protegen a los gatos contra la **radiación** cuando salen de la base.
- Tienen una **barra de integridad**.
- **Umbral del 20 %:** representa el soporte vital de **presión atmosférica**. Cuando la integridad llega al 20 %, el traje comienza a **dañar la salud** del gato.
- Los **eventos de arena** u otros eventos destructivos bajan la integridad por debajo del 20 %.
- Si la integridad baja del 20 % por un evento, el gato sufre una de dos consecuencias, con **50 % de probabilidad** cada una:
  - **Muerte.**
  - **Degradación:** por el daño de radiación, el gato **no puede volver a salir** de la base.
- Se fabrican con **fibra** (no incluido en el PMV).

## Pendientes

1. Si las **tierras raras** son el recurso y los **chips** se fabrican con ellas, o si son lo mismo. Cuánto cuesta automatizar.
2. Si **tierra, bacterias y abono** son recursos del pool general, y de dónde sale la **luz**.
3. Si la **preparación** entra en el PMV y si las tierras raras también.
4. Si la automatización es permanente por edificio o se paga cada vez.
5. Qué pasa si los insumos quedan fuera del rango: ¿se pierden o se devuelven?
6. Preparación de energía y de extracción de agua.
7. Costo en metal y escombros de cada reparación.
8. Cuánto produce y consume cada edificio, y cuánto consume cada gato.
9. En qué zona está cada edificio.
10. Si el daño a la salud por el traje empieza **al llegar** al 20 % o **por debajo**, y a qué ritmo.
11. Si un gato degradado puede seguir trabajando dentro de la base, y si los trajes se pueden reparar.
12. Contrato de cada sistema para Lógica (clase y namespace `MarsCat.Logic.<Sistema>`), que se acuerda en el issue.
