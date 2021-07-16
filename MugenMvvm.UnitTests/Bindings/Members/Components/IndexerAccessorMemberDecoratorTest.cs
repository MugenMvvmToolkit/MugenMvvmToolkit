using System;
using System.Globalization;
using System.Linq;
using MugenMvvm.Bindings.Constants;
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
    public class IndexerAccessorMemberDecoratorTest : UnitTestBase
    {
        public IndexerAccessorMemberDecoratorTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            MemberManager.AddComponent(new IndexerAccessorMemberDecorator(GlobalValueConverter));
            MemberManager.AddComponent(new ReflectionMemberProvider(ObservationManager));
            MemberManager.AddComponent(new NameRequestMemberManagerDecorator());
            MemberManager.AddComponent(TestMemberManagerComponent.Selector);
            RegisterDisposeToken(WithGlobalService(ReflectionManager));
        }

        [Fact]
        public void TryGetMembersShouldHandleArray()
        {
            MemberManager.TryGetMembers(typeof(int[]), MemberType.Method, MemberFlags.All, "[1]", Metadata).IsEmpty.ShouldBeTrue();

            var member = (MethodAccessorMemberInfo)MemberManager.TryGetMembers(typeof(int[]), MemberType.Accessor, MemberFlags.All, "[1]", Metadata).Item!;
            member.ShouldNotBeNull();

            var array = new[] { 1, 2 };
            member.GetValue(array, Metadata).ShouldEqual(array[1]);

            array = new[] { 1, 5 };
            member.GetValue(array, Metadata).ShouldEqual(array[1]);

            array = new[] { 1, 2 };
            member.SetValue(array, int.MaxValue, Metadata);
            array[1].ShouldEqual(int.MaxValue);
        }

        [Fact]
        public void TryGetMembersShouldHandleIndexer()
        {
            const int index = 1;

            var members = MemberManager
                          .TryGetMembers(typeof(TestIndexer), MemberType.Accessor, MemberFlags.All, $"[{index.ToString(CultureInfo.InvariantCulture)}]", Metadata)
                          .OfType<MethodAccessorMemberInfo>()
                          .ToList();
            members.Count.ShouldEqual(4);
            var member = members.Single(info => info.ArgumentFlags == default);
            members.Single(info => info.ArgumentFlags.HasFlag(ArgumentFlags.EmptyParamArray)).ShouldNotBeNull();
            members.Single(info => info.ArgumentFlags.HasFlag(ArgumentFlags.Optional)).ShouldNotBeNull();
            members.Single(info => info.ArgumentFlags.HasFlag(ArgumentFlags.Metadata)).ShouldNotBeNull();

            var indexValue = 2;
            var getter = 0;
            var setter = 0;
            var indexerTest = new TestIndexer
            {
                GetIndex = i =>
                {
                    ++getter;
                    i.ShouldEqual(index);
                    return index;
                },
                SetIndex = (i, value) =>
                {
                    ++setter;
                    i.ShouldEqual(index);
                    indexValue.ShouldEqual(value);
                }
            };
            member.GetValue(indexerTest, Metadata).ShouldEqual(index);
            getter.ShouldEqual(1);
            setter.ShouldEqual(0);

            member.SetValue(indexerTest, indexValue, Metadata);
            getter.ShouldEqual(1);
            setter.ShouldEqual(1);
        }

        [Fact]
        public void TryGetMembersShouldHandleIndexerArray()
        {
            const int index1 = 2;
            var args = new[] { 2, 3, 4, 56 };

            var member = MemberManager.TryGetMembers(typeof(TestIndexer), MemberType.Accessor, MemberFlags.All, $"[{index1}, {string.Join(" , ", args)}]", Metadata)
                                      .OfType<MethodAccessorMemberInfo>()
                                      .Single();
            member.ArgumentFlags.HasFlag(ArgumentFlags.ParamArray).ShouldBeTrue();

            var indexValue = 2;
            var getter = 0;
            var setter = 0;
            var indexerTest = new TestIndexer
            {
                GetIndexParams = (i1, i2) =>
                {
                    ++getter;
                    i1.ShouldEqual(index1);
                    i2.ShouldEqual(args);
                    return index1;
                },
                SetIndexParams = (i1, i2, value) =>
                {
                    ++setter;
                    i1.ShouldEqual(index1);
                    i2.ShouldEqual(args);
                    indexValue.ShouldEqual(value);
                }
            };
            member.GetValue(indexerTest, Metadata).ShouldEqual(index1);
            getter.ShouldEqual(1);
            setter.ShouldEqual(0);

            member.SetValue(indexerTest, indexValue, Metadata);
            getter.ShouldEqual(1);
            setter.ShouldEqual(1);
        }

        [Fact]
        public void TryGetMembersShouldHandleIndexerEmptyArray()
        {
            const int index1 = 2;
            var members = MemberManager.TryGetMembers(typeof(TestIndexer), MemberType.Accessor, MemberFlags.All, $"[{index1}]", Metadata)
                                       .OfType<MethodAccessorMemberInfo>()
                                       .ToList();
            members.Count.ShouldEqual(4);
            var member = members.Single(info => info.ArgumentFlags.HasFlag(ArgumentFlags.EmptyParamArray));
            members.Single(info => info.ArgumentFlags == default).ShouldNotBeNull();
            members.Single(info => info.ArgumentFlags.HasFlag(ArgumentFlags.Optional)).ShouldNotBeNull();
            members.Single(info => info.ArgumentFlags.HasFlag(ArgumentFlags.Metadata)).ShouldNotBeNull();

            var indexValue = 2;
            var getter = 0;
            var setter = 0;
            var indexerTest = new TestIndexer
            {
                GetIndexParams = (i1, i2) =>
                {
                    ++getter;
                    i1.ShouldEqual(index1);
                    i2.ShouldBeEmpty();
                    return index1;
                },
                SetIndexParams = (i1, i2, value) =>
                {
                    ++setter;
                    i1.ShouldEqual(index1);
                    i2.ShouldBeEmpty();
                    indexValue.ShouldEqual(value);
                }
            };
            member.GetValue(indexerTest, Metadata).ShouldEqual(index1);
            getter.ShouldEqual(1);
            setter.ShouldEqual(0);

            member.SetValue(indexerTest, indexValue, Metadata);
            getter.ShouldEqual(1);
            setter.ShouldEqual(1);
        }

        [Fact]
        public void TryGetMembersShouldHandleIndexerMetadata()
        {
            const string index1 = "test";
            var members = MemberManager.TryGetMembers(typeof(TestIndexer), MemberType.Accessor, MemberFlags.All, $"['{index1}']", Metadata)
                                       .OfType<MethodAccessorMemberInfo>()
                                       .ToList();
            members.Count.ShouldEqual(2);
            var member = members.Single(info => info.ArgumentFlags.HasFlag(ArgumentFlags.Metadata));
            members.Single(info => info.ArgumentFlags.HasFlag(ArgumentFlags.Optional)).ShouldNotBeNull();

            var indexValue = 2;
            var getter = 0;
            var setter = 0;
            var indexerTest = new TestIndexer
            {
                GetIndexMetadata = (i1, i2) =>
                {
                    ++getter;
                    i1.ShouldEqual(index1);
                    i2.ShouldEqual(Metadata);
                    return index1;
                },
                SetIndexMetadata = (i1, i2, value) =>
                {
                    ++setter;
                    i1.ShouldEqual(index1);
                    i2.ShouldEqual(Metadata);
                    indexValue.ShouldEqual(value);
                }
            };
            member.GetValue(indexerTest, Metadata).ShouldEqual(index1);
            getter.ShouldEqual(1);
            setter.ShouldEqual(0);

            member.SetValue(indexerTest, indexValue, Metadata);
            getter.ShouldEqual(1);
            setter.ShouldEqual(1);
        }

        [Fact]
        public void TryGetMembersShouldHandleIndexerOptional()
        {
            const string index1 = "t1";
            var members = MemberManager.TryGetMembers(typeof(TestIndexer), MemberType.Accessor, MemberFlags.All, $"['{index1}']", Metadata)
                                       .OfType<MethodAccessorMemberInfo>()
                                       .ToList();
            members.Count.ShouldEqual(2);
            var member = members.Single(info => info.ArgumentFlags.HasFlag(ArgumentFlags.Optional));
            members.Single(info => info.ArgumentFlags.HasFlag(ArgumentFlags.Metadata)).ShouldNotBeNull();

            var indexValue = 2;
            var getter = 0;
            var setter = 0;
            var indexerTest = new TestIndexer
            {
                GetIndexOptional = (i1, i2) =>
                {
                    ++getter;
                    i1.ShouldEqual(index1);
                    i2.ShouldEqual(TestIndexer.OptionalValue);
                    return index1;
                },
                SetIndexOptional = (i1, i2, value) =>
                {
                    ++setter;
                    i1.ShouldEqual(index1);
                    i2.ShouldEqual(TestIndexer.OptionalValue);
                    indexValue.ShouldEqual(value);
                }
            };
            member.GetValue(indexerTest, Metadata).ShouldEqual(index1);
            getter.ShouldEqual(1);
            setter.ShouldEqual(0);

            member.SetValue(indexerTest, indexValue, Metadata);
            getter.ShouldEqual(1);
            setter.ShouldEqual(1);
        }

        [Fact]
        public void TryGetMembersShouldHandleStringIndexer()
        {
            var member = (MethodAccessorMemberInfo)MemberManager.TryGetMembers(typeof(string), MemberType.Accessor, MemberFlags.All, "[1]", Metadata).Item!;
            member.ShouldNotBeNull();

            var value = "12";
            member.GetValue(value, Metadata).ShouldEqual(value[1]);

            value = "13";
            member.GetValue(value, Metadata).ShouldEqual(value[1]);

            ShouldThrow<InvalidOperationException>(() => member.SetValue(value, int.MaxValue, Metadata));
        }

        [Fact]
        public void TryGetMembersShouldIgnoreNonIndexerMembers() =>
            MemberManager.TryGetMembers(typeof(string), MemberType.Accessor, MemberFlags.All, BindingInternalConstant.IndexerGetterName, Metadata).IsEmpty.ShouldBeTrue();

        protected override IMemberManager GetMemberManager() => new MemberManager(ComponentCollectionManager);

        public class TestIndexer
        {
            public const string OptionalValue = "index2";

            public Func<int, int>? GetIndex { get; set; }

            public Action<int, int>? SetIndex { get; set; }

            public Func<string, string, object>? GetIndexOptional { get; set; }

            public Action<string, string, object>? SetIndexOptional { get; set; }

            public Func<string, IReadOnlyMetadataContext, object>? GetIndexMetadata { get; set; }

            public Action<string, IReadOnlyMetadataContext, object>? SetIndexMetadata { get; set; }

            public Func<int, int[], object>? GetIndexParams { get; set; }

            public Action<int, int[], object>? SetIndexParams { get; set; }

            public int this[int index]
            {
                get => GetIndex!(index);
                set => SetIndex!(index, value);
            }

            public object this[string index1, string index2 = OptionalValue]
            {
                get => GetIndexOptional!(index1, index2);
                set => SetIndexOptional!(index1, index2, value);
            }

            public object this[string index1, IReadOnlyMetadataContext index2]
            {
                get => GetIndexMetadata!(index1, index2);
                set => SetIndexMetadata!(index1, index2, value);
            }

            public object this[int index, params int[] indexes]
            {
                get => GetIndexParams!(index, indexes);
                set => SetIndexParams!(index, indexes, value);
            }
        }
    }
}