using MugenMvvm.Binding.Interfaces.Observers;

namespace MugenMvvm.Binding.Observers
{
    public sealed class EmptyMemberPath : IMemberPath
    {
        #region Fields

        public static readonly EmptyMemberPath Instance = new EmptyMemberPath();

        #endregion

        #region Constructors

        private EmptyMemberPath()
        {
        }

        #endregion

        #region Properties

        public string Path => "";

        public string[] Members => Default.EmptyArray<string>();

        public bool IsSingle => false;

        #endregion
    }
}