using System;
using System.Collections.Generic;
using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Compiling;
using MugenMvvm.Binding.Interfaces.Core;
using MugenMvvm.Binding.Interfaces.Core.Components;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Binding.Members.Components;
using MugenMvvm.Binding.Metadata;
using MugenMvvm.Binding.Parsing.Expressions;
using MugenMvvm.Binding.Parsing.Visitors;
using MugenMvvm.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Internal;

namespace MugenMvvm.Binding.Core.Components
{
    public sealed class BindingConfiguratorComponent : AttachableComponentBase<IBindingManager>, IBindingExpressionInterceptorComponent, IHasPriority, IBindingComponentProviderComponent
    {
        #region Fields

        private readonly IExpressionCompiler? _compiler;

        private readonly Func<ValueTuple<ValueTuple<object?, ICompiledExpression?>, bool>, IBinding, object, object?, IReadOnlyMetadataContext?, IComponent<IBinding>>
            _getEventHandlerDelegate;

        private static readonly Func<ValueTuple<ValueTuple<object?, ICompiledExpression?>, ValueTuple<object?, ICompiledExpression?>, ValueTuple<object?, ICompiledExpression?>,
            ValueTuple<object?, ICompiledExpression?>>, IBinding, object, object?, IReadOnlyMetadataContext?, IComponent<IBinding>> GetParametersComponentDelegate =
            GetParametersComponent;

        private static readonly HashSet<string> BoolParameters = new HashSet<string>
        {
            BindingParameterNameConstants.Optional,
            BindingParameterNameConstants.HasStablePath,
            BindingParameterNameConstants.Observable,
            BindingParameterNameConstants.ToggleEnabled,
            BindingParameterNameConstants.IgnoreMethodMembers,
            BindingParameterNameConstants.IgnoreIndexMembers
        };

        private static readonly BindingMemberExpressionVisitor MemberExpressionVisitor = new BindingMemberExpressionVisitor();
        private static readonly BindingMemberExpressionCollectorVisitor MemberExpressionCollectorVisitor = new BindingMemberExpressionCollectorVisitor();

        #endregion

        #region Constructors

        public BindingConfiguratorComponent(IExpressionCompiler? compiler = null)
        {
            _getEventHandlerDelegate = GetEventHandlerComponent;
            _compiler = compiler;
            Builders = new Dictionary<string, IBindingComponentBuilder>();
        }

        #endregion

        #region Properties

        public Dictionary<string, IBindingComponentBuilder> Builders { get; }

        public BindingMemberExpressionFlags Flags { get; set; } = BindingMemberExpressionFlags.Observable;

        public bool ToggleEnabledState { get; set; }

        public int Priority { get; set; } = int.MinValue;

        #endregion

        #region Implementation of interfaces

