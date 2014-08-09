using MugenMvvmToolkit.Interfaces.Views;

namespace MugenMvvmToolkit.Test.TestModels
{
    public class ViewMock : IView
    {
        #region Implementation of IView

        /// <summary>
        ///     Gets or sets the data context of <see cref="IView" />.
        /// </summary>
        public object DataContext { get; set; }

        #endregion
    }
}