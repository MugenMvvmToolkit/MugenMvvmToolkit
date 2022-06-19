using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace MugenMvvm.Internal
{
    [StructLayout(LayoutKind.Auto)]
    public readonly struct VoidResult : IEquatable<VoidResult>, IComparable<VoidResult>, IComparable
    {
        public static readonly VoidResult Value;

        public static readonly Task<VoidResult> TaskValue = Task.FromResult(Value);

        public bool Equals(VoidResult other) => true;

        public int CompareTo(VoidResult other) => 0;

        public int CompareTo(object? obj) => 0;

        public override int GetHashCode() => 0;

        public override bool Equals(object? obj) => obj is VoidResult;

        public static bool operator ==(VoidResult left, VoidResult right) => true;

        public static bool operator !=(VoidResult left, VoidResult right) => false;

        public override string ToString() => "void";
    }
}