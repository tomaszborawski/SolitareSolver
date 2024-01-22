// See https://aka.ms/new-console-template for more information
using System.Collections.Immutable;
using SolitareSolver;
using SolitareSolver.Model;
var table = Table.Generate();
table = Game.RemoveAcesFromHand(table);

var moves = Game.GetInitialMoves(table);
foreach (var move in moves)
{
    GetAndExecuteNextMoves(table, moves, move);
}
Console.ReadLine();

static void GetAndExecuteNextMoves(Table table, ImmutableArray<CardMoves> moves, CardMoves move)
{
    table = table.ExecuteMoves(move);
    table.Show(moves);
    var newmoves = Game.GetNextMoves(table, moves, move.AfectedColumns);
    for(int i=0; i<newmoves.Length; i++)
    {
        var item = newmoves[i];
        GetAndExecuteNextMoves(table, newmoves.Remove(item), item);
    }
}