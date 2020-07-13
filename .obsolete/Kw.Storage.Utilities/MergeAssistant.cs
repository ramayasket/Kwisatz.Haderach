using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kw.Common;

namespace Kw.Storage.Utilities
{
	public static class MergeAssistant
	{
		private const string TEMPLATE = @"
merge [{1}] as target
using
(
	select {2} from @{0}
) as source
(
	{2}
)
on ({3})

";
		private const string TEMPLATE_I = @"
		
when not matched then
	insert ({2}) values ({4})
";
		private const string TEMPLATE_U = @"
when matched then
	update set {5}
";
		public static string CreateStatement(string source, string target, ColumnSet columns, bool insert = true, bool update = true)
		{
			if(!insert && !update)
				throw new IncorrectOperationException("Either insert or update or both must be true.");

			var names = columns.SelectFormat("[{0}]");
			var keying = columns.SelectFormat("target.[{0}] = source.[{0}]", " and ", true);
			var sources = columns.SelectFormat("source.[{0}]");
			var updates = columns.SelectFormat("target.[{0}] = source.[{0}]", ", ", false);

			var template = TEMPLATE;

			if(insert)
			{
				template += TEMPLATE_I;
			}

			if(update)
			{
				template += TEMPLATE_U;
			}

			template += Environment.NewLine + ";";

			var mergeSql = String.Format(template, source, target, names, keying, sources, updates);

			return mergeSql;
		}
	}
}

