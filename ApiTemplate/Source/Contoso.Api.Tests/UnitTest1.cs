﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Contoso.Api.Models;
using System.Threading;

namespace Contoso.Api.Tests
{
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
            _client = new Client(APP_ID, APP_SECRET, null);
        }

        #endregion

        #region Methods

        [TestMethod]
        public void TestAuthentication()
        {
            UserResponse response = _client.AuthenticateAsync("username", "password", CancellationToken.None).Result;

        }

        [TestMethod]
        public void TestMethod1()
        {
        }

        #endregion
    }
}
