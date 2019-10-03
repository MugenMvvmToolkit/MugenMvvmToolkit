using System;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using MugenMvvm.Attributes;
using MugenMvvm.Constants;

namespace MugenMvvm.Enums
{
    [Serializable]
    [DataContract(Namespace = BuildConstants.DataContractNamespace)]
    public class ApplicationState : EnumBase<ApplicationState, string>
    {
        #region Fields

        public static readonly ApplicationState Active = new ApplicationState(nameof(Active));
        public static readonly ApplicationState Background = new ApplicationState(nameof(Background));

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        protected ApplicationState()
        {
        }

        public ApplicationState(string value) : base(value)
        {
        }

        #endregion

        #region Methods

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(ApplicationState? left, ApplicationState? right)
        {
            if (ReferenceEquals(left, right))
                return true;
            if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
                return false;
            return left.Value == right.Value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(ApplicationState? left, ApplicationState? right)
        {
            return !(left == right);
        }

        protected override bool Equals(string value)
        {
            return Value.Equals(value);
        }

        #endregion
    }
}