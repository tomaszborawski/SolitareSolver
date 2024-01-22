using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolitareSolver.Model
{
    public abstract class CardMove(Card card)
    {
        public Card Card { get; init; } = card;
    }
}
