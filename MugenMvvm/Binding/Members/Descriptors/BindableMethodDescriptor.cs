using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace MugenMvvm.Bindings.Members.Descriptors
{
    [StructLayout(LayoutKind.Auto)]
    public readonly struct BindableMethodDescriptor<TTarget, TReturn> where TTarget : class
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

        #endregion

        #region Methods

        [Pure]
        public BindableMethodDescriptor<TNewTarget, TReturn> Override<TNewTarget>() where TNewTarget : class => new BindableMethodDescriptor<TNewTarget, TReturn>(Request!);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator string(BindableMethodDescriptor<TTarget, TReturn> member) => member.Request?.Name ?? "";

        public override string ToString() => Request?.ToString() ?? "";

        #endregion
    }
}