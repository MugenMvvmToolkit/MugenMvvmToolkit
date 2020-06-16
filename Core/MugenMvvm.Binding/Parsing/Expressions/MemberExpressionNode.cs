using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Parsing;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Parsing.Expressions
{
    public sealed class MemberExpressionNode : ExpressionNodeBase, IMemberExpressionNode
    {
        #region Fields

        public static readonly MemberExpressionNode Action = new MemberExpressionNode(null, MacrosConstant.Action);
        public static readonly MemberExpressionNode EventArgs = new MemberExpressionNode(null, MacrosConstant.EventArgs);
        public static readonly MemberExpressionNode Source = new MemberExpressionNode(null, MacrosConstant.Source);
        public static readonly MemberExpressionNode Self = new MemberExpressionNode(null, MacrosConstant.Target);
        public static readonly MemberExpressionNode Context = new MemberExpressionNode(null, MacrosConstant.Context);
        public static readonly MemberExpressionNode Binding = new MemberExpressionNode(null, MacrosConstant.Binding);
        public static readonly MemberExpressionNode Empty = new MemberExpressionNode(null, string.Empty);

        public static readonly MemberExpressionNode NoneMode = new MemberExpressionNode(null, BindingModeNameConstant.None);
        public static readonly MemberExpressionNode OneTimeMode = new MemberExpressionNode(null, BindingModeNameConstant.OneTime);
        public static readonly MemberExpressionNode OneWayMode = new MemberExpressionNode(null, BindingModeNameConstant.OneWay);
        public static readonly MemberExpressionNode OneWayToSourceMode = new MemberExpressionNode(null, BindingModeNameConstant.OneWayToSource);
        public static readonly MemberExpressionNode TwoWayMode = new MemberExpressionNode(null, BindingModeNameConstant.TwoWay);

        public static readonly MemberExpressionNode OptionalParameter = new MemberExpressionNode(null, BindingParameterNameConstant.Optional);
        public static readonly MemberExpressionNode HasStablePathParameter = new MemberExpressionNode(null, BindingParameterNameConstant.HasStablePath);
        public static readonly MemberExpressionNode ObservableParameter = new MemberExpressionNode(null, BindingParameterNameConstant.Observable);
        public static readonly MemberExpressionNode ToggleEnabledParameter = new MemberExpressionNode(null, BindingParameterNameConstant.ToggleEnabled);
        public static readonly MemberExpressionNode IgnoreMethodMembersParameter = new MemberExpressionNode(null, BindingParameterNameConstant.IgnoreMethodMembers);
        public static readonly MemberExpressionNode IgnoreIndexMembersParameter = new MemberExpressionNode(null, BindingParameterNameConstant.IgnoreIndexMembers);
        public static readonly MemberExpressionNode ObservableMethodsParameter = new MemberExpressionNode(null, BindingParameterNameConstant.ObservableMethods);
        public static readonly MemberExpressionNode ConverterParameter = new MemberExpressionNode(null, BindingParameterNameConstant.Converter);
        public static readonly MemberExpressionNode ConverterParameterParameter = new MemberExpressionNode(null, BindingParameterNameConstant.ConverterParameter);
        public static readonly MemberExpressionNode FallbackParameter = new MemberExpressionNode(null, BindingParameterNameConstant.Fallback);
        public static readonly MemberExpressionNode TargetNullValueParameter = new MemberExpressionNode(null, BindingParameterNameConstant.TargetNullValue);
        public static readonly MemberExpressionNode CommandParameterParameter = new MemberExpressionNode(null, BindingParameterNameConstant.CommandParameter);
        public static readonly MemberExpressionNode DelayParameter = new MemberExpressionNode(null, BindingParameterNameConstant.Delay);
        public static readonly MemberExpressionNode TargetDelayParameter = new MemberExpressionNode(null, BindingParameterNameConstant.TargetDelay);

        #endregion

        #region Constructors

        public MemberExpressionNode(IExpressionNode? target, string member)
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

        public IMemberExpressionNode UpdateTarget(IExpressionNode? target)
        {
            if (ReferenceEquals(target, Target))
                return this;
            return new MemberExpressionNode(target, Member);
        }

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
                    case BindingParameterNameConstant.IgnoreMethodMembers:
                        return IgnoreMethodMembersParameter;
                    case BindingParameterNameConstant.IgnoreIndexMembers:
                        return IgnoreIndexMembersParameter;
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

        protected override IExpressionNode VisitInternal(IExpressionVisitor visitor, IReadOnlyMetadataContext? metadata)
        {
            if (Target == null)
                return this;
            var changed = false;
            var node = VisitWithCheck(visitor, Target, false, ref changed, metadata);
            if (changed)
                return new MemberExpressionNode(node, Member);
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