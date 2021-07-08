using System;
using System.Runtime.CompilerServices;

namespace MugenMvvm.Android.Internal
{
    public sealed class NativeStringAccessor
    {
        private IntPtr _handle;

        public unsafe ReadOnlySpan<char> Span
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => new(_handle.ToPointer(), Length);
        }

        public int Length { get; private set; }

        public override string ToString() => new(Span);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Initialize(IntPtr handle, int length)
        {
            _handle = handle;
            Length = length;
        }
    }
}