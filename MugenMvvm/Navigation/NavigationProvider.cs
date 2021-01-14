using MugenMvvm.Interfaces.Navigation;

namespace MugenMvvm.Navigation
{
    public sealed class NavigationProvider : INavigationProvider
    {
        public static readonly NavigationProvider System = new(nameof(System));

        public NavigationProvider(string id)
        {
            Should.NotBeNull(id, nameof(id));
            Id = id;
        }

        public string Id { get; }

        public override string ToString() => Id;
    }
}