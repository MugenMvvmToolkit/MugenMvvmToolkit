#region Copyright

// ****************************************************************************
// <copyright file="MethodData.cs">
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
using System.Reflection;
using JetBrains.Annotations;

namespace MugenMvvmToolkit.Binding.Models
{
    internal class MethodData
    {
        #region Fields

        private Func<IList<ArgumentData>, MethodInfo> _buildMethod;
        private MethodInfo _method;
        private bool _isExtensionMethod;
        private IList<ParameterInfo> _parameters;

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
            SetMethod(method);
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

        public bool IsLateBinding
        {
            get { return _buildMethod != null; }
        }

        #endregion

        #region Methods

        [CanBeNull]
        public MethodInfo Build(IList<ArgumentData> args)
        {
            if (_buildMethod == null)
                return _method;
            SetMethod(_buildMethod(args));
            _buildMethod = null;
            return _method;
        }

        private void SetMethod(MethodInfo method)
        {
            _method = method;
            if (method != null)
            {
                _isExtensionMethod = method.IsExtensionMethod();
                _parameters = method.GetParameters();
            }
        }

        #endregion
    }
}