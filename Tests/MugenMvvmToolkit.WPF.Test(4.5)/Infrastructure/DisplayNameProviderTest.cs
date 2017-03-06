#region Copyright

// ****************************************************************************
// <copyright file="DisplayNameProviderTest.cs">
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
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MugenMvvmToolkit.Attributes;
using MugenMvvmToolkit.Infrastructure;
using MugenMvvmToolkit.Interfaces;
using Should;

namespace MugenMvvmToolkit.Test.Infrastructure
{
    [TestClass]
    public class DisplayNameProviderTest
    {
        #region Fields

        private const string Name = "test";

        #endregion

        #region Nested types

        public sealed class DefaultClass
        {
            public string Field;

            public string Property { get; set; }

            public void Method()
            {
            }

            public event Action Event;
        }

        [DisplayName(Name)]
        public sealed class ClassWithDisplayNameAttributeToolkit
        {
            [DisplayName(Name)]
            public string Field;

            [DisplayName(Name)]
            public string Property { get; set; }

            [DisplayName(Name)]
            public void Method()
            {
            }

            [DisplayName(Name)]
            public event Action Event;
        }

        [System.ComponentModel.DisplayName(Name)]
        public sealed class ClassWithDisplayNameAttributeSystem
        {
            [System.ComponentModel.DisplayName(Name)]
            public string Property { get; set; }

            [System.ComponentModel.DisplayName(Name)]
            public void Method()
            {
            }

            [System.ComponentModel.DisplayName(Name)]
            public event Action Event;
        }

        [Attributes.MetadataType(typeof(ToolkitMeta))]
        public sealed class ClassWithDisplayNameAttributeToolkitMeta
        {
            public string Field;

            public string Property { get; set; }

            public void Method()
            {
            }

            public event Action Event;
        }

        [System.ComponentModel.DataAnnotations.MetadataType(typeof(SystemMeta))]
        public sealed class ClassWithDisplayNameAttributeSystemMeta
        {
            public string Property { get; set; }

            public void Method()
            {
            }

            public event Action Event;
        }

        [DisplayName(Name)]
        public sealed class ToolkitMeta
        {
            [DisplayName(Name)]
            public string Field;

            [DisplayName(Name)]
            public string Property { get; set; }

            [DisplayName(Name)]
            public void Method()
            {
            }

            [DisplayName(Name)]
            public event Action Event;
        }

        [System.ComponentModel.DisplayName(Name)]
        public sealed class SystemMeta
        {
            [System.ComponentModel.DisplayName(Name)]
            public string Property { get; set; }

            [System.ComponentModel.DisplayName(Name)]
            public void Method()
            {
            }

            [System.ComponentModel.DisplayName(Name)]
            public event Action Event;
        }

        public sealed class DisplayAttributeClass
        {
            [Display(Name = Name)]
            public string Field;

            [Display(Name = Name)]
            public string Property { get; set; }

            [Display(Name = Name)]
            public void Method()
            {
            }
        }

        [Attributes.MetadataType("GetMetaTypes")]
        public sealed class DisplayAttributeClassMugenMeta
        {
            public string Field;

            public string Property { get; set; }

            public void Method()
            {
            }

            private static IEnumerable<Type> GetMetaTypes()
            {
                return new[] { typeof(DisplayAttributeClass) };
            }
        }

        [System.ComponentModel.DataAnnotations.MetadataType(typeof(DisplayAttributeClass))]
        public sealed class DisplayAttributeClassSystemMeta
        {
            public string Field;

            public string Property { get; set; }

            public void Method()
            {
            }
        }

        #endregion

        #region Methods

        [TestMethod]
        public void ProviderShouldReturnMemberNameByDefault()
        {
            var displayNameProvider = GetDisplayNameProvider();
#if NETFX_CORE
            MemberInfo member = typeof(DefaultClass).GetTypeInfo();
#else
            MemberInfo member = typeof(DefaultClass);
#endif
            var displayNameAccessor = displayNameProvider.GetDisplayNameAccessor(member);
            displayNameAccessor().ShouldEqual(string.Empty);

            member = GetFields(typeof(DefaultClass))[0];
            displayNameAccessor = displayNameProvider.GetDisplayNameAccessor(member);
            displayNameAccessor().ShouldEqual(member.Name);

            member = GetProperties(typeof(DefaultClass))[0];
            displayNameAccessor = displayNameProvider.GetDisplayNameAccessor(member);
            displayNameAccessor().ShouldEqual(member.Name);

            member = GetEvents(typeof(DefaultClass))[0];
            displayNameAccessor = displayNameProvider.GetDisplayNameAccessor(member);
            displayNameAccessor().ShouldEqual(member.Name);

            member = GetMethods(typeof(DefaultClass)).First(info => !info.IsSpecialName);
            displayNameAccessor = displayNameProvider.GetDisplayNameAccessor(member);
            displayNameAccessor().ShouldEqual(member.Name);
        }

