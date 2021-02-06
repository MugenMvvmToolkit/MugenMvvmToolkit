using System;
using MugenMvvm.Android.Internal;
using MugenMvvm.Bindings.Parsing;

namespace MugenMvvm.Android.Bindings
{
    public sealed class NativeStringTokenParserContext : TokenParserContextBase<NativeStringAccessor>
    {
        public NativeStringTokenParserContext() : base(null)
        {
        }

        public override char TokenAt(int position) => Source.Span[position];

        public override string GetValue(int start, int end) => Source.Span.Slice(start, end - start).ToString();

        public override ReadOnlySpan<char> GetValueSpan(int start, int end) => Source.Span.Slice(start, end - start);

        protected override int GetLength() => Source.Length;
    }
}