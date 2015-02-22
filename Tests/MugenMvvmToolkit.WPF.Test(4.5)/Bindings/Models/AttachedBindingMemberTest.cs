using System;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Binding.Models;
using MugenMvvmToolkit.Infrastructure;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Test.TestModels;
using Should;

namespace MugenMvvmToolkit.Test.Bindings.Models
{
    [TestClass]
    public class AttachedBindingMemberTest : BindingTestBase
    {
        #region Methods

        [TestMethod]
        public void AutoPropertyTest()
        {
            var source = new BindingSourceModel();
            const string path = "path";
            Type type = typeof(string);
            IAttachedBindingMemberInfo<object, object> property = AttachedBindingMember.CreateAutoProperty(path, type);
            property.Path.ShouldEqual(path);
            property.Type.ShouldEqual(type);
            property.CanRead.ShouldBeTrue();
            property.CanWrite.ShouldBeTrue();
            property.Member.ShouldBeNull();
            property.MemberType.ShouldEqual(BindingMemberType.Attached);

            property.GetValue(source, null).ShouldBeNull();
            property.SetValue(source, new object[] { path });
            property.GetValue(source, null).ShouldEqual(path);
        }

        [TestMethod]
        public void AutoPropertyGenericTest()
        {
            var source = new BindingSourceModel();
            const string path = "path";
            Type type = typeof(string);
            IAttachedBindingMemberInfo<BindingSourceModel, string> property =
                AttachedBindingMember.CreateAutoProperty<BindingSourceModel, string>(path);
            property.Path.ShouldEqual(path);
            property.Type.ShouldEqual(type);
            property.CanRead.ShouldBeTrue();
            property.CanWrite.ShouldBeTrue();
            property.Member.ShouldBeNull();
            property.MemberType.ShouldEqual(BindingMemberType.Attached);


            property.GetValue(source, null).ShouldBeNull();
            property.SetValue(source, new object[] { path });
            property.GetValue(source, null).ShouldEqual(path);
        }

        [TestMethod]
        public void AutoPropertyMemberChangeTest()
        {
            bool isInvoked = false;
            var source = new BindingSourceModel();
            const string path = "path";
            IAttachedBindingMemberInfo<object, object> property = AttachedBindingMember.CreateAutoProperty(path, typeof(string),
                (o, args) =>
                {
                    isInvoked = true;
                    o.ShouldEqual(source);
                    args.OldValue.ShouldBeNull();
                    args.NewValue.ShouldEqual(path);
                });

            property.SetValue(source, new object[] { path });
            isInvoked.ShouldBeTrue();

            isInvoked = false;
            property.SetValue(source, new object[] { path });
            isInvoked.ShouldBeFalse();
        }

        [TestMethod]
        public void AutoPropertyGenericMemberChangeTest()
        {
            bool isInvoked = false;
            var source = new BindingSourceModel();
            const string path = "path";
            var property = AttachedBindingMember.CreateAutoProperty<BindingSourceModel, string>(path,
                (o, args) =>
                {
                    isInvoked = true;
                    o.ShouldEqual(source);
                    args.OldValue.ShouldBeNull();
                    args.NewValue.ShouldEqual(path);
                });

            property.SetValue(source, new object[] { path });
            isInvoked.ShouldBeTrue();

            isInvoked = false;
            property.SetValue(source, new object[] { path });
            isInvoked.ShouldBeFalse();
        }

        [TestMethod]
        public void AutoPropertyObserveTest()
        {
            bool isInvoked = false;
            var listenerMock = new EventListenerMock
            {
                Handle = (o, o1) => isInvoked = true
            };
            var source = new BindingSourceModel();
            const string path = "path";
            var property = AttachedBindingMember.CreateAutoProperty(path, typeof(string));

            IDisposable subscriber = property.TryObserve(source, listenerMock);
            property.SetValue(source, new object[] { path });
            isInvoked.ShouldBeTrue();
            subscriber.ShouldNotBeNull();

            isInvoked = false;
            property.SetValue(source, new object[] { path });
            isInvoked.ShouldBeFalse();

            isInvoked = false;
            property.SetValue(source, new object[] { null });
            isInvoked.ShouldBeTrue();

            subscriber.Dispose();
            isInvoked = false;
            property.SetValue(source, new object[] { path });
            isInvoked.ShouldBeFalse();
        }

