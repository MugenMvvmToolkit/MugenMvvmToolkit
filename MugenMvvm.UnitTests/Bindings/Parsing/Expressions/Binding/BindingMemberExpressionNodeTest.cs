using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Observation;
using MugenMvvm.Bindings.Observation.Observers;
using MugenMvvm.Bindings.Observation.Paths;
using MugenMvvm.Bindings.Parsing.Expressions.Binding;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.UnitTests.Bindings.Observation.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Bindings.Parsing.Expressions.Binding
{
    public class BindingMemberExpressionNodeTest : BindingMemberExpressionNodeBaseTest
    {
        #region Methods

        [Fact]
        public void ConstructorShouldInitializeValues()
        {
            var exp = new BindingMemberExpressionNode(Path);
            exp.ExpressionType.ShouldEqual(ExpressionNodeType.BindingMember);
            exp.Path.ShouldEqual(Path);
            exp.Index.ShouldEqual(-1);
        }

        [Theory]
        [InlineData("Test", "DataContext.Test")]
        [InlineData("[test]", "DataContext[test]")]
        [InlineData("", "DataContext")]
        public void GetSourceShouldRespectTargetType(string inputPath, string dataContextInputPath)
        {
            var expectedPath = inputPath;
            var path = new SingleMemberPath(Path);
            var observationManager = new ObservationManager();
            var component = new TestMemberPathProviderComponent
            {
                TryGetMemberPath = (o, arg3) =>
                {
                    o.ShouldEqual(expectedPath);
                    arg3.ShouldEqual(DefaultMetadata);
                    return path;
                }
            };
            observationManager.AddComponent(component);

            var target = new object();
            var source = new object();

            var exp = new BindingMemberExpressionNode(inputPath, observationManager)
            {
                MemberFlags = MemberFlags.All,
                Flags = BindingMemberExpressionFlags.Target
            };
            exp.GetSource(target, source, DefaultMetadata, out var p).ShouldEqual(target);
            p.ShouldEqual(path);

            exp.GetSource(target, null, DefaultMetadata, out p).ShouldEqual(target);
            p.ShouldEqual(path);

            expectedPath = inputPath;
            exp = new BindingMemberExpressionNode(inputPath, observationManager)
            {
                MemberFlags = MemberFlags.All
            };
            exp.GetSource(target, source, DefaultMetadata, out p).ShouldEqual(source);
            p.ShouldEqual(path);

            expectedPath = dataContextInputPath;
            exp.GetSource(target, null, DefaultMetadata, out p).ShouldEqual(target);
            p.ShouldEqual(path);
        }


        [Fact]
        public void GetBindingSourceShouldRespectTargetType()
        {
            var path = new SingleMemberPath(Path);
            var observer = EmptyPathObserver.Empty;
            var t = "r";
            var src = new object();
            object expectedTarget = src;
            var expectedPath = Path;
            var observationManager = new ObservationManager();
            var exp = new BindingMemberExpressionNode(Path, observationManager)
            {
                MemberFlags = MemberFlags.All,
                Flags = BindingMemberExpressionFlags.Observable | BindingMemberExpressionFlags.Optional | BindingMemberExpressionFlags.StablePath | BindingMemberExpressionFlags.ObservableMethods |
                        BindingMemberExpressionFlags.Target,
                ObservableMethodName = "M"
            };

            observationManager.AddComponent(new TestMemberPathProviderComponent
            {
                TryGetMemberPath = (o, arg3) =>
                {
                    o.ShouldEqual(expectedPath);
                    arg3.ShouldEqual(DefaultMetadata);
                    return path;
                }
            });
            observationManager.AddComponent(new TestMemberPathObserverProviderComponent
            {
                TryGetMemberPathObserver = (target, req, arg4) =>
                {
                    ((IWeakReference) target).Target.ShouldEqual(expectedTarget);
                    var request = (MemberPathObserverRequest) req;
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

            expectedTarget = t;
            exp.GetBindingSource(t, src, DefaultMetadata).ShouldEqual(observer);
            exp.GetBindingSource(t, null, DefaultMetadata).ShouldEqual(observer);

            exp = new BindingMemberExpressionNode(Path, observationManager)
            {
                MemberFlags = MemberFlags.All,
                Flags = BindingMemberExpressionFlags.Observable | BindingMemberExpressionFlags.Optional | BindingMemberExpressionFlags.StablePath | BindingMemberExpressionFlags.ObservableMethods,
                ObservableMethodName = "M"
            };
            expectedTarget = src;
            exp.GetBindingSource(t, src, DefaultMetadata).ShouldEqual(observer);

            expectedPath = $"DataContext.{Path}";
            expectedTarget = t;
            exp.GetBindingSource(t, null, DefaultMetadata).ShouldEqual(observer);

            exp = new BindingMemberExpressionNode(Path, observationManager)
            {
                MemberFlags = MemberFlags.All,
                Flags = BindingMemberExpressionFlags.Observable | BindingMemberExpressionFlags.Optional | BindingMemberExpressionFlags.StablePath | BindingMemberExpressionFlags.ObservableMethods |
                        BindingMemberExpressionFlags.DataContextPath,
                ObservableMethodName = "M"
            };
            expectedPath = $"Parent.DataContext.{Path}";
            expectedTarget = t;
            exp.GetBindingSource(t, null, DefaultMetadata).ShouldEqual(observer);
        }

        protected override BindingMemberExpressionNodeBase GetExpression() => new BindingMemberExpressionNode(Path);

        #endregion
    }
}