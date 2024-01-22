using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolitareSolver.Model
{
    public class CardMoveFromColumn(Card card, int fromColumn, int toColumn) : CardMove(card)
    {
        public int FromColumn { get; init; } = fromColumn;
        public int ToColumn { get; init; } = toColumn;
    }
}
