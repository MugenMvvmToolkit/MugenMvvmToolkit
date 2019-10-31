using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Metadata
{
    public sealed class AggregatedMetadataContext : IReadOnlyMetadataContext
    {
        #region Fields

        private readonly List<IReadOnlyMetadataContext> _list;

        #endregion

        #region Constructors

        public AggregatedMetadataContext(IReadOnlyMetadataContext? m1 = null, IReadOnlyMetadataContext? m2 = null, IReadOnlyMetadataContext? m3 = null)
        {
            _list = new List<IReadOnlyMetadataContext>();
            if (m3 != null)
                _list.Add(m3);
            if (m2 != null)
                _list.Add(m2);
            if (m1 != null)
                _list.Add(m1);
        }

        #endregion

        #region Properties

        public int Count => _list.Sum(context => context.Count);

        #endregion

        #region Implementation of interfaces

        public IEnumerator<MetadataContextValue> GetEnumerator()
        {
            return _list.SelectMany(context => context).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool TryGet<T>(IMetadataContextKey<T> contextKey, out T value, T defaultValue = default)
        {
            for (var i = 0; i < _list.Count; i++)
            {
                if (_list[i].TryGet(contextKey, out value, defaultValue))
                    return true;
            }

            value = contextKey.GetDefaultValue(this, defaultValue);
            return false;
        }

        public bool Contains(IMetadataContextKey contextKey)
        {
            for (var i = 0; i < _list.Count; i++)
            {
                if (_list[i].Contains(contextKey))
                    return true;
            }

            return false;
        }

        #endregion

        #region Methods

        public void Aggregate(IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(metadata, nameof(metadata));
            _list.Insert(0, metadata);
        }

        #endregion
    }
}