using System;
using System.Collections.Generic;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Interfaces.Resources;
using MugenMvvm.Bindings.Observation;
using MugenMvvm.Bindings.Observation.Observers;
using MugenMvvm.Bindings.Parsing.Expressions;
using MugenMvvm.Bindings.Parsing.Expressions.Binding;
using MugenMvvm.Bindings.Resources;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Tests.Bindings.Observation;
using MugenMvvm.Tests.Bindings.Parsing;
using MugenMvvm.Tests.Bindings.Resources;
using MugenMvvm.UnitTests.Bindings.Parsing.Internal;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Bindings.Parsing.Expressions.Binding
{
    [Collection(SharedContext)]
    public class BindingResourceMemberExpressionNodeTest : BindingMemberExpressionNodeBaseTest<BindingResourceMemberExpressionNode>
    {
        protected override IResourceManager GetResourceManager() => new ResourceManager(ComponentCollectionManager);

        public BindingResourceMemberExpressionNodeTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            RegisterDisposeToken(WithGlobalService(ResourceManager));
        }

        [Fact]
        public void ConstructorShouldInitializeValues()
        {
            var index = 1;
            EnumFlags<BindingMemberExpressionFlags> flags = BindingMemberExpressionFlags.Target;
            EnumFlags<MemberFlags> memberFlags = MemberFlags.Static;
            var e = ConstantExpressionNode.EmptyString;
            var observableMethodName = nameof(memberFlags);
            var exp = new BindingResourceMemberExpressionNode(ResourceName, Path, index, flags, memberFlags, observableMethodName, e, EmptyDictionary);
            exp.ExpressionType.ShouldEqual(ExpressionNodeType.BindingParameter);
            exp.ResourceName.ShouldEqual(ResourceName);
            exp.Path.ShouldEqual(Path);
            exp.Index.ShouldEqual(index);
            exp.Flags.ShouldEqual(flags);
            exp.MemberFlags.ShouldEqual(memberFlags);
            exp.Expression.ShouldEqual(e);
            exp.ObservableMethodName.ShouldEqual(observableMethodName);
            exp.Metadata.ShouldEqual(EmptyDictionary);
        }

        [Fact]
        public void GetBindingSourceShouldReturnRawValueEmptyPath()
        {
            var t = "r";
            var src = new object();
            var resource = new object();

            ResourceManager.AddComponent(new TestResourceResolverComponent
            {
                TryGetResource = (_, s, o, arg4) =>
                {
                    s.ShouldEqual(ResourceName);
                    o.ShouldEqual(t);
                    arg4.ShouldEqual(DefaultMetadata);
                    return new ResourceResolverResult(resource);
                }
            });
            ObservationManager.AddComponent(new TestMemberPathProviderComponent
            {
                TryGetMemberPath = (_, o, arg3) =>
                {
                    o.ShouldEqual("");
                    arg3.ShouldEqual(DefaultMetadata);
                    return MemberPath.Empty;
                }
            });

            var exp = new BindingResourceMemberExpressionNode(ResourceName, "", 0,
                BindingMemberExpressionFlags.Observable | BindingMemberExpressionFlags.Optional | BindingMemberExpressionFlags.StablePath |
                BindingMemberExpressionFlags.ObservableMethods, MemberFlags.All, "M");
            exp.GetBindingSource(t, src, DefaultMetadata).ShouldEqual(resource);
        }

        [Fact]
        public void GetBindingSourceShouldReturnResourceObserver()
        {
            var path = MemberPath.Get(Path);
            var observer = EmptyPathObserver.Empty;
            var t = "r";
            var src = new object();
            var resource = new object();


            ResourceManager.AddComponent(new TestResourceResolverComponent
            {
                TryGetResource = (_, s, o, arg4) =>
                {
                    s.ShouldEqual(ResourceName);
                    o.ShouldEqual(t);
                    arg4.ShouldEqual(DefaultMetadata);
                    return new ResourceResolverResult(resource);
                }
            });
            ObservationManager.AddComponent(new TestMemberPathProviderComponent
            {
                TryGetMemberPath = (_, o, arg3) =>
                {
                    o.ShouldEqual(Path);
                    arg3.ShouldEqual(DefaultMetadata);
                    return path;
                }
            });

            var exp = new BindingResourceMemberExpressionNode(ResourceName, Path, 0,
                BindingMemberExpressionFlags.Observable | BindingMemberExpressionFlags.Optional | BindingMemberExpressionFlags.StablePath |
                BindingMemberExpressionFlags.ObservableMethods,
                MemberFlags.All, "M");

            ObservationManager.AddComponent(new TestMemberPathObserverProviderComponent
            {
                TryGetMemberPathObserver = (_, target, req, arg4) =>
                {
                    target.ShouldEqual(resource);
                    var request = (MemberPathObserverRequest)req;
                    request.Expression.ShouldEqual(exp);
                    request.Path.ShouldEqual(path);
                    request.MemberFlags.ShouldEqual(exp.MemberFlags);
                    request.ObservableMethodName.ShouldEqual(exp.ObservableMethodName);
                    request.HasStablePath.ShouldBeTrue();
                    request.Optional.ShouldBeTrue();
                    request.Observable.ShouldBeTrue();
                    arg4.ShouldEqual(DefaultMetadata);
                    return observer;
                }
            });

            exp.GetBindingSource(t, src, DefaultMetadata).ShouldEqual(observer);
        }

        [Fact]
        public void GetBindingSourceShouldReturnResourceObserverDynamic()
        {
            var path = MemberPath.Get(Path);
            var observer = EmptyPathObserver.Empty;
            var t = "r";
            var src = new object();
            var resource = new TestDynamicResource { Value = new object() };

            ResourceManager.AddComponent(new TestResourceResolverComponent
            {
                TryGetResource = (_, s, o, arg4) =>
                {
                    s.ShouldEqual(ResourceName);
                    o.ShouldEqual(t);
                    arg4.ShouldEqual(DefaultMetadata);
                    return new ResourceResolverResult(resource);
                }
            });

            var exp = new BindingResourceMemberExpressionNode(ResourceName, Path, 0,
                BindingMemberExpressionFlags.Observable | BindingMemberExpressionFlags.Optional | BindingMemberExpressionFlags.StablePath |
                BindingMemberExpressionFlags.ObservableMethods,
                MemberFlags.All, "M");

            ObservationManager.AddComponent(new TestMemberPathProviderComponent
            {
                TryGetMemberPath = (_, o, arg3) =>
                {
                    o.ShouldEqual(nameof(IDynamicResource.Value) + "." + Path);
                    arg3.ShouldEqual(DefaultMetadata);
                    return path;
                }
            });
            ObservationManager.AddComponent(new TestMemberPathObserverProviderComponent
            {
                TryGetMemberPathObserver = (_, target, req, arg4) =>
                {
                    target.ShouldEqual(resource);
                    var request = (MemberPathObserverRequest)req;
                    request.Expression.ShouldEqual(exp);
                    request.Path.ShouldEqual(path);
                    request.MemberFlags.ShouldEqual(exp.MemberFlags);
                    request.ObservableMethodName.ShouldEqual(exp.ObservableMethodName);
                    request.HasStablePath.ShouldBeTrue();
                    request.Optional.ShouldBeTrue();
                    request.Observable.ShouldBeTrue();
                    arg4.ShouldEqual(DefaultMetadata);
                    return observer;
                }
            });

            exp.GetBindingSource(t, src, DefaultMetadata).ShouldEqual(observer);
        }

        [Fact]
        public void GetSourceShouldReturnResource()
        {
            var path = MemberPath.Get(Path);
            var t = "r";
            var src = new object();
            var resource = new object();


            ResourceManager.AddComponent(new TestResourceResolverComponent
            {
                TryGetResource = (_, s, o, arg4) =>
                {
                    s.ShouldEqual(ResourceName);
                    o.ShouldEqual(t);
                    arg4.ShouldEqual(DefaultMetadata);
                    return new ResourceResolverResult(resource);
                }
            });

            ObservationManager.AddComponent(new TestMemberPathProviderComponent
            {
                TryGetMemberPath = (_, o, arg3) =>
                {
                    o.ShouldEqual(Path);
                    arg3.ShouldEqual(DefaultMetadata);
                    return path;
                }
            });

            var exp = new BindingResourceMemberExpressionNode(ResourceName, Path, 0, default, MemberFlags.All);
            var target = exp.GetSource(t, src, DefaultMetadata, out var p);
            target.ShouldEqual(resource);
            p.ShouldEqual(path);
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(false, false)]
        public void GetHashCodeEqualsShouldBeValid(bool withComparer, bool hasTarget)
        {
            var comparer = withComparer ? new TestExpressionEqualityComparer() : null;
            var exp1 = new BindingResourceMemberExpressionNode("R", "P", 0, default, default, "M", hasTarget ? GetTestEqualityExpression(comparer, 1) : null,
                new Dictionary<string, object?> { { "k", null } });
            var exp2 = new BindingResourceMemberExpressionNode("R", "P", 0, default, default, "M", hasTarget ? GetTestEqualityExpression(comparer, 1) : null,
                new Dictionary<string, object?> { { "k", null } });
            if (hasTarget)
            {
                HashCode.Combine((GetBaseHashCode(exp1) * 397) ^ exp1.ResourceName.GetHashCode(), exp1.Index, exp1.Path, exp1.Flags.Value(), exp1.MemberFlags.Value(),
                            exp1.ObservableMethodName, 1)
                        .ShouldEqual(exp1.GetHashCode(comparer));
                ((TestExpressionNode)exp1.Expression!).GetHashCodeCount.ShouldEqual(1);
            }
            else
            {
                HashCode.Combine((GetBaseHashCode(exp1) * 397) ^ exp1.ResourceName.GetHashCode(), exp1.Index, exp1.Path, exp1.Flags.Value(), exp1.MemberFlags.Value(),
                            exp1.ObservableMethodName)
                        .ShouldEqual(exp1.GetHashCode(comparer));
            }

            exp1.Equals(exp2, comparer).ShouldBeTrue();
            ((TestExpressionNode?)exp1.Expression)?.EqualsCount.ShouldEqual(1);

            exp1.Equals(exp2.UpdateMetadata(null), comparer).ShouldBeFalse();
            exp1.Equals(
                    new BindingResourceMemberExpressionNode("RR", "P", 0, default, default, "M", hasTarget ? GetTestEqualityExpression(comparer, 1) : null,
                        new Dictionary<string, object?> { { "k", null } }), comparer)
                .ShouldBeFalse();
            exp1.Equals(exp2.Update(int.MaxValue, exp2.Flags, exp2.MemberFlags, exp2.ObservableMethodName), comparer).ShouldBeFalse();
            exp1.Equals(exp2.Update(exp2.Index, BindingMemberExpressionFlags.Target, exp2.MemberFlags, exp2.ObservableMethodName), comparer).ShouldBeFalse();
            exp1.Equals(exp2.Update(exp2.Index, exp2.Flags, MemberFlags.Instance, exp2.ObservableMethodName), comparer).ShouldBeFalse();
            exp1.Equals(exp2.Update(exp2.Index, exp2.Flags, exp2.MemberFlags, null), comparer).ShouldBeFalse();
            ((TestExpressionNode?)exp1.Expression)?.EqualsCount.ShouldEqual(1);

            if (comparer == null || !hasTarget)
                return;
            comparer.GetHashCode = node =>
            {
                ReferenceEquals(node, exp1).ShouldBeTrue();
                return int.MaxValue;
            };
            comparer.Equals = (x1, x2) =>
            {
                ReferenceEquals(x1, exp1).ShouldBeTrue();
                ReferenceEquals(x2, exp2).ShouldBeTrue();
                return false;
            };
            exp1.GetHashCode(comparer).ShouldEqual(int.MaxValue);
            exp1.Equals(exp2, comparer).ShouldBeFalse();
            ((TestExpressionNode)exp1.Expression!).EqualsCount.ShouldEqual(1);
        }

        protected override BindingResourceMemberExpressionNode GetExpression(IReadOnlyDictionary<string, object?>? metadata = null) =>
            new(ResourceName, Path, 0, default, default, metadata: metadata);
    }
}