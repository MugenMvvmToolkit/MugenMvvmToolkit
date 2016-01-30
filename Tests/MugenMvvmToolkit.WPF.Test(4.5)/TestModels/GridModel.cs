using System;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.ViewModels;

namespace MugenMvvmToolkit.Test.TestModels
{
    public class GridModel : NotifyPropertyChangedBase
    {
        #region Fields

        private Guid _id;
        private string _name;

        #endregion

        #region Constructors

        public GridModel()
        {
            Id = Guid.NewGuid();
        }

        #endregion

        #region Properties

        public string Name
        {
            get { return _name; }
            set
            {
                if (value == _name) return;
                _name = value;
                OnPropertyChanged();
            }
        }

        public Guid Id
        {
            get { return _id; }
            set
            {
                if (value.Equals(_id)) return;
                _id = value;
                OnPropertyChanged();
            }
        }

        #endregion
    }
}
