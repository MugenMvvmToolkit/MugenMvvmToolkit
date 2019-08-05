namespace MugenMvvm.Binding.Interfaces.Parsing
{
    public interface IRelativeSourceExpressionNode : IExpressionNode
    {
        string Type { get; }

        string? ElementName { get; }

        string? Path { get; }

        uint Level { get; }
    }
}