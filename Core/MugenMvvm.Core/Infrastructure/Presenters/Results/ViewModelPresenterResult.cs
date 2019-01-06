using System.Threading.Tasks;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Presenters.Results;
using MugenMvvm.Models;

namespace MugenMvvm.Infrastructure.Presenters.Results
{
    public class ViewModelPresenterResult : IViewModelPresenterResult
    {
        #region Constructors

        public ViewModelPresenterResult(IReadOnlyMetadataContext metadata, NavigationType navigationType, Task openNavigationTask, Task closeNavigationTask)
        {
            Metadata = metadata;
            NavigationType = navigationType;
            OpenNavigationTask = openNavigationTask;
            CloseNavigationTask = closeNavigationTask;
        }

        #endregion

        #region Properties

        public IReadOnlyMetadataContext Metadata { get; }

        public NavigationType NavigationType { get; }

        public Task OpenNavigationTask { get; }

        public Task CloseNavigationTask { get; }

        #endregion
    }
}