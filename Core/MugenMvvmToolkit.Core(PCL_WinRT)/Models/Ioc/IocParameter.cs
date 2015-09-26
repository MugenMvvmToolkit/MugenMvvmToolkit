#region Copyright

// ****************************************************************************
// <copyright file="IocParameter.cs">
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

using MugenMvvmToolkit.Interfaces;

namespace MugenMvvmToolkit.Models.IoC
{
    public class IocParameter : IIocParameter
    {
        #region Fields

        private readonly string _name;
        private readonly IocParameterType _parameterType;
        private readonly object _value;

        #endregion

        #region Constructors

        public IocParameter(string name, object value, IocParameterType parameterType)
        {
            _name = name;
            _value = value;
            _parameterType = parameterType;
        }

        #endregion

        #region Properties

        public IocParameterType ParameterType
        {
            get { return _parameterType; }
        }

        public string Name
        {
            get { return _name; }
        }

        public object Value
        {
            get { return _value; }
        }

        #endregion
    }
}
