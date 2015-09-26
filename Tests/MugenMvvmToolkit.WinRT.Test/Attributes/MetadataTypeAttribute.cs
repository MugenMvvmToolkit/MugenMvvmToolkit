// ReSharper disable once CheckNamespace

namespace System.ComponentModel.DataAnnotations
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class MetadataTypeAttribute : Attribute
    {
        #region Constructors

        public MetadataTypeAttribute(Type metadataClassType)
        {
            MetadataClassType = metadataClassType;
        }

        #endregion

        #region Properties

        public Type MetadataClassType { get; private set; }

        #endregion
    }
}