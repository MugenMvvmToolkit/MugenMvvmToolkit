using System;
using MugenMvvm.Binding.Compiling;
using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Core;
using MugenMvvm.Binding.Core.Components;
using MugenMvvm.Binding.Core.Components.Binding;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Core;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Binding.Members;
using MugenMvvm.Binding.Metadata;
using MugenMvvm.Binding.Observers.MemberPaths;
using MugenMvvm.Binding.Parsing.Expressions;
using MugenMvvm.Binding.Parsing.Visitors;
using MugenMvvm.Extensions;
using MugenMvvm.Metadata;
using MugenMvvm.UnitTest.Binding.Compiling.Internal;
using MugenMvvm.UnitTest.Binding.Core.Internal;
using MugenMvvm.UnitTest.Binding.Members.Internal;
using MugenMvvm.UnitTest.Binding.Observers.Internal;
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
                    var expressionVisitor = (BindingMemberExpressionVisitor)visitor;
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
                    var expressionVisitor = (BindingMemberExpressionVisitor)visitor;
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
        [InlineData(true, 1)]
        [InlineData(true, 2)]
        [InlineData(true, 3)]
        [InlineData(false, 1)]
        [InlineData(false, 2)]
        [InlineData(false, 3)]
        public void InitializeShouldRespectSettingsEvent(bool parametersSetting, int cmdParameterMode)
        {
            var binding = new TestBinding();
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
                    var request = (MemberManagerRequest)o;
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

            var parameterVisitCount = 0;
            IExpressionNode cmdParameterNode;
            var exp = new TestCompiledExpression();
            object cmdParameter;
            var compiler = new ExpressionCompiler();
            if (cmdParameterMode == 1)
            {
                cmdParameter = new object();
                cmdParameterNode = ConstantExpressionNode.Get(cmdParameter);
            }
            else if (cmdParameterMode == 2 || cmdParameterMode == 3)
            {
                cmdParameter = new TestMemberPathObserver();
                cmdParameterNode = new TestBindingMemberExpressionNode
                {
                    GetBindingSource = (t, s, m) =>
                    {
                        t.ShouldEqual(targetSrc);
                        s.ShouldEqual(sourceSrc);
                        m.ShouldEqual(context.GetMetadataOrDefault());
                        return cmdParameter;
                    },
                    Visit = (visitor, metadataContext) =>
                    {
                        ++parameterVisitCount;
                        metadataContext.ShouldEqual(context.GetMetadataOrDefault());
                        if (visitor is BindingMemberExpressionVisitor expressionVisitor)
                        {
                            expressionVisitor.Flags.ShouldEqual(flags & ~BindingMemberExpressionFlags.ObservableMethod);
                            expressionVisitor.IgnoreIndexMembers.ShouldBeTrue();
                            expressionVisitor.IgnoreMethodMembers.ShouldBeTrue();
                            expressionVisitor.MemberFlags.ShouldEqual(memberFlags);
                        }
                        return null;
                    }
                };
                if (cmdParameterMode == 3)
                {
                    cmdParameterNode = new UnaryExpressionNode(UnaryTokenType.Minus, cmdParameterNode);
                    cmdParameter = new TestMemberPathObserver();
                    compiler.AddComponent(new TestExpressionCompilerComponent
                    {
                        TryCompile = (node, m) =>
                        {
                            node.ShouldEqual(cmdParameterNode);
                            m.ShouldEqual(context.GetMetadataOrDefault());
                            return exp;
                        }
                    });
                }
            }
            else
                throw new NotSupportedException();

            var bindingManager = new BindingManager();
            var component = new BindingInitializer(compiler, memberManager);
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
                    new MemberExpressionNode(null, BindingInitializer.OneTimeBindingMode),
                    new BinaryExpressionNode(BinaryTokenType.Assignment, new MemberExpressionNode(null, BindingParameterNameConstant.CommandParameter), cmdParameterNode)
                };
            }
            else
            {
                binding.Metadata = MetadataContextValue.Create(BindingMetadata.IsMultiBinding, false).ToContext();
                parameters = new[] { new BinaryExpressionNode(BinaryTokenType.Assignment, new MemberExpressionNode(null, BindingParameterNameConstant.CommandParameter), cmdParameterNode) };
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
                    var expressionVisitor = (BindingMemberExpressionVisitor)visitor;
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
                    var expressionVisitor = (BindingMemberExpressionVisitor)visitor;
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
            var bindingComponentProvider = (IBindingComponentProvider)context.BindingComponents[BindingParameterNameConstant.EventHandler]!;
            var bindingComponent = (EventHandlerBindingComponent)bindingComponentProvider.GetComponent(binding, targetSrc, sourceSrc, DefaultMetadata)!;
            if (parametersSetting)
                bindingComponent.ShouldBeType<EventHandlerBindingComponent>();
            else
                bindingComponent.ShouldBeType<EventHandlerBindingComponent.OneWay>();
            bindingComponent.ToggleEnabledState.ShouldEqual(toggleEnabledState);

            if (cmdParameterMode == 1)
            {
                parameterVisitCount.ShouldEqual(0);
                bindingComponent.CommandParameter.IsEmpty.ShouldBeFalse();
                bindingComponent.CommandParameter.Parameter.ShouldEqual(cmdParameter);
                bindingComponent.CommandParameter.Expression.ShouldBeNull();
            }
            else if (cmdParameterMode == 2)
            {
                parameterVisitCount.ShouldEqual(1);
                bindingComponent.CommandParameter.IsEmpty.ShouldBeFalse();
                bindingComponent.CommandParameter.Parameter.ShouldEqual(cmdParameter);
                bindingComponent.CommandParameter.Expression.ShouldBeNull();
            }
            else
            {
                parameterVisitCount.ShouldEqual(2);
                bindingComponent.CommandParameter.IsEmpty.ShouldBeFalse();
                bindingComponent.CommandParameter.Parameter.ShouldEqual(cmdParameter);
                bindingComponent.CommandParameter.Expression.ShouldEqual(exp);
            }
        }

        #endregion
    }
}