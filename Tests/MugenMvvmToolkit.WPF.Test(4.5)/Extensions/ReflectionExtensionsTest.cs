using System;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MugenMvvmToolkit.Models;
using Should;

namespace MugenMvvmToolkit.Test.Extensions
{
    [TestClass]
    public class ReflectionExtensionsTest : TestBase
    {
        #region Nested types

        public class ReflectionBaseClass
        {
            public static readonly object PublicFieldSt;
            protected static readonly object ProtectedFieldSt;
            private static readonly object _privateFieldSt;

            public readonly object PublicField;
            protected readonly object ProtectedField;
            private readonly object _privateField;

            public object PublicProperty { get; set; }
            protected object ProtectedProperty { get; set; }
            private object PrivateProperty { get; set; }

            public static object PublicPropertySt { get; set; }
            protected static object ProtectedPropertySt { get; set; }
            private static object PrivatePropertySt { get; set; }

            public event EventHandler PublicEvent;
            protected event EventHandler ProtectedEvent;
            private event EventHandler PrivateEvent;

            public static event EventHandler PublicEventSt;
            protected static event EventHandler ProtectedEventSt;
            private static event EventHandler PrivateEventSt;

            public void PublicMethod()
            {
            }

            protected void ProtectedMethod()
            {
            }

            private void PrivateMethod()
            {
            }

            public static void PublicMethodSt()
            {
            }

            protected static void ProtectedMethodSt()
            {
            }

            private static void PrivateMethodSt()
            {
            }


            public void ArgMethod(object arg)
            {
            }

            public void ArgMethod(string arg)
            {
            }
        }

        public class ReflectionClass : ReflectionBaseClass
        {
            public ReflectionClass(object arg)
            {
            }

            public ReflectionClass(string arg)
            {
            }
        }

        #endregion

        #region Methods

        [TestMethod]
        public void TestGetPublicField()
        {
            typeof(ReflectionClass)
                .GetFieldEx("PublicField", MemberFlags.Public | MemberFlags.Instance)
                .ShouldNotBeNull();
            typeof(ReflectionClass)
                .GetFieldEx("PublicField", MemberFlags.Public | MemberFlags.Instance | MemberFlags.Static | MemberFlags.NonPublic)
                .ShouldNotBeNull();
            typeof(ReflectionClass)
                .GetFieldEx("PublicField", MemberFlags.NonPublic | MemberFlags.Instance)
                .ShouldBeNull();
            typeof(ReflectionClass)
                .GetFieldEx("PublicField", MemberFlags.Public | MemberFlags.Static)
                .ShouldBeNull();
            typeof(ReflectionClass)
                .GetFieldEx("PublicField", MemberFlags.NonPublic | MemberFlags.Static)
                .ShouldBeNull();
        }

        [TestMethod]
        public void TestGetPublicFieldStatic()
        {
            typeof(ReflectionClass)
                .GetFieldEx("PublicFieldSt", MemberFlags.Public | MemberFlags.Instance)
                .ShouldBeNull();
            typeof(ReflectionClass)
                .GetFieldEx("PublicFieldSt", MemberFlags.Public | MemberFlags.Instance | MemberFlags.Static | MemberFlags.NonPublic)
                .ShouldNotBeNull();
            typeof(ReflectionClass)
                .GetFieldEx("PublicFieldSt", MemberFlags.NonPublic | MemberFlags.Instance)
                .ShouldBeNull();
            typeof(ReflectionClass)
                .GetFieldEx("PublicFieldSt", MemberFlags.Public | MemberFlags.Static)
                .ShouldNotBeNull();
            typeof(ReflectionClass)
                .GetFieldEx("PublicFieldSt", MemberFlags.NonPublic | MemberFlags.Static)
                .ShouldBeNull();
        }

        [TestMethod]
        public void TestGetProtectedField()
        {
            typeof(ReflectionClass)
                .GetFieldEx("ProtectedField", MemberFlags.Public | MemberFlags.Instance)
                .ShouldBeNull();
            typeof(ReflectionClass)
                .GetFieldEx("ProtectedField", MemberFlags.Public | MemberFlags.Instance | MemberFlags.Static | MemberFlags.NonPublic)
                .ShouldNotBeNull();
            typeof(ReflectionClass)
                .GetFieldEx("ProtectedField", MemberFlags.NonPublic | MemberFlags.Instance)
                .ShouldNotBeNull();
            typeof(ReflectionClass)
                .GetFieldEx("ProtectedField", MemberFlags.Public | MemberFlags.Static)
                .ShouldBeNull();
            typeof(ReflectionClass)
                .GetFieldEx("ProtectedField", MemberFlags.NonPublic | MemberFlags.Static)
                .ShouldBeNull();
        }

