using System;
using MugenMvvm.Bindings.Constants;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Bindings.Members;
using MugenMvvm.Bindings.Observation;
using MugenMvvm.Bindings.Parsing.Expressions;
using MugenMvvm.Bindings.Parsing.Expressions.Binding;
using MugenMvvm.Bindings.Parsing.Visitors;
using MugenMvvm.Bindings.Resources;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Internal;
using MugenMvvm.UnitTests.Bindings.Members.Internal;
using MugenMvvm.UnitTests.Bindings.Observation.Internal;
using MugenMvvm.UnitTests.Bindings.Resources.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Bindings.Parsing.Visitors
{
    public class BindingMemberExpressionVisitorTest : UnitTestBase
    {
        private const string MemberName = "Test1";
        private const string MethodName = "Test2";
        private const string MemberName2 = "Test3";
        private const string TypeName = "T";
        private const string ResourceName = "R";

        [Fact]
        public void VisitShouldCacheIndexerMember()
        {
            var expression = new BinaryExpressionNode(BinaryTokenType.Addition, new IndexExpressionNode(null, new[] {ConstantExpressionNode.Null}),
                new IndexExpressionNode(null, new[] {ConstantExpressionNode.Null}));
            var visitor = new BindingMemberExpressionVisitor();
            var expressionNode = (BinaryExpressionNode) visitor.Visit(expression, true, DefaultMetadata);
            ReferenceEquals(expressionNode.Left, expressionNode.Right).ShouldBeTrue();
        }

        [Fact]
        public void VisitShouldCacheMember()
        {
            var expression = new BinaryExpressionNode(BinaryTokenType.Addition, new MemberExpressionNode(new MemberExpressionNode(null, MemberName2), MemberName),
                new MemberExpressionNode(new MemberExpressionNode(null, MemberName2), MemberName));
            var visitor = new BindingMemberExpressionVisitor();
            var expressionNode = (BinaryExpressionNode) visitor.Visit(expression, true, DefaultMetadata);
            ReferenceEquals(expressionNode.Left, expressionNode.Right).ShouldBeTrue();
        }

        [Fact]
        public void VisitShouldCacheMethodCallMember()
        {
            var expression = new BinaryExpressionNode(BinaryTokenType.Addition, new MethodCallExpressionNode(null, MethodName, Default.Array<IExpressionNode>()),
                new MethodCallExpressionNode(null, MethodName, Default.Array<IExpressionNode>()));
            var visitor = new BindingMemberExpressionVisitor();
            var expressionNode = (BinaryExpressionNode) visitor.Visit(expression, true, DefaultMetadata);
            ReferenceEquals(expressionNode.Left, expressionNode.Right).ShouldBeTrue();
        }

        [Fact]
        public void VisitShouldCacheResourceMember()
        {
            var expression = new BinaryExpressionNode(BinaryTokenType.Addition,
                new UnaryExpressionNode(UnaryTokenType.DynamicExpression, new MemberExpressionNode(null, ResourceName)),
                new UnaryExpressionNode(UnaryTokenType.DynamicExpression, new MemberExpressionNode(null, ResourceName)));
            var visitor = new BindingMemberExpressionVisitor();
            var expressionNode = (BinaryExpressionNode) visitor.Visit(expression, true, DefaultMetadata);
            ReferenceEquals(expressionNode.Left, expressionNode.Right).ShouldBeTrue();
        }

        [Fact]
        public void VisitShouldCacheResourceMemberStatic()
        {
            var resolver = new ResourceManager();
            resolver.AddComponent(new TestResourceResolverComponent
            {
                TryGetResource = (s, o, arg4) => new ResourceResolverResult(1)
            });
            var observationManager = new ObservationManager();
            observationManager.AddComponent(new TestMemberPathProviderComponent
            {
                TryGetMemberPath = (o, arg3) => MemberPath.Empty
            });

            var expression = new BinaryExpressionNode(BinaryTokenType.Addition,
                new UnaryExpressionNode(UnaryTokenType.StaticExpression, new MemberExpressionNode(null, ResourceName)),
                new UnaryExpressionNode(UnaryTokenType.StaticExpression, new MemberExpressionNode(null, ResourceName)));
            var visitor = new BindingMemberExpressionVisitor(observationManager, resolver);
            var expressionNode = (BinaryExpressionNode) visitor.Visit(expression, true, DefaultMetadata);
            ReferenceEquals(expressionNode.Left, expressionNode.Right).ShouldBeTrue();
        }

        [Fact]
        public void VisitShouldCacheTypeMember()
        {
            var resolver = new ResourceManager();
            resolver.AddComponent(new TestTypeResolverComponent
            {
                TryGetType = (s, o, arg4) => typeof(object)
            });

            var expression = new BinaryExpressionNode(BinaryTokenType.Addition, new UnaryExpressionNode(UnaryTokenType.DynamicExpression, new MemberExpressionNode(null, TypeName)),
                new UnaryExpressionNode(UnaryTokenType.DynamicExpression, new MemberExpressionNode(null, TypeName)));
            var visitor = new BindingMemberExpressionVisitor(null, resolver);
            var expressionNode = (BinaryExpressionNode) visitor.Visit(expression, true, DefaultMetadata);
            ReferenceEquals(expressionNode.Left, expressionNode.Right).ShouldBeTrue();
        }

        [Fact]
        public void VisitShouldCacheTypeMemberStatic()
        {
            var resolver = new ResourceManager();
            resolver.AddComponent(new TestTypeResolverComponent
            {
                TryGetType = (s, o, arg4) => typeof(string)
            });
            var observationManager = new ObservationManager();
            observationManager.AddComponent(new TestMemberPathProviderComponent
            {
                TryGetMemberPath = (o, arg3) => MemberPath.Empty
            });

            var expression = new BinaryExpressionNode(BinaryTokenType.Addition, new UnaryExpressionNode(UnaryTokenType.StaticExpression, new MemberExpressionNode(null, TypeName)),
                new UnaryExpressionNode(UnaryTokenType.StaticExpression, new MemberExpressionNode(null, TypeName)));
            var visitor = new BindingMemberExpressionVisitor(observationManager, resolver);
            var expressionNode = (BinaryExpressionNode) visitor.Visit(expression, true, DefaultMetadata);
            ReferenceEquals(expressionNode.Left, expressionNode.Right).ShouldBeTrue();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void VisitShouldHandleMethodCall1(bool isTarget)
        {
            var expression = new MethodCallExpressionNode(null, MethodName, Default.Array<IExpressionNode>());
            var visitor = new BindingMemberExpressionVisitor {MemberFlags = MemberFlags.All, Flags = BindingMemberExpressionFlags.Observable};
            visitor.Visit(expression, isTarget, DefaultMetadata)
                   .ShouldEqual(new BindingMemberExpressionNode($"{expression.Method}()", 0, visitor.Flags.SetTargetFlags(isTarget),
                       visitor.MemberFlags.SetInstanceOrStaticFlags(false), null, expression));

            visitor.SuppressMethodAccessors = true;
            visitor.Visit(expression, isTarget, DefaultMetadata)
                   .ShouldEqual(expression.UpdateTarget(new BindingMemberExpressionNode(string.Empty, 0, visitor.Flags.SetTargetFlags(isTarget),
                       visitor.MemberFlags.SetInstanceOrStaticFlags(false), MethodName,
                       expression)));

            visitor.SuppressMethodAccessors = false;
            expression = expression.UpdateMetadata(BindingParameterNameConstant.SuppressMethodAccessors, BoxingExtensions.TrueObject);
            visitor.Visit(expression, isTarget, DefaultMetadata)
                   .ShouldEqual(expression.UpdateTarget(new BindingMemberExpressionNode(string.Empty, 0, visitor.Flags.SetTargetFlags(isTarget),
                       visitor.MemberFlags.SetInstanceOrStaticFlags(false), MethodName,
                       expression)));
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void VisitShouldHandleMethodCall2(bool isTarget)
        {
            var expression = new MethodCallExpressionNode(null, MethodName, new[] {ConstantExpressionNode.Get(1), ConstantExpressionNode.Get(2)});
            var visitor = new BindingMemberExpressionVisitor {MemberFlags = MemberFlags.All, Flags = BindingMemberExpressionFlags.Observable};
            visitor.Visit(expression, isTarget, DefaultMetadata)
                   .ShouldEqual(new BindingMemberExpressionNode($"{expression.Method}(1,2)", 0, visitor.Flags.SetTargetFlags(isTarget),
                       visitor.MemberFlags.SetInstanceOrStaticFlags(false), null, expression));

            visitor.SuppressMethodAccessors = true;
            visitor.Visit(expression, isTarget, DefaultMetadata)
                   .ShouldEqual(expression.UpdateTarget(new BindingMemberExpressionNode(string.Empty, 0, visitor.Flags.SetTargetFlags(isTarget),
                       visitor.MemberFlags.SetInstanceOrStaticFlags(false), MethodName,
                       expression)));

            visitor.SuppressMethodAccessors = false;
            expression = expression.UpdateMetadata(BindingParameterNameConstant.SuppressMethodAccessors, BoxingExtensions.TrueObject);
            visitor.Visit(expression, isTarget, DefaultMetadata)
                   .ShouldEqual(expression.UpdateTarget(new BindingMemberExpressionNode(string.Empty, 0, visitor.Flags.SetTargetFlags(isTarget),
                       visitor.MemberFlags.SetInstanceOrStaticFlags(false), MethodName,
                       expression)));
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void VisitShouldHandleMethodCall3(bool isTarget)
        {
            var expression = new MethodCallExpressionNode(new MemberExpressionNode(null, MemberName), MethodName,
                new[] {ConstantExpressionNode.Get(1), ConstantExpressionNode.Get(2)});
            var visitor = new BindingMemberExpressionVisitor {MemberFlags = MemberFlags.All, Flags = BindingMemberExpressionFlags.Observable};
            visitor.Visit(expression, isTarget, DefaultMetadata).ShouldEqual(new BindingMemberExpressionNode($"{MemberName}.{expression.Method}(1,2)", 0,
                visitor.Flags.SetTargetFlags(isTarget),
                visitor.MemberFlags.SetInstanceOrStaticFlags(false), null, expression));

            visitor.SuppressMethodAccessors = true;
            visitor.Visit(expression, isTarget, DefaultMetadata).ShouldEqual(expression.UpdateTarget(new BindingMemberExpressionNode(MemberName, 0,
                visitor.Flags.SetTargetFlags(isTarget),
                visitor.MemberFlags.SetInstanceOrStaticFlags(false), MethodName, expression.Target)));

            visitor.SuppressMethodAccessors = false;
            expression = expression.UpdateMetadata(BindingParameterNameConstant.SuppressMethodAccessors, BoxingExtensions.TrueObject);
            visitor.Visit(expression, isTarget, DefaultMetadata).ShouldEqual(expression.UpdateTarget(new BindingMemberExpressionNode(MemberName, 0,
                visitor.Flags.SetTargetFlags(isTarget),
                visitor.MemberFlags.SetInstanceOrStaticFlags(false), MethodName, expression.Target)));
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void VisitShouldHandleMethodCall4(bool isTarget)
        {
            var expression = new MethodCallExpressionNode(new IndexExpressionNode(new MemberExpressionNode(null, MemberName), new[] {ConstantExpressionNode.Get(1)}), MethodName,
                new[]
                {
                    ConstantExpressionNode.Get(1), ConstantExpressionNode.Get(2)
                });
            var visitor = new BindingMemberExpressionVisitor {MemberFlags = MemberFlags.All, Flags = BindingMemberExpressionFlags.Observable};
            visitor.Visit(expression, isTarget, DefaultMetadata).ShouldEqual(new BindingMemberExpressionNode($"{MemberName}[1].{expression.Method}(1,2)", 0,
                visitor.Flags.SetTargetFlags(isTarget),
                visitor.MemberFlags.SetInstanceOrStaticFlags(false), null, expression));

            visitor.SuppressMethodAccessors = true;
            visitor.Visit(expression, isTarget, DefaultMetadata).ShouldEqual(expression.UpdateTarget(new BindingMemberExpressionNode($"{MemberName}[1]", 0,
                visitor.Flags.SetTargetFlags(isTarget),
                visitor.MemberFlags.SetInstanceOrStaticFlags(false), MethodName, expression.Target)));

            visitor.SuppressMethodAccessors = false;
            expression = expression.UpdateMetadata(BindingParameterNameConstant.SuppressMethodAccessors, BoxingExtensions.TrueObject);
            visitor.Visit(expression, isTarget, DefaultMetadata).ShouldEqual(expression.UpdateTarget(new BindingMemberExpressionNode($"{MemberName}[1]", 0,
                visitor.Flags.SetTargetFlags(isTarget),
                visitor.MemberFlags.SetInstanceOrStaticFlags(false), MethodName, expression.Target)));
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void VisitShouldHandleIndexer1(bool isTarget)
        {
            var expression = new IndexExpressionNode(null, new[] {ConstantExpressionNode.Get(1), ConstantExpressionNode.Get(2)});
            var visitor = new BindingMemberExpressionVisitor {MemberFlags = MemberFlags.All, Flags = BindingMemberExpressionFlags.Observable};
            visitor.Visit(expression, isTarget, DefaultMetadata)
                   .ShouldEqual(new BindingMemberExpressionNode("[1,2]", 0, visitor.Flags.SetTargetFlags(isTarget), visitor.MemberFlags.SetInstanceOrStaticFlags(false), null,
                       expression));

            visitor.SuppressIndexAccessors = true;
            visitor.Visit(expression, isTarget, DefaultMetadata)
                   .ShouldEqual(expression.UpdateTarget(new BindingMemberExpressionNode(string.Empty, 0, visitor.Flags.SetTargetFlags(isTarget),
                       visitor.MemberFlags.SetInstanceOrStaticFlags(false), null, expression)));

            visitor.SuppressIndexAccessors = false;
            expression = expression.UpdateMetadata(BindingParameterNameConstant.SuppressIndexAccessors, BoxingExtensions.TrueObject);
            visitor.Visit(expression, isTarget, DefaultMetadata)
                   .ShouldEqual(expression.UpdateTarget(new BindingMemberExpressionNode(string.Empty, 0, visitor.Flags.SetTargetFlags(isTarget),
                       visitor.MemberFlags.SetInstanceOrStaticFlags(false), null, expression)));
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void VisitShouldHandleIndexer2(bool isTarget)
        {
            var expression = new IndexExpressionNode(new MemberExpressionNode(null, MemberName), new[] {ConstantExpressionNode.Get(1), ConstantExpressionNode.Get(2)});
            var visitor = new BindingMemberExpressionVisitor {MemberFlags = MemberFlags.All, Flags = BindingMemberExpressionFlags.Observable};
            visitor.Visit(expression, isTarget, DefaultMetadata)
                   .ShouldEqual(new BindingMemberExpressionNode($"{MemberName}[1,2]", 0, visitor.Flags.SetTargetFlags(isTarget),
                       visitor.MemberFlags.SetInstanceOrStaticFlags(false), null, expression));

            visitor.SuppressIndexAccessors = true;
            visitor.Visit(expression, isTarget, DefaultMetadata)
                   .ShouldEqual(expression.UpdateTarget(new BindingMemberExpressionNode(MemberName, 0, visitor.Flags.SetTargetFlags(isTarget),
                       visitor.MemberFlags.SetInstanceOrStaticFlags(false), null, expression.Target)));

            visitor.SuppressIndexAccessors = false;
            expression = expression.UpdateMetadata(BindingParameterNameConstant.SuppressIndexAccessors, BoxingExtensions.TrueObject);
            visitor.Visit(expression, isTarget, DefaultMetadata)
                   .ShouldEqual(expression.UpdateTarget(new BindingMemberExpressionNode(MemberName, 0, visitor.Flags.SetTargetFlags(isTarget),
                       visitor.MemberFlags.SetInstanceOrStaticFlags(false), null, expression.Target)));
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void VisitShouldHandleMember1(bool isTarget)
        {
            var expression = new MemberExpressionNode(null, MemberName);
            var visitor = new BindingMemberExpressionVisitor {MemberFlags = MemberFlags.All, Flags = BindingMemberExpressionFlags.Observable};
            visitor.Visit(expression, isTarget, DefaultMetadata)
                   .ShouldEqual(new BindingMemberExpressionNode(MemberName, 0, visitor.Flags.SetTargetFlags(isTarget), visitor.MemberFlags.SetInstanceOrStaticFlags(false), null,
                       expression));
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void VisitShouldHandleMember2(bool isTarget)
        {
            var expression = new MemberExpressionNode(new MemberExpressionNode(null, MemberName2), MemberName);
            var visitor = new BindingMemberExpressionVisitor {MemberFlags = MemberFlags.All, Flags = BindingMemberExpressionFlags.Observable};
            visitor.Visit(expression, isTarget, DefaultMetadata)
                   .ShouldEqual(new BindingMemberExpressionNode($"{MemberName2}.{MemberName}", 0, visitor.Flags.SetTargetFlags(isTarget),
                       visitor.MemberFlags.SetInstanceOrStaticFlags(false), null, expression));
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void VisitShouldHandleMember3(bool isTarget)
        {
            var expression = MemberExpressionNode.Empty;
            var visitor = new BindingMemberExpressionVisitor {MemberFlags = MemberFlags.All, Flags = BindingMemberExpressionFlags.Observable};
            visitor.Visit(expression, isTarget, DefaultMetadata)
                   .ShouldEqual(new BindingMemberExpressionNode("", 0, visitor.Flags.SetTargetFlags(isTarget), visitor.MemberFlags.SetInstanceOrStaticFlags(false), null,
                       expression));
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void VisitShouldHandleMember4(bool isTarget)
        {
            EnumFlags<MemberFlags> memberFlags1 = MemberFlags.Attached;
            EnumFlags<MemberFlags> memberFlags2 = MemberFlags.Dynamic;
            EnumFlags<BindingMemberExpressionFlags> bindingMemberFlags1 = BindingMemberExpressionFlags.Target;
            EnumFlags<BindingMemberExpressionFlags> bindingMemberFlags2 = BindingMemberExpressionFlags.Optional;
            var target = new MemberExpressionNode(null, MemberName2).UpdateMetadata(BindingParameterNameConstant.MemberFlags, memberFlags2,
                BindingParameterNameConstant.BindingMemberFlags, bindingMemberFlags2);
            var expression = new MemberExpressionNode(target, MemberName).UpdateMetadata(BindingParameterNameConstant.MemberFlags, memberFlags1,
                BindingParameterNameConstant.BindingMemberFlags, bindingMemberFlags1);
            var visitor = new BindingMemberExpressionVisitor {MemberFlags = MemberFlags.All, Flags = BindingMemberExpressionFlags.Observable};
            visitor.Visit(expression, isTarget, DefaultMetadata).ShouldEqual(new BindingMemberExpressionNode($"{MemberName2}.{MemberName}", 0,
                bindingMemberFlags1 | bindingMemberFlags2,
                (memberFlags1 | memberFlags2).SetInstanceOrStaticFlags(false), null, expression));
        }

        [Theory]
        [InlineData(MacrosConstant.Target, true)]
        [InlineData(MacrosConstant.Self, true)]
        [InlineData(MacrosConstant.This, true)]
        [InlineData(MacrosConstant.Target, false)]
        [InlineData(MacrosConstant.Self, false)]
        [InlineData(MacrosConstant.This, false)]
        public void VisitShouldHandleTargetMacros1(string macros, bool isTarget)
        {
            var visitor = new BindingMemberExpressionVisitor {MemberFlags = MemberFlags.All, Flags = BindingMemberExpressionFlags.Observable};

            IExpressionNode expression = new UnaryExpressionNode(UnaryTokenType.DynamicExpression, new MemberExpressionNode(null, macros));
            visitor.Visit(expression, isTarget, DefaultMetadata)
                   .ShouldEqual(new BindingMemberExpressionNode(string.Empty, 0, visitor.Flags.SetTargetFlags(true), visitor.MemberFlags.SetInstanceOrStaticFlags(false), null,
                       expression));

            expression = new MemberExpressionNode(new UnaryExpressionNode(UnaryTokenType.DynamicExpression, new MemberExpressionNode(null, macros)), MemberName);
            visitor.Visit(expression, isTarget, DefaultMetadata)
                   .ShouldEqual(new BindingMemberExpressionNode(MemberName, 0, visitor.Flags.SetTargetFlags(true), visitor.MemberFlags.SetInstanceOrStaticFlags(false), null,
                       expression));
        }

        [Theory]
        [InlineData(MacrosConstant.Target, true)]
        [InlineData(MacrosConstant.Self, true)]
        [InlineData(MacrosConstant.This, true)]
        [InlineData(MacrosConstant.Target, false)]
        [InlineData(MacrosConstant.Self, false)]
        [InlineData(MacrosConstant.This, false)]
        public void VisitShouldHandleTargetMacros2(string macros, bool isTarget)
        {
            EnumFlags<MemberFlags> memberFlags = MemberFlags.Attached;
            EnumFlags<BindingMemberExpressionFlags> bindingMemberFlags = BindingMemberExpressionFlags.Optional;
            var visitor = new BindingMemberExpressionVisitor {MemberFlags = MemberFlags.All, Flags = BindingMemberExpressionFlags.Observable};

            IExpressionNode expression = new UnaryExpressionNode(UnaryTokenType.DynamicExpression, new MemberExpressionNode(null, macros))
                .UpdateMetadata(BindingParameterNameConstant.MemberFlags, memberFlags, BindingParameterNameConstant.BindingMemberFlags, bindingMemberFlags);

            visitor.Visit(expression, isTarget, DefaultMetadata)
                   .ShouldEqual(new BindingMemberExpressionNode(string.Empty, 0, bindingMemberFlags.SetTargetFlags(true), memberFlags.SetInstanceOrStaticFlags(false), null,
                       expression));

            expression = new MemberExpressionNode(new UnaryExpressionNode(UnaryTokenType.DynamicExpression, new MemberExpressionNode(null, macros)), MemberName)
                .UpdateMetadata(BindingParameterNameConstant.MemberFlags, memberFlags, BindingParameterNameConstant.BindingMemberFlags, bindingMemberFlags);
            visitor.Visit(expression, isTarget, DefaultMetadata)
                   .ShouldEqual(new BindingMemberExpressionNode(MemberName, 0, bindingMemberFlags.SetTargetFlags(true), memberFlags.SetInstanceOrStaticFlags(false), null,
                       expression));
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void VisitShouldHandleSourceMacros(bool isTarget)
        {
            var visitor = new BindingMemberExpressionVisitor {MemberFlags = MemberFlags.All, Flags = BindingMemberExpressionFlags.Observable};

            IExpressionNode expression = new UnaryExpressionNode(UnaryTokenType.DynamicExpression, new MemberExpressionNode(null, MacrosConstant.Source));
            visitor.Visit(expression, isTarget, DefaultMetadata)
                   .ShouldEqual(new BindingMemberExpressionNode(string.Empty, 0, visitor.Flags.SetTargetFlags(false), visitor.MemberFlags.SetInstanceOrStaticFlags(false), null,
                       expression));

            expression = new MemberExpressionNode(new UnaryExpressionNode(UnaryTokenType.DynamicExpression, new MemberExpressionNode(null, MacrosConstant.Source)), MemberName);
            visitor.Visit(expression, isTarget, DefaultMetadata)
                   .ShouldEqual(new BindingMemberExpressionNode(MemberName, 0, visitor.Flags.SetTargetFlags(false), visitor.MemberFlags.SetInstanceOrStaticFlags(false), null,
                       expression));
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void VisitShouldHandleSourceContext(bool isTarget)
        {
            var visitor = new BindingMemberExpressionVisitor {MemberFlags = MemberFlags.All, Flags = BindingMemberExpressionFlags.Observable};

            IExpressionNode expression = new UnaryExpressionNode(UnaryTokenType.DynamicExpression, new MemberExpressionNode(null, MacrosConstant.Context));
            visitor.Visit(expression, isTarget, DefaultMetadata).ShouldEqual(new BindingMemberExpressionNode(BindableMembers.For<object>().DataContext(), 0,
                visitor.Flags.SetTargetFlags(true),
                visitor.MemberFlags.SetInstanceOrStaticFlags(false), null, expression));

            expression = new MemberExpressionNode(new UnaryExpressionNode(UnaryTokenType.DynamicExpression, new MemberExpressionNode(null, MacrosConstant.Context)), MemberName);
            visitor.Visit(expression, isTarget, DefaultMetadata).ShouldEqual(new BindingMemberExpressionNode($"{BindableMembers.For<object>().DataContext()}.{MemberName}", 0,
                visitor.Flags.SetTargetFlags(true),
                visitor.MemberFlags.SetInstanceOrStaticFlags(false), null, expression));
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void VisitShouldHandleType(bool isTarget)
        {
            var returnType = typeof(string);
            var invokeCount = 0;
            var resolver = new ResourceManager();
            resolver.AddComponent(new TestTypeResolverComponent
            {
                TryGetType = (s, o, arg4) =>
                {
                    s.ShouldEqual(TypeName);
                    arg4.ShouldEqual(DefaultMetadata);
                    ++invokeCount;
                    return returnType;
                }
            });

            var visitor = new BindingMemberExpressionVisitor(null, resolver) {MemberFlags = MemberFlags.All, Flags = BindingMemberExpressionFlags.Observable};
            IExpressionNode expression = new UnaryExpressionNode(UnaryTokenType.DynamicExpression, new MemberExpressionNode(null, TypeName));
            visitor.Visit(expression, isTarget, DefaultMetadata)
                   .ShouldEqual(new BindingInstanceMemberExpressionNode(returnType, string.Empty, 0, visitor.Flags.SetTargetFlags(isTarget),
                       visitor.MemberFlags.SetInstanceOrStaticFlags(true), null, expression));
            invokeCount.ShouldEqual(1);

            invokeCount = 0;
            expression = new MemberExpressionNode(new UnaryExpressionNode(UnaryTokenType.DynamicExpression, new MemberExpressionNode(null, TypeName)), MemberName);
            visitor.Visit(expression, isTarget, DefaultMetadata)
                   .ShouldEqual(new BindingInstanceMemberExpressionNode(returnType, MemberName, 0, visitor.Flags.SetTargetFlags(isTarget),
                       visitor.MemberFlags.SetInstanceOrStaticFlags(true), null, expression));
            invokeCount.ShouldEqual(1);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void VisitShouldHandleTypeStatic(bool isTarget)
        {
            var returnType = typeof(string);
            var member1Result = 1;
            var member2Result = "";
            var member1 = new TestAccessorMemberInfo
            {
                GetValue = (o, context) =>
                {
                    o.ShouldBeNull();
                    context.ShouldEqual(DefaultMetadata);
                    return member1Result;
                }
            };
            var member2 = new TestAccessorMemberInfo
            {
                GetValue = (o, context) =>
                {
                    o.ShouldEqual(member1Result);
                    context.ShouldEqual(DefaultMetadata);
                    return member2Result;
                }
            };

            var invokeCount = 0;
            var resolver = new ResourceManager();
            resolver.AddComponent(new TestTypeResolverComponent
            {
                TryGetType = (s, o, arg4) =>
                {
                    s.ShouldEqual(TypeName);
                    arg4.ShouldEqual(DefaultMetadata);
                    ++invokeCount;
                    return returnType;
                }
            });
            var memberManager = new MemberManager();
            memberManager.AddComponent(new TestMemberManagerComponent
            {
                TryGetMembers = (t, m, f, r, meta) =>
                {
                    m.ShouldEqual(MemberType.Accessor);
                    if (r.Equals(MemberName))
                    {
                        t.ShouldEqual(returnType);
                        f.ShouldEqual(MemberFlags.All.SetInstanceOrStaticFlags(true));
                        return member1;
                    }

                    if (r.Equals(MemberName2))
                    {
                        t.ShouldEqual(member1Result.GetType());
                        f.ShouldEqual(MemberFlags.All.SetInstanceOrStaticFlags(false));
                        return member2;
                    }

                    throw new NotSupportedException();
                }
            });

            var observationManager = new ObservationManager();
            observationManager.AddComponent(new TestMemberPathProviderComponent
            {
                TryGetMemberPath = (o, arg3) =>
                {
                    var path = (string) o!;
                    if (path == "")
                        return MemberPath.Empty;
                    if (path == MemberName)
                        return MemberPath.Get(MemberName);
                    if (path == $"{MemberName}.{MemberName2}")
                        return MemberPath.Get(path);
                    throw new NotSupportedException();
                }
            });

            var visitor = new BindingMemberExpressionVisitor(observationManager, resolver, memberManager)
                {MemberFlags = MemberFlags.All, Flags = BindingMemberExpressionFlags.Observable};
            IExpressionNode expression = new UnaryExpressionNode(UnaryTokenType.StaticExpression, new MemberExpressionNode(null, TypeName));
            visitor.Visit(expression, isTarget, DefaultMetadata).ShouldEqual(ConstantExpressionNode.Get(returnType));
            invokeCount.ShouldEqual(1);

            invokeCount = 0;
            expression = new MemberExpressionNode(new UnaryExpressionNode(UnaryTokenType.StaticExpression, new MemberExpressionNode(null, TypeName)), MemberName);
            visitor.Visit(expression, isTarget, DefaultMetadata).ShouldEqual(ConstantExpressionNode.Get(member1Result));
            invokeCount.ShouldEqual(1);

            invokeCount = 0;
            expression = new MemberExpressionNode(
                new MemberExpressionNode(new UnaryExpressionNode(UnaryTokenType.StaticExpression, new MemberExpressionNode(null, TypeName)), MemberName), MemberName2);
            visitor.Visit(expression, isTarget, DefaultMetadata).ShouldEqual(ConstantExpressionNode.Get(member2Result));
            invokeCount.ShouldEqual(1);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void VisitShouldHandleResource1(bool isTarget)
        {
            var visitor = new BindingMemberExpressionVisitor {MemberFlags = MemberFlags.All, Flags = BindingMemberExpressionFlags.Observable};
            IExpressionNode expression = new UnaryExpressionNode(UnaryTokenType.DynamicExpression, new MemberExpressionNode(null, ResourceName));
            visitor.Visit(expression, isTarget, DefaultMetadata)
                   .ShouldEqual(new BindingResourceMemberExpressionNode(ResourceName, string.Empty, 0, visitor.Flags.SetTargetFlags(isTarget),
                       visitor.MemberFlags.SetInstanceOrStaticFlags(false), null, expression));

            expression = new MemberExpressionNode(new UnaryExpressionNode(UnaryTokenType.DynamicExpression, new MemberExpressionNode(null, ResourceName)), MemberName);
            visitor.Visit(expression, isTarget, DefaultMetadata)
                   .ShouldEqual(new BindingResourceMemberExpressionNode(ResourceName, MemberName, 0, visitor.Flags.SetTargetFlags(isTarget),
                       visitor.MemberFlags.SetInstanceOrStaticFlags(false), null, expression));
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void VisitShouldHandleResource2(bool isTarget)
        {
            EnumFlags<MemberFlags> memberFlags = MemberFlags.Attached;
            EnumFlags<BindingMemberExpressionFlags> bindingMemberFlags = BindingMemberExpressionFlags.Optional;
            var visitor = new BindingMemberExpressionVisitor {MemberFlags = MemberFlags.All, Flags = BindingMemberExpressionFlags.Observable};
            IExpressionNode expression = new UnaryExpressionNode(UnaryTokenType.DynamicExpression, new MemberExpressionNode(null, ResourceName))
                .UpdateMetadata(BindingParameterNameConstant.MemberFlags, memberFlags, BindingParameterNameConstant.BindingMemberFlags, bindingMemberFlags);
            visitor.Visit(expression, isTarget, DefaultMetadata)
                   .ShouldEqual(new BindingResourceMemberExpressionNode(ResourceName, string.Empty, 0, bindingMemberFlags, memberFlags.SetInstanceOrStaticFlags(false), null,
                       expression));

            expression = new MemberExpressionNode(new UnaryExpressionNode(UnaryTokenType.DynamicExpression, new MemberExpressionNode(null, ResourceName)), MemberName)
                .UpdateMetadata(BindingParameterNameConstant.MemberFlags, memberFlags, BindingParameterNameConstant.BindingMemberFlags, bindingMemberFlags);
            visitor.Visit(expression, isTarget, DefaultMetadata)
                   .ShouldEqual(new BindingResourceMemberExpressionNode(ResourceName, MemberName, 0, bindingMemberFlags, memberFlags.SetInstanceOrStaticFlags(false), null,
                       expression));
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(false, false)]
        public void VisitShouldHandleResourceStatic(bool isTarget, bool wrapToDynamic)
        {
            var resource = 1;
            var invokeCount = 0;
            var resolver = new ResourceManager();
            resolver.AddComponent(new TestResourceResolverComponent
            {
                TryGetResource = (s, o, arg4) =>
                {
                    s.ShouldEqual(ResourceName);
                    arg4.ShouldEqual(DefaultMetadata);
                    ++invokeCount;
                    return new ResourceResolverResult(wrapToDynamic ? new TestDynamicResource {Value = resource} : (object) resource);
                }
            });
            var memberResult = "w";
            var member = new TestAccessorMemberInfo
            {
                GetValue = (o, context) =>
                {
                    o.ShouldEqual(resource);
                    context.ShouldEqual(DefaultMetadata);
                    return memberResult;
                }
            };

            var memberManager = new MemberManager();
            memberManager.AddComponent(new TestMemberManagerComponent
            {
                TryGetMembers = (t, m, f, r, meta) =>
                {
                    m.ShouldEqual(MemberType.Accessor);
                    if (r.Equals(MemberName))
                    {
                        t.ShouldEqual(resource.GetType());
                        f.ShouldEqual(MemberFlags.All.SetInstanceOrStaticFlags(false));
                        return member;
                    }

                    throw new NotSupportedException();
                }
            });

            var observationManager = new ObservationManager();
            observationManager.AddComponent(new TestMemberPathProviderComponent
            {
                TryGetMemberPath = (o, arg3) =>
                {
                    var path = (string) o!;
                    if (path == "")
                        return MemberPath.Empty;
                    if (path == MemberName)
                        return MemberPath.Get(path);
                    throw new NotSupportedException();
                }
            });

            var visitor = new BindingMemberExpressionVisitor(observationManager, resolver, memberManager)
                {MemberFlags = MemberFlags.All, Flags = BindingMemberExpressionFlags.Observable};
            IExpressionNode expression = new UnaryExpressionNode(UnaryTokenType.StaticExpression, new MemberExpressionNode(null, ResourceName));
            visitor.Visit(expression, isTarget, DefaultMetadata).ShouldEqual(ConstantExpressionNode.Get(resource));
            invokeCount.ShouldEqual(1);

            invokeCount = 0;
            expression = new MemberExpressionNode(new UnaryExpressionNode(UnaryTokenType.StaticExpression, new MemberExpressionNode(null, ResourceName)), MemberName);
            visitor.Visit(expression, isTarget, DefaultMetadata).ShouldEqual(ConstantExpressionNode.Get(memberResult));
            invokeCount.ShouldEqual(1);
        }

        [Theory]
        [InlineData(MacrosConstant.Target)]
        [InlineData(MacrosConstant.Self)]
        [InlineData(MacrosConstant.This)]
        [InlineData(MacrosConstant.Source)]
        [InlineData(MacrosConstant.Context)]
        public void VisitShouldCacheMacrosExpression(string macros)
        {
            var expression = new BinaryExpressionNode(BinaryTokenType.Addition, new UnaryExpressionNode(UnaryTokenType.DynamicExpression, new MemberExpressionNode(null, macros)),
                new UnaryExpressionNode(UnaryTokenType.DynamicExpression, new MemberExpressionNode(null, macros)));
            var visitor = new BindingMemberExpressionVisitor();
            var expressionNode = (BinaryExpressionNode) visitor.Visit(expression, true, DefaultMetadata);
            ReferenceEquals(expressionNode.Left, expressionNode.Right).ShouldBeTrue();
        }
    }
}