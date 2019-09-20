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
    }
}