using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Interfaces.Internal;

namespace MugenMvvm.Binding.Observers
{
    public sealed class EmptyMemberPath : IMemberPath, IValueHolder<string>
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

        string IValueHolder<string>.Value { get; set; }

        #endregion
    }
}