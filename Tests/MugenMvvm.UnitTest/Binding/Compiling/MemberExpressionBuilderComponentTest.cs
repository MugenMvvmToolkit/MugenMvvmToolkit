using MugenMvvm.Binding.Compiling.Components;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Extensions;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Members;
using MugenMvvm.Binding.Parsing.Expressions;
using MugenMvvm.Extensions;
using MugenMvvm.UnitTest.Binding.Members;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Binding.Compiling
{
    public class MemberExpressionBuilderComponentTest : UnitTestBase
    {
        #region Fields

        private readonly MemberExpressionBuilderComponent _component;
        private readonly TestMemberManagerComponent _memberManagerComponent;

        #endregion

        #region Constructors

        public MemberExpressionBuilderComponentTest()
        {
            IMemberProvider memberProvider = new MemberProvider();
            _memberManagerComponent = new TestMemberManagerComponent();
            memberProvider.AddComponent(_memberManagerComponent);
            _component = new MemberExpressionBuilderComponent(memberProvider);
        }

        #endregion

        #region Properties

        public static string? StaticProperty { get; set; }

        public string? InstanceProperty { get; set; }

        #endregion

        #region Methods

        [Fact]
        public void TryBuildShouldIgnoreNotMemberExpression()
        {
            var ctx = new TestExpressionBuilderContext();
            _component.TryBuild(ctx, ConstantExpressionNode.False).ShouldBeNull();
        }

        [Fact]
        public void TryBuildShouldBuildMemberAccessInstance()
        {
            const string memberName = nameof(InstanceProperty);
            var ctx = new TestExpressionBuilderContext();
            var metadataContext = ctx.Metadata;
            IMemberAccessorInfo result = new TestMemberAccessorInfo
            {
                GetValue = (o, context) =>
                {
                    o.ShouldEqual(this);
                    context.ShouldEqual(DefaultMetadata);
                    return InstanceProperty;
                },
                Type = typeof(string)
            };
            _memberManagerComponent.TryGetMember = (type, s, memberType, flags, context) =>
            {
                type.ShouldEqual(GetType());
                s.ShouldEqual(memberName);
                memberType.HasFlagEx(MemberType.Accessor).ShouldBeTrue();
                flags.HasFlagEx(_component.MemberFlags & ~MemberFlags.Static).ShouldBeTrue();
                context.ShouldEqual(metadataContext);
                return result;
            };

            var expressionNode = new MemberExpressionNode(ConstantExpressionNode.Get(this), memberName);
            var build = _component.TryBuild(ctx, expressionNode)!;

            build.Invoke(new[] {ctx.MetadataExpression}, DefaultMetadata).ShouldEqual(InstanceProperty);
            InstanceProperty = "f";
            build.Invoke(new[] {ctx.MetadataExpression}, DefaultMetadata).ShouldEqual(InstanceProperty);
        }

        [Fact]
        public void TryBuildShouldBuildMemberAccessInstanceUnderlyingMember()
        {
            const string memberName = nameof(InstanceProperty);
            var ctx = new TestExpressionBuilderContext();
            var metadataContext = ctx.Metadata;
            IMemberAccessorInfo result = new TestMemberAccessorInfo
            {
                UnderlyingMember = GetType().GetProperty(memberName)
            };
            _memberManagerComponent.TryGetMember = (type, s, memberType, flags, context) =>
            {
                type.ShouldEqual(GetType());
                s.ShouldEqual(memberName);
                memberType.HasFlagEx(MemberType.Accessor).ShouldBeTrue();
                flags.HasFlagEx(_component.MemberFlags & ~MemberFlags.Static).ShouldBeTrue();
                context.ShouldEqual(metadataContext);
                return result;
            };

            var expressionNode = new MemberExpressionNode(ConstantExpressionNode.Get(this), memberName);
            var build = _component.TryBuild(ctx, expressionNode)!;

            build.Invoke().ShouldEqual(InstanceProperty);
            InstanceProperty = "f";
            build.Invoke().ShouldEqual(InstanceProperty);
        }

        [Fact]
        public void TryBuildShouldBuildMemberAccessInstanceDynamic()
        {
            const string memberName = nameof(InstanceProperty);
            var ctx = new TestExpressionBuilderContext();
            var metadataContext = ctx.Metadata;
            IMemberAccessorInfo? result = null;
            _memberManagerComponent.TryGetMember = (type, s, memberType, flags, context) =>
            {
                type.ShouldEqual(GetType());
                s.ShouldEqual(memberName);
                memberType.HasFlagEx(MemberType.Accessor).ShouldBeTrue();
                flags.HasFlagEx(_component.MemberFlags & ~MemberFlags.Static).ShouldBeTrue();
                context.ShouldEqual(metadataContext);
                return result;
            };

            var expressionNode = new MemberExpressionNode(ConstantExpressionNode.Get(this), memberName);
            var build = _component.TryBuild(ctx, expressionNode)!;

            var invokeCount = 0;
            result = new TestMemberAccessorInfo
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

            build.Invoke(new[] {ctx.MetadataExpression}, DefaultMetadata).ShouldEqual(InstanceProperty);
            InstanceProperty = "f";
            build.Invoke(new[] {ctx.MetadataExpression}, DefaultMetadata).ShouldEqual(InstanceProperty);
            invokeCount.ShouldEqual(2);
        }

        [Fact]
        public void TryBuildShouldThrowAccessInstanceDynamic()
        {
            const string memberName = nameof(InstanceProperty);
            var ctx = new TestExpressionBuilderContext();
            var metadataContext = ctx.Metadata;
            IMemberAccessorInfo? result = null;
            _memberManagerComponent.TryGetMember = (type, s, memberType, flags, context) =>
            {
                type.ShouldEqual(GetType());
                s.ShouldEqual(memberName);
                memberType.HasFlagEx(MemberType.Accessor).ShouldBeTrue();
                flags.HasFlagEx(_component.MemberFlags & ~MemberFlags.Static).ShouldBeTrue();
                context.ShouldEqual(metadataContext);
                return result;
            };

            var expressionNode = new MemberExpressionNode(ConstantExpressionNode.Get(this), memberName);
            var build = _component.TryBuild(ctx, expressionNode)!;
            ShouldThrow(() => build.Invoke(new[] {ctx.MetadataExpression}, DefaultMetadata));
        }


        [Fact]
        public void TryBuildShouldBuildMemberAccessStatic()
        {
            const string memberName = nameof(StaticProperty);
            var ctx = new TestExpressionBuilderContext();
            var metadataContext = ctx.Metadata;
            IMemberAccessorInfo result = new TestMemberAccessorInfo
            {
                GetValue = (o, context) =>
                {
                    o.ShouldBeNull();
                    context.ShouldEqual(DefaultMetadata);
                    return StaticProperty;
                },
                Type = typeof(string)
            };
            _memberManagerComponent.TryGetMember = (type, s, memberType, flags, context) =>
            {
                type.ShouldEqual(GetType());
                s.ShouldEqual(memberName);
                memberType.HasFlagEx(MemberType.Accessor).ShouldBeTrue();
                flags.HasFlagEx(_component.MemberFlags & ~MemberFlags.Instance).ShouldBeTrue();
                context.ShouldEqual(metadataContext);
                return result;
            };

            var expressionNode = new MemberExpressionNode(ConstantExpressionNode.Get(GetType()), memberName);
            var build = _component.TryBuild(ctx, expressionNode)!;

            build.Invoke(new[] {ctx.MetadataExpression}, DefaultMetadata).ShouldEqual(StaticProperty);
            StaticProperty = "f";
            build.Invoke(new[] {ctx.MetadataExpression}, DefaultMetadata).ShouldEqual(StaticProperty);
        }

        [Fact]
        public void TryBuildShouldBuildMemberAccessStaticUnderlyingMember()
        {
            const string memberName = nameof(StaticProperty);
            var ctx = new TestExpressionBuilderContext();
            var metadataContext = ctx.Metadata;
            IMemberAccessorInfo result = new TestMemberAccessorInfo
            {
                UnderlyingMember = GetType().GetProperty(memberName)
            };
            _memberManagerComponent.TryGetMember = (type, s, memberType, flags, context) =>
            {
                type.ShouldEqual(GetType());
                s.ShouldEqual(memberName);
                memberType.HasFlagEx(MemberType.Accessor).ShouldBeTrue();
                flags.HasFlagEx(_component.MemberFlags & ~MemberFlags.Instance).ShouldBeTrue();
                context.ShouldEqual(metadataContext);
                return result;
            };

            var expressionNode = new MemberExpressionNode(ConstantExpressionNode.Get(GetType()), memberName);
            var build = _component.TryBuild(ctx, expressionNode)!;

            build.Invoke().ShouldEqual(StaticProperty);
            StaticProperty = "f";
            build.Invoke().ShouldEqual(StaticProperty);
        }

        [Fact]
        public void TryBuildShouldBuildMemberAccessStaticIgnoreDynamic()
        {
            const string memberName = nameof(StaticProperty);
            var ctx = new TestExpressionBuilderContext();
            var metadataContext = ctx.Metadata;
            _memberManagerComponent.TryGetMember = (type, s, memberType, flags, context) =>
            {
                type.ShouldEqual(GetType());
                s.ShouldEqual(memberName);
                memberType.HasFlagEx(MemberType.Accessor).ShouldBeTrue();
                flags.HasFlagEx(_component.MemberFlags & ~MemberFlags.Instance).ShouldBeTrue();
                context.ShouldEqual(metadataContext);
                return null;
            };

            var expressionNode = new MemberExpressionNode(ConstantExpressionNode.Get(GetType()), memberName);
            _component.TryBuild(ctx, expressionNode).ShouldBeNull();
        }

        #endregion
    }
}