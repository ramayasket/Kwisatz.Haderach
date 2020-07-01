using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Kw.Common.Test.Singleton
{
    [TestClass]
    public class Singletoned
    {
        private class Sc1 { }
        private class Sc2 { }
        private class Sc3 { }

        [TestMethod]
        public void UponCreation1()
        {
            Assert.IsNull(Singleton<Sc1>.Instance);
        }

        [TestMethod]
        public void UponCreation2()
        {
            Assert.IsNull(Singleton<Sc2>.Instance);

            Singleton<Sc2>.Instance = new Sc2();
            Assert.IsNotNull(Singleton<Sc2>.Instance);
        }

        [TestMethod]
        [ExpectedException(typeof(IncorrectOperationException))]
        public void UponCreation3()
        {
            Assert.IsNull(Singleton<Sc3>.Instance);

            Singleton<Sc3>.Instance = new Sc3();
            Assert.IsNotNull(Singleton<Sc3>.Instance);

            Singleton<Sc3>.Instance = new Sc3();
            Assert.IsNotNull(Singleton<Sc3>.Instance);
        }
    }
}
