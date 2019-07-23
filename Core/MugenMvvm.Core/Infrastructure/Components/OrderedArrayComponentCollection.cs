namespace MugenMvvm.Infrastructure.Components
{
    public sealed class OrderedArrayComponentCollection<T> : ArrayComponentCollectionBase<T> where T : class
    {
        #region Constructors

        public OrderedArrayComponentCollection(object owner)
        {
            Should.NotBeNull(owner, nameof(owner));
            Owner = owner;
        }

        #endregion

        #region Properties

        public override object Owner { get; }

        protected override bool IsOrdered => true;

        protected override bool IsSynchronized => true;

        #endregion
    }
}