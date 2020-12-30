using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace MugenMvvm.Bindings.Members.Descriptors
{
    [StructLayout(LayoutKind.Auto)]
    public readonly struct BindableMethodDescriptor<TTarget, TArg1, TArg2, TArg3, TArg4, TReturn> where TTarget : class
    {
        #region Fields

        public readonly MemberTypesRequest? Request;

        #endregion

        #region Constructors

        public BindableMethodDescriptor(MemberTypesRequest request)
        {
            Should.NotBeNull(request, nameof(request));
            Request = request;
        }

        #endregion

        #region Properties

        public bool IsStatic
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => typeof(TTarget) == typeof(Type);
        }

        public BindableMethodDescriptor<TTarget, TReturn> RawMethod
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this;
        }

        #endregion

        #region Methods

        [Pure]
        public BindableMethodDescriptor<TNewTarget, TArg1, TArg2, TArg3, TArg4, TReturn> Override<TNewTarget>() where TNewTarget : class => Request!;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator BindableMethodDescriptor<TTarget, TArg1, TArg2, TArg3, TArg4, TReturn>(MemberTypesRequest request) =>
            new(request);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator MemberTypesRequest(BindableMethodDescriptor<TTarget, TArg1, TArg2, TArg3, TArg4, TReturn> member) => member.Request!;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator string(BindableMethodDescriptor<TTarget, TArg1, TArg2, TArg3, TArg4, TReturn> member) => member.Request?.Name ?? "";

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator BindableMethodDescriptor<TTarget, TReturn>(BindableMethodDescriptor<TTarget, TArg1, TArg2, TArg3, TArg4, TReturn> member) =>
            new(member.Request!);

        public override string ToString() => Request?.ToString() ?? "";

        #endregion
    }
}