        public ItemOrList<IBindingComponentBuilder?, IReadOnlyList<IBindingComponentBuilder>> TryGetComponentBuilders(IBinding binding, object target, object? source,
            ItemOrList<IExpressionNode?, IReadOnlyList<IExpressionNode>> parameters, IReadOnlyMetadataContext? metadata)
        {
            var isEvent = binding.Target.GetLastMember().Member.MemberType == MemberType.Event;
            var isMulti = isEvent && (binding is MultiBinding || binding.Metadata.Get(BindingMetadata.IsMultiBinding));
            var hasEventHandler = false;
            ItemOrList<IBindingComponentBuilder?, List<IBindingComponentBuilder>> result = default;
            for (var i = 0; i < parameters.Count(); i++)
            {
                var node = parameters.GetItemAt(i);
                if (node is IBindingComponentBuilder builder)
                {
                    if (isMulti && builder.Name == BindingParameterNameConstants.Mode)
                        continue;
                    if (!isEvent && builder.Name == BindingParameterNameConstants.EventHandler)
                        continue;

                    if (isEvent && !hasEventHandler)
                        hasEventHandler = builder.Name == BindingParameterNameConstants.EventHandler;
                    result.Add(builder);
                    continue;
                }

                if (!isMulti && node is IBinaryExpressionNode binary
                             && binary.Token == BinaryTokenType.Equality
                             && binary.Left is IMemberExpressionNode memberExpression
                             && memberExpression.MemberName == BindingParameterNameConstants.Mode)
                {
                    node = binary.Right;
                }

                if (!(node is IMemberExpressionNode member) || !Builders.TryGetValue(member.MemberName, out var value))
                    continue;

                if (!isMulti || value.Name != BindingParameterNameConstants.Mode)
                    result.Add(value);
            }

            if (isEvent)
            {
                if (!hasEventHandler)
                {
                    result.Add(new DelegateBindingComponentBuilder<ValueTuple<ValueTuple<object?, ICompiledExpression?>, bool>>(_getEventHandlerDelegate,
                    BindingParameterNameConstants.EventHandler, ValueTuple.Create<(object?, ICompiledExpression?), bool>(default, ToggleEnabledState)));
                }
                if (isMulti)
                    result.Add(InstanceBindingComponentBuilder.NoneMode);
            }

            return result.Cast<IReadOnlyList<IBindingComponentBuilder>>();
        }