        [TestMethod]
        public void ProviderShouldGetNameFromToolkitDisplayNameAttribute()
        {
            Type target = typeof(ClassWithDisplayNameAttributeToolkit);
            var displayNameProvider = GetDisplayNameProvider();
#if NETFX_CORE
            MemberInfo member = target.GetTypeInfo();
#else
            MemberInfo member = target;
#endif
            var displayNameAccessor = displayNameProvider.GetDisplayNameAccessor(member);
            displayNameAccessor().ShouldEqual(Name);

            member = GetFields(target)[0];
            displayNameAccessor = displayNameProvider.GetDisplayNameAccessor(member);
            displayNameAccessor().ShouldEqual(Name);

            member = GetProperties(target)[0];
            displayNameAccessor = displayNameProvider.GetDisplayNameAccessor(member);
            displayNameAccessor().ShouldEqual(Name);

            member = GetEvents(target)[0];
            displayNameAccessor = displayNameProvider.GetDisplayNameAccessor(member);
            displayNameAccessor().ShouldEqual(Name);

            member = GetMethods(target).First(info => !info.IsSpecialName);
            displayNameAccessor = displayNameProvider.GetDisplayNameAccessor(member);
            displayNameAccessor().ShouldEqual(Name);
        }

        [TestMethod]
        public void ProviderShouldGetNameFromSystemDisplayNameAttribute()
        {
            Type target = typeof(ClassWithDisplayNameAttributeSystem);
            var displayNameProvider = GetDisplayNameProvider();
#if NETFX_CORE
            MemberInfo member = target.GetTypeInfo();
#else
            MemberInfo member = target;
#endif
            var displayNameAccessor = displayNameProvider.GetDisplayNameAccessor(member);
            displayNameAccessor().ShouldEqual(Name);

            member = GetProperties(target)[0];
            displayNameAccessor = displayNameProvider.GetDisplayNameAccessor(member);
            displayNameAccessor().ShouldEqual(Name);

            member = GetEvents(target)[0];
            displayNameAccessor = displayNameProvider.GetDisplayNameAccessor(member);
            displayNameAccessor().ShouldEqual(Name);

            member = GetMethods(target).First(info => !info.IsSpecialName);
            displayNameAccessor = displayNameProvider.GetDisplayNameAccessor(member);
            displayNameAccessor().ShouldEqual(Name);
        }

        [TestMethod]
        public void ProviderShouldGetNameFromToolkitDisplayNameAttributeMeta()
        {
            Type target = typeof(ClassWithDisplayNameAttributeToolkitMeta);
            var displayNameProvider = GetDisplayNameProvider();
#if NETFX_CORE
            MemberInfo member = target.GetTypeInfo();
#else
            MemberInfo member = target;
#endif
            var displayNameAccessor = displayNameProvider.GetDisplayNameAccessor(member);
            displayNameAccessor().ShouldEqual(Name);

            member = GetFields(target)[0];
            displayNameAccessor = displayNameProvider.GetDisplayNameAccessor(member);
            displayNameAccessor().ShouldEqual(Name);

            member = GetProperties(target)[0];
            displayNameAccessor = displayNameProvider.GetDisplayNameAccessor(member);
            displayNameAccessor().ShouldEqual(Name);

            member = GetEvents(target)[0];
            displayNameAccessor = displayNameProvider.GetDisplayNameAccessor(member);
            displayNameAccessor().ShouldEqual(Name);

            member = GetMethods(target).First(info => !info.IsSpecialName);
            displayNameAccessor = displayNameProvider.GetDisplayNameAccessor(member);
            displayNameAccessor().ShouldEqual(Name);
        }

