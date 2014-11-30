using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.Binding.Infrastructure;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Binding.Models;
using MugenMvvmToolkit.Test.TestInfrastructure;
using MugenMvvmToolkit.Test.TestModels;
using Should;

namespace MugenMvvmToolkit.Test.Bindings.Infrastructure
{
    [TestClass]
    public class BindingMemberProviderTest : BindingTestBase
    {
        #region Nested types

        private interface IInterface
        {

        }

        private class BaseClass : IInterface
        {

        }

        private class Class : BaseClass
        {

        }

        #endregion

        #region Methods

        [TestMethod]
        public void ProviderShouldReturnMemberForProperty()
        {
            var model = new BindingSourceModel();
            var path = GetMemberPath(model, m => m.IntProperty);
            var provider = CreateMemberProvider();

            var member = provider.GetBindingMember(typeof(BindingSourceModel), path, true, true);
            member.CanRead.ShouldBeTrue();
            member.CanWrite.ShouldBeTrue();
            member.Type.ShouldEqual(typeof(int));
            member.MemberType.ShouldEqual(BindingMemberType.Property);
            member.Path.ShouldEqual(path);
            (member.Member is PropertyInfo).ShouldBeTrue();

            member.SetValue(model, new object[] { int.MaxValue });
            member.GetValue(model, null).ShouldEqual(int.MaxValue);
        }

        [TestMethod]
        public void ProviderShouldReturnMemberForField()
        {
            const string value = "value";
            var model = new BindingSourceModel();
            var path = GetMemberPath(model, m => m.PublicField);
            var provider = CreateMemberProvider();

            var member = provider.GetBindingMember(typeof(BindingSourceModel), path, true, true);
            member.CanRead.ShouldBeTrue();
            member.CanWrite.ShouldBeTrue();
            member.Type.ShouldEqual(typeof(string));
            member.MemberType.ShouldEqual(BindingMemberType.Field);
            member.Path.ShouldEqual(path);
            (member.Member is FieldInfo).ShouldBeTrue();

            member.SetValue(model, new object[] { value });
            member.GetValue(model, null).ShouldEqual(value);
        }

        [TestMethod]
        public void ProviderShouldReturnMemberForEvent()
        {
            const string path = BindingSourceModel.EventName;
            var provider = CreateMemberProvider();

            var member = provider.GetBindingMember(typeof(BindingSourceModel), path, true, true);
            member.CanRead.ShouldBeTrue();
            member.CanWrite.ShouldBeTrue();
            member.Type.ShouldEqual(typeof(EventHandler));
            member.MemberType.ShouldEqual(BindingMemberType.Event);
            member.Path.ShouldEqual(path);
            (member.Member is EventInfo).ShouldBeTrue();
        }

        [TestMethod]
        public void ProviderShouldReturnMemberForBindingContext()
        {
            const string path = AttachedMemberConstants.DataContext;
            var model = new BindingSourceModel();
            var contextMock = new BindingContextMock();
            BindingServiceProvider.ContextManager = new BindingContextManagerMock
            {
                GetBindingContext = o => contextMock
            };
            var provider = CreateMemberProvider();

            var member = provider.GetBindingMember(typeof(object), path, false, true);
            member.CanRead.ShouldBeTrue();
            member.CanWrite.ShouldBeTrue();
            member.Type.ShouldEqual(typeof(object));
            member.MemberType.ShouldEqual(BindingMemberType.BindingContext);
            member.Path.ShouldEqual(path);
            member.Member.ShouldBeNull();

            member.GetValue(model, null).ShouldBeNull();
            member.SetValue(model, new object[] { model });

            member.GetValue(model, null).ShouldEqual(model);
            contextMock.Value.ShouldEqual(model);
        }

        [TestMethod]
        public void ProviderShouldThrowExceptionIfMemberIsWriteOnly()
        {
            const string path = "WriteOnly";
            var model = new BindingSourceModel();
            var provider = CreateMemberProvider();

            var member = provider.GetBindingMember(typeof(BindingSourceModel), path, true, true);
            member.CanRead.ShouldBeFalse();
            ShouldThrow(() => member.GetValue(model, null));
        }

        [TestMethod]
        public void ProviderShouldThrowExceptionIfMemberNotFound()
        {
            var provider = CreateMemberProvider();
            ShouldThrow(() => provider.GetBindingMember(typeof(object), "invalid", true, true));
        }

        [TestMethod]
        public void ProviderShouldIgnoreImplicitBindingContextIfFlagIsTrue()
        {
            var model = new ExplicitDataContext();
            var path = GetMemberPath(model, context => context.DataContext);
            var provider = CreateMemberProvider();

            var member = provider.GetBindingMember(typeof(ExplicitDataContext), path, true, true);
            member.MemberType.ShouldEqual(BindingMemberType.Property);
        }

        [TestMethod]
        public void ProviderShouldUseImplicitBindingContextIfFlagIsFalse()
        {
            var model = new ExplicitDataContext();
            var path = GetMemberPath(model, context => context.DataContext);
            var provider = CreateMemberProvider();

            var member = provider.GetBindingMember(typeof(ExplicitDataContext), path, false, true);
            member.MemberType.ShouldEqual(BindingMemberType.BindingContext);
        }

        [TestMethod]
        public void ProviderShouldRegisterUnregisterAndResolveAttachedMemberSingle()
        {
            const string memberPath = "Test";
            var targetType = typeof(object);
            var memberInfo = AttachedBindingMember.CreateAutoProperty(memberPath, typeof(object));
            var provider = CreateMemberProvider();
            provider.GetBindingMember(targetType, memberPath, false, false).ShouldBeNull();
            provider.Register(targetType, memberInfo, true);
            provider.GetBindingMember(targetType, memberPath, false, false).ShouldEqual(memberInfo);

            provider.Unregister(targetType, memberPath);
            provider.GetBindingMember(targetType, memberPath, false, false).ShouldBeNull();
        }

