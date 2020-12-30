using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using MugenMvvm.Attributes;
using MugenMvvm.Constants;

namespace MugenMvvm.Enums
{
    [Serializable]
    [DataContract(Namespace = BuildConstant.DataContractNamespace)]
    public class NavigationType : EnumBase<NavigationType, string>
    {
        #region Fields

        public static readonly NavigationType Undefined = new(nameof(Undefined));

        public static readonly NavigationType Tab = new(nameof(Tab)) {IsNestedNavigation = true};
        public static readonly NavigationType Window = new(nameof(Window)) {IsRootNavigation = true};
        public static readonly NavigationType Popup = new(nameof(Popup)) {IsRootNavigation = true};
        public static readonly NavigationType Page = new(nameof(Page)) {IsRootNavigation = true};

        public static readonly NavigationType Background = new(nameof(Background));
        public static readonly NavigationType Alert = new(nameof(Alert));
        public static readonly NavigationType Toast = new(nameof(Toast));

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
        [XmlIgnore]
        public bool IsUndefined => Undefined == this;

        [DataMember(Name = "r")]
        public bool IsRootNavigation { get; protected set; }

        [DataMember(Name = "n")]
        public bool IsNestedNavigation { get; protected set; }

        #endregion
    }
}