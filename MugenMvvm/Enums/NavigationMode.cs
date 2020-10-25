using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using MugenMvvm.Attributes;
using MugenMvvm.Constants;

namespace MugenMvvm.Enums
{
    [Serializable]
    [DataContract(Namespace = BuildConstant.DataContractNamespace)]
    public class NavigationMode : EnumBase<NavigationMode, string>
    {
        #region Fields

        public static readonly NavigationMode Undefined = new NavigationMode(nameof(Undefined));
        public static readonly NavigationMode New = new NavigationMode(nameof(New)) {IsNew = true};
        public static readonly NavigationMode Refresh = new NavigationMode(nameof(Refresh)) {IsRefresh = true};
        public static readonly NavigationMode Restore = new NavigationMode(nameof(Restore)) {IsRefresh = true, IsRestore = true};
        public static readonly NavigationMode Close = new NavigationMode(nameof(Close)) {IsClose = true};

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        protected NavigationMode()
        {
        }


        public NavigationMode(string value) : base(value, value)
        {
        }

        #endregion

        #region Properties

        [IgnoreDataMember]
        [XmlIgnore]
        public bool IsUndefined => this == Undefined;

        [DataMember(Name = "n")]
        public bool IsNew { get; protected set; }

        [DataMember(Name = "r")]
        public bool IsRefresh { get; protected set; }

        [DataMember(Name = "c")]
        public bool IsClose { get; protected set; }

        [DataMember(Name = "bg")]
        public bool IsBackground { get; protected set; }

        [DataMember(Name = "f")]
        public bool IsForeground { get; protected set; }

        [DataMember(Name = "rs")]
        public bool IsRestore { get; protected set; }

        #endregion
    }
}