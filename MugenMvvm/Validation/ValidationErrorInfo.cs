using System.Runtime.InteropServices;

namespace MugenMvvm.Validation
{
    [StructLayout(LayoutKind.Auto)]
    public readonly struct ValidationErrorInfo
    {
        public readonly object? Target;
        public readonly string Member;
        public readonly object Error;

        public ValidationErrorInfo(object? target, string member, object error)
        {
            Should.NotBeNull(member, nameof(member));
            Should.NotBeNull(error, nameof(error));
            Target = target;
            Member = member;
            Error = error;
        }

        public bool IsEmpty => Member == null;

        public override string ToString() => Error?.ToString() ?? "";
    }
}