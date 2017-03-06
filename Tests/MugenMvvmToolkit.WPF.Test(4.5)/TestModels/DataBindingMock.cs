#region Copyright

// ****************************************************************************
// <copyright file="DataBindingMock.cs">
// Copyright (c) 2012-2017 Vyacheslav Volkov
// </copyright>
// ****************************************************************************
// <author>Vyacheslav Volkov</author>
// <email>vvs0205@outlook.com</email>
// <project>MugenMvvmToolkit</project>
// <web>https://github.com/MugenMvvmToolkit/MugenMvvmToolkit</web>
// <license>
// See license.txt in this solution or http://opensource.org/licenses/MS-PL
// </license>
// ****************************************************************************

#endregion

using System;
using System.Collections.Generic;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Binding.Interfaces.Accessors;
using MugenMvvmToolkit.Binding.Models.EventArg;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Test.TestModels
{
    public class DataBindingMock : DisposableObject, IDataBinding
    {
        #region Properties

        public Action UpdateSource { get; set; }

        public Action UpdateTarget { get; set; }

        public Func<bool> Validate { get; set; }

        public Func<IDataContext> GetContext { get; set; }

        #endregion

        #region Implementation of IBinding

        public IDataContext Context => GetContext();

        public ISingleBindingSourceAccessor TargetAccessor { get; set; }

        public IBindingSourceAccessor SourceAccessor { get; set; }

        public ICollection<IBindingBehavior> Behaviors { get; set; }

        bool IDataBinding.UpdateSource()
        {
            UpdateSource();
            return true;
        }

        bool IDataBinding.UpdateTarget()
        {
            UpdateTarget();
            return true;
        }

        bool IDataBinding.Validate()
        {
            return Validate();
        }

        public event EventHandler<IDataBinding, BindingEventArgs> BindingUpdated;

        #endregion

        #region Methods

        public void RaiseBindingUpdated(BindingEventArgs e)
        {
            var handler = BindingUpdated;
            if (handler != null) handler(this, e);
        }

        #endregion
    }
}
