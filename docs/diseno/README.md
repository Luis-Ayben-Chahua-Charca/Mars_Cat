# docs/diseno/ — Diseño del juego

Documentos de diseño del juego. **Nombre del juego: por definir** (`Mars Cat` es solo el identificador del producto). Los mantiene Gestión; el contenido sale de decisiones del equipo registradas en los issues. Los demás perfiles solo los leen.

## Reglas

- **Un dato, un lugar.** Los números de balance viven solo en `MarsCat/Assets/Data/`. Los documentos explican el porqué de cada valor y apuntan al archivo y la clave, sin copiar el número.
- **Datos NASA trazables.** Todo valor real se registra en `05_datos_nasa.md` antes de usarse.
- **Valores realistas.** Si no hay dato de NASA ni artículo, el valor se toma de cómo funciona en la Tierra, tirando a lo realista y evitando lo fantasioso (por ejemplo, "2 gotas de agua alcanzan para 30 plantas" no sirve).
- **Una página por documento**, con tres campos arriba: Estado, Decidido en (#issue) y Pendientes.
- Estados posibles: `por crear`, `borrador`, `acordado`.

## Índice

| Archivo | Contenido | Estado |
|---|---|---|
| `01_vision.md` | Logline, tema, tono, reglas del universo, agencia del jugador, niveles | borrador |
| `02_core_loop.md` | Etapas, zonas, tutorial, PMV, momentos narrativos de la partida | borrador |
| `03_sistemas.md` | Recursos, edificios, trajes y sus reglas | borrador |
| `04_economia_y_balance.md` | Ciclo diario, almacenamiento, fuentes, sumideros, cascadas, escalado | borrador |
| `05_datos_nasa.md` | APIs, base científica de comida y oxígeno, referencias | borrador |
| `06_eventos_y_ritmo.md` | Catálogo de eventos, curva de presión, clímax | borrador |
| `07_lore.md` | Premisa, inicio, IA guía, antagonista, beacon y rescate | borrador |
| `08_direccion_arte.md` | Paleta, escala, referencias y lista de assets | por crear |
