namespace MugenMvvm.Binding.Constants
{
    public static class ExpressionCompilerComponentPriority
    {
        #region Fields

        public const int Binary = 1000;
        public const int Condition = 990;
        public const int Unary = 980;
        public const int Lambda = 970;
        public const int NullConditionalMember = 960;
        public const int Member = 950;
        public const int Constant = 940;

        public const int LinqCompiler = 0;

        #endregion
    }
}