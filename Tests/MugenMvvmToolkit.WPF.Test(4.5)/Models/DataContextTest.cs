#region Copyright

// ****************************************************************************
// <copyright file="DataContextTest.cs">
// Copyright (c) 2012-2017 Vyacheslav Volkov
// </copyright>
// ****************************************************************************
// <author>Vyacheslav Volkov</author>
// <email>vvs0205@outlook.com</email>
// <project>MugenMvvmToolkit</project>
// <web>https://github.com/MugenMvvmToolkit/MugenMvvmToolkit</web>
// <license>
// See license.txt in this solution or http://opensource.org/licenses/MS-PL
// </license>
// ****************************************************************************

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MugenMvvmToolkit.DataConstants;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Models;
using Should;

namespace MugenMvvmToolkit.Test.Models
{
    [TestClass]
    public class DataContextTest : TestBase
    {
        #region Methods

        [TestMethod]
        public void AddGetTryGetTest()
        {
            const string st = "test";
            IDataContext context = Create();
            context.Add(InitializationConstants.ObservationMode, ObservationMode.Both);
            context.Count.ShouldEqual(1);

            context.Add(InitializationConstants.ViewModelBindingName, st);
            context.Count.ShouldEqual(2);

            ObservationMode data;
            context.TryGetData(InitializationConstants.ObservationMode, out data).ShouldBeTrue();
            data.ShouldEqual(ObservationMode.Both);
            context.GetData(InitializationConstants.ViewModelBindingName).ShouldEqual(st);
        }

        [TestMethod]
        public void ContainsRemoveTest()
        {
            IDataContext context = Create();
            context.Add(InitializationConstants.ObservationMode, ObservationMode.Both);
            context.Contains(InitializationConstants.ObservationMode).ShouldBeTrue();

            context.Remove(InitializationConstants.ObservationMode).ShouldBeTrue();
            context.Contains(InitializationConstants.ObservationMode).ShouldBeFalse();
            context.Remove(InitializationConstants.ObservationMode).ShouldBeFalse();
        }

        [TestMethod]
        public void EnumerableTest()
        {
            const string st = "test";
            IDataContext context = Create();
            context.Add(InitializationConstants.ObservationMode, ObservationMode.Both);
            context.Count.ShouldEqual(1);

            context.Add(InitializationConstants.ViewModelBindingName, st);
            context.Count.ShouldEqual(2);

            var dictionary = context.ToList().ToDictionary(constantValue => constantValue.DataConstant, value => value.Value);
            dictionary.Count.ShouldEqual(2);
            dictionary[InitializationConstants.ObservationMode].ShouldEqual(ObservationMode.Both);
            dictionary[InitializationConstants.ViewModelBindingName].ShouldEqual(st);
        }

        [TestMethod]
        public void AddNotNullValueShouldThrowException()
        {
            var dataConstant = new DataConstant<object>("id", true);
            var dataContext = Create();
            ShouldThrow(() => dataContext.Add(dataConstant, null));
        }

        protected virtual IDataContext Create()
        {
            return new DataContext();
        }

        #endregion
    }

    [TestClass]
    public class DataContextSerializationTest : SerializationTestBase<DataContext>
    {
        #region Overrides of SerializationTestBase<DataContext>

        [Ignore]
        public override void TestXmlSerialization()
        {
            base.TestXmlSerialization();
        }

        protected override DataContext GetObject()
        {
            return new DataContext
            {
                {InitializationConstants.ObservationMode, ObservationMode.Both},
                {InitializationConstants.ViewName, "Test"}
            };
        }

        protected override void AssertObject(DataContext deserializedObj)
        {
            deserializedObj.Count.ShouldEqual(2);
            deserializedObj.GetDataTest(InitializationConstants.ObservationMode).ShouldEqual(ObservationMode.Both);
            deserializedObj.GetData(InitializationConstants.ViewName).ShouldEqual("Test");
        }

        protected override IEnumerable<Type> GetKnownTypes()
        {
            return new Type[] { typeof(DataConstant), typeof(string), typeof(ObservationMode) };
        }

        #endregion
    }
}
