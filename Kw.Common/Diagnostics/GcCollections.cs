#pragma warning disable CA1051 // Do not declare visible instance fields
#pragma warning disable CA2225 // Operator overloads have named alternates

using System;

namespace Kw.Common.Diagnostics
{
    /// <summary> Структура для хранения числа сборок мусора в разных поколениях. </summary>
    public readonly struct GcCollections : IEquatable<GcCollections>, IComparable<GcCollections>
    {
        /// <summary> Структура задаёт случай, когда сборка мусора не происходила. </summary>
        public static readonly GcCollections Empty;

        /// <summary> Число сборок мусора в нулевом поколении. </summary>
        public readonly int Gen0;

        /// <summary> Число сборок мусора в первом поколении. </summary>
        public readonly int Gen1;

        /// <summary> Число сборок мусора во втором поколении. </summary>
        public readonly int Gen2;

        /// <summary> Инициализирует новый экземпляр структуры <see cref="GcCollections"/>, заполняя структуру всеми значениями. </summary>
        /// <param name="gen0"> Число сборок мусора в нулевом поколении. </param>
        /// <param name="gen1"> Число сборок мусора в первом поколении. </param>
        /// <param name="gen2"> Число сборок мусора во втором поколении. </param>
        public GcCollections(int gen0, int gen1, int gen2)
        {
            Gen0 = gen0;
            Gen1 = gen1;
            Gen2 = gen2;
        }

        /// <summary> Получить текущее состояние сборок мусора. </summary>
        public static GcCollections Current => new GcCollections(GC.CollectionCount(0), GC.CollectionCount(1), GC.CollectionCount(2));

        /// <summary> Определяет операцию сложения для двух структур. </summary>
        /// <param name="a"> Первое слагаемое. </param>
        /// <param name="b"> Второе слагаемое. </param>
        /// <returns> Сумма сборок мусора по поколениям. </returns>
        public static GcCollections operator +(in GcCollections a, in GcCollections b) => new GcCollections(a.Gen0 + b.Gen0, a.Gen1 + b.Gen1, a.Gen2 + b.Gen2);

        /// <summary> Определяет операцию вычитания для двух структур. </summary>
        /// <param name="a"> Уменьшаемое. </param>
        /// <param name="b"> Вычитаемое. </param>
        /// <returns> Разность сборок мусора по поколениям. </returns>
        public static GcCollections operator -(in GcCollections a, in GcCollections b) => new GcCollections(a.Gen0 - b.Gen0, a.Gen1 - b.Gen1, a.Gen2 - b.Gen2);

        /// <summary> Определяет операцию равенства двух структур. </summary>
        /// <param name="a"> Первая структура. </param>
        /// <param name="b"> Вторая структура. </param>
        /// <returns> true, если две структуры равны, иначе false. </returns>
        public static bool operator ==(in GcCollections a, in GcCollections b) => a.Gen0 == b.Gen0 && a.Gen1 == b.Gen1 && a.Gen2 == b.Gen2;

        /// <summary> Определяет операцию неравенства двух структур. </summary>
        /// <param name="a"> Первая структура. </param>
        /// <param name="b"> Вторая структура. </param>
        /// <returns> true, если две структуры неравны, иначе false. </returns>
        public static bool operator !=(in GcCollections a, in GcCollections b) => a.Gen0 != b.Gen0 || a.Gen1 != b.Gen1 || a.Gen2 != b.Gen2;

        /// <summary> Определяет операцию сравнения двух структур. </summary>
        /// <param name="a"> Первая структура. </param>
        /// <param name="b"> Вторая структура. </param>
        /// <returns> true, если первая структура меньше или равна второй, иначе false. </returns>
        public static bool operator <=(in GcCollections a, in GcCollections b) => a.Gen0 <= b.Gen0 && a.Gen1 <= b.Gen1 && a.Gen2 <= b.Gen2;

        /// <summary> Определяет операцию сравнения двух структур. </summary>
        /// <param name="a"> Первая структура. </param>
        /// <param name="b"> Вторая структура. </param>
        /// <returns> true, если первая структура больше или равна второй, иначе false. </returns>
        public static bool operator >=(in GcCollections a, in GcCollections b) => a.Gen0 >= b.Gen0 && a.Gen1 >= b.Gen1 && a.Gen2 >= b.Gen2;

        /// <summary> Определяет операцию сравнения двух структур. </summary>
        /// <param name="a"> Первая структура. </param>
        /// <param name="b"> Вторая структура. </param>
        /// <returns> true, если первая структура меньше второй, иначе false. </returns>
        public static bool operator <(in GcCollections a, in GcCollections b) => a.Gen0 < b.Gen0 && a.Gen1 < b.Gen1 && a.Gen2 < b.Gen2;

        /// <summary> Определяет операцию сравнения двух структур. </summary>
        /// <param name="a"> Первая структура. </param>
        /// <param name="b"> Вторая структура. </param>
        /// <returns> true, если первая структура больше второй равны, иначе false. </returns>
        public static bool operator >(in GcCollections a, in GcCollections b) => a.Gen0 > b.Gen0 && a.Gen1 > b.Gen1 && a.Gen2 > b.Gen2;

        /// <summary> Определяет эквивалентность с другим объектом. </summary>
        /// <param name="obj"> Сравниваемый объект. </param>
        /// <returns> true, если эквивалентны, иначе false. </returns>
        public override bool Equals(object obj) => obj is GcCollections gc ? this == gc : base.Equals(obj);

        /// <summary> Определяет эквивалентность с другой структурой. </summary>
        /// <param name="other"> Сравниваемая структура. </param>
        /// <returns> true, если эквивалентны, иначе false. </returns>
        public bool Equals(GcCollections other) => this == other;

        /// <summary> Получает хеш-код. </summary>
        /// <returns> Хеш-код. </returns>
        public override int GetHashCode() => (Gen0, Gen1, Gen2).GetHashCode();

        /// <summary> Сравнивает две структуры по количеству сборок мусора. Более высокие поколения имеют приоритет (они важнее). </summary>
        /// <param name="other"> Сравниваемая структура. </param>
        /// <returns> Результат сравнения. </returns>
        public int CompareTo(GcCollections other)
        {
            int test = Gen2 - other.Gen2;
            if (test != 0) return test;

            test = Gen1 - other.Gen1;
            return test != 0 ? test : Gen0 - other.Gen0;
        }

        /// <summary> Получить строковое представление структуры. </summary>
        /// <returns> Строка. </returns>
        public override string ToString() => $"Gen 2/1/0:    {Gen2}/{Gen1}/{Gen0}";
    }
}
