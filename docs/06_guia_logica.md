# 06 — Guía de Lógica: olas 1 a 3

**Estado:** contratos acordados (#15 a #22).
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

- `enum ResourceId`: `Electricity`, `Food`, `Water`, `Metal`, `Debris`, `Compost`, `CatWaste`, `RatWaste`, `CropResidue`, `Bacteria`, `Regolith`. `Bacteria` y `Regolith` son infinitos.
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
- **Datos:** solo entran valores con fuente en la columna `fuente`. Un valor de diseño se marca `diseno`; uno que falta, `placeholder`. Las cifras de `docs/Propuesta_mecanica/balance_recursos.md` no se copian enteras: varias chocan con lo acordado (muerte a los 2 soles, oxígeno en tanques, sin agricultura en el PMV, 20 gatos).

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

## Al terminar cada issue

1. Sube **solo los `.cs`** con `[logica] <descripción> #n`, sin `.meta`.
2. Si no pudiste compilar ni correr las pruebas, dilo: *"No verificado en Unity"*.
3. Integración abre Unity, genera y sube los `.meta`, y corre el Test Runner (ver "Traspaso de Lógica a Unity" en `05_plan_implementacion.md`).
4. Si cambiaste algo público del contrato, avisa en el issue **antes** de subir.
