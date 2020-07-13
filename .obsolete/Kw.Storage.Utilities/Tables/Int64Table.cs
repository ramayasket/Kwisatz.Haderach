using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace Kw.Storage.Utilities.Tables
{
	public class Int64Table : SqlStructured<long>
	{
		public Int64Table(string name, IEnumerable<long> rows) : base(name, rows)
		{
		}

		protected override void SetColumns(DataColumnCollection columns)
		{
			columns.Add("Value", typeof (long));
		}

		protected override object[] WrapRow(long row)
		{
			return new object[] { row };
		}

		public override bool IsDynamic
		{
			get { return false; }
		}

		protected override string TypeName
		{
			get { return "Int64"; }
		}

		//public override SqlParameter Introduce(SqlConnection conn)
		//{
		//    return Parameter;
		//}
	}

	public class ChowInt64Table : Int64Table
	{
		public ChowInt64Table(string name, IEnumerable<long> rows)
			: base(name, rows)
		{
		}

		protected override string TypeName
		{
			get { return "[FY_TableValued_int64]"; }
		}
	}
}

