# 05 — Plan de implementación

**Estado:** borrador
**Qué es:** el orden en que se implementa la lógica y la integración en Unity, y qué depende de qué. **No es un reporte de avance:** el avance vive en el tablero (columna **Estado**) y en los commits `#n`.

## Cómo usarlo (agentes y personas)

1. Lee este archivo después de `STRUCTURE.md` y de tu perfil en `agents/`. Los contratos acordados de las olas 1 a 3, el reparto y la tabla de dependencias están en `06_guia_logica.md`.
2. Toma el issue de la **ola más baja** que no esté bloqueado. Una ola empieza cuando los issues de los que depende están en `Hecho`, o cuando su **contrato** ya está acordado y el código vecino se puede simular con una interfaz.
3. Un issue se cierra cuando cumple su checklist. Los checks los marca Gestión; el cierre del issue lo hace Leyni a mano.
4. Si un issue depende de una decisión abierta (columna "Decisión abierta"), **se pregunta, no se inventa** (`AGENTS.md`, regla 7). Esas decisiones se cierran en #14.

## Reglas de este plan

1. **Contrato primero.** Antes de escribir un sistema, su issue tiene el contrato (clase, namespace, métodos y eventos) con el check "Contrato acordado con Integración Unity" marcado (`agents/agent_codigo.md`).
2. **Sin números en el código.** Costos, tasas, capacidades y rangos se leen de `MarsCat/Assets/Data/` (issue #22). Mientras un valor no esté acordado, la prueba usa un valor de prueba propio, nunca un dato de NASA inventado.
3. **Lógica pura.** `MarsCat.Logic.*` no usa `UnityEngine`. El tiempo entra como parámetro (`Tick(float deltaTime)`). Cada sistema trae pruebas NUnit en `Scripts/Logic/Tests/`.
4. **Los `.cs` se suben sin `.meta`.** Los `.meta` los genera y sube Integración (ver "Traspaso de Lógica a Unity").
5. **Un script, un dueño.** Dos personas nunca editan el mismo `.cs`.

## Estado de partida

- **H0** casi cerrado: faltan el check de #6 y el de #9 (Integración Unity 2 clona y abre el proyecto) y #4.
- **H1:** la lógica de las olas 1 a 3 (#15 a #22) ya está escrita. Falta su cierre (ola 3.5 en `06_guia_logica.md`: `FromBalance` pendientes y CSV completo) y los `.meta` de `Buildings/` y `Energy/`. Los contratos de la ola 4 en adelante están propuestos en `06_guia_logica.md`.
- **Hay código de prototipo fuera de la estructura** de `STRUCTURE.md`: `Assets/Scripts/Enemies`, `Environment`, `Player` y `Systems`, `Assets/Editor`, `Assets/Sprites`, `Assets/CabinaNavegable` y `Scenes/CabinaPrincipal.unity`. **No se toca ni se mueve** hasta que Gestión e Integración decidan (paso 0.3), con una excepción acordada: la **cabina** (`CabinaNavegable`) se adopta como zona Segura de `Main` y sus scripts de puertas y animación se adaptan a `MarsCat.Unity.*` (no se mueven ni se editan los originales). El ratón enemigo no se integra: no está en el diseño del PMV.

## Ola 0 — Preparación

| Paso | Quién | Qué | Listo cuando |
|---|---|---|---|
| 0.1 | Leyni | Aprobar los contratos sugeridos de #15–#21, #23, #54–#56 y #58 | Check "Contrato acordado" marcado en cada issue |
| 0.2 | Integración (con aprobación de Leyni) | Crear tres `asmdef` desde el Editor: `MarsCat.Logic` (con **No Engine References**), `MarsCat.Logic.Tests` (referencia a Logic y a NUnit; solo Editor) y `MarsCat.Unity` (referencia a Logic). Sin esto la lógica no queda aislada de Unity ni el Test Runner ve las pruebas. Este paso es el "pedido explícito" que exige `agent_codigo.md` para un `asmdef` nuevo | Test Runner → EditMode abre sin errores |
| 0.3 | Gestión | Registrar en `STRUCTURE.md` las carpetas de prototipo de arriba, o decidir su traslado | `STRUCTURE.md` y carpetas coinciden |
| 0.4 | Lógica | Acordar el formato del archivo de balance en `Data/` (parte de #22) | Formato aprobado |
| 0.5 | Gestión | Confirmar que #65, #66 y #67 (edificio de desechos) quedan absorbidos por #58, #61 y #64 (reciclador orgánico) | Duplicados cerrados |

## Olas de Lógica (H1)

| Ola | # | Issue | Depende de | Decisión abierta (#14 / `03_sistemas.md`) | Arranque |
|---|---|---|---|---|---|
| 1 | 15 | Reloj de día y noche | 0.1, 0.2 | Ninguna (duración del sol ya acordada) | Sí |
| 1 | 16 | Recursos y depósito | 0.1, 0.2 | Tope de los tanques de agua | Sí |
| 1 | 22 | Cargador de balance en `Data/` | 0.4 | Solo se cargan valores acordados | Sí |
| 2 | 17 | Gatos: hambre, sed y estados | #15, #16 | Confirmar el estado "Buscando comida" | Sí |
| 2 | 20 (parte 1) | `PreparationResult` común | #22 | Ninguna | Sí |
| 2 | 21 | Energía: solar, consumo y noche | #15, #16 | ¿"Operación Rescate Solar" es la preparación del PMV? | Solo el modelo de red |
| 3 | 19 | Ciclo del edificio (reparar, preparar, funcionar) | #16, #20 (tipo), #21 | Costo de reparación; duración del funcionamiento; zona de cada edificio | Sí, con valores inyectados |
| 3 | 18 | Asignación automática de trabajo | #17, #19 | Orden de prioridad para tareas puntuales | Con prioridad inyectable |
| 4 | 20 (parte 2) | `CropPreparation` (granja de plantas) | #19, #22 | Valores guía de agua, luz y bacterias; de dónde sale la luz | Sí, con el abono ya acordado |
| 4 | 55 | Aire de la base (% de O₂) | #17 | Aprobar meta, franjas, volumen y fuga | Tras la aprobación en #14 |
| 5 | 54 | Preparación del extractor de agua | #20, #21, #16 | Veta, profundidad y electricidad por metro (valores) | Sí (la preparación ya está aprobada en #14) |
| 5 | 56 | MOXIE y filtros | #21, #55, #18 | Cada cuánto se ensucia un filtro; cuánto dura limpiarlo | Tras #55 y los valores |
| 6 | 57 | Granja de ratones | #19, #20 | **Parcial:** acordado que un gato fijo trae ratas y la granja las convierte en raciones (`03_sistemas.md`); faltan insumos, minijuego, cadencia y desechos (propuesta en `Propuesta_mecanica/mecanica_comida.md`) | Esqueleto del puesto y las raciones |
| 6 | 58 | Reciclador orgánico (desechos y abono) | #57, #19, #20 | Aprobar controles; desechos por gato y por ratón; abono por lote | Parcial: el modelo ya está acordado, falta el minijuego |
| 7 | 23 | Secuencia del tutorial | Todas las anteriores | Pasos después de "Trajes"; ¿el paso Trajes va al final del tutorial? | Esqueleto de los pasos 1 a 5 |

**Cierre de cada issue de Lógica:** contrato acordado, pruebas NUnit en verde (las corre Integración en el Test Runner), valores leídos de `Data/` y los checks propios del issue.

## Traspaso de Lógica a Unity (`.meta`)

Cuando un agente deja un `.cs` en `Scripts/Logic/` o `Scripts/Unity/`:

1. **Código** sube solo los `.cs` con `[logica] <descripción> #n` (o `[unity]` para MonoBehaviours). Nunca sube `.meta`. Si abrió Unity y se le generaron `.meta` locales, no los sube y los borra antes del siguiente `pull`.
2. **Integración** hace `git pull`, abre Unity y deja que genere los `.meta`.
3. **Integración** revisa que la Console no tenga errores ni advertencias nuevas y corre **Test Runner → EditMode**.
4. **Integración** sube los `.meta` con `[integracion] meta de <sistema> #n`, en un commit aparte del código. Un `.meta` existente nunca se borra ni se regenera.
5. **Integración** conecta lo que haga falta en el Editor (referencias, componentes, `ScriptableObject` de `Config/`, prefabs) siguiendo el comentario de la tarjeta, y recién entonces pasa a los issues de Unity de la siguiente tabla.
6. Si el agente no pudo compilar, lo dice con todas las letras: *"No verificado en Unity"*. Ese resultado lo confirma Integración en el paso 3.

## Integración Unity por olas (H1)

| Orden | Issues | Depende de | Nota |
|---|---|---|---|
| A | #24 (escena Main), #25 (cámara 2.5D), #34 (luz de día y noche) | Ola 0.2; #34 también de #15 | Se puede avanzar con cubos, sin esperar la lógica |
| B | #26 (GameBootstrap), #27 (HUD de recursos y reloj), #33 (gatos en escena) | #15, #16, #17 | El HUD sigue el wireframe acordado en `docs/diseno/09_interfaz.md` |
| C | #28 (interacción con edificios) | #19, #18, #26 | — |
| D | #29 (minijuego de plantas), #30 (preparación de agua), #59 (filtros del MOXIE) | #20, #54, #56 y #28 | Cada minijuego espera su lógica |
| E | #31 (Rescate Solar), #60 (ratones), #61 (reciclador), #35 (Trajes), #32 (diálogos de la IA) | #21, #57, #58, #23 | Dependen de decisiones aún abiertas |
| F | #36 (build de prueba) | Todas las anteriores | Cierra H1 junto con una prueba completa del tutorial |

**Cierre de H1:** se juega el tutorial de principio a fin (`02_core_loop.md`), aunque sea con cubos. Se cierra a mano después de probar la build y deja tag `h1-jugable` y una línea en `BITACORA.md`.

## Decisiones abiertas y los issues que desbloquean

| Decisión (checks de #14) | Desbloquea |
|---|---|
| Aprobar reglas 1–6 del PMV | #21, #23, #35 |
| ¿"Operación Rescate Solar" es la preparación de energía? | #21, #31 |
| Costo de reparación, producción y consumo por sol, zonas | #19, #22 y todos los edificios |
| Granja de ratones: insumos, minijuego, comida y desechos | #57, #60, #58 |
| Reciclador orgánico: controles, desechos y abono por lote | #58, #61 |
| Aire: meta, franjas, volumen y fuga | #55 |
| MOXIE: ensuciamiento y limpieza | #56, #59 |
| Orden de prioridad de edificios para tareas puntuales | #18 |
| Pasos después de "Trajes" y su lugar en el tutorial | #23, #35 |
| ¿De dónde sale la luz? | #20 (parte 2) |
