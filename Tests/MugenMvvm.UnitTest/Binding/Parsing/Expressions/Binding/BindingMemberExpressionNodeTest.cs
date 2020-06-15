using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Observers;
using MugenMvvm.Binding.Observers.MemberPaths;
using MugenMvvm.Binding.Observers.PathObservers;
using MugenMvvm.Binding.Parsing.Expressions.Binding;
using MugenMvvm.Extensions;
using MugenMvvm.UnitTest.Binding.Observers.Internal;
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
            var observerProvider = new ObserverProvider();
            var component = new TestMemberPathProviderComponent
            {
                TryGetMemberPath = (o, type, arg3) =>
                {
                    o.ShouldEqual(expectedPath);
                    arg3.ShouldEqual(DefaultMetadata);
                    return path;
                }
            };
            observerProvider.AddComponent(component);

            var target = new object();
            var source = new object();

            var exp = new BindingMemberExpressionNode(inputPath, observerProvider)
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
            exp = new BindingMemberExpressionNode(inputPath, observerProvider)
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
            var observerProvider = new ObserverProvider();
            var exp = new BindingMemberExpressionNode(Path, observerProvider)
            {
                MemberFlags = MemberFlags.All,
                Flags = BindingMemberExpressionFlags.Observable | BindingMemberExpressionFlags.Optional | BindingMemberExpressionFlags.StablePath | BindingMemberExpressionFlags.ObservableMethod |
                        BindingMemberExpressionFlags.Target,
                ObservableMethodName = "M"
            };

            observerProvider.AddComponent(new TestMemberPathProviderComponent
            {
                TryGetMemberPath = (o, type, arg3) =>
                {
                    o.ShouldEqual(expectedPath);
                    arg3.ShouldEqual(DefaultMetadata);
                    return path;
                }
            });
            observerProvider.AddComponent(new TestMemberPathObserverProviderComponent
            {
                TryGetMemberPathObserver = (target, req, arg3, arg4) =>
                {
                    target.ShouldEqual(expectedTarget);
                    var request = (MemberPathObserverRequest) req;
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

            exp = new BindingMemberExpressionNode(Path, observerProvider)
            {
                MemberFlags = MemberFlags.All,
                Flags = BindingMemberExpressionFlags.Observable | BindingMemberExpressionFlags.Optional | BindingMemberExpressionFlags.StablePath | BindingMemberExpressionFlags.ObservableMethod,
                ObservableMethodName = "M"
            };
            expectedTarget = src;
            exp.GetBindingSource(t, src, DefaultMetadata).ShouldEqual(observer);

            expectedPath = $"DataContext.{Path}";
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