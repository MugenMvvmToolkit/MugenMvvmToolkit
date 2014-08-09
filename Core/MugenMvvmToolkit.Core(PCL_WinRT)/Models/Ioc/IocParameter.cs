#region Copyright
// ****************************************************************************
// <copyright file="IocParameter.cs">
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
using MugenMvvmToolkit.Interfaces;

namespace MugenMvvmToolkit.Models.IoC
{
    /// <summary>
    ///     Represents the base interface for all ioc parameters.
    /// </summary>
    public class IocParameter : IIocParameter
    {
        #region Fields

        private readonly string _name;
        private readonly IocParameterType _parameterType;
        private readonly object _value;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="IocParameter" /> class.
        /// </summary>
        /// <param name="name">Specified parameter name.</param>
        /// <param name="value">Specified parameter value.</param>
        /// <param name="parameterType">Specified parameter type. </param>
        public IocParameter(string name, object value, IocParameterType parameterType)
        {
            _name = name;
            _value = value;
            _parameterType = parameterType;
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets the parameter type.
        /// </summary>
        public IocParameterType ParameterType
        {
            get { return _parameterType; }
        }

        /// <summary>
        ///     Gets the parameter name.
        /// </summary>
        public string Name
        {
            get { return _name; }
        }

        /// <summary>
        ///     Gets the parameter value.
        /// </summary>
        public object Value
        {
            get { return _value; }
        }

        #endregion
    }
}