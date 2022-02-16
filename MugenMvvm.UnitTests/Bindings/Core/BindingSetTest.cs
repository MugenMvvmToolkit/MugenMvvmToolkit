using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Bindings.Core;
using MugenMvvm.Bindings.Delegates;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Bindings.Interfaces.Core;
using MugenMvvm.Bindings.Parsing;
using MugenMvvm.Collections;
using MugenMvvm.Extensions;
using MugenMvvm.Tests.Bindings.Core;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Bindings.Core
{
    public class BindingSetTest : UnitTestBase
    {
        private static readonly BindingExpressionRequest ConverterRequest = new("", null, default);
        private static readonly BindingBuilderDelegate<object, object> Delegate = target => ConverterRequest;

        public BindingSetTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
        }

        [Fact]
        public void BindShouldBuildBinding1()
        {
            var target = this;
            object? source = null;
            var testBuilder = new TestBindingBuilder
            {
                Build = (o, o1, arg3) =>
                {
                    o.ShouldEqual(target);
                    o1.ShouldEqual(source);
                    arg3.ShouldEqual(Metadata);
                    return Binding;
                }
            };

            var invokeCount = 0;
            BindingManager.AddComponent(new TestBindingExpressionParserComponent
            {
                TryParseBindingExpression = (_, o, arg3) =>
                {
                    ++invokeCount;
                    o.ShouldEqual(Delegate.ToBindingBuilderDelegate());
                    arg3.ShouldEqual(Metadata);
                    return testBuilder;
                }
            });

            var bindingSet = new BindingSet<object>(source, BindingManager);
            bindingSet.Bind(target, Delegate, Metadata);
            bindingSet.Build(Metadata).Item.ShouldEqual(Binding);
            invokeCount.ShouldEqual(1);
        }

        [Fact]
        public void BindShouldBuildBinding2()
        {
            var target = this;
            var source = new object();
            var testBuilder = new TestBindingBuilder
            {
                Build = (o, o1, arg3) =>
                {
                    o.ShouldEqual(target);
                    o1.ShouldEqual(source);
                    arg3.ShouldEqual(Metadata);
                    return Binding;
                }
            };

            var invokeCount = 0;
            BindingManager.AddComponent(new TestBindingExpressionParserComponent
            {
                TryParseBindingExpression = (_, o, arg3) =>
                {
                    ++invokeCount;
                    o.ShouldEqual(Delegate.ToBindingBuilderDelegate());
                    arg3.ShouldEqual(Metadata);
                    return testBuilder;
                }
            });

            var bindingSet = new BindingSet<object>(BindingManager);
            bindingSet.Bind(target, source, Delegate, Metadata);
            bindingSet.Build(Metadata).Item.ShouldEqual(Binding);
            invokeCount.ShouldEqual(1);
        }

        [Fact]
        public void BindShouldBuildBinding3()
        {
            var request = "Test";
            var target = this;
            var source = "";
            var testBuilder = new TestBindingBuilder
            {
                Build = (o, o1, arg3) =>
                {
                    o.ShouldEqual(target);
                    o1.ShouldEqual(source);
                    arg3.ShouldEqual(Metadata);
                    return Binding;
                }
            };

            var invokeCount = 0;
            BindingManager.AddComponent(new TestBindingExpressionParserComponent
            {
                TryParseBindingExpression = (_, o, arg3) =>
                {
                    ++invokeCount;
                    o.ShouldEqual(request);
                    arg3.ShouldEqual(Metadata);
                    return testBuilder;
                }
            });

            var bindingSet = new BindingSet<object>(BindingManager);
            bindingSet.Bind(target, request, source, Metadata);
            bindingSet.Build(Metadata).Item.ShouldEqual(Binding);
            invokeCount.ShouldEqual(1);
        }

        [Fact]
        public void BindShouldBuildBinding4()
        {
            var request = "Test";
            var target = this;
            var source = "";
            var testBuilder = new TestBindingBuilder
            {
                Build = (o, o1, arg3) =>
                {
                    o.ShouldEqual(target);
                    o1.ShouldEqual(source);
                    arg3.ShouldEqual(Metadata);
                    return Binding;
                }
            };

            var invokeCount = 0;
            BindingManager.AddComponent(new TestBindingExpressionParserComponent
            {
                TryParseBindingExpression = (_, o, arg3) =>
                {
                    ++invokeCount;
                    o.ShouldEqual(request);
                    arg3.ShouldEqual(Metadata);
                    return testBuilder;
                }
            });

            var bindingSet = new BindingSet<object>(source, BindingManager);
            bindingSet.Bind(target, request, source: null, Metadata);
            bindingSet.Build(Metadata).Item.ShouldEqual(Binding);
            invokeCount.ShouldEqual(1);
        }

        [Theory]
        [InlineData(10, 1)]
        [InlineData(10, 10)]
        [InlineData(1, 10)]
        [InlineData(1, 1)]
        public void BuildIncludeBindingsShouldHandleListOfTargets(int count, int bindingCount)
        {
            var invokeBuilderCount = 0;
            var list = new List<(object target, object source, TestBindingBuilder builder, TestBinding binding, string request)>();
            for (var i = 0; i < count; i++)
            {
                var target = new object();
                var source = new object();
                var binding = new TestBinding();
                var testBuilder = new TestBindingBuilder
                {
                    Build = (o, o1, arg3) =>
                    {
                        ++invokeBuilderCount;
                        o.ShouldEqual(target);
                        o1.ShouldEqual(source);
                        arg3.ShouldEqual(Metadata);
                        return binding;
                    }
                };

                list.Add((target, source, testBuilder, binding, i.ToString()));
            }

            var sortCount = 0;
            BindingManager.AddComponent(new TestBindingExpressionParserComponent
            {
                TryParseBindingExpression = (_, o, arg3) =>
                {
                    if (o is IReadOnlyList<IBindingBuilder> builders)
                    {
                        ++sortCount;
                        return ItemOrIReadOnlyList.FromList(builders);
                    }

                    return ItemOrIReadOnlyList.FromItem<IBindingBuilder>(list.Single(tuple => tuple.request.Equals(o)).builder);
                }
            });

            var bindingSet = new BindingSet<object>(BindingManager);
            for (var i = 0; i < bindingCount; i++)
            {
                foreach (var valueTuple in list)
                    bindingSet.Bind(valueTuple.target, valueTuple.request, valueTuple.source, Metadata);
            }

            var bindings = bindingSet.Build(Metadata);
            invokeBuilderCount.ShouldEqual(count * bindingCount);
            sortCount.ShouldEqual(bindingCount > 1 ? count : 0);
            var groupBy = bindings.GroupBy(binding => binding);
            groupBy.Count().ShouldEqual(count);
            foreach (var group in groupBy)
                group.Count().ShouldEqual(bindingCount);

            bindingSet.Build(Metadata).IsEmpty.ShouldBeTrue();
        }

        [Theory]
        [InlineData(10, 1)]
        [InlineData(10, 10)]
        [InlineData(1, 10)]
        [InlineData(1, 1)]
        public void BuildShouldHandleListOfTargets(int count, int bindingCount)
        {
            var invokeBuilderCount = 0;
            var list = new List<(object target, object source, TestBindingBuilder builder, TestBinding binding, string request)>();
            for (var i = 0; i < count; i++)
            {
                var target = new object();
                var source = new object();
                var binding = new TestBinding();
                var testBuilder = new TestBindingBuilder
                {
                    Build = (o, o1, arg3) =>
                    {
                        ++invokeBuilderCount;
                        o.ShouldEqual(target);
                        o1.ShouldEqual(source);
                        arg3.ShouldEqual(Metadata);
                        return binding;
                    }
                };

                list.Add((target, source, testBuilder, binding, i.ToString()));
            }

            var sortCount = 0;
            BindingManager.AddComponent(new TestBindingExpressionParserComponent
            {
                TryParseBindingExpression = (_, o, arg3) =>
                {
                    if (o is IReadOnlyList<IBindingBuilder> builders)
                    {
                        ++sortCount;
                        return ItemOrIReadOnlyList.FromList(builders);
                    }

                    return ItemOrIReadOnlyList.FromItem<IBindingBuilder>(list.Single(tuple => tuple.request.Equals(o)).builder);
                }
            });

            var bindingSet = new BindingSet<object>(BindingManager);
            for (var i = 0; i < bindingCount; i++)
            {
                foreach (var valueTuple in list)
                    bindingSet.Bind(valueTuple.target, valueTuple.request, valueTuple.source, Metadata);
            }

            bindingSet.Build(Metadata);
            invokeBuilderCount.ShouldEqual(count * bindingCount);
            sortCount.ShouldEqual(bindingCount > 1 ? count : 0);
            bindingSet.Dispose();
        }

        [Theory]
        [InlineData(10, 1)]
        [InlineData(10, 10)]
        [InlineData(1, 10)]
        [InlineData(1, 1)]
        public void DisposeShouldHandleListOfTargets(int count, int bindingCount)
        {
            var invokeBuilderCount = 0;
            var list = new List<(object target, object source, TestBindingBuilder builder, TestBinding binding, string request)>();
            for (var i = 0; i < count; i++)
            {
                var target = new object();
                var source = new object();
                var binding = new TestBinding();
                var testBuilder = new TestBindingBuilder
                {
                    Build = (o, o1, arg3) =>
                    {
                        ++invokeBuilderCount;
                        o.ShouldEqual(target);
                        o1.ShouldEqual(source);
                        arg3.ShouldBeNull();
                        return binding;
                    }
                };

                list.Add((target, source, testBuilder, binding, i.ToString()));
            }

            var sortCount = 0;
            BindingManager.AddComponent(new TestBindingExpressionParserComponent
            {
                TryParseBindingExpression = (_, o, arg3) =>
                {
                    if (o is IReadOnlyList<IBindingBuilder> builders)
                    {
                        ++sortCount;
                        return ItemOrIReadOnlyList.FromList(builders);
                    }

                    return ItemOrIReadOnlyList.FromItem<IBindingBuilder>(list.Single(tuple => tuple.request.Equals(o)).builder);
                }
            });

            var bindingSet = new BindingSet<object>(BindingManager);
            for (var i = 0; i < bindingCount; i++)
            {
                foreach (var valueTuple in list)
                    bindingSet.Bind(valueTuple.target, valueTuple.request, valueTuple.source, Metadata);
            }

            bindingSet.Dispose();
            invokeBuilderCount.ShouldEqual(count * bindingCount);
            sortCount.ShouldEqual(bindingCount > 1 ? count : 0);
            bindingSet.Dispose();
        }

        protected override IBindingManager GetBindingManager() => new BindingManager(ComponentCollectionManager);
    }
}