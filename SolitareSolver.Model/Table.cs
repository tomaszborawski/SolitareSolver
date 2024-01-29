using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SolitareSolver.Model
{
    public record Table(Hand Hand, ImmutableArray<Top>? Tops, ImmutableArray<Column> Columns, ImmutableArray<CardMove>? Moves)
    {
        public static Table Generate()
        {
            // Generate deck
            var colors = Enum.GetValues<Colors>();
            var numbers = Enum.GetValues<Numbers>();
            var deck = (from c in colors
                        from n in numbers
                        select new Card(c, n)).ToList();

            // Bit array for selected cards
            var bitArray = new BitArray(deck.Count);

            // Columns data
            var columns = ImmutableArray.CreateBuilder<Column>();

            // Random
            var rand = new Random();

            for(int i = 0; i<7;i++) // 7 column
            {
                var cards = ImmutableArray.CreateBuilder<Card>();
                for (int j = 0; j < i+1; j++) // initial number of cards in column equal to column number
                {
                    // Select card
                    int cid;
                    do
                    {
                        cid = rand.Next(deck.Count);

                    } while (bitArray[cid]);
                    bitArray[cid] = true;
                    cards.Add(deck[cid]);
                }
                columns.Add(new Column(i+1, cards.ToImmutableArray(), i));
            }

            //Hand
            var hand = ImmutableArray.CreateBuilder<Card>();
            for (int cid = 0; cid < deck.Count; cid++)
            {
                if (!bitArray[cid]) hand.Add(deck[cid]);
            }

            return new Table(new Hand(hand.ToImmutableArray()), null, columns.ToImmutableArray(), null);
        }

        public void Show(ImmutableArray<CardMoves> moves)
        {
            Console.Clear();

            //Tops
            var left = 0;
            if (Tops != null)
            {
                foreach (var item in Tops)
                {
                    Console.CursorLeft = left;
                    Console.Write(item.Color.GetText());
                    Console.Write(item.Cards[^1].Number.GetText());
                    Console.CursorLeft = left + 4;
                    Console.Write("|");
                    left += 6;
                }
            }

            //Columns
            int maxtop = 2;
            foreach (var item in Columns)
            {
                if (item.Cards != null)
                {
                    var top = 2;
                    for (int i = 0; i < item.Cards.Value.Length; i++)
                    {
                        Console.CursorLeft = (item.ID-1)*6;
                        Console.CursorTop = top;
                        Console.ForegroundColor = (i < item.Position) ? ConsoleColor.White : ConsoleColor.Red;
                        Console.Write(item.Cards.Value[i].Color.GetText());
                        Console.Write(item.Cards.Value[i].Number.GetText());
                        Console.CursorLeft = (item.ID - 1)*6 + 4;
                        Console.Write("|");
                        top++;
                    }
                    if (top>maxtop) maxtop = top;
                }
            }

            //Result
            if (moves.Length == 0)
            {
                Console.CursorLeft = 0;
                Console.CursorTop = maxtop + 2;
                Console.ForegroundColor = ConsoleColor.Blue;

                if (Columns.All(o => o.Position == -1))
                    Console.WriteLine($"Solitare solved. Number of movements {Moves!.Value.Length} ");
                else
                    Console.WriteLine($"Solitate unsolved");
            }

        }
        public Table ExecuteMoves(CardMoves cardMoves)
        {
            //copy columns
            var columnlist = new List<Column>();
            foreach (var item in cardMoves.AfectedColumns)
            {
                columnlist.Add(Columns.Single(o => o.ID == item));
            }
            var orgcolumntab = columnlist.ToArray();
            //copy hand
            var hand = Hand;

            //copy Tops
            var topslsit = Tops?.ToList() ?? [];
            var orgtopslist = Tops?.ToList() ?? [];
            int? fromColumn = null;

            foreach (var item in cardMoves.Moves)
            {
                switch (item)
                {
                    case CardMoveFromHand cm1: //Remove from Hand and add to Column
                        hand = new Hand(hand.Cards!.Value.Remove(item.Card));
                        var col = columnlist.Single(o => o.ID == cm1.ToColumn);
                        var ind = columnlist.IndexOf(col);
                        columnlist[ind] = columnlist[ind] with { Cards = col.Cards!.Value.Add(item.Card) };
                        break;
                    case CardMoveFromColumn cm2: // Move card from Column to Column
                        var col1 = columnlist.Single(o => o.ID == cm2.FromColumn);
                        var ind1 = columnlist.IndexOf(col1);
                        columnlist[ind1] = columnlist[ind1] with { Cards = col1.Cards!.Value.Remove(item.Card) };
                        var col2 = columnlist.Single(o => o.ID == cm2.ToColumn);
                        var ind2 = columnlist.IndexOf(col2);
                        columnlist[ind2] = columnlist[ind2] with { Cards = col2.Cards!.Value.Add(item.Card) };
                        fromColumn = cm2.FromColumn;
                        break;
                    case CardMoveToTopFromHand cm3: //Move card from Hand to Top
                        hand = hand with { Cards = hand.Cards!.Value.Remove(item.Card) };

                        TopOperation(topslsit, cm3.Card);
                        break;
                    case CardMoveToTopFromColumn cm4: // Move card from Column to Top
                        col1 = columnlist.Single(o => o.ID == cm4.FromColumn);
                        ind1 = columnlist.IndexOf(col1);
                        columnlist[ind1] = columnlist[ind1] with { Cards = col1.Cards!.Value.Remove(item.Card) };
                        fromColumn = cm4.FromColumn;

                        TopOperation(topslsit, cm4.Card);
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }

            //We only update position once
            if (fromColumn.HasValue)
            {
                var col = columnlist.Single(o => o.ID == fromColumn);
                var ind = columnlist.IndexOf(col);
                columnlist[ind] = columnlist[ind] with { Position = col.Position - 1 };
            }

            //Add Moves
            var moves = (Moves == null) ? ImmutableArray.CreateBuilder<CardMove>() : Moves!.Value.ToBuilder();
            moves.AddRange(cardMoves.Moves);

            //Change in Tops
            var tops = (Tops == null) ? ImmutableArray.CreateBuilder<Top>() : Tops!.Value.ToBuilder();
            if (orgtopslist != null) tops.RemoveRange(orgtopslist);
            if (topslsit != null) tops.AddRange(topslsit);

            return new Table(hand, tops.ToImmutableArray(), Columns.RemoveRange(orgcolumntab).AddRange(columnlist), moves.ToImmutableArray());
        }

        private static void TopOperation(List<Top> topslsit, Card card)
        {
            ImmutableArray<Card>.Builder topcards;
            Top? top;
            // Remove old top if needed
            top = topslsit.SingleOrDefault(o => o.Color == card.Color);
            if (top != null)
            {
                topslsit.Remove(top);
                topcards = top.Cards.ToBuilder();
            }
            else
                topcards = ImmutableArray.CreateBuilder<Card>();

            //Add card to the top
            topcards.Add(card);
            top = new Top(card.Color, topcards.ToImmutableArray());
            topslsit.Add(top);
        }
    }
}
