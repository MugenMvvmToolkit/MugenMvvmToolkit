using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using MugenMvvm.Bindings.Constants;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Interfaces.Core;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Bindings.Parsing;
using MugenMvvm.Bindings.Parsing.Expressions;
using MugenMvvm.Collections;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Internal;
#if AVALONIA
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Markup.Xaml;

namespace MugenMvvm.Avalonia.Bindings.Markup
#else
using System.Windows.Data;
using System.Windows.Markup;
namespace MugenMvvm.Windows.Bindings.Markup
#endif
{
    public partial class MugenBindingExtension : IEqualityComparer<MugenBindingExtension>
    {
        private static readonly Func<object?> NoDoFunc = () => null;
        private static readonly Dictionary<Type, Delegate> CachedDelegates = new(InternalEqualityComparer.Type);

        private string? _targetPath;
        private object? _defaultValue;
        private ItemOrIReadOnlyList<IBindingBuilder> _bindingBuilders;

        public MugenBindingExtension() : this(null)
        {
        }

        public MugenBindingExtension(string? path)
        {
            Path = path;
            Mode = BindingMode.Default;
        }

        [ConstructorArgument("path")]
        public string? Path { get; set; }

        public BindingMode Mode { get; set; }

        public bool? Observable { get; set; }

        public bool? Optional { get; set; }

        public bool? HasStablePath { get; set; }

        public bool? ToggleEnabled { get; set; }

        public bool? SuppressMethodAccessors { get; set; }

        public bool? SuppressIndexAccessors { get; set; }

        public bool? ObservableMethods { get; set; }

        public int Delay { get; set; }

        public int TargetDelay { get; set; }

        public object? CommandParameter { get; set; }

        public object? Converter { get; set; }

        public object? ConverterParameter { get; set; }

        public object? Fallback { get; set; }

        public object? TargetNullValue { get; set; }

        public string? Trace { get; set; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void BoolParameter(ref ItemOrListEditor<KeyValuePair<string?, object>> editor, IExpressionNode parameter, bool? value)
        {
            if (!value.HasValue)
                return;

            if (!value.Value)
                parameter = new UnaryExpressionNode(UnaryTokenType.LogicalNegation, parameter);

            editor.Add(new KeyValuePair<string?, object>(null, parameter));
        }

        private static Delegate CreateDelegateForEvent(Type eventHandlerType)
        {
            if (!CachedDelegates.TryGetValue(eventHandlerType, out Delegate value))
            {
                var parameters = eventHandlerType.GetMethod(nameof(Action.Invoke), BindingFlagsEx.InstancePublic)!
                                 .GetParameters()
                                 .ToArray(parameter => Expression.Parameter(parameter.ParameterType));

                var callExpression = Expression.Call(Expression.Constant(NoDoFunc, typeof(Func<object>)), nameof(Action.Invoke),
                    Array.Empty<Type>());
                value = Expression
                        .Lambda(eventHandlerType, callExpression, parameters)
                        .Compile();
                CachedDelegates[eventHandlerType] = value;
            }

            return value;
        }

        public BindingExpressionRequest ToRequest()
        {
            var editor = new ItemOrListEditor<KeyValuePair<string?, object>>();
            switch (Mode)
            {
                case BindingMode.TwoWay:
                    editor.Add(new KeyValuePair<string?, object>(null, MemberExpressionNode.TwoWayMode));
                    break;
                case BindingMode.OneWay:
                    editor.Add(new KeyValuePair<string?, object>(null, MemberExpressionNode.OneWayMode));
                    break;
                case BindingMode.OneTime:
                    editor.Add(new KeyValuePair<string?, object>(null, MemberExpressionNode.OneTimeMode));
                    break;
                case BindingMode.OneWayToSource:
                    editor.Add(new KeyValuePair<string?, object>(null, MemberExpressionNode.OneWayToSourceMode));
                    break;
            }

            BoolParameter(ref editor, MemberExpressionNode.ObservableParameter, Observable);
            BoolParameter(ref editor, MemberExpressionNode.OptionalParameter, Optional);
            BoolParameter(ref editor, MemberExpressionNode.HasStablePathParameter, HasStablePath);
            BoolParameter(ref editor, MemberExpressionNode.ToggleEnabledParameter, ToggleEnabled);
            BoolParameter(ref editor, MemberExpressionNode.SuppressMethodAccessorsParameter, SuppressMethodAccessors);
            BoolParameter(ref editor, MemberExpressionNode.SuppressIndexAccessorsParameter, SuppressIndexAccessors);
            BoolParameter(ref editor, MemberExpressionNode.ObservableMethodsParameter, ObservableMethods);

            if (Delay != 0)
                editor.Add(new KeyValuePair<string?, object>(BindingParameterNameConstant.Delay, BoxingExtensions.Box(Delay)));
            if (TargetDelay != 0)
                editor.Add(new KeyValuePair<string?, object>(BindingParameterNameConstant.TargetDelay, BoxingExtensions.Box(TargetDelay)));
            if (CommandParameter != null)
                editor.Add(new KeyValuePair<string?, object>(BindingParameterNameConstant.CommandParameter, ConstantExpressionNode.Get(CommandParameter)));
            var converter = Converter;
            if (converter != null)
            {
                if (converter is IValueConverter valueConverter)
                    converter = new BindingValueConverterWrapper(valueConverter);
                editor.Add(new KeyValuePair<string?, object>(BindingParameterNameConstant.Converter, ConstantExpressionNode.Get(converter)));
            }

            if (ConverterParameter != null)
                editor.Add(new KeyValuePair<string?, object>(BindingParameterNameConstant.ConverterParameter, ConstantExpressionNode.Get(ConverterParameter)));
            if (TargetNullValue != null)
                editor.Add(new KeyValuePair<string?, object>(BindingParameterNameConstant.TargetNullValue, ConstantExpressionNode.Get(TargetNullValue)));
            if (Fallback != null)
                editor.Add(new KeyValuePair<string?, object>(BindingParameterNameConstant.Fallback, ConstantExpressionNode.Get(Fallback)));
            if (Trace != null)
                editor.Add(new KeyValuePair<string?, object>(BindingParameterNameConstant.Trace, ConstantExpressionNode.Get(Trace)));
            return new BindingExpressionRequest(_targetPath!, Path, editor);
        }

        bool IEqualityComparer<MugenBindingExtension>.Equals(MugenBindingExtension? x, MugenBindingExtension? y)
        {
            if (ReferenceEquals(x, y))
                return true;
            if (ReferenceEquals(x, null))
                return false;
            if (ReferenceEquals(y, null))
                return false;
            return x.Path == y.Path && x.Mode == y.Mode && x.Observable == y.Observable && x.Optional == y.Optional && x.HasStablePath == y.HasStablePath &&
                   x.ToggleEnabled == y.ToggleEnabled && x.SuppressMethodAccessors == y.SuppressMethodAccessors && x.SuppressIndexAccessors == y.SuppressIndexAccessors &&
                   x.ObservableMethods == y.ObservableMethods && x.Delay == y.Delay && x.TargetDelay == y.TargetDelay && Equals(x.CommandParameter, y.CommandParameter) &&
                   Equals(x.Converter, y.Converter) && Equals(x.ConverterParameter, y.ConverterParameter) && Equals(x.Fallback, y.Fallback) &&
                   Equals(x.TargetNullValue, y.TargetNullValue) && x.Trace == y.Trace;
        }

        int IEqualityComparer<MugenBindingExtension>.GetHashCode(MugenBindingExtension obj)
        {
            var hashCode = new HashCode();
            hashCode.Add(obj.Path);
            hashCode.Add((int) obj.Mode);
            hashCode.Add(obj.Observable);
            hashCode.Add(obj.Optional);
            hashCode.Add(obj.HasStablePath);
            hashCode.Add(obj.ToggleEnabled);
            hashCode.Add(obj.SuppressMethodAccessors);
            hashCode.Add(obj.SuppressIndexAccessors);
            hashCode.Add(obj.ObservableMethods);
            hashCode.Add(obj.Delay);
            hashCode.Add(obj.TargetDelay);
            hashCode.Add(obj.CommandParameter);
            hashCode.Add(obj.Converter);
            hashCode.Add(obj.ConverterParameter);
            hashCode.Add(obj.Fallback);
            hashCode.Add(obj.TargetNullValue);
            return hashCode.ToHashCode();
        }
    }
}