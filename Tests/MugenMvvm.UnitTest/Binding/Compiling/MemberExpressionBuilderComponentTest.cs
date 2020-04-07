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
            IMemberManager memberManager = new MemberManager();
            _memberManagerComponent = new TestMemberManagerComponent();
            memberManager.AddComponent(_memberManagerComponent);
            _component = new MemberExpressionBuilderComponent(memberManager);
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
            TestMemberAccessorInfo result = new TestMemberAccessorInfo
            {
                GetValue = (o, context) =>
                {
                    o.ShouldEqual(this);
                    context.ShouldEqual(DefaultMetadata);
                    return InstanceProperty;
                },
                Type = typeof(string)
            };
            _memberManagerComponent.TryGetMembers = (r, t, context) =>
            {
                var request = (MemberManagerRequest)r;
                request.Type.ShouldEqual(GetType());
                request.Name.ShouldEqual(memberName);
                request.MemberTypes.HasFlagEx(MemberType.Accessor).ShouldBeTrue();
                request.Flags.HasFlagEx(_component.MemberFlags & ~MemberFlags.Static).ShouldBeTrue();
                context.ShouldEqual(metadataContext);
                return result;
            };

            var expressionNode = new MemberExpressionNode(ConstantExpressionNode.Get(this), memberName);
            var build = _component.TryBuild(ctx, expressionNode)!;

            build.Invoke(new[] { ctx.MetadataExpression }, DefaultMetadata).ShouldEqual(InstanceProperty);
            InstanceProperty = "f";
            build.Invoke(new[] { ctx.MetadataExpression }, DefaultMetadata).ShouldEqual(InstanceProperty);
        }

        [Fact]
        public void TryBuildShouldBuildMemberAccessInstanceUnderlyingMember()
        {
            const string memberName = nameof(InstanceProperty);
            var ctx = new TestExpressionBuilderContext();
            var metadataContext = ctx.Metadata;
            TestMemberAccessorInfo result = new TestMemberAccessorInfo
            {
                UnderlyingMember = GetType().GetProperty(memberName)
            };
            _memberManagerComponent.TryGetMembers = (r, t, context) =>
            {
                var request = (MemberManagerRequest)r;
                request.Type.ShouldEqual(GetType());
                request.Name.ShouldEqual(memberName);
                request.MemberTypes.HasFlagEx(MemberType.Accessor).ShouldBeTrue();
                request.Flags.HasFlagEx(_component.MemberFlags & ~MemberFlags.Static).ShouldBeTrue();
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
            TestMemberAccessorInfo? result = null;
            _memberManagerComponent.TryGetMembers = (r, t, context) =>
            {
                var request = (MemberManagerRequest)r;
                request.Type.ShouldEqual(GetType());
                request.Name.ShouldEqual(memberName);
                request.MemberTypes.HasFlagEx(MemberType.Accessor).ShouldBeTrue();
                request.Flags.HasFlagEx(_component.MemberFlags & ~MemberFlags.Static).ShouldBeTrue();
                context.ShouldEqual(metadataContext);
                return result!;
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

            build.Invoke(new[] { ctx.MetadataExpression }, DefaultMetadata).ShouldEqual(InstanceProperty);
            InstanceProperty = "f";
            build.Invoke(new[] { ctx.MetadataExpression }, DefaultMetadata).ShouldEqual(InstanceProperty);
            invokeCount.ShouldEqual(2);
        }

        [Fact]
        public void TryBuildShouldThrowAccessInstanceDynamic()
        {
            const string memberName = nameof(InstanceProperty);
            var ctx = new TestExpressionBuilderContext();
            var metadataContext = ctx.Metadata;
            TestMemberAccessorInfo? result = null;
            _memberManagerComponent.TryGetMembers = (r, t, context) =>
            {
                var request = (MemberManagerRequest)r;
                request.Type.ShouldEqual(GetType());
                request.Name.ShouldEqual(memberName);
                request.MemberTypes.HasFlagEx(MemberType.Accessor).ShouldBeTrue();
                request.Flags.HasFlagEx(_component.MemberFlags & ~MemberFlags.Static).ShouldBeTrue();
                context.ShouldEqual(metadataContext);
                return result!;
            };

            var expressionNode = new MemberExpressionNode(ConstantExpressionNode.Get(this), memberName);
            var build = _component.TryBuild(ctx, expressionNode)!;
            ShouldThrow(() => build.Invoke(new[] { ctx.MetadataExpression }, DefaultMetadata));
        }


        [Fact]
        public void TryBuildShouldBuildMemberAccessStatic()
        {
            const string memberName = nameof(StaticProperty);
            var ctx = new TestExpressionBuilderContext();
            var metadataContext = ctx.Metadata;
            TestMemberAccessorInfo result = new TestMemberAccessorInfo
            {
                GetValue = (o, context) =>
                {
                    o.ShouldBeNull();
                    context.ShouldEqual(DefaultMetadata);
                    return StaticProperty;
                },
                Type = typeof(string)
            };
            _memberManagerComponent.TryGetMembers = (r, t, context) =>
            {
                var request = (MemberManagerRequest)r;
                request.Type.ShouldEqual(GetType());
                request.Name.ShouldEqual(memberName);
                request.MemberTypes.HasFlagEx(MemberType.Accessor).ShouldBeTrue();
                request.Flags.HasFlagEx(_component.MemberFlags & ~MemberFlags.Instance).ShouldBeTrue();
                context.ShouldEqual(metadataContext);
                return result;
            };

            var expressionNode = new MemberExpressionNode(ConstantExpressionNode.Get(GetType()), memberName);
            var build = _component.TryBuild(ctx, expressionNode)!;

            build.Invoke(new[] { ctx.MetadataExpression }, DefaultMetadata).ShouldEqual(StaticProperty);
            StaticProperty = "f";
            build.Invoke(new[] { ctx.MetadataExpression }, DefaultMetadata).ShouldEqual(StaticProperty);
        }

        [Fact]
        public void TryBuildShouldBuildMemberAccessStaticUnderlyingMember()
        {
            const string memberName = nameof(StaticProperty);
            var ctx = new TestExpressionBuilderContext();
            var metadataContext = ctx.Metadata;
            TestMemberAccessorInfo result = new TestMemberAccessorInfo
            {
                UnderlyingMember = GetType().GetProperty(memberName)
            };
            _memberManagerComponent.TryGetMembers = (r, t, context) =>
            {
                var request = (MemberManagerRequest)r;
                request.Type.ShouldEqual(GetType());
                request.Name.ShouldEqual(memberName);
                request.MemberTypes.HasFlagEx(MemberType.Accessor).ShouldBeTrue();
                request.Flags.HasFlagEx(_component.MemberFlags & ~MemberFlags.Instance).ShouldBeTrue();
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
            _memberManagerComponent.TryGetMembers = (r, t, context) =>
            {
                var request = (MemberManagerRequest)r;
                request.Type.ShouldEqual(GetType());
                request.Name.ShouldEqual(memberName);
                request.MemberTypes.HasFlagEx(MemberType.Accessor).ShouldBeTrue();
                request.Flags.HasFlagEx(_component.MemberFlags & ~MemberFlags.Instance).ShouldBeTrue();
                context.ShouldEqual(metadataContext);
                return default;
            };

            var expressionNode = new MemberExpressionNode(ConstantExpressionNode.Get(GetType()), memberName);
            _component.TryBuild(ctx, expressionNode).ShouldBeNull();
        }

        #endregion
    }
}