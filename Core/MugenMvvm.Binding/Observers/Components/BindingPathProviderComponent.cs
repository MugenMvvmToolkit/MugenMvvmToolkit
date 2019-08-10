using System;
using System.Collections.Generic;
using MugenMvvm.Attributes;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Binding.Interfaces.Observers.Components;
using MugenMvvm.Collections;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Binding.Observers.Components
{
    public sealed class BindingPathProviderComponent : IBindingPathProviderComponent, IHasPriority
    {
        #region Fields

        private readonly CacheDictionary _cache;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public BindingPathProviderComponent()
        {
            _cache = new CacheDictionary();
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = 10;

        public bool UseCache { get; set; } = true;

        #endregion

        #region Implementation of interfaces

        public IBindingPath? TryGetBindingPath(object path, IReadOnlyMetadataContext? metadata)
        {
            if (!(path is string stringPath))
                return null;

            if (stringPath.Length == 0)
                return EmptyBindingPath.Instance;

            if (UseCache)
                return GetFromCache(stringPath);

            return GetObserver(stringPath);
        }

        #endregion

        #region Methods

        private IBindingPath GetFromCache(string path)
        {
            if (!_cache.TryGetValue(path, out var value))
            {
                value = GetObserver(path);
                _cache[path] = value;
            }

            return value;
        }

        private IBindingPath GetObserver(string path)
        {
            var hasBracket = path.IndexOf('[') >= 0;
            if (path.IndexOf('.') >= 0 || hasBracket)
                return new MultiBindingPath(path, hasBracket);
            return new SingleBindingPath(path);
        }

        #endregion

        #region Nested types

        private sealed class CacheDictionary : LightDictionaryBase<string, IBindingPath>
        {
            #region Constructors

            public CacheDictionary() : base(59)
            {
            }

            #endregion

            #region Methods

            protected override bool Equals(string x, string y)
            {
                return x.Equals(y);
            }

            protected override int GetHashCode(string key)
            {
                return key.GetHashCode();
            }

            #endregion
        }

        private sealed class SingleBindingPath : IBindingPath
        {
            #region Fields

            private string[]? _parts;

            #endregion

            #region Constructors

            public SingleBindingPath(string path)
            {
                Path = path;
            }

            #endregion

            #region Properties

            public string Path { get; }

            public string[] Parts
            {
                get
                {
                    if (_parts == null)
                        _parts = new[] { Path };
                    return _parts;
                }
            }

            public bool IsSingle => true;

            #endregion
        }

        private sealed class MultiBindingPath : IBindingPath
        {
            #region Constructors

            public MultiBindingPath(string path, bool hasIndexer)
            {
                Path = path;
                Parts = path.Split(BindingMugenExtensions.DotSeparator, StringSplitOptions.RemoveEmptyEntries);

                if (hasIndexer)
                {
                    var items = new List<string>();
                    for (var index = 0; index < Parts.Length; index++)
                    {
                        var s = Parts[index];
                        var start = s.IndexOf('[');
                        var end = s.IndexOf(']');
                        if (start <= 0 || end < 0)
                        {
                            items.Add(s.Trim());
                            continue;
                        }

                        var indexer = s.Substring(start, end - start + 1).Trim();
                        items.Add(s.Substring(0, start).Trim());
                        items.Add(indexer);
                    }


                    Parts = items.ToArray();
                }
            }

            #endregion

            #region Properties

            public string Path { get; }

            public string[] Parts { get; }

            public bool IsSingle => false;

            #endregion
        }

        #endregion
    }
}