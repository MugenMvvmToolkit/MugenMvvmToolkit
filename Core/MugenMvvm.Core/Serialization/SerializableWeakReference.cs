using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using MugenMvvm.Attributes;
using MugenMvvm.Constants;
using MugenMvvm.Interfaces.Internal;

namespace MugenMvvm.Serialization
{
    [Serializable]
    [DataContract(Namespace = BuildConstants.DataContractNamespace)]
    [Preserve(Conditional = true, AllMembers = true)]
    public sealed class SerializableWeakReference//todo check
    {
        #region Constructors

        public SerializableWeakReference(IWeakReference weakReference)
        {
            Target = weakReference;
        }

        public SerializableWeakReference(object? target)
            : this(target.ToWeakReference())
        {
        }

        #endregion

        #region Properties

        [DataMember(Name = "S")]
        internal object? SerializableTarget
        {
            get => Target.Target;
            set => Target = value.ToWeakReference();
        }

        [field: IgnoreDataMember]
        [field: XmlIgnore]
        [field: NonSerialized]
        public IWeakReference Target { get; private set; }

        #endregion
    }
}