using System;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SolitareSolver.Model;
using SolitareSolver.Game;

namespace SolitareSolver.Test
{
    public class GameTests
    {
        [Test]
        public void TestRemoveAcesFromHand()
        {
            var card = new Card(Colors.Spades, Numbers.Ace);
            var t = new Table(new Hand(ImmutableArray<Card>.Empty.Add(card)), null, [], null);

            var moves  = Game.Game.RemoveAcesFromHand(t);

            Assert.Multiple(() =>
            {
                Assert.That(moves, Has.Length.EqualTo(1));
                Assert.That(moves[0].Moves, Has.Length.EqualTo(1));
                Assert.That(moves[0].Moves[0].Card,Is.EqualTo(card));
                Assert.That(moves[0].AfectedColumns, Has.Count.EqualTo(0));
            });
        }

        [Test]
        public void TestRemoveAcesFromColumns()
        {
            var card = new Card(Colors.Spades, Numbers.Ace);
            var t = new Table(new Hand(null), null, [new Column(1, ImmutableArray<Card>.Empty.Add(card), 0)], null);

            var ret = ImmutableArray.CreateBuilder<CardMoves>();

            Game.Game.RemoveAcesFromColumns(t, ret);

            Assert.Multiple(() =>
            {
                Assert.That(ret, Has.Count.EqualTo(1));
                Assert.That(ret[0].AfectedColumns, Has.Count.EqualTo(1));
                Assert.That(ret[0].AfectedColumns[0], Is.EqualTo(1));
                Assert.That(ret[0].Moves, Has.Length.EqualTo(1));
                Assert.That(ret[0].Moves[0].GetType(), Is.EqualTo(typeof(CardMoveToTopFromColumn)));
                Assert.That(ret[0].Moves[0].Card, Is.EqualTo(card));
                Assert.That(((CardMoveToTopFromColumn)(ret[0].Moves[0])).FromColumn, Is.EqualTo(1));
            });
        }

        [Test]
        public void TestCheckColumnsForMove()
        {
            var card1 = new Card(Colors.Clubs, Numbers.Ace);
            var card2 = new Card(Colors.Hearts, Numbers.Ten);
            var card3 = new Card(Colors.Hearts, Numbers.Two);
            var card4 = new Card(Colors.Hearts, Numbers.Queen);
            var card5 = new Card(Colors.Clubs, Numbers.King);
            var card6 = new Card(Colors.Clubs, Numbers.Jack);

            var hand = new Hand([card4]);
            var column1 = new Column(1, [card1, card6, card2], 1);
            var column2 = new Column(2, [card3, card5], 1);
            var t = new Table(hand, null, [column1, column2], null);

            var ret = ImmutableArray.CreateBuilder<CardMoves>();

            Game.Game.CheckColumnsForMove(t, ret, column1, column2, card6, card5);


            Assert.That(ret, Has.Count.EqualTo(1));
            var move = ret[0];
            Assert.Multiple(() =>
            {
                Assert.That(move.AfectedColumns, Has.Count.EqualTo(2));
                Assert.That(move.Moves, Has.Length.EqualTo(3));
                Assert.That(move.Moves[0].Card, Is.EqualTo(card4));
                Assert.That(move.Moves[0].GetType(), Is.EqualTo(typeof(CardMoveFromHand)));
                Assert.That(((CardMoveFromHand)(move.Moves[0])).ToColumn, Is.EqualTo(2));
                Assert.That(move.Moves[1].Card, Is.EqualTo(card6));
                Assert.That(move.Moves[1].GetType(), Is.EqualTo(typeof(CardMoveFromColumn)));
                Assert.That(((CardMoveFromColumn)(move.Moves[1])).ToColumn, Is.EqualTo(2));
                Assert.That(((CardMoveFromColumn)(move.Moves[1])).FromColumn, Is.EqualTo(1));
                Assert.That(move.Moves[2].Card, Is.EqualTo(card2));
                Assert.That(move.Moves[2].GetType(), Is.EqualTo(typeof(CardMoveFromColumn)));
                Assert.That(((CardMoveFromColumn)(move.Moves[2])).ToColumn, Is.EqualTo(2));
                Assert.That(((CardMoveFromColumn)(move.Moves[2])).FromColumn, Is.EqualTo(1));
            });
        }

