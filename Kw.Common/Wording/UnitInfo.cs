namespace Kw.Common.Wording
{
    /// <summary> Информация о валюте или единице измерения. </summary>
    internal struct UnitInfo
    {
        /// <summary> Род. </summary>
        public NounGender Gender;

        /// <summary> Обозначение единицы целой части. </summary>
        public string MajorOne;

        /// <summary> Обозначение от 2 до 4 единиц целой части. </summary>
        public string MajorTwo;

        /// <summary> Обозначение 5 и более единиц целой части. </summary>
        public string MajorFive;

        /// <summary> Обозначение единицы дробной части. </summary>
        public string MinorOne;

        /// <summary> Обозначение от 2 до 4 единиц дробной части. </summary>
        public string MinorTwo;

        /// <summary> Обозначение 5 и более единиц дробной части. </summary>
        public string MinorFive;

        /// <summary> Точность. </summary>
        public short Precision;
    }
}
