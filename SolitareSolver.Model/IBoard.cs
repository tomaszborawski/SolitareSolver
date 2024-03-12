using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolitareSolver.Model
{
    public interface IBoard
    {
        Hand Hand { get; } 
        ImmutableArray<Top>? Tops { get; }
        ImmutableArray<Column> Columns { get; }
        ImmutableArray<CardMove>? Moves { get; }

        void Show(ImmutableArray<CardMoves> moves);
        IBoard ExecuteMoves(CardMoves cardMoves);

        static IBoard Generate()
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

            for (int i = 0; i < 7; i++) // 7 column
            {
                var cards = ImmutableArray.CreateBuilder<Card>();
                for (int j = 0; j < i + 1; j++) // initial number of cards in column equal to column number
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
                columns.Add(new Column(i + 1, cards.ToImmutableArray(), i));
            }

            //Hand
            var hand = ImmutableArray.CreateBuilder<Card>();
            for (int cid = 0; cid < deck.Count; cid++)
            {
                if (!bitArray[cid]) hand.Add(deck[cid]);
            }

            return new Board(new Hand(hand.ToImmutableArray()), null, columns.ToImmutableArray(), null);
        }
    }
}