        [TestMethod]
        public void TestGetProtectedFieldStatic()
        {
            typeof(ReflectionClass)
                .GetFieldEx("ProtectedFieldSt", MemberFlags.Public | MemberFlags.Instance)
                .ShouldBeNull();
            typeof(ReflectionClass)
                .GetFieldEx("ProtectedFieldSt", MemberFlags.Public | MemberFlags.Instance | MemberFlags.Static | MemberFlags.NonPublic)
                .ShouldNotBeNull();
            typeof(ReflectionClass)
                .GetFieldEx("ProtectedFieldSt", MemberFlags.NonPublic | MemberFlags.Instance)
                .ShouldBeNull();
            typeof(ReflectionClass)
                .GetFieldEx("ProtectedFieldSt", MemberFlags.Public | MemberFlags.Static)
                .ShouldBeNull();
            typeof(ReflectionClass)
                .GetFieldEx("ProtectedFieldSt", MemberFlags.NonPublic | MemberFlags.Static)
                .ShouldNotBeNull();
        }

        [TestMethod]
        public void TestGetPrivateField()
        {
            typeof(ReflectionClass)
                .GetFieldEx("_privateField", MemberFlags.Public | MemberFlags.Instance)
                .ShouldBeNull();
            typeof(ReflectionClass)
                .GetFieldEx("_privateField", MemberFlags.Public | MemberFlags.Instance | MemberFlags.Static | MemberFlags.NonPublic)
                .ShouldNotBeNull();
            typeof(ReflectionClass)
                .GetFieldEx("_privateField", MemberFlags.NonPublic | MemberFlags.Instance)
                .ShouldNotBeNull();
            typeof(ReflectionClass)
                .GetFieldEx("_privateField", MemberFlags.Public | MemberFlags.Static)
                .ShouldBeNull();
            typeof(ReflectionClass)
                .GetFieldEx("_privateField", MemberFlags.NonPublic | MemberFlags.Static)
                .ShouldBeNull();
        }

        [TestMethod]
        public void TestGetPrivateFieldStatic()
        {
            typeof(ReflectionClass)
                .GetFieldEx("_privateFieldSt", MemberFlags.Public | MemberFlags.Instance)
                .ShouldBeNull();
            typeof(ReflectionClass)
                .GetFieldEx("_privateFieldSt", MemberFlags.Public | MemberFlags.Instance | MemberFlags.Static | MemberFlags.NonPublic)
                .ShouldNotBeNull();
            typeof(ReflectionClass)
                .GetFieldEx("_privateFieldSt", MemberFlags.NonPublic | MemberFlags.Instance)
                .ShouldBeNull();
            typeof(ReflectionClass)
                .GetFieldEx("_privateFieldSt", MemberFlags.Public | MemberFlags.Static)
                .ShouldBeNull();
            typeof(ReflectionClass)
                .GetFieldEx("_privateFieldSt", MemberFlags.NonPublic | MemberFlags.Static)
                .ShouldNotBeNull();
        }

        [TestMethod]
        public void TestGetPublicProperty()
        {
            typeof(ReflectionClass)
                .GetPropertyEx("PublicProperty", MemberFlags.Public | MemberFlags.Instance)
                .ShouldNotBeNull();
            typeof(ReflectionClass)
                .GetPropertyEx("PublicProperty", MemberFlags.Public | MemberFlags.Instance | MemberFlags.Static | MemberFlags.NonPublic)
                .ShouldNotBeNull();
            typeof(ReflectionClass)
                .GetPropertyEx("PublicProperty", MemberFlags.NonPublic | MemberFlags.Instance)
                .ShouldBeNull();
            typeof(ReflectionClass)
                .GetPropertyEx("PublicProperty", MemberFlags.Public | MemberFlags.Static)
                .ShouldBeNull();
            typeof(ReflectionClass)
                .GetPropertyEx("PublicProperty", MemberFlags.NonPublic | MemberFlags.Static)
                .ShouldBeNull();
        }