        [TestMethod]
        public void AutoPropertyGenericObserveTest()
        {
            bool isInvoked = false;
            var listenerMock = new EventListenerMock
            {
                Handle = (o, o1) => isInvoked = true
            };
            var source = new BindingSourceModel();
            const string path = "path";
            var property = AttachedBindingMember.CreateAutoProperty<BindingSourceModel, string>(path);

            IDisposable subscriber = property.TryObserve(source, listenerMock);
            property.SetValue(source, new object[] { path });
            isInvoked.ShouldBeTrue();
            subscriber.ShouldNotBeNull();

            isInvoked = false;
            property.SetValue(source, new object[] { path });
            isInvoked.ShouldBeFalse();

            isInvoked = false;
            property.SetValue(source, new object[] { null });
            isInvoked.ShouldBeTrue();

            subscriber.Dispose();
            isInvoked = false;
            property.SetValue(source, new object[] { path });
            isInvoked.ShouldBeFalse();
        }

        [TestMethod]
        public void AutoPropertyMemberAttachTest()
        {
            bool isInvoked = false;
            var source = new BindingSourceModel();
            const string path = "path";
            var property = AttachedBindingMember.CreateAutoProperty(path, typeof(string), null,
                (model, args) =>
                {
                    model.ShouldEqual(source);
                    args.ShouldNotBeNull();
                    isInvoked = true;
                });

            property.SetValue(source, new object[] { path });
            isInvoked.ShouldBeTrue();

            isInvoked = false;
            property.SetValue(source, new object[] { null });
            isInvoked.ShouldBeFalse();

            source = new BindingSourceModel();
            property.SetValue(source, new object[] { path });
            isInvoked.ShouldBeTrue();
        }

        [TestMethod]
        public void AutoPropertyGenericMemberAttachTest()
        {
            bool isInvoked = false;
            var source = new BindingSourceModel();
            const string path = "path";
            var property = AttachedBindingMember.CreateAutoProperty<BindingSourceModel, string>(path, memberChangedHandler: null,
                memberAttachedHandler: (model, args) =>
                {
                    model.ShouldEqual(source);
                    args.ShouldNotBeNull();
                    isInvoked = true;
                });

            property.SetValue(source, new object[] { path });
            isInvoked.ShouldBeTrue();

            isInvoked = false;
            property.SetValue(source, new object[] { null });
            isInvoked.ShouldBeFalse();

            source = new BindingSourceModel();
            property.SetValue(source, new object[] { path });
            isInvoked.ShouldBeTrue();
        }

        [TestMethod]
        public void AutoPropertyDefaultValueTest()
        {
            string defaultValue = "test";
            bool isInvoked = false;
            var source = new BindingSourceModel();
            const string path = "path";
            var property = AttachedBindingMember.CreateAutoProperty(path, typeof(string), null, null, (o, info) =>
            {
                isInvoked = true;
                o.ShouldEqual(source);
                info.ShouldNotBeNull();
                return defaultValue;
            });

            property.GetValue(source, null).ShouldEqual(defaultValue);
            isInvoked.ShouldBeTrue();

            isInvoked = false;
            property.GetValue(source, null).ShouldEqual(defaultValue);
            isInvoked.ShouldBeFalse();

            source = new BindingSourceModel();
            defaultValue = path;
            property.GetValue(source, null).ShouldEqual(defaultValue);
            isInvoked.ShouldBeTrue();
        }

        [TestMethod]
        public void AutoPropertyGenericDefaultValueTest()
        {
            string defaultValue = "test";
            bool isInvoked = false;
            var source = new BindingSourceModel();
            const string path = "path";
            var property = AttachedBindingMember.CreateAutoProperty<BindingSourceModel, string>(path, null, null, (o, info) =>
            {
                isInvoked = true;
                o.ShouldEqual(source);
                info.ShouldNotBeNull();
                return defaultValue;
            });

            property.GetValue(source, null).ShouldEqual(defaultValue);
            isInvoked.ShouldBeTrue();

            isInvoked = false;
            property.GetValue(source, null).ShouldEqual(defaultValue);
            isInvoked.ShouldBeFalse();

            source = new BindingSourceModel();
            defaultValue = path;
            property.GetValue(source, null).ShouldEqual(defaultValue);
            isInvoked.ShouldBeTrue();
        }

