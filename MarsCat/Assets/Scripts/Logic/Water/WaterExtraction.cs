using System;
using MarsCat.Logic.Preparation;
using MarsCat.Logic.Resources;

namespace MarsCat.Logic.Water
{
    public sealed class WaterExtraction
    {
        private readonly IIceSource _source;
        private readonly IWaterTreatment _treatment;
        private readonly ResourcePool _pool;

        // Litros que realmente entraron al pool (el tope de los tanques puede cortar).
        public event Action<float> OnWaterExtracted;
        // Clave del aviso cuando el lote se pierde (agua hervida o no apta).
        public event Action<string> OnBatchLost;

        public WaterExtraction(IIceSource source, MeltingChamberSession chamber,
            IWaterTreatment treatment, ResourcePool pool)
        {
            _source = source ?? throw new ArgumentNullException(nameof(source));
            if (chamber == null)
                throw new ArgumentNullException(nameof(chamber));
            _treatment = treatment ?? throw new ArgumentNullException(nameof(treatment));
            _pool = pool ?? throw new ArgumentNullException(nameof(pool));
            if (chamber.IsFinished)
                throw new InvalidOperationException("La cámara ya terminó; la cadena debe crearse antes de jugarla.");
            chamber.OnFinished += HandleFinished;
        }

        private void HandleFinished(PreparationResult result)
        {
            if (!result.Starts)
            {
                OnBatchLost?.Invoke(result.ReasonKey);
                return;
            }

            IceBatch treated = _treatment.Treat(_source.Take());
            if (!_treatment.IsSafe(treated))
            {
                OnBatchLost?.Invoke("prep.water.unsafe");
                return;
            }

            float added = _pool.Add(ResourceId.Water, treated.Liters * treated.Purity * result.Yield);
            OnWaterExtracted?.Invoke(added);
        }
    }
}
