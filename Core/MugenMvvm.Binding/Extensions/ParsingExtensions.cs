using MugenMvvm.Binding.Interfaces.Parsing;

// ReSharper disable once CheckNamespace
namespace MugenMvvm.Binding
{
    public static partial class BindingMugenExtensions
    {
        #region Methods

        public static int GetPosition(this IBindingParserContext context, int? position)
        {
            return position.GetValueOrDefault(context.Position);
        }

        public static char Current(this IBindingParserContext context, int? position)
        {
            return context.Source[context.GetPosition(position)];
        }

        public static bool IsEof(this IBindingParserContext context, int? position)
        {
            return context.GetPosition(position) == context.Source.Length;
        }

        public static void SkipWhitespaces(this IBindingParserContext context, out int position)
        {
            position = context.Position;
            while (char.IsWhiteSpace(context.Current(position)))
                ++position;
        }

        public static bool IsToken(this IBindingParserContext context, char value, int? position)
        {
            return context.Current(position) == value;
        }

        public static bool IsToken(this IBindingParserContext context, string value, int? position)
        {
            var p = context.GetPosition(position);
            var i = 0;
            while (i != value.Length)
            {
                if (context.Current(p + i) != value[i])
                    return false;
                ++i;
            }

            return true;
        }

        public static bool IsIdentifier(this IBindingParserContext context, int? position, out int endPosition)
        {
            endPosition = context.GetPosition(position);
            if (!IsValidIdentifierSymbol(true, context.Current(endPosition)))
                return false;

            do
            {
                ++endPosition;
            } while (IsValidIdentifierSymbol(false, context.Current(endPosition)));

            return true;
        }

        public static string GetToken(this IBindingParserContext context, int start, int end)
        {
            return context.Source.Substring(start, end - start);
        }

        private static bool IsValidIdentifierSymbol(bool firstSymbol, char symbol)
        {
            if (firstSymbol)
                return char.IsLetter(symbol) || symbol == '@' || symbol == '_';
            return char.IsLetterOrDigit(symbol) || symbol == '_';
        }

        #endregion
    }
}