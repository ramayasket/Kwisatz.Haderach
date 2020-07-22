using System;

namespace Kw.Aspects
{
    /// <summary>
    /// Интерфейс основного объекта по отношению к подмешанному.
    /// </summary>
    public interface IAdmixee
    {
        /// <summary>
        /// Возвращает ссылку на подмешанный объект.
        /// </summary>
        object Admixed { get; }
    }

    /// <summary>
    /// Интерфейс подмешанного объекта.
    /// </summary>
    public interface IAdmixed
    {
        /// <summary>
        /// Возвращает слабую ссылку на основной объект.
        /// </summary>
        WeakReference Admixee { get; set; }
    }
}
