using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Core.Components.Binding;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Extensions;
using MugenMvvm.Binding.Interfaces.Compiling;
using MugenMvvm.Binding.Interfaces.Core;
using MugenMvvm.Binding.Interfaces.Core.Components;
using MugenMvvm.Binding.Parsing.Visitors;
using MugenMvvm.Delegates;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Binding.Core.Components
{
    public sealed class BindingParameterInitializerComponent : IBindingExpressionInitializerComponent, IHasPriority
    {
        #region Fields

        private readonly IExpressionCompiler? _compiler;

        private static readonly
            FuncIn<(BindingParameterExpression, BindingParameterExpression, BindingParameterExpression, BindingParameterExpression), IBinding, object, object?, IReadOnlyMetadataContext?, IComponent<IBinding>?>
            GetParametersComponentDelegate = GetParametersComponent;

        private static readonly BindingMemberExpressionVisitor MemberExpressionVisitor = new BindingMemberExpressionVisitor { IgnoreIndexMembers = true, IgnoreMethodMembers = true };
        private static readonly BindingMemberExpressionCollectorVisitor MemberExpressionCollectorVisitor = new BindingMemberExpressionCollectorVisitor();

        #endregion

        #region Constructors

        public BindingParameterInitializerComponent(IExpressionCompiler? compiler = null)
        {
            _compiler = compiler;
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = BindingComponentPriority.BindingParameterInitializer;

        public MemberFlags MemberFlags { get; set; } = MemberFlags.All & ~MemberFlags.NonPublic;

        #endregion

        #region Implementation of interfaces

        public void Initialize(IBindingExpressionInitializerContext context)
        {
            if (context.BindingComponents.ContainsKey(BindingParameterNameConstant.ParameterHandler) || context.BindingComponents.ContainsKey(BindingParameterNameConstant.EventHandler))
                return;

            MemberExpressionVisitor.MemberFlags = MemberFlags;
            MemberExpressionVisitor.Flags = BindingMemberExpressionFlags.Observable;
            var metadata = context.GetMetadataOrDefault();
            var converter = context.TryGetParameterExpression(_compiler, MemberExpressionVisitor, MemberExpressionCollectorVisitor, BindingParameterNameConstant.Converter, metadata);
            var converterParameter = context.TryGetParameterExpression(_compiler, MemberExpressionVisitor, MemberExpressionCollectorVisitor, BindingParameterNameConstant.ConverterParameter, metadata);
            var fallback = context.TryGetParameterExpression(_compiler, MemberExpressionVisitor, MemberExpressionCollectorVisitor, BindingParameterNameConstant.Fallback, metadata);
            var targetNullValue = context.TryGetParameterExpression(_compiler, MemberExpressionVisitor, MemberExpressionCollectorVisitor, BindingParameterNameConstant.TargetNullValue, metadata);
            if (!converter.IsEmpty || !converterParameter.IsEmpty || !fallback.IsEmpty || !targetNullValue.IsEmpty)
            {
                var state = (converter, converterParameter, fallback, targetNullValue);
                var provider = new DelegateBindingComponentProvider<(BindingParameterExpression, BindingParameterExpression, BindingParameterExpression, BindingParameterExpression)>(GetParametersComponentDelegate, state);
                context.BindingComponents[BindingParameterNameConstant.ParameterHandler] = provider;
            }
        }

        #endregion

        #region Methods

        private static IComponent<IBinding> GetParametersComponent(in (BindingParameterExpression, BindingParameterExpression, BindingParameterExpression, BindingParameterExpression) state,
            IBinding binding, object target, object? source, IReadOnlyMetadataContext? metadata)
        {
            var (converter, converterParameter, fallback, targetNullValue) = state;
            return new ParameterHandlerInterceptorBindingComponent(converter.ToBindingParameter(target, source, metadata), converterParameter.ToBindingParameter(target, source, metadata),
                fallback.ToBindingParameter(target, source, metadata), targetNullValue.ToBindingParameter(target, source, metadata));
        }

        #endregion
    }
}