        [TestMethod]
        public void CreateMemberTest()
        {
            object value = null;
            var source = new BindingSourceModel();
            const string path = "path";
            Type type = typeof(string);
            var property = AttachedBindingMember.CreateMember(path, type, (info, o) => value,
                (info, o, v) => value = v, member: BindingSourceModel.IntPropertyInfo);
            property.Path.ShouldEqual(path);
            property.Type.ShouldEqual(type);
            property.CanRead.ShouldBeTrue();
            property.CanWrite.ShouldBeTrue();
            property.MemberType.ShouldEqual(BindingMemberType.Attached);
            property.Member.ShouldEqual(BindingSourceModel.IntPropertyInfo);

            property.GetValue(source, null).ShouldBeNull();
            property.SetValue(source, new object[] { path });
            property.GetValue(source, null).ShouldEqual(path);
            value.ShouldEqual(path);
        }

        [TestMethod]
        public void CreateMemberGenericTest()
        {
            string value = null;
            var source = new BindingSourceModel();
            const string path = "path";
            Type type = typeof(string);
            var property = AttachedBindingMember.CreateMember<BindingSourceModel, string>(path, (info, o) => value,
                (info, o, v) => value = v, member: BindingSourceModel.IntPropertyInfo);
            property.Path.ShouldEqual(path);
            property.Type.ShouldEqual(type);
            property.CanRead.ShouldBeTrue();
            property.CanWrite.ShouldBeTrue();
            property.MemberType.ShouldEqual(BindingMemberType.Attached);
            property.Member.ShouldEqual(BindingSourceModel.IntPropertyInfo);

            property.GetValue(source, null).ShouldBeNull();
            property.SetValue(source, new object[] { path });
            property.GetValue(source, null).ShouldEqual(path);
            value.ShouldEqual(path);
        }

        [TestMethod]
        public void CreateMemberGetterTest()
        {
            var args = new object[0];
            const string value = "1";
            var source = new BindingSourceModel();
            const string path = "path";
            Type type = typeof(string);
            var property = AttachedBindingMember.CreateMember(path, type, (info, o, arg3) =>
            {
                info.ShouldNotBeNull();
                o.ShouldEqual(source);
                arg3.ShouldEqual(args);
                return value;
            });
            property.Path.ShouldEqual(path);
            property.Type.ShouldEqual(type);
            property.CanRead.ShouldBeTrue();
            property.CanWrite.ShouldBeFalse();
            property.MemberType.ShouldEqual(BindingMemberType.Attached);

            property.GetValue(source, args).ShouldEqual(value);
            ShouldThrow(() => property.SetValue(source, new object[] { path }));
        }

        [TestMethod]
        public void CreateMemberGenericGetterTest()
        {
            var args = new object[0];
            const string value = "1";
            var source = new BindingSourceModel();
            const string path = "path";
            Type type = typeof(string);
            var property = AttachedBindingMember.CreateMember<BindingSourceModel, string>(path, (info, o, arg3) =>
            {
                info.ShouldNotBeNull();
                o.ShouldEqual(source);
                arg3.ShouldEqual(args);
                return value;
            });
            property.Path.ShouldEqual(path);
            property.Type.ShouldEqual(type);
            property.CanRead.ShouldBeTrue();
            property.CanWrite.ShouldBeFalse();
            property.MemberType.ShouldEqual(BindingMemberType.Attached);

            property.GetValue(source, args).ShouldEqual(value);
            ShouldThrow(() => property.SetValue(source, new object[] { path }));
        }