        [TestMethod]
        public void TestGetPublicPropertyStatic()
        {
            typeof(ReflectionClass)
                .GetPropertyEx("PublicPropertySt", MemberFlags.Public | MemberFlags.Instance)
                .ShouldBeNull();
            typeof(ReflectionClass)
                .GetPropertyEx("PublicPropertySt", MemberFlags.Public | MemberFlags.Instance | MemberFlags.Static | MemberFlags.NonPublic)
                .ShouldNotBeNull();
            typeof(ReflectionClass)
                .GetPropertyEx("PublicPropertySt", MemberFlags.NonPublic | MemberFlags.Instance)
                .ShouldBeNull();
            typeof(ReflectionClass)
                .GetPropertyEx("PublicPropertySt", MemberFlags.Public | MemberFlags.Static)
                .ShouldNotBeNull();
            typeof(ReflectionClass)
                .GetPropertyEx("PublicPropertySt", MemberFlags.NonPublic | MemberFlags.Static)
                .ShouldBeNull();
        }

        [TestMethod]
        public void TestGetProtectedProperty()
        {
            typeof(ReflectionClass)
                .GetPropertyEx("ProtectedProperty", MemberFlags.Public | MemberFlags.Instance)
                .ShouldBeNull();
            typeof(ReflectionClass)
                .GetPropertyEx("ProtectedProperty", MemberFlags.Public | MemberFlags.Instance | MemberFlags.Static | MemberFlags.NonPublic)
                .ShouldNotBeNull();
            typeof(ReflectionClass)
                .GetPropertyEx("ProtectedProperty", MemberFlags.NonPublic | MemberFlags.Instance)
                .ShouldNotBeNull();
            typeof(ReflectionClass)
                .GetPropertyEx("ProtectedProperty", MemberFlags.Public | MemberFlags.Static)
                .ShouldBeNull();
            typeof(ReflectionClass)
                .GetPropertyEx("ProtectedProperty", MemberFlags.NonPublic | MemberFlags.Static)
                .ShouldBeNull();
        }

        [TestMethod]
        public void TestGetProtectedPropertyStatic()
        {
            typeof(ReflectionClass)
                .GetPropertyEx("ProtectedPropertySt", MemberFlags.Public | MemberFlags.Instance)
                .ShouldBeNull();
            typeof(ReflectionClass)
                .GetPropertyEx("ProtectedPropertySt", MemberFlags.Public | MemberFlags.Instance | MemberFlags.Static | MemberFlags.NonPublic)
                .ShouldNotBeNull();
            typeof(ReflectionClass)
                .GetPropertyEx("ProtectedPropertySt", MemberFlags.NonPublic | MemberFlags.Instance)
                .ShouldBeNull();
            typeof(ReflectionClass)
                .GetPropertyEx("ProtectedPropertySt", MemberFlags.Public | MemberFlags.Static)
                .ShouldBeNull();
            typeof(ReflectionClass)
                .GetPropertyEx("ProtectedPropertySt", MemberFlags.NonPublic | MemberFlags.Static)
                .ShouldNotBeNull();
        }

        [TestMethod]
        public void TestGetPrivateProperty()
        {
            typeof(ReflectionClass)
                .GetPropertyEx("PrivateProperty", MemberFlags.Public | MemberFlags.Instance)
                .ShouldBeNull();
            typeof(ReflectionClass)
                .GetPropertyEx("PrivateProperty", MemberFlags.Public | MemberFlags.Instance | MemberFlags.Static | MemberFlags.NonPublic)
                .ShouldNotBeNull();
            typeof(ReflectionClass)
                .GetPropertyEx("PrivateProperty", MemberFlags.NonPublic | MemberFlags.Instance)
                .ShouldNotBeNull();
            typeof(ReflectionClass)
                .GetPropertyEx("PrivateProperty", MemberFlags.Public | MemberFlags.Static)
                .ShouldBeNull();
            typeof(ReflectionClass)
                .GetPropertyEx("PrivateProperty", MemberFlags.NonPublic | MemberFlags.Static)
                .ShouldBeNull();
        }

        [TestMethod]
        public void TestGetPrivatePropertyStatic()
        {
            typeof(ReflectionClass)
                .GetPropertyEx("PrivatePropertySt", MemberFlags.Public | MemberFlags.Instance)
                .ShouldBeNull();
            typeof(ReflectionClass)
                .GetPropertyEx("PrivatePropertySt", MemberFlags.Public | MemberFlags.Instance | MemberFlags.Static | MemberFlags.NonPublic)
                .ShouldNotBeNull();
            typeof(ReflectionClass)
                .GetPropertyEx("PrivatePropertySt", MemberFlags.NonPublic | MemberFlags.Instance)
                .ShouldBeNull();
            typeof(ReflectionClass)
                .GetPropertyEx("PrivatePropertySt", MemberFlags.Public | MemberFlags.Static)
                .ShouldBeNull();
            typeof(ReflectionClass)
                .GetPropertyEx("PrivatePropertySt", MemberFlags.NonPublic | MemberFlags.Static)
                .ShouldNotBeNull();
        }

