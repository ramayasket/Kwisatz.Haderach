using System;

namespace Kw.Common.DunamicResources
{
	/// <summary>
	/// Аллокатор ресурса для динамического пула.
	/// </summary>
	/// <typeparam name="T">Тип управляемого ресурса.</typeparam>
	public abstract class DynamicResourceAllocator<T> : IDisposable where T : class, IDisposable
	{
		/// <summary>
		/// Выделяет потоку экземпляр ресурса при входе в блок using.
		/// </summary>
		protected DynamicResourceAllocator()
		{
			DynamicResourcePool<T>.AllocateResource(this);
		}

		public virtual bool VerifyResource(T resource)
		{
			return true;
		}

		/// <summary>
		/// Создает экземпляр ресурса.
		/// </summary>
		/// <returns>Ссылка на ресурс.</returns>
		public abstract T CreateInstance();

		/// <summary>
		/// Освобождает экземпляр ресурса при выходе из блока using.
		/// </summary>
		public void Dispose()
		{
			DynamicResourcePool<T>.FreeResource();
		}
	}
}
