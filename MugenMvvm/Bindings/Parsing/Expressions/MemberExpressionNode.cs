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

        public static readonly MemberExpressionNode Action = new MemberExpressionNode(null, MacrosConstant.Action, Default.ReadOnlyDictionary<string, object?>());
        public static readonly MemberExpressionNode EventArgs = new MemberExpressionNode(null, MacrosConstant.EventArgs, Default.ReadOnlyDictionary<string, object?>());
        public static readonly MemberExpressionNode Source = new MemberExpressionNode(null, MacrosConstant.Source, Default.ReadOnlyDictionary<string, object?>());
        public static readonly MemberExpressionNode Self = new MemberExpressionNode(null, MacrosConstant.Target, Default.ReadOnlyDictionary<string, object?>());
        public static readonly MemberExpressionNode Context = new MemberExpressionNode(null, MacrosConstant.Context, Default.ReadOnlyDictionary<string, object?>());
        public static readonly MemberExpressionNode Binding = new MemberExpressionNode(null, MacrosConstant.Binding, Default.ReadOnlyDictionary<string, object?>());
        public static readonly MemberExpressionNode Empty = new MemberExpressionNode(null, string.Empty, Default.ReadOnlyDictionary<string, object?>());

        public static readonly MemberExpressionNode NoneMode = new MemberExpressionNode(null, BindingModeNameConstant.None, Default.ReadOnlyDictionary<string, object?>());
        public static readonly MemberExpressionNode OneTimeMode = new MemberExpressionNode(null, BindingModeNameConstant.OneTime, Default.ReadOnlyDictionary<string, object?>());
        public static readonly MemberExpressionNode OneWayMode = new MemberExpressionNode(null, BindingModeNameConstant.OneWay, Default.ReadOnlyDictionary<string, object?>());
        public static readonly MemberExpressionNode OneWayToSourceMode = new MemberExpressionNode(null, BindingModeNameConstant.OneWayToSource, Default.ReadOnlyDictionary<string, object?>());
        public static readonly MemberExpressionNode TwoWayMode = new MemberExpressionNode(null, BindingModeNameConstant.TwoWay, Default.ReadOnlyDictionary<string, object?>());

        public static readonly MemberExpressionNode OptionalParameter = new MemberExpressionNode(null, BindingParameterNameConstant.Optional, Default.ReadOnlyDictionary<string, object?>());
        public static readonly MemberExpressionNode HasStablePathParameter = new MemberExpressionNode(null, BindingParameterNameConstant.HasStablePath, Default.ReadOnlyDictionary<string, object?>());
        public static readonly MemberExpressionNode ObservableParameter = new MemberExpressionNode(null, BindingParameterNameConstant.Observable, Default.ReadOnlyDictionary<string, object?>());
        public static readonly MemberExpressionNode ToggleEnabledParameter = new MemberExpressionNode(null, BindingParameterNameConstant.ToggleEnabled, Default.ReadOnlyDictionary<string, object?>());
        public static readonly MemberExpressionNode IgnoreMethodMembersParameter = new MemberExpressionNode(null, BindingParameterNameConstant.IgnoreMethodMembers, Default.ReadOnlyDictionary<string, object?>());
        public static readonly MemberExpressionNode IgnoreIndexMembersParameter = new MemberExpressionNode(null, BindingParameterNameConstant.IgnoreIndexMembers, Default.ReadOnlyDictionary<string, object?>());
        public static readonly MemberExpressionNode ObservableMethodsParameter = new MemberExpressionNode(null, BindingParameterNameConstant.ObservableMethods, Default.ReadOnlyDictionary<string, object?>());
        public static readonly MemberExpressionNode ConverterParameter = new MemberExpressionNode(null, BindingParameterNameConstant.Converter, Default.ReadOnlyDictionary<string, object?>());
        public static readonly MemberExpressionNode ConverterParameterParameter = new MemberExpressionNode(null, BindingParameterNameConstant.ConverterParameter, Default.ReadOnlyDictionary<string, object?>());
        public static readonly MemberExpressionNode FallbackParameter = new MemberExpressionNode(null, BindingParameterNameConstant.Fallback, Default.ReadOnlyDictionary<string, object?>());
        public static readonly MemberExpressionNode TargetNullValueParameter = new MemberExpressionNode(null, BindingParameterNameConstant.TargetNullValue, Default.ReadOnlyDictionary<string, object?>());
        public static readonly MemberExpressionNode CommandParameterParameter = new MemberExpressionNode(null, BindingParameterNameConstant.CommandParameter, Default.ReadOnlyDictionary<string, object?>());
        public static readonly MemberExpressionNode DelayParameter = new MemberExpressionNode(null, BindingParameterNameConstant.Delay, Default.ReadOnlyDictionary<string, object?>());
        public static readonly MemberExpressionNode TargetDelayParameter = new MemberExpressionNode(null, BindingParameterNameConstant.TargetDelay, Default.ReadOnlyDictionary<string, object?>());

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

            return new MemberExpressionNode(target, member, null);
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