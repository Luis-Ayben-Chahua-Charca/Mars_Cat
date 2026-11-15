# Dataset: Radiación en la superficie de Marte (MSL/RAD)

## ¿Qué es?

Son mediciones de radiación tomadas por el instrumento **RAD** (*Radiation Assessment Detector*) del rover **Curiosity** de la NASA, que está en el **cráter Gale** de Marte.

- **Fuente:** NASA CDAWeb – [cdaweb.gsfc.nasa.gov](https://cdaweb.gsfc.nasa.gov/)
- **Dataset:** `MSL_RAD_OBS-L2` (nivel 2: datos ya calibrados)
- **Periodo descargado:** 12/09/2026 al 11/11/2026 (unos 60 días)
- **Cantidad de datos:** 5089 mediciones, una cada ~17 minutos
- **Responsable del instrumento:** Dr. D. Hassler, Southwest Research Institute (SwRI)

## Archivos

| Archivo | Qué contiene |
|---|---|
| `MSL_RAD_OBS-L2_70348.csv` | **Los datos:** una fila por medición, con fecha y radiación. Se abre en Excel. |
| `MSL_RAD_OBS-L2_70348.json` | **La descripción del dataset:** misión, instrumento, responsable, periodo, cada cuánto se mide (~16 min) y qué significa cada variable. No trae mediciones. |

## Columnas del CSV

| Columna | Unidad | Significado |
|---|---|---|
| `EPOCH` | fecha y hora UTC | Momento en que terminó la medición. La "Z" indica hora UTC (Lima = UTC − 5). |
| `ALIVE_TIME` | segundos | Tiempo que el detector estuvo midiendo (normalmente ~928 s ≈ 15,5 min). |
| `TOTAL_DOSE_B` | µGy/día | Radiación en el detector de silicio, **incluyendo** la radiación del generador nuclear del rover. |
| `RTG_CORRECTED_DOSE_B` | µGy/día | Radiación en el detector de silicio **sin** la del generador del rover. **Es la radiación real del ambiente.** |
| `TOTAL_DOSE_E` | µGy/día | Radiación en un detector de plástico que imita el tejido humano. |

**Unidad µGy/día:** el gray (Gy) mide la energía de radiación absorbida. µGy/día es el *ritmo* al que llega la radiación (como km/h para la velocidad).

## Ejemplo de una fila

```
2026-09-12T17:01:38Z, 927.990, 180.703, 123.655, 184.812
```

El 12/09/2026 a las 17:01 UTC (12:01 en Lima), durante ~15,5 minutos, la radiación del ambiente en Marte llegaba a un ritmo de **123,7 µGy por día**.

## Resultados principales (columna `RTG_CORRECTED_DOSE_B`)

- **Promedio del periodo:** ~**130 µGy/día**
- **Variación entre días:** el promedio diario estuvo entre 126,3 y 134,1 µGy/día (cambia menos de 7%).
- **Variación dentro del día:** un poco más alta de noche (~131,7 µGy/día) y más baja al mediodía (~127,3 µGy/día), en hora local de Marte. Se relaciona con los cambios de presión del aire entre el día y la noche.

## Limpieza aplicada

- Se descartaron **87 mediciones incompletas** (`ALIVE_TIME` menor a 900 s).
- Se descartaron **5 valores extremos aislados**.
- El promedio casi no cambia con la limpieza (130,0 → 129,9 µGy/día).

## Notas

- El número `70348` del nombre es solo el número de descarga que asigna CDAWeb; no identifica un dataset distinto.
- Los datos venían como *All quality* (sin filtrar por calidad), por eso se hizo la limpieza anterior.
- No confundir µGy (energía absorbida) con mSv (que ajusta por daño biológico): no se comparan directamente.

## Cita

> Hassler, D. M., et al. (2012). *The Radiation Assessment Detector (RAD) Investigation*. Space Science Reviews, 170, 503–558. https://doi.org/10.1007/s11214-012-9913-1
