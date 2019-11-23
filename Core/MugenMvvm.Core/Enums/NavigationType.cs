using System;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using MugenMvvm.Attributes;
using MugenMvvm.Constants;

namespace MugenMvvm.Enums
{
    [Serializable]
    [DataContract(Namespace = BuildConstant.DataContractNamespace)]
#pragma warning disable 660,661
    public class NavigationType : EnumBase<NavigationType, string>
#pragma warning restore 660,661
    {
        #region Fields

        public static readonly NavigationType Undefined = new NavigationType(nameof(Undefined));

        public static readonly NavigationType Tab = new NavigationType(nameof(Tab)) {IsNestedNavigation = true};
        public static readonly NavigationType Window = new NavigationType(nameof(Window)) {IsRootNavigation = true};
        public static readonly NavigationType Popup = new NavigationType(nameof(Popup)) {IsRootNavigation = true};
        public static readonly NavigationType Page = new NavigationType(nameof(Page)) {IsRootNavigation = true};

        public static readonly NavigationType System = new NavigationType(nameof(System)) {IsSystemNavigation = true};
        public static readonly NavigationType Alert = new NavigationType(nameof(Alert)) {IsSystemNavigation = true};
        public static readonly NavigationType Toast = new NavigationType(nameof(Toast)) {IsSystemNavigation = true};

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        protected NavigationType()
        {
        }

        public NavigationType(string value) : base(value)
        {
        }

        #endregion

        #region Properties

        [IgnoreDataMember]
        public bool IsUndefined => Undefined == this;

        [DataMember(Name = "r")]
        public bool IsRootNavigation { get; protected set; }

        [DataMember(Name = "n")]
        public bool IsNestedNavigation { get; protected set; }

        [DataMember(Name = "s")]
        public bool IsSystemNavigation { get; protected set; }

        #endregion

        #region Methods

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(NavigationType? left, NavigationType? right)
        {
            if (ReferenceEquals(left, right))
                return true;
            if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
                return false;
            return left.Value == right.Value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(NavigationType? left, NavigationType? right)
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