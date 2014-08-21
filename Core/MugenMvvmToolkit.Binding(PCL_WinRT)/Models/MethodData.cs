#region Copyright
// ****************************************************************************
// <copyright file="MethodData.cs">
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
using System.Reflection;

namespace MugenMvvmToolkit.Binding.Models
{
    internal class MethodData
    {
        #region Fields

        private readonly MethodInfo _method;
        private readonly Func<IList<ArgumentData>, MethodInfo> _buildMethod;
        private readonly bool _isExtensionMethod;
        private readonly IList<ParameterInfo> _parameters;

        #endregion

        #region Constructors

        public MethodData(MethodInfo method)
            : this(method, null)
        {
            _method = method;
        }

        public MethodData(MethodInfo method, Func<IList<ArgumentData>, MethodInfo> buildMethod)
        {
            _buildMethod = buildMethod;
            _isExtensionMethod = method.IsExtensionMethod();
            _parameters = method.GetParameters();
        }

        #endregion

        #region Properties

        public IList<ParameterInfo> Parameters
        {
            get { return _parameters; }
        }

        public bool IsExtensionMethod
        {
            get { return _isExtensionMethod; }
        }

        #endregion

        #region Methods

        public MethodInfo Build(IList<ArgumentData> args)
        {
            if (_buildMethod == null)
                return _method;
            return _buildMethod(args);
        }

        #endregion
    }
}