using MugenMvvm.Interfaces.Navigation;

namespace MugenMvvm.Navigation
{
    public sealed class NavigationProvider : INavigationProvider
    {
        #region Fields

        public static readonly NavigationProvider System = new(nameof(System));

        #endregion

        #region Constructors

        public NavigationProvider(string id)
        {
            Should.NotBeNull(id, nameof(id));
            Id = id;
        }

        #endregion

        #region Properties

        public string Id { get; }

        #endregion

        #region Methods

        public override string ToString() => Id;

        #endregion
    }
}