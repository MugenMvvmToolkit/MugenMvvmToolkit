using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using MugenMvvm.Bindings.Compiling.Components;
using MugenMvvm.Bindings.Constants;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Bindings.Interfaces.Members;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Bindings.Interfaces.Resources;
using MugenMvvm.Bindings.Members;
using MugenMvvm.Bindings.Parsing.Expressions;
using MugenMvvm.Bindings.Parsing.Expressions.Binding;
using MugenMvvm.Bindings.Resources;
using MugenMvvm.Collections;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Metadata;
using MugenMvvm.Tests.Bindings.Members;
using MugenMvvm.Tests.Bindings.Resources;
using MugenMvvm.UnitTests.Bindings.Compiling.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Bindings.Compiling.Components
{
    [Collection(SharedContext)]
    public class MethodCallIndexerExpressionBuilderTest : UnitTestBase
    {
        private const int ConstantParameterTypeState = 0;
        private const int ConstantObjectParameterTypeState = 1;
        private const int BindingParameterTypeState = 2;

        private readonly MethodCallIndexerExpressionBuilder _component;
        private readonly TestMemberManagerComponent _memberManagerComponent;
        private readonly TestTypeResolverComponent _typeResolver;
        private readonly TestExpressionBuilderContext _context;

        public MethodCallIndexerExpressionBuilderTest()
        {
            _context = new TestExpressionBuilderContext();
            _memberManagerComponent = new TestMemberManagerComponent();
            MemberManager.AddComponent(_memberManagerComponent);

            _typeResolver = new TestTypeResolverComponent();
            ResourceManager.AddComponent(_typeResolver);
            _component = new MethodCallIndexerExpressionBuilder(MemberManager, ResourceManager, GlobalValueConverter);
            WithGlobalService(ReflectionManager);
        }

        [Fact]
        public void TryBuildShouldBuildArray()
        {
            var array = new[] { 1, 2, 3 };
            var expressionNode = new IndexExpressionNode(ConstantExpressionNode.Get(array), new[] { ConstantExpressionNode.Get(1) });
            var build = _component.TryBuild(_context, expressionNode)!;
            build.Invoke().ShouldEqual(array[1]);
        }

        [Fact]
        public void TryBuildShouldBuildMethodCallIndexer()
        {
            var invokeCount = 0;
            const string memberName = BindingInternalConstant.IndexerGetterName;
            var metadataContext = _context.Metadata;
            var result = new TestMethodMemberInfo
            {
                Type = typeof(object),
                GetParameters = () => new[] { new TestParameterInfo { ParameterType = typeof(bool) } },
                Invoke = (o, objects, arg3) =>
                {
                    o.ShouldEqual(this);
                    objects.Item.ShouldEqual(false);
                    arg3.ShouldEqual(Metadata);
                    return this;
                }
            };
            _memberManagerComponent.TryGetMembers = (_, t, m, f, r, meta) =>
            {
                ++invokeCount;
                t.ShouldEqual(GetType());
                r.ShouldEqual(memberName);
                m.ShouldEqual(MemberType.Method);
                f.HasFlag(_component.MemberFlags & ~MemberFlags.Static).ShouldBeTrue();
                meta.ShouldEqual(metadataContext);
                return result;
            };

            var expressionNode = new IndexExpressionNode(ConstantExpressionNode.Get(this), new[] { ConstantExpressionNode.False });
            var build = _component.TryBuild(_context, expressionNode)!;
            build.Invoke(new[] { _context.MetadataExpression }, Metadata).ShouldEqual(this);
            invokeCount.ShouldEqual(1);
        }

        [Theory]
        [InlineData(true, ConstantParameterTypeState)]
        [InlineData(true, BindingParameterTypeState)]
        [InlineData(false, ConstantParameterTypeState)]
        [InlineData(false, ConstantObjectParameterTypeState)]
        [InlineData(false, BindingParameterTypeState)]
        public void TryBuildShouldBuildMethodCallInstance1(bool underlying, int state)
        {
            const string memberName = nameof(MethodInvokerInstance.Method);
            var members = GetMethods(typeof(MethodInvokerInstance), memberName, underlying);
            var instance = new MethodInvokerInstance();
            _memberManagerComponent.TryGetMembers = (_, t, m, f, r, meta) =>
            {
                if (t == typeof(object))
                    return Array.Empty<IMemberInfo>();
                return members;
            };


            var call = GetMethodCall(_context, memberName, underlying, instance, state, 2M, 3M);
            call.exp.Invoke(call.parameters, call.compiledArgs);
            instance.Assert(() => instance.Method(2M, 3M), call.args);

            call = GetMethodCall(_context, memberName, underlying, instance, state, 2, 3);
            call.exp.Invoke(call.parameters, call.compiledArgs);
            instance.Assert(() => instance.Method(2, 3), call.args);

            call = GetMethodCall(_context, memberName, underlying, instance, state, 2M, "t");
            call.exp.Invoke(call.parameters, call.compiledArgs);
            instance.Assert(() => instance.Method(2M, "t"), call.args);

            call = GetMethodCall(_context, memberName, underlying, instance, state, 2, "t");
            call.exp.Invoke(call.parameters, call.compiledArgs);
            instance.Assert(() => instance.Method(2, "t"), call.args);

            call = GetMethodCall(_context, memberName, underlying, instance, state, "t", 2M);
            call.exp.Invoke(call.parameters, call.compiledArgs);
            instance.Assert(() => instance.Method("t", 2M), call.args);

            call = GetMethodCall(_context, memberName, underlying, instance, state, 2, 3, "st", 3);
            call.exp.Invoke(call.parameters, call.compiledArgs);
            instance.Assert(() => instance.Method(2, 3, "st", 3), call.args);

            call = GetMethodCall(_context, memberName, underlying, instance, state, 2M, 3M, "st", 3);
            call.exp.Invoke(call.parameters, call.compiledArgs);
            instance.Assert(() => instance.Method(2M, 3M, "st", 3), call.args);

            call = GetMethodCall(_context, memberName, underlying, instance, state, 2M, "t", "st", 3);
            call.exp.Invoke(call.parameters, call.compiledArgs);
            instance.Assert(() => instance.Method(2M, "t", "st", 3), call.args);

            call = GetMethodCall(_context, memberName, underlying, instance, state, "t", 2M, "st", 3);
            call.exp.Invoke(call.parameters, call.compiledArgs);
            instance.Assert(() => instance.Method("t", 2M, "st", 3), call.args);
        }

        [Theory]
        [InlineData(true, ConstantParameterTypeState)]
        [InlineData(true, BindingParameterTypeState)]
        [InlineData(false, ConstantParameterTypeState)]
        [InlineData(false, ConstantObjectParameterTypeState)]
        [InlineData(false, BindingParameterTypeState)]
        public void TryBuildShouldBuildMethodCallInstance2(bool underlying, int state)
        {
            const string memberName = nameof(MethodInvokerInstance.Method1);
            var members = GetMethods(typeof(MethodInvokerInstance), memberName, underlying);
            var instance = new MethodInvokerInstance();
            _memberManagerComponent.TryGetMembers = (_, t, m, f, r, meta) =>
            {
                if (t == typeof(object))
                    return Array.Empty<IMemberInfo>();
                return members;
            };

            var call = GetMethodCall(_context, memberName, underlying, instance, state, "st");
            call.exp.Invoke(call.parameters, call.compiledArgs);
            instance.Assert(() => instance.Method1("st", 0M, "", int.MaxValue), "st", 0M, "", int.MaxValue);
        }

        [Theory]
        [InlineData(true, ConstantParameterTypeState)]
        [InlineData(true, BindingParameterTypeState)]
        [InlineData(false, ConstantParameterTypeState)]
        [InlineData(false, ConstantObjectParameterTypeState)]
        [InlineData(false, BindingParameterTypeState)]
        public void TryBuildShouldBuildMethodCallInstance3(bool underlying, int state)
        {
            const string memberName = nameof(MethodInvokerInstance.Method2);
            var members = GetMethods(typeof(MethodInvokerInstance), memberName, underlying);
            var instance = new MethodInvokerInstance();
            _memberManagerComponent.TryGetMembers = (_, t, m, f, r, meta) =>
            {
                if (t == typeof(object))
                    return Array.Empty<IMemberInfo>();
                return members;
            };

            var call = GetMethodCall(_context, memberName, underlying, instance, state, 2, 3);
            call.exp.Invoke(call.parameters, call.compiledArgs);
            instance.Assert(() => instance.Method2(2, 3), call.args);
        }

        [Theory]
        [InlineData(true, ConstantParameterTypeState)]
        [InlineData(true, BindingParameterTypeState)]
        [InlineData(false, ConstantParameterTypeState)]
        [InlineData(false, ConstantObjectParameterTypeState)]
        [InlineData(false, BindingParameterTypeState)]
        public void TryBuildShouldBuildMethodCallInstance4(bool underlying, int state)
        {
            const string memberName = nameof(MethodInvokerInstance.Method3);
            var members = GetMethods(typeof(MethodInvokerInstance), memberName, underlying);
            var instance = new MethodInvokerInstance();
            _memberManagerComponent.TryGetMembers = (_, t, m, f, r, meta) =>
            {
                if (t == typeof(object))
                    return Array.Empty<IMemberInfo>();
                return members;
            };

            var call = GetMethodCall(_context, memberName, false, instance, state, 1, 2);
            call.exp.Invoke(call.parameters, call.compiledArgs);
            instance.Assert(() => instance.Method3(1, 2, Metadata), 1, 2, Metadata);
        }

        [Theory]
        [InlineData(ConstantParameterTypeState)]
        [InlineData(ConstantObjectParameterTypeState)]
        [InlineData(BindingParameterTypeState)]
        public void TryBuildShouldBuildMethodCallInstance5(int state)
        {
            const string memberName = nameof(MethodInvokerInstance.Method4);
            var members = new[]
            {
                new MethodMemberInfo(memberName, typeof(MethodInvokerInstance).GetMethod(memberName)!, false, typeof(MethodInvokerInstance), null, null)
            };
            var instance = new MethodInvokerInstance();
            var metadata = _context.Metadata;
            _memberManagerComponent.TryGetMembers = (_, t, m, f, r, meta) =>
            {
                if (t == typeof(object))
                    return Array.Empty<IMemberInfo>();
                return members;
            };

            var invokeCount = 0;
            var typeName = typeof(decimal).Name;
            _typeResolver.TryGetType = (_, s, request, m) =>
            {
                ++invokeCount;
                s.ShouldEqual(typeName);
                metadata.ShouldEqual(m);
                return typeof(decimal);
            };

            var call = GetMethodCall(_context, memberName, false, instance, state, new[] { typeName }, 1);
            call.exp.Invoke(call.parameters, call.compiledArgs);
            invokeCount.ShouldEqual(1);
            instance.Assert(() => instance.Method4<decimal>(1), 1M);
        }

        [Fact]
        public void TryBuildShouldBuildMethodCallInstanceNoArgs()
        {
            var invokeCount = 0;
            const string memberName = nameof(InstanceMethod1);
            var metadataContext = _context.Metadata;
            var result = new TestMethodMemberInfo
            {
                Type = typeof(object),
                Invoke = (o, objects, arg3) =>
                {
                    ++invokeCount;
                    o.ShouldEqual(this);
                    objects.IsEmpty.ShouldBeTrue();
                    arg3.ShouldEqual(Metadata);
                    return this;
                }
            };
            _memberManagerComponent.TryGetMembers = (_, t, m, f, r, meta) =>
            {
                t.ShouldEqual(GetType());
                r.ShouldEqual(memberName);
                m.ShouldEqual(MemberType.Method);
                f.HasFlag(_component.MemberFlags & ~MemberFlags.Static).ShouldBeTrue();
                meta.ShouldEqual(metadataContext);
                return result;
            };

            var expressionNode = new MethodCallExpressionNode(ConstantExpressionNode.Get(this), memberName, default);
            var build = _component.TryBuild(_context, expressionNode)!;

            build.Invoke(new[] { _context.MetadataExpression }, Metadata).ShouldEqual(this);
            invokeCount.ShouldEqual(1);
        }

        [Fact]
        public void TryBuildShouldBuildMethodCallInstanceNoArgsDynamic()
        {
            var invokeCount = 0;
            const string memberName = nameof(InstanceMethod1);
            var metadataContext = _context.Metadata;
            var result = new TestMethodMemberInfo
            {
                Type = typeof(object),
                Invoke = (o, objects, arg3) =>
                {
                    ++invokeCount;
                    o.ShouldEqual(this);
                    objects.IsEmpty.ShouldBeTrue();
                    arg3.ShouldEqual(Metadata);
                    return this;
                }
            };

            var members = Array.Empty<IMemberInfo>();
            _memberManagerComponent.TryGetMembers = (_, t, m, f, r, meta) =>
            {
                t.ShouldEqual(GetType());
                r.ShouldEqual(memberName);
                m.ShouldEqual(MemberType.Method);
                f.HasFlag(_component.MemberFlags & ~MemberFlags.Static).ShouldBeTrue();
                meta.ShouldEqual(metadataContext);
                return members;
            };

            var expressionNode = new MethodCallExpressionNode(ConstantExpressionNode.Get(this), memberName, default);
            var build = _component.TryBuild(_context, expressionNode)!;

            members = new[] { result };
            build.Invoke(new[] { _context.MetadataExpression }, Metadata).ShouldEqual(this);
            invokeCount.ShouldEqual(1);
        }

        [Fact]
        public void TryBuildShouldBuildMethodCallInstanceNoArgsUnderlyingMember()
        {
            const string memberName = nameof(InstanceMethod1);
            var metadataContext = _context.Metadata;
            var result = new TestMethodMemberInfo
            {
                UnderlyingMember = GetType().GetMethod(memberName)
            };
            _memberManagerComponent.TryGetMembers = (_, t, m, f, r, meta) =>
            {
                t.ShouldEqual(GetType());
                r.ShouldEqual(memberName);
                m.ShouldEqual(MemberType.Method);
                f.HasFlag(_component.MemberFlags & ~MemberFlags.Static).ShouldBeTrue();
                meta.ShouldEqual(metadataContext);
                return result;
            };

            var expressionNode = new MethodCallExpressionNode(ConstantExpressionNode.Get(this), memberName, default);
            var build = _component.TryBuild(_context, expressionNode)!;

            var invokeCount = 0;
            InstanceMethod1Delegate = () =>
            {
                ++invokeCount;
                return this;
            };

            build.Invoke().ShouldEqual(this);
            invokeCount.ShouldEqual(1);
        }

        [Theory]
        [InlineData(true, ConstantParameterTypeState)]
        [InlineData(true, BindingParameterTypeState)]
        [InlineData(false, ConstantParameterTypeState)]
        [InlineData(false, BindingParameterTypeState)]
        public void TryBuildShouldBuildMethodCallStatic1(bool underlying, int state)
        {
            const string memberName = nameof(MethodInvokerStatic.Method);
            var members = GetMethods(typeof(MethodInvokerStatic), memberName, underlying);
            _memberManagerComponent.TryGetMembers = (_, t, m, f, r, meta) => members;

            var type = typeof(MethodInvokerStatic);
            var call = GetMethodCall(_context, memberName, underlying, type, state, 2M, 3M);
            call.exp.Invoke(call.parameters, call.compiledArgs);
            MethodInvokerStatic.Assert(() => MethodInvokerStatic.Method(2M, 3M), call.args);

            call = GetMethodCall(_context, memberName, underlying, type, state, 2, 3);
            call.exp.Invoke(call.parameters, call.compiledArgs);
            MethodInvokerStatic.Assert(() => MethodInvokerStatic.Method(2, 3), call.args);

            call = GetMethodCall(_context, memberName, underlying, type, state, 2M, "t");
            call.exp.Invoke(call.parameters, call.compiledArgs);
            MethodInvokerStatic.Assert(() => MethodInvokerStatic.Method(2M, "t"), call.args);

            call = GetMethodCall(_context, memberName, underlying, type, state, 2, "t");
            call.exp.Invoke(call.parameters, call.compiledArgs);
            MethodInvokerStatic.Assert(() => MethodInvokerStatic.Method(2, "t"), call.args);

            call = GetMethodCall(_context, memberName, underlying, type, state, "t", 2M);
            call.exp.Invoke(call.parameters, call.compiledArgs);
            MethodInvokerStatic.Assert(() => MethodInvokerStatic.Method("t", 2M), call.args);

            call = GetMethodCall(_context, memberName, underlying, type, state, 2, 3, "st", 3);
            call.exp.Invoke(call.parameters, call.compiledArgs);
            MethodInvokerStatic.Assert(() => MethodInvokerStatic.Method(2, 3, "st", 3), call.args);

            call = GetMethodCall(_context, memberName, underlying, type, state, 2M, 3M, "st", 3);
            call.exp.Invoke(call.parameters, call.compiledArgs);
            MethodInvokerStatic.Assert(() => MethodInvokerStatic.Method(2M, 3M, "st", 3), call.args);

            call = GetMethodCall(_context, memberName, underlying, type, state, 2M, "t", "st", 3);
            call.exp.Invoke(call.parameters, call.compiledArgs);
            MethodInvokerStatic.Assert(() => MethodInvokerStatic.Method(2M, "t", "st", 3), call.args);

            call = GetMethodCall(_context, memberName, underlying, type, state, "t", 2M, "st", 3);
            call.exp.Invoke(call.parameters, call.compiledArgs);
            MethodInvokerStatic.Assert(() => MethodInvokerStatic.Method("t", 2M, "st", 3), call.args);
        }

        [Theory]
        [InlineData(true, ConstantParameterTypeState)]
        [InlineData(true, BindingParameterTypeState)]
        [InlineData(false, ConstantParameterTypeState)]
        [InlineData(false, BindingParameterTypeState)]
        public void TryBuildShouldBuildMethodCallStatic2(bool underlying, int state)
        {
            const string memberName = nameof(MethodInvokerStatic.Method1);
            var members = GetMethods(typeof(MethodInvokerStatic), memberName, underlying);
            _memberManagerComponent.TryGetMembers = (_, t, m, f, r, meta) =>
            {
                if (t == typeof(object))
                    return Array.Empty<IMemberInfo>();
                return members;
            };

            var call = GetMethodCall(_context, memberName, underlying, typeof(MethodInvokerStatic), state, "st");
            call.exp.Invoke(call.parameters, call.compiledArgs);
            MethodInvokerStatic.Assert(() => MethodInvokerStatic.Method1("st", 0M, "", int.MaxValue), "st", 0M, "", int.MaxValue);
        }

        [Theory]
        [InlineData(true, ConstantParameterTypeState)]
        [InlineData(true, BindingParameterTypeState)]
        [InlineData(false, ConstantParameterTypeState)]
        [InlineData(false, BindingParameterTypeState)]
        public void TryBuildShouldBuildMethodCallStatic3(bool underlying, int state)
        {
            const string memberName = nameof(MethodInvokerStatic.Method2);
            var members = GetMethods(typeof(MethodInvokerStatic), memberName, underlying);
            _memberManagerComponent.TryGetMembers = (_, t, m, f, r, meta) =>
            {
                if (t == typeof(object))
                    return Array.Empty<IMemberInfo>();
                return members;
            };

            var call = GetMethodCall(_context, memberName, underlying, typeof(MethodInvokerStatic), state, 2, 3);
            call.exp.Invoke(call.parameters, call.compiledArgs);
            MethodInvokerStatic.Assert(() => MethodInvokerStatic.Method2(2, 3), call.args);
        }

        [Theory]
        [InlineData(true, ConstantParameterTypeState)]
        [InlineData(true, BindingParameterTypeState)]
        [InlineData(false, ConstantParameterTypeState)]
        [InlineData(false, BindingParameterTypeState)]
        public void TryBuildShouldBuildMethodCallStatic4(bool underlying, int state)
        {
            const string memberName = nameof(MethodInvokerStatic.Method3);
            var members = GetMethods(typeof(MethodInvokerStatic), memberName, underlying);
            _memberManagerComponent.TryGetMembers = (_, t, m, f, r, meta) =>
            {
                if (t == typeof(object))
                    return Array.Empty<IMemberInfo>();
                return members;
            };

            var call = GetMethodCall(_context, memberName, false, typeof(MethodInvokerStatic), state, 1, 2);
            call.exp.Invoke(call.parameters, call.compiledArgs);
            MethodInvokerStatic.Assert(() => MethodInvokerStatic.Method3(1, 2, Metadata), 1, 2, Metadata);
        }

        [Theory]
        [InlineData(ConstantParameterTypeState)]
        [InlineData(BindingParameterTypeState)]
        public void TryBuildShouldBuildMethodCallStatic5(int state)
        {
            const string memberName = nameof(MethodInvokerInstance.Method4);
            var members = new[]
            {
                new MethodMemberInfo(memberName, typeof(MethodInvokerStatic).GetMethod(memberName)!, false, typeof(MethodInvokerInstance), null, null)
            };
            var metadata = _context.Metadata;
            _memberManagerComponent.TryGetMembers = (_, t, m, f, r, meta) =>
            {
                if (t == typeof(object))
                    return Array.Empty<IMemberInfo>();
                return members;
            };

            var invokeCount = 0;
            var typeName = typeof(decimal).Name;
            _typeResolver.TryGetType = (_, s, request, m) =>
            {
                ++invokeCount;
                s.ShouldEqual(typeName);
                metadata.ShouldEqual(m);
                return typeof(decimal);
            };

            var call = GetMethodCall(_context, memberName, false, typeof(MethodInvokerStatic), state, new[] { typeName }, 1);
            call.exp.Invoke(call.parameters, call.compiledArgs);
            invokeCount.ShouldEqual(1);
            MethodInvokerStatic.Assert(() => MethodInvokerStatic.Method4<decimal>(1), 1M);
        }

        [Fact]
        public void TryBuildShouldBuildMethodCallStaticNoArgs()
        {
            var invokeCount = 0;
            const string memberName = nameof(StaticMethod1);
            var metadataContext = _context.Metadata;
            var result = new TestMethodMemberInfo
            {
                Type = typeof(object),
                Invoke = (o, objects, arg3) =>
                {
                    ++invokeCount;
                    o.ShouldBeNull();
                    objects.IsEmpty.ShouldBeTrue();
                    arg3.ShouldEqual(Metadata);
                    return this;
                }
            };
            _memberManagerComponent.TryGetMembers = (_, t, m, f, r, meta) =>
            {
                t.ShouldEqual(GetType());
                r.ShouldEqual(memberName);
                m.ShouldEqual(MemberType.Method);
                f.HasFlag(_component.MemberFlags & ~MemberFlags.Instance).ShouldBeTrue();
                meta.ShouldEqual(metadataContext);
                return result;
            };

            var expressionNode = new MethodCallExpressionNode(TypeAccessExpressionNode.Get(GetType()), memberName, default);
            var build = _component.TryBuild(_context, expressionNode)!;

            build.Invoke(new[] { _context.MetadataExpression }, Metadata).ShouldEqual(this);
            invokeCount.ShouldEqual(1);
        }

        [Fact]
        public void TryBuildShouldBuildMethodCallStaticNoArgsUnderlyingMember()
        {
            const string memberName = nameof(StaticMethod1);
            var metadataContext = _context.Metadata;
            var result = new TestMethodMemberInfo
            {
                UnderlyingMember = GetType().GetMethod(memberName)
            };
            _memberManagerComponent.TryGetMembers = (_, t, m, f, r, meta) =>
            {
                t.ShouldEqual(GetType());
                r.ShouldEqual(memberName);
                m.ShouldEqual(MemberType.Method);
                f.HasFlag(_component.MemberFlags & ~MemberFlags.Instance).ShouldBeTrue();
                meta.ShouldEqual(metadataContext);
                return result;
            };

            var expressionNode = new MethodCallExpressionNode(TypeAccessExpressionNode.Get(GetType()), memberName, default);
            var build = _component.TryBuild(_context, expressionNode)!;

            var invokeCount = 0;
            StaticMethod1Delegate = () =>
            {
                ++invokeCount;
                return this;
            };

            build.Invoke().ShouldEqual(this);
            invokeCount.ShouldEqual(1);
        }

        [Fact]
        public void TryBuildShouldIgnoreNotNullMethodCallExpression()
        {
            var component = new MethodCallIndexerExpressionBuilder();
            var ctx = new TestExpressionBuilderContext();
            component.TryBuild(ctx, ConstantExpressionNode.False).ShouldBeNull();
        }

        [Fact]
        public void TryBuildShouldThrowInstanceNoArgsDynamic()
        {
            const string memberName = nameof(InstanceMethod1);
            var metadataContext = _context.Metadata;

            var members = Array.Empty<IMemberInfo>();
            _memberManagerComponent.TryGetMembers = (_, t, m, f, r, meta) =>
            {
                t.ShouldEqual(GetType());
                r.ShouldEqual(memberName);
                m.ShouldEqual(MemberType.Method);
                f.HasFlag(_component.MemberFlags & ~MemberFlags.Static).ShouldBeTrue();
                meta.ShouldEqual(metadataContext);
                return members;
            };

            var expressionNode = new MethodCallExpressionNode(ConstantExpressionNode.Get(this), memberName, default);
            var build = _component.TryBuild(_context, expressionNode)!;

            ShouldThrow(() => build.Invoke(new[] { _context.MetadataExpression }, Metadata));
        }

        protected override IMemberManager GetMemberManager() => new MemberManager(ComponentCollectionManager);

        protected override IResourceManager GetResourceManager() => new ResourceManager(ComponentCollectionManager);

        public Func<object?>? InstanceMethod1Delegate { get; set; }

        public static Func<object?>? StaticMethod1Delegate { get; set; }

        public object? InstanceMethod1() => InstanceMethod1Delegate?.Invoke();

        public static object? StaticMethod1() => StaticMethod1Delegate?.Invoke();

        private static List<IMemberInfo> GetMethods(Type type, string memberName, bool underlying)
        {
            var members = new List<IMemberInfo>();
            var methods = type.GetMethods();
            for (var i = 0; i < methods.Length; i++)
            {
                var member = methods[i];
                if (member.Name != memberName)
                    continue;

                var methodInfo = new TestMethodMemberInfo
                {
                    Type = member.ReturnType,
                    GetParameters = () => member.GetParameters().Select(info => new ParameterInfoImpl(info)).ToArray()
                };
                if (underlying)
                    methodInfo.UnderlyingMember = member;
                else
                {
                    methodInfo.Invoke = (o, objects, arg3) =>
                    {
                        arg3.ShouldEqual(EmptyMetadataContext.Instance);
                        return member.Invoke(o, objects.AsList());
                    };
                }

                members.Add(methodInfo);
            }

            return members;
        }

        private (Expression exp, Expression[] parameters, object?[] args, object?[] compiledArgs) GetMethodCall(TestExpressionBuilderContext context, string method,
            bool underlying,
            object? instance, int state, params object[] args) =>
            GetMethodCall(context, method, underlying, instance, state, null, args);

        private (Expression exp, Expression[] parameters, object?[] args, object?[] compiledArgs) GetMethodCall(TestExpressionBuilderContext context, string method,
            bool underlying,
            object? instance, int state, IReadOnlyList<string>? typeArgs, params object[] args)
        {
            var parameters = new List<Expression>();
            var compilingArgs = state == 2 ? args : Array.Empty<object?>();

            if (state == 2)
            {
                context.Build = expressionNode =>
                {
                    if (expressionNode is IConstantExpressionNode c)
                        return Expression.Constant(c.Value, c.Type);
                    return parameters[((IBindingMemberExpressionNode)expressionNode).Index];
                };
            }

            IExpressionNode? target;
            switch (state)
            {
                case ConstantParameterTypeState:
                    target = instance is Type tt ? TypeAccessExpressionNode.Get(tt) : ConstantExpressionNode.Get(instance);
                    break;
                case ConstantObjectParameterTypeState:
                    target = ConstantExpressionNode.Get(instance, typeof(object));
                    break;
                default:
                    if (instance is Type t)
                    {
                        target = TypeAccessExpressionNode.Get(t);
                        break;
                    }

                    target = new BindingInstanceMemberExpressionNode(instance!, "", 0, default, default);
                    parameters.Add(Expression.Parameter(instance!.GetType()));
                    compilingArgs = compilingArgs.InsertFirstArg(instance);
                    break;
            }

            var arguments = new List<IExpressionNode>();
            for (var i = 0; i < args.Length; i++)
            {
                IExpressionNode arg;
                switch (state)
                {
                    case ConstantObjectParameterTypeState:
                    case ConstantParameterTypeState:
                        arg = ConstantExpressionNode.Get(args[i]);
                        break;
                    default:
                        arg = new BindingInstanceMemberExpressionNode(args[i], "", parameters.Count, default, default);
                        parameters.Add(Expression.Parameter(args[i].GetType(), i.ToString()));
                        break;
                }

                arguments.Add(arg);
            }

            if (state != 0 || !underlying)
                parameters.Add(context.MetadataExpression);

            var node = new MethodCallExpressionNode(target, method, arguments, ItemOrIReadOnlyList.FromList(typeArgs));
            var expression = _component.TryBuild(context, node);
            return (expression!, parameters.ToArray(), args, state == 0 && underlying ? compilingArgs : compilingArgs.Concat(new[] { Metadata }).ToArray());
        }

        public class MethodInvokerInstance
        {
            public static MethodInfo? LastMethod { get; private set; }

            public static object?[]? Args { get; private set; }

            public void Assert(Expression<Action> expression, params object?[] args)
            {
                var m = GetMethodInfo(expression);
                LastMethod.ShouldEqual(m);
                args.ShouldEqual(Args);
            }

            public object This() => this;

            public void Method(decimal x1, decimal x2) => SetMethod(() => Method(x1, x2), x1, x2);

            public void Method(int x1, int x2) => SetMethod(() => Method(x1, x2), x1, x2);

            public void Method(object x1, object x2) => SetMethod(() => Method(x1, x2), x1, x2);

            public void Method(decimal x1, decimal x2, params object[] items) => SetMethod(() => Method(x1, x2, items), x1, x2, items);

            public void Method(int x1, int x2, params object[] items) => SetMethod(() => Method(x1, x2, items), x1, x2, items);

            public void Method(object x1, object x2, params object[] items) => SetMethod(() => Method(x1, x2, items), x1, x2, items);

            public void Method(object item, decimal x = 0, params object[] items) => SetMethod(() => Method(item, x, items), item, x, items);

            public void Method(string item, decimal x = 0) => SetMethod(() => Method(item, x), item, x);

            public void Method1(string item, decimal x = 0, string st = "", int v = int.MaxValue, params int[] items) =>
                SetMethod(() => Method1(item, x, st, v, items), item, x, st, v, items);

            public void Method2(int x, int y = 1) => SetMethod(() => Method2(x, y), x, y);

            public void Method2(int x, params int[] items) => SetMethod(() => Method2(x, items), x, items);

            public void Method3(object x1, object x2, IReadOnlyMetadataContext? metadata = null) => SetMethod(() => Method3(x1, x2, metadata), x1, x2, metadata);

            public void Method4<T>(T value) => SetMethod(() => Method4(value), value);

            private static void SetMethod(Expression<Action> expression, params object?[] args)
            {
                LastMethod = GetMethodInfo(expression);
                var objects = new List<object>();
                foreach (var o in args)
                {
                    var array = o as Array;
                    if (array == null)
                        objects.Add(o!);
                    else
                        objects.AddRange(array.OfType<object>());
                }

                Args = objects.ToArray();
            }

            private static MethodInfo GetMethodInfo(LambdaExpression expression)
            {
                if (expression.Body is UnaryExpression unaryExpression && unaryExpression.Operand is MethodCallExpression memberExpression)
                    return memberExpression.Method;
                return ((MethodCallExpression)expression.Body).Method;
            }
        }

        public static class MethodInvokerStatic
        {
            public static MethodInfo? LastMethod { get; private set; }

            public static object?[]? Args { get; private set; }

            public static void Assert(Expression<Action> expression, params object?[] args)
            {
                var m = GetMethodInfo(expression);
                LastMethod.ShouldEqual(m);
                args.ShouldEqual(Args);
            }

            public static void Method(decimal x1, decimal x2) => SetMethod(() => Method(x1, x2), x1, x2);

            public static void Method(int x1, int x2) => SetMethod(() => Method(x1, x2), x1, x2);

            public static void Method(object x1, object x2) => SetMethod(() => Method(x1, x2), x1, x2);

            public static void Method(decimal x1, decimal x2, params object[] items) => SetMethod(() => Method(x1, x2, items), x1, x2, items);

            public static void Method(int x1, int x2, params object[] items) => SetMethod(() => Method(x1, x2, items), x1, x2, items);

            public static void Method(object x1, object x2, params object[] items) => SetMethod(() => Method(x1, x2, items), x1, x2, items);

            public static void Method(object item, decimal x = 0, params object[] items) => SetMethod(() => Method(item, x, items), item, x, items);

            public static void Method(string item, decimal x = 0) => SetMethod(() => Method(item, x), item, x);

            public static void Method1(string item, decimal x = 0, string st = "", int v = int.MaxValue, params int[] items) =>
                SetMethod(() => Method1(item, x, st, v, items), item, x, st, v, items);

            public static void Method2(int x, int y = 1) => SetMethod(() => Method2(x, y), x, y);

            public static void Method2(int x, params int[] items) => SetMethod(() => Method2(x, items), x, items);

            public static void Method3(object x1, object x2, IReadOnlyMetadataContext? metadata = null) => SetMethod(() => Method3(x1, x2, metadata), x1, x2, metadata);

            public static void Method4<T>(T value) => SetMethod(() => Method4(value), value);

            private static void SetMethod(Expression<Action> expression, params object?[] args)
            {
                LastMethod = GetMethodInfo(expression);
                var objects = new List<object>();
                foreach (var o in args)
                {
                    var array = o as Array;
                    if (array == null)
                        objects.Add(o!);
                    else
                        objects.AddRange(array.OfType<object>());
                }

                Args = objects.ToArray();
            }

            private static MethodInfo GetMethodInfo(LambdaExpression expression)
            {
                if (expression.Body is UnaryExpression unaryExpression && unaryExpression.Operand is MethodCallExpression memberExpression)
                    return memberExpression.Method;

                return ((MethodCallExpression)expression.Body).Method;
            }
        }
    }
}