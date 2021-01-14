using System;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Bindings.Interfaces.Observation;
using MugenMvvm.Collections;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Internal;

namespace MugenMvvm.Bindings.Observation
{
    public sealed class MemberPath : IMemberPath, IValueHolder<string>
    {
        public static readonly IMemberPath Empty = new MemberPath();

        private readonly object? _members;

        private MemberPath()
        {
            Path = "";
        }

        private MemberPath(string path, bool hasIndexer)
        {
            Path = path;
            if (!hasIndexer)
            {
                _members = path.Split(BindingMugenExtensions.DotSeparator, StringSplitOptions.RemoveEmptyEntries);
                return;
            }

            var size = 0;
#if SPAN_API
            var span = path.AsSpan();
            var enumerator = span.Split(BindingMugenExtensions.DotChar);
            foreach (var range in enumerator)
            {
                var s = span[range];
                size += s.IndexOf('[') <= 0 || s.IndexOf(']') < 0 ? 1 : 2;
            }

            var members = ItemOrArray.Get<string>(size);
            size = 0;
            foreach (var range in enumerator)
            {
                var s = span[range];
                var start = s.IndexOf('[');
                var end = s.IndexOf(']');
                if (start <= 0 || end < 0)
                {
                    members.SetAt(size++, s.Trim().ToString());
                    continue;
                }

                members.SetAt(size++, s.Slice(0, start).Trim().ToString());
                ;
                members.SetAt(size++, s.Slice(start, end - start + 1).Trim().ToString());
            }
#else
            var split = path.Split(BindingMugenExtensions.DotSeparator, StringSplitOptions.RemoveEmptyEntries);
            for (var index = 0; index < split.Length; index++)
            {
                var s = split[index];
                size += s.IndexOf('[') <= 0 || s.IndexOf(']') < 0 ? 1 : 2;
            }

            var members = ItemOrArray.Get<string>(size);
            size = 0;
            for (var index = 0; index < split.Length; index++)
            {
                var s = split[index];
                var start = s.IndexOf('[');
                var end = s.IndexOf(']');
                if (start <= 0 || end < 0)
                {
                    members.SetAt(size++, s.Trim());
                    continue;
                }

                members.SetAt(size++, s.Substring(0, start).Trim());
                members.SetAt(size++, s.Substring(start, end - start + 1).Trim());
            }
#endif
            _members = members.GetRawValue();
        }

        public string Path { get; }

        public ItemOrIReadOnlyList<string> Members => ItemOrIReadOnlyList.FromRawValue<string>(_members);

        string? IValueHolder<string>.Value { get; set; }

        public static IMemberPath Get(string path)
        {
            Should.NotBeNull(path, nameof(path));
            if (string.IsNullOrWhiteSpace(path))
                return Empty;
            var hasBracket = path.IndexOf('[') >= 0;
            if (path.IndexOf('.') >= 0 || hasBracket)
                return new MemberPath(path, hasBracket);
            return new SingleMemberPath(path);
        }

        private sealed class SingleMemberPath : IMemberPath
        {
            public SingleMemberPath(string path)
            {
                Path = path;
            }

            public string Path { get; }

            public ItemOrIReadOnlyList<string> Members => Path;
        }
    }
}