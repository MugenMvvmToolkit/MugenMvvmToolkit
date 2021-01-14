using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using MugenMvvm.Bindings.Interfaces.Parsing;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Internal;

namespace MugenMvvm.Bindings.Attributes
{
    [AttributeUsage(AttributeTargets.All)]
    public abstract class BindingSyntaxExtensionAttributeBase : Attribute
    {
        private static readonly Dictionary<MemberInfo, BindingSyntaxExtensionAttributeBase?> Cache = new(23, InternalEqualityComparer.MemberInfo);

        public static BindingSyntaxExtensionAttributeBase? TryGet(MemberInfo member)
        {
            Should.NotBeNull(member, nameof(member));
            lock (Cache)
            {
                if (!Cache.TryGetValue(member, out var attribute))
                {
                    attribute = member.GetCustomAttribute<BindingSyntaxExtensionAttributeBase>(true);
                    Cache[member] = attribute;
                }

                return attribute;
            }
        }

        public bool TryConvert(IExpressionConverterContext<Expression> context, Expression? expression, out IExpressionNode? result)
        {
            Should.NotBeNull(context, nameof(context));
            return TryConvertInternal(context, expression, out result);
        }

        protected abstract bool TryConvertInternal(IExpressionConverterContext<Expression> context, Expression? expression, out IExpressionNode? result);
    }
}