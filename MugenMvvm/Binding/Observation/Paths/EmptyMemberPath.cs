using System.Collections.Generic;
using MugenMvvm.Binding.Interfaces.Observation;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Internal;

namespace MugenMvvm.Binding.Observation.Paths
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

        public IReadOnlyList<string> Members => Default.Array<string>();

        string? IValueHolder<string>.Value { get; set; }

        #endregion
    }
}