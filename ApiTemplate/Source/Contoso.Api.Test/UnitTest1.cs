using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Contoso.Api.Test
{
    [TestClass]
    public class UnitTest1
    {
        #region Variables

        private const string APP_ID = "APP_ID";
        private const string APP_SECRET = "APP_SECRET";

        private Client _client;

        #endregion

        #region Constructors

        public UnitTest1()
        {
            _client = new Client(APP_ID, APP_SECRET);
        }

        #endregion

        #region Methods

        [TestMethod]
        public void TestMethod1()
        {
        }

        #endregion
    }
}
