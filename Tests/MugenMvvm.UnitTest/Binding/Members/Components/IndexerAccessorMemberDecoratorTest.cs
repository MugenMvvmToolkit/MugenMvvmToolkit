using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Extensions;
using MugenMvvm.Binding.Interfaces.Members.Components;
using MugenMvvm.Binding.Members;
using MugenMvvm.Binding.Members.Components;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Binding.Members.Components
{
    public class IndexerAccessorMemberDecoratorTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void TryGetMembersShouldIgnoreNonIndexerMembers()
        {
            var decorator = new IndexerAccessorMemberDecorator();
            var component = new ReflectionMemberProviderComponent();
            ((IDecoratorComponentCollectionComponent<IMemberProviderComponent>)decorator).Decorate(new List<IMemberProviderComponent> { decorator, component }, DefaultMetadata);

            decorator.TryGetMembers(typeof(string), "get_Chars", DefaultMetadata).Item.ShouldNotBeNull();
        }

        [Fact]
        public void TryGetMembersShouldHandleArray()
        {
            var manager = new MemberManager();
            var decorator = new IndexerAccessorMemberDecorator();
            var component = new ReflectionMemberProviderComponent();
            var selector = new MemberSelectorComponent();
            manager.AddComponent(decorator);
            manager.AddComponent(component);
            manager.AddComponent(selector);
            ((IDecoratorComponentCollectionComponent<IMemberProviderComponent>)decorator).Decorate(new List<IMemberProviderComponent> { decorator, component }, DefaultMetadata);

            var member = (MethodMemberAccessorInfo)decorator.TryGetMembers(typeof(int[]), "[1]", DefaultMetadata).Item!;
            member.ShouldNotBeNull();

            var array = new[] { 1, 2 };
            member.GetValue(array, DefaultMetadata).ShouldEqual(array[1]);

            array = new[] { 1, 5 };
            member.GetValue(array, DefaultMetadata).ShouldEqual(array[1]);

            array = new[] { 1, 2 };
            member.SetValue(array, int.MaxValue, DefaultMetadata);
            array[1].ShouldEqual(int.MaxValue);
        }

        [Fact]
        public void TryGetMembersShouldHandleStringIndexer()
        {
            var manager = new MemberManager();
            var decorator = new IndexerAccessorMemberDecorator();
            var component = new ReflectionMemberProviderComponent();
            var selector = new MemberSelectorComponent();
            manager.AddComponent(decorator);
            manager.AddComponent(component);
            manager.AddComponent(selector);
            ((IDecoratorComponentCollectionComponent<IMemberProviderComponent>)decorator).Decorate(new List<IMemberProviderComponent> { decorator, component }, DefaultMetadata);

            var member = (MethodMemberAccessorInfo)decorator.TryGetMembers(typeof(string), "[1]", DefaultMetadata).Item!;
            member.ShouldNotBeNull();

            var value = "12";
            member.GetValue(value, DefaultMetadata).ShouldEqual(value[1]);

            value = "13";
            member.GetValue(value, DefaultMetadata).ShouldEqual(value[1]);

            ShouldThrow<InvalidOperationException>(() => member.SetValue(value, int.MaxValue, DefaultMetadata));
        }

        [Fact]
        public void TryGetMembersShouldHandleIndexer()
        {
            const int index = 1;
            var manager = new MemberManager();
            var decorator = new IndexerAccessorMemberDecorator();
            var component = new ReflectionMemberProviderComponent();
            var selector = new MemberSelectorComponent();
            manager.AddComponent(decorator);
            manager.AddComponent(component);
            manager.AddComponent(selector);
            ((IDecoratorComponentCollectionComponent<IMemberProviderComponent>)decorator).Decorate(new List<IMemberProviderComponent> { decorator, component }, DefaultMetadata);

            var member = decorator.TryGetMembers(typeof(TestIndexer), $"[{index.ToString(CultureInfo.InvariantCulture)}]", DefaultMetadata)
                .ToArray()
                .OfType<MethodMemberAccessorInfo>()
                .Single(info => info.ArgumentFlags == 0);

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
            member.GetValue(indexerTest, DefaultMetadata).ShouldEqual(index);
            getter.ShouldEqual(1);
            setter.ShouldEqual(0);

            member.SetValue(indexerTest, indexValue, DefaultMetadata);
            getter.ShouldEqual(1);
            setter.ShouldEqual(1);
        }

        [Fact]
        public void TryGetMembersShouldHandleIndexerOptional()
        {
            const string index1 = "t1";
            var manager = new MemberManager();
            var decorator = new IndexerAccessorMemberDecorator();
            var component = new ReflectionMemberProviderComponent();
            var selector = new MemberSelectorComponent();
            manager.AddComponent(decorator);
            manager.AddComponent(component);
            manager.AddComponent(selector);
            ((IDecoratorComponentCollectionComponent<IMemberProviderComponent>)decorator).Decorate(new List<IMemberProviderComponent> { decorator, component }, DefaultMetadata);

            var member = decorator.TryGetMembers(typeof(TestIndexer), $"['{index1}']", DefaultMetadata)
                .ToArray()
                .OfType<MethodMemberAccessorInfo>()
                .Single(info => info.ArgumentFlags.HasFlagEx(ArgumentFlags.Optional));

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
            member.GetValue(indexerTest, DefaultMetadata).ShouldEqual(index1);
            getter.ShouldEqual(1);
            setter.ShouldEqual(0);

            member.SetValue(indexerTest, indexValue, DefaultMetadata);
            getter.ShouldEqual(1);
            setter.ShouldEqual(1);
        }

        [Fact]
        public void TryGetMembersShouldHandleIndexerEmptyArray()
        {
            const int index1 = 2;
            var manager = new MemberManager();
            var decorator = new IndexerAccessorMemberDecorator();
            var component = new ReflectionMemberProviderComponent();
            var selector = new MemberSelectorComponent();
            manager.AddComponent(decorator);
            manager.AddComponent(component);
            manager.AddComponent(selector);
            ((IDecoratorComponentCollectionComponent<IMemberProviderComponent>)decorator).Decorate(new List<IMemberProviderComponent> { decorator, component }, DefaultMetadata);

            var member = decorator.TryGetMembers(typeof(TestIndexer), $"[{index1}]", DefaultMetadata)
                .ToArray()
                .OfType<MethodMemberAccessorInfo>()
                .Single(info => info.ArgumentFlags.HasFlagEx(ArgumentFlags.EmptyParamArray));

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
            member.GetValue(indexerTest, DefaultMetadata).ShouldEqual(index1);
            getter.ShouldEqual(1);
            setter.ShouldEqual(0);

            member.SetValue(indexerTest, indexValue, DefaultMetadata);
            getter.ShouldEqual(1);
            setter.ShouldEqual(1);
        }

        [Fact]
        public void TryGetMembersShouldHandleIndexerArray()
        {
            const int index1 = 2;
            var args = new[] { 2, 3, 4, 56 };
            var manager = new MemberManager();
            var decorator = new IndexerAccessorMemberDecorator();
            var component = new ReflectionMemberProviderComponent();
            var selector = new MemberSelectorComponent();
            manager.AddComponent(decorator);
            manager.AddComponent(component);
            manager.AddComponent(selector);
            ((IDecoratorComponentCollectionComponent<IMemberProviderComponent>)decorator).Decorate(new List<IMemberProviderComponent> { decorator, component }, DefaultMetadata);

            var member = decorator.TryGetMembers(typeof(TestIndexer), $"[{index1}, {string.Join(",", args)}]", DefaultMetadata)
                .ToArray()
                .OfType<MethodMemberAccessorInfo>()
                .Single(info => info.ArgumentFlags.HasFlagEx(ArgumentFlags.ParamArray));

            var indexValue = 2;
            var getter = 0;
            var setter = 0;
            var indexerTest = new TestIndexer
            {
                GetIndexParams = (i1, i2) =>
                {
                    ++getter;
                    i1.ShouldEqual(index1);
                    i2.SequenceEqual(args).ShouldBeTrue();
                    return index1;
                },
                SetIndexParams = (i1, i2, value) =>
                {
                    ++setter;
                    i1.ShouldEqual(index1);
                    i2.SequenceEqual(args).ShouldBeTrue();
                    indexValue.ShouldEqual(value);
                }
            };
            member.GetValue(indexerTest, DefaultMetadata).ShouldEqual(index1);
            getter.ShouldEqual(1);
            setter.ShouldEqual(0);

            member.SetValue(indexerTest, indexValue, DefaultMetadata);
            getter.ShouldEqual(1);
            setter.ShouldEqual(1);
        }

        [Fact]
        public void TryGetMembersShouldHandleIndexerMetadata()
        {
            const string index1 = "test";
            var manager = new MemberManager();
            var decorator = new IndexerAccessorMemberDecorator();
            var component = new ReflectionMemberProviderComponent();
            var selector = new MemberSelectorComponent();
            manager.AddComponent(decorator);
            manager.AddComponent(component);
            manager.AddComponent(selector);
            ((IDecoratorComponentCollectionComponent<IMemberProviderComponent>)decorator).Decorate(new List<IMemberProviderComponent> { decorator, component }, DefaultMetadata);

            var member = decorator.TryGetMembers(typeof(TestIndexer), $"['{index1}']", DefaultMetadata)
                .ToArray()
                .OfType<MethodMemberAccessorInfo>()
                .Single(info => info.ArgumentFlags.HasFlagEx(ArgumentFlags.Metadata));

            var indexValue = 2;
            var getter = 0;
            var setter = 0;
            var indexerTest = new TestIndexer
            {
                GetIndexMetadata = (i1, i2) =>
                {
                    ++getter;
                    i1.ShouldEqual(index1);
                    i2.ShouldEqual(DefaultMetadata);
                    return index1;
                },
                SetIndexMetadata = (i1, i2, value) =>
                {
                    ++setter;
                    i1.ShouldEqual(index1);
                    i2.ShouldEqual(DefaultMetadata);
                    indexValue.ShouldEqual(value);
                }
            };
            member.GetValue(indexerTest, DefaultMetadata).ShouldEqual(index1);
            getter.ShouldEqual(1);
            setter.ShouldEqual(0);

            member.SetValue(indexerTest, indexValue, DefaultMetadata);
            getter.ShouldEqual(1);
            setter.ShouldEqual(1);
        }

        #endregion

        #region Nested types

        public class TestIndexer
        {
            #region Fields

            public const string OptionalValue = "index2";

            #endregion

            #region Properties

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

            #endregion
        }

        #endregion
    }
}