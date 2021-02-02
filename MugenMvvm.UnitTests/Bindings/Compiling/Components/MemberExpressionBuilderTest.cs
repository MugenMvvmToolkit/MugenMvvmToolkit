using MugenMvvm.Bindings.Compiling.Components;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Interfaces.Members;
using MugenMvvm.Bindings.Members;
using MugenMvvm.Bindings.Parsing.Expressions;
using MugenMvvm.Extensions;
using MugenMvvm.UnitTests.Bindings.Compiling.Internal;
using MugenMvvm.UnitTests.Bindings.Members.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Bindings.Compiling.Components
{
    public class MemberExpressionBuilderTest : UnitTestBase
    {
        private readonly MemberExpressionBuilder _component;
        private readonly TestMemberManagerComponent _memberManagerComponent;
        private readonly TestExpressionBuilderContext _context;

        public MemberExpressionBuilderTest()
        {
            IMemberManager memberManager = new MemberManager(ComponentCollectionManager); 
            _memberManagerComponent = new TestMemberManagerComponent(memberManager);
            memberManager.AddComponent(_memberManagerComponent);
            _component = new MemberExpressionBuilder(memberManager);
            _context = new TestExpressionBuilderContext();
        }

        [Fact]
        public void TryBuildShouldBuildMemberAccessInstance()
        {
            const string memberName = nameof(InstanceProperty);
            var metadataContext = _context.Metadata;
            var result = new TestAccessorMemberInfo
            {
                GetValue = (o, context) =>
                {
                    o.ShouldEqual(this);
                    context.ShouldEqual(DefaultMetadata);
                    return InstanceProperty;
                },
                Type = typeof(string)
            };
            _memberManagerComponent.TryGetMembers = (t, m, f, r, meta) =>
            {
                t.ShouldEqual(GetType());
                r.ShouldEqual(memberName);
                m.HasFlag(MemberType.Accessor).ShouldBeTrue();
                f.HasFlag(_component.MemberFlags & ~MemberFlags.Static).ShouldBeTrue();
                meta.ShouldEqual(metadataContext);
                return result;
            };

            var expressionNode = new MemberExpressionNode(ConstantExpressionNode.Get(this), memberName);
            var build = _component.TryBuild(_context, expressionNode)!;

            build.Invoke(new[] {_context.MetadataExpression}, DefaultMetadata).ShouldEqual(InstanceProperty);
            InstanceProperty = "f";
            build.Invoke(new[] {_context.MetadataExpression}, DefaultMetadata).ShouldEqual(InstanceProperty);
        }

        [Fact]
        public void TryBuildShouldBuildMemberAccessInstanceDynamic()
        {
            const string memberName = nameof(InstanceProperty);
            var metadataContext = _context.Metadata;
            TestAccessorMemberInfo? result = null;
            _memberManagerComponent.TryGetMembers = (t, m, f, r, meta) =>
            {
                t.ShouldEqual(GetType());
                r.ShouldEqual(memberName);
                m.HasFlag(MemberType.Accessor).ShouldBeTrue();
                f.HasFlag(_component.MemberFlags & ~MemberFlags.Static).ShouldBeTrue();
                meta.ShouldEqual(metadataContext);
                return result;
            };

            var expressionNode = new MemberExpressionNode(ConstantExpressionNode.Get(this), memberName);
            var build = _component.TryBuild(_context, expressionNode)!;

            var invokeCount = 0;
            result = new TestAccessorMemberInfo
            {
                GetValue = (o, context) =>
                {
                    ++invokeCount;
                    o.ShouldEqual(this);
                    context.ShouldEqual(DefaultMetadata);
                    return InstanceProperty;
                },
                Type = typeof(string)
            };

            build.Invoke(new[] {_context.MetadataExpression}, DefaultMetadata).ShouldEqual(InstanceProperty);
            InstanceProperty = "f";
            build.Invoke(new[] {_context.MetadataExpression}, DefaultMetadata).ShouldEqual(InstanceProperty);
            invokeCount.ShouldEqual(2);
        }

        [Fact]
        public void TryBuildShouldBuildMemberAccessInstanceUnderlyingMember()
        {
            const string memberName = nameof(InstanceProperty);
            var metadataContext = _context.Metadata;
            var result = new TestAccessorMemberInfo
            {
                UnderlyingMember = GetType().GetProperty(memberName)
            };
            _memberManagerComponent.TryGetMembers = (t, m, f, r, meta) =>
            {
                t.ShouldEqual(GetType());
                r.ShouldEqual(memberName);
                m.HasFlag(MemberType.Accessor).ShouldBeTrue();
                f.HasFlag(_component.MemberFlags & ~MemberFlags.Static).ShouldBeTrue();
                meta.ShouldEqual(metadataContext);
                return result;
            };

            var expressionNode = new MemberExpressionNode(ConstantExpressionNode.Get(this), memberName);
            var build = _component.TryBuild(_context, expressionNode)!;

            build.Invoke().ShouldEqual(InstanceProperty);
            InstanceProperty = "f";
            build.Invoke().ShouldEqual(InstanceProperty);
        }

        [Fact]
        public void TryBuildShouldBuildMemberAccessStatic()
        {
            const string memberName = nameof(StaticProperty);
            var metadataContext = _context.Metadata;
            TestAccessorMemberInfo result = new()
            {
                GetValue = (o, context) =>
                {
                    o.ShouldBeNull();
                    context.ShouldEqual(DefaultMetadata);
                    return StaticProperty;
                },
                Type = typeof(string)
            };
            _memberManagerComponent.TryGetMembers = (t, m, f, r, meta) =>
            {
                t.ShouldEqual(GetType());
                r.ShouldEqual(memberName);
                m.HasFlag(MemberType.Accessor).ShouldBeTrue();
                f.HasFlag(_component.MemberFlags & ~MemberFlags.Instance).ShouldBeTrue();
                meta.ShouldEqual(metadataContext);
                return result;
            };

            var expressionNode = new MemberExpressionNode(ConstantExpressionNode.Get(GetType()), memberName);
            var build = _component.TryBuild(_context, expressionNode)!;

            build.Invoke(new[] {_context.MetadataExpression}, DefaultMetadata).ShouldEqual(StaticProperty);
            StaticProperty = "f";
            build.Invoke(new[] {_context.MetadataExpression}, DefaultMetadata).ShouldEqual(StaticProperty);
        }

        [Fact]
        public void TryBuildShouldBuildMemberAccessStaticIgnoreDynamic()
        {
            const string memberName = nameof(StaticProperty);
            var metadataContext = _context.Metadata;
            _memberManagerComponent.TryGetMembers = (t, m, f, r, meta) =>
            {
                t.ShouldEqual(GetType());
                r.ShouldEqual(memberName);
                m.HasFlag(MemberType.Accessor).ShouldBeTrue();
                f.HasFlag(_component.MemberFlags & ~MemberFlags.Instance).ShouldBeTrue();
                meta.ShouldEqual(metadataContext);
                return default;
            };

            var expressionNode = new MemberExpressionNode(ConstantExpressionNode.Get(GetType()), memberName);
            _component.TryBuild(_context, expressionNode).ShouldBeNull();
        }

        [Fact]
        public void TryBuildShouldBuildMemberAccessStaticUnderlyingMember()
        {
            const string memberName = nameof(StaticProperty);
            var metadataContext = _context.Metadata;
            TestAccessorMemberInfo result = new()
            {
                UnderlyingMember = GetType().GetProperty(memberName)
            };
            _memberManagerComponent.TryGetMembers = (t, m, f, r, meta) =>
            {
                t.ShouldEqual(GetType());
                r.ShouldEqual(memberName);
                m.HasFlag(MemberType.Accessor).ShouldBeTrue();
                f.HasFlag(_component.MemberFlags & ~MemberFlags.Instance).ShouldBeTrue();
                meta.ShouldEqual(metadataContext);
                return result;
            };

            var expressionNode = new MemberExpressionNode(ConstantExpressionNode.Get(GetType()), memberName);
            var build = _component.TryBuild(_context, expressionNode)!;

            build.Invoke().ShouldEqual(StaticProperty);
            StaticProperty = "f";
            build.Invoke().ShouldEqual(StaticProperty);
        }

        [Fact]
        public void TryBuildShouldIgnoreNotMemberExpression() => _component.TryBuild(_context, ConstantExpressionNode.False).ShouldBeNull();

        [Fact]
        public void TryBuildShouldThrowAccessInstanceDynamic()
        {
            const string memberName = nameof(InstanceProperty);
            var metadataContext = _context.Metadata;
            TestAccessorMemberInfo? result = null;
            _memberManagerComponent.TryGetMembers = (t, m, f, r, meta) =>
            {
                t.ShouldEqual(GetType());
                r.ShouldEqual(memberName);
                m.HasFlag(MemberType.Accessor).ShouldBeTrue();
                f.HasFlag(_component.MemberFlags & ~MemberFlags.Static).ShouldBeTrue();
                meta.ShouldEqual(metadataContext);
                return result;
            };

            var expressionNode = new MemberExpressionNode(ConstantExpressionNode.Get(this), memberName);
            var build = _component.TryBuild(_context, expressionNode)!;
            ShouldThrow(() => build.Invoke(new[] {_context.MetadataExpression}, DefaultMetadata));
        }

        public static string? StaticProperty { get; set; }

        public string? InstanceProperty { get; set; }
    }
}