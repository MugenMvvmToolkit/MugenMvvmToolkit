using System;
using System.Linq.Expressions;
using System.Reflection;
using MugenMvvm.Binding.Interfaces.Parsing;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Collections.Internal;

namespace MugenMvvm.Binding.Attributes
{
    [AttributeUsage(AttributeTargets.All)]
    public abstract class BindingSyntaxExtensionAttributeBase : Attribute
    {
        #region Fields

        private static readonly MemberInfoLightDictionary<MemberInfo, BindingSyntaxExtensionAttributeBase?> Cache = new MemberInfoLightDictionary<MemberInfo, BindingSyntaxExtensionAttributeBase?>(23);

        #endregion

        #region Methods

        public static BindingSyntaxExtensionAttributeBase? TryGet(MemberInfo member)
        {
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

        #endregion
    }
}