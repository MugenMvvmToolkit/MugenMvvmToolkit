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
            var exp = new BindingMemberExpressionNode(BindingMemberExpressionNode.TargetType.SourceOnly, Path);
            exp.ExpressionType.ShouldEqual(ExpressionNodeType.BindingMember);
            exp.Path.ShouldEqual(Path);
            exp.Index.ShouldEqual(-1);
            exp.Type.ShouldEqual(BindingMemberExpressionNode.TargetType.SourceOnly);
        }

        [Fact]
        public void GetTargetShouldRespectTargetType()
        {
            var path = new SingleMemberPath(Path);
            var observerProvider = new ObserverProvider();
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

            var target = new object();
            var source = new object();

            var exp = new BindingMemberExpressionNode(BindingMemberExpressionNode.TargetType.Default, Path, observerProvider)
            {
                MemberFlags = MemberFlags.All
            };
            exp.GetTarget(target, source, DefaultMetadata, out var p, out var flags).ShouldEqual(target);
            flags.ShouldEqual(MemberFlags.All);
            p.ShouldEqual(path);

            exp.GetTarget(target, null, DefaultMetadata, out p, out flags).ShouldEqual(target);
            flags.ShouldEqual(MemberFlags.All);
            p.ShouldEqual(path);

            exp = new BindingMemberExpressionNode(BindingMemberExpressionNode.TargetType.TargetOnly, Path, observerProvider)
            {
                MemberFlags = MemberFlags.All
            };
            exp.GetTarget(target, null, DefaultMetadata, out p, out flags).ShouldEqual(target);
            flags.ShouldEqual(MemberFlags.All);
            p.ShouldEqual(path);

            exp.GetTarget(target, source, DefaultMetadata, out p, out flags).ShouldEqual(target);
            flags.ShouldEqual(MemberFlags.All);
            p.ShouldEqual(path);

            exp = new BindingMemberExpressionNode(BindingMemberExpressionNode.TargetType.SourceOnly, Path, observerProvider)
            {
                MemberFlags = MemberFlags.All
            };
            exp.GetTarget(target, null, DefaultMetadata, out p, out flags).ShouldEqual(target);
            flags.ShouldEqual(MemberFlags.All);
            p.ShouldEqual(path);

            exp.GetTarget(target, source, DefaultMetadata, out p, out flags).ShouldEqual(source);
            flags.ShouldEqual(MemberFlags.All);
            p.ShouldEqual(path);
        }

        [Theory]
        [InlineData("Test", "DataContext.Test")]
        [InlineData("[test]", "DataContext[test]")]
        [InlineData("", "DataContext")]
        public void GetSourceShouldRespectTargetType(string inputPath, string expectedInputPath)
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

            var exp = new BindingMemberExpressionNode(BindingMemberExpressionNode.TargetType.Default, inputPath, observerProvider)
            {
                MemberFlags = MemberFlags.All
            };
            exp.GetSource(target, source, DefaultMetadata, out var p, out var flags).ShouldEqual(source);
            flags.ShouldEqual(MemberFlags.All);
            p.ShouldEqual(path);

            expectedPath = expectedInputPath;
            exp.GetSource(target, null, DefaultMetadata, out p, out flags).ShouldEqual(target);
            flags.ShouldEqual(MemberFlags.All);
            p.ShouldEqual(path);

            expectedPath = inputPath;
            exp = new BindingMemberExpressionNode(BindingMemberExpressionNode.TargetType.TargetOnly, inputPath, observerProvider)
            {
                MemberFlags = MemberFlags.All
            };
            exp.GetSource(target, null, DefaultMetadata, out p, out flags).ShouldEqual(target);
            flags.ShouldEqual(MemberFlags.All);
            p.ShouldEqual(path);

            exp.GetSource(target, source, DefaultMetadata, out p, out flags).ShouldEqual(target);
            flags.ShouldEqual(MemberFlags.All);
            p.ShouldEqual(path);

            exp = new BindingMemberExpressionNode(BindingMemberExpressionNode.TargetType.SourceOnly, inputPath, observerProvider)
            {
                MemberFlags = MemberFlags.All
            };
            exp.GetSource(target, null, DefaultMetadata, out p, out flags).ShouldEqual(target);
            flags.ShouldEqual(MemberFlags.All);
            p.ShouldEqual(path);

            exp.GetSource(target, source, DefaultMetadata, out p, out flags).ShouldEqual(source);
            flags.ShouldEqual(MemberFlags.All);
            p.ShouldEqual(path);
        }

        [Fact]
        public void GetBindingTargetShouldRespectTargetType()
        {
            var path = new SingleMemberPath(Path);
            var observer = EmptyPathObserver.Empty;
            var t = "r";
            var src = new object();
            object expectedTarget = t;
            var observerProvider = new ObserverProvider();
            var exp = new BindingMemberExpressionNode(BindingMemberExpressionNode.TargetType.Default, Path, observerProvider)
            {
                MemberFlags = MemberFlags.All,
                Flags = BindingMemberExpressionFlags.Observable | BindingMemberExpressionFlags.Optional | BindingMemberExpressionFlags.StablePath | BindingMemberExpressionFlags.ObservableMethod,
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

            exp.GetBindingTarget(t, src, DefaultMetadata).ShouldEqual(observer);
            exp.GetBindingTarget(t, null, DefaultMetadata).ShouldEqual(observer);

            exp = new BindingMemberExpressionNode(BindingMemberExpressionNode.TargetType.TargetOnly, Path, observerProvider)
            {
                MemberFlags = MemberFlags.All,
                Flags = BindingMemberExpressionFlags.Observable | BindingMemberExpressionFlags.Optional | BindingMemberExpressionFlags.StablePath | BindingMemberExpressionFlags.ObservableMethod,
                ObservableMethodName = "M"
            };
            exp.GetBindingTarget(t, src, DefaultMetadata).ShouldEqual(observer);
            exp.GetBindingTarget(t, null, DefaultMetadata).ShouldEqual(observer);

            exp = new BindingMemberExpressionNode(BindingMemberExpressionNode.TargetType.SourceOnly, Path, observerProvider)
            {
                MemberFlags = MemberFlags.All,
                Flags = BindingMemberExpressionFlags.Observable | BindingMemberExpressionFlags.Optional | BindingMemberExpressionFlags.StablePath | BindingMemberExpressionFlags.ObservableMethod,
                ObservableMethodName = "M"
            };
            exp.GetBindingTarget(t, null, DefaultMetadata).ShouldEqual(observer);
            expectedTarget = src;
            exp.GetBindingTarget(t, src, DefaultMetadata).ShouldEqual(observer);
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
            var exp = new BindingMemberExpressionNode(BindingMemberExpressionNode.TargetType.Default, Path, observerProvider)
            {
                MemberFlags = MemberFlags.All,
                Flags = BindingMemberExpressionFlags.Observable | BindingMemberExpressionFlags.Optional | BindingMemberExpressionFlags.StablePath | BindingMemberExpressionFlags.ObservableMethod,
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
            expectedPath = $"DataContext.{Path}";
            expectedTarget = t;
            exp.GetBindingSource(t, null, DefaultMetadata).ShouldEqual(observer);

            expectedPath = Path;
            exp = new BindingMemberExpressionNode(BindingMemberExpressionNode.TargetType.TargetOnly, Path, observerProvider)
            {
                MemberFlags = MemberFlags.All,
                Flags = BindingMemberExpressionFlags.Observable | BindingMemberExpressionFlags.Optional | BindingMemberExpressionFlags.StablePath | BindingMemberExpressionFlags.ObservableMethod,
                ObservableMethodName = "M"
            };
            exp.GetBindingSource(t, src, DefaultMetadata).ShouldEqual(observer);
            exp.GetBindingSource(t, null, DefaultMetadata).ShouldEqual(observer);

            exp = new BindingMemberExpressionNode(BindingMemberExpressionNode.TargetType.SourceOnly, Path, observerProvider)
            {
                MemberFlags = MemberFlags.All,
                Flags = BindingMemberExpressionFlags.Observable | BindingMemberExpressionFlags.Optional | BindingMemberExpressionFlags.StablePath | BindingMemberExpressionFlags.ObservableMethod,
                ObservableMethodName = "M"
            };
            exp.GetBindingSource(t, null, DefaultMetadata).ShouldEqual(observer);
            expectedTarget = src;
            exp.GetBindingSource(t, src, DefaultMetadata).ShouldEqual(observer);
        }

        protected override BindingMemberExpressionNodeBase GetExpression()
        {
            return new BindingMemberExpressionNode(BindingMemberExpressionNode.TargetType.Default, Path);
        }

        #endregion
    }
}