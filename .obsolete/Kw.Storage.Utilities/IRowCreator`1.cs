using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Kw.Storage.Utilities
{
	public interface IRowCreator<out T>
	{
		T CreateInstance(IDataRecord source);
	}
}

