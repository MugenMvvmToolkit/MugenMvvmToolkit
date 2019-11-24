using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Interfaces.Internal;

namespace MugenMvvm.Binding.Observers
{
    public sealed class SingleMemberPath : IMemberPath, IValueHolder<string>
    {
        #region Fields

        private string[]? _members;

        #endregion

        #region Constructors

        public SingleMemberPath(string path)
        {
            Should.NotBeNull(path, nameof(path));
            Path = path;
        }

        #endregion

        #region Properties

        public string Path { get; }

        public string[] Members
        {
            get
            {
                if (_members == null)
                    _members = new[] {Path};
                return _members;
            }
        }

        public bool IsSingle => true;

        string? IValueHolder<string>.Value { get; set; }

        #endregion
    }
}