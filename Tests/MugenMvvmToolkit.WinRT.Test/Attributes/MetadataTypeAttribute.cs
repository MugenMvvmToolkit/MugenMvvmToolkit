// ReSharper disable once CheckNamespace

namespace System.ComponentModel.DataAnnotations
{
    /// <summary>
    ///     Specifies the metadata class to associate with a data model class.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class MetadataTypeAttribute : Attribute
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="T:System.ComponentModel.DataAnnotations.MetadataTypeAttribute" />
        ///     class.
        /// </summary>
        /// <param name="metadataClassType">The metadata class to reference.</param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="metadataClassType" /> is null. </exception>
        public MetadataTypeAttribute(Type metadataClassType)
        {
            MetadataClassType = metadataClassType;
        }

        /// <summary>
        ///     Gets the metadata class that is associated with a data-model partial class.
        /// </summary>
        /// <returns>
        ///     The type value that represents the metadata class.
        /// </returns>
        public Type MetadataClassType { get; private set; }
    }
}