        [TestMethod]
        public void TestGetPublicEvent()
        {
            typeof(ReflectionClass)
                .GetEventEx("PublicEvent", MemberFlags.Public | MemberFlags.Instance)
                .ShouldNotBeNull();
            typeof(ReflectionClass)
                .GetEventEx("PublicEvent", MemberFlags.Public | MemberFlags.Instance | MemberFlags.Static | MemberFlags.NonPublic)
                .ShouldNotBeNull();
            typeof(ReflectionClass)
                .GetEventEx("PublicEvent", MemberFlags.NonPublic | MemberFlags.Instance)
                .ShouldBeNull();
            typeof(ReflectionClass)
                .GetEventEx("PublicEvent", MemberFlags.Public | MemberFlags.Static)
                .ShouldBeNull();
            typeof(ReflectionClass)
                .GetEventEx("PublicEvent", MemberFlags.NonPublic | MemberFlags.Static)
                .ShouldBeNull();
        }

        [TestMethod]
        public void TestGetPublicEventStatic()
        {
            typeof(ReflectionClass)
                .GetEventEx("PublicEventSt", MemberFlags.Public | MemberFlags.Instance)
                .ShouldBeNull();
            typeof(ReflectionClass)
                .GetEventEx("PublicEventSt", MemberFlags.Public | MemberFlags.Instance | MemberFlags.Static | MemberFlags.NonPublic)
                .ShouldNotBeNull();
            typeof(ReflectionClass)
                .GetEventEx("PublicEventSt", MemberFlags.NonPublic | MemberFlags.Instance)
                .ShouldBeNull();
            typeof(ReflectionClass)
                .GetEventEx("PublicEventSt", MemberFlags.Public | MemberFlags.Static)
                .ShouldNotBeNull();
            typeof(ReflectionClass)
                .GetEventEx("PublicEventSt", MemberFlags.NonPublic | MemberFlags.Static)
                .ShouldBeNull();
        }

        [TestMethod]
        public void TestGetProtectedEvent()
        {
            typeof(ReflectionClass)
                .GetEventEx("ProtectedEvent", MemberFlags.Public | MemberFlags.Instance)
                .ShouldBeNull();
            typeof(ReflectionClass)
                .GetEventEx("ProtectedEvent", MemberFlags.Public | MemberFlags.Instance | MemberFlags.Static | MemberFlags.NonPublic)
                .ShouldNotBeNull();
            typeof(ReflectionClass)
                .GetEventEx("ProtectedEvent", MemberFlags.NonPublic | MemberFlags.Instance)
                .ShouldNotBeNull();
            typeof(ReflectionClass)
                .GetEventEx("ProtectedEvent", MemberFlags.Public | MemberFlags.Static)
                .ShouldBeNull();
            typeof(ReflectionClass)
                .GetEventEx("ProtectedEvent", MemberFlags.NonPublic | MemberFlags.Static)
                .ShouldBeNull();
        }

        [TestMethod]
        public void TestGetProtectedEventStatic()
        {
            typeof(ReflectionClass)
                .GetEventEx("ProtectedEventSt", MemberFlags.Public | MemberFlags.Instance)
                .ShouldBeNull();
            typeof(ReflectionClass)
                .GetEventEx("ProtectedEventSt", MemberFlags.Public | MemberFlags.Instance | MemberFlags.Static | MemberFlags.NonPublic)
                .ShouldNotBeNull();
            typeof(ReflectionClass)
                .GetEventEx("ProtectedEventSt", MemberFlags.NonPublic | MemberFlags.Instance)
                .ShouldBeNull();
            typeof(ReflectionClass)
                .GetEventEx("ProtectedEventSt", MemberFlags.Public | MemberFlags.Static)
                .ShouldBeNull();
            typeof(ReflectionClass)
                .GetEventEx("ProtectedEventSt", MemberFlags.NonPublic | MemberFlags.Static)
                .ShouldNotBeNull();
        }

