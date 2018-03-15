using System;
using System.Globalization;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Kw.Common.Test
{
	[TestClass]
	public class CultureIssues
	{
		[TestMethod]
		public void GetRussianCulture()
		{
			var ruRU = Literals.RussianCulture;	//	ru-RU

			Assert.AreEqual("ru-RU", ruRU.Name);
			//
			Assert.AreEqual("ru-RU", ruRU.IetfLanguageTag);
			Assert.IsFalse(ruRU.IsNeutralCulture);
			Assert.AreEqual(0x419, ruRU.KeyboardLayoutId);
		}
	}
}
