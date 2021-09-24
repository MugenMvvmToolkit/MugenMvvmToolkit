﻿using System;
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
        private static readonly
            Func<(BindingParameterExpression, BindingParameterExpression, BindingParameterExpression, BindingParameterExpression), IBinding, object, object?,
                IReadOnlyMetadataContext?, IComponent<IBinding>?>
            GetParametersComponentDelegate = GetParametersComponent;

        private readonly IExpressionCompiler? _compiler;
        private readonly BindingMemberExpressionCollectorVisitor _memberExpressionCollectorVisitor;
        private readonly BindingMemberExpressionVisitor _memberExpressionVisitor;

        [Preserve(Conditional = true)]
        public BindingParameterInitializer(IExpressionCompiler? compiler = null)
        {
            _memberExpressionVisitor = new BindingMemberExpressionVisitor
            {
                SuppressIndexAccessors = true,
                SuppressMethodAccessors = true,
                MemberFlags = Enums.MemberFlags.All & ~Enums.MemberFlags.NonPublic
            };
            _memberExpressionCollectorVisitor = new BindingMemberExpressionCollectorVisitor();
            _compiler = compiler;
        }

        public EnumFlags<MemberFlags> MemberFlags
        {
            get => _memberExpressionVisitor.MemberFlags;
            set => _memberExpressionVisitor.MemberFlags = value;
        }

        public int Priority { get; init; } = BindingComponentPriority.ParameterInitializer;

        public void Initialize(IBindingManager bindingManager, IBindingExpressionInitializerContext context)
        {
            if (context.Components.ContainsKey(BindingParameterNameConstant.ParameterHandler))
                return;

            EnumFlags<BindingMemberExpressionFlags> flags = BindingMemberExpressionFlags.Observable;
            context.ApplyFlags(BindingParameterNameConstant.Optional, BindingMemberExpressionFlags.Optional, ref flags);
            context.ApplyFlags(BindingParameterNameConstant.HasStablePath, BindingMemberExpressionFlags.StablePath, ref flags);
            _memberExpressionVisitor.Flags = flags;

            var metadata = context.GetMetadataOrDefault();
            var hasConverter = context.TryGetParameterExpression(_compiler, _memberExpressionVisitor, _memberExpressionCollectorVisitor,
                BindingParameterNameConstant.Converter, false, metadata, out var converter);
            var hasConverterParameter = context.TryGetParameterExpression(_compiler, _memberExpressionVisitor, _memberExpressionCollectorVisitor,
                BindingParameterNameConstant.ConverterParameter, true, metadata, out var converterParameter);
            var hasFallback = context.TryGetParameterExpression(_compiler, _memberExpressionVisitor, _memberExpressionCollectorVisitor,
                BindingParameterNameConstant.Fallback, false, metadata, out var fallback);
            var hasTargetNullValue = context.TryGetParameterExpression(_compiler, _memberExpressionVisitor, _memberExpressionCollectorVisitor,
                BindingParameterNameConstant.TargetNullValue, false, metadata, out var targetNullValue);
            if (hasConverter || hasConverterParameter || hasFallback || hasTargetNullValue)
            {
                var state = (converter, converterParameter, fallback, targetNullValue);
                var provider =
                    new DelegateBindingComponentProvider<(BindingParameterExpression, BindingParameterExpression, BindingParameterExpression, BindingParameterExpression)>(
                        GetParametersComponentDelegate, state);
                context.Components[BindingParameterNameConstant.ParameterHandler] = provider;
            }
        }

        private static IComponent<IBinding> GetParametersComponent(
            (BindingParameterExpression, BindingParameterExpression, BindingParameterExpression, BindingParameterExpression) state,
            IBinding binding, object target, object? source, IReadOnlyMetadataContext? metadata)
        {
            var (converter, converterParameter, fallback, targetNullValue) = state;
            return new BindingParameterHandler(converter.ToBindingParameter(target, source, metadata), converterParameter.ToBindingParameter(target, source, metadata),
                fallback.ToBindingParameter(target, source, metadata), targetNullValue.ToBindingParameter(target, source, metadata));
        }
    }
}