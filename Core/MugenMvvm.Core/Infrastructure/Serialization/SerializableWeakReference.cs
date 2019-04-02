using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using MugenMvvm.Attributes;
using MugenMvvm.Constants;

namespace MugenMvvm.Infrastructure.Serialization
{
    [Serializable]
    [DataContract(Namespace = BuildConstants.DataContractNamespace)]
    [Preserve(Conditional = true, AllMembers = true)]
    public sealed class SerializableWeakReference
    {
        #region Fields

        [IgnoreDataMember]
        [XmlIgnore]
        [NonSerialized]
        private WeakReference _weakReference;

        #endregion

        #region Constructors

        public SerializableWeakReference(WeakReference weakReference)
        {
            _weakReference = weakReference;
            TargetType = GetTarget<object>()?.GetType() ?? typeof(object);
        }

        public SerializableWeakReference(object? target)
            : this(MugenExtensions.GetWeakReference(target))
        {
        }

        #endregion

        #region Properties

        [DataMember(Name = "S")]
        internal object? SerializableTarget
        {
            get => _weakReference.Target;
            set => _weakReference = MugenExtensions.GetWeakReference(value);
        }

        [IgnoreDataMember]
        [XmlIgnore]
        [field: DataMember(Name = "T")]
        public Type TargetType { get; }

        #endregion

        #region Methods

        public T GetTarget<T>()
            where T : class?
        {
            return (T) _weakReference.Target;
        }

        #endregion
    }
}