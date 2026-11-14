# 03 — Sistemas

**Estado:** borrador
**Decidido en:** #__
**Pendientes:** ver la sección "Pendientes" al final.

Los porcentajes de esta página son valores de diseño. Cuando Lógica cree el archivo de balance en `MarsCat/Assets/Data/`, pasan a vivir ahí y esta página solo explica su porqué (ver `README.md`).

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

## Edificios del tutorial

| Edificio | Produce | Acción en el tutorial | Zona |
|---|---|---|---|
| Energía (paneles solares) | Electricidad | Reparar | Por definir |
| Producción de alimentos (plantas y ratones) | Comida | Reparar | Por definir |
| Extracción de agua del subsuelo | Agua | Reparar | Por definir |

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

1. Costo en metal y escombros de cada reparación.
2. Cuánto produce y consume cada edificio, y cuánto consume cada gato.
3. En qué zona está cada edificio.
4. Si el daño a la salud empieza **al llegar** al 20 % o **por debajo** del 20 %, y a qué ritmo.
5. Si un gato degradado puede seguir trabajando dentro de la base.
6. Si los trajes se pueden reparar y con qué recurso.
7. Contrato de cada sistema para Lógica (clase y namespace `MarsCat.Logic.<Sistema>`), que se acuerda en el issue.