        [TestMethod]
        public void ProviderShouldRegisterUnregisterAndResolveAttachedMemberMulti()
        {
            const int count = 10;
            const string memberPath = "Test";
            var targetType = typeof(object);
            var members = new List<IBindingMemberInfo>();
            var provider = CreateMemberProvider();
            for (int i = 0; i < count; i++)
            {
                var memberInfo = AttachedBindingMember.CreateAutoProperty(memberPath + i, typeof(object));
                provider.GetBindingMember(targetType, memberPath + i, false, false).ShouldBeNull();
                members.Add(memberInfo);
                provider.Register(targetType, memberInfo, false);
            }

            for (int i = 0; i < count; i++)
            {
                provider.GetBindingMember(targetType, memberPath + i, false, false)
                        .ShouldEqual(members[i]);
            }
            provider.Unregister(targetType);
            for (int i = 0; i < count; i++)
            {
                provider.GetBindingMember(targetType, memberPath + i, false, false)
                        .ShouldBeNull();
            }
        }

        [TestMethod]
        public void ProviderShouldSelectTheBestMember1()
        {
            const string path = "Test";
            var baseType = typeof(object);
            var parentType = typeof(BindingTestBase);
            var targetType = GetType();
            var provider = CreateMemberProvider();

            var baseMember = AttachedBindingMember.CreateMember(path, typeof(object), (info, o) => null, null);
            var parentMember = AttachedBindingMember.CreateMember(path, typeof(object), (info, o) => null, null);
            var targetMember = AttachedBindingMember.CreateMember(path, typeof(object), (info, o) => null, null);
            provider.Register(baseType, baseMember, false);
            provider.Register(parentType, parentMember, false);
            provider.Register(targetType, targetMember, false);

            provider.GetBindingMember(targetType, path, false, false).ShouldEqual(targetMember);
            provider.GetBindingMember(parentType, path, false, false).ShouldEqual(parentMember);
            provider.GetBindingMember(baseType, path, false, false).ShouldEqual(baseMember);

            provider.Unregister(targetType, path);
            provider.GetBindingMember(targetType, path, false, false).ShouldEqual(parentMember);
            provider.GetBindingMember(parentType, path, false, false).ShouldEqual(parentMember);
            provider.GetBindingMember(baseType, path, false, false).ShouldEqual(baseMember);

            provider.Unregister(parentType, path);
            provider.GetBindingMember(targetType, path, false, false).ShouldEqual(baseMember);
            provider.GetBindingMember(parentType, path, false, false).ShouldEqual(baseMember);
            provider.GetBindingMember(baseType, path, false, false).ShouldEqual(baseMember);
        }

        [TestMethod]
        public void ProviderShouldSelectTheBestMember2()
        {
            const string path = "Test";
            var baseType = typeof(IInterface);
            var parentType = typeof(BaseClass);
            var targetType = typeof(Class);
            var provider = CreateMemberProvider();

            var baseMember = AttachedBindingMember.CreateMember(path, typeof(object), (info, o) => null, null);
            var parentMember = AttachedBindingMember.CreateMember(path, typeof(object), (info, o) => null, null);
            var targetMember = AttachedBindingMember.CreateMember(path, typeof(object), (info, o) => null, null);
            provider.Register(baseType, baseMember, false);
            provider.Register(parentType, parentMember, false);
            provider.Register(targetType, targetMember, false);

            provider.GetBindingMember(targetType, path, false, false).ShouldEqual(targetMember);
            provider.GetBindingMember(parentType, path, false, false).ShouldEqual(parentMember);
            provider.GetBindingMember(baseType, path, false, false).ShouldEqual(baseMember);

            provider.Unregister(targetType, path);
            provider.GetBindingMember(targetType, path, false, false).ShouldEqual(parentMember);
            provider.GetBindingMember(parentType, path, false, false).ShouldEqual(parentMember);
            provider.GetBindingMember(baseType, path, false, false).ShouldEqual(baseMember);

            provider.Unregister(parentType, path);
            provider.GetBindingMember(targetType, path, false, false).ShouldEqual(baseMember);
            provider.GetBindingMember(parentType, path, false, false).ShouldEqual(baseMember);
            provider.GetBindingMember(baseType, path, false, false).ShouldEqual(baseMember);
        }

        [TestMethod]
        public void ProviderShouldReturnFakeMember1()
        {
            const string path = "FakeTest";
            var provider = CreateMemberProvider();
            var member = provider.GetBindingMember(typeof(object), path, false, false);
            member.ShouldNotBeNull();
            member.GetValue(null, null).ShouldBeNull();
            member.SetValue(null, null).ShouldBeNull();
        }

        [TestMethod]
        public void ProviderShouldReturnFakeMember2()
        {
            const string path = "NodoTest";
            BindingServiceProvider.FakeMemberPrefixes.Clear();
            BindingServiceProvider.FakeMemberPrefixes.Add("Nodo");
            var provider = CreateMemberProvider();
            var member = provider.GetBindingMember(typeof(object), path, false, false);
            member.ShouldNotBeNull();
            member.GetValue(null, null).ShouldBeNull();
            member.SetValue(null, null).ShouldBeNull();
        }

        protected virtual IBindingMemberProvider CreateMemberProvider()
        {
            return new BindingMemberProvider();
        }

        #endregion
    }
}