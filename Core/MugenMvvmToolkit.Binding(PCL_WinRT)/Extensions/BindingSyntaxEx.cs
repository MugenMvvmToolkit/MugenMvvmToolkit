#region Copyright

// ****************************************************************************
// <copyright file="BindingSyntaxEx.cs">
// Copyright (c) 2012-2015 Vyacheslav Volkov
// </copyright>
// ****************************************************************************
// <author>Vyacheslav Volkov</author>
// <email>vvs0205@outlook.com</email>
// <project>MugenMvvmToolkit</project>
// <web>https://github.com/MugenMvvmToolkit/MugenMvvmToolkit</web>
// <license>
// See license.txt in this solution or http://opensource.org/licenses/MS-PL
// </license>
// ****************************************************************************

#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using MugenMvvmToolkit.Binding.Attributes;
using MugenMvvmToolkit.Binding.Behaviors;
using MugenMvvmToolkit.Binding.DataConstants;
using MugenMvvmToolkit.Binding.Interfaces.Syntax;
using MugenMvvmToolkit.Binding.Models;
using MugenMvvmToolkit.Binding.Parse;
using MugenMvvmToolkit.Binding.Parse.Nodes;
using MugenMvvmToolkit.Infrastructure;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Models;

// ReSharper disable once CheckNamespace
namespace MugenMvvmToolkit.Binding.Extensions.Syntax
{
    [BindingSyntaxExtensions]
    public static class BindingSyntaxEx
    {
        #region Fields

        public const string ProvideExpressionMethodName = "ProvideExpression";

        private const string ResourceMethodName = "Resource";
        private const string SelfMethodName = "Self";
        private const string SourceMethodName = "Source";
        private const string RootMethodName = "Root";
        private const string RelativeMethodName = "Relative";
        private static readonly MethodInfo GetEventArgsMethod;
        private static readonly MethodInfo GetErrorsMethod;
        private static readonly MethodInfo ResourceMethodInfo;
        private static readonly MethodInfo ResourceMethodImplMethod;
        private static readonly object FirstLevelBoxed;

        #endregion

        #region Constructors

        static BindingSyntaxEx()
        {
            GetEventArgsMethod = typeof(BindingSyntaxEx).GetMethodEx("GetEventArgs",
                MemberFlags.NonPublic | MemberFlags.Static);
            GetErrorsMethod = typeof(BindingSyntaxEx).GetMethodEx("GetErrorsImpl",
                MemberFlags.NonPublic | MemberFlags.Static);
            ResourceMethodImplMethod = typeof(BindingSyntaxEx).GetMethodEx("ResourceMethodImpl",
                MemberFlags.NonPublic | MemberFlags.Static);
            ResourceMethodInfo = typeof(BindingSyntaxEx)
                .GetMethodsEx(MemberFlags.Public | MemberFlags.Static)
                .First(info => info.Name == ResourceMethodName && info.GetParameters().Length == 2);
            FirstLevelBoxed = 1u;
        }

        #endregion

        #region Methods

        public static T DataContext<T>(this IBindingSyntaxContext context)
        {
            return MethodNotSupported<T>();
        }

        public static T Relative<T>(this IBindingSyntaxContext context)
        {
            return MethodNotSupported<T>();
        }

        public static T Relative<T>(this IBindingSyntaxContext context, uint level)
        {
            return MethodNotSupported<T>();
        }

        public static T Element<T>(this IBindingSyntaxContext context, object elementId)
        {
            return MethodNotSupported<T>();
        }

        public static T Self<T>(this IBindingSyntaxContext context)
        {
            return MethodNotSupported<T>();
        }

        public static T Self<T, TSource>(this IBindingSyntaxContext<T, TSource> context)
            where T : class
        {
            return MethodNotSupported<T>();
        }

        public static T Root<T>(this IBindingSyntaxContext context)
        {
            return MethodNotSupported<T>();
        }

        public static T Source<T>(this IBindingSyntaxContext context)
        {
            return MethodNotSupported<T>();
        }

        public static T Source<TTarget, T>(this IBindingSyntaxContext<TTarget, T> context)
            where TTarget : class
        {
            return MethodNotSupported<T>();
        }

        public static T Resource<T>(this IBindingSyntaxContext context, string name)
        {
            return (T)BindingServiceProvider
                .ResourceResolver
                .ResolveObject(name, MugenMvvmToolkit.Models.DataContext.Empty, true)
                .Value;
        }

