using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using MugenMvvm.Attributes;
using MugenMvvm.Constants;

namespace MugenMvvm.Collections
{
    [Serializable]
    [DataContract(Namespace = BuildConstants.DataContractNamespace)]
    [Preserve(Conditional = true, AllMembers = true)]
    public class OrderedLightArrayList<T> : LightArrayList<T>
    {
        #region Constructors

        public OrderedLightArrayList(IComparer<T> comparer)
        {
            Comparer = comparer;
        }

        public OrderedLightArrayList(IComparer<T> comparer, uint capacity)
            : base(capacity)
        {
            Comparer = comparer;
        }

        public OrderedLightArrayList(IComparer<T> comparer, IEnumerable<T> collection) : base(collection)
        {
            Comparer = comparer;
        }

        #endregion

        #region Properties

        [field: DataMember(Name = "C")]
        public IComparer<T> Comparer { get; }

        #endregion

        #region Methods

        public void ReorderWithLock()
        {
            lock (this)
            {
                Reorder();
            }
        }

        public void Reorder()
        {
            Array.Sort(Items, 0, Size, Comparer);
        }

        protected override void AddInternal(T item)
        {
            InsertInternal(GetInsertIndex(item), item);
        }

        private int InsertInternal(int index, T item)
        {
            if (Size == Items.Length)
                EnsureCapacity((uint)Size + 1);
            if (index < Size) 
                Array.Copy(Items, index, Items, index + 1, Size - index);
            Items[index] = item;
            ++Size;
            return index;
        }

        private int GetInsertIndex(T item)
        {
            var num = Array.BinarySearch(Items, 0, Size, item, Comparer);
            if (num >= 0)
                return num;
            return ~num;
        }

        #endregion
    }
}