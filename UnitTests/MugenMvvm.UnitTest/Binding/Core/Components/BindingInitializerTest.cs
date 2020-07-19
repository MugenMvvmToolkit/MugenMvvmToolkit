using System;
using MugenMvvm.Binding.Compiling;
using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Core;
using MugenMvvm.Binding.Core.Components;
using MugenMvvm.Binding.Core.Components.Binding;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Extensions;
using MugenMvvm.Binding.Interfaces.Core;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Binding.Members;
using MugenMvvm.Binding.Metadata;
using MugenMvvm.Binding.Observation;
using MugenMvvm.Binding.Observation.Paths;
using MugenMvvm.Binding.Parsing.Expressions;
using MugenMvvm.Binding.Parsing.Expressions.Binding;
using MugenMvvm.Binding.Parsing.Visitors;
using MugenMvvm.Extensions;
using MugenMvvm.Internal;
using MugenMvvm.Metadata;
using MugenMvvm.UnitTest.Binding.Compiling.Internal;
using MugenMvvm.UnitTest.Binding.Core.Internal;
using MugenMvvm.UnitTest.Binding.Members.Internal;
using MugenMvvm.UnitTest.Binding.Observation.Internal;
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
                VisitHandler = (visitor, metadataContext) => throw new NotSupportedException()
            };
            var source = new TestExpressionNode
            {
                VisitHandler = (visitor, metadataContext) => throw new NotSupportedException()
            };
            context.Initialize(this, this, target, source, default, DefaultMetadata);
            context.BindingComponents[BindingParameterNameConstant.EventHandler] = null;
            component.Initialize(null!, context);
        }

        [Fact]
        public void ShouldUseParentDataContextForDataContextBind()
        {
            var context = new BindingExpressionInitializerContext(this);
            var bindingManager = new BindingManager();
            var component = new BindingInitializer();
            bindingManager.AddComponent(component);

            var sourceVisitCount = 0;
            var target = new TestBindingMemberExpressionNode(BindableMembers.For<object>().DataContext())
            {
                GetSource = (o, o1, arg3) => (this, EmptyMemberPath.Instance, MemberFlags.Instance)
            };
            var source = new TestExpressionNode
            {
                VisitHandler = (visitor, metadataContext) =>
                {
                    ++sourceVisitCount;
                    metadataContext.ShouldEqual(context.GetMetadataOrDefault());
                    var expressionVisitor = (BindingMemberExpressionVisitor)visitor;
                    expressionVisitor.Flags.HasFlagEx(BindingMemberExpressionFlags.DataContextPath);
                    return null;
                }
            };
            context.Initialize(this, this, target, source, default, DefaultMetadata);
            component.Initialize(null!, context);
            sourceVisitCount.ShouldEqual(1);
            context.BindingComponents.ShouldBeEmpty();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void InitializeShouldRespectSettings(bool parametersSetting)
        {
            var flags = BindingMemberExpressionFlags.StablePath | BindingMemberExpressionFlags.Observable | BindingMemberExpressionFlags.ObservableMethods | BindingMemberExpressionFlags.Optional;
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
                    new MemberExpressionNode(null, BindingParameterNameConstant.ObservableMethods),
                    new MemberExpressionNode(null, BindingParameterNameConstant.Observable),
                    new MemberExpressionNode(null, BindingParameterNameConstant.Optional)
                };
            }
            else
            {
                parameters = Default.Array<IExpressionNode>();
                component.Flags = flags;
                component.IgnoreIndexMembers = ignoreIndexMembers;
                component.IgnoreMethodMembers = ignoreMethodMembers;
            }

            var targetVisitCount = 0;
            var sourceVisitCount = 0;
            var target = new TestExpressionNode
            {
                VisitHandler = (visitor, metadataContext) =>
                {
                    ++targetVisitCount;
                    metadataContext.ShouldEqual(context.GetMetadataOrDefault());
                    var expressionVisitor = (BindingMemberExpressionVisitor)visitor;
                    expressionVisitor.Flags.ShouldEqual(flags.SetTargetFlags(true));
                    expressionVisitor.IgnoreIndexMembers.ShouldEqual(ignoreIndexMembers);
                    expressionVisitor.IgnoreMethodMembers.ShouldEqual(ignoreMethodMembers);
                    expressionVisitor.MemberFlags.ShouldEqual(memberFlags);
                    return null;
                }
            };
            var source = new TestExpressionNode
            {
                VisitHandler = (visitor, metadataContext) =>
                {
                    ++sourceVisitCount;
                    metadataContext.ShouldEqual(context.GetMetadataOrDefault());
                    var expressionVisitor = (BindingMemberExpressionVisitor)visitor;
                    expressionVisitor.Flags.ShouldEqual(flags.SetTargetFlags(false));
                    expressionVisitor.IgnoreIndexMembers.ShouldEqual(ignoreIndexMembers);
                    expressionVisitor.IgnoreMethodMembers.ShouldEqual(ignoreMethodMembers);
                    expressionVisitor.MemberFlags.ShouldEqual(memberFlags);
                    return null;
                }
            };
            context.Initialize(this, this, target, source, parameters, DefaultMetadata);
            component.Initialize(null!, context);
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
            var flags = BindingMemberExpressionFlags.StablePath | BindingMemberExpressionFlags.Observable | BindingMemberExpressionFlags.ObservableMethods | BindingMemberExpressionFlags.Optional;
            var ignoreMethodMembers = true;
            var ignoreIndexMembers = true;
            var toggleEnabledState = true;
            var memberFlags = MemberFlags.Static | MemberFlags.Public;
            var context = new BindingExpressionInitializerContext(this);

            var memberManager = new MemberManager();
            memberManager.AddComponent(new TestMemberManagerComponent
            {
                TryGetMembers = (t, m, f, r, meta) =>
                {
                    meta.ShouldEqual(context.GetMetadataOrDefault());
                    if (r.Equals(targetPath.Members[0]))
                    {
                        f.ShouldEqual(MemberFlags.StaticPublic);
                        t.ShouldEqual(typeof(string));
                        m.ShouldEqual(MemberType.Accessor);
                        return new TestAccessorMemberInfo
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

                    if (r.Equals(targetPath.Members[1]))
                    {
                        f.ShouldEqual(MemberFlags.InstancePublic);
                        t.ShouldEqual(GetType());
                        m.ShouldEqual(MemberType.Event);
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
                    VisitHandler = (visitor, metadataContext) =>
                    {
                        ++parameterVisitCount;
                        metadataContext.ShouldEqual(context.GetMetadataOrDefault());
                        if (visitor is BindingMemberExpressionVisitor expressionVisitor)
                        {
                            expressionVisitor.Flags.ShouldEqual(flags.SetTargetFlags(false) & ~BindingMemberExpressionFlags.ObservableMethods);
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
                    new MemberExpressionNode(null, BindingParameterNameConstant.ObservableMethods),
                    new MemberExpressionNode(null, BindingParameterNameConstant.Observable),
                    new MemberExpressionNode(null, BindingParameterNameConstant.Optional),
                    new MemberExpressionNode(null, BindingParameterNameConstant.ToggleEnabled),
                    new MemberExpressionNode(null, component.OneTimeBindingMode),
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
                GetSource = (t, s, m) =>
                {
                    t.ShouldEqual(targetSrc);
                    s.ShouldEqual(sourceSrc);
                    m.ShouldEqual(context.GetMetadataOrDefault());
                    return (targetSrc, targetPath, memberFlags);
                },
                VisitHandler = (visitor, metadataContext) =>
                {
                    ++targetVisitCount;
                    metadataContext.ShouldEqual(context.GetMetadataOrDefault());
                    var expressionVisitor = (BindingMemberExpressionVisitor)visitor;
                    expressionVisitor.Flags.ShouldEqual(flags.SetTargetFlags(true));
                    expressionVisitor.IgnoreIndexMembers.ShouldEqual(ignoreIndexMembers);
                    expressionVisitor.IgnoreMethodMembers.ShouldEqual(ignoreMethodMembers);
                    expressionVisitor.MemberFlags.ShouldEqual(memberFlags);
                    return null;
                }
            };
            var source = new TestBindingMemberExpressionNode
            {
                VisitHandler = (visitor, metadataContext) =>
                {
                    ++sourceVisitCount;
                    metadataContext.ShouldEqual(context.GetMetadataOrDefault());
                    var expressionVisitor = (BindingMemberExpressionVisitor)visitor;
                    expressionVisitor.Flags.ShouldEqual(flags.SetTargetFlags(false) & ~(BindingMemberExpressionFlags.Observable | BindingMemberExpressionFlags.ObservableMethods));
                    expressionVisitor.IgnoreIndexMembers.ShouldBeTrue();
                    expressionVisitor.IgnoreMethodMembers.ShouldBeTrue();
                    expressionVisitor.MemberFlags.ShouldEqual(memberFlags);
                    return null;
                }
            };

            context.Initialize(targetSrc, sourceSrc, target, source, parameters, DefaultMetadata);
            component.Initialize(null!, context);
            targetVisitCount.ShouldEqual(1);
            sourceVisitCount.ShouldEqual(1);
            context.BindingComponents[BindingParameterNameConstant.Mode].ShouldBeNull();
            var bindingComponentProvider = (IBindingComponentProvider)context.BindingComponents[BindingParameterNameConstant.EventHandler]!;
            var bindingComponent = (EventHandlerBindingComponent)bindingComponentProvider.TryGetComponent(binding, targetSrc, sourceSrc, DefaultMetadata)!;
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