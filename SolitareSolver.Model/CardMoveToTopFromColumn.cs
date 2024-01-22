using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolitareSolver.Model
{
    public class CardMoveToTopFromColumn(Card card, int fromColuumn) : CardMove(card)
    {
        public int FromColumn { get; init; } = fromColuumn;
    }
}
