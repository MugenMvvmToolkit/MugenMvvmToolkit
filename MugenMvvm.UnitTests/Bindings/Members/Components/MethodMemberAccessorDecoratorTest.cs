using System;
using System.Globalization;
using System.Linq;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Interfaces.Members;
using MugenMvvm.Bindings.Members;
using MugenMvvm.Bindings.Members.Components;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Tests.Bindings.Members;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Bindings.Members.Components
{
    [Collection(SharedContext)]
    public class MethodMemberAccessorDecoratorTest : UnitTestBase
    {
        private readonly ReflectionMemberProvider _reflectionMemberProvider;

        public MethodMemberAccessorDecoratorTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            _reflectionMemberProvider = new ReflectionMemberProvider(ObservationManager);
            MemberManager.AddComponent(TestMemberManagerComponent.Selector);
            MemberManager.AddComponent(new NameRequestMemberManagerDecorator());
            MemberManager.AddComponent(new MethodMemberAccessorDecorator(GlobalValueConverter));
            RegisterDisposeToken(WithGlobalService(ReflectionManager));
        }

        [Fact]
        public void TryGetMembersShouldHandleCustomMethodCall()
        {
            const int index = 1;
            var accessor = new TestAccessorMemberInfo();
            var method = new TestMethodMemberInfo
            {
                DeclaringType = typeof(object),
                MemberFlags = MemberFlags.InstancePublic,
                TryGetAccessor = (flags, objects, arg3) =>
                {
                    flags.ShouldEqual(default);
                    objects!.Count.ShouldEqual(1);
                    objects![0].ShouldEqual(1);
                    arg3.ShouldEqual(DefaultMetadata);
                    return accessor;
                },
                GetParameters = () => new[] { new TestParameterInfo { ParameterType = typeof(int) } }
            };

            MemberManager.AddComponent(new TestMemberProviderComponent
            {
                TryGetMembers = (_, _, _, t, _) =>
                {
                    if (t == MemberType.Method)
                        return method;
                    return default;
                }
            });
            MemberManager.TryGetMembers(typeof(TestMethodInvoker), MemberType.Event, MemberFlags.All,
                             $"{nameof(TestMethodInvoker.GetValue)}({index.ToString(CultureInfo.InvariantCulture)})", DefaultMetadata)
                         .IsEmpty
                         .ShouldBeTrue();
            MemberManager.TryGetMembers(typeof(TestMethodInvoker), MemberType.Accessor, MemberFlags.All,
                             $"{nameof(TestMethodInvoker.GetValue)}({index.ToString(CultureInfo.InvariantCulture)})", DefaultMetadata)
                         .OfType<TestAccessorMemberInfo>()
                         .Single()
                         .ShouldEqual(accessor);
        }

        [Fact]
        public void TryGetMembersShouldHandleCustomMethodCallNoParameters()
        {
            var accessor = new TestAccessorMemberInfo();
            var method = new TestMethodMemberInfo
            {
                DeclaringType = typeof(object),
                MemberFlags = MemberFlags.InstancePublic,
                TryGetAccessor = (flags, objects, arg3) =>
                {
                    flags.ShouldEqual(default);
                    objects!.Count.ShouldEqual(0);
                    arg3.ShouldEqual(DefaultMetadata);
                    return accessor;
                },
                GetParameters = () => default
            };

            MemberManager.AddComponent(new TestMemberProviderComponent
            {
                TryGetMembers = (_, _, _, t, _) =>
                {
                    if (t == MemberType.Method)
                        return method;
                    return default;
                }
            });
            MemberManager.TryGetMembers(typeof(TestMethodInvoker), MemberType.Event, MemberFlags.All, $"{nameof(TestMethodInvoker.GetValueNoParameters)}()", DefaultMetadata)
                         .IsEmpty
                         .ShouldBeTrue();
            MemberManager.TryGetMembers(typeof(TestMethodInvoker), MemberType.Accessor, MemberFlags.All, $"{nameof(TestMethodInvoker.GetValueNoParameters)}()", DefaultMetadata)
                         .OfType<TestAccessorMemberInfo>()
                         .Single()
                         .ShouldEqual(accessor);
        }

        [Fact]
        public void TryGetMembersShouldHandleCustomMethodCallNullParameters()
        {
            var accessor = new TestAccessorMemberInfo();
            var method = new TestMethodMemberInfo
            {
                DeclaringType = typeof(object),
                MemberFlags = MemberFlags.InstancePublic,
                TryGetAccessor = (flags, objects, arg3) =>
                {
                    flags.ShouldEqual(default);
                    objects!.Count.ShouldEqual(1);
                    objects[0].ShouldBeNull();
                    arg3.ShouldEqual(DefaultMetadata);
                    return accessor;
                },
                GetParameters = () => new[] { new TestParameterInfo { ParameterType = typeof(string) } }
            };

            MemberManager.AddComponent(new TestMemberProviderComponent
            {
                TryGetMembers = (_, _, _, t, _) =>
                {
                    if (t == MemberType.Method)
                        return method;
                    return default;
                }
            });
            MemberManager.TryGetMembers(typeof(TestMethodInvoker), MemberType.Event, MemberFlags.All, $"{nameof(TestMethodInvoker.GetValueNull)}(null)", DefaultMetadata)
                         .IsEmpty
                         .ShouldBeTrue();
            MemberManager.TryGetMembers(typeof(TestMethodInvoker), MemberType.Accessor, MemberFlags.All, $"{nameof(TestMethodInvoker.GetValueNull)}(null)", DefaultMetadata)
                         .OfType<TestAccessorMemberInfo>()
                         .Single()
                         .ShouldEqual(accessor);
        }

        [Fact]
        public void TryGetMembersShouldHandleMethodCall()
        {
            const int index = 1;
            MemberManager.AddComponent(_reflectionMemberProvider);

            var members = MemberManager.TryGetMembers(typeof(TestMethodInvoker), MemberType.Accessor, MemberFlags.All,
                                           $"{nameof(TestMethodInvoker.GetValue)}({index.ToString(CultureInfo.InvariantCulture)})", DefaultMetadata)
                                       .OfType<MethodAccessorMemberInfo>()
                                       .ToList();
            members.Count.ShouldEqual(4);
            var member = members.Single(info => info.ArgumentFlags == default);
            members.Single(info => info.ArgumentFlags.HasFlag(ArgumentFlags.EmptyParamArray)).ShouldNotBeNull();
            members.Single(info => info.ArgumentFlags.HasFlag(ArgumentFlags.Optional)).ShouldNotBeNull();
            members.Single(info => info.ArgumentFlags.HasFlag(ArgumentFlags.Metadata)).ShouldNotBeNull();

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
        public void TryGetMembersShouldHandleMethodCallArray()
        {
            const int index1 = 2;
            var args = new[] { 2, 3, 4, 56 };
            MemberManager.AddComponent(_reflectionMemberProvider);

            var member = MemberManager.TryGetMembers(typeof(TestMethodInvoker), MemberType.Accessor, MemberFlags.All,
                                          $"{nameof(TestMethodInvoker.GetValue)}({index1}, {string.Join(",", args)})", DefaultMetadata)
                                      .OfType<MethodAccessorMemberInfo>()
                                      .Single();
            member.ArgumentFlags.HasFlag(ArgumentFlags.ParamArray).ShouldBeTrue();

            var getter = 0;
            var instance = new TestMethodInvoker
            {
                GetIndexParams = (i1, i2) =>
                {
                    ++getter;
                    i1.ShouldEqual(index1);
                    i2.ShouldEqual(args);
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
            MemberManager.AddComponent(_reflectionMemberProvider);

            var members = MemberManager.TryGetMembers(typeof(TestMethodInvoker), MemberType.Accessor, MemberFlags.All, $"{nameof(TestMethodInvoker.GetValue)}({index1})",
                                           DefaultMetadata)
                                       .OfType<MethodAccessorMemberInfo>()
                                       .ToList();
            members.Count.ShouldEqual(4);
            var member = members.Single(info => info.ArgumentFlags.HasFlag(ArgumentFlags.EmptyParamArray));
            members.Single(info => info.ArgumentFlags == default).ShouldNotBeNull();
            members.Single(info => info.ArgumentFlags.HasFlag(ArgumentFlags.Optional)).ShouldNotBeNull();
            members.Single(info => info.ArgumentFlags.HasFlag(ArgumentFlags.Metadata)).ShouldNotBeNull();

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
        public void TryGetMembersShouldHandleMethodCallMetadata()
        {
            const string index1 = "test";
            MemberManager.AddComponent(_reflectionMemberProvider);

            var members = MemberManager.TryGetMembers(typeof(TestMethodInvoker), MemberType.Accessor, MemberFlags.All, $"{nameof(TestMethodInvoker.GetValue)}('{index1}')",
                                           DefaultMetadata)
                                       .OfType<MethodAccessorMemberInfo>()
                                       .ToList();
            members.Count.ShouldEqual(2);
            var member = members.Single(info => info.ArgumentFlags.HasFlag(ArgumentFlags.Metadata));
            members.Single(info => info.ArgumentFlags.HasFlag(ArgumentFlags.Optional)).ShouldNotBeNull();

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

        [Fact]
        public void TryGetMembersShouldHandleMethodCallOptional()
        {
            const string index1 = "t1";
            MemberManager.AddComponent(_reflectionMemberProvider);

            var members = MemberManager.TryGetMembers(typeof(TestMethodInvoker), MemberType.Accessor, MemberFlags.All, $"{nameof(TestMethodInvoker.GetValue)}('{index1}')",
                                           DefaultMetadata)
                                       .OfType<MethodAccessorMemberInfo>()
                                       .ToList();
            members.Count.ShouldEqual(2);
            var member = members.Single(info => info.ArgumentFlags.HasFlag(ArgumentFlags.Optional));
            members.Single(info => info.ArgumentFlags.HasFlag(ArgumentFlags.Metadata)).ShouldNotBeNull();

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

        protected override IMemberManager GetMemberManager() => new MemberManager(ComponentCollectionManager);

        public class TestMethodInvoker
        {
            public const string OptionalValue = "index2";

            public Func<int>? GetValueRaw { get; set; }

            public Func<string?, string?>? GetValueNullRaw { get; set; }

            public Func<int, int>? GetIndex { get; set; }

            public Func<string, string, object>? GetIndexOptional { get; set; }

            public Func<string, IReadOnlyMetadataContext, object>? GetIndexMetadata { get; set; }

            public Func<int, int[], object>? GetIndexParams { get; set; }

            public int GetValueNoParameters() => GetValueRaw!();

            public string? GetValueNull(string? value) => GetValueNullRaw!(value);

            public int GetValue(int index) => GetIndex!(index);

            public object GetValue(string index1, string index2 = OptionalValue) => GetIndexOptional!(index1, index2);

            public object GetValue(string index1, IReadOnlyMetadataContext index2) => GetIndexMetadata!(index1, index2);

            public object GetValue(int index, params int[] indexes) => GetIndexParams!(index, indexes);
        }
    }
}