using System;

namespace MugenMvvm.Bindings.Parsing
{
    public sealed class StringTokenParserContext : TokenParserContextBase<string>
    {
        public StringTokenParserContext() : base(null)
        {
        }

        public override char TokenAt(int position) => Source[position];

        public override string GetValue(int start, int end) => Source.Substring(start, end - start);

#if SPAN_API
        public override ReadOnlySpan<char> GetValueSpan(int start, int end) => Source.AsSpan(start, end - start);
#endif

        protected override int GetLength() => Source.Length;
    }
}