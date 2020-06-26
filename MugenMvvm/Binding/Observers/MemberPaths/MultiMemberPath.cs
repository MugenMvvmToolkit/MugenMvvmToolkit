using System;
using System.Collections.Generic;
using MugenMvvm.Binding.Extensions;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Internal;

namespace MugenMvvm.Binding.Observers.MemberPaths
{
    public sealed class MultiMemberPath : IMemberPath, IValueHolder<string>
    {
        #region Constructors

        public MultiMemberPath(string path) : this(path, path.IndexOf('[') >= 0)
        {
        }

        public MultiMemberPath(string path, bool hasIndexer)
        {
            Should.NotBeNull(path, nameof(path));
            Path = path;
            if (!hasIndexer)
            {
                Members = path.Split(MugenBindingExtensions.DotSeparator, StringSplitOptions.RemoveEmptyEntries);
                return;
            }

            var items = new List<string>();
#if SPAN_API
            var span = path.AsSpan();
            foreach (var range in path.AsSpan().Split(MugenBindingExtensions.DotChar))
            {
                var s = span[range];
                var start = s.IndexOf('[');
                var end = s.IndexOf(']');
                if (start <= 0 || end < 0)
                {
                    items.Add(s.Trim().ToString());
                    continue;
                }

                items.Add(s.Slice(0, start).Trim().ToString());
                items.Add(s.Slice(start, end - start + 1).Trim().ToString());
            }
#else
            Members = path.Split(MugenBindingExtensions.DotSeparator, StringSplitOptions.RemoveEmptyEntries);
            for (var index = 0; index < Members.Count; index++)
            {
                var s = Members[index];
                var start = s.IndexOf('[');
                var end = s.IndexOf(']');
                if (start <= 0 || end < 0)
                {
                    items.Add(s.Trim());
                    continue;
                }

                items.Add(s.Substring(0, start).Trim());
                items.Add(s.Substring(start, end - start + 1).Trim());
            }
#endif
            Members = items;
        }

        #endregion

        #region Properties

        public string Path { get; }

        public IReadOnlyList<string> Members { get; }

        string? IValueHolder<string>.Value { get; set; }

        #endregion
    }
}