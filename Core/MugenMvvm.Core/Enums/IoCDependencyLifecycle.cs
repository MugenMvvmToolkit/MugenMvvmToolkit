using MugenMvvm.Attributes;
using MugenMvvm.Models;

namespace MugenMvvm.Enums
{
    public class IoCDependencyLifecycle : EnumBase<IoCDependencyLifecycle, int>
    {
        #region Fields

        public static readonly IoCDependencyLifecycle Singleton = new IoCDependencyLifecycle(1);
        public static readonly IoCDependencyLifecycle Transient = new IoCDependencyLifecycle(2);

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        protected IoCDependencyLifecycle()
        {
        }

        public IoCDependencyLifecycle(int value) : base(value)
        {
        }

        #endregion
    }
}