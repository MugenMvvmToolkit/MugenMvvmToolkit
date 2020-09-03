using System;
using System.Collections.Generic;
using Android.Graphics;
using MugenMvvm.Android.Constants;
using MugenMvvm.Android.Native;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Extensions;
using MugenMvvm.Binding.Interfaces.Parsing;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Binding.Parsing.Expressions;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Android.Binding
{
    public sealed class AndroidResourceExpressionVisitor : IExpressionVisitor
    {
        #region Constructors

        public AndroidResourceExpressionVisitor()
        {
            Resources = new Dictionary<string, (Func<string, object?> resolver, Delegate genericResolver)>(StringComparer.Ordinal);
            var stringResolver = new Func<string, string>(MugenNativeUtils.GetResourceString);
            Resources[AndroidInternalConstant.ColorResource] = (s => new Color(MugenNativeUtils.GetResourceColor(s)), new Func<string, Color>(s => new Color(MugenNativeUtils.GetResourceColor(s))));
            Resources[AndroidInternalConstant.BoolResource] = (s => BoxingExtensions.Box(MugenNativeUtils.GetResourceBool(s)), new Func<string, bool>(MugenNativeUtils.GetResourceBool));
            Resources[AndroidInternalConstant.DimenResource] = (s => BoxingExtensions.Box(MugenNativeUtils.GetResourceDimen(s)), new Func<string, float>(MugenNativeUtils.GetResourceDimen));
            Resources[AndroidInternalConstant.IdResource] = (s => BoxingExtensions.Box(MugenNativeUtils.GetResourceId(s)), new Func<string, int>(MugenNativeUtils.GetResourceId));
            Resources[AndroidInternalConstant.IntegerResource] = (s => BoxingExtensions.Box(MugenNativeUtils.GetResourceInteger(s)), new Func<string, int>(MugenNativeUtils.GetResourceInteger));
            Resources[AndroidInternalConstant.StringResource] = (stringResolver, stringResolver);
        }

        #endregion

        #region Properties

        public bool IsPostOrder => false;

        public Dictionary<string, (Func<string, object?> resolver, Delegate genericResolver)> Resources { get; }

        #endregion

        #region Implementation of interfaces

        public IExpressionNode? Visit(IExpressionNode expression, IReadOnlyMetadataContext? metadata)
        {
            if (!(expression is IMemberExpressionNode member) || !(member.Target is IUnaryExpressionNode unaryExpression) || !unaryExpression.IsMacros() ||
                !(unaryExpression.Operand is IMemberExpressionNode resourceMember) || !Resources.TryGetValue(resourceMember.Member, out var resolver))
                return expression;

            if (unaryExpression.Token == UnaryTokenType.StaticExpression)
                return ConstantExpressionNode.Get(resolver.resolver(member.Member));
            return new MethodCallExpressionNode(ConstantExpressionNode.Get(resolver.genericResolver), nameof(resolver.resolver.Invoke), new[] { ConstantExpressionNode.Get(member.Member) });
        }

        #endregion
    }
}