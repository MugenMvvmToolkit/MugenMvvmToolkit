using System.Collections.Generic;
using MugenMvvm.Bindings.Constants;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Interfaces.Parsing;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Bindings.Parsing.Expressions
{
    public sealed class MemberExpressionNode : ExpressionNodeBase, IMemberExpressionNode
    {
        #region Fields

        public static readonly MemberExpressionNode Action = new(null, MacrosConstant.Action, Default.ReadOnlyDictionary<string, object?>());
        public static readonly MemberExpressionNode EventArgs = new(null, MacrosConstant.EventArgs, Default.ReadOnlyDictionary<string, object?>());
        public static readonly MemberExpressionNode Source = new(null, MacrosConstant.Source, Default.ReadOnlyDictionary<string, object?>());
        public static readonly MemberExpressionNode Self = new(null, MacrosConstant.Target, Default.ReadOnlyDictionary<string, object?>());
        public static readonly MemberExpressionNode Context = new(null, MacrosConstant.Context, Default.ReadOnlyDictionary<string, object?>());
        public static readonly MemberExpressionNode Binding = new(null, MacrosConstant.Binding, Default.ReadOnlyDictionary<string, object?>());
        public static readonly MemberExpressionNode Empty = new(null, string.Empty, Default.ReadOnlyDictionary<string, object?>());

        public static readonly MemberExpressionNode NoneMode = new(null, BindingModeNameConstant.None, Default.ReadOnlyDictionary<string, object?>());
        public static readonly MemberExpressionNode OneTimeMode = new(null, BindingModeNameConstant.OneTime, Default.ReadOnlyDictionary<string, object?>());
        public static readonly MemberExpressionNode OneWayMode = new(null, BindingModeNameConstant.OneWay, Default.ReadOnlyDictionary<string, object?>());
        public static readonly MemberExpressionNode OneWayToSourceMode = new(null, BindingModeNameConstant.OneWayToSource, Default.ReadOnlyDictionary<string, object?>());
        public static readonly MemberExpressionNode TwoWayMode = new(null, BindingModeNameConstant.TwoWay, Default.ReadOnlyDictionary<string, object?>());

        public static readonly MemberExpressionNode OptionalParameter = new(null, BindingParameterNameConstant.Optional, Default.ReadOnlyDictionary<string, object?>());
        public static readonly MemberExpressionNode HasStablePathParameter = new(null, BindingParameterNameConstant.HasStablePath, Default.ReadOnlyDictionary<string, object?>());
        public static readonly MemberExpressionNode ObservableParameter = new(null, BindingParameterNameConstant.Observable, Default.ReadOnlyDictionary<string, object?>());
        public static readonly MemberExpressionNode ToggleEnabledParameter = new(null, BindingParameterNameConstant.ToggleEnabled, Default.ReadOnlyDictionary<string, object?>());
        public static readonly MemberExpressionNode SuppressMethodAccessorsParameter = new(null, BindingParameterNameConstant.SuppressMethodAccessors, Default.ReadOnlyDictionary<string, object?>());
        public static readonly MemberExpressionNode SuppressIndexAccessorsParameter = new(null, BindingParameterNameConstant.SuppressIndexAccessors, Default.ReadOnlyDictionary<string, object?>());
        public static readonly MemberExpressionNode ObservableMethodsParameter = new(null, BindingParameterNameConstant.ObservableMethods, Default.ReadOnlyDictionary<string, object?>());
        public static readonly MemberExpressionNode ConverterParameter = new(null, BindingParameterNameConstant.Converter, Default.ReadOnlyDictionary<string, object?>());
        public static readonly MemberExpressionNode ConverterParameterParameter = new(null, BindingParameterNameConstant.ConverterParameter, Default.ReadOnlyDictionary<string, object?>());
        public static readonly MemberExpressionNode FallbackParameter = new(null, BindingParameterNameConstant.Fallback, Default.ReadOnlyDictionary<string, object?>());
        public static readonly MemberExpressionNode TargetNullValueParameter = new(null, BindingParameterNameConstant.TargetNullValue, Default.ReadOnlyDictionary<string, object?>());
        public static readonly MemberExpressionNode CommandParameterParameter = new(null, BindingParameterNameConstant.CommandParameter, Default.ReadOnlyDictionary<string, object?>());
        public static readonly MemberExpressionNode DelayParameter = new(null, BindingParameterNameConstant.Delay, Default.ReadOnlyDictionary<string, object?>());
        public static readonly MemberExpressionNode TargetDelayParameter = new(null, BindingParameterNameConstant.TargetDelay, Default.ReadOnlyDictionary<string, object?>());

        #endregion

        #region Constructors

        public MemberExpressionNode(IExpressionNode? target, string member, IDictionary<string, object?>? metadata = null) : base(metadata)
        {
            Should.NotBeNull(member, nameof(member));
            Target = target;
            Member = member;
        }

        #endregion

        #region Properties

        public override ExpressionNodeType ExpressionType => ExpressionNodeType.Member;

        public string Member { get; }

        public IExpressionNode? Target { get; }

        #endregion

        #region Implementation of interfaces

        public IMemberExpressionNode UpdateTarget(IExpressionNode? target) => target == Target ? this : new MemberExpressionNode(target, Member, MetadataRaw);

        #endregion

        #region Methods

        public static MemberExpressionNode Get(IExpressionNode? target, string member)
        {
            if (target == null)
            {
                switch (member)
                {
                    case MacrosConstant.Self:
                    case MacrosConstant.This:
                    case MacrosConstant.Target:
                        return Self;
                    case MacrosConstant.Context:
                        return Context;
                    case MacrosConstant.Source:
                        return Source;
                    case MacrosConstant.EventArgs:
                        return EventArgs;
                    case MacrosConstant.Binding:
                        return Binding;
                    case MacrosConstant.Action:
                        return Action;
                    case BindingModeNameConstant.None:
                        return NoneMode;
                    case BindingModeNameConstant.OneTime:
                        return OneTimeMode;
                    case BindingModeNameConstant.OneWay:
                        return OneWayMode;
                    case BindingModeNameConstant.OneWayToSource:
                        return OneWayToSourceMode;
                    case BindingModeNameConstant.TwoWay:
                        return TwoWayMode;
                    case BindingParameterNameConstant.Optional:
                        return OptionalParameter;
                    case BindingParameterNameConstant.HasStablePath:
                        return HasStablePathParameter;
                    case BindingParameterNameConstant.Observable:
                        return ObservableParameter;
                    case BindingParameterNameConstant.ToggleEnabled:
                        return ToggleEnabledParameter;
                    case BindingParameterNameConstant.SuppressMethodAccessors:
                        return SuppressMethodAccessorsParameter;
                    case BindingParameterNameConstant.SuppressIndexAccessors:
                        return SuppressIndexAccessorsParameter;
                    case BindingParameterNameConstant.ObservableMethods:
                        return ObservableMethodsParameter;
                    case BindingParameterNameConstant.Converter:
                        return ConverterParameter;
                    case BindingParameterNameConstant.ConverterParameter:
                        return ConverterParameterParameter;
                    case BindingParameterNameConstant.Fallback:
                        return FallbackParameter;
                    case BindingParameterNameConstant.TargetNullValue:
                        return TargetNullValueParameter;
                    case BindingParameterNameConstant.CommandParameter:
                        return CommandParameterParameter;
                    case BindingParameterNameConstant.Delay:
                        return DelayParameter;
                    case BindingParameterNameConstant.TargetDelay:
                        return TargetDelayParameter;
                    case "":
                        return Empty;
                }
            }

            return new MemberExpressionNode(target, member);
        }

        protected override IExpressionNode Visit(IExpressionVisitor visitor, IReadOnlyMetadataContext? metadata)
        {
            if (Target == null)
                return this;
            var changed = false;
            var node = VisitWithCheck(visitor, Target, false, ref changed, metadata);
            if (changed)
                return new MemberExpressionNode(node, Member, MetadataRaw);
            return this;
        }

        public override string ToString()
        {
            if (Target == null)
                return Member;
            return $"{Target}.{Member}";
        }

        #endregion
    }
}