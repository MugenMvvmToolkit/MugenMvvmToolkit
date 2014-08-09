#region Copyright
// ****************************************************************************
// <copyright file="BindingBuilder.cs">
// Copyright © Vyacheslav Volkov 2012-2014
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
using MugenMvvmToolkit.Utils;

namespace MugenMvvmToolkit.Binding.Builders
{
    /// <summary>
    ///     Represents the data binding builder.
    /// </summary>
    public sealed class BindingBuilder : IBindingBuilder
    {
        #region Fields

        private readonly IDataContext _internalContext;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="BindingBuilder" /> class.
        /// </summary>
        public BindingBuilder()
        {
            _internalContext = new DataContext();
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="BindingBuilder" /> class.
        /// </summary>
        public BindingBuilder(IDataContext internalContext)
        {
            _internalContext = internalContext;
        }

        #endregion

        #region Implementation of IDataContext

        /// <summary>
        ///     Gets the number of elements contained in the <see cref="IDataContext" />.
        /// </summary>
        /// <returns>
        ///     The number of elements contained in the <see cref="IDataContext" />.
        /// </returns>
        public int Count
        {
            get { return _internalContext.Count; }
        }

        /// <summary>
        ///     Gets a value indicating whether the <see cref="IDataContext" /> is read-only.
        /// </summary>
        /// <returns>
        ///     true if the <see cref="IDataContext" /> is read-only; otherwise, false.
        /// </returns>
        public bool IsReadOnly
        {
            get { return _internalContext.IsReadOnly; }
        }

        /// <summary>
        ///     Adds the data constant value.
        /// </summary>
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

        /// <summary>
        ///     Adds the data constant value or update existing.
        /// </summary>
        public void AddOrUpdate<T>(DataConstant<T> dataConstant, T value)
        {
            _internalContext.AddOrUpdate(dataConstant, value);
        }

        /// <summary>
        ///     Gets the data using the specified data constant.
        /// </summary>
        public T GetData<T>(DataConstant<T> dataConstant)
        {
            return _internalContext.GetData(dataConstant);
        }

        /// <summary>
        ///     Gets the data using the specified data constant.
        /// </summary>
        public bool TryGetData<T>(DataConstant<T> dataConstant, out T data)
        {
            return _internalContext.TryGetData(dataConstant, out data);
        }

        /// <summary>
        ///     Determines whether the <see cref="IDataContext" /> contains the specified key.
        /// </summary>
        public bool Contains(DataConstant dataConstant)
        {
            return _internalContext.Contains(dataConstant);
        }

        /// <summary>
        ///     Removes the data constant value.
        /// </summary>
        public bool Remove(DataConstant dataConstant)
        {
            return _internalContext.Remove(dataConstant);
        }

        /// <summary>
        ///     Updates the current context.
        /// </summary>
        public void Update(IDataContext context)
        {
            _internalContext.Update(context);
        }

        /// <summary>
        /// Removes all values from current context.
        /// </summary>
        public void Clear()
        {
            _internalContext.Clear();
        }

        /// <summary>
        ///     Creates an instance of <see cref="IList{DataConstantValue}" /> from current context.
        /// </summary>
        public IList<DataConstantValue> ToList()
        {
            return _internalContext.ToList();
        }

        #endregion

        #region Implementation of IBindingBuilder

        /// <summary>
        ///     Builds an instance of <see cref="IDataBinding" />.
        /// </summary>
        /// <returns>
        ///     The builded <see cref="IDataBinding" />.
        /// </returns>
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