using System;
using MugenMvvm.Attributes;
using MugenMvvm.Bindings.Constants;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Bindings.Interfaces.Compiling;
using MugenMvvm.Bindings.Interfaces.Core;
using MugenMvvm.Bindings.Interfaces.Core.Components;
using MugenMvvm.Bindings.Interfaces.Members;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Bindings.Members;
using MugenMvvm.Bindings.Metadata;
using MugenMvvm.Bindings.Parsing.Visitors;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Bindings.Core.Components
{
    public sealed class BindingExpressionInitializer : IBindingExpressionInitializerComponent, IHasPriority
    {
        private static readonly Func<(BindingParameterExpression, bool, bool), IBinding, object, object?, IReadOnlyMetadataContext?, IComponent<IBinding>?>
            GetEventHandlerDelegate = GetEventHandlerComponent;

        private readonly IExpressionCompiler? _compiler;
        private readonly BindingMemberExpressionCollectorVisitor _memberExpressionCollectorVisitor;
        private readonly BindingMemberExpressionVisitor _memberExpressionVisitor;
        private readonly IMemberManager? _memberManager;

        [Preserve(Conditional = true)]
        public BindingExpressionInitializer(IExpressionCompiler? compiler = null, IMemberManager? memberManager = null)
        {
            _memberExpressionVisitor = new BindingMemberExpressionVisitor
            {
                MemberFlags = Enums.MemberFlags.All & ~Enums.MemberFlags.NonPublic
            };
            _memberExpressionCollectorVisitor = new BindingMemberExpressionCollectorVisitor();
            _compiler = compiler;
            _memberManager = memberManager;
        }

        public string OneTimeBindingMode { get; set; } = BindingModeNameConstant.OneTime;

        public EnumFlags<BindingMemberExpressionFlags> Flags { get; set; } = BindingMemberExpressionFlags.Observable;

        public EnumFlags<MemberFlags> MemberFlags
        {
            get => _memberExpressionVisitor.MemberFlags;
            set => _memberExpressionVisitor.MemberFlags = value;
        }

        public bool SuppressMethodAccessors { get; set; }

        public bool SuppressIndexAccessors { get; set; }

        public bool ToggleEnabledState { get; set; }

        public int Priority { get; init; } = BindingComponentPriority.BindingInitializer;

        public void Initialize(IBindingManager bindingManager, IBindingExpressionInitializerContext context)
        {
            if (context.Components.ContainsKey(BindingParameterNameConstant.EventHandler))
                return;

            var metadata = context.GetMetadataOrDefault();
            var flags = Flags;
            _memberExpressionVisitor.SuppressMethodAccessors =
                context.TryGetParameterValue<bool?>(BindingParameterNameConstant.SuppressMethodAccessors).GetValueOrDefault(SuppressMethodAccessors);
            _memberExpressionVisitor.SuppressIndexAccessors =
                context.TryGetParameterValue<bool?>(BindingParameterNameConstant.SuppressIndexAccessors).GetValueOrDefault(SuppressIndexAccessors);
            context.ApplyFlags(BindingParameterNameConstant.Observable, BindingMemberExpressionFlags.Observable, ref flags);
            context.ApplyFlags(BindingParameterNameConstant.Optional, BindingMemberExpressionFlags.Optional, ref flags);
            context.ApplyFlags(BindingParameterNameConstant.HasStablePath, BindingMemberExpressionFlags.StablePath, ref flags);
            context.ApplyFlags(BindingParameterNameConstant.ObservableMethods, BindingMemberExpressionFlags.ObservableMethods, ref flags);
            _memberExpressionVisitor.Flags = flags;

            context.TargetExpression = _memberExpressionVisitor.Visit(context.TargetExpression, true, metadata);
            if (!IsEvent(context.Target, context.Source, context.TargetExpression, metadata))
            {
                if (context.TargetExpression is IBindingMemberExpressionNode member && member.Path == nameof(BindableMembers.DataContext))
                    _memberExpressionVisitor.Flags |= BindingMemberExpressionFlags.ParentDataContext;
                context.SourceExpression = _memberExpressionVisitor.Visit(context.SourceExpression, false, metadata);
                return;
            }

            flags = _memberExpressionVisitor.Flags;
            _memberExpressionVisitor.Flags &= ~(BindingMemberExpressionFlags.Observable | BindingMemberExpressionFlags.ObservableMethods);
            _memberExpressionVisitor.SuppressIndexAccessors = true;
            _memberExpressionVisitor.SuppressMethodAccessors = true;
            context.SourceExpression = _memberExpressionVisitor.Visit(context.SourceExpression, false, metadata);
            if (context.SourceExpression is IBindingMemberExpressionNode memberExpression)
            {
                var f = memberExpression.Flags;
                if (flags.HasFlag(BindingMemberExpressionFlags.Observable))
                    f |= BindingMemberExpressionFlags.Observable;
                if (flags.HasFlag(BindingMemberExpressionFlags.ObservableMethods))
                    f |= BindingMemberExpressionFlags.ObservableMethods;
                context.SourceExpression = memberExpression.Update(memberExpression.Index, f, memberExpression.MemberFlags, memberExpression.ObservableMethodName);
            }

            _memberExpressionVisitor.Flags |= BindingMemberExpressionFlags.Observable;
            context.TryGetParameterExpression(_compiler, _memberExpressionVisitor, _memberExpressionCollectorVisitor, BindingParameterNameConstant.CommandParameter, true, metadata,
                out var parameter);
            var toggle = context.TryGetParameterValue<bool?>(BindingParameterNameConstant.ToggleEnabled).GetValueOrDefault(ToggleEnabledState);
            context.Components[BindingParameterNameConstant.EventHandler] =
                new DelegateBindingComponentProvider<(BindingParameterExpression, bool, bool)>(GetEventHandlerDelegate, (parameter, toggle, IsOneTime(context)));
            context.Components[BindingParameterNameConstant.Mode] = null;
        }

        private static IComponent<IBinding> GetEventHandlerComponent((BindingParameterExpression value, bool toggle, bool isOneTime) state, IBinding binding, object target,
            object? source,
            IReadOnlyMetadataContext? metadata) => BindingEventHandler.Get(state.value.ToBindingParameter(target, source, metadata), state.toggle,
            state.isOneTime || binding.GetOrDefault(BindingMetadata.IsExpressionBinding));

        private bool IsOneTime(IBindingExpressionInitializerContext context) =>
            context.TryGetParameterValue<string>(BindingParameterNameConstant.Mode) == OneTimeBindingMode || context.TryGetParameterValue<bool>(OneTimeBindingMode);

        private bool IsEvent(object target, object? source, IExpressionNode targetExpression, IReadOnlyMetadataContext? metadata)
        {
            if (targetExpression is IBindingMemberExpressionNode bindingMemberExpression)
            {
                target = bindingMemberExpression.GetSource(target, source, metadata, out var path)!;
                if (target == null)
                    return false;
                var flags = bindingMemberExpression.MemberFlags;
                return path.GetLastMemberFromPath(flags.GetTargetType(ref target!), target, flags, MemberType.Event, metadata, _memberManager) != null;
            }

            return false;
        }
    }
}