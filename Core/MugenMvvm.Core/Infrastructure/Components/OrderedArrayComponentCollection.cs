using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Infrastructure.Components
{
    public class OrderedArrayComponentCollection<T> : ArrayComponentCollection<T> where T : class
    {
        #region Constructors

        public OrderedArrayComponentCollection(object owner) : base(owner)
        {
        }

        #endregion

        #region Methods

        protected override bool AddInternal(T component, IReadOnlyMetadataContext? metadata)
        {
            var array = new T[Items.Length + 1];
            var added = false;
            var priority = GetPriority(component);
            for (var i = 0; i < Items.Length; i++)
            {
                if (added)
                {
                    array[i + 1] = Items[i];
                    continue;
                }

                var oldItem = Items[i];
                var compareTo = priority.CompareTo(GetPriority(oldItem));
                if (compareTo > 0)
                {
                    array[i] = component;
                    added = true;
                    --i;
                }
                else
                    array[i] = oldItem;
            }

            if (!added)
                array[array.Length - 1] = component;
            Items = array;
            return true;
        }

        private int GetPriority(T component)
        {
            if (component is IComponent c)
                return c.GetPriority(Owner);
            return ((IHasPriority) component).Priority;
        }

        #endregion
    }
}