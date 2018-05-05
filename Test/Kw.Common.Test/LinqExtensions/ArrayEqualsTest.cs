using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Kw.Common.Test.LinqExtensions
{
	[TestClass]
	public class ArrayEqualsTest
	{
		[TestMethod]
		public void Equality()
		{
			var eq = new[] {0, 1}.ArrayEquals(0, 1);
			Assert.IsTrue(eq);
		}

		[TestMethod]
		public void EqualityEmpty()
		{
			var eq = new int[0].ArrayEquals();
			Assert.IsTrue(eq);
		}

		[TestMethod]
		public void NonEqualityValue()
		{
			var eq = new[] { 0, 1 }.ArrayEquals(0, 2);
			Assert.IsFalse(eq);
		}

		[TestMethod]
		public void NonEqualityLength()
		{
			var eq = new[] { 0, 1 }.ArrayEquals(0);
			Assert.IsFalse(eq);
		}

		[TestMethod]
		public void NonEqualityEmpty()
		{
			var eq = new[] { 0, 1 }.ArrayEquals(0);
			Assert.IsFalse(eq);
		}
	}
}
