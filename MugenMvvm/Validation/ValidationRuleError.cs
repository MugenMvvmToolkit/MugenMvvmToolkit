using System;
using System.Runtime.InteropServices;

namespace MugenMvvm.Validation
{
    [StructLayout(LayoutKind.Auto)]
    public readonly struct ValidationRuleError
    {
        internal ValidationRuleError(object error)
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

        public static implicit operator ValidationRuleError(string error) => new(error);

        public static implicit operator ValidationRuleError(Func<object> getError) => new(getError);

        public static ValidationRuleError FromDelegate(Func<object> getError) => new(getError);

        public static ValidationRuleError FromValue(object error) => new(error);
    }
}