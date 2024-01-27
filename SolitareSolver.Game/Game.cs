using SolitareSolver.Model;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolitareSolver.Game
{
    public static class Game
    {
        public static Table RemoveAcesFromHand(Table t) // Remove Aces from Hand
        {
            var ases = t.Hand.Cards!.Value.Where(o => o.Number == Numbers.Ace).Select(o => o).ToList();
            if (ases.Count > 0)
            {
                var tops = ImmutableArray.CreateBuilder<Top>();
                foreach (var item in ases)
                {
                    tops.Add(new Top(item.Color, [item]));
                }
                return t with { Hand = t.Hand with { Cards = t.Hand.Cards!.Value.RemoveRange(ases) }, Tops = tops.ToImmutableArray() };
            }
            else
                return t;
        }

        public static ImmutableArray<CardMoves> GetInitialMoves(Table t)
        {
            var ret = ImmutableArray.CreateBuilder<CardMoves>();

            RemoveAcesFromColumns(t, ret);


            //Map column
            var colids = Enumerable.Range(0, t.Columns.Length).ToList();
            var map = from cid1 in colids
                      from cid2 in colids
                      where cid1 < cid2
                      select new { cid1, cid2 };

            foreach (var item in map)
            {
                var colbegin = t.Columns[item.cid1];
                var colend = t.Columns[item.cid2];
                var cbegin = colbegin.Cards!.Value[colbegin.Position];
                var cend = colend.Cards!.Value[^1];
                CheckColumnsForMove(t, ret, colbegin, colend, cbegin, cend);

                colbegin = t.Columns[item.cid2];
                colend = t.Columns[item.cid1];
                cbegin = colbegin.Cards!.Value[colbegin.Position];
                cend = colend.Cards!.Value[^1];
                CheckColumnsForMove(t, ret, colbegin, colend, cbegin, cend);
            }

            return ret.ToImmutableArray();
        }

        public static void RemoveAcesFromColumns(Table t, ImmutableArray<CardMoves>.Builder ret)
        {
            //Remove Aces
            for (int i = 0; i < t.Columns.Length; i++)
            {
                if (t.Columns[i].Position >= 0)
                {
                    var c = t.Columns[i].Cards!.Value[t.Columns[i].Position];
                    if (c.Number == Numbers.Ace)
                    {
                        ret.Add(new CardMoves([new CardMoveToTopFromColumn(c, t.Columns[i].ID)], [t.Columns[i].ID]));
                    }
                }
            }
        }

        public static void CheckColumnsForMove(Table t, ImmutableArray<CardMoves>.Builder ret, Column colbegin, Column colend, Card cbegin, Card cend)
        {
            if (cbegin.Number != Numbers.Ace && cend.Number != Numbers.Ace)
            {
                //Search if we have all cards in Hand
                var moves = ImmutableArray.CreateBuilder<CardMove>();
                var number = cend.Number - 1;
                var color = cend.Color;
                var found = false;
                while (number > cbegin.Number)
                {
                    var card = t.Hand.Cards!.Value.Where(o => o.Number == number && HelperData.CheckColors(o.Color, color)).FirstOrDefault();
                    found = card != null;
                    if (found)
                    {
                        moves.Add(new CardMoveFromHand(card, colend.ID));
                        color = card.Color;
                        number--;
                    }
                    else
                        break;
                }
                if (found)
                {
                    if (HelperData.CheckColors(color, cbegin.Color))
                    {
                        for (int i = colbegin.Position; i < colbegin.Cards!.Value.Length; i++)
                        {
                            moves.Add(new CardMoveFromColumn(colbegin.Cards!.Value[i], colbegin.ID, colend.ID));
                        }
                        ret.Add(new CardMoves(moves.ToImmutableArray(), [colbegin.ID, colend.ID]));
                    }
                }
            }
        }
        public static ImmutableArray<CardMoves> GetNextMoves(Table t, ImmutableArray<CardMoves> oldmoves, List<int> afectedColumns)
        {
            //Delete moves that we can't process because previosly we use these column
            var ret = oldmoves.ToBuilder();
            var toDelete = oldmoves.Where(o => o.AfectedColumns.Intersect(afectedColumns).Any()).ToList();
            ret.RemoveRange(toDelete);


            RemoveAcesFromColumns(t, ret);

            //Map column
            var colids = Enumerable.Range(0, t.Columns.Length).ToList();
            var map = (from cid1 in colids
                       from cid2 in afectedColumns
                       where cid1 + 1 != cid2
                       select new { cid1 = cid1 + 1, cid2 }).ToList();

            //Remove duplicate
            if (afectedColumns.Count == 2)
            {
                var cid1 = afectedColumns[0];
                var cid2 = afectedColumns[1];
                var m = map.Single(o => o.cid1 == cid1 && o.cid2 == cid2);
                map.Remove(m);
            }

            foreach (var item in map)
            {
                var col1 = t.Columns.Single(o => o.ID == item.cid1);
                var col2 = t.Columns.Single(o => o.ID == item.cid2);
                if (col1.Position >= 0 && col2.Position >= 0)
                {
                    var colbegin = col1;
                    var colend = col2;
                    var cbegin = colbegin.Cards!.Value[colbegin.Position];
                    var cend = colend.Cards!.Value[^1];
                    CheckColumnsForMove(t, ret, colbegin, colend, cbegin, cend);

                    colbegin = col2;
                    colend = col1;
                    cbegin = colbegin.Cards!.Value[colbegin.Position];
                    cend = colend.Cards!.Value[^1];
                    CheckColumnsForMove(t, ret, colbegin, colend, cbegin, cend);
                }
            }

            MoveKingFromColumnOrHand(t, ret);

            MoveCardfromHandAndColumnToTop(t, ret);

            return ret.ToImmutableArray();
        }

        public static void MoveCardfromHandAndColumnToTop(Table t, ImmutableArray<CardMoves>.Builder ret)
        {
            if (t.Tops != null)
            {
                foreach (var item in t.Tops)
                {
                    var card = item.Cards[^1];
                    card = card with { Number = card.Number + 1 };

                    //From Hand
                    if (!ret.Any(o => o.Moves.OfType<CardMoveToTopFromHand>().Any()))
                    {
                        if (t.Hand.Cards != null && t.Hand.Cards.Value.Contains(card))
                        {
                            ret.Add(new CardMoves([new CardMoveToTopFromHand(card)], []));
                        }
                    }

                    //From Column
                    foreach (var col in t.Columns.Where(o => o.Cards != null && o.Cards.Value.Length > 0))
                    {
                        if (col.Cards!.Value[^1] == card)
                        {
                            ret.Add(new CardMoves([new CardMoveToTopFromColumn(card, col.ID)], [col.ID]));
                        }
                    }
                }
            }
        }

        public static void MoveKingFromColumnOrHand(Table t, ImmutableArray<CardMoves>.Builder ret)
        {
            // Move King from Column or Hand to empty Column
            var emptyCols = t.Columns.Where(o => !o.Cards.HasValue || o.Cards.Value.Length == 0).ToList();
            if (emptyCols.Count > 0)
            {
                //from Column
                var kingCols = t.Columns.Where(o => o.Cards.HasValue && o.Position > 0 && o.Cards.Value[o.Position].Number == Numbers.King).ToList();
                for (int i = 0; i < kingCols.Count; i++)
                {
                    var moves = ImmutableArray.CreateBuilder<CardMove>();
                    for (int j = kingCols[i].Position; j < kingCols[i].Cards!.Value.Length; j++)
                    {
                        moves.Add(new CardMoveFromColumn(kingCols[i].Cards!.Value[j], kingCols[i].ID, emptyCols[i % emptyCols.Count].ID));
                    }
                    ret.Add(new CardMoves(moves.ToImmutableArray(), [emptyCols[i % emptyCols.Count].ID, kingCols[i].ID]));
                }

                //from Hand
                foreach (var colbegin in t.Columns.Where(o => o.Position > 0))
                {
                    int j = 0;
                    var moves = ImmutableArray.CreateBuilder<CardMove>();
                    var cend = colbegin.Cards!.Value[colbegin.Position];
                    var number = cend.Number + 1;
                    var color = cend.Color;
                    var found = false;
                    while (number <= Numbers.King)
                    {
                        var card = t.Hand.Cards!.Value.Where(o => o.Number == number && HelperData.CheckColors(o.Color, color)).FirstOrDefault();
                        found = card != null;
                        if (found)
                        {
                            moves.Add(new CardMoveFromHand(card, emptyCols[j % emptyCols.Count].ID));
                            color = card.Color;
                            number++;
                        }
                        else
                            break;
                    }
                    if (found)
                    {
                        for (int i = colbegin.Position; i < colbegin.Cards!.Value.Length; i++)
                        {
                            moves.Add(new CardMoveFromColumn(colbegin.Cards!.Value[i], colbegin.ID, emptyCols[j % emptyCols.Count].ID));
                        }
                        ret.Add(new CardMoves(moves.ToImmutableArray(), [colbegin.ID, emptyCols[j % emptyCols.Count].ID]));
                        j++;
                    }
                }
            }
        }
    }
}