        public void Intercept(ref IExpressionNode targetExpression, ref IExpressionNode sourceExpression,
            ref ItemOrList<IExpressionNode?, List<IExpressionNode>> parameters, IReadOnlyMetadataContext? metadata)
        {
            //source is empty, target is expression
            if (sourceExpression is IMemberExpressionNode member && string.IsNullOrEmpty(member.MemberName)
                                                                 && !(targetExpression is IMemberExpressionNode)
                                                                 && !(targetExpression is IBindingMemberExpressionNode))
            {
                sourceExpression = targetExpression;
                targetExpression = new MemberExpressionNode(null, FakeMemberProviderComponent.FakeMemberPrefixSymbol + Default.NextCounter().ToString());
            }

            bool? toggleEnabledState = null;
            int? delay = null, targetDelay = null;
            IExpressionNode? converter = null, converterParameter = null, fallback = null, targetNullValue = null, commandParameter = null;
            var flags = Flags;
            bool suppressMethodMembers = false;
            bool suppressIndexMembers = false;
            for (var i = 0; i < parameters.Count(); i++)
            {
                var node = parameters.GetItemAt(i);
                var b = TryGetBoolValue(BoolParameters, node, out var name);
                if (b == null)
                {
                    if (node is IBinaryExpressionNode binary && binary.Token == BinaryTokenType.Equality
                                                             && binary.Left is IMemberExpressionNode memberExpression)
                    {
                        switch (memberExpression.MemberName)
                        {
                            case BindingParameterNameConstants.Converter:
                                converter = binary.Right;
                                break;
                            case BindingParameterNameConstants.ConverterParameter:
                                converterParameter = binary.Right;
                                break;
                            case BindingParameterNameConstants.Fallback:
                                fallback = binary.Right;
                                break;
                            case BindingParameterNameConstants.TargetNullValue:
                                targetNullValue = binary.Right;
                                break;
                            case BindingParameterNameConstants.CommandParameter:
                                commandParameter = binary.Right;
                                break;
                            case BindingParameterNameConstants.Delay:
                                delay = (int)((IConstantExpressionNode)binary.Right).Value!;
                                break;
                            case BindingParameterNameConstants.TargetDelay:
                                targetDelay = (int)((IConstantExpressionNode)binary.Right).Value!;
                                break;
                            default:
                                continue;
                        }
                    }
                    else
                        continue;
                }
                else
                {
                    if (name == BindingParameterNameConstants.ToggleEnabled)
                        toggleEnabledState = b.Value;
                    else if (name == BindingParameterNameConstants.IgnoreMethodMembers)
                        suppressMethodMembers = b.Value;
                    else if (name == BindingParameterNameConstants.IgnoreIndexMembers)
                        suppressIndexMembers = b.Value;
                    else
                        flags = ApplyFlags(flags, name!, b.Value);
                }

                parameters.RemoveAt(i);
                --i;
            }

            MemberExpressionVisitor.Flags = flags;
            MemberExpressionVisitor.IgnoreIndexMembers = suppressIndexMembers;
            MemberExpressionVisitor.IgnoreMethodMembers = suppressMethodMembers;
            targetExpression = MemberExpressionVisitor.Accept(targetExpression)!;
            sourceExpression = MemberExpressionVisitor.Accept(sourceExpression)!;

            converter = MemberExpressionVisitor.Accept(converter);
            converterParameter = MemberExpressionVisitor.Accept(converterParameter);
            fallback = MemberExpressionVisitor.Accept(fallback);
            targetNullValue = MemberExpressionVisitor.Accept(targetNullValue);
            commandParameter = MemberExpressionVisitor.Accept(commandParameter);

            if (converter != null || converterParameter != null || fallback != null || targetNullValue != null)
            {
                var tuple = ValueTuple.Create(GetValueOrExpression(converter, metadata), GetValueOrExpression(converterParameter, metadata),
                    GetValueOrExpression(fallback, metadata),
                    GetValueOrExpression(targetNullValue, metadata));
                parameters.Add(new DelegateBindingComponentBuilder<ValueTuple<ValueTuple<object?, ICompiledExpression?>, ValueTuple<object?, ICompiledExpression?>,
                    ValueTuple<object?, ICompiledExpression?>, ValueTuple<object?, ICompiledExpression?>>>(GetParametersComponentDelegate,
                    BindingParameterNameConstants.Parameters, tuple));
            }

            if (commandParameter != null || toggleEnabledState != null)
            {
                var valueTuple = ValueTuple.Create(GetValueOrExpression(commandParameter, metadata), toggleEnabledState.GetValueOrDefault(ToggleEnabledState));
                parameters.Add(new DelegateBindingComponentBuilder<ValueTuple<ValueTuple<object?, ICompiledExpression?>, bool>>(_getEventHandlerDelegate,
                    BindingParameterNameConstants.EventHandler, valueTuple));
            }

            if (delay != null)
                parameters.Add(new DelegateBindingComponentBuilder<int>((i, _, __, ___, ____) => DelayBindingComponent.GetSource(i), BindingParameterNameConstants.Delay, delay.Value));

            if (targetDelay != null)
                parameters.Add(new DelegateBindingComponentBuilder<int>((i, _, __, ___, ____) => DelayBindingComponent.GetTarget(i), BindingParameterNameConstants.Delay, targetDelay.Value));
        }

        #endregion

        #region Methods

        private IComponent<IBinding> GetEventHandlerComponent(ValueTuple<ValueTuple<object?, ICompiledExpression?>, bool> state,
            IBinding binding, object target, object? source, IReadOnlyMetadataContext? metadata)
        {
            var (commandParameter, toggleEnabledState) = state;
            return new EventTargetValueInterceptorBindingComponent(GetValueOrExpression(commandParameter, target, source, metadata), toggleEnabledState, Owner);
        }

        private static IComponent<IBinding> GetParametersComponent(ValueTuple<ValueTuple<object?, ICompiledExpression?>, ValueTuple<object?, ICompiledExpression?>, ValueTuple<object?, ICompiledExpression?>,
                ValueTuple<object?, ICompiledExpression?>> state, IBinding binding, object target, object? source, IReadOnlyMetadataContext? metadata)
        {
            var (converter, converterParameter, fallback, targetNullValue) = state;
            return new BindingParametersValueInterceptorComponent(GetValueOrExpression(converter, target, source, metadata),
                GetValueOrExpression(converterParameter, target, source, metadata), GetValueOrExpression(fallback, target, source, metadata),
                GetValueOrExpression(targetNullValue, target, source, metadata));
        }

