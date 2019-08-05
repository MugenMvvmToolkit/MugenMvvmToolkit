namespace MugenMvvm.Binding.Interfaces.Parsing
{
    public interface IMemberExpressionNode : IExpressionNode
    {
        string Member { get; }

        IExpressionNode? Target { get; }
    }
}