using System;
using System.Collections.Generic;
using MugenMvvm.Bindings.Constants;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Interfaces.Parsing;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Bindings.Parsing.Expressions
{
    public sealed class MemberExpressionNode : ExpressionNodeBase<IMemberExpressionNode>, IMemberExpressionNode
    {
        #region Fields

        public static readonly MemberExpressionNode Action = new(null, MacrosConstant.Action);
        public static readonly MemberExpressionNode EventArgs = new(null, MacrosConstant.EventArgs);
        public static readonly MemberExpressionNode Source = new(null, MacrosConstant.Source);
        public static readonly MemberExpressionNode Self = new(null, MacrosConstant.Target);
        public static readonly MemberExpressionNode Context = new(null, MacrosConstant.Context);
        public static readonly MemberExpressionNode Binding = new(null, MacrosConstant.Binding);
        public static readonly MemberExpressionNode Empty = new(null, string.Empty);

        public static readonly MemberExpressionNode NoneMode = new(null, BindingModeNameConstant.None);
        public static readonly MemberExpressionNode OneTimeMode = new(null, BindingModeNameConstant.OneTime);
        public static readonly MemberExpressionNode OneWayMode = new(null, BindingModeNameConstant.OneWay);
        public static readonly MemberExpressionNode OneWayToSourceMode = new(null, BindingModeNameConstant.OneWayToSource);
        public static readonly MemberExpressionNode TwoWayMode = new(null, BindingModeNameConstant.TwoWay);

        public static readonly MemberExpressionNode OptionalParameter = new(null, BindingParameterNameConstant.Optional);
        public static readonly MemberExpressionNode HasStablePathParameter = new(null, BindingParameterNameConstant.HasStablePath);
        public static readonly MemberExpressionNode ObservableParameter = new(null, BindingParameterNameConstant.Observable);
        public static readonly MemberExpressionNode ToggleEnabledParameter = new(null, BindingParameterNameConstant.ToggleEnabled);
        public static readonly MemberExpressionNode SuppressMethodAccessorsParameter = new(null, BindingParameterNameConstant.SuppressMethodAccessors);
        public static readonly MemberExpressionNode SuppressIndexAccessorsParameter = new(null, BindingParameterNameConstant.SuppressIndexAccessors);
        public static readonly MemberExpressionNode ObservableMethodsParameter = new(null, BindingParameterNameConstant.ObservableMethods);
        public static readonly MemberExpressionNode ConverterParameter = new(null, BindingParameterNameConstant.Converter);
        public static readonly MemberExpressionNode ConverterParameterParameter = new(null, BindingParameterNameConstant.ConverterParameter);
        public static readonly MemberExpressionNode FallbackParameter = new(null, BindingParameterNameConstant.Fallback);
        public static readonly MemberExpressionNode TargetNullValueParameter = new(null, BindingParameterNameConstant.TargetNullValue);
        public static readonly MemberExpressionNode CommandParameterParameter = new(null, BindingParameterNameConstant.CommandParameter);
        public static readonly MemberExpressionNode DelayParameter = new(null, BindingParameterNameConstant.Delay);
        public static readonly MemberExpressionNode TargetDelayParameter = new(null, BindingParameterNameConstant.TargetDelay);

        #endregion

        #region Constructors

        public MemberExpressionNode(IExpressionNode? target, string member, IReadOnlyDictionary<string, object?>? metadata = null) : base(metadata)
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

        public IMemberExpressionNode UpdateTarget(IExpressionNode? target) => Equals(target, Target) ? this : new MemberExpressionNode(target, Member, Metadata);

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
                return new MemberExpressionNode(node, Member, Metadata);
            return this;
        }

        protected override IMemberExpressionNode Clone(IReadOnlyDictionary<string, object?> metadata) => new MemberExpressionNode(Target, Member, metadata);

        protected override bool Equals(IMemberExpressionNode other, IExpressionEqualityComparer? comparer) => Member.Equals(other.Member) && Equals(Target, other.Target, comparer);

        protected override int GetHashCode(int hashCode, IExpressionEqualityComparer? comparer)
        {
            if (Target == null)
                return HashCode.Combine(hashCode, Member);
            return HashCode.Combine(hashCode, Member, Target.GetHashCode(comparer));
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