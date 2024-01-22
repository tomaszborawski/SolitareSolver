using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolitareSolver.Model
{
    public record CardMoves(ImmutableArray<CardMove> Moves, List<int> AfectedColumns);
}
