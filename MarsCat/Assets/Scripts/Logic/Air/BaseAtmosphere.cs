// Asistencia de IA: escrito con Claude Code (Anthropic).
using System;

namespace MarsCat.Logic.Air
{
    // Aire de la base: se guarda la masa de O2 y el % sale de masa / (volumen x densidad).
    // Aproximación declarada: la masa de aire es volumen x densidad, sin cambios de presión.
    public sealed class BaseAtmosphere
    {
        private readonly AtmosphereConfig _config;
        private double _volumeM3;
        private double _oxygenKg;
        private double _leakKgPerSecond;
        private float _criticalElapsed;
        private bool _timeoutFired;

        public event Action<AirBand> OnBandChanged;
        public event Action OnCriticalTimeout;

        public AirBand Band { get; private set; }
        public float VolumeM3 { get { return (float)_volumeM3; } }
        public float OxygenKg { get { return (float)_oxygenKg; } }
        public float LeakKgPerSecond { get { return (float)_leakKgPerSecond; } }
        public float OxygenFraction { get { return (float)Fraction(); } }

        public BaseAtmosphere(AtmosphereConfig config, float initialVolumeM3)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));
            if (float.IsNaN(initialVolumeM3) || float.IsInfinity(initialVolumeM3) || initialVolumeM3 <= 0f)
                throw new ArgumentOutOfRangeException(nameof(initialVolumeM3),
                    "El volumen debe ser finito y mayor que cero.");
            _config = config;
            _volumeM3 = initialVolumeM3;
            _oxygenKg = (double)config.InitialFraction * _volumeM3 * config.AirDensityKgPerM3;
            Band = BandFor(Fraction());
        }

        // Presuriza un módulo: el aire nuevo entra con su % propio, así que el % de la base
        // baja una sola vez por la mezcla; la fuga queda permanente (kg de O2 por sol).
        public void AddModule(float volumeM3, float leakKgPerSol)
        {
            if (float.IsNaN(volumeM3) || float.IsInfinity(volumeM3) || volumeM3 <= 0f)
                throw new ArgumentOutOfRangeException(nameof(volumeM3),
                    "El volumen debe ser finito y mayor que cero.");
            float leakPerSecond = _config.ToRatePerSecond(leakKgPerSol);
            _volumeM3 += volumeM3;
            _oxygenKg += (double)volumeM3 * _config.NewModuleFraction * _config.AirDensityKgPerM3;
            _leakKgPerSecond += leakPerSecond;
            Reclassify();
        }

        public void Tick(float gameDt, float producedKg, int catsResting, int catsWorking)
        {
            Tick(gameDt, producedKg, catsResting, catsWorking, gameDt);
        }

        // realDt es el tiempo real transcurrido: el margen en crítico se cuenta en segundos reales.
        public void Tick(float gameDt, float producedKg, int catsResting, int catsWorking, float realDt)
        {
            ValidateNonNegative(gameDt, nameof(gameDt));
            ValidateNonNegative(producedKg, nameof(producedKg));
            ValidateNonNegative(realDt, nameof(realDt));
            if (catsResting < 0)
                throw new ArgumentOutOfRangeException(nameof(catsResting), "La cantidad no puede ser negativa.");
            if (catsWorking < 0)
                throw new ArgumentOutOfRangeException(nameof(catsWorking), "La cantidad no puede ser negativa.");

            double consumed = ((double)catsResting * _config.RestKgPerSecond
                + (double)catsWorking * _config.WorkKgPerSecond + _leakKgPerSecond) * gameDt;
            double max = _volumeM3 * _config.AirDensityKgPerM3;
            _oxygenKg = Math.Min(max, Math.Max(0.0, _oxygenKg + producedKg - consumed));
            Reclassify();

            if (Band == AirBand.Critical && !_timeoutFired)
            {
                _criticalElapsed += realDt;
                if (_criticalElapsed >= _config.CriticalSeconds)
                {
                    _timeoutFired = true;
                    var handler = OnCriticalTimeout;
                    if (handler != null)
                        handler();
                }
            }
        }

        private double Fraction()
        {
            return _oxygenKg / (_volumeM3 * _config.AirDensityKgPerM3);
        }

        private AirBand BandFor(double fraction)
        {
            if (fraction >= _config.YellowBelow) return AirBand.Green;
            if (fraction >= _config.RedBelow) return AirBand.Yellow;
            if (fraction >= _config.CriticalBelow) return AirBand.Red;
            return AirBand.Critical;
        }

        private void Reclassify()
        {
            AirBand band = BandFor(Fraction());
            if (band == Band)
                return;
            Band = band;
            if (band != AirBand.Critical)
            {
                _criticalElapsed = 0f;
                _timeoutFired = false;
            }
            var handler = OnBandChanged;
            if (handler != null)
                handler(band);
        }

        private static void ValidateNonNegative(float value, string name)
        {
            if (float.IsNaN(value) || float.IsInfinity(value) || value < 0f)
                throw new ArgumentOutOfRangeException(name, "El valor debe ser finito y no negativo.");
        }
    }
}
