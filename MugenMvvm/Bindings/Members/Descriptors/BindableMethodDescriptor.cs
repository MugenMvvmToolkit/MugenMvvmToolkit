using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace MugenMvvm.Bindings.Members.Descriptors
{
    [StructLayout(LayoutKind.Auto)]
    public readonly struct BindableMethodDescriptor<TTarget, TReturn> where TTarget : class
    {
        public readonly MemberTypesRequest? Request;

        public BindableMethodDescriptor(MemberTypesRequest request)
        {
            Should.NotBeNull(request, nameof(request));
            Request = request;
        }

        public bool IsStatic => typeof(TTarget) == typeof(Type);

        [Pure]
        public BindableMethodDescriptor<TNewTarget, TReturn> Override<TNewTarget>() where TNewTarget : class => new(Request!);

        public static implicit operator string(BindableMethodDescriptor<TTarget, TReturn> member) => member.Request?.Name ?? "";

        public override string ToString() => Request?.ToString() ?? "";
    }
}