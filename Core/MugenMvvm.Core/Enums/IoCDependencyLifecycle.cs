using System.Runtime.CompilerServices;
using MugenMvvm.Attributes;

namespace MugenMvvm.Enums
{
    public class IocDependencyLifecycle : EnumBase<IocDependencyLifecycle, int>
    {
        #region Fields

        public static readonly IocDependencyLifecycle Singleton = new IocDependencyLifecycle(1);
        public static readonly IocDependencyLifecycle Transient = new IocDependencyLifecycle(2);

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        protected IocDependencyLifecycle()
        {
        }

        public IocDependencyLifecycle(int value) : base(value)
        {
        }

        #endregion

        #region Methods

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(IocDependencyLifecycle? left, IocDependencyLifecycle? right)
        {
            if (ReferenceEquals(left, right))
                return true;
            if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
                return false;
            return left.Value == right.Value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(IocDependencyLifecycle? left, IocDependencyLifecycle? right)
        {
            return !(left == right);
        }

        protected override bool Equals(int value)
        {
            return Value == value;
        }

        #endregion
    }
}