// Asistencia de IA: escrito con Claude Code (Anthropic).
using System;
using System.Collections.Generic;
using MarsCat.Logic.Preparation;

namespace MarsCat.Logic.Food
{
    // La Redada: preparación de la granja de ratones.
    public sealed class RatRaid
    {
        private readonly LooseRats _loose;
        private readonly RatFarm _farm;
        private readonly HashSet<int> _marked = new HashSet<int>();

        public RatRaid(LooseRats loose, RatFarm farm)
        {
            _loose = loose ?? throw new ArgumentNullException(nameof(loose));
            _farm = farm ?? throw new ArgumentNullException(nameof(farm));
        }

        public PreparationResult Result => _farm.CanStart
            ? PreparationResult.Ok(1f)
            : PreparationResult.Fail("prep.rat_farm.need_rats");

        public bool IsMarked(int ratId)
        {
            return _marked.Contains(ratId);
        }

        // Falso si la rata ya no está suelta o ya estaba marcada.
        public bool Mark(int ratId)
        {
            return _loose.IsLoose(ratId) && _marked.Add(ratId);
        }

        // Cancela la marca (por ejemplo, si la tarea puntual se descarta).
        public bool Unmark(int ratId)
        {
            return _marked.Remove(ratId);
        }

        // Falso si no estaba marcada, otro gato la cazó antes o la granja está llena.
        public bool Capture(int ratId)
        {
            if (!_marked.Contains(ratId))
                return false;
            if (!_loose.IsLoose(ratId))
            {
                _marked.Remove(ratId);
                return false;
            }
            if (_farm.AddRats(1) == 0)
                return false;
            _marked.Remove(ratId);
            _loose.TryTake(ratId);
            return true;
        }
    }
}