        [TestMethod]
        public void CreateMemberSetterTest()
        {
            object value = null;
            var source = new BindingSourceModel();
            const string path = "path";
            Type type = typeof(string);
            var property = AttachedBindingMember.CreateMember(path, type, null, (info, o, v) =>
            {
                info.ShouldNotBeNull();
                o.ShouldEqual(source);
                v.ShouldEqual(path);
                value = v;
            });
            property.Path.ShouldEqual(path);
            property.Type.ShouldEqual(type);
            property.CanRead.ShouldBeFalse();
            property.CanWrite.ShouldBeTrue();
            property.MemberType.ShouldEqual(BindingMemberType.Attached);

            property.SetValue(source, new object[] { path });
            value.ShouldEqual(path);
            ShouldThrow(() => property.GetValue(source, null));
        }

        [TestMethod]
        public void CreateMemberGenericSetterTest()
        {
            object value = null;
            var source = new BindingSourceModel();
            const string path = "path";
            Type type = typeof(string);
            var property = AttachedBindingMember.CreateMember<BindingSourceModel, string>(path, null,
                (info, o, v) =>
                {
                    info.ShouldNotBeNull();
                    o.ShouldEqual(source);
                    v.ShouldEqual(path);
                    value = v;
                });
            property.Path.ShouldEqual(path);
            property.Type.ShouldEqual(type);
            property.CanRead.ShouldBeFalse();
            property.CanWrite.ShouldBeTrue();
            property.MemberType.ShouldEqual(BindingMemberType.Attached);

            property.SetValue(source, new object[] { path });
            value.ShouldEqual(path);
            ShouldThrow(() => property.GetValue(source, null));
        }

        [TestMethod]
        public void CreateMemberMemberAttachTest()
        {
            bool isInvoked = false;
            var source = new BindingSourceModel();
            const string path = "path";
            var property = AttachedBindingMember.CreateMember(path, typeof(string), (info, o) => null, (info, o, v) => { }, memberAttachedHandler:
                (model, args) =>
                {
                    model.ShouldEqual(source);
                    args.ShouldNotBeNull();
                    isInvoked = true;
                });

            property.SetValue(source, new object[] { path });
            isInvoked.ShouldBeTrue();

            isInvoked = false;
            property.SetValue(source, new object[] { null });
            isInvoked.ShouldBeFalse();

            source = new BindingSourceModel();
            property.SetValue(source, new object[] { path });
            isInvoked.ShouldBeTrue();
        }

        [TestMethod]
        public void CreateMemberGenericMemberAttachTest()
        {
            bool isInvoked = false;
            var source = new BindingSourceModel();
            const string path = "path";
            var property = AttachedBindingMember.CreateMember<BindingSourceModel, string>(path, (info, o) => null, (info, o, v) => { }, memberAttachedHandler:
                (model, args) =>
                {
                    model.ShouldEqual(source);
                    args.ShouldNotBeNull();
                    isInvoked = true;
                });

            property.SetValue(source, new object[] { path });
            isInvoked.ShouldBeTrue();

            isInvoked = false;
            property.SetValue(source, new object[] { null });
            isInvoked.ShouldBeFalse();

            source = new BindingSourceModel();
            property.SetValue(source, new object[] { path });
            isInvoked.ShouldBeTrue();
        }

        [TestMethod]
        public void CreateMemberObserveEventTest()
        {
            bool isInvoked = false;
            var listenerMock = new EventListenerMock
            {
                Handle = (o, o1) => isInvoked = true
            };
            var source = new BindingSourceModel();
            const string path = "path";
            source.RaiseEvent();
            var property = AttachedBindingMember.CreateMember(path, typeof(string), (info, o) => null,
                (info, o, v) => { }, BindingSourceModel.EventName);

            IDisposable subscriber = property.TryObserve(source, listenerMock);
            source.RaiseEvent();
            isInvoked.ShouldBeTrue();
            subscriber.ShouldNotBeNull();

            isInvoked = false;
            source.RaiseEvent();
            isInvoked.ShouldBeTrue();

            subscriber.Dispose();
            isInvoked = false;
            source.RaiseEvent();
            isInvoked.ShouldBeFalse();
        }

