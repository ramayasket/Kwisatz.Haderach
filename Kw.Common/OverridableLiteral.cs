using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kw.Common
{
	/// <summary>
	/// Переопределяемый литерал.
	/// ReSharper disable InconsistentNaming
	/// </summary>
	public class OverridableLiteral
	{
		protected readonly string _literal;
		protected string _override;

		public OverridableLiteral(string literal)
		{
			_literal = literal;
		}

		public static implicit operator string(OverridableLiteral that)
		{
			return that.Value;
		}

		public virtual string Value => _override ?? _literal;

		public void Override(string to)
		{
			_override = to;
		}

		public override string ToString()
		{
			return Value;
		}
	}
}

