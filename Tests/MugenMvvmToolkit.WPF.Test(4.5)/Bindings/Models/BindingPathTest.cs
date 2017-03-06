#region Copyright

// ****************************************************************************
// <copyright file="BindingPathTest.cs">
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

using System.Linq;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Binding.Models;
using Should;

namespace MugenMvvmToolkit.Test.Bindings.Models
{
    [TestClass]
    public class BindingPathTest
    {
        #region Methods

        [TestMethod]
        public void BindingPathShouldParseSingleProperty()
        {
            const string path = "Test";
            IBindingPath bindingPath = GetBindingPath(path);
            bindingPath.Parts.Single().ShouldEqual(path);
            bindingPath.Path.ShouldEqual(path);
        }

        [TestMethod]
        public void BindingPathShouldParseSingleIndexerProperty()
        {
            const string path = "[test]";
            IBindingPath bindingPath = GetBindingPath(path);
            bindingPath.Parts.Single().ShouldEqual(path);
            bindingPath.Path.ShouldEqual(path);
        }

        [TestMethod]
        public void BindingPathShouldParseMultiProperty()
        {
            var strings = new[] {"Test", "Test1", "Test2", "Test3"};
            string path = string.Join(".", strings);
            IBindingPath bindingPath = GetBindingPath(path);
            bindingPath.Parts.SequenceEqual(strings).ShouldBeTrue();
            bindingPath.Path.ShouldEqual(path);
        }

        [TestMethod]
        public void BindingPathShouldParseMultiPropertyWithIndexer()
        {
            var strings = new[] {"[test]", "Test1", "Test2[test]", "Test3"};
            var items = new[] {"[test]", "Test1", "Test2", "[test]", "Test3"};
            string path = string.Join(".", strings);
            IBindingPath bindingPath = GetBindingPath(path);
            bindingPath.Parts.SequenceEqual(items).ShouldBeTrue();
            bindingPath.Path.ShouldEqual(path);
        }

        [TestMethod]
        public void BindingPathShouldRemoveWhitespaces()
        {
            var strings = new[] {"[test, test2] ", "Test1 ", "Test2  [test , 10] ", "Test3 "};
            var items = new[] {"[test, test2]", "Test1", "Test2", "[test , 10]", "Test3"};
            string path = string.Join(".", strings);
            IBindingPath bindingPath = GetBindingPath(path);
            bindingPath.Parts.SequenceEqual(items).ShouldBeTrue();
        }

        [TestMethod]
        public void BindingPathShouldDetectSinglePath()
        {
            const string path = "Test";
            IBindingPath bindingPath = GetBindingPath(path);
            bindingPath.Parts.Single().ShouldEqual(path);
            bindingPath.Path.ShouldEqual(path);
            bindingPath.IsSingle.ShouldBeTrue();
        }

        [TestMethod]
        public void BindingPathShouldDetectEmptyPath()
        {
            string path = string.Empty;
            IBindingPath bindingPath = GetBindingPath(path);
            bindingPath.Path.ShouldEqual(path);
            bindingPath.IsEmpty.ShouldBeTrue();
        }

        [TestMethod]
        public void BindingPathShouldDetectMultiPath()
        {
            const string path = "Test.Test";
            IBindingPath bindingPath = GetBindingPath(path);
            bindingPath.Path.ShouldEqual(path);
            bindingPath.IsEmpty.ShouldBeFalse();
            bindingPath.IsSingle.ShouldBeFalse();
        }

        protected virtual IBindingPath GetBindingPath(string path)
        {
            return new BindingPath(path);
        }

        #endregion
    }
}