        [TestMethod]
        public void CreateMemberGenericObserveEventTest()
        {
            bool isInvoked = false;
            var listenerMock = new EventListenerMock
            {
                Handle = (o, o1) => isInvoked = true
            };
            var source = new BindingSourceModel();
            const string path = "path";
            source.RaiseEvent();
            var property = AttachedBindingMember.CreateMember<BindingSourceModel, string>(path, (info, o) => null,
                (info, o, v) => { }, BindingSourceModel.EventName);

            IDisposable subscriber = property.TryObserve(source, listenerMock);
            source.RaiseEvent();
            isInvoked.ShouldBeTrue();
            subscriber.ShouldNotBeNull();

            isInvoked = false;
            source.RaiseEvent();
            isInvoked.ShouldBeTrue();

            subscriber.Dispose();
            isInvoked = false;
            source.RaiseEvent();
            isInvoked.ShouldBeFalse();
        }

        [TestMethod]
        public void CreateMemberObserveTest()
        {
            bool isInvoked = false;
            IDisposable result = null;
            var listenerMock = new EventListenerMock();
            var source = new BindingSourceModel();
            const string path = "path";
            var property = AttachedBindingMember.CreateMember(path, typeof(string), (info, o) => null,
                (info, o, v) => { },
                (info, o, arg3) =>
                {
                    isInvoked = true;
                    info.ShouldNotBeNull();
                    o.ShouldEqual(source);
                    arg3.ShouldEqual(listenerMock);
                    return result;
                });

            property.TryObserve(source, listenerMock).ShouldBeNull();
            isInvoked.ShouldBeTrue();

            isInvoked = false;
            result = new ActionToken(() => { });
            property.TryObserve(source, listenerMock).ShouldEqual(result);
            isInvoked.ShouldBeTrue();
        }

        [TestMethod]
        public void CreateMemberGenericObserveTest()
        {
            bool isInvoked = false;
            IDisposable result = null;
            var listenerMock = new EventListenerMock();
            var source = new BindingSourceModel();
            const string path = "path";
            var property = AttachedBindingMember.CreateMember<BindingSourceModel, string>(path, (info, o) => null,
                (info, o, v) => { },
                (info, o, arg3) =>
                {
                    isInvoked = true;
                    info.ShouldNotBeNull();
                    o.ShouldEqual(source);
                    arg3.ShouldEqual(listenerMock);
                    return result;
                });

            property.TryObserve(source, listenerMock).ShouldBeNull();
            isInvoked.ShouldBeTrue();

            isInvoked = false;
            result = new ActionToken(() => { });
            property.TryObserve(source, listenerMock).ShouldEqual(result);
            isInvoked.ShouldBeTrue();
        }




        [TestMethod]
        public void CreateNotifiableMemberTest()
        {
            object value = null;
            var source = new BindingSourceModel();
            const string path = "path";
            Type type = typeof(string);
            var property = AttachedBindingMember.CreateNotifiableMember(path, type, (info, o) => value,
                (info, o, v) =>
                {
                    value = v;
                    return true;
                }, member: BindingSourceModel.IntPropertyInfo);
            property.Path.ShouldEqual(path);
            property.Type.ShouldEqual(type);
            property.CanRead.ShouldBeTrue();
            property.CanWrite.ShouldBeTrue();
            property.MemberType.ShouldEqual(BindingMemberType.Attached);
            property.Member.ShouldEqual(BindingSourceModel.IntPropertyInfo);

            property.GetValue(source, null).ShouldBeNull();
            property.SetValue(source, new object[] { path });
            property.GetValue(source, null).ShouldEqual(path);
            value.ShouldEqual(path);
        }

        [TestMethod]
        public void CreateNotifiableMemberGenericTest()
        {
            string value = null;
            var source = new BindingSourceModel();
            const string path = "path";
            Type type = typeof(string);
            var property = AttachedBindingMember.CreateNotifiableMember<BindingSourceModel, string>(path, (info, o) => value,
                (info, o, v) =>
                {
                    value = v;
                    return true;
                }, member: BindingSourceModel.IntPropertyInfo);
            property.Path.ShouldEqual(path);
            property.Type.ShouldEqual(type);
            property.CanRead.ShouldBeTrue();
            property.CanWrite.ShouldBeTrue();
            property.MemberType.ShouldEqual(BindingMemberType.Attached);
            property.Member.ShouldEqual(BindingSourceModel.IntPropertyInfo);

            property.GetValue(source, null).ShouldBeNull();
            property.SetValue(source, new object[] { path });
            property.GetValue(source, null).ShouldEqual(path);
            value.ShouldEqual(path);
        }

