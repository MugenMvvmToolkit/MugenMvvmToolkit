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

        protected override GridModel OnSelectedItemChanging(GridModel newValue)
        {
            SelectedItemChanging = newValue;
            return SelectedItemChangingResult;
        }

        protected override void OnSelectedItemChanged(GridModel oldValue, GridModel newValue)
        {
            SelectedItemChangedOld = oldValue;
            SelectedItemChangedNew = newValue;
        }

        protected override IEnumerable<GridModel> OnItemsSourceChanging(IEnumerable<GridModel> data)
        {
            ItemsSourceChangingValue = data;
            return ItemsSourceChangingResult;
        }

        protected override void OnItemsSourceChanged(IEnumerable<GridModel> data)
        {
            ItemsSourceChangedValue = data;
        }

        #endregion
    }
}
