using System;
using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Core;
using MugenMvvm.Binding.Core.Components;
using MugenMvvm.Binding.Core.Components.Binding;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Core;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Binding.Members;
using MugenMvvm.Binding.Observers.MemberPaths;
using MugenMvvm.Binding.Parsing.Expressions;
using MugenMvvm.Binding.Parsing.Visitors;
using MugenMvvm.Extensions;
using MugenMvvm.UnitTest.Binding.Members.Internal;
using MugenMvvm.UnitTest.Binding.Parsing.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Binding.Core.Components
{
    public class BindingInitializerTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void InitializeShouldIgnoreHasEventComponent()
        {
            var context = new BindingExpressionInitializerContext(this);
            var bindingManager = new BindingManager();
            var component = new BindingInitializer();
            bindingManager.AddComponent(component);
            var target = new TestExpressionNode
            {
                Visit = (visitor, metadataContext) => throw new NotSupportedException()
            };
            var source = new TestExpressionNode
            {
                Visit = (visitor, metadataContext) => throw new NotSupportedException()
            };
            context.Initialize(this, this, target, source, default, DefaultMetadata);
            context.BindingComponents[BindingParameterNameConstant.EventHandler] = null;
            component.Initialize(context);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void InitializeShouldRespectSettings(bool parametersSetting)
        {
            var flags = BindingMemberExpressionFlags.StablePath | BindingMemberExpressionFlags.Observable | BindingMemberExpressionFlags.ObservableMethod | BindingMemberExpressionFlags.Optional;
            var ignoreMethodMembers = true;
            var ignoreIndexMembers = true;
            var memberFlags = MemberFlags.Static;
            var context = new BindingExpressionInitializerContext(this);
            var bindingManager = new BindingManager();
            var component = new BindingInitializer();
            bindingManager.AddComponent(component);
            component.Flags.ShouldNotEqual(flags);
            component.IgnoreIndexMembers.ShouldNotEqual(ignoreIndexMembers);
            component.IgnoreMethodMembers.ShouldNotEqual(ignoreMethodMembers);
            component.MemberFlags.ShouldNotEqual(memberFlags);
            component.MemberFlags = memberFlags;

            IExpressionNode[] parameters;
            if (parametersSetting)
            {
                parameters = new IExpressionNode[]
                {
                    new MemberExpressionNode(null, BindingParameterNameConstant.IgnoreMethodMembers),
                    new MemberExpressionNode(null, BindingParameterNameConstant.IgnoreIndexMembers),
                    new MemberExpressionNode(null, BindingParameterNameConstant.HasStablePath),
                    new MemberExpressionNode(null, BindingParameterNameConstant.ObservableMethod),
                    new MemberExpressionNode(null, BindingParameterNameConstant.Observable),
                    new MemberExpressionNode(null, BindingParameterNameConstant.Optional)
                };
            }
            else
            {
                parameters = Default.EmptyArray<IExpressionNode>();
                component.Flags = flags;
                component.IgnoreIndexMembers = ignoreIndexMembers;
                component.IgnoreMethodMembers = ignoreMethodMembers;
            }

            var targetVisitCount = 0;
            var sourceVisitCount = 0;
            var target = new TestExpressionNode
            {
                Visit = (visitor, metadataContext) =>
                {
                    ++targetVisitCount;
                    metadataContext.ShouldEqual(context.GetMetadataOrDefault());
                    var expressionVisitor = (BindingMemberExpressionVisitor) visitor;
                    expressionVisitor.Flags.ShouldEqual(flags);
                    expressionVisitor.IgnoreIndexMembers.ShouldEqual(ignoreIndexMembers);
                    expressionVisitor.IgnoreMethodMembers.ShouldEqual(ignoreMethodMembers);
                    expressionVisitor.MemberFlags.ShouldEqual(memberFlags);
                    return null;
                }
            };
            var source = new TestExpressionNode
            {
                Visit = (visitor, metadataContext) =>
                {
                    ++sourceVisitCount;
                    metadataContext.ShouldEqual(context.GetMetadataOrDefault());
                    var expressionVisitor = (BindingMemberExpressionVisitor) visitor;
                    expressionVisitor.Flags.ShouldEqual(flags);
                    expressionVisitor.IgnoreIndexMembers.ShouldEqual(ignoreIndexMembers);
                    expressionVisitor.IgnoreMethodMembers.ShouldEqual(ignoreMethodMembers);
                    expressionVisitor.MemberFlags.ShouldEqual(memberFlags);
                    return null;
                }
            };
            context.Initialize(this, this, target, source, parameters, DefaultMetadata);
            component.Initialize(context);
            targetVisitCount.ShouldEqual(1);
            sourceVisitCount.ShouldEqual(1);
            context.BindingComponents.ShouldBeEmpty();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void InitializeShouldRespectSettingsEvent(bool parametersSetting)
        {
            var cmdParameter = new object();
            var targetSrc = "";
            var sourceSrc = new object();
            var targetPath = new MultiMemberPath("Member1.Member2");
            var flags = BindingMemberExpressionFlags.StablePath | BindingMemberExpressionFlags.Observable | BindingMemberExpressionFlags.ObservableMethod | BindingMemberExpressionFlags.Optional;
            var ignoreMethodMembers = true;
            var ignoreIndexMembers = true;
            var toggleEnabledState = true;
            var memberFlags = MemberFlags.Static | MemberFlags.Public;
            var context = new BindingExpressionInitializerContext(this);

            var memberManager = new MemberManager();
            memberManager.AddComponent(new TestMemberManagerComponent
            {
                TryGetMembers = (o, type, arg3) =>
                {
                    arg3.ShouldEqual(context.GetMetadataOrDefault());
                    var request = (MemberManagerRequest) o;
                    if (request.Name == targetPath.Members[0])
                    {
                        request.Flags.ShouldEqual(MemberFlags.StaticPublic);
                        request.Type.ShouldEqual(typeof(string));
                        request.MemberTypes.ShouldEqual(MemberType.Accessor);
                        return new TestMemberAccessorInfo
                        {
                            CanRead = true,
                            GetValue = (o1, metadataContext) =>
                            {
                                o1.ShouldBeNull();
                                metadataContext.ShouldEqual(context.GetMetadataOrDefault());
                                return this;
                            }
                        };
                    }

                    if (request.Name == targetPath.Members[1])
                    {
                        request.Flags.ShouldEqual(MemberFlags.InstancePublic);
                        request.Type.ShouldEqual(GetType());
                        request.MemberTypes.ShouldEqual(MemberType.Event);
                        return new TestEventInfo
                        {
                            MemberType = MemberType.Event
                        };
                    }

                    throw new NotSupportedException();
                }
            });

            var bindingManager = new BindingManager();
            var component = new BindingInitializer(memberManager: memberManager);
            bindingManager.AddComponent(component);
            component.Flags.ShouldNotEqual(flags);
            component.IgnoreIndexMembers.ShouldNotEqual(ignoreIndexMembers);
            component.IgnoreMethodMembers.ShouldNotEqual(ignoreMethodMembers);
            component.ToggleEnabledState.ShouldNotEqual(toggleEnabledState);
            component.MemberFlags.ShouldNotEqual(memberFlags);
            component.MemberFlags = memberFlags;

            IExpressionNode[] parameters;
            if (parametersSetting)
            {
                parameters = new IExpressionNode[]
                {
                    new MemberExpressionNode(null, BindingParameterNameConstant.IgnoreMethodMembers),
                    new MemberExpressionNode(null, BindingParameterNameConstant.IgnoreIndexMembers),
                    new MemberExpressionNode(null, BindingParameterNameConstant.HasStablePath),
                    new MemberExpressionNode(null, BindingParameterNameConstant.ObservableMethod),
                    new MemberExpressionNode(null, BindingParameterNameConstant.Observable),
                    new MemberExpressionNode(null, BindingParameterNameConstant.Optional),
                    new MemberExpressionNode(null, BindingParameterNameConstant.ToggleEnabled),
                    new BinaryExpressionNode(BinaryTokenType.Assignment, new MemberExpressionNode(null, BindingParameterNameConstant.CommandParameter), ConstantExpressionNode.Get(cmdParameter))
                };
            }
            else
            {
                parameters = new[] {new BinaryExpressionNode(BinaryTokenType.Assignment, new MemberExpressionNode(null, BindingParameterNameConstant.CommandParameter), ConstantExpressionNode.Get(cmdParameter))};
                component.Flags = flags;
                component.IgnoreIndexMembers = ignoreIndexMembers;
                component.IgnoreMethodMembers = ignoreMethodMembers;
                component.ToggleEnabledState = toggleEnabledState;
            }

            var targetVisitCount = 0;
            var sourceVisitCount = 0;
            var target = new TestBindingMemberExpressionNode
            {
                GetTarget = (t, s, m) =>
                {
                    t.ShouldEqual(targetSrc);
                    s.ShouldEqual(sourceSrc);
                    m.ShouldEqual(context.GetMetadataOrDefault());
                    return (targetSrc, targetPath, memberFlags);
                },
                Visit = (visitor, metadataContext) =>
                {
                    ++targetVisitCount;
                    metadataContext.ShouldEqual(context.GetMetadataOrDefault());
                    var expressionVisitor = (BindingMemberExpressionVisitor) visitor;
                    expressionVisitor.Flags.ShouldEqual(flags);
                    expressionVisitor.IgnoreIndexMembers.ShouldEqual(ignoreIndexMembers);
                    expressionVisitor.IgnoreMethodMembers.ShouldEqual(ignoreMethodMembers);
                    expressionVisitor.MemberFlags.ShouldEqual(memberFlags);
                    return null;
                }
            };
            var source = new TestBindingMemberExpressionNode
            {
                Visit = (visitor, metadataContext) =>
                {
                    ++sourceVisitCount;
                    metadataContext.ShouldEqual(context.GetMetadataOrDefault());
                    var expressionVisitor = (BindingMemberExpressionVisitor) visitor;
                    expressionVisitor.Flags.ShouldEqual(flags & ~(BindingMemberExpressionFlags.Observable | BindingMemberExpressionFlags.ObservableMethod));
                    expressionVisitor.IgnoreIndexMembers.ShouldBeTrue();
                    expressionVisitor.IgnoreMethodMembers.ShouldBeTrue();
                    expressionVisitor.MemberFlags.ShouldEqual(memberFlags);
                    return null;
                }
            };

            context.Initialize(targetSrc, sourceSrc, target, source, parameters, DefaultMetadata);
            component.Initialize(context);
            targetVisitCount.ShouldEqual(1);
            sourceVisitCount.ShouldEqual(1);
            context.BindingComponents[BindingParameterNameConstant.Mode].ShouldBeNull();
            var bindingComponentProvider = (IBindingComponentProvider) context.BindingComponents[BindingParameterNameConstant.EventHandler]!;
            var bindingComponent = (EventHandlerBindingComponent) bindingComponentProvider.GetComponent(null!, targetSrc, sourceSrc, DefaultMetadata)!;
            bindingComponent.ToggleEnabledState.ShouldEqual(toggleEnabledState);
            bindingComponent.CommandParameter.IsEmpty.ShouldBeFalse();
            bindingComponent.CommandParameter.Parameter.ShouldEqual(cmdParameter);
            bindingComponent.CommandParameter.Expression.ShouldBeNull();
        }

        #endregion
    }
}