        [TestMethod]
        public void CreateNotifiableMemberGetterTest()
        {
            var args = new object[0];
            const string value = "1";
            var source = new BindingSourceModel();
            const string path = "path";
            Type type = typeof(string);
            var property = AttachedBindingMember.CreateNotifiableMember(path, type, (info, o) =>
            {
                info.ShouldNotBeNull();
                o.ShouldEqual(source);
                return value;
            }, (info, o, v) => true);
            property.Path.ShouldEqual(path);
            property.Type.ShouldEqual(type);
            property.CanRead.ShouldBeTrue();
            property.CanWrite.ShouldBeTrue();
            property.MemberType.ShouldEqual(BindingMemberType.Attached);
            property.GetValue(source, args).ShouldEqual(value);
        }

        [TestMethod]
        public void CreateNotifiableMemberGenericGetterTest()
        {
            var args = new object[0];
            const string value = "1";
            var source = new BindingSourceModel();
            const string path = "path";
            Type type = typeof(string);
            var property = AttachedBindingMember.CreateNotifiableMember<BindingSourceModel, string>(path, (info, o) =>
            {
                info.ShouldNotBeNull();
                o.ShouldEqual(source);
                return value;
            }, (info, o, v) => true);
            property.Path.ShouldEqual(path);
            property.Type.ShouldEqual(type);
            property.CanRead.ShouldBeTrue();
            property.CanWrite.ShouldBeTrue();
            property.MemberType.ShouldEqual(BindingMemberType.Attached);
            property.GetValue(source, args).ShouldEqual(value);
        }

        [TestMethod]
        public void CreateNotifiableMemberSetterTest()
        {
            object value = null;
            var source = new BindingSourceModel();
            const string path = "path";
            Type type = typeof(string);
            var property = AttachedBindingMember.CreateNotifiableMember(path, type, null, (info, o, v) =>
            {
                info.ShouldNotBeNull();
                o.ShouldEqual(source);
                v.ShouldEqual(path);
                value = v;
                return true;
            });
            property.Path.ShouldEqual(path);
            property.Type.ShouldEqual(type);
            property.CanRead.ShouldBeFalse();
            property.CanWrite.ShouldBeTrue();
            property.MemberType.ShouldEqual(BindingMemberType.Attached);

            property.SetValue(source, new object[] { path });
            value.ShouldEqual(path);
            ShouldThrow(() => property.GetValue(source, null));
        }

        [TestMethod]
        public void CreateNotifiableMemberGenericSetterTest()
        {
            object value = null;
            var source = new BindingSourceModel();
            const string path = "path";
            Type type = typeof(string);
            var property = AttachedBindingMember.CreateNotifiableMember<BindingSourceModel, string>(path, null, (info, o, v) =>
            {
                info.ShouldNotBeNull();
                o.ShouldEqual(source);
                v.ShouldEqual(path);
                value = v;
                return true;
            });
            property.Path.ShouldEqual(path);
            property.Type.ShouldEqual(type);
            property.CanRead.ShouldBeFalse();
            property.CanWrite.ShouldBeTrue();
            property.MemberType.ShouldEqual(BindingMemberType.Attached);

            property.SetValue(source, new object[] { path });
            value.ShouldEqual(path);
            ShouldThrow(() => property.GetValue(source, null));
        }


        [TestMethod]
        public void CreateNotifiableMemberMemberAttachTest()
        {
            bool isInvoked = false;
            var source = new BindingSourceModel();
            const string path = "path";
            var property = AttachedBindingMember.CreateNotifiableMember(path, typeof(string), (info, o) => null, (info, o, v) => true,
                (model, args) =>
                {
                    model.ShouldEqual(source);
                    args.ShouldNotBeNull();
                    isInvoked = true;
                });

            property.SetValue(source, new object[] { path });
            isInvoked.ShouldBeTrue();

            isInvoked = false;
            property.SetValue(source, new object[] { null });
            isInvoked.ShouldBeFalse();

            source = new BindingSourceModel();
            property.SetValue(source, new object[] { path });
            isInvoked.ShouldBeTrue();
        }


