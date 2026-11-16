namespace MarsCat.Logic.Water
{
    public sealed class NoTreatment : IWaterTreatment
    {
        public IceBatch Treat(IceBatch batch)
        {
            return batch;
        }

        public bool IsSafe(IceBatch batch)
        {
            return true;
        }
    }
}
