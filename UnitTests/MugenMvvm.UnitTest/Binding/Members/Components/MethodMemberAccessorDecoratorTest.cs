using System;
using System.Globalization;
using System.Linq;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Extensions;
using MugenMvvm.Binding.Members;
using MugenMvvm.Binding.Members.Components;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.UnitTest.Binding.Members.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Binding.Members.Components
{
    public class MethodMemberAccessorDecoratorTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void TryGetMembersShouldHandleCustomMethodCall()
        {
            const int index = 1;
            var accessor = new TestAccessorMemberInfo();
            var method = new TestMethodMemberInfo
            {
                DeclaringType = typeof(object),
                AccessModifiers = MemberFlags.InstancePublic,
                TryGetAccessor = (flags, objects, arg3) =>
                {
                    flags.ShouldEqual((ArgumentFlags)0);
                    objects!.Length.ShouldEqual(1);
                    objects![0].ShouldEqual(1);
                    arg3.ShouldEqual(DefaultMetadata);
                    return accessor;
                },
                GetParameters = () => new[] { new TestParameterInfo { ParameterType = typeof(int) }, }
            };
            var manager = new MemberManager();
            manager.AddComponent(TestMemberManagerComponent.Selector);
            manager.AddComponent(new NameRequestMemberManagerDecorator());
            manager.AddComponent(new MethodMemberAccessorDecorator());
            manager.AddComponent(new TestMemberProviderComponent
            {
                TryGetMembers = (type, s, t, arg3) =>
                {
                    if (t == MemberType.Method)
                        return method;
                    return default;
                }
            });

            manager.TryGetMembers(typeof(TestMethodInvoker), MemberType.Event, MemberFlags.All, $"{nameof(TestMethodInvoker.GetValue)}({index.ToString(CultureInfo.InvariantCulture)})", DefaultMetadata)
                .IsNullOrEmpty()
                .ShouldBeTrue();
            manager.TryGetMembers(typeof(TestMethodInvoker), MemberType.Accessor, MemberFlags.All, $"{nameof(TestMethodInvoker.GetValue)}({index.ToString(CultureInfo.InvariantCulture)})", DefaultMetadata)
                .AsList()
                .OfType<TestAccessorMemberInfo>()
                .Single()
                .ShouldEqual(accessor);
        }

        [Fact]
        public void TryGetMembersShouldHandleMethodCall()
        {
            const int index = 1;
            var manager = new MemberManager();
            manager.AddComponent(new MethodMemberAccessorDecorator());
            manager.AddComponent(new ReflectionMemberProvider());
            manager.AddComponent(TestMemberManagerComponent.Selector);
            manager.AddComponent(new NameRequestMemberManagerDecorator());

            var member = manager.TryGetMembers(typeof(TestMethodInvoker), MemberType.Accessor, MemberFlags.All, $"{nameof(TestMethodInvoker.GetValue)}({index.ToString(CultureInfo.InvariantCulture)})", DefaultMetadata)
                .AsList()
                .OfType<MethodAccessorMemberInfo>()
                .Single(info => info.ArgumentFlags == 0);

            var getter = 0;
            var instance = new TestMethodInvoker
            {
                GetIndex = i =>
                {
                    ++getter;
                    i.ShouldEqual(index);
                    return index;
                }
            };
            member.GetValue(instance, DefaultMetadata).ShouldEqual(index);
            getter.ShouldEqual(1);
            ShouldThrow<InvalidOperationException>(() => member.SetValue(instance, null, DefaultMetadata));
        }

        [Fact]
        public void TryGetMembersShouldHandleMethodCallOptional()
        {
            const string index1 = "t1";
            var manager = new MemberManager();
            manager.AddComponent(new MethodMemberAccessorDecorator());
            manager.AddComponent(new ReflectionMemberProvider());
            manager.AddComponent(new NameRequestMemberManagerDecorator());
            manager.AddComponent(TestMemberManagerComponent.Selector);

            var member = manager.TryGetMembers(typeof(TestMethodInvoker), MemberType.Accessor, MemberFlags.All, $"{nameof(TestMethodInvoker.GetValue)}('{index1}')", DefaultMetadata)
                .AsList()
                .OfType<MethodAccessorMemberInfo>()
                .Single(info => info.ArgumentFlags.HasFlagEx(ArgumentFlags.Optional));

            var getter = 0;
            var instance = new TestMethodInvoker
            {
                GetIndexOptional = (i1, i2) =>
                {
                    ++getter;
                    i1.ShouldEqual(index1);
                    i2.ShouldEqual(TestMethodInvoker.OptionalValue);
                    return index1;
                }
            };
            member.GetValue(instance, DefaultMetadata).ShouldEqual(index1);
            getter.ShouldEqual(1);
            ShouldThrow<InvalidOperationException>(() => member.SetValue(instance, null, DefaultMetadata));
        }

        [Fact]
        public void TryGetMembersShouldHandleMethodCallEmptyArray()
        {
            const int index1 = 2;
            var manager = new MemberManager();
            manager.AddComponent(new MethodMemberAccessorDecorator());
            manager.AddComponent(new ReflectionMemberProvider());
            manager.AddComponent(new NameRequestMemberManagerDecorator());
            manager.AddComponent(TestMemberManagerComponent.Selector);

            var member = manager.TryGetMembers(typeof(TestMethodInvoker), MemberType.Accessor, MemberFlags.All, $"{nameof(TestMethodInvoker.GetValue)}({index1})", DefaultMetadata)
                .AsList()
                .OfType<MethodAccessorMemberInfo>()
                .Single(info => info.ArgumentFlags.HasFlagEx(ArgumentFlags.EmptyParamArray));

            var getter = 0;
            var instance = new TestMethodInvoker
            {
                GetIndexParams = (i1, i2) =>
                {
                    ++getter;
                    i1.ShouldEqual(index1);
                    i2.ShouldBeEmpty();
                    return index1;
                }
            };
            member.GetValue(instance, DefaultMetadata).ShouldEqual(index1);
            getter.ShouldEqual(1);
            ShouldThrow<InvalidOperationException>(() => member.SetValue(instance, null, DefaultMetadata));
        }

        [Fact]
        public void TryGetMembersShouldHandleMethodCallArray()
        {
            const int index1 = 2;
            var args = new[] { 2, 3, 4, 56 };
            var manager = new MemberManager();
            manager.AddComponent(new MethodMemberAccessorDecorator());
            manager.AddComponent(new ReflectionMemberProvider());
            manager.AddComponent(new NameRequestMemberManagerDecorator());
            manager.AddComponent(TestMemberManagerComponent.Selector);

            var member = manager.TryGetMembers(typeof(TestMethodInvoker), MemberType.Accessor, MemberFlags.All, $"{nameof(TestMethodInvoker.GetValue)}({index1}, {string.Join(",", args)})", DefaultMetadata)
                .AsList()
                .OfType<MethodAccessorMemberInfo>()
                .Single(info => info.ArgumentFlags.HasFlagEx(ArgumentFlags.ParamArray));

            var getter = 0;
            var instance = new TestMethodInvoker
            {
                GetIndexParams = (i1, i2) =>
                {
                    ++getter;
                    i1.ShouldEqual(index1);
                    i2.SequenceEqual(args).ShouldBeTrue();
                    return index1;
                }
            };
            member.GetValue(instance, DefaultMetadata).ShouldEqual(index1);
            getter.ShouldEqual(1);
            ShouldThrow<InvalidOperationException>(() => member.SetValue(instance, null, DefaultMetadata));
        }

        [Fact]
        public void TryGetMembersShouldHandleMethodCallMetadata()
        {
            const string index1 = "test";
            var manager = new MemberManager();
            manager.AddComponent(new MethodMemberAccessorDecorator());
            manager.AddComponent(new ReflectionMemberProvider());
            manager.AddComponent(new NameRequestMemberManagerDecorator());
            manager.AddComponent(TestMemberManagerComponent.Selector);

            var member = manager.TryGetMembers(typeof(TestMethodInvoker), MemberType.Accessor, MemberFlags.All, $"{nameof(TestMethodInvoker.GetValue)}('{index1}')", DefaultMetadata)
                .AsList()
                .OfType<MethodAccessorMemberInfo>()
                .Single(info => info.ArgumentFlags.HasFlagEx(ArgumentFlags.Metadata));

            var getter = 0;
            var instance = new TestMethodInvoker
            {
                GetIndexMetadata = (i1, i2) =>
                {
                    ++getter;
                    i1.ShouldEqual(index1);
                    i2.ShouldEqual(DefaultMetadata);
                    return index1;
                }
            };
            member.GetValue(instance, DefaultMetadata).ShouldEqual(index1);
            getter.ShouldEqual(1);
            ShouldThrow<InvalidOperationException>(() => member.SetValue(instance, null, DefaultMetadata));
        }

        #endregion

        #region Nested types

        public class TestMethodInvoker
        {
            #region Fields

            public const string OptionalValue = "index2";

            #endregion

            #region Properties

            public Func<int, int>? GetIndex { get; set; }

            public Func<string, string, object>? GetIndexOptional { get; set; }

            public Func<string, IReadOnlyMetadataContext, object>? GetIndexMetadata { get; set; }

            public Func<int, int[], object>? GetIndexParams { get; set; }

            #endregion

            #region Methods

            public int GetValue(int index)
            {
                return GetIndex!(index);
            }

            public object GetValue(string index1, string index2 = OptionalValue)
            {
                return GetIndexOptional!(index1, index2);
            }

            public object GetValue(string index1, IReadOnlyMetadataContext index2)
            {
                return GetIndexMetadata!(index1, index2);
            }

            public object GetValue(int index, params int[] indexes)
            {
                return GetIndexParams!(index, indexes);
            }

            #endregion
        }

        #endregion
    }
}