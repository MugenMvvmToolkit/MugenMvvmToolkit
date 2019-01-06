using System.Threading.Tasks;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Presenters.Results;

namespace MugenMvvm.Infrastructure.Presenters.Results
{
    public class ClosingViewModelPresenterResult : IClosingViewModelPresenterResult
    {
        #region Fields

        public static readonly IClosingViewModelPresenterResult FalseResult;

        #endregion

        #region Constructors

        static ClosingViewModelPresenterResult()
        {
            FalseResult = new ClosingViewModelPresenterResult(Default.MetadataContext, Default.FalseTask);
        }

        public ClosingViewModelPresenterResult(IReadOnlyMetadataContext metadata, Task<bool> closingTask)
        {
            Metadata = metadata;
            ClosingTask = closingTask;
        }

        #endregion

        #region Properties

        public IReadOnlyMetadataContext Metadata { get; }

        public Task<bool> ClosingTask { get; }

        #endregion
    }
}