        [TestMethod]
        public void TestGetPrivateEvent()
        {
            typeof(ReflectionClass)
                .GetEventEx("PrivateEvent", MemberFlags.Public | MemberFlags.Instance)
                .ShouldBeNull();
            typeof(ReflectionClass)
                .GetEventEx("PrivateEvent", MemberFlags.Public | MemberFlags.Instance | MemberFlags.Static | MemberFlags.NonPublic)
                .ShouldNotBeNull();
            typeof(ReflectionClass)
                .GetEventEx("PrivateEvent", MemberFlags.NonPublic | MemberFlags.Instance)
                .ShouldNotBeNull();
            typeof(ReflectionClass)
                .GetEventEx("PrivateEvent", MemberFlags.Public | MemberFlags.Static)
                .ShouldBeNull();
            typeof(ReflectionClass)
                .GetEventEx("PrivateEvent", MemberFlags.NonPublic | MemberFlags.Static)
                .ShouldBeNull();
        }

        [TestMethod]
        public void TestGetPrivateEventStatic()
        {
            typeof(ReflectionClass)
                .GetEventEx("PrivateEventSt", MemberFlags.Public | MemberFlags.Instance)
                .ShouldBeNull();
            typeof(ReflectionClass)
                .GetEventEx("PrivateEventSt", MemberFlags.Public | MemberFlags.Instance | MemberFlags.Static | MemberFlags.NonPublic)
                .ShouldNotBeNull();
            typeof(ReflectionClass)
                .GetEventEx("PrivateEventSt", MemberFlags.NonPublic | MemberFlags.Instance)
                .ShouldBeNull();
            typeof(ReflectionClass)
                .GetEventEx("PrivateEventSt", MemberFlags.Public | MemberFlags.Static)
                .ShouldBeNull();
            typeof(ReflectionClass)
                .GetEventEx("PrivateEventSt", MemberFlags.NonPublic | MemberFlags.Static)
                .ShouldNotBeNull();
        }

        [TestMethod]
        public void TestGetPublicMethod()
        {
            typeof(ReflectionClass)
                .GetMethodEx("PublicMethod", MemberFlags.Public | MemberFlags.Instance)
                .ShouldNotBeNull();
            typeof(ReflectionClass)
                .GetMethodEx("PublicMethod", MemberFlags.Public | MemberFlags.Instance | MemberFlags.Static | MemberFlags.NonPublic)
                .ShouldNotBeNull();
            typeof(ReflectionClass)
                .GetMethodEx("PublicMethod", MemberFlags.NonPublic | MemberFlags.Instance)
                .ShouldBeNull();
            typeof(ReflectionClass)
                .GetMethodEx("PublicMethod", MemberFlags.Public | MemberFlags.Static)
                .ShouldBeNull();
            typeof(ReflectionClass)
                .GetMethodEx("PublicMethod", MemberFlags.NonPublic | MemberFlags.Static)
                .ShouldBeNull();
        }

        [TestMethod]
        public void TestGetPublicMethodStatic()
        {
            typeof(ReflectionClass)
                .GetMethodEx("PublicMethodSt", MemberFlags.Public | MemberFlags.Instance)
                .ShouldBeNull();
            typeof(ReflectionClass)
                .GetMethodEx("PublicMethodSt", MemberFlags.Public | MemberFlags.Instance | MemberFlags.Static | MemberFlags.NonPublic)
                .ShouldNotBeNull();
            typeof(ReflectionClass)
                .GetMethodEx("PublicMethodSt", MemberFlags.NonPublic | MemberFlags.Instance)
                .ShouldBeNull();
            typeof(ReflectionClass)
                .GetMethodEx("PublicMethodSt", MemberFlags.Public | MemberFlags.Static)
                .ShouldNotBeNull();
            typeof(ReflectionClass)
                .GetMethodEx("PublicMethodSt", MemberFlags.NonPublic | MemberFlags.Static)
                .ShouldBeNull();
        }

        [TestMethod]
        public void TestGetProtectedMethod()
        {
            typeof(ReflectionClass)
                .GetMethodEx("ProtectedMethod", MemberFlags.Public | MemberFlags.Instance)
                .ShouldBeNull();
            typeof(ReflectionClass)
                .GetMethodEx("ProtectedMethod", MemberFlags.Public | MemberFlags.Instance | MemberFlags.Static | MemberFlags.NonPublic)
                .ShouldNotBeNull();
            typeof(ReflectionClass)
                .GetMethodEx("ProtectedMethod", MemberFlags.NonPublic | MemberFlags.Instance)
                .ShouldNotBeNull();
            typeof(ReflectionClass)
                .GetMethodEx("ProtectedMethod", MemberFlags.Public | MemberFlags.Static)
                .ShouldBeNull();
            typeof(ReflectionClass)
                .GetMethodEx("ProtectedMethod", MemberFlags.NonPublic | MemberFlags.Static)
                .ShouldBeNull();
        }

