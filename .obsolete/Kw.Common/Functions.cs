using System;
using System.Diagnostics;

/* ReSharper disable once CheckNamespace */
namespace Kw.Common
{
    /// <summary>
    /// Формирует цепочку описания функции на основе <see cref="FunctionNode{TA,TV}"/>.
    /// </summary>
    public static class Function
    {
        /// <summary>
        /// Создает цепочку описания функции.
        /// </summary>
        /// <typeparam name="TA">Тип аргумента.</typeparam>
        /// <typeparam name="TV">Тип функции.</typeparam>
        /// <param name="argument">Аргумент.</param>
        /// <param name="value">Значение.</param>
        /// <returns>Элемент цепочки.</returns>
        public static FunctionNode<TA, TV> On<TA, TV>(TA argument, TV value)
        {
            return new FunctionNode<TA, TV>(argument, value);
        }

        /// <summary>
        /// Продолжает цепочку описания функции и вычисляет ее.
        /// </summary>
        /// <typeparam name="TA">Тип аргумента.</typeparam>
        /// <typeparam name="TV">Тип функции.</typeparam>
        /// <param name="previous">Предыдущий элемент цепочки.</param>
        /// <param name="argument">Аргумент.</param>
        /// <param name="value">Значение.</param>
        /// <returns>Элемент цепочки.</returns>
        public static FunctionNode<TA, TV> On<TA, TV>(this FunctionNode<TA, TV> previous, TA argument, TV value)
        {
            return new FunctionNode<TA, TV>(previous, argument, value);
        }

        /// <summary>
        /// Завершает цепочку описания функции.
        /// </summary>
        /// <typeparam name="TA">Тип аргумента.</typeparam>
        /// <typeparam name="TV">Тип функции.</typeparam>
        /// <param name="previous">Предыдущий элемент цепочки.</param>
        /// <param name="value">Значение по умолчанию.</param>
        /// <returns>Вычисленное значение.</returns>
        public static FunctionNode<TA, TV> Otherwise<TA, TV>(this FunctionNode<TA, TV> previous, TV value)
        {
            return new FunctionNode<TA, TV>(previous, value);
        }

        /// <summary>
        /// Завершает цепочку описания функции и вычисляет ее.
        /// </summary>
        /// <typeparam name="TA">Тип аргумента.</typeparam>
        /// <typeparam name="TV">Тип функции.</typeparam>
        /// <param name="previous">Предыдущий элемент цепочки.</param>
        /// <param name="value">Значение по умолчанию.</param>
        /// <param name="argument">Аргумент функции.</param>
        /// <returns>Вычисленное значение.</returns>
        public static TV Otherwise<TA, TV>(this FunctionNode<TA, TV> previous, TV value, TA argument)
        {
            var last = new FunctionNode<TA, TV>(previous, value);

            try
            {
                return last.ComputeFunction(argument);
            }
            catch    //    handled
            {
                return value;
            }
        }

        /// <summary>
        /// Вычисляет описанную цепочкой функцию.
        /// </summary>
        /// <typeparam name="TA">Тип аргумента.</typeparam>
        /// <typeparam name="TV">Тип функции.</typeparam>
        /// <param name="last">Последний элемент цепочки.</param>
        /// <param name="argument">Аргумент функции.</param>
        /// <returns>Вычисленное значение.</returns>
        public static TV Compute<TA, TV>(this FunctionNode<TA, TV> last, TA argument)
        {
            if (last == null)
                throw new ArgumentNullException("last");

            return last.ComputeFunction(argument);
        }
    }

    /// <summary>
    /// Элемент цепочки описания функции.
    /// </summary>
    /// <typeparam name="TA">Тип аргумента.</typeparam>
    /// <typeparam name="TV">Тип значения.</typeparam>
    [DebuggerDisplay("{Display(), nq}")]
    public sealed class FunctionNode<TA, TV>
    {
        private readonly TA Argument;
        private readonly TV Value;

        private readonly FunctionNode<TA, TV> Previous;
        private FunctionNode<TA, TV> Next;

        private readonly bool Constant;
        private bool Terminating;

        internal FunctionNode(TA argument, TV value)
        {
            Argument = argument;
            Value = value;
        }

        internal FunctionNode(FunctionNode<TA, TV> previous, TA argument, TV value)
        {
            if (null == previous)
                throw new ArgumentNullException("previous");

            Argument = argument;
            Value = value;

            Previous = previous;
            previous.Next = this;
        }

        internal FunctionNode(FunctionNode<TA, TV> previous, TV value)
        {
            if (null == previous)
                throw new ArgumentNullException("previous");

            if (previous.Terminating)
                throw new ArgumentException("Function is already terminated.");

            Value = value;

            Previous = previous;
            previous.Next = this;

            Terminating = true;
            Constant = true;
        }

        internal FunctionNode<TA, TV> Root
        {
            get
            {
                return null != Previous ? Previous.Root : this;
            }
        }

        internal TV ComputeFunction(TA argument)
        {
            Terminating = true;
            return Root.ComputeNode(argument);
        }

        private TV ComputeNode(TA argument)
        {
            if (Equals(Argument, argument) || Constant)
                return Value;

            if (null != Next)
                return Next.ComputeNode(argument);

            if (Terminating)
                throw new ArgumentOutOfRangeException("argument", argument, "Reached end of function.");

            return Value;
        }

        /* ReSharper disable once UnusedMember.Local */
        private string Display()
        {
            return string.Format("{0}/{1}", Argument.AsDebuggerDisplay(), Value.AsDebuggerDisplay());
        }
    }
}

