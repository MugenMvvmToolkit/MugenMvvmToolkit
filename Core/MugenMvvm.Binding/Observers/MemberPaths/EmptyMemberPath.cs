using System.Collections.Generic;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Interfaces.Internal;

namespace MugenMvvm.Binding.Observers.MemberPaths
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

        public IReadOnlyList<string> Members => Default.EmptyArray<string>();

        string? IValueHolder<string>.Value { get; set; }

        #endregion
    }
}