        [TestMethod]
        public void TestGetProtectedMethodStatic()
        {
            typeof(ReflectionClass)
                .GetMethodEx("ProtectedMethodSt", MemberFlags.Public | MemberFlags.Instance)
                .ShouldBeNull();
            typeof(ReflectionClass)
                .GetMethodEx("ProtectedMethodSt", MemberFlags.Public | MemberFlags.Instance | MemberFlags.Static | MemberFlags.NonPublic)
                .ShouldNotBeNull();
            typeof(ReflectionClass)
                .GetMethodEx("ProtectedMethodSt", MemberFlags.NonPublic | MemberFlags.Instance)
                .ShouldBeNull();
            typeof(ReflectionClass)
                .GetMethodEx("ProtectedMethodSt", MemberFlags.Public | MemberFlags.Static)
                .ShouldBeNull();
            typeof(ReflectionClass)
                .GetMethodEx("ProtectedMethodSt", MemberFlags.NonPublic | MemberFlags.Static)
                .ShouldNotBeNull();
        }

        [TestMethod]
        public void TestGetPrivatedMethod()
        {
            typeof(ReflectionClass)
                .GetMethodEx("PrivateMethod", MemberFlags.Public | MemberFlags.Instance)
                .ShouldBeNull();
            typeof(ReflectionClass)
                .GetMethodEx("PrivateMethod", MemberFlags.Public | MemberFlags.Instance | MemberFlags.Static | MemberFlags.NonPublic)
                .ShouldNotBeNull();
            typeof(ReflectionClass)
                .GetMethodEx("PrivateMethod", MemberFlags.NonPublic | MemberFlags.Instance)
                .ShouldNotBeNull();
            typeof(ReflectionClass)
                .GetMethodEx("PrivateMethod", MemberFlags.Public | MemberFlags.Static)
                .ShouldBeNull();
            typeof(ReflectionClass)
                .GetMethodEx("PrivateMethod", MemberFlags.NonPublic | MemberFlags.Static)
                .ShouldBeNull();
        }

        [TestMethod]
        public void TestGetPrivatedMethodStatic()
        {
            typeof(ReflectionClass)
                .GetMethodEx("PrivateMethodSt", MemberFlags.Public | MemberFlags.Instance)
                .ShouldBeNull();
            typeof(ReflectionClass)
                .GetMethodEx("PrivateMethodSt", MemberFlags.Public | MemberFlags.Instance | MemberFlags.Static | MemberFlags.NonPublic)
                .ShouldNotBeNull();
            typeof(ReflectionClass)
                .GetMethodEx("PrivateMethodSt", MemberFlags.NonPublic | MemberFlags.Instance)
                .ShouldBeNull();
            typeof(ReflectionClass)
                .GetMethodEx("PrivateMethodSt", MemberFlags.Public | MemberFlags.Static)
                .ShouldBeNull();
            typeof(ReflectionClass)
                .GetMethodEx("PrivateMethodSt", MemberFlags.NonPublic | MemberFlags.Static)
                .ShouldNotBeNull();
        }

        [TestMethod]
        public void TestGetMethodWithArgs()
        {
            typeof(ReflectionClass)
                .GetMethodEx("ArgMethod", new[] { typeof(object) }, MemberFlags.Public | MemberFlags.Instance)
                .GetParameters()[0]
                .ParameterType
                .ShouldEqual(typeof(object));

            typeof(ReflectionClass)
                .GetMethodEx("ArgMethod", new[] { typeof(string) }, MemberFlags.Public | MemberFlags.Instance)
                .GetParameters()[0]
                .ParameterType
                .ShouldEqual(typeof(string));
        }

#if !SILVERLIGHT
        [TestMethod]
        public void TestGetConstructorWithArgs()
        {
            ReflectionExtensions
                .GetConstructor(typeof(ReflectionClass), new[] { typeof(object) })
                .GetParameters()[0]
                .ParameterType
                .ShouldEqual(typeof(object));

            ReflectionExtensions
                .GetConstructor(typeof(ReflectionClass), new[] { typeof(string) })
                .GetParameters()[0]
                .ParameterType
                .ShouldEqual(typeof(string));
        }
#endif
        #endregion
    }
}
