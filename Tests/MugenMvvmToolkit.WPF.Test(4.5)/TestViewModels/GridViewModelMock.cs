using System.Collections.Generic;
using MugenMvvmToolkit.Test.TestModels;
using MugenMvvmToolkit.ViewModels;

namespace MugenMvvmToolkit.Test.TestViewModels
{
    public class GridViewModelMock : GridViewModel<GridModel>
    {
        #region Properties

        public GridModel SelectedItemChanging { get; set; }

        public GridModel SelectedItemChangingResult { get; set; }

        public GridModel SelectedItemChangedOld { get; set; }

        public GridModel SelectedItemChangedNew { get; set; }

        public IEnumerable<GridModel> ItemsSourceChangingValue { get; set; }

        public IEnumerable<GridModel> ItemsSourceChangingResult { get; set; }

        public IEnumerable<GridModel> ItemsSourceChangedValue { get; set; }

        #endregion

        #region Overrides of GridViewModelBase<GridModel>

        /// <summary>
        ///     Occurs when the <c>SelectedItem</c> property changing.
        /// </summary>
        /// <param name="newValue">The new value.</param>
        /// <returns>The value to set as selected item.</returns>
        protected override GridModel OnSelectedItemChanging(GridModel newValue)
        {
            SelectedItemChanging = newValue;
            return SelectedItemChangingResult;
        }

        /// <summary>
        ///     Occurs when the <c>SelectedItem</c> property changed.
        /// </summary>
        /// <param name="oldValue">The old value.</param>
        /// <param name="newValue">The new value.</param>
        protected override void OnSelectedItemChanged(GridModel oldValue, GridModel newValue)
        {
            SelectedItemChangedOld = oldValue;
            SelectedItemChangedNew = newValue;
        }

        /// <summary>
        ///     Occurs when the <c>ItemsSource</c> property changing.
        /// </summary>
        /// <param name="data">The new item source data.</param>
        /// <returns>
        ///     An instance of <see cref="IEnumerable{T}" />.
        /// </returns>
        protected override IEnumerable<GridModel> OnItemsSourceChanging(IEnumerable<GridModel> data)
        {
            ItemsSourceChangingValue = data;
            return ItemsSourceChangingResult;
        }

        /// <summary>
        ///     Occurs when the <c>ItemsSource</c> property changed.
        /// </summary>
        /// <param name="data">The new item source data.</param>
        protected override void OnItemsSourceChanged(IEnumerable<GridModel> data)
        {
            ItemsSourceChangedValue = data;
        }

        #endregion
    }
}