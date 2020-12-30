using System;
using System.Collections.Generic;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Bindings.Interfaces.Observation;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Internal;

namespace MugenMvvm.Bindings.Observation.Paths
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

            var size = 0;
#if SPAN_API
            var span = path.AsSpan();
            var enumerator = span.Split(MugenBindingExtensions.DotChar);
            foreach (var range in enumerator)
            {
                var s = span[range];
                size += s.IndexOf('[') <= 0 || s.IndexOf(']') < 0 ? 1 : 2;
            }

            var members = new string[size];
            size = 0;
            foreach (var range in enumerator)
            {
                var s = span[range];
                var start = s.IndexOf('[');
                var end = s.IndexOf(']');
                if (start <= 0 || end < 0)
                {
                    members[size++] = s.Trim().ToString();
                    continue;
                }

                members[size++] = s.Slice(0, start).Trim().ToString();
                members[size++] = s.Slice(start, end - start + 1).Trim().ToString();
            }
#else
            var split = path.Split(MugenBindingExtensions.DotSeparator, StringSplitOptions.RemoveEmptyEntries);
            for (var index = 0; index < split.Length; index++)
            {
                var s = split[index];
                size += s.IndexOf('[') <= 0 || s.IndexOf(']') < 0 ? 1 : 2;
            }

            var members = new string[size];
            size = 0;
            for (var index = 0; index < split.Length; index++)
            {
                var s = split[index];
                var start = s.IndexOf('[');
                var end = s.IndexOf(']');
                if (start <= 0 || end < 0)
                {
                    members[size++] = s.Trim();
                    continue;
                }

                members[size++] = s.Substring(0, start).Trim();
                members[size++] = s.Substring(start, end - start + 1).Trim();
            }
#endif
            Members = members;
        }

        #endregion

        #region Properties

        public string Path { get; }

        public IReadOnlyList<string> Members { get; }

        string? IValueHolder<string>.Value { get; set; }

        #endregion
    }
}