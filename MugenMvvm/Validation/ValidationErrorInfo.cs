using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace MugenMvvm.Validation
{
    [StructLayout(LayoutKind.Auto)]
    public readonly struct ValidationErrorInfo : IEquatable<ValidationErrorInfo>
    {
        public readonly object? Target;
        public readonly string Member;
        public readonly object? Error;

        public ValidationErrorInfo(object? target, string member, object? error)
        {
            Should.NotBeNull(member, nameof(member));
            Target = target;
            Member = member;
            Error = error;
        }

        [MemberNotNullWhen(false, nameof(Member))]
        public bool IsEmpty => Member == null;

        [MemberNotNullWhen(true, nameof(Error), nameof(Member))]
        public bool HasError => Error != null;

        public override string ToString() => Error?.ToString() ?? "";

        public bool Equals(ValidationErrorInfo other) => Equals(Target, other.Target) && Member == other.Member && Equals(Error, other.Error);

        public override bool Equals(object? obj) => obj is ValidationErrorInfo other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(Target, Member, Error);
    }
}