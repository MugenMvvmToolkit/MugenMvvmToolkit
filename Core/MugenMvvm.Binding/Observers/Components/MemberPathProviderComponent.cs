using System;
using System.Collections.Generic;
using MugenMvvm.Attributes;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Binding.Interfaces.Observers.Components;
using MugenMvvm.Collections;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Binding.Observers.Components
{
    public sealed class MemberPathProviderComponent : IMemberPathProviderComponent, IMemberPathProviderComponentInternal<string>, IHasPriority
    {
        #region Fields

        private readonly CacheDictionary _cache;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public MemberPathProviderComponent()
        {
            _cache = new CacheDictionary();
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = 10;

        public bool UseCache { get; set; } = true;

        #endregion

        #region Implementation of interfaces

        public IMemberPath? TryGetMemberPath<TPath>(in TPath path, IReadOnlyMetadataContext? metadata)
        {
            if (this is IMemberPathProviderComponentInternal<TPath> provider)
                return provider.TryGetMemberPath(path, metadata);
            return null;
        }

        public IMemberPath? TryGetMemberPath(in string path, IReadOnlyMetadataContext? metadata)
        {
            if (path.Length == 0)
                return EmptyMemberPath.Instance;

            if (UseCache)
                return GetFromCache(path);

            return GetObserver(path);
        }

        #endregion

        #region Methods

        private IMemberPath GetFromCache(string path)
        {
            if (!_cache.TryGetValue(path, out var value))
            {
                value = GetObserver(path);
                _cache[path] = value;
            }

            return value;
        }

        private static IMemberPath GetObserver(string path)
        {
            var hasBracket = path.IndexOf('[') >= 0;
            if (path.IndexOf('.') >= 0 || hasBracket)
                return new MultiMemberPath(path, hasBracket);
            return new SingleMemberPath(path);
        }

        #endregion

        #region Nested types

        private sealed class CacheDictionary : LightDictionary<string, IMemberPath>
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

        private sealed class SingleMemberPath : IMemberPath
        {
            #region Fields

            private string[]? _members;

            #endregion

            #region Constructors

            public SingleMemberPath(string path)
            {
                Path = path;
            }

            #endregion

            #region Properties

            public string Path { get; }

            public string[] Members
            {
                get
                {
                    if (_members == null)
                        _members = new[] { Path };
                    return _members;
                }
            }

            public bool IsSingle => true;

            #endregion
        }

        private sealed class MultiMemberPath : IMemberPath
        {
            #region Constructors

            public MultiMemberPath(string path, bool hasIndexer)
            {
                Path = path;
                Members = path.Split(BindingMugenExtensions.DotSeparator, StringSplitOptions.RemoveEmptyEntries);

                if (hasIndexer)
                {
                    var items = new List<string>();
                    for (var index = 0; index < Members.Length; index++)
                    {
                        var s = Members[index];
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


                    Members = items.ToArray();
                }
            }

            #endregion

            #region Properties

            public string Path { get; }

            public string[] Members { get; }

            public bool IsSingle => false;

            #endregion
        }

        #endregion
    }
}