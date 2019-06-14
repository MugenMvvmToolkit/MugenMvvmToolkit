using MugenMvvm.Binding.Interfaces.Observers;

namespace MugenMvvm.Binding.Infrastructure.Observers
{
    public sealed class EmptyBindingPath : IBindingPath
    {
        #region Fields

        public static readonly EmptyBindingPath Instance = new EmptyBindingPath();

        #endregion

        #region Constructors

        private EmptyBindingPath()
        {
        }

        #endregion

        #region Properties

        public string Path => "";

        public string[] Parts => Default.EmptyArray<string>();

        public bool IsSingle => false;

        #endregion
    }
}