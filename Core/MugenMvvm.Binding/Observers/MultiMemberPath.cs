using System;
using System.Collections.Generic;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Interfaces.Internal;

namespace MugenMvvm.Binding.Observers
{
    public sealed class MultiMemberPath : IMemberPath, IValueHolder<string>
    {
        #region Constructors

        public MultiMemberPath(string path, bool hasIndexer)
        {
            Should.NotBeNull(path, nameof(path));
            Path = path;
            Members = path.Split(MugenBindingExtensions.DotSeparator, StringSplitOptions.RemoveEmptyEntries);

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

        public MultiMemberPath(string path) : this(path, path.IndexOf('[') >= 0)
        {
        }

        #endregion

        #region Properties

        public string Path { get; }

        public string[] Members { get; }

        public bool IsSingle => false;

        string? IValueHolder<string>.Value { get; set; }

        #endregion
    }
}