        public static T Resource<T>(this IBindingSyntaxContext context, string name, Expression<Func<T>> member)
        {
            var value = BindingServiceProvider
                .ResourceResolver
                .ResolveObject(name, MugenMvvmToolkit.Models.DataContext.Empty, true)
                .Value;
            return (T)BindingExtensions.GetValueFromPath(value, member.GetMemberInfo().Name);
        }

        public static T ResourceMethod<T>(this IBindingSyntaxContext context, string name, params object[] args)
        {
            return (T)ResourceMethodImpl(name, Empty.Array<Type>(), MugenMvvmToolkit.Models.DataContext.Empty, args);
        }

        [BindingSyntaxMember]
        public static T Member<T>(this object target, string member)
        {
            return target.GetBindingMemberValue<object, T>(member);
        }

        [BindingSyntaxMember]
        public static TValue Member<TSource, TValue>(this TSource target, BindingMemberDescriptor<TSource, TValue> member)
            where TSource : class
        {
            return target.GetBindingMemberValue(member);
        }

        public static T EventArgs<T>(this IBindingSyntaxContext context)
        {
            return MethodNotSupported<T>();
        }

        public static IEnumerable<object> GetErrors(this IBindingSyntaxContext context, params object[] args)
        {
            return MethodNotSupported<IEnumerable<object>>();
        }

        public static T OneTime<T>(this IBindingSyntaxContext context, T value)
        {
            return MethodNotSupported<T>();
        }