        [Test]
        public void TestMoveKingFromColumn()
        {
            var card1 = new Card(Colors.Spades, Numbers.Two);
            var card2 = new Card(Colors.Spades, Numbers.King);
            var card3 = new Card(Colors.Diamonds, Numbers.Queen);
            var column1 = new Column(1, [], -1);
            var column2 = new Column(2, [card1, card2, card3], 1);

            var t = new Table(new Hand(null), null, [column1, column2], null);

            var ret = ImmutableArray.CreateBuilder<CardMoves>();

            Game.Game.MoveKingFromColumnOrHand(t, ret);

            Assert.That(ret, Has.Count.EqualTo(1));
            var move = ret[0];
            Assert.Multiple(() =>
            {
                Assert.That(move.Moves, Has.Length.EqualTo(2));
                Assert.That(move.Moves[0].Card, Is.EqualTo(card2));
                Assert.That(move.Moves[0].GetType(), Is.EqualTo(typeof(CardMoveFromColumn)));
                Assert.That(((CardMoveFromColumn)(move.Moves[0])).ToColumn, Is.EqualTo(1));
                Assert.That(((CardMoveFromColumn)(move.Moves[0])).FromColumn, Is.EqualTo(2));
                Assert.That(move.Moves[1].Card, Is.EqualTo(card3));
                Assert.That(move.Moves[1].GetType(), Is.EqualTo(typeof(CardMoveFromColumn)));
                Assert.That(((CardMoveFromColumn)(move.Moves[1])).ToColumn, Is.EqualTo(1));
                Assert.That(((CardMoveFromColumn)(move.Moves[1])).FromColumn, Is.EqualTo(2));
            });
        }

        [Test]
        public void TestMoveKingFromHand()
        {
            var card1 = new Card(Colors.Spades, Numbers.King);
            var card2 = new Card(Colors.Spades, Numbers.Six);
            var card3 = new Card(Colors.Hearts, Numbers.Queen);
            var card4 = new Card(Colors.Clubs, Numbers.Jack);

            var hand = new Hand([card1]);
            var column1 = new Column(1, [], -1);
            var column2 = new Column(2, [card2, card3, card4], 1);

            var t = new Table(hand, null, [column1, column2], null);

            var ret = ImmutableArray.CreateBuilder<CardMoves>();

            Game.Game.MoveKingFromColumnOrHand(t, ret);

            Assert.That(ret, Has.Count.EqualTo(1));
            var move = ret[0];
            Assert.Multiple(() =>
            {
                Assert.That(move.Moves, Has.Length.EqualTo(3));
                Assert.That(move.Moves[0].Card, Is.EqualTo(card1));
                Assert.That(move.Moves[0].GetType(), Is.EqualTo(typeof(CardMoveFromHand)));
                Assert.That(((CardMoveFromHand)(move.Moves[0])).ToColumn, Is.EqualTo(1));
                Assert.That(move.Moves[1].Card, Is.EqualTo(card3));
                Assert.That(move.Moves[1].GetType(), Is.EqualTo(typeof(CardMoveFromColumn)));
                Assert.That(((CardMoveFromColumn)(move.Moves[1])).ToColumn, Is.EqualTo(1));
                Assert.That(((CardMoveFromColumn)(move.Moves[1])).FromColumn, Is.EqualTo(2));
                Assert.That(move.Moves[2].Card, Is.EqualTo(card4));
                Assert.That(move.Moves[2].GetType(), Is.EqualTo(typeof(CardMoveFromColumn)));
                Assert.That(((CardMoveFromColumn)(move.Moves[2])).ToColumn, Is.EqualTo(1));
                Assert.That(((CardMoveFromColumn)(move.Moves[2])).FromColumn, Is.EqualTo(2));
            });
        }

        [Test]
        public void MoveCardfromHandToTop()
        {
            var card1 = new Card(Colors.Spades, Numbers.Ace);
            var card2 = new Card(Colors.Spades, Numbers.Two);
            var hand = new Hand([card2]);
            var top = new Top(Colors.Spades, [card1]);
            var t = new Table(hand, [top], [], null);

            var ret = ImmutableArray.CreateBuilder<CardMoves>();

            Game.Game.MoveCardfromHandAndColumnToTop(t, ret);

            Assert.That(ret, Has.Count.EqualTo(1));
            var move = ret[0];
            Assert.Multiple(() =>
            {
                Assert.That(move.Moves, Has.Length.EqualTo(1));
                Assert.That(move.Moves[0].Card, Is.EqualTo(card2));
                Assert.That(move.Moves[0].GetType(), Is.EqualTo(typeof(CardMoveToTopFromHand)));
            });
        }

        [Test]
        public void MoveCardfromColumnToTop()
        {
            var card1 = new Card(Colors.Spades, Numbers.Ace);
            var card2 = new Card(Colors.Spades, Numbers.Two);
            var card3 = new Card(Colors.Diamonds, Numbers.Eight);
            var top = new Top(Colors.Spades, [card1]);
            var column = new Column(1, [card3, card2], 1);
            var t = new Table(new Hand(null), [top], [column], null);

            var ret = ImmutableArray.CreateBuilder<CardMoves>();

            Game.Game.MoveCardfromHandAndColumnToTop(t, ret);

            Assert.That(ret, Has.Count.EqualTo(1));
            var move = ret[0];
            Assert.Multiple(() =>
            {
                Assert.That(move.Moves, Has.Length.EqualTo(1));
                Assert.That(move.Moves[0].Card, Is.EqualTo(card2));
                Assert.That(move.Moves[0].GetType(), Is.EqualTo(typeof(CardMoveToTopFromColumn)));
                Assert.That(((CardMoveToTopFromColumn)(move.Moves[0])).FromColumn, Is.EqualTo(1));
            });

        }
    }
}
