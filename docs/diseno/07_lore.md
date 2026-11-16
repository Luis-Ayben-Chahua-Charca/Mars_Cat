# 07 — Lore

**Estado:** borrador
**Decidido en:** #__
**Pendientes:** ver la sección "Pendientes" al final.

## Nombre del juego

**Mars Miau House** (acordado). `Mars Cat` / `MarsCat` sigue siendo el identificador técnico (repo, proyecto Unity, namespaces).

## Premisa

- En Marte existe una **colonia humana ya funcional**. No es necesario explicar cómo se llegó a ella.
- En la colonia, los humanos trabajaban con **gatos genéticamente modificados** para hacerlos más inteligentes. Se eligieron gatos por ser atractivos para los niños.
- Un **cataclismo** obliga a los humanos a abandonar la base: una **lluvia de meteoritos** que merma la integridad de la base. Es el **primer** cataclismo.
- Antes de irse, dejan a los gatos **congelados en un módulo seguro: la cabina principal, dentro del cráter Vivero** (ubicación real; ver `05_datos_nasa.md`), con la intención de volver cuando el cataclismo termine.

## Inicio del juego

Las etapas jugables y el tutorial están en `02_core_loop.md`.

- Una **cinemática** muestra la evacuación y la destrucción de la base.
- La **electricidad de soporte** del módulo se deshabilita y los gatos quedan **descongelados**.
- La **IA** que supervisaba a los gatos pasa a ser una **guía**:
  - ayuda a los gatos dentro de la historia;
  - es la **voz que acompaña al jugador**.

## Protagonistas y antagonista

- Los **gatos genéticamente modificados** son los protagonistas: buscan sobrevivir.
- No hay un personaje antagonista: el **ambiente de Marte** es el antagonista.

## Narrativa ambiental

- Escombros de la base.
- El cráter Vivero y la cabina principal.
- Datos de NASA integrados en el escenario.

## Arco y final

- El **beacon** estaba enterrado; una tormenta lo desentierra. La misión es repararlo para comunicarse con la Tierra.
- Emitir la señal desencadena la crisis máxima (ver `06_eventos_y_ritmo.md`).
- Al recibir la señal, los humanos responden: *"En 15 días las tormentas darán un descanso. Podemos rescatarles en esa brecha de tiempo; sobrevivan hasta ese momento."*
- El final es el **rescate por los humanos**, con una cinemática final.
- Momentos narrativos completos en `02_core_loop.md`; tono y referencias en `01_vision.md`.

## Pendientes

1. Balance y recursos: primera versión en `04_economia_y_balance.md`; el equipo sigue trabajando en ello.
2. ~~Nombre final del juego~~: Mars Miau House (acordado).
3. Cuántos gatos hay y si tienen nombres o personalidades distintas.
4. Personalidad y tono de la IA guía (afecta voz, textos y audio).
5. Si habrá más cataclismos después de la lluvia de meteoritos y cuáles.

## Versión anterior descartada

El primer borrador (protagonistas infantes o jóvenes, colonia gestionada por IA que falla, tipos de ciudadano como prisioneros y científicos, máquina "purificador") quedó **desactualizado** y se reemplaza por esta premisa. Las notas de diseño que traía siguen vigentes como ideas y se pasarán a su documento cuando exista:

- Eventos aleatorios como presión y para marcar el ritmo → `06_eventos_y_ritmo.md`.
- Tres niveles de progresión, el primero de recursos → `02_core_loop.md`.

Del documento "Lore, mecánicas y balance" también se descarta el pasado que proponía (una tormenta espacial, solar o de arena, que activó a los gatos antes de tiempo): prevalece la lluvia de meteoritos y el corte de la electricidad de soporte.
