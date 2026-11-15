# 09 — Interfaz (HUD)

**Estado:** acordado
**Decidido en:** #27 (distribución acordada con Leyni)
**Pendientes:** ver la sección "Pendientes" al final.

Wireframe básico del HUD de la partida. Boceto original: `img/wireframe_hud.png`.

![Wireframe del HUD](img/wireframe_hud.png)

## Distribución

```
┌───────────────────────────────────────────────────────────────┐
│ §1 IA │ §2 🍖 # │ §3 💧 # │  §4 Reloj  │ §10 metal · escombros · abono │ §5 Misiones │
│       └─────────┴─────────┘ (día/noche)│                                │  - ...      │
│                                                                         │  - ...      │
│                     Vista de la base (gatos, edificios)                 └─────────────│
│                                                                               │
│      ╭──────╮                                                                 │
│   §7 │ §6   │ §8 Minimapa                                         ┌───────────┐ │
│      ╰──────╯                                                   │ §9 Detalle│ │
│                                                                 └───────────┘ │
└───────────────────────────────────────────────────────────────┘
```

| # | Elemento | Ubicación | Qué muestra | Lógica que lo alimenta |
|---|---|---|---|---|
| §1 | **Esquina de la IA** | Arriba a la izquierda | Retrato de la IA guía; desde aquí habla (comunicación con el jugador) | Secuencia del tutorial y avisos (#23, #32) |
| §2 | **Cantidad de alimentos** | Barra superior, junto a §1 | Ícono + número | Recursos y depósito (#16) |
| §3 | **Cantidad de agua** | Barra superior, junto a §2 | Ícono + número | Recursos (#16) |
| §4 | **Reloj** | Arriba al centro, semicírculo | Ciclo de día y noche (sol actual) | Reloj de día y noche (#15) |
| §10 | **Materiales** | A la derecha del reloj | Metal, escombros y abono (ícono + número cada uno) | Recursos y depósito (#16) |
| §5 | **Tablero de misiones** | Arriba a la derecha | Lista de misiones activas de la IA | Secuencia del tutorial (#23) |
| §6 | **Minimapa** | Abajo a la izquierda, circular | Vista reducida de la base y sus zonas | Escena y cámara (#24, #25) |
| §7 | **Barra de oxígeno** | Arco de **1/4 de círculo** a la **izquierda** del minimapa | Indicador continuo del % de O₂, con las franjas de color | Aire de la base (#55) |
| §8 | **Barra de energía** | Arco de **1/4 de círculo** a la **derecha** del minimapa | Indicador continuo de la electricidad disponible | Energía (#21) |
| §9 | **Panel de detalle** | Abajo a la derecha | Aparece al **seleccionar un gato o un edificio**. Gato: hambre, sed, salud, traje, estado. Edificio: estado, producción y acciones (reparar; una vez reparado, la opción de **abrir su minijuego**) | Gatos (#17), edificios (#19) |

- **Gatos en la vista:** se ven en la escena de la base (las manchas oscuras del boceto).
- **Desechos:** no van en la barra superior. Aparecen como **estado sobre el edificio del depósito** (por ejemplo, un ícono cuando se está llenando), en el panel de detalle al seleccionar el depósito y **visualmente dentro del propio almacén**.

## Relación con el diseño

- **Oxígeno (§7):** muestra el % de O₂ con las franjas verde, amarilla, roja y crítica (`03_sistemas.md`, "Oxígeno").
- **Misiones (§5):** cada paso del pipeline del tutorial (`02_core_loop.md`) aparece como una misión de la IA.
- **IA (§1):** es la voz del jugador (`07_lore.md`); sus diálogos se abren desde esta esquina.

- **Minijuegos:** se abren desde el panel de detalle §9, como una opción del edificio **una vez que ya fue reparado**.

## Pendientes

Ninguno por ahora.
