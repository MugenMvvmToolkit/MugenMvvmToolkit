#region Copyright

// ****************************************************************************
// <copyright file="ValidatorContext.cs">
// Copyright (c) 2012-2015 Vyacheslav Volkov
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
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.Validation;

namespace MugenMvvmToolkit.Models.Validation
{
    /// <summary>
    ///     Represents the validation context.
    /// </summary>
    public class ValidatorContext : IValidatorContext
    {
        #region Fields

        private readonly ICollection<string> _ignoreProperties;
        private readonly object _instance;
        private readonly IDictionary<string, ICollection<string>> _propertyMappings;
        private readonly IServiceProvider _serviceProvider;
        private readonly IDataContext _validationMetadata;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="ValidatorContext" /> class.
        /// </summary>
        public ValidatorContext(object instanceToValidate, IServiceProvider serviceProvider = null)
            : this(instanceToValidate, null, null, null, serviceProvider)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ValidatorContext" /> class.
        /// </summary>
        public ValidatorContext(object instanceToValidate, IDictionary<string, ICollection<string>> propertyMappings,
            ICollection<string> ignoredProperties, IDataContext validationMetadata = null,
            IServiceProvider serviceProvider = null)
        {
            Should.NotBeNull(instanceToValidate, "instanceToValidate");
            _instance = instanceToValidate;
            _propertyMappings = propertyMappings ?? new Dictionary<string, ICollection<string>>();
            _ignoreProperties = ignoredProperties ?? new HashSet<string>();
            _validationMetadata = validationMetadata ?? new DataContext();
            _serviceProvider = serviceProvider;
        }

        #endregion

        #region Implementation of IValidatorContext

        /// <summary>
        ///     Gets the object to validate.
        /// </summary>
        public object Instance
        {
            get { return _instance; }
        }

        /// <summary>
        ///     Gets or sets the validation metadata.
        /// </summary>
        public IDataContext ValidationMetadata
        {
            get { return _validationMetadata; }
        }

        /// <summary>
        ///     Gets the error properties mapping.
        /// </summary>
        public IDictionary<string, ICollection<string>> PropertyMappings
        {
            get { return _propertyMappings; }
        }

        /// <summary>
        ///     Gets the list of properties that will not be validated.
        /// </summary>
        public ICollection<string> IgnoreProperties
        {
            get { return _ignoreProperties; }
        }

        /// <summary>
        ///     Gets the service provider.
        /// </summary>
        public IServiceProvider ServiceProvider
        {
            get { return _serviceProvider; }
        }

        #endregion

        #region Implementation of IServiceProvider

        /// <summary>
        ///     Gets the service object of the specified type.
        /// </summary>
        /// <returns>
        ///     A service object of type <paramref name="serviceType" />.-or- null if there is no service object of type
        ///     <paramref name="serviceType" />.
        /// </returns>
        /// <param name="serviceType">An object that specifies the type of service object to get. </param>
        object IServiceProvider.GetService(Type serviceType)
        {
            if (ServiceProvider == null)
                return null;
            return ServiceProvider.GetService(serviceType);
        }

        #endregion
    }
}