        [TestMethod]
        public void CreateNotifiableMemberGenericMemberAttachTest()
        {
            bool isInvoked = false;
            var source = new BindingSourceModel();
            const string path = "path";
            var property = AttachedBindingMember.CreateNotifiableMember<BindingSourceModel, string>(path, (info, o) => null, (info, o, v) => true,
                (model, args) =>
                {
                    model.ShouldEqual(source);
                    args.ShouldNotBeNull();
                    isInvoked = true;
                });

            property.SetValue(source, new object[] { path });
            isInvoked.ShouldBeTrue();

            isInvoked = false;
            property.SetValue(source, new object[] { null });
            isInvoked.ShouldBeFalse();

            source = new BindingSourceModel();
            property.SetValue(source, new object[] { path });
            isInvoked.ShouldBeTrue();
        }

        [TestMethod]
        public void CreateNotifiableMemberObserveTest()
        {
            bool isInvoked = false;
            bool raiseEvent = true;
            var listenerMock = new EventListenerMock
            {
                Handle = (o, o1) => isInvoked = true
            };
            var source = new BindingSourceModel();
            const string path = "path";
            var property = AttachedBindingMember.CreateNotifiableMember(path, typeof(string), (info, o) => null,
                (info, o, v) => raiseEvent);

            IDisposable subscriber = property.TryObserve(source, listenerMock);
            property.SetValue(source, new object[] { path });
            isInvoked.ShouldBeTrue();
            subscriber.ShouldNotBeNull();

            raiseEvent = false;
            isInvoked = false;
            property.SetValue(source, new object[] { path });
            isInvoked.ShouldBeFalse();

            raiseEvent = true;
            isInvoked = false;
            property.SetValue(source, new object[] { null });
            isInvoked.ShouldBeTrue();

            subscriber.Dispose();
            isInvoked = false;
            property.SetValue(source, new object[] { path });
            isInvoked.ShouldBeFalse();
        }

        [TestMethod]
        public void CreateNotifiableMemberGenericObserveTest()
        {
            bool isInvoked = false;
            bool raiseEvent = true;
            var listenerMock = new EventListenerMock
            {
                Handle = (o, o1) => isInvoked = true
            };
            var source = new BindingSourceModel();
            const string path = "path";
            var property = AttachedBindingMember.CreateNotifiableMember<BindingSourceModel, string>(path, (info, o) => null,
                (info, o, v) => raiseEvent);

            IDisposable subscriber = property.TryObserve(source, listenerMock);
            property.SetValue(source, new object[] { path });
            isInvoked.ShouldBeTrue();
            subscriber.ShouldNotBeNull();

            raiseEvent = false;
            isInvoked = false;
            property.SetValue(source, new object[] { path });
            isInvoked.ShouldBeFalse();

            raiseEvent = true;
            isInvoked = false;
            property.SetValue(source, new object[] { null });
            isInvoked.ShouldBeTrue();

            subscriber.Dispose();
            isInvoked = false;
            property.SetValue(source, new object[] { path });
            isInvoked.ShouldBeFalse();
        }


        [TestMethod]
        public void CreateEventTest()
        {
            IEventListener listener = new EventListenerMock();
            var source = new BindingSourceModel();
            const string path = "path";
            var @event = AttachedBindingMember.CreateEvent(path, (info, o, arg3) => null);
            @event.Path.ShouldEqual(path);
            @event.Type.ShouldEqual(typeof(Delegate));
            @event.CanRead.ShouldBeTrue();
            @event.CanWrite.ShouldBeTrue();
            @event.Member.ShouldBeNull();
            @event.MemberType.ShouldEqual(BindingMemberType.Event);

            @event.GetValue(source, null).ShouldBeType<BindingMemberValue>();
            @event.SetValue(source, new object[] { listener });
            var value = (BindingMemberValue)@event.GetValue(source, null);
            value.Member.ShouldEqual(@event);
            value.MemberSource.Target.ShouldEqual(source);
        }

