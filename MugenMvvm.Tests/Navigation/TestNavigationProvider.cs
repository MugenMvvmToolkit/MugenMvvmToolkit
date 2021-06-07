using MugenMvvm.Interfaces.Navigation;

namespace MugenMvvm.Tests.Navigation
{
    public class TestNavigationProvider : INavigationProvider
    {
        public static readonly TestNavigationProvider Instance = new();

        public string Id { get; set; } = "test";
    }
}