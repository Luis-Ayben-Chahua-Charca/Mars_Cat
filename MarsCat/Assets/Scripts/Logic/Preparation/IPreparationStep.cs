namespace MarsCat.Logic.Preparation
{
    // Un paso de una preparación de valor guía y rango. Cada paso guarda sus propios valores.
    public interface IPreparationStep
    {
        string Id { get; }

        StepScore Evaluate();

        // Indica si el pool alcanza para cobrar este paso, sin descontar nada.
        bool CanCharge();

        // Cobra el paso al confirmar. Todo o nada; false si no alcanza.
        bool TryCharge();
    }
}
