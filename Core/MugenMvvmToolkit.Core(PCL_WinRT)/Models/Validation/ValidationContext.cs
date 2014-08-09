#region Copyright
// ****************************************************************************
// <copyright file="ValidationContext.cs">
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
using JetBrains.Annotations;
using MugenMvvmToolkit.Interfaces.Validation;

namespace MugenMvvmToolkit.Models.Validation
{
    /// <summary>
    ///     Describes the context in which a validation check is performed.
    /// </summary>
    public class ValidationContext : IValidationContext
    {
        #region Fields

        private readonly IDictionary<object, object> _items;
        private readonly object _objectInstance;
        private readonly Type _objectType;
        private readonly IServiceProvider _serviceProvider;
        private string _displayName;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="ValidationContext" /> class using the service provider and dictionary
        ///     of service consumers.
        /// </summary>
        /// <param name="instance">The object to validate. This parameter is required.</param>
        /// <param name="serviceProvider">
        ///     The object that implements the <see cref="T:System.IServiceProvider" /> interface. This
        ///     parameter is optional.
        /// </param>
        /// <param name="items">
        ///     A dictionary of key/value pairs to make available to the service consumers. This parameter is
        ///     optional.
        /// </param>
        public ValidationContext([NotNull] object instance, IServiceProvider serviceProvider = null,
            IDictionary<object, object> items = null)
        {
            Should.NotBeNull(instance, "instance");
            _objectInstance = instance;
            _objectType = instance.GetType();
            _serviceProvider = serviceProvider;
            _items = items;
        }

        #endregion

        #region Implementation of IValidationContext

        /// <summary>
        ///     Gets the object to validate.
        /// </summary>
        /// <returns>
        ///     The object to validate.
        /// </returns>
        public object ObjectInstance
        {
            get { return _objectInstance; }
        }

        /// <summary>
        ///     Gets the type of the object to validate.
        /// </summary>
        /// <returns>
        ///     The type of the object to validate.
        /// </returns>
        public Type ObjectType
        {
            get { return _objectType; }
        }

        /// <summary>
        ///     Gets or sets the name of the member to validate.
        /// </summary>
        /// <returns>
        ///     The name of the member to validate.
        /// </returns>
        public string DisplayName
        {
            get { return _displayName; }
            set
            {
                Should.PropertyBeNotNullOrEmpty(value, "DisplayName");
                _displayName = value;
            }
        }

        /// <summary>
        ///     Gets or sets the name of the member to validate.
        /// </summary>
        /// <returns>
        ///     The name of the member to validate.
        /// </returns>
        public string MemberName { get; set; }

        /// <summary>
        ///     Gets the dictionary of key/value pairs that is associated with this context.
        /// </summary>
        /// <returns>
        ///     The dictionary of the key/value pairs for this context.
        /// </returns>
        public IDictionary<object, object> Items
        {
            get { return _items; }
        }

        /// <summary>
        ///     Gets the validation services provider.
        /// </summary>
        /// <returns>
        ///     The validation services provider.
        /// </returns>
        public IServiceProvider ServiceProvider
        {
            get { return _serviceProvider; }
        }

        /// <summary>
        ///     Gets the service object of the specified type.
        /// </summary>
        /// <returns>
        ///     A service object of type <paramref name="serviceType" />.
        ///     -or-
        ///     null if there is no service object of type <paramref name="serviceType" />.
        /// </returns>
        /// <param name="serviceType">
        ///     An object that specifies the type of service object to get.
        /// </param>
        public object GetService(Type serviceType)
        {
            if (ServiceProvider == null)
                return null;
            return ServiceProvider.GetService(serviceType);
        }

        #endregion
    }
}