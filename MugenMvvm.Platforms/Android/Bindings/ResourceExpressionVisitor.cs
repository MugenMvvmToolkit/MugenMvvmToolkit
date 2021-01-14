using System;
using System.Collections.Generic;
using Android.Graphics;
using MugenMvvm.Android.Constants;
using MugenMvvm.Android.Native;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Bindings.Interfaces.Parsing;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Bindings.Parsing.Expressions;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Android.Bindings
{
    public sealed class ResourceExpressionVisitor : IExpressionVisitor
    {
        public ResourceExpressionVisitor()
        {
            Resources = new Dictionary<string, (Func<string, object?> resolver, Delegate genericResolver)>(StringComparer.Ordinal);
            var stringResolver = new Func<string, string>(MugenAndroidUtils.GetResourceString);
            Resources[AndroidInternalConstant.ColorResource] = (s => new Color(MugenAndroidUtils.GetResourceColor(s)),
                new Func<string, Color>(s => new Color(MugenAndroidUtils.GetResourceColor(s))));
            Resources[AndroidInternalConstant.BoolResource] =
                (s => BoxingExtensions.Box(MugenAndroidUtils.GetResourceBool(s)), new Func<string, bool>(MugenAndroidUtils.GetResourceBool));
            Resources[AndroidInternalConstant.DimenResource] =
                (s => BoxingExtensions.Box(MugenAndroidUtils.GetResourceDimen(s)), new Func<string, float>(MugenAndroidUtils.GetResourceDimen));
            Resources[AndroidInternalConstant.IdResource] = (s => BoxingExtensions.Box(MugenAndroidUtils.GetResourceId(s)), new Func<string, int>(MugenAndroidUtils.GetResourceId));
            Resources[AndroidInternalConstant.LayoutResource] =
                (s => BoxingExtensions.Box(MugenAndroidUtils.GetResourceLayout(s)), new Func<string, int>(MugenAndroidUtils.GetResourceLayout));
            Resources[AndroidInternalConstant.IntegerResource] = (s => BoxingExtensions.Box(MugenAndroidUtils.GetResourceInteger(s)),
                new Func<string, int>(MugenAndroidUtils.GetResourceInteger));
            Resources[AndroidInternalConstant.StringResource] = (stringResolver, stringResolver);
        }

        public Dictionary<string, (Func<string, object?> resolver, Delegate genericResolver)> Resources { get; }

        public ExpressionTraversalType TraversalType => ExpressionTraversalType.Preorder;

        public IExpressionNode Visit(IExpressionNode expression, IReadOnlyMetadataContext? metadata)
        {
            if (!(expression is IMemberExpressionNode member) || !(member.Target is IUnaryExpressionNode unaryExpression) || !unaryExpression.IsMacros() ||
                !(unaryExpression.Operand is IMemberExpressionNode resourceMember) || !Resources.TryGetValue(resourceMember.Member, out var resolver))
                return expression;

            if (unaryExpression.Token == UnaryTokenType.StaticExpression)
                return ConstantExpressionNode.Get(resolver.resolver(member.Member));
            return new MethodCallExpressionNode(ConstantExpressionNode.Get(resolver.genericResolver), nameof(resolver.resolver.Invoke), ConstantExpressionNode.Get(member.Member));
        }
    }
}