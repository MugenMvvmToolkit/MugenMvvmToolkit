using System.Collections;
using System.Collections.Generic;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Interfaces.Internal;

namespace MugenMvvm.Binding.Observers
{
    public sealed class SingleMemberPath : IMemberPath, IValueHolder<string>, IReadOnlyList<string>
    {
        #region Constructors

        public SingleMemberPath(string path)
        {
            Should.NotBeNull(path, nameof(path));
            Path = path;
        }

        #endregion

        #region Properties

        public string Path { get; }

        public IReadOnlyList<string> Members => this;

        string? IValueHolder<string>.Value { get; set; }

        int IReadOnlyCollection<string>.Count => 1;

        string IReadOnlyList<string>.this[int index]
        {
            get
            {
                if (index != 0)
                    ExceptionManager.ThrowIndexOutOfRangeCollection(nameof(index));
                return Path;
            }
        }

        #endregion

        #region Implementation of interfaces

        IEnumerator<string> IEnumerable<string>.GetEnumerator()
        {
            return Default.SingleValueEnumerator(Path);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Default.SingleValueEnumerator(Path);
        }

        #endregion
    }
}