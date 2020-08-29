using System;
using System.Collections.Generic;
using System.Text;

namespace Kw.Common.Wording
{
    /// <summary> Русские письменные числа. </summary>
    internal class RusNumber
    {
        private static readonly string[] Hunds =
        {
            string.Empty, "сто ", "двести ", "триста ", "четыреста ",
            "пятьсот ", "шестьсот ", "семьсот ", "восемьсот ", "девятьсот ",
        };

        private static readonly string[] Tens =
        {
            string.Empty, "десять ", "двадцать ", "тридцать ", "сорок ", "пятьдесят ",
            "шестьдесят ", "семьдесят ", "восемьдесят ", "девяносто ",
        };

        private static readonly string[] Numerals20 =
        {
            "три ", "четыре ", "пять ", "шесть ",
            "семь ", "восемь ", "девять ", "десять ", "одиннадцать ",
            "двенадцать ", "тринадцать ", "четырнадцать ", "пятнадцать ",
            "шестнадцать ", "семнадцать ", "восемнадцать ", "девятнадцать ",
        };

        private static readonly Dictionary<NounGender, string[]> DependentGenderNumerals = new Dictionary<NounGender, string[]>
        {
            { NounGender.Masculine, new[] { string.Empty, "один ", "два " } },
            { NounGender.Feminine,  new[] { string.Empty, "одна ", "две " } },
            { NounGender.Neuter,    new[] { string.Empty, "одно ", "два " } },
        };

        /// <summary> Строковое представление числа. </summary>
        /// <param name="val"> Значение числа. </param>
        /// <param name="gender"> Склонение по родам. </param>
        /// <param name="one"> Строка для обозначение единицы в разряде (например: миллион). </param>
        /// <param name="two"> Строка для обозначение 2,3,4 в разряде (например: миллиарда). </param>
        /// <param name="five"> Строка для обозначение 5 и выше в разряде (например: триллионов). </param>
        /// <returns> Строковое представление. </returns>
        internal static string Str(long val, NounGender gender, string one, string two, string five)
        {
            long num = val % 1000;
            if (num == 0)
                return string.Empty;

            if (num < 0)
                throw new ArgumentOutOfRangeException(nameof(val), "Параметр не может быть отрицательным");

            var list = new List<string>(DependentGenderNumerals[gender]);
            list.AddRange(Numerals20);
            string[] frac20 = list.ToArray();

            var r = new StringBuilder(Hunds[num / 100]);

            if (num % 100 < 20)
            {
                r.Append(frac20[num % 100]);
            }
            else
            {
                r.Append(Tens[num % 100 / 10]);
                r.Append(frac20[num % 10]);
            }

            r.Append(Case(num, one, two, five));

            if (r.Length != 0)
                r.Append(" ");
            return r.ToString();
        }

        internal static string Case(long val, string one, string two, string five)
        {
            long temp = (val % 100 > 20) ? val % 10 : val % 20;

            switch (temp)
            {
                case 1:
                    return one;
                case 2:
                case 3:
                case 4:
                    return two;
                default:
                    return five;
            }
        }
    }
}
