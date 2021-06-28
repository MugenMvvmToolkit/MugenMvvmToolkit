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
    public partial class MugenBindingExtension : IEquatable<MugenBindingExtension>
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

        public override bool Equals(object? obj) => ReferenceEquals(this, obj) || obj is MugenBindingExtension other && Equals(other);

        public override int GetHashCode()
        {
            var hashCode = new HashCode();
            hashCode.Add(Path);
            hashCode.Add((int)Mode);
            hashCode.Add(Observable);
            hashCode.Add(Optional);
            hashCode.Add(HasStablePath);
            hashCode.Add(ToggleEnabled);
            hashCode.Add(SuppressMethodAccessors);
            hashCode.Add(SuppressIndexAccessors);
            hashCode.Add(ObservableMethods);
            hashCode.Add(Delay);
            hashCode.Add(TargetDelay);
            hashCode.Add(CommandParameter);
            hashCode.Add(Converter);
            hashCode.Add(ConverterParameter);
            hashCode.Add(Fallback);
            hashCode.Add(TargetNullValue);
            return hashCode.ToHashCode();
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

        public bool Equals(MugenBindingExtension? other)
        {
            if (ReferenceEquals(this, other))
                return true;
            if (ReferenceEquals(other, null))
                return false;
            return Path == other.Path && Mode == other.Mode && Observable == other.Observable && Optional == other.Optional && HasStablePath == other.HasStablePath &&
                   ToggleEnabled == other.ToggleEnabled && SuppressMethodAccessors == other.SuppressMethodAccessors && SuppressIndexAccessors == other.SuppressIndexAccessors &&
                   ObservableMethods == other.ObservableMethods && Delay == other.Delay && TargetDelay == other.TargetDelay && Equals(CommandParameter, other.CommandParameter) &&
                   Equals(Converter, other.Converter) && Equals(ConverterParameter, other.ConverterParameter) && Equals(Fallback, other.Fallback) &&
                   Equals(TargetNullValue, other.TargetNullValue) && Trace == other.Trace;
        }

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
            if (!CachedDelegates.TryGetValue(eventHandlerType, out var value))
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
    }
}