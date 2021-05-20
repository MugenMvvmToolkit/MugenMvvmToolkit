namespace MugenMvvm.Commands
{
    public sealed class RawCommandRequest
    {
        public static readonly RawCommandRequest Instance = new();

        private RawCommandRequest()
        {
        }
    }
}