        [UsedImplicitly]
        private static Expression ProvideExpression(IBuilderSyntaxContext context)
        {
            var mExp = context.MethodExpression;
            var name = mExp.Method.Name;
            if (name == "EventArgs")
            {
                if (context.IsSameExpression())
                    return Expression.Convert(Expression.Call(GetEventArgsMethod, context.ContextParameter), mExp.Method.ReturnType);
                return null;
            }

            if (name == "ResourceMethod")
            {
                if (!context.IsSameExpression())
                    return null;
                var typeArgsEx = Expression.NewArrayInit(typeof(Type), Expression.Constant(mExp.Method.ReturnType, typeof(Type)));
                return Expression.Convert(Expression.Call(ResourceMethodImplMethod, mExp.Arguments[1], typeArgsEx,
                            context.ContextParameter, mExp.Arguments[2]), context.Expression.Type);
            }

            if (name == "OneTime")
            {
                if (!context.IsSameExpression())
                    return null;
                var item = Expression.Constant(new MacrosExpressionVisitor.OneTimeImpl());
                var parameter = Expression.Lambda(mExp.Arguments[1], Empty.Array<ParameterExpression>());
                return Expression.Call(item, MacrosExpressionVisitor.OneTimeImpl.GetValueMethod.MakeGenericMethod(mExp.Arguments[1].Type), parameter);
            }

            if (name == "GetErrors")
            {
                if (!context.IsSameExpression())
                    return null;
                var id = Guid.NewGuid();
                var args = new List<Expression>();
                var members = new List<string>();
                var arrayExpression = mExp.Arguments[1] as NewArrayExpression;
                if (arrayExpression != null)
                {
                    for (int i = 0; i < arrayExpression.Expressions.Count; i++)
                    {
                        var constantExpression = arrayExpression.Expressions[i] as ConstantExpression;
                        if (constantExpression == null)
                            args.Add(arrayExpression.Expressions[i]);
                        else
                            members.Add((string)constantExpression.Value);
                    }
                }
                if (args.Count == 0)
                    args.Add(Expression.Call(ResourceMethodInfo.MakeGenericMethod(typeof(object)), Expression.Constant(null, typeof(IBindingSyntaxContext)),
                        Expression.Constant(BindingServiceProvider.ResourceResolver.BindingSourceResourceName)));
                context.AddBuildCallback(syntax =>
                {
                    var behaviors = syntax.Builder.GetOrAddBehaviors();
                    if (!behaviors.Any(behavior => behavior is NotifyDataErrorsAggregatorBehavior))
                    {
                        behaviors.Clear();
                        behaviors.Add(new OneTimeBindingMode(false));
                    }
                    behaviors.Add(new NotifyDataErrorsAggregatorBehavior(id) { ErrorPaths = members.ToArray() });
                });
                var array = Expression.NewArrayInit(typeof(object), args.Select(e => ExpressionReflectionManager.ConvertIfNeed(e, typeof(object), false)));
                return Expression.Call(GetErrorsMethod, Expression.Constant(id), context.ContextParameter, array);
            }

            Expression lastExpression;
            string path = string.Empty;
            if (!context.IsSameExpression() &&
                !BindingExtensions.TryGetMemberPath(context.Expression, ".", false, out lastExpression, out path) &&
                lastExpression != mExp)
                return null;

            if (name == AttachedMemberConstants.DataContext)
                return context.GetOrAddParameterExpression(string.Empty, path, context.Expression,
                    (dataContext, s) => BindingExtensions.CreateBindingSource(dataContext, s, null, true));

            if (name == SelfMethodName || name == RootMethodName || name == ResourceMethodName || name == SourceMethodName)
            {
                string resourceName;
                switch (name)
                {
                    case SelfMethodName:
                        resourceName = BindingServiceProvider.ResourceResolver.SelfResourceName;
                        break;
                    case RootMethodName:
                        resourceName = BindingServiceProvider.ResourceResolver.RootElementResourceName;
                        break;
                    case SourceMethodName:
                        resourceName = BindingServiceProvider.ResourceResolver.BindingSourceResourceName;
                        break;
                    case ResourceMethodName:
                        mExp.Arguments[1].TryGetStaticValue(out resourceName, true);
                        if (mExp.Arguments.Count == 3)
                        {
#pragma warning disable 618
                            LambdaExpression lambda = mExp.Arguments[2] as LambdaExpression;
                            if (lambda == null)
                                lambda = (LambdaExpression)((UnaryExpression)mExp.Arguments[2]).Operand;
                            path = BindingExtensions.MergePath(lambda.GetMemberInfo().Name, path);
#pragma warning restore 618
                        }
                        break;
                    default:
                        mExp.Arguments[1].TryGetStaticValue(out resourceName, true);
                        break;
                }
                return context.GetOrAddParameterExpression("res:" + resourceName, path, context.Expression,
                    (dataContext, s) =>
                    {
                        var value = BindingServiceProvider
                            .ResourceResolver
                            .ResolveObject(resourceName, dataContext, true);
                        return BindingExtensions.CreateBindingSource(dataContext, s, value);
                    });
            }

            if (name == RelativeMethodName || name == "Element")
            {
                object firstArg;
                if (mExp.Arguments.Count == 1)
                    firstArg = FirstLevelBoxed;
                else
                    mExp.Arguments[1].TryGetStaticValue(out firstArg, true);
                var node = name == RelativeMethodName
                    ? RelativeSourceExpressionNode
                        .CreateRelativeSource(mExp.Method.ReturnType.AssemblyQualifiedName, (uint)firstArg, null)
                    : RelativeSourceExpressionNode.CreateElementSource(firstArg.ToString(), null);
                return context
                    .GetOrAddParameterExpression(name + mExp.Method.ReturnType.FullName, path, context.Expression,
                        (dataContext, s) => BindingExtensions.CreateBindingSource(node, dataContext.GetData(BindingBuilderConstants.Target), s));
            }
            return null;
        }

        private static bool IsSameExpression(this IBuilderSyntaxContext context)
        {
            return context.MethodExpression == context.Expression;
        }

        [UsedImplicitly]
        private static object GetEventArgs(IDataContext context)
        {
            return context.GetData(BindingConstants.CurrentEventArgs);
        }

        private static object ResourceMethodImpl(string name, IList<Type> typeArgs, IDataContext context, params object[] args)
        {
            return BindingServiceProvider
                .ResourceResolver
                .ResolveMethod(name, context, true)
                .Invoke(typeArgs, args, context);
        }

        internal static IEnumerable<object> GetErrorsImpl(Guid id, IDataContext context, object[] args)
        {
            var binding = context.GetData(BindingConstants.Binding);
            if (binding == null)
                return Empty.Array<object>();
            foreach (var behavior in binding.Behaviors)
            {
                if (behavior.Id == id)
                    return ((NotifyDataErrorsAggregatorBehavior)behavior).Errors;
            }
            return Empty.Array<object>();
        }

        private static T MethodNotSupported<T>()
        {
            throw new NotSupportedException("This method is used exclusively for the construction of binding expression.");
        }

        #endregion
    }
}
