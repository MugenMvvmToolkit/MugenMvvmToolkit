using System;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using MugenMvvm.Binding.Interfaces.Build;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Binding.Members;
using MugenMvvm.Binding.Parsing.Expressions;

namespace MugenMvvm.Binding.Build
{
    [StructLayout(LayoutKind.Auto)]
    public readonly ref struct BindingBuilderTarget<TTarget, TSource>
        where TTarget : class
        where TSource : class
    {
        #region Properties

        public IBindableMembersDescriptor<TTarget> TargetMembers => BindableMembers.For<TTarget>();

        public IBindableMembersDescriptor<TSource> SourceMembers => BindableMembers.For<TSource>();

        #endregion

        #region Methods

        public BindingBuilderFrom<TTarget, TSource> For(string path)
        {
            return new BindingBuilderFrom<TTarget, TSource>(path);
        }

        public BindingBuilderFrom<TTarget, TSource> For(IExpressionNode expression)
        {
            return new BindingBuilderFrom<TTarget, TSource>(expression);
        }

        public BindingBuilderFrom<TTarget, TSource> For<TValue>(Expression<Func<TTarget, TValue>> expression)
        {
            return new BindingBuilderFrom<TTarget, TSource>(expression);
        }

        public BindingBuilderTo<TTarget, TSource> Action(Expression<Func<IBindingBuilderContext<TTarget, TSource>, object?>> expression)
        {
            return Action<TSource>(expression);
        }

        public BindingBuilderTo<TTarget, T> Action<T>(Expression<Func<IBindingBuilderContext<TTarget, T>, object?>> expression)
            where T : class
        {
            return new BindingBuilderTo<TTarget, T>(new BindingBuilderFrom<TTarget, T>(MemberExpressionNode.Action), expression, default);
        }

        public BindingBuilderTo<TTarget, TSource> Action(Expression<Action<IBindingBuilderContext<TTarget, TSource>>> expression)
        {
            return Action<TSource>(expression);
        }

        public BindingBuilderTo<TTarget, T> Action<T>(Expression<Action<IBindingBuilderContext<TTarget, T>>> expression)
            where T : class
        {
            return new BindingBuilderTo<TTarget, T>(new BindingBuilderFrom<TTarget, T>(MemberExpressionNode.Action), expression, default);
        }

        #endregion
    }
}