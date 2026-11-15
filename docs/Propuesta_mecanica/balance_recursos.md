# Contexto de Balance Económico y Recursos (MVP y Post-MVP)
**Proyecto:** Junior Astronaut Mission Trainer (NASA Space Apps Challenge)
**Objetivo del Documento:** Proveer las métricas exactas, flujos diarios (Soles) y variables matemáticas para programar la economía del juego.

## 1. Reglas Globales y Tiempos
* **Ciclo Base:** 1 Sol (Día marciano). Todas las métricas de producción y consumo se calculan por Sol.
* **Población Base:** 20 Gatos (Obreros/Consumidores finales).
* **Licencia Creativa 1 (Flujo Continuo):** Las máquinas y procesos biológicos (biorreactor, cultivos, granjas) entregan sus recursos de forma diaria (lotes por Sol), simulando un ecosistema ya maduro, para no romper la jugabilidad con tiempos de espera reales (meses).
* **Licencia Creativa 2 (Súper Fertilidad):** Las ratas de laboratorio están genéticamente modificadas para crecer y reproducirse a tasas extremas, requiriendo una población menor para sostener la colonia.

---

## 2. Fases del Juego (Manejo de Estados)

### A. Estado MVP (Supervivencia Inicial / Reloj de Tensión)
* **Regla principal:** NO hay cultivos vivos. NO hay agricultura. Todo depende de reservas finitas dejadas por los humanos.
* **Inventario Inicial Crítico (El Reloj):**
  * **Pellets/Reservas vegetales:** 10 kg. (Suficiente para alimentar a la granja de ratas por 10 Soles).
  * Si se agotan los pellets antes de desbloquear la hidroponía (Post-MVP), la granja colapsa y los gatos mueren de hambre 2 días después.

### B. Estado Post-MVP (Circuito Cerrado / Expansión)
* **Regla principal:** Se desbloquea la **Hidroponía (Papa Marciana - Proyecto CIP-NASA)** y el **Reactor KRUSTY**.
* El sistema pasa de gastar inventario finito a un ecosistema de circuito cerrado donde el output de un edificio es el input de otro.

---

## 3. Balance de Recursos: La Cadena Trófica (Comida)
**Flujo Post-MVP:** Cultivos (1 kg biomasa) -> Ratas (Criadero de 50) -> Gatos (20 raciones).

* **Consumidor Final: Gatos (x20)**
  * **Consumo:** 20 raciones de comida al día (1 por gato/sol).
  * **Condición de Muerte:** Si un gato pasa **2 Soles seguidos** sin comer, muere (espiral de muerte).
* **Transformador: Granja de Ratas (x50 reproductoras)**
  * **Consumo:** 1 kg (1,000 g) de biomasa vegetal o pellets al día.
  * **Producción:** 20 raciones de comida (ratas adultas procesadas) al día.
* **Productor: Hidroponía de Papa (33 m²)**
  * **Consumo:** 95 Litros de agua/sol + Energía (Iluminación LED).
  * **Producción:** 1 kg (1,000 g) de biomasa total al día (tallo + papa).
  * *Nota:* La papa fue elegida por ser altamente eficiente en el uso de agua (2.88 L/m²).

---

## 4. Balance de Recursos: El Agua (Líquido Vital)
**Flujo:** Taladro -> Tanques -> Gatos/Cultivos/Biorreactor -> Reciclaje.

* **Consumo Total Diario: ~105 Litros / Sol**
  * **Gatos:** ~5 L/sol (Para beber).
  * **Biorreactor:** ~5 L/sol (Para minijuegos de control térmico).
  * **Hidroponía (Papa 33 m²):** 95 L/sol (Pérdida por entropía/transpiración no reciclable).
* **Producción (Taladros Térmicos):**
  * **Taladro Base (Mid-Game / Solar):** Produce **133 L/sol**. (Deja un pequeño superávit de +28 L/sol para emergencias).
  * **Taladro Mejorado (Late-Game / KRUSTY):** Hasta 2,880 L/sol (Rompe el cuello de botella).

---

## 5. Balance de Recursos: El Oxígeno (Soporte Vital)
**Flujo:** Papas + MOXIEs -> Tanques O2 -> Gatos + Ratas.
* **Consumo Total Diario: 1.75 kg de O₂ / Sol**
  * **Gatos:** 1.60 kg/sol (0.08 kg por gato).
  * **Ratas (x50):** 0.15 kg/sol.
* **Meta de Producción Óptima:** **2.20 kg / Sol** (Incluye margen de seguridad del 25% para llenar los tanques de reserva contra fallos).
* **Producción Total (Cómo llegar a 2.21 kg):**
  * **Papa Marciana (33 m²):** Produce 1.06 kg/sol de manera pasiva.
  * **4 Unidades MOXIE limpias:** Producen 1.15 kg/sol (0.288 kg c/u).
  * *Riesgo (Entropía):* Si los filtros MOXIE se ensucian con polvo, su eficiencia cae al 50%, provocando un déficit de oxígeno. Requiere mantenimiento del gato conserje.

---

## 6. Balance de Recursos: Energía Eléctrica (El Cuello de Botella Final)
**Flujo:** Paneles solares / Reactor -> Edificios.
Unidad de medida: kWh / Sol (Kilovatios-hora por día marciano).

* **Consumo Total de la Base: ~141 kWh / Sol**
  * **4x MOXIEs:** 28.8 kWh/sol.
  * **1x Taladro Térmico (133 L):** 42.5 kWh/sol.
  * **Invernadero LED (33 m²):** ~60 kWh/sol.
  * **Sistemas Base + Biorreactor:** ~10 kWh/sol.
* **Generación (El Reto Logístico):**
  * **Panel Solar (Arreglo de 10 m²):** Produce solo **+6 kWh/sol**.
    * *Dificultad:* Para sostener la base completa, el jugador necesita **24 arreglos solares**, los cuales se ensucian con el Polvo (Opacidad Tau / $\tau$) y deben ser limpiados usando CO₂ comprimido (que a su vez gasta energía).
  * **Reactor Nuclear KRUSTY (Late-Game):** Produce **+240 kWh/sol**.
    * *Ventaja:* Ignora el polvo marciano, produce el doble de lo que la base necesita y permite automatizar todo.
    * *Riesgo:* Vulnerable a PEMs (Llamaradas Solares / Datos API DONKI), requiere mantenimiento de calor y materiales raros.

---

## 7. Manejo de Desechos (Biorreactor)
* **Input:** Desechos orgánicos de los gatos y biomasa muerta.
* **Output:** Abono (Acelera cultivos o limpia percloratos del regolito).
* **Mecánica (Flujo Continuo):** Produce 1 unidad de abono por Sol. Si falla la temperatura o humedad (Minijuego "Sopa de Bacterias"), la colonia bacteriana muere y se impone un "retraso (cooldown) de 1 Sol" sin producción.