        private static BindingParameterValue GetValueOrExpression(ValueTuple<object?, ICompiledExpression?> value, object target, object? source,
            IReadOnlyMetadataContext? metadata)
        {
            if (value.Item1 is IBindingMemberExpressionNode v)
            {
                var observer = v.GetSourceObserver(target, source, metadata);
                return new BindingParameterValue(observer, value.Item2);
            }

            if (value.Item1 is IBindingMemberExpressionNode[] nodes)
            {
                var observers = new IMemberPathObserver[nodes.Length];
                for (var i = 0; i < nodes.Length; i++)
                    observers[i] = nodes[i].GetSourceObserver(target, source, metadata);
                return new BindingParameterValue(observers, value.Item2);
            }

            return new BindingParameterValue(value.Item1, value.Item2);
        }

        private ValueTuple<object?, ICompiledExpression?> GetValueOrExpression(IExpressionNode? node, IReadOnlyMetadataContext? metadata)
        {
            if (node == null)
                return default;
            if (node is IConstantExpressionNode constant)
                return ValueTuple.Create<object?, ICompiledExpression?>(constant.Value, null);
            if (node is IBindingMemberExpressionNode)
                return ValueTuple.Create<object?, ICompiledExpression?>(node, null);

            var collect = MemberExpressionCollectorVisitor.Collect(node);
            var compiledExpression = _compiler.DefaultIfNull().Compile(node, metadata);
            if (collect.Item == null && collect.List == null)
                return ValueTuple.Create<object?, ICompiledExpression?>(compiledExpression.Invoke(default, metadata), null);

            return ValueTuple.Create(collect.GetRawValue(), compiledExpression);
        }

        private static BindingMemberExpressionFlags ApplyFlags(BindingMemberExpressionFlags flags, string parameterName, bool hasFlag)
        {
            return parameterName switch
            {
                BindingParameterNameConstants.Observable => ApplyFlags(flags, hasFlag, BindingMemberExpressionFlags.Observable),
                BindingParameterNameConstants.Optional => ApplyFlags(flags, hasFlag, BindingMemberExpressionFlags.Optional),
                BindingParameterNameConstants.HasStablePath => ApplyFlags(flags, hasFlag, BindingMemberExpressionFlags.StablePath),
                _ => flags
            };
        }

        private static BindingMemberExpressionFlags ApplyFlags(BindingMemberExpressionFlags flags, bool hasFlag, BindingMemberExpressionFlags value)
        {
            return hasFlag ? flags | value : flags & ~value;
        }

        private static bool? TryGetBoolValue(HashSet<string> parameterNames, IExpressionNode node, out string? parameterName)
        {
            //Optional, HasStablePath etc
            if (node is IMemberExpressionNode memberExpression)
            {
                if (parameterNames.Contains(memberExpression.MemberName))
                {
                    parameterName = memberExpression.MemberName;
                    return true;
                }

                parameterName = null;
                return null;
            }

            //!Optional, !HasStablePath etc
            if (node is IUnaryExpressionNode unary && unary.Token == UnaryTokenType.LogicalNegation)
            {
                if (unary.Operand is IMemberExpressionNode member && parameterNames.Contains(member.MemberName))
                {
                    parameterName = member.MemberName;
                    return false;
                }

                parameterName = null;
                return null;
            }

            //Optional, HasStablePath etc
            if (node is IBinaryExpressionNode binary && binary.Token == BinaryTokenType.Equality)
            {
                if (binary.Left is IMemberExpressionNode member && parameterNames.Contains(member.MemberName) && binary.Right is IConstantExpressionNode constant &&
                    constant.Value is bool v)
                {
                    parameterName = member.MemberName;
                    return v;
                }
            }

            parameterName = null;
            return null;
        }

        #endregion
    }
}