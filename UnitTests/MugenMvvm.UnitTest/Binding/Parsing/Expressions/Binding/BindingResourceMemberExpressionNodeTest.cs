using MugenMvvm.Binding.Core;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Observation;
using MugenMvvm.Binding.Observation.Observers;
using MugenMvvm.Binding.Observation.Paths;
using MugenMvvm.Binding.Parsing.Expressions.Binding;
using MugenMvvm.Binding.Resources;
using MugenMvvm.Extensions;
using MugenMvvm.UnitTest.Binding.Observation.Internal;
using MugenMvvm.UnitTest.Binding.Resources.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Binding.Parsing.Expressions.Binding
{
    public class BindingResourceMemberExpressionNodeTest : BindingMemberExpressionNodeBaseTest
    {
        #region Methods

        [Fact]
        public void ConstructorShouldInitializeValues()
        {
            var exp = new BindingResourceMemberExpressionNode(ResourceName, Path);
            exp.ExpressionType.ShouldEqual(ExpressionNodeType.BindingMember);
            exp.ResourceName.ShouldEqual(ResourceName);
            exp.Path.ShouldEqual(Path);
            exp.Index.ShouldEqual(-1);
        }

        [Fact]
        public void GetTargetSourceShouldReturnResource()
        {
            var path = new SingleMemberPath(Path);
            var t = "r";
            var src = new object();
            var resource = new TestResourceValue();

            var resourceResolver = new ResourceResolver();
            resourceResolver.AddComponent(new TestResourceResolverComponent
            {
                TryGetResourceValue = (s, o, arg3, arg4) =>
                {
                    s.ShouldEqual(ResourceName);
                    var state = (BindingTargetSourceState)o!;
                    state.Target.ShouldEqual(t);
                    state.Source.ShouldEqual(src);
                    arg4.ShouldEqual(DefaultMetadata);
                    return resource;
                }
            });

            var observerProvider = new ObservationManager();
            var component = new TestMemberPathProviderComponent
            {
                TryGetMemberPath = (o, type, arg3) =>
                {
                    o.ShouldEqual(Path);
                    arg3.ShouldEqual(DefaultMetadata);
                    return path;
                }
            };
            observerProvider.AddComponent(component);

            var exp = new BindingResourceMemberExpressionNode(ResourceName, Path, observerProvider, resourceResolver)
            {
                MemberFlags = MemberFlags.All
            };

            var target = exp.GetSource(t, src, DefaultMetadata, out var p, out var flags);
            target.ShouldEqual(resource);
            flags.ShouldEqual(MemberFlags.All);
            p.ShouldEqual(path);
        }

        [Fact]
        public void GetBindingTargetSourceShouldReturnResourceObserver()
        {
            var path = new SingleMemberPath(Path);
            var observer = EmptyPathObserver.Empty;
            var t = "r";
            var src = new object();
            var resource = new TestResourceValue();

            var resourceResolver = new ResourceResolver();
            resourceResolver.AddComponent(new TestResourceResolverComponent
            {
                TryGetResourceValue = (s, o, arg3, arg4) =>
                {
                    s.ShouldEqual(ResourceName);
                    var state = (BindingTargetSourceState)o!;
                    state.Target.ShouldEqual(t);
                    state.Source.ShouldEqual(src);
                    arg4.ShouldEqual(DefaultMetadata);
                    return resource;
                }
            });
            var observerProvider = new ObservationManager();

            var exp = new BindingResourceMemberExpressionNode(ResourceName, Path, observerProvider, resourceResolver)
            {
                MemberFlags = MemberFlags.All,
                Flags = BindingMemberExpressionFlags.Observable | BindingMemberExpressionFlags.Optional | BindingMemberExpressionFlags.StablePath | BindingMemberExpressionFlags.ObservableMethods,
                ObservableMethodName = "M"
            };

            observerProvider.AddComponent(new TestMemberPathProviderComponent
            {
                TryGetMemberPath = (o, type, arg3) =>
                {
                    o.ShouldEqual(Path);
                    arg3.ShouldEqual(DefaultMetadata);
                    return path;
                }
            });
            observerProvider.AddComponent(new TestMemberPathObserverProviderComponent
            {
                TryGetMemberPathObserver = (target, req, arg3, arg4) =>
                {
                    target.ShouldEqual(resource);
                    var request = (MemberPathObserverRequest)req;
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
            resource.IsStatic = true;
            exp.GetBindingSource(t, src, DefaultMetadata).ShouldBeNull();
        }

        protected override BindingMemberExpressionNodeBase GetExpression()
        {
            return new BindingResourceMemberExpressionNode(ResourceName, Path);
        }

        #endregion
    }
}