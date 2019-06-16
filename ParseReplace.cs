		static string GenerateJson(string format)
		{
			const string GUID = "<Guid>";
			const string DATETIME = "<DateTime>";

			int offset = 0, cut = 0;
			string output = "";

			var time = new DateTime(2017, 1, 1);

			while ((offset = format.IndexOf('<', offset)) != -1)
			{
				output += format.Substring(cut, offset - cut);
				cut = offset;

				if (format.Substring(offset).StartsWith(GUID))
				{
					offset += GUID.Length;
					cut += GUID.Length;
					output += Guid.NewGuid().ToString().ToLowerInvariant();
				}
				else if (format.Substring(offset).StartsWith(DATETIME))
				{
					offset += DATETIME.Length;
					cut += DATETIME.Length;
					output += time.ToShortDateString().ToLowerInvariant();
					time += TimeSpan.FromDays(1);
				}
			}

			output += format.Substring(cut);

			return output;
		}
