#region Copyright

// ****************************************************************************
// <copyright file="BindingBuilder.cs">
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
using MugenMvvmToolkit.Binding.DataConstants;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Binding.Builders
{
    public sealed class BindingBuilder : IBindingBuilder
    {
        #region Fields

        private readonly IDataContext _internalContext;

        #endregion

        #region Constructors

        public BindingBuilder()
        {
            _internalContext = new DataContext();
        }

        public BindingBuilder(IDataContext internalContext)
        {
            _internalContext = internalContext;
        }

        #endregion

        #region Implementation of IDataContext

        public int Count => _internalContext.Count;

        public bool IsReadOnly => _internalContext.IsReadOnly;

        public void Add<T>(DataConstant<T> data, T value)
        {
            try
            {
                _internalContext.Add(data, value);
            }
            catch (ArgumentException)
            {
                throw BindingExceptionManager.DuplicateDataConstant(data);
            }
        }

        public void AddOrUpdate<T>(DataConstant<T> dataConstant, T value)
        {
            _internalContext.AddOrUpdate(dataConstant, value);
        }

        public T GetData<T>(DataConstant<T> dataConstant)
        {
            return _internalContext.GetData(dataConstant);
        }

        public bool TryGetData<T>(DataConstant<T> dataConstant, out T data)
        {
            return _internalContext.TryGetData(dataConstant, out data);
        }

        public bool Contains(DataConstant dataConstant)
        {
            return _internalContext.Contains(dataConstant);
        }

        public bool Remove(DataConstant dataConstant)
        {
            return _internalContext.Remove(dataConstant);
        }

        public void Merge(IDataContext context)
        {
            _internalContext.Merge(context);
        }

        public void Clear()
        {
            _internalContext.Clear();
        }

        public IList<DataConstantValue> ToList()
        {
            return _internalContext.ToList();
        }

        #endregion

        #region Implementation of IBindingBuilder

        public IDataBinding Build()
        {
            Func<IDataContext, IDataBinding> del;
            if (!_internalContext.TryGetData(BindingBuilderConstants.BuildDelegate, out del))
                throw ExceptionManager.DataConstantNotFound(BindingBuilderConstants.BuildDelegate);
            return del(this);
        }

        #endregion
    }
}
