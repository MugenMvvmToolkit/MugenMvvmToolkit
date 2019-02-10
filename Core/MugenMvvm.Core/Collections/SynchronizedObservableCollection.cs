using System;
using System.Collections.Generic;
using MugenMvvm.Delegates;

namespace MugenMvvm.Collections
{
//    [Serializable]
//    public class SynchronizedObservableCollection<T> : SynchronizedObservableCollectionBase<T, List<T>>
//    {
//        #region Fields
//
//        [NonSerialized]
//        private FilterDelegate<T>? _filter;
//        private List<KeyValuePair<int, T>>? _filterItems;
//
//        #endregion
//
//        #region Constructors
//
//        public SynchronizedObservableCollection(IEnumerable<T> items)
//            : base(new List<T>(items))
//        {
//        }dfgfd
//
//        public SynchronizedObservableCollection()
//            : base(new List<T>())
//        { 
//        }
//
//        #endregion
//
//        #region Properties
//
//        public FilterDelegate<T>? Filter
//        {
//            get => _filter;
//            set
//            {
//                lock (Locker)
//                {
//                    if (Equals(Filter, value))
//                        return;
//                    _filter = value;
//                    UpdateFilterInternal(value);
//                }
//            }
//        }
//
//        #endregion
//
//        #region Methods
//
//        protected virtual void UpdateFilterInternal(FilterDelegate<T> filter)
//        {
//            using (BeginBatchUpdate())
//            {
//                if (filter == null)
//                {
//
//                }
//            }
//        }
//
//        protected override void ClearInternal()
//        {
//            base.ClearInternal();
//        }
//
//        protected override void CopyToInternal(Array array, int index)
//        {
//            base.CopyToInternal(array, index);
//        }
//
//        protected override int GetCountInternal()
//        {
//            return base.GetCountInternal();
//        }
//
//        protected override T GetInternal(int index)
//        {
//            return base.GetInternal(index);
//        }
//
//        protected override int InsertInternal(int index, T item, bool isAdd)
//        {
//            return base.InsertInternal(index, item, isAdd);
//        }
//
//        protected override void MoveInternal(int oldIndex, int newIndex)
//        {
//            base.MoveInternal(oldIndex, newIndex);
//        }
//
//        protected override void RemoveInternal(int index)
//        {
//            base.RemoveInternal(index);
//        }
//
//        protected override void SetInternal(int index, T item)
//        {
//            base.SetInternal(index, item);
//        }
//
//        private bool IsSatisfy(T item)
//        {
//            return _filter?.Invoke(item) ?? true;
//        }
//
//        #endregion
//    }
}