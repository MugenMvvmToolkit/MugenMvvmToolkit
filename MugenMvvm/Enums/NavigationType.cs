using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using MugenMvvm.Attributes;
using MugenMvvm.Constants;

namespace MugenMvvm.Enums
{
    [Serializable]
    [DataContract(Namespace = InternalConstant.DataContractNamespace)]
    public class NavigationType : EnumBase<NavigationType, string>
    {
        public static readonly NavigationType Undefined = new(nameof(Undefined));

        public static readonly NavigationType Page = new(nameof(Page)) {IsRootNavigation = true};
        public static readonly NavigationType Popup = new(nameof(Popup)) {IsRootNavigation = true};
        public static readonly NavigationType Window = new(nameof(Window)) {IsRootNavigation = true};

        public static readonly NavigationType Background = new(nameof(Background));
        public static readonly NavigationType Alert = new(nameof(Alert));
        public static readonly NavigationType Toast = new(nameof(Toast));

        public NavigationType(string value, string? name = null) : base(value, name)
        {
        }

        [Preserve(Conditional = true)]
        protected NavigationType()
        {
        }

        [IgnoreDataMember]
        [XmlIgnore]
        public bool IsUndefined => Undefined == this;

        [DataMember(Name = "r")]
        public bool IsRootNavigation { get; set; }
    }
}