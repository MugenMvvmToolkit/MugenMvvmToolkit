using System;
using System.Runtime.InteropServices;

namespace MugenMvvm.ViewModels
{
    [StructLayout(LayoutKind.Auto)]
    public readonly struct ViewModelProviderRequest
    {
        #region Fields

        public readonly Guid? Id;
        public readonly Type? Type;

        #endregion

        #region Constructors

        public ViewModelProviderRequest(Type? type, Guid? id = null)
        {
            Type = type;
            Id = id;
        }

        #endregion

        #region Properties

        public bool IsEmpty => Id == null && Type == null;

        #endregion
    }
}