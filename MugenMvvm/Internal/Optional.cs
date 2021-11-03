using System.Runtime.CompilerServices;

namespace MugenMvvm.Internal
{
    public static class Optional
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Optional<T> Get<T>(T? value) => new(value, value != null);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Optional<T> Get<T>(T? value, bool hasValue) => new(value, hasValue);
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Optional<T> GetNullable<T>(T? value) where T : class => new(value);
    }
}