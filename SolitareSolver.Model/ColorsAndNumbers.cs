using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolitareSolver.Model
{
    public enum Colors
    {
        Hearts, Diamonds, Spades, Clubs
    }

    public enum Numbers
    {
        Ace, Two, Three, Four, Five, Six, Seven, Eight, Nine, Ten, Jack, Queen, King
    }

    public static class HelperData
    {
        private static readonly string[] ColorTexts = ["H", "D", "S", "C"];
        private static readonly string[] NumberTexts = ["A", "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K"];
        public static string GetText(this Colors color)
        {
            return ColorTexts[(int)color];
        }
        public static string GetText(this Numbers number)
        {
            return NumberTexts[(int)number];
        }

        public static bool CheckColors(Colors color1, Colors color2)
        {
            return ((color1 == Colors.Hearts || color1==Colors.Diamonds) && (color2 == Colors.Spades || color2 == Colors.Clubs)) ||
                ((color1 == Colors.Spades || color1==Colors.Clubs) && (color2 == Colors.Hearts || color2 == Colors.Diamonds));
        }
    }
}
