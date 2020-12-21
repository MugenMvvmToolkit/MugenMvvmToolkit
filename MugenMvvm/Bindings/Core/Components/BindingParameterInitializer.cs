using System;
using MugenMvvm.Attributes;
using MugenMvvm.Bindings.Constants;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Bindings.Interfaces.Compiling;
using MugenMvvm.Bindings.Interfaces.Core;
using MugenMvvm.Bindings.Interfaces.Core.Components;
using MugenMvvm.Bindings.Parsing.Visitors;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Bindings.Core.Components
{
    public sealed class BindingParameterInitializer : IBindingExpressionInitializerComponent, IHasPriority
    {
        #region Fields

        private readonly IExpressionCompiler? _compiler;
        private readonly BindingMemberExpressionCollectorVisitor _memberExpressionCollectorVisitor;
        private readonly BindingMemberExpressionVisitor _memberExpressionVisitor;

        private static readonly
            Func<(BindingParameterExpression, BindingParameterExpression, BindingParameterExpression, BindingParameterExpression), IBinding, object, object?, IReadOnlyMetadataContext?, IComponent<IBinding>?>
            GetParametersComponentDelegate = GetParametersComponent;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public BindingParameterInitializer(IExpressionCompiler? compiler = null)
        {
            _memberExpressionVisitor = new BindingMemberExpressionVisitor
            {
                IgnoreIndexMembers = true,
                IgnoreMethodMembers = true,
                MemberFlags = Enums.MemberFlags.All & ~Enums.MemberFlags.NonPublic
            };
            _memberExpressionCollectorVisitor = new BindingMemberExpressionCollectorVisitor();
            _compiler = compiler;
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = BindingComponentPriority.BindingParameterInitializer;

        public EnumFlags<MemberFlags> MemberFlags
        {
            get => _memberExpressionVisitor.MemberFlags;
            set => _memberExpressionVisitor.MemberFlags = value;
        }

        #endregion

        #region Implementation of interfaces

        public void Initialize(IBindingManager bindingManager, IBindingExpressionInitializerContext context)
        {
            if (context.Components.ContainsKey(BindingParameterNameConstant.ParameterHandler) || context.Components.ContainsKey(BindingParameterNameConstant.EventHandler))
                return;

            _memberExpressionVisitor.Flags = BindingMemberExpressionFlags.Observable;
            context.ApplyFlags(_memberExpressionVisitor, BindingParameterNameConstant.Optional, BindingMemberExpressionFlags.Optional);
            context.ApplyFlags(_memberExpressionVisitor, BindingParameterNameConstant.HasStablePath, BindingMemberExpressionFlags.StablePath);
            var metadata = context.GetMetadataOrDefault();
            var converter = context.TryGetParameterExpression(_compiler, _memberExpressionVisitor, _memberExpressionCollectorVisitor, BindingParameterNameConstant.Converter, metadata);
            var converterParameter = context.TryGetParameterExpression(_compiler, _memberExpressionVisitor, _memberExpressionCollectorVisitor, BindingParameterNameConstant.ConverterParameter, metadata);
            var fallback = context.TryGetParameterExpression(_compiler, _memberExpressionVisitor, _memberExpressionCollectorVisitor, BindingParameterNameConstant.Fallback, metadata);
            var targetNullValue = context.TryGetParameterExpression(_compiler, _memberExpressionVisitor, _memberExpressionCollectorVisitor, BindingParameterNameConstant.TargetNullValue, metadata);
            if (!converter.IsEmpty || !converterParameter.IsEmpty || !fallback.IsEmpty || !targetNullValue.IsEmpty)
            {
                var state = (converter, converterParameter, fallback, targetNullValue);
                var provider =
                    new DelegateBindingComponentProvider<(BindingParameterExpression, BindingParameterExpression, BindingParameterExpression, BindingParameterExpression)>(GetParametersComponentDelegate, state);
                context.Components[BindingParameterNameConstant.ParameterHandler] = provider;
            }
        }

        #endregion

        #region Methods

        private static IComponent<IBinding> GetParametersComponent((BindingParameterExpression, BindingParameterExpression, BindingParameterExpression, BindingParameterExpression) state,
            IBinding binding, object target, object? source, IReadOnlyMetadataContext? metadata)
        {
            var (converter, converterParameter, fallback, targetNullValue) = state;
            return new ParameterHandlerBindingComponent(converter.ToBindingParameter(target, source, metadata), converterParameter.ToBindingParameter(target, source, metadata),
                fallback.ToBindingParameter(target, source, metadata), targetNullValue.ToBindingParameter(target, source, metadata));
        }

        #endregion
    }
}