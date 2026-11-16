// Asistencia de IA: escrito con Claude Code (Anthropic).
namespace MarsCat.Logic.Water
{
    public interface IWaterTreatment
    {
        IceBatch Treat(IceBatch batch);
        bool IsSafe(IceBatch batch);
    }
}
