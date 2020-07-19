using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Observation;
using MugenMvvm.Binding.Observation.Observers;
using MugenMvvm.Binding.Observation.Paths;
using MugenMvvm.Binding.Parsing.Expressions.Binding;
using MugenMvvm.Extensions;
using MugenMvvm.UnitTest.Binding.Observation.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Binding.Parsing.Expressions.Binding
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
            exp.GetSource(target, source, DefaultMetadata, out var p, out var flags).ShouldEqual(target);
            flags.ShouldEqual(MemberFlags.All);
            p.ShouldEqual(path);

            exp.GetSource(target, null, DefaultMetadata, out p, out flags).ShouldEqual(target);
            flags.ShouldEqual(MemberFlags.All);
            p.ShouldEqual(path);

            expectedPath = inputPath;
            exp = new BindingMemberExpressionNode(inputPath, observationManager)
            {
                MemberFlags = MemberFlags.All
            };
            exp.GetSource(target, source, DefaultMetadata, out p, out flags).ShouldEqual(source);
            flags.ShouldEqual(MemberFlags.All);
            p.ShouldEqual(path);

            expectedPath = dataContextInputPath;
            exp.GetSource(target, null, DefaultMetadata, out p, out flags).ShouldEqual(target);
            flags.ShouldEqual(MemberFlags.All);
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
                    target.ShouldEqual(expectedTarget);
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

        protected override BindingMemberExpressionNodeBase GetExpression()
        {
            return new BindingMemberExpressionNode(Path);
        }

        #endregion
    }
}