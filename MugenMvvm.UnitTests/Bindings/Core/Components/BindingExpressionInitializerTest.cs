using System;
using MugenMvvm.Bindings.Compiling;
using MugenMvvm.Bindings.Constants;
using MugenMvvm.Bindings.Core;
using MugenMvvm.Bindings.Core.Components;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Bindings.Interfaces.Core;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Bindings.Members;
using MugenMvvm.Bindings.Metadata;
using MugenMvvm.Bindings.Observation;
using MugenMvvm.Bindings.Parsing.Expressions;
using MugenMvvm.Bindings.Parsing.Visitors;
using MugenMvvm.Extensions;
using MugenMvvm.UnitTests.Bindings.Compiling.Internal;
using MugenMvvm.UnitTests.Bindings.Core.Internal;
using MugenMvvm.UnitTests.Bindings.Members.Internal;
using MugenMvvm.UnitTests.Bindings.Observation.Internal;
using MugenMvvm.UnitTests.Bindings.Parsing.Internal;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Bindings.Core.Components
{
    public class BindingExpressionInitializerTest : UnitTestBase
    {
        private readonly BindingExpressionInitializerContext _context;
        private readonly BindingManager _bindingManager;
        private readonly ExpressionCompiler _compiler;
        private readonly MemberManager _memberManager;
        private readonly BindingExpressionInitializer _component;

        public BindingExpressionInitializerTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            _context = new BindingExpressionInitializerContext(this);
            _bindingManager = new BindingManager(ComponentCollectionManager);
            _compiler = new ExpressionCompiler(ComponentCollectionManager);
            _memberManager = new MemberManager(ComponentCollectionManager);
            _component = new BindingExpressionInitializer(_compiler, _memberManager);
            _bindingManager.AddComponent(_component);
        }

        [Fact]
        public void InitializeShouldIgnoreHasEventComponent()
        {
            var target = new TestExpressionNode
            {
                VisitHandler = (visitor, metadataContext) => throw new NotSupportedException()
            };
            var source = new TestExpressionNode
            {
                VisitHandler = (visitor, metadataContext) => throw new NotSupportedException()
            };
            _context.Initialize(this, this, target, source, default, DefaultMetadata);
            _context.Components[BindingParameterNameConstant.EventHandler] = null;
            _component.Initialize(_bindingManager, _context);
        }

        [Fact]
        public void ShouldUseParentDataContextForDataContextBind()
        {
            var sourceVisitCount = 0;
            var target = new TestBindingMemberExpressionNode(BindableMembers.For<object>().DataContext())
            {
                MemberFlags = MemberFlags.Instance,
                GetSource = (o, o1, arg3) => (this, MemberPath.Empty)
            };
            var source = new TestExpressionNode
            {
                VisitHandler = (visitor, metadataContext) =>
                {
                    ++sourceVisitCount;
                    metadataContext.ShouldEqual(_context.GetMetadataOrDefault());
                    var expressionVisitor = (BindingMemberExpressionVisitor) visitor;
                    expressionVisitor.Flags.HasFlag(BindingMemberExpressionFlags.ParentDataContext);
                    return null;
                }
            };
            _context.Initialize(this, this, target, source, default, DefaultMetadata);
            _component.Initialize(null!, _context);
            sourceVisitCount.ShouldEqual(1);
            _context.Components.ShouldBeEmpty();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void InitializeShouldRespectSettings(bool parametersSetting)
        {
            var flags = BindingMemberExpressionFlags.StablePath | BindingMemberExpressionFlags.Observable | BindingMemberExpressionFlags.ObservableMethods |
                        BindingMemberExpressionFlags.Optional;
            var suppressMethodAccessors = true;
            var suppressIndexAccessors = true;
            var memberFlags = MemberFlags.Static;

            _component.Flags.ShouldNotEqual(flags);
            _component.SuppressIndexAccessors.ShouldNotEqual(suppressIndexAccessors);
            _component.SuppressMethodAccessors.ShouldNotEqual(suppressMethodAccessors);
            _component.MemberFlags.ShouldNotEqual(memberFlags);
            _component.MemberFlags = memberFlags;

            IExpressionNode[]? parameters;
            if (parametersSetting)
            {
                parameters = new IExpressionNode[]
                {
                    new MemberExpressionNode(null, BindingParameterNameConstant.SuppressMethodAccessors),
                    new MemberExpressionNode(null, BindingParameterNameConstant.SuppressIndexAccessors),
                    new MemberExpressionNode(null, BindingParameterNameConstant.HasStablePath),
                    new MemberExpressionNode(null, BindingParameterNameConstant.ObservableMethods),
                    new MemberExpressionNode(null, BindingParameterNameConstant.Observable),
                    new MemberExpressionNode(null, BindingParameterNameConstant.Optional)
                };
            }
            else
            {
                parameters = default;
                _component.Flags = flags;
                _component.SuppressIndexAccessors = suppressIndexAccessors;
                _component.SuppressMethodAccessors = suppressMethodAccessors;
            }

            var targetVisitCount = 0;
            var sourceVisitCount = 0;
            var target = new TestExpressionNode
            {
                VisitHandler = (visitor, metadataContext) =>
                {
                    ++targetVisitCount;
                    metadataContext.ShouldEqual(_context.GetMetadataOrDefault());
                    var expressionVisitor = (BindingMemberExpressionVisitor) visitor;
                    expressionVisitor.Flags.ShouldEqual(flags.SetTargetFlags(true));
                    expressionVisitor.SuppressIndexAccessors.ShouldEqual(suppressIndexAccessors);
                    expressionVisitor.SuppressMethodAccessors.ShouldEqual(suppressMethodAccessors);
                    expressionVisitor.MemberFlags.ShouldEqual(memberFlags);
                    return null;
                }
            };
            var source = new TestExpressionNode
            {
                VisitHandler = (visitor, metadataContext) =>
                {
                    ++sourceVisitCount;
                    metadataContext.ShouldEqual(_context.GetMetadataOrDefault());
                    var expressionVisitor = (BindingMemberExpressionVisitor) visitor;
                    expressionVisitor.Flags.ShouldEqual(flags.SetTargetFlags(false));
                    expressionVisitor.SuppressIndexAccessors.ShouldEqual(suppressIndexAccessors);
                    expressionVisitor.SuppressMethodAccessors.ShouldEqual(suppressMethodAccessors);
                    expressionVisitor.MemberFlags.ShouldEqual(memberFlags);
                    return null;
                }
            };
            _context.Initialize(this, this, target, source, parameters, DefaultMetadata);
            _component.Initialize(null!, _context);
            targetVisitCount.ShouldEqual(1);
            sourceVisitCount.ShouldEqual(1);
            _context.Components.ShouldBeEmpty();
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
            var targetPath = MemberPath.Get("Member1.Member2");
            var flags = BindingMemberExpressionFlags.StablePath | BindingMemberExpressionFlags.Observable | BindingMemberExpressionFlags.ObservableMethods |
                        BindingMemberExpressionFlags.Optional;
            var suppressMethodAccessors = true;
            var suppressIndexAccessors = true;
            var toggleEnabledState = true;
            var memberFlags = MemberFlags.Static | MemberFlags.Public;

            _memberManager.AddComponent(new TestMemberManagerComponent
            {
                TryGetMembers = (t, m, f, r, meta) =>
                {
                    meta.ShouldEqual(_context.GetMetadataOrDefault());
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
                                metadataContext.ShouldEqual(_context.GetMetadataOrDefault());
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
                        m.ShouldEqual(_context.GetMetadataOrDefault());
                        return cmdParameter;
                    },
                    VisitHandler = (visitor, metadataContext) =>
                    {
                        ++parameterVisitCount;
                        metadataContext.ShouldEqual(_context.GetMetadataOrDefault());
                        if (visitor is BindingMemberExpressionVisitor expressionVisitor)
                        {
                            expressionVisitor.Flags.ShouldEqual(flags.SetTargetFlags(false) & ~BindingMemberExpressionFlags.ObservableMethods);
                            expressionVisitor.SuppressIndexAccessors.ShouldBeTrue();
                            expressionVisitor.SuppressMethodAccessors.ShouldBeTrue();
                            expressionVisitor.MemberFlags.ShouldEqual(memberFlags);
                        }

                        return null;
                    }
                };
                if (cmdParameterMode == 3)
                {
                    cmdParameterNode = new UnaryExpressionNode(UnaryTokenType.Minus, cmdParameterNode);
                    cmdParameter = new TestMemberPathObserver();
                    _compiler.AddComponent(new TestExpressionCompilerComponent
                    {
                        TryCompile = (node, m) =>
                        {
                            node.ShouldEqual(cmdParameterNode);
                            m.ShouldEqual(_context.GetMetadataOrDefault());
                            return exp;
                        }
                    });
                }
            }
            else
                throw new NotSupportedException();

            _component.Flags.ShouldNotEqual(flags);
            _component.SuppressIndexAccessors.ShouldNotEqual(suppressIndexAccessors);
            _component.SuppressMethodAccessors.ShouldNotEqual(suppressMethodAccessors);
            _component.ToggleEnabledState.ShouldNotEqual(toggleEnabledState);
            _component.MemberFlags.ShouldNotEqual(memberFlags);
            _component.MemberFlags = memberFlags;

            IExpressionNode[] parameters;
            if (parametersSetting)
            {
                parameters = new IExpressionNode[]
                {
                    new MemberExpressionNode(null, BindingParameterNameConstant.SuppressMethodAccessors),
                    new MemberExpressionNode(null, BindingParameterNameConstant.SuppressIndexAccessors),
                    new MemberExpressionNode(null, BindingParameterNameConstant.HasStablePath),
                    new MemberExpressionNode(null, BindingParameterNameConstant.ObservableMethods),
                    new MemberExpressionNode(null, BindingParameterNameConstant.Observable),
                    new MemberExpressionNode(null, BindingParameterNameConstant.Optional),
                    new MemberExpressionNode(null, BindingParameterNameConstant.ToggleEnabled),
                    new MemberExpressionNode(null, _component.OneTimeBindingMode),
                    new BinaryExpressionNode(BinaryTokenType.Assignment, new MemberExpressionNode(null, BindingParameterNameConstant.CommandParameter), cmdParameterNode)
                };
            }
            else
            {
                binding.Metadata = BindingMetadata.IsMultiBinding.ToContext(false);
                parameters = new[]
                    {new BinaryExpressionNode(BinaryTokenType.Assignment, new MemberExpressionNode(null, BindingParameterNameConstant.CommandParameter), cmdParameterNode)};
                _component.Flags = flags;
                _component.SuppressIndexAccessors = suppressIndexAccessors;
                _component.SuppressMethodAccessors = suppressMethodAccessors;
                _component.ToggleEnabledState = toggleEnabledState;
            }

            var targetVisitCount = 0;
            var sourceVisitCount = 0;
            var target = new TestBindingMemberExpressionNode
            {
                MemberFlags = memberFlags,
                GetSource = (t, s, m) =>
                {
                    t.ShouldEqual(targetSrc);
                    s.ShouldEqual(sourceSrc);
                    m.ShouldEqual(_context.GetMetadataOrDefault());
                    return (targetSrc, targetPath);
                },
                VisitHandler = (visitor, metadataContext) =>
                {
                    ++targetVisitCount;
                    metadataContext.ShouldEqual(_context.GetMetadataOrDefault());
                    var expressionVisitor = (BindingMemberExpressionVisitor) visitor;
                    expressionVisitor.Flags.ShouldEqual(flags.SetTargetFlags(true));
                    expressionVisitor.SuppressIndexAccessors.ShouldEqual(suppressIndexAccessors);
                    expressionVisitor.SuppressMethodAccessors.ShouldEqual(suppressMethodAccessors);
                    expressionVisitor.MemberFlags.ShouldEqual(memberFlags);
                    return null;
                }
            };
            var source = new TestBindingMemberExpressionNode
            {
                VisitHandler = (visitor, metadataContext) =>
                {
                    ++sourceVisitCount;
                    metadataContext.ShouldEqual(_context.GetMetadataOrDefault());
                    var expressionVisitor = (BindingMemberExpressionVisitor) visitor;
                    expressionVisitor.Flags.ShouldEqual(flags.SetTargetFlags(false) & ~(BindingMemberExpressionFlags.Observable | BindingMemberExpressionFlags.ObservableMethods));
                    expressionVisitor.SuppressIndexAccessors.ShouldBeTrue();
                    expressionVisitor.SuppressMethodAccessors.ShouldBeTrue();
                    expressionVisitor.MemberFlags.ShouldEqual(memberFlags);
                    return null;
                }
            };

            _context.Initialize(targetSrc, sourceSrc, target, source, parameters, DefaultMetadata);
            _component.Initialize(_bindingManager, _context);
            targetVisitCount.ShouldEqual(1);
            sourceVisitCount.ShouldEqual(1);
            _context.Components[BindingParameterNameConstant.Mode].ShouldBeNull();
            var bindingComponentProvider = (IBindingComponentProvider) _context.Components[BindingParameterNameConstant.EventHandler]!;
            var bindingComponent = (BindingEventHandler) bindingComponentProvider.TryGetComponent(binding, targetSrc, sourceSrc, DefaultMetadata)!;
            if (parametersSetting)
                bindingComponent.ShouldBeType<BindingEventHandler>();
            else
                bindingComponent.ShouldBeType<BindingEventHandler.OneWay>();
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
    }
}