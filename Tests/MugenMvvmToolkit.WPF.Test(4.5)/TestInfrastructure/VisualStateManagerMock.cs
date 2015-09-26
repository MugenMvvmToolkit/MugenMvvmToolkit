using System.Threading.Tasks;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;

namespace MugenMvvmToolkit.Test.TestInfrastructure
{
    public class VisualStateManagerMock : IVisualStateManager
    {
        #region Implementation of IVisualStateManager

        public Task<bool> GoToStateAsync(object view, string stateName, bool useTransitions, IDataContext context)
        {
            return Empty.FalseTask;
        }

        #endregion
    }
}
