using SolitareSolver.Model;
using System.Collections.Immutable;
using System.Data.Common;
using System.Drawing;

namespace SolitareSolver.Test
{
    public class TableTests
    {
        private IBoard? t;

        [SetUp]
        public void Setup()
        {
            var handcards = ImmutableArray.CreateBuilder<Card>();
            handcards.Add(new Card(Colors.Hearts, Numbers.Ace));
            handcards.Add(new Card(Colors.Spades, Numbers.Ten));

            var columns = ImmutableArray.CreateBuilder<Column>();
            columns.Add(new Column(1, ImmutableArray<Card>.Empty.Add(new(Colors.Hearts, Numbers.King)), 0));
            columns.Add(new Column(2, ImmutableArray<Card>.Empty.Add(new(Colors.Spades, Numbers.Queen)), 0));
            columns.Add(new Column(3, ImmutableArray<Card>.Empty.Add(new(Colors.Hearts, Numbers.Jack)), 0));
            columns.Add(new Column(4, ImmutableArray<Card>.Empty.Add(new(Colors.Spades, Numbers.Ace)), 0));

            t = new Board(new Hand(handcards.ToImmutableArray()), null, columns.ToImmutableArray(), null);
        }

        [Test]
        public void TestCardMoveFromHand()
        {
            if (t != null)
            {
                var card = new Card(Colors.Spades, Numbers.Ten);
                var colid = 3;
                var moves = ImmutableArray<CardMove>.Empty.Add(new CardMoveFromHand(card, colid));

                t = t.ExecuteMoves(new CardMoves(moves, [colid]));

                Assert.That(t.Hand.Cards!.Value, Does.Not.Contain(card));
                var column = t.Columns.Single(o => o.ID == colid);
                Assert.That(column.Cards!.Value[^1], Is.EqualTo(card));
            }
        }

        [Test]
        public void TestCardMoveFromColumn()
        {
            if (t != null)
            {
                var card = new Card(Colors.Spades, Numbers.Queen);
                var colfrom = 2;
                var colto = 1;
                var moves = ImmutableArray<CardMove>.Empty.Add(new CardMoveFromColumn(card, colfrom, colto));

                t = t.ExecuteMoves(new CardMoves(moves, [colfrom, colto]));

                var columnfrom = t.Columns.Single(o => o.ID == colfrom);
                Assert.Multiple(() =>
                {
                    Assert.That(columnfrom.Position, Is.EqualTo(-1));
                    Assert.That(columnfrom.Cards!.Value, Is.Empty);
                });
                var columnto = t.Columns.Single(o => o.ID == colto);
                Assert.That(columnto.Cards!.Value[^1], Is.EqualTo(card));
            }
        }

        [Test]
        public void TestCardMoveToTopFromHand()
        {
            if (t != null)
            {
                var card = new Card(Colors.Hearts, Numbers.Ace);
                var moves = ImmutableArray<CardMove>.Empty.Add(new CardMoveToTopFromHand(card));

                t = t.ExecuteMoves(new CardMoves(moves, []));

                Assert.Multiple(() =>
                {
                    Assert.That(t.Tops, Is.Not.Null);
                    Assert.That(t.Tops!.Value, Has.Length.EqualTo(1));
                    Assert.That(card.Color, Is.EqualTo(t.Tops!.Value[0].Color));
                    Assert.That(t.Tops!.Value[0].Cards[^1], Is.EqualTo(card));
                });
            }
        }

        [Test]
        public void TestCardMoveToTopFromColumn()
        {
            if (t != null)
            {
                var card = new Card(Colors.Spades, Numbers.Ace);
                var colid = 4;
                var moves = ImmutableArray<CardMove>.Empty.Add(new CardMoveToTopFromColumn(card, colid));

                t = t.ExecuteMoves(new CardMoves(moves, [colid]));

                var columnfrom = t.Columns.Single(o => o.ID == colid);
                Assert.Multiple(() =>
                {
                    Assert.That(columnfrom.Position, Is.EqualTo(-1));
                    Assert.That(columnfrom.Cards!.Value, Is.Empty);
                });

                Assert.Multiple(() =>
                {
                    Assert.That(t.Tops, Is.Not.Null);
                    Assert.That(t.Tops!.Value, Has.Length.EqualTo(1));
                    Assert.That(card.Color, Is.EqualTo(t.Tops!.Value[0].Color));
                    Assert.That(t.Tops!.Value[0].Cards[^1], Is.EqualTo(card));
                });
            }
        }
    }
}