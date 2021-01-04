using System;
using System.Collections.Generic;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Observation;
using MugenMvvm.Bindings.Observation.Observers;
using MugenMvvm.Bindings.Observation.Paths;
using MugenMvvm.Bindings.Parsing.Expressions;
using MugenMvvm.Bindings.Parsing.Expressions.Binding;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.UnitTests.Bindings.Observation.Internal;
using MugenMvvm.UnitTests.Bindings.Parsing.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Bindings.Parsing.Expressions.Binding
{
    public class BindingMemberExpressionNodeTest : BindingMemberExpressionNodeBaseTest<BindingMemberExpressionNode>
    {
        #region Methods

        [Fact]
        public void ConstructorShouldInitializeValues()
        {
            int index = 1;
            EnumFlags<BindingMemberExpressionFlags> flags = BindingMemberExpressionFlags.Target;
            EnumFlags<MemberFlags> memberFlags = MemberFlags.Static;
            var e = ConstantExpressionNode.EmptyString;
            var observableMethodName = nameof(memberFlags);
            var exp = new BindingMemberExpressionNode(Path, index, flags, memberFlags, observableMethodName, e, EmptyDictionary);
            exp.ExpressionType.ShouldEqual(ExpressionNodeType.BindingParameter);
            exp.Path.ShouldEqual(Path);
            exp.Index.ShouldEqual(index);
            exp.Flags.ShouldEqual(flags);
            exp.MemberFlags.ShouldEqual(memberFlags);
            exp.Expression.ShouldEqual(e);
            exp.ObservableMethodName.ShouldEqual(observableMethodName);
            exp.Metadata.ShouldEqual(EmptyDictionary);
        }

        [Theory]
        [InlineData("Test", "DataContext.Test")]
        [InlineData("[test]", "DataContext[test]")]
        [InlineData("", "DataContext")]
        public void GetSourceShouldRespectTargetType(string inputPath, string dataContextInputPath)
        {
            var expectedPath = inputPath;
            var path = new SingleMemberPath(Path);

            using var t = MugenService.AddComponent(new TestMemberPathProviderComponent
            {
                TryGetMemberPath = (o, arg3) =>
                {
                    o.ShouldEqual(expectedPath);
                    arg3.ShouldEqual(DefaultMetadata);
                    return path;
                }
            });

            var target = new object();
            var source = new object();

            var exp = new BindingMemberExpressionNode(inputPath, 0, BindingMemberExpressionFlags.Target, MemberFlags.All);
            exp.GetSource(target, source, DefaultMetadata, out var p).ShouldEqual(target);
            p.ShouldEqual(path);

            exp.GetSource(target, null, DefaultMetadata, out p).ShouldEqual(target);
            p.ShouldEqual(path);

            expectedPath = inputPath;
            exp = new BindingMemberExpressionNode(inputPath, 0, default, MemberFlags.All);
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
            var exp = new BindingMemberExpressionNode(Path, 0, BindingMemberExpressionFlags.Observable | BindingMemberExpressionFlags.Optional | BindingMemberExpressionFlags.StablePath |
                                                               BindingMemberExpressionFlags.ObservableMethods |
                                                               BindingMemberExpressionFlags.Target, MemberFlags.All, "M");

            using var t1 = MugenService.AddComponent(new TestMemberPathProviderComponent
            {
                TryGetMemberPath = (o, arg3) =>
                {
                    o.ShouldEqual(expectedPath);
                    arg3.ShouldEqual(DefaultMetadata);
                    return path;
                }
            });
            using var t2 = MugenService.AddComponent(new TestMemberPathObserverProviderComponent
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

            exp = new BindingMemberExpressionNode(Path, 0,
                BindingMemberExpressionFlags.Observable | BindingMemberExpressionFlags.Optional | BindingMemberExpressionFlags.StablePath | BindingMemberExpressionFlags.ObservableMethods, MemberFlags.All, "M");
            expectedTarget = src;
            exp.GetBindingSource(t, src, DefaultMetadata).ShouldEqual(observer);

            expectedPath = $"DataContext.{Path}";
            expectedTarget = t;
            exp.GetBindingSource(t, null, DefaultMetadata).ShouldEqual(observer);

            exp = new BindingMemberExpressionNode(Path, 0, BindingMemberExpressionFlags.Observable | BindingMemberExpressionFlags.Optional | BindingMemberExpressionFlags.StablePath |
                                                           BindingMemberExpressionFlags.ObservableMethods |
                                                           BindingMemberExpressionFlags.ParentDataContext, MemberFlags.All, "M");
            expectedPath = $"Parent.DataContext.{Path}";
            expectedTarget = t;
            exp.GetBindingSource(t, null, DefaultMetadata).ShouldEqual(observer);
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(false, false)]
        public void GetHashCodeEqualsShouldBeValid(bool withComparer, bool hasTarget)
        {
            var comparer = withComparer ? new TestExpressionEqualityComparer() : null;
            var exp1 = new BindingMemberExpressionNode("P", 0, default, default, "M", hasTarget ? GetTestEqualityExpression(comparer, 1) : null, new Dictionary<string, object?> {{"k", null}});
            var exp2 = new BindingMemberExpressionNode("P", 0, default, default, "M", hasTarget ? GetTestEqualityExpression(comparer, 1) : null, new Dictionary<string, object?> {{"k", null}});
            if (hasTarget)
            {
                HashCode.Combine(GetBaseHashCode(exp1), exp1.Index, exp1.Path, exp1.Flags.Value(), exp1.MemberFlags.Value(), exp1.ObservableMethodName, 1)
                    .ShouldEqual(exp1.GetHashCode(comparer));
                ((TestExpressionNode) exp1.Expression!).GetHashCodeCount.ShouldEqual(1);
            }
            else
                HashCode.Combine(GetBaseHashCode(exp1), exp1.Index, exp1.Path, exp1.Flags.Value(), exp1.MemberFlags.Value(), exp1.ObservableMethodName)
                    .ShouldEqual(exp1.GetHashCode(comparer));

            exp1.Equals(exp2, comparer).ShouldBeTrue();
            ((TestExpressionNode?) exp1.Expression)?.EqualsCount.ShouldEqual(1);

            exp1.Equals(exp2.UpdateMetadata(null), comparer).ShouldBeFalse();
            exp1.Equals(exp2.Update(int.MaxValue, exp2.Flags, exp2.MemberFlags, exp2.ObservableMethodName), comparer).ShouldBeFalse();
            exp1.Equals(exp2.Update(exp2.Index, BindingMemberExpressionFlags.Target, exp2.MemberFlags, exp2.ObservableMethodName), comparer).ShouldBeFalse();
            exp1.Equals(exp2.Update(exp2.Index, exp2.Flags, MemberFlags.Instance, exp2.ObservableMethodName), comparer).ShouldBeFalse();
            exp1.Equals(exp2.Update(exp2.Index, exp2.Flags, exp2.MemberFlags, null), comparer).ShouldBeFalse();
            ((TestExpressionNode?) exp1.Expression)?.EqualsCount.ShouldEqual(1);

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
            ((TestExpressionNode) exp1.Expression!).EqualsCount.ShouldEqual(1);
        }

        protected override BindingMemberExpressionNode GetExpression(IReadOnlyDictionary<string, object?>? metadata = null) => new(Path, 0, default, default, metadata: metadata);

        #endregion
    }
}