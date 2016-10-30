using System;

namespace MugenMvvmToolkit.Attributes
{
    [AttributeUsage(
         AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Constructor | AttributeTargets.Method |
         AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Event | AttributeTargets.Interface | AttributeTargets.Delegate)]
    internal sealed class PreserveAttribute : Attribute
    {
        #region Fields

        public bool AllMembers;

        public bool Conditional;

        #endregion
    }
}