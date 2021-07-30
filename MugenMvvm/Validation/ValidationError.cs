using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MugenMvvm.Validation
{
    [StructLayout(LayoutKind.Auto)]
    public readonly struct ValidationError
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal ValidationError(object error)
        {
            Should.NotBeNull(error, nameof(error));
            ErrorRaw = error;
        }

        internal object ErrorRaw
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get;
        }

        public object Error
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (ErrorRaw is Func<object> func)
                    return func();
                return ErrorRaw;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ValidationError(string error) => new(error);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ValidationError(Func<object> getError) => new(getError);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ValidationError FromDelegate(Func<object> getError) => new(getError);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ValidationError FromValue(object error) => new(error);
    }
}