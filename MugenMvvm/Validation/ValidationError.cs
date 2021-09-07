using System;
using System.Runtime.InteropServices;

namespace MugenMvvm.Validation
{
    [StructLayout(LayoutKind.Auto)]
    public readonly struct ValidationError
    {
        internal ValidationError(object error)
        {
            Should.NotBeNull(error, nameof(error));
            ErrorRaw = error;
        }

        internal object ErrorRaw { get; }

        public object Error
        {
            get
            {
                if (ErrorRaw is Func<object> func)
                    return func();
                return ErrorRaw;
            }
        }

        public static implicit operator ValidationError(string error) => new(error);

        public static implicit operator ValidationError(Func<object> getError) => new(getError);

        public static ValidationError FromDelegate(Func<object> getError) => new(getError);

        public static ValidationError FromValue(object error) => new(error);
    }
}