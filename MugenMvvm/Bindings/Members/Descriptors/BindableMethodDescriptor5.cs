using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace MugenMvvm.Bindings.Members.Descriptors
{
    [StructLayout(LayoutKind.Auto)]
    public readonly struct BindableMethodDescriptor<TTarget, TArg1, TArg2, TArg3, TArg4, TArg5, TReturn> where TTarget : class
    {
        public readonly MemberTypesRequest? Request;

        public BindableMethodDescriptor(MemberTypesRequest request)
        {
            Should.NotBeNull(request, nameof(request));
            Request = request;
        }

        public bool IsStatic => typeof(TTarget) == typeof(Type);

        public BindableMethodDescriptor<TTarget, TReturn> RawMethod => this;

        [Pure]
        public BindableMethodDescriptor<TNewTarget, TArg1, TArg2, TArg3, TArg4, TArg5, TReturn> Override<TNewTarget>() where TNewTarget : class => Request!;

        public static implicit operator BindableMethodDescriptor<TTarget, TArg1, TArg2, TArg3, TArg4, TArg5, TReturn>(MemberTypesRequest request) =>
            new(request);

        public static implicit operator MemberTypesRequest(BindableMethodDescriptor<TTarget, TArg1, TArg2, TArg3, TArg4, TArg5, TReturn> member) => member.Request!;

        public static implicit operator string(BindableMethodDescriptor<TTarget, TArg1, TArg2, TArg3, TArg4, TArg5, TReturn> member) => member.Request?.Name ?? "";

        public static implicit operator BindableMethodDescriptor<TTarget, TReturn>(BindableMethodDescriptor<TTarget, TArg1, TArg2, TArg3, TArg4, TArg5, TReturn> member) =>
            new(member.Request!);

        public override string ToString() => Request?.ToString() ?? "";
    }
}