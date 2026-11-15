# 06 — Guía de Lógica: olas 1 a 3

**Estado:** contratos acordados de las olas 1 a 3 (#15 a #22). Contratos de la ola 4 en adelante: propuesta pendiente de aprobación (ver su sección).
**Para quién:** Lógica 1, Lógica 2 y sus agentes. Integración Unity la usa para saber qué conectar.
**Qué es:** qué clases se crean, en qué orden, para qué sirve cada una y de qué depende. Complementa `05_plan_implementacion.md`. El avance no se anota aquí: vive en el tablero y en los commits `#n`.

## Antes de escribir código

1. Los tres `asmdef` deben existir (paso 0.2 del plan): `MarsCat.Logic`, `MarsCat.Logic.Tests` y `MarsCat.Unity`. Si el Test Runner no muestra la pestaña EditMode con el ensamblado de pruebas, avisa a Integración antes de seguir.
2. Lee `AGENTS.md`, `STRUCTURE.md`, `agents/agent_codigo.md` y tu issue.
3. Trabaja solo en tus archivos (tabla "Reparto"). Si necesitas algo de un archivo ajeno, se pide en el issue del dueño.

## Reglas comunes (acordadas)

| # | Regla |
|---|---|
| T1 | El reloj vive en `MarsCat.Logic.Clock` (no `Time`, para no chocar con `UnityEngine.Time`). |
| T2 | `Tick` recibe **segundos reales**. Hambre y sed usan el tiempo real. La producción y el consumo de edificios y energía usan el **tiempo de juego** (`DayNightClock.GameDeltaTime`): de noche corre 3 veces más rápido. |
| T3 | El oxígeno **no** está en `ResourcePool`: es el % de O₂ del aire (`BaseAtmosphere`, #55, ola 4). Agua y electricidad sí están en el pool, con topes propios (tanques y baterías). |
| T4 | El balance está en `MarsCat/Assets/Data/balance_pmv.csv`, con columnas `clave;valor;unidad;fuente`. La primera línea es el encabezado; las líneas que empiezan con `#` son comentarios. Los números se leen con `CultureInfo.InvariantCulture` y **punto** decimal. `BalanceLoader` recibe el texto como `string`; el adaptador de Unity se lo pasa. |
| T5 | Cada sistema recibe en el constructor una clase de configuración (`DayNightConfig`, `ResourceConfig`, etc.) con un método estático `FromBalance(BalanceData)`. Si falta una clave, se lanza una excepción con el nombre de la clave. Las pruebas arman la configuración a mano. |
| T6 | Los cambios se avisan con `event Action<...>`. Un argumento inválido (negativo, nulo, id desconocido) lanza una excepción con un mensaje claro. Los textos para el jugador son **claves** (por ejemplo `prep.water.out_of_range`), nunca frases en el código. |
| T7 | Las tasas del CSV van **por sol**. `FromBalance` las convierte a **por segundo de juego**, dividiendo por la duración del sol en tiempo de juego: `clock.day_seconds + clock.night_seconds × clock.night_time_scale` (360 + 120 × 3 = 720). Los tiempos de hambre y sed van en **segundos reales**. |

Sin números en el código: todo valor de balance sale de la configuración. En las pruebas se usan valores de prueba propios, nunca datos de NASA inventados.

## Carpetas y archivos

Una carpeta por sistema dentro de `MarsCat/Assets/Scripts/Logic/`; el namespace sigue a la carpeta. Un archivo por clase. Las pruebas van en `Scripts/Logic/Tests/`, con el nombre `<Clase>Tests.cs`.

```
Scripts/Logic/
├── Clock/        DayNightConfig, DayNightClock
├── Resources/    ResourceId, ResourceConfig, ResourcePool
├── Data/         BalanceData, BalanceLoader
├── Cats/         CatState, Need, CatConfig, Cat, CatColony
├── Preparation/  PreparationResult
├── Energy/       EnergyPriority, EnergyConfig, EnergyGrid
├── Buildings/    BuildingState, BuildingConfig, Building
├── Work/         JobKind, Job, WorkConfig, JobAssigner
├── Air/          (ola 4) MoxieFilterConfig, MoxieFilters
├── Food/         (ola 4) RatFarmConfig, RatFarm, LooseRats, RatRaid, RatHunt
├── Tutorial/     (ola 4) TutorialStep, TutorialSequence
├── Water/        (ola 4) MeltPhase, MeltingConfig, MeltingChamberSession
├── Recycling/    (ola 4) CompostConfig, CompostSession
└── Tests/        <Clase>Tests.cs
```

## Reparto (un script, un dueño)

| Persona | Orden de trabajo | Archivos |
|---|---|---|
| **Lógica 1** | #15 → #16 → #17 → #18 | `Clock/`, `Resources/`, `Cats/`, `Work/` y sus pruebas |
| **Lógica 2** | #22 → #20 (parte 1) → #21 → #19 | `Data/`, `Preparation/`, `Energy/`, `Buildings/` y sus pruebas |

Lógica 2 usa `ResourceId` y `ResourcePool` (de Lógica 1) desde #21. Por eso Lógica 1 sube #16 **lo antes posible**, justo después de #15. Mientras tanto, Lógica 2 avanza con #22 y #20, que no dependen de nadie.

## Tabla de dependencias

| Ola | Issue | Clase principal | Dueño | Depende de | Lo usan | Puede empezar |
|---|---|---|---|---|---|---|
| 1 | #15 | `DayNightClock` | Lógica 1 | — | #17, #18, #21, #19 (vía `GameDeltaTime`) | Ya |
| 1 | #16 | `ResourcePool` | Lógica 1 | — | #17, #21, #19 | Ya |
| 1 | #22 | `BalanceLoader` | Lógica 2 | — | `FromBalance` de todos | Ya |
| 2 | #20 (1) | `PreparationResult` | Lógica 2 | — | #19 y todos los minijuegos | Ya |
| 2 | #17 | `CatColony` | Lógica 1 | #16 (y el `bool isNight` de #15) | #18 | Cuando #16 esté en `main` |
| 2 | #21 | `EnergyGrid` | Lógica 2 | #16 (y el tiempo de juego de #15) | #19 | Cuando #16 esté en `main` |
| 3 | #19 | `Building` | Lógica 2 | #16, #20 (1), #21 | #18 (por `BuildingId`), Unity #28 | Cuando #21 esté en `main` |
| 3 | #18 | `JobAssigner` | Lógica 1 | #17 | Unity #26 y #28 | Cuando #17 esté en `main` |

`JobAssigner` no usa la clase `Building`: un trabajo solo guarda el `BuildingId` (texto). Avisarle al edificio que tiene un trabajador (`SetWorkerPresent`) lo hace el adaptador de Unity (`GameBootstrap`, #26) escuchando `OnAssigned` y `OnReleased`. Así #18 no espera a #19.

## Contratos

Todo en `MarsCat.Logic.<Sistema>`. Los miembros listados son la parte **pública**; lo interno queda a criterio del dueño.

### #15 — Reloj de día y noche (`Clock`) · Lógica 1

**Para qué:** lleva el sol actual, si es de día o de noche, y cuánto tiempo de juego pasó en cada tick. Todos los demás sistemas se mueven con él.

- `DayNightConfig`: `DaySeconds` (360, reales), `NightSeconds` (120, reales), `NightTimeScale` (3), `StartProgress` (0 = amanecer). `FromBalance` lee `clock.day_seconds`, `clock.night_seconds`, `clock.night_time_scale` y `clock.start_progress`.
- `DayNightClock(DayNightConfig)`:
  - `void Tick(float dt)`: segundos reales.
  - `int CurrentSol` (empieza en 1), `bool IsNight`.
  - `float DayProgress`: 0–1 del sol, medido en tiempo de juego. Con los valores acordados, el día ocupa la primera mitad.
  - `float GameDeltaTime`: tiempo de juego del último tick (`dt` de día, `dt × 3` de noche).
  - Eventos `OnDayStarted(int sol)` y `OnNightStarted(int sol)`.
- **Probar:** a los 360 s empieza la noche; la noche dura 120 s reales; el sol avanza al amanecer; un `dt` grande que cruza el cambio dispara el evento correcto; `GameDeltaTime` se triplica de noche; `dt` negativo lanza excepción.

### #16 — Recursos (`Resources`) · Lógica 1

**Para qué:** el inventario de la base. Cada recurso tiene cantidad y tope propio. En el PMV no hay tope compartido del depósito (acordado).

- `enum ResourceId`: `Electricity`, `Food`, `Water`, `Metal`, `Debris`, `Compost`, `CatWaste`, `RatWaste`, `CropResidue`, `Forage`, `Fiber`, `Bacteria`, `Regolith`. `Bacteria` y `Regolith` son infinitos. `Forage` (alimento de las ratas) y `Fiber` se agregaron en la ola 3.5 (cadena alimentaria acordada).
- `ResourceConfig`: cantidad inicial y tope por recurso. `FromBalance` lee `resource.<id>.initial` y `resource.<id>.capacity` (por ejemplo `resource.water.capacity`). El tope de `Electricity` es la capacidad de las baterías; el de `Water`, la de los tanques.
- `ResourcePool(ResourceConfig)`:
  - `float Get(ResourceId)`; `float Capacity(ResourceId)`; `bool IsInfinite(ResourceId)`.
  - `float Add(ResourceId, float amount)`: devuelve lo que **realmente** entró. Si el tope corta, el resto lo maneja quien produce (la cola de exceso).
  - `bool TryConsume(ResourceId, float amount)`: todo o nada. Si no alcanza, no descuenta nada y devuelve `false`.
  - Un recurso infinito siempre acepta y siempre alcanza.
  - Evento `OnChanged(ResourceId, float newAmount)`.
- **Probar:** el tope corta y `Add` devuelve lo agregado; `TryConsume` no descuenta parcial; los infinitos nunca se agotan; montos negativos lanzan excepción.

### #22 — Cargador de balance (`Data`) · Lógica 2

**Para qué:** leer `balance_pmv.csv` y entregar los valores a las configuraciones.

- `static BalanceData BalanceLoader.Parse(string csv)`.
- `BalanceData`:
  - `float Get(string key)`: si falta la clave, `KeyNotFoundException` con el nombre de la clave.
  - `bool TryGet(string key, out float value)`.
  - `string Source(string key)`: la fuente de ese valor.
- Reglas: formato de T4; clave duplicada o número inválido → excepción con el **número de línea**.
- **Probar:** lee con punto decimal aunque la cultura del sistema sea `es-PE`; ignora comentarios y líneas vacías; detecta duplicados; `Source` devuelve la fuente.
- **Datos:** solo entran valores con fuente en la columna `fuente`, con una de estas etiquetas: `nasa`, `tierra`, `derivado`, `estimacion`, `diseno` o `placeholder` (qué significa cada una y cuándo se usa: `docs/diseno/05_datos_nasa.md`, "Cuando no hay dato"). Para el código la fuente es solo texto: agregar etiquetas no cambia `BalanceLoader`. Las cifras de `docs/Propuesta_mecanica/balance_recursos.md` no se copian enteras: varias chocan con lo acordado (muerte a los 2 soles, oxígeno en tanques, sin agricultura en el PMV, 20 gatos).

### #20 (parte 1) — Resultado de preparación (`Preparation`) · Lógica 2

**Para qué:** el resultado común de todos los minijuegos de preparación, para que el edificio no necesite saber qué minijuego se jugó.

- `readonly struct PreparationResult`: `bool Starts`, `float Yield` (0 en adelante; 1 = rendimiento normal), `string ReasonKey` (clave del aviso cuando no arranca).
- Fábricas: `PreparationResult.Ok(float yield)` y `PreparationResult.Fail(string reasonKey)`.
- **Probar:** `Ok` con rendimiento negativo lanza excepción; `Fail` sin clave lanza excepción.

### #17 — Gatos (`Cats`) · Lógica 1

**Para qué:** cada gato con su hambre, sed, salud y estado, y la colonia que los alimenta desde el pool.

- `enum CatState` (8 estados aprobados): `Free`, `WorkingIndoor`, `WorkingOutdoor`, `Fallen`, `Recovering`, `Resting`, `SeekingFood`, `Dead`. El PMV usa `Free`, `WorkingIndoor`, `WorkingOutdoor`, `Resting`, `SeekingFood` y `Dead`.
- `enum Need`: `Food`, `Water`. `SeekingFood` cubre las dos necesidades; el evento indica cuál.
- `CatConfig` (segundos **reales**): intervalo, alerta y muerte por necesidad, más la ración por comida y por bebida. Valores acordados: comida 240 / 480 / 600 s; agua 120 / 240 / 300 s. Claves: `cat.food.interval_seconds`, `cat.food.alert_seconds`, `cat.food.death_seconds`, `cat.food.ration` y las mismas con `water`.
- `Cat`:
  - `int Id`, `CatState State`, `float Health` (0–1), `bool IsDegraded`.
  - `float Hunger` y `float Thirst`: 0–1, donde 1 es la muerte. Con los valores acordados, come o bebe en 0,4 y entra en alerta en 0,8.
  - `bool TryAssign(bool outdoor)`: solo si está `Free`; un gato degradado nunca acepta un puesto exterior.
  - `void Release()`.
- `CatColony(CatConfig, ResourcePool, int seed)`:
  - `IReadOnlyList<Cat> Cats`, `IEnumerable<Cat> FreeCats`, `void Spawn(int count)`. La cantidad inicial viene de `colony.initial_cats` (acordado: **15**). Cada gato arranca con un **hambre y una sed iniciales aleatorias** entre 0 y `cat.initial_need_spread` (por ejemplo 0,3), para que no coman todos a la vez. `CatColony` recibe la semilla aleatoria (`int seed`) en el constructor para que las pruebas sean repetibles.
  - `void Tick(float dt, bool isNight)`.
  - Eventos: `OnStateChanged(Cat, CatState from, CatState to)`, `OnExtremeAlert(Cat, Need)`, `OnDied(Cat)`.
- Reglas:
  - Hambre y sed suben con el tiempo real, también de noche.
  - Al llegar al intervalo, el gato come o bebe si hay en el pool; si falta, comen primero los más cercanos a morir.
  - En alerta extrema pasa a `SeekingFood`: deja el trabajo y no acepta otro.
  - En 1, muere.
  - De noche, los gatos libres pasan a `Resting`; al amanecer, a `Free`.
- **Probar:** con la misma semilla, el hambre inicial se repite y queda dentro del rango; come a los 240 s si hay comida; alerta a los 480 s; muere a los 600 s; con poca comida come el más hambriento; de noche descansa; el degradado no acepta puestos exteriores.

### #21 — Energía (`Energy`) · Lógica 2

**Para qué:** la red eléctrica. Suma lo que producen los paneles, descuenta lo que consumen los edificios y la calefacción, guarda el excedente en las baterías (`Electricity` del pool) y decide qué se apaga primero cuando no alcanza.

- `enum EnergyPriority`: `None`, `High`, `Max` (acordado):

| Prioridad | Consumidores | En un apagón |
|---|---|---|
| `Max` | MOXIE (oxígeno) y calefacción nocturna | Se apaga último |
| `High` | Extracción de agua, taller y depósito | Se apaga después de los `None` |
| `None` | Reciclador orgánico (desechos), granja de ratones y granja de plantas (cosechas) | Se apaga primero |

  Dentro de una misma prioridad, por defecto se apaga primero el último registrado.
- `EnergyConfig`: consumo de calefacción por sol de noche y lo que haga falta para los paneles. `FromBalance` convierte por sol a por segundo de juego (T7).
- `EnergyGrid(EnergyConfig, ResourcePool)`:
  - `void Tick(float gameDt, bool isNight)`: recibe **tiempo de juego**.
  - `void RegisterProducer(string id, float ratePerSol, bool solar)`; `void SetProducerYield(string id, float yield)` (el resultado de la limpieza de polvo). Un productor solar no produce de noche.
  - `void RegisterConsumer(string id, float ratePerSol, EnergyPriority priority)`; `void SetActive(string id, bool active)`.
  - `bool IsPowered(string id)`; `bool HasPower`; `float SolarOutput` (producción solar actual).
  - Eventos `OnBlackout` y `OnPowerRestored`.
- **Probar:** de día los paneles cargan las baterías; de noche no producen y corre la calefacción; si no alcanza, se apagan primero los `None`; al volver la energía se encienden en orden inverso; un id repetido o desconocido lanza excepción.

### #19 — Edificios (`Buildings`) · Lógica 2

**Para qué:** el ciclo de un edificio de producción (reparar → preparar → funcionar → volver a preparar) y su producción hacia el pool.

- `enum BuildingState`: `Damaged`, `Repairing`, `Unprepared`, `Running`, `Depleted`.
- `BuildingConfig`: id, costo de reparación (metal y escombros), tiempo de reparación, recurso que produce y tasa por sol, duración del funcionamiento, consumo eléctrico y su `EnergyPriority`.
- `Building(BuildingConfig, ResourcePool, EnergyGrid)`:
  - `string Id`, `BuildingState State`, `float Yield`.
  - `bool StartRepair()`: cobra el costo con `TryConsume`; si no alcanza, devuelve `false` y no cambia de estado.
  - `void ApplyPreparation(PreparationResult)`: si arranca, pasa a `Running` con su rendimiento; si no, se queda en `Unprepared`.
  - `void SetWorkerPresent(bool)`.
  - `void Tick(float gameDt)`: produce `tasa × Yield` solo si está en `Running`, con trabajador y con energía (`IsPowered`). Al cumplir la duración pasa a `Depleted` y hay que volver a preparar.
  - Evento `OnStateChanged(Building, BuildingState from, BuildingState to)`.
- El MOXIE no sigue este ciclo (mantenimiento continuo de filtros): se resuelve en #56, no aquí.
- **Probar:** sin materiales no repara; una preparación fallida no arranca; sin trabajador o sin energía no produce; al agotarse pide preparar de nuevo; si el pool está lleno, la producción no se pierde (queda en cola).

### #18 — Asignación de trabajo (`Work`) · Lógica 1

**Para qué:** reparte automáticamente los gatos libres en los puestos libres, como en *Frostpunk*.

- `enum JobKind`: `Post` (puesto fijo en un edificio) y `OneOff` (tarea puntual, por ejemplo limpiar filtros).
- `enum TaskPriority`: `Normal`, `Vital` (acordado). `Vital` es solo auxiliar a un gato caído (post-PMV: en el PMV no hay caídas).
- `Job`: `string Id`, `JobKind Kind`, `string BuildingId`, `bool Outdoor`, `TaskPriority Priority` (solo cuenta en tareas puntuales).
- `WorkConfig`: importancia de cada edificio para sacarle un gato. Se reutiliza el orden de la energía: primero los `None`, después los `High`, nunca los `Max` salvo en tareas `Vital`.
- `JobAssigner(WorkConfig)`:
  - `void RegisterJob(Job)`, `void UnregisterJob(string id)`, `void Release(string buildingId)` (liberación manual).
  - `void Tick(float dt, CatColony colony, bool isNight)`.
  - Eventos `OnAssigned(Job, Cat)` y `OnReleased(Job, Cat)`.
- Reglas (de `04_economia_y_balance.md`, "Asignación de trabajo"):
  - No se asigna de noche.
  - Las tareas puntuales tienen prioridad sobre los puestos fijos. Primero se usan gatos libres.
  - **Sin gatos libres (acordado):**
    - Tarea `Normal`: saca un gato solo de un edificio `None`. Si no hay, espera.
    - Tarea `Vital`: saca un gato de un `None`; si no hay, de un `High`; y como último recurso, de un `Max`.
  - `Release(buildingId)` permite que el jugador elija de qué edificio sale el gato.
  - Al terminar una tarea puntual, el gato vuelve a estar libre.
  - Un gato degradado nunca va a un puesto exterior; un gato que pasa a `SeekingFood` o muere libera su puesto.
- **Probar:** asigna gatos libres de día y no de noche; respeta el orden; libera al gato en alerta extrema; no manda degradados afuera.

## Ola 3.5 — Cierre de las olas 1 a 3 (ya se puede tomar)

Sin esto el GameBootstrap (#26) no puede cargar el balance.

### `FromBalance` pendientes · Lógica 1 (#15, #16, #17, #18)

`DayNightConfig`, `ResourceConfig`, `CatConfig` y `WorkConfig` todavía no tienen `FromBalance` (T5). Cada dueño lo agrega en su archivo, con su prueba.

| Clase | Claves |
|---|---|
| `DayNightConfig` | `clock.day_seconds`, `clock.night_seconds`, `clock.night_time_scale`, `clock.start_progress` |
| `ResourceConfig` | `resource.<id>.initial` y `resource.<id>.capacity` para cada recurso finito. Ids en minúsculas con guion bajo: `electricity`, `food`, `water`, `metal`, `debris`, `compost`, `cat_waste`, `rat_waste`, `crop_residue`, `forage`, `fiber` |
| `CatConfig` | `cat.food.interval_seconds`, `cat.food.alert_seconds`, `cat.food.death_seconds`, `cat.food.ration`, lo mismo con `water`, y `cat.initial_need_spread` |
| `WorkConfig` | La importancia de cada edificio es su prioridad de energía (acordado): `FromBalance(BalanceData, IEnumerable<string> buildingIds)` lee `building.<id>.energy_priority` |

`colony.initial_cats` no pertenece a ninguna configuración: lo lee el GameBootstrap para llamar a `Spawn`.

**También en la ola 3.5 (cadena alimentaria acordada en `03_sistemas.md`):**
- **Lógica 1 (#16):** agregar `Forage` y `Fiber` a `ResourceId`, con sus pruebas.
- **Lógica 2 (#19):** un edificio puede dar **subproductos** además de su producto principal. La granja de plantas produce `Forage` y, como subproductos, `Fiber` y `CropResidue`. `BuildingConfig` suma `IReadOnlyDictionary<ResourceId, float> ByproductRatesPerSol`; `FromBalance` los lee con `TryGet` de `building.<id>.byproduct.<recurso>_per_sol` (si la clave no está, no hay subproducto). El edificio los entrega al pool junto con el producto, con el mismo rendimiento y la misma cola de exceso.

### CSV completo · Lógica 2 (#22)

`Data/balance_pmv.csv` debe tener **todas** las claves que piden las configuraciones de arriba, más `colony.initial_cats`, `energy.heating.rate_per_sol` y, para cada edificio del PMV, las de `BuildingConfig`: `building.<id>.energy_priority`, `repair_metal_cost`, `repair_debris_cost`, `repair_seconds`, `production_rate_per_sol`, `run_seconds` y `energy_rate_per_sol`.

- Ids de edificio: `solar_panels`, `moxie`, `water_extractor`, `plant_farm`, `rat_farm`, `organic_recycler`, `depot`.
- Prioridad de energía como número: `None` = 0, `High` = 1, `Max` = 2.
- Valor acordado → su fuente (`diseno` o la referencia). Valor sin acordar → `placeholder`. Nunca una cifra de NASA inventada.
- Prueba recomendada: cargar el CSV del repositorio y construir todas las configuraciones sin excepción.

**Primera tanda de valores (acordada; detalle y fuentes en `04_economia_y_balance.md`):**

| Clave | Valor | Unidad | Fuente (columna del CSV) |
|---|---|---|---|
| `colony.initial_cats` | 15 | gatos | diseno |
| `cat.initial_need_spread` | 0.3 | fraccion | diseno |
| `cat.food.ration` | 1 | raciones | diseno: 1 racion = 125 kcal; 250 kcal/sol por gato |
| `cat.water.ration` | 0.05 | L | Tierra: 50 mL/kg/dia para un gato de 4 kg |
| `resource.food.initial` | 60 | raciones | diseno: 2 soles de margen |
| `resource.water.initial` | 6 | L | diseno: 2 soles de margen |
| `building.solar_panels.production_rate_per_sol` | 6 | kWh | derivado: irradiancia 43 % y eficiencia del panel |

Unidades del pool: `food` en raciones, `water` en litros, `electricity` en kWh, `forage` y `fiber` en kg. El resto de las claves va como `placeholder`. La fuente no puede llevar `;`.

## Olas 4 en adelante — contratos propuestos

**Estado:** **acordados** el #57 (granja, redada y caza), el #54 (solo el paso 2, con la cadena de abajo) y el #20 parte 2 (Fábrica de Suelo y su fórmula de rendimiento). El resto es propuesta de Gestión pendiente de aprobación de Leyni: no se programa hasta que el issue tenga marcado "Contrato acordado".

### Reglas de los minijuegos

- La lógica de cada minijuego es una **sesión** en C# puro: recibe la entrada del jugador como métodos (`Fire`, `SetHeat`…), avanza con `Tick(float dt)` en **segundos reales** (es interacción del jugador) y al terminar entrega un `PreparationResult`.
- Unity solo dibuja la sesión y le pasa la entrada. Nada de física ni de animación en la lógica.
- Lo aleatorio recibe una semilla (`int seed`) para que las pruebas sean repetibles.
- Todo valor de balance va en el CSV con el prefijo `minigame.<nombre>.`.

### Reparto propuesto

| Persona | Issues | Carpetas nuevas |
|---|---|---|
| **Lógica 1** | #56 (filtros del MOXIE), #57 (granja de ratones, esqueleto), #23 (secuencia del tutorial, esqueleto). Después, #55 (aire), cuando se apruebe su modelo | `Air/`, `Food/`, `Tutorial/` |
| **Lógica 2** | Rescate Solar (lógica; issue por crear o parte de #31), #54 (solo el paso 2, acordado), #58 (Compostador + Sopa de Bacterias), #20 parte 2 (Fábrica de Suelo) | `Water/`, `Recycling/`, `Preparation/` (ya suya) y `Energy/` (ya suya) |

### Rescate Solar (lógica) · Lógica 2 · `MarsCat.Logic.Energy`

**Para qué:** la limpieza de paneles con disparos de CO₂ (`03_sistemas.md`, "Operación Rescate Solar").

- `SolarCleaningConfig`: carriles (3), segundos por fila (1), filas por sesión, probabilidad de celda sucia, electricidad por disparo. Claves `minigame.solar.*`.
- `SolarCleaningSession(SolarCleaningConfig, ResourcePool, int seed)`:
  - `void Tick(float dt)`; `void Fire(int lane)` (0 a 2): siempre cobra la electricidad, acierte o no.
  - `bool[] CurrentRow` (qué celdas de la fila actual están sucias), `int RowsRemaining`, `bool IsFinished`.
  - `float Efficiency` = celdas sucias limpiadas ÷ total de celdas sucias.
  - Al terminar: `PreparationResult.Ok(Efficiency)`.
  - Eventos `OnRowPassed(int cleaned, int missed)` y `OnFinished(PreparationResult)`.
- **Probar:** disparar a una celda limpia gasta energía y no suma; una fila sin limpiar pasa al segundo; con la misma semilla se repite la banda; sin electricidad, el disparo no sale.

### Cámara de derretido (#54, paso 2) · Lógica 2 · `MarsCat.Logic.Water`

**Para qué:** las dos fases con aguja y círculo de progreso del extractor (`03_sistemas.md`, "Paso 2"). **Acordado: el PMV lleva solo este paso**, dentro de una cadena que deja enchufar los pasos 1 y 3 después sin cambiar nada.

- `enum MeltPhase`: `Pressurize`, `Heat`, `Done`, `Lost`.
- `MeltingConfig`: progreso inicial (0,4), segundos para llenar (5), velocidad de bajada fuera de zona, ancho de la zona verde, oscilación de la aguja, factor del botón acelerar (2) y electricidad extra al acelerar. Claves `minigame.melt.*`.
- `MeltingChamberSession(MeltingConfig, ResourcePool, int seed)`:
  - `void Tick(float dt, float input)`: `input` de −1 a 1 empuja la aguja; la lógica le suma la oscilación.
  - `void SetAccelerated(bool)`.
  - `MeltPhase Phase`, `float Needle` (0–1), `float Progress` (0–1).
  - Si `Progress` llega a 0: en `Pressurize` la fase se reinicia; en `Heat` el agua hierve y el lote se pierde (`Fail("prep.water.boiled")`).
  - Al terminar `Heat`: `PreparationResult.Ok(1)`.
  - Eventos `OnPhaseCompleted(MeltPhase)`, `OnPhaseFailed(MeltPhase)`, `OnFinished(PreparationResult)`.
- **Probar:** en el centro se llena en 5 s; acelerado, en 2,5 s y cobra electricidad; fallar la presión reinicia sin perder el lote; fallar el calor pierde el lote.

**Cadena del extractor, sin deuda técnica** (`MarsCat.Logic.Water`):

- `readonly struct IceBatch`: `float Liters`, `float Purity` (0–1).
- `interface IIceSource`: `IceBatch Take()`. PMV: `FixedVein(IceBatch)`, leído de `minigame.water.vein.liters` y `minigame.water.vein.purity`. Post-PMV: el sondeo (paso 1) implementa la misma interfaz y además puede devolver `Fail`.
- `interface IWaterTreatment`: `IceBatch Treat(IceBatch)` y `bool IsSafe(IceBatch)`. PMV: `NoTreatment` (devuelve el mismo lote y lo da por seguro). Post-PMV: el biorreactor (paso 3).
- `WaterExtraction(IIceSource, MeltingChamberSession, IWaterTreatment, ResourcePool)`: cuando la cámara termina con `Ok`, toma el lote, lo trata y, si es seguro, suma `Liters × Purity × Yield` de agua al pool. Si la cámara falla, el lote se pierde y no se suma nada.
- **Probar:** con la veta fija y la cámara bien jugada se suman los litros del CSV; si el agua hierve no se suma nada; cambiar `NoTreatment` por un tratamiento de prueba no cambia la cámara.

### Fábrica de Suelo (#20, parte 2) · Lógica 2 · `MarsCat.Logic.Preparation`

**Para qué:** la preparación de la granja de plantas (`03_sistemas.md`, "Fábrica de Suelo"). En el PMV tiene dos pasos (lavar y mezclar); la luz se agrega después **sin cambiar nada de lo existente**.

- `interface IPreparationStep`: `string Id`; `StepScore Evaluate()`. Cada paso guarda sus propios valores y sabe si está dentro del rango.
- `readonly struct StepScore`: `bool InRange`, `float Score` (0–1; 1 = en el valor guía), `string ReasonKey` (qué corregir si está fuera de rango).
- `GuideRange` (valor guía, mínimo y máximo de un insumo, leídos del CSV) con `StepScore Evaluate(float value)`: puntaje `1 − |valor − guía| / tolerancia` hacia el lado que corresponda (el rango puede ser asimétrico, como el abono de 0 a 30 %).
- Pasos del PMV:
  - `WashStep(WashConfig, ResourcePool)`: `void SetWater(float liters)`; el perclorato baja según el agua y parte de 0,4–0,6 %; cobra el agua al confirmar. Claves `minigame.soil.wash.*`.
  - `MixStep(MixConfig)`: `void SetCompost(float percent)`, `void SetBacteria(float dose)`; usa `GuideRange` (abono: guía 20 %, rango 0–30 %). Cobra el abono al confirmar. Claves `minigame.soil.mix.*`.
  - Post-PMV: `LightStep` (horas de luz, guía 12 h) se agrega a la lista.
- `MultiStepPreparation(IReadOnlyList<IPreparationStep> steps, YieldConfig)`:
  - `PreparationResult Confirm()`: si algún paso está fuera de rango, `Fail` con la clave de ese paso (no cobra nada); si no, `Ok(0,8 + 0,4 × promedio de los puntajes)`.
  - Sirve también para el Compostador y para cualquier preparación futura de valor guía y rango.
- **Probar:** un paso fuera de rango no arranca y no cobra; en el valor guía rinde 1,2; abono en 0 % está dentro del rango y rinde menos; agregar un tercer paso no cambia los resultados de los otros dos.
- `YieldConfig`: `Min` (0,8), `Max` (1,2), `Plateau` (0,1), `RobotYield` (1,0) y el peso de cada paso. Claves `yield.*` y `minigame.<juego>.<paso>.weight`. Detalle y ejemplos en `03_sistemas.md`, "Rendimiento de las preparaciones de valor guía".
  - `GuideRange`: tolerancia por lado; con tolerancia 0 de un lado, quedarse corto es fuera de rango; dentro de la meseta, puntaje 1.
  - `Confirm()`: promedio ponderado; si `S < 1`, la clave de motivo es la del paso con menor puntaje (`prep.<paso>.improve`).
  - **Probar también:** abono 20 % y agua 1,5 × guía rinden 1,10; dentro de la meseta el puntaje es 1.
- La fórmula está **acordada**; como vive en `YieldConfig`, ajustar los números no toca el código.

### Compostador (#58, paso 1) · Lógica 2 · `MarsCat.Logic.Recycling`

**Para qué:** la mezcla por relación C:N antes de la Sopa (`03_sistemas.md`, "Compostador").

- `CompostMixConfig`: C:N de cada insumo, meta (guía 30, rango 25–30 o el acordado en el CSV). Claves `minigame.compost.mix.*`.
- `CompostMix(CompostMixConfig, ResourcePool)`:
  - `void SetAmount(ResourceId id, float amount)` para `CatWaste`, `RatWaste` y `CropResidue`.
  - `float Ratio`: carbono total ÷ nitrógeno total, con `cn` y `n_fraction` de cada insumo (`minigame.compost.mix.<insumo>.cn` y `.n_fraction`). Con `n_fraction` igual en todos, es el promedio ponderado por cantidad.
  - Valores de partida (`03_sistemas.md`): gato ~10, cama de rata 100–300, restos de papa 20; guía 30, rango 20–40 (pendiente de aprobación).
  - `StepScore Evaluate()` (con `GuideRange`); `bool TryConfirm()`: si está en rango, cobra los desechos y devuelve `true`.
- La Sopa recibe la mezcla ya cobrada: `CompostSession.TryStart()` deja de cobrar los desechos y pasa a recibir el puntaje de la mezcla, que multiplica el rendimiento final.
- **Probar:** solo desechos de gato da una relación baja y fuera de rango; sumar cama de rata la sube; en rango cobra; fuera de rango no cobra.

### Sopa de Bacterias (#58, paso 2) · Lógica 2 · `MarsCat.Logic.Recycling`

**Para qué:** el minijuego del reciclador orgánico (`03_sistemas.md`, "Operación Sopa de Bacterias"). Zonas verdes con valores reales.

- `CompostConfig`: zona verde de temperatura (55–65 °C), sobrecalentamiento (70 °C), zona verde de humedad (50–60 %), límites de 65 % y 40 %, enfriamiento por segundo, inercia de la temperatura, electricidad por segundo al 100 % de calor, agua por segundo del aspersor, duración del ciclo y desechos por lote. Claves `minigame.compost.*`.
- `CompostSession(CompostConfig, ResourcePool)`:
  - `void Start(float mixScore)`: arranca con la mezcla ya cobrada por el Compostador.
  - `void Tick(float dt)`; `void SetHeat(float level)` (0–1, gasta electricidad); `void SetSprinkler(bool on)` (gasta agua).
  - `float Temperature`, `float Humidity`, `float TimeInGreen`, `bool IsFinished`.
  - Rendimiento = (tiempo con las dos agujas en verde ÷ duración del ciclo) × puntaje de la mezcla. Sobre 70 °C: `Fail("prep.compost.overheated")`. La zona verde de temperatura es también la sanitización (Toxoplasma).
  - Eventos `OnFinished(PreparationResult)`.
- **Probar:** sin calor la temperatura baja; la inercia retrasa la subida; el aspersor gasta agua; sobre 70 °C se pierde el lote; todo el ciclo en verde rinde 1.

### Filtros del MOXIE (#56) · Lógica 1 · `MarsCat.Logic.Air`

**Para qué:** el mantenimiento continuo del MOXIE (`03_sistemas.md`, "MOXIE y sus filtros").

- `MoxieFilterConfig`: cantidad de filtros (5), tiempo de juego en que se ensucia un filtro, segundos de limpieza por filtro. Claves `minigame.moxie.*`.
- `MoxieFilters(MoxieFilterConfig)`:
  - `void Tick(float gameDt)`: los filtros se ensucian de a uno con el tiempo.
  - `void RequestCleaning(int count)` (1, 2 o 5); `void SetWorkerPresent(bool)`: la limpieza solo avanza con un gato (tarea puntual de #18).
  - `int CleanFilters`; `float Yield` = filtros limpios ÷ total.
  - Eventos `OnFilterDirty`, `OnCleaningFinished(int cleaned)`.
- El MOXIE gasta electricidad igual con filtros sucios (#21); su oxígeno entra al aire cuando exista #55.
- **Probar:** se ensucian con el tiempo; sin gato no se limpia; limpiar 2 de 5 sube el rendimiento a lo esperado.

### Granja de ratones, La Redada y caza (#57) · Lógica 1 · `MarsCat.Logic.Food`

**Para qué:** `03_sistemas.md`, "Granja de ratones y raciones" y "La Redada y la caza" (PMV). Reemplaza el esqueleto anterior: ya no hay un gato que trae ratas por intervalo, y el Criadero queda descartado.

- `RatFarmConfig`: raciones por rata, segundos de conversión por rata (tiempo de juego), **forraje por rata por sol**, desechos por rata convertida, tope de ratas (`MaxRats`), `MinBreeders` (pie de cría) y `BreedRate` (por sol; 0 = no se reproducen). Claves `building.rat_farm.*` (valores propuestos en `03_sistemas.md`, "Ritmo de la granja de ratas").
- `RatFarm(RatFarmConfig, ResourcePool)`:
  - `void Tick(float gameDt)`; `void SetWorkerPresent(bool)`: sin gato en el puesto no convierte.
  - `int Rats`; `bool CanStart` = `Rats > MinBreeders`.
  - Las ratas comen `Forage` del pool por sol. Con forraje y gato, convierte una rata cada cierto tiempo en raciones (`Food`) y suma `RatWaste`, sin bajar del pie de cría. Con forraje, crecen de forma logística: `BreedRate × Rats × (1 − Rats / MaxRats)` por sol, acumulando la fracción hasta completar una rata. Sin forraje, no convierte ni se reproduce.
  - `int AddRats(int count)` (devuelve las que entraron, respeta el tope) y `int RemoveRats(int count)` (devuelve las que quitó): para la redada ahora y las fugas después.
  - Eventos `OnRatsChanged(int)` y `OnRationsProduced(float)`.
- `LooseRats(int count)`: ratas sueltas con id `0..count−1` (Unity las ubica en sus puntos). `int Count`; `bool IsLoose(int id)`; `bool TryTake(int id)`; `int AnyLoose()` (−1 si no queda ninguna); evento `OnTaken(int id)`. Clave `rat.loose.initial`.
- `RatRaid(LooseRats, RatFarm)`, la preparación de la granja:
  - `bool Mark(int ratId)`: el jugador marca una rata; Integración pide un gato libre como tarea puntual normal (#18).
  - `bool Capture(int ratId)`: cuando el gato llega; `TryTake` + `AddRats(1)`.
  - `PreparationResult Result`: `Ok(1f)` si `CanStart`; si no, `Fail("prep.rat_farm.need_rats")`.
- `RatHunt(CatColony, LooseRats, ResourcePool, RatHuntConfig)`:
  - `void Tick(float gameDt)`, llamado **después** de `CatColony.Tick`: cada gato en `SeekingFood` sin raciones en el pool acumula tiempo de caza; al llegar a `HuntSeconds` toma `AnyLoose()` y satisface su hambre (`Cat.Satisfy(Need.Food)` es `internal` y se ve desde el mismo ensamblado: no hay que tocar `Cats/`).
  - Eventos `OnHunted(Cat, int ratId)` y `OnNoLooseRats` (una vez). Clave `rat.hunt_seconds`.
- **Probar:** marcar y capturar suma a la granja y quita de las sueltas; no arranca hasta superar el pie de cría; convierte sin bajar del pie; sin forraje no convierte; con `BreedRate = 0` no crecen; con 10 ratas, tope 40 y 0,75 llegan a 15 en ~0,8 sol; en el tope no nacen más; un gato con hambre extrema y sin raciones caza una rata y deja de tener hambre; con raciones no caza; sin ratas sueltas no caza y avisa una vez.

### Secuencia del tutorial (#23, esqueleto) · Lógica 1 · `MarsCat.Logic.Tutorial`

**Para qué:** el pipeline acordado de 8 pasos (`02_core_loop.md`). No decide cuándo se cumple un paso: el GameBootstrap escucha los eventos de los sistemas y le avisa. Así no depende de ninguna otra clase.

- `enum TutorialStep`: `Cinematic`, `MoveCamera`, `Energy`, `Oxygen`, `Water`, `PlantFarm`, `RatFarm`, `Recycler`, `Done`.
- `TutorialSequence()`:
  - `TutorialStep CurrentStep`; `string MissionKey` (por ejemplo `tutorial.energy.mission`).
  - `bool Complete(TutorialStep step)`: avanza solo si es el paso actual; si no, devuelve `false` y no cambia nada.
  - Eventos `OnStepStarted(TutorialStep)` y `OnStepCompleted(TutorialStep)`.
- Sin derrota (regla 7 del PMV). El paso Trajes queda fuera hasta que se decida en #14.
- **Probar:** avanza en orden; completar un paso que no es el actual no hace nada; al final queda en `Done`.

## Al terminar cada issue

1. Sube **solo los `.cs`** con `[logica] <descripción> #n`, sin `.meta`.
2. Si no pudiste compilar ni correr las pruebas, dilo: *"No verificado en Unity"*.
3. Integración abre Unity, genera y sube los `.meta`, y corre el Test Runner (ver "Traspaso de Lógica a Unity" en `05_plan_implementacion.md`).
4. Si cambiaste algo público del contrato, avisa en el issue **antes** de subir.