        [TestMethod]
        public void CreateEventGenericTest()
        {
            IEventListener listener = new EventListenerMock();
            var source = new BindingSourceModel();
            const string path = "path";
            var @event = AttachedBindingMember.CreateEvent<BindingSourceModel>(path, (info, o, arg3) => null);
            @event.Path.ShouldEqual(path);
            @event.Type.ShouldEqual(typeof(Delegate));
            @event.CanRead.ShouldBeTrue();
            @event.CanWrite.ShouldBeTrue();
            @event.Member.ShouldBeNull();
            @event.MemberType.ShouldEqual(BindingMemberType.Event);

            @event.GetValue(source, null).ShouldBeType<BindingMemberValue>();
            @event.SetValue(source, new object[] { listener });
            var value = (BindingMemberValue)@event.GetValue(source, null);
            value.Member.ShouldEqual(@event);
            value.MemberSource.Target.ShouldEqual(source);
        }

        [TestMethod]
        public void CreateEventSetterTest()
        {
            bool isInvoked = false;
            var source = new BindingSourceModel();
            IEventListener listener = new EventListenerMock();
            const string path = "path";
            var property = AttachedBindingMember.CreateEvent(path, (info, o, arg3) =>
            {
                info.ShouldNotBeNull();
                o.ShouldEqual(source);
                arg3.ShouldEqual(listener);
                isInvoked = true;
                return null;
            });
            property.Path.ShouldEqual(path);
            property.Type.ShouldEqual(typeof(Delegate));
            property.CanRead.ShouldBeTrue();
            property.CanWrite.ShouldBeTrue();
            property.MemberType.ShouldEqual(BindingMemberType.Event);

            property.SetValue(source, new object[] { listener });
            isInvoked.ShouldBeTrue();
        }

        [TestMethod]
        public void CreateEventGenericSetterTest()
        {
            bool isInvoked = false;
            var source = new BindingSourceModel();
            IEventListener listener = new EventListenerMock();
            const string path = "path";
            var property = AttachedBindingMember.CreateEvent<BindingSourceModel>(path, (info, o, arg3) =>
            {
                info.ShouldNotBeNull();
                o.ShouldEqual(source);
                arg3.ShouldEqual(listener);
                isInvoked = true;
                return null;
            });
            property.Path.ShouldEqual(path);
            property.Type.ShouldEqual(typeof(Delegate));
            property.CanRead.ShouldBeTrue();
            property.CanWrite.ShouldBeTrue();
            property.MemberType.ShouldEqual(BindingMemberType.Event);

            property.SetValue(source, new object[] { listener });
            isInvoked.ShouldBeTrue();
        }

        [TestMethod]
        public void CreateEventMemberAttachTest()
        {
            bool isInvoked = false;
            var source = new BindingSourceModel();
            IEventListener listener = new EventListenerMock();
            const string path = "path";
            var property = AttachedBindingMember.CreateEvent(path, (info, o, arg3) => null,
                (model, args) =>
                {
                    model.ShouldEqual(source);
                    args.ShouldNotBeNull();
                    isInvoked = true;
                });

            property.SetValue(source, new object[] { listener });
            isInvoked.ShouldBeTrue();

            isInvoked = false;
            property.SetValue(source, new object[] { null });
            isInvoked.ShouldBeFalse();

            source = new BindingSourceModel();
            property.SetValue(source, new object[] { listener });
            isInvoked.ShouldBeTrue();
        }

        [TestMethod]
        public void CreateEventGenericMemberAttachTest()
        {
            bool isInvoked = false;
            var source = new BindingSourceModel();
            IEventListener listener = new EventListenerMock();
            const string path = "path";
            var property = AttachedBindingMember.CreateEvent<BindingSourceModel>(path, (info, o, arg3) => null,
                (model, args) =>
                {
                    model.ShouldEqual(source);
                    args.ShouldNotBeNull();
                    isInvoked = true;
                });

            property.SetValue(source, new object[] { listener });
            isInvoked.ShouldBeTrue();

            isInvoked = false;
            property.SetValue(source, new object[] { null });
            isInvoked.ShouldBeFalse();

            source = new BindingSourceModel();
            property.SetValue(source, new object[] { listener });
            isInvoked.ShouldBeTrue();
        }


        #endregion
    }
}