using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolitareSolver.Model
{
    public class CardMoveFromHand(Card card, int toColumn) : CardMove(card)
    {
        public int ToColumn { get; init; } = toColumn;
    }
}