        [TestMethod]
        public void ProviderShouldGetNameFromSystemDisplayNameAttributeMeta()
        {
            Type target = typeof(ClassWithDisplayNameAttributeSystemMeta);
            var displayNameProvider = GetDisplayNameProvider();
#if NETFX_CORE
            MemberInfo member = target.GetTypeInfo();
#else
            MemberInfo member = target;
#endif
            var displayNameAccessor = displayNameProvider.GetDisplayNameAccessor(member);
            displayNameAccessor().ShouldEqual(Name);

            member = GetProperties(target)[0];
            displayNameAccessor = displayNameProvider.GetDisplayNameAccessor(member);
            displayNameAccessor().ShouldEqual(Name);

            member = GetEvents(target)[0];
            displayNameAccessor = displayNameProvider.GetDisplayNameAccessor(member);
            displayNameAccessor().ShouldEqual(Name);

            member = GetMethods(target).First(info => !info.IsSpecialName);
            displayNameAccessor = displayNameProvider.GetDisplayNameAccessor(member);
            displayNameAccessor().ShouldEqual(Name);
        }

        [TestMethod]
        public void ProviderShouldGetNameFromDisplayAttribute()
        {
            Type target = typeof(DisplayAttributeClass);
            var displayNameProvider = GetDisplayNameProvider();

            MemberInfo member = GetFields(target)[0];
            var displayNameAccessor = displayNameProvider.GetDisplayNameAccessor(member);
            displayNameAccessor().ShouldEqual(Name);

            member = GetProperties(target)[0];
            displayNameAccessor = displayNameProvider.GetDisplayNameAccessor(member);
            displayNameAccessor().ShouldEqual(Name);

            member = GetMethods(target).First(info => !info.IsSpecialName);
            displayNameAccessor = displayNameProvider.GetDisplayNameAccessor(member);
            displayNameAccessor().ShouldEqual(Name);
        }

        [TestMethod]
        public void ProviderShouldGetNameFromDisplayAttributeMugenMeta()
        {
            Type target = typeof(DisplayAttributeClassMugenMeta);
            var displayNameProvider = GetDisplayNameProvider();

            MemberInfo member = GetFields(target)[0];
            var displayNameAccessor = displayNameProvider.GetDisplayNameAccessor(member);
            displayNameAccessor().ShouldEqual(Name);

            member = GetProperties(target)[0];
            displayNameAccessor = displayNameProvider.GetDisplayNameAccessor(member);
            displayNameAccessor().ShouldEqual(Name);

            member = GetMethods(target).First(info => !info.IsSpecialName);
            displayNameAccessor = displayNameProvider.GetDisplayNameAccessor(member);
            displayNameAccessor().ShouldEqual(Name);
        }

        [TestMethod]
        public void ProviderShouldGetNameFromDisplayAttributeSystemMeta()
        {
            Type target = typeof(DisplayAttributeClassSystemMeta);
            var displayNameProvider = GetDisplayNameProvider();

            MemberInfo member = GetFields(target)[0];
            var displayNameAccessor = displayNameProvider.GetDisplayNameAccessor(member);
            displayNameAccessor().ShouldEqual(Name);

            member = GetProperties(target)[0];
            displayNameAccessor = displayNameProvider.GetDisplayNameAccessor(member);
            displayNameAccessor().ShouldEqual(Name);

            member = GetMethods(target).First(info => !info.IsSpecialName);
            displayNameAccessor = displayNameProvider.GetDisplayNameAccessor(member);
            displayNameAccessor().ShouldEqual(Name);
        }

        protected virtual IDisplayNameProvider GetDisplayNameProvider()
        {
            return new DisplayNameProvider();
        }

        private static FieldInfo[] GetFields(Type type)
        {
#if NETFX_CORE
            return TypeExtensions.GetFields(type);
#else
            return type.GetFields();
#endif
        }

        private static PropertyInfo[] GetProperties(Type type)
        {
#if NETFX_CORE
            return TypeExtensions.GetProperties(type);
#else
            return type.GetProperties();
#endif
        }

        private static EventInfo[] GetEvents(Type type)
        {
#if NETFX_CORE
            return TypeExtensions.GetEvents(type);
#else
            return type.GetEvents();
#endif
        }

        private static MethodInfo[] GetMethods(Type type)
        {
#if NETFX_CORE
            return TypeExtensions.GetMethods(type);
#else
            return type.GetMethods();
#endif
        }

        #endregion
    }
}
