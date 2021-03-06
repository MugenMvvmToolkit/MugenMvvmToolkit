﻿#region Copyright

// ****************************************************************************
// <copyright file="ViewModelToViewBinding.cs">
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

using System.Windows.Data;
using MugenMvvmToolkit.WPF.Binding.Converters;

namespace MugenMvvmToolkit.WPF.MarkupExtensions
{
    public sealed class ViewModelToViewBinding : System.Windows.Data.Binding
    {
        #region Constructors

        public ViewModelToViewBinding()
        {
            Initialize();
        }

#if !WINDOWS_UWP
        public ViewModelToViewBinding(string path)
            : base(path)
        {
            Initialize();
        }
#endif

        #endregion

        #region Properties

        public bool? AlwaysCreateNewView
        {
            get { return Converter.AlwaysCreateNewView; }
            set { Converter.AlwaysCreateNewView = value; }
        }

        public string ViewName
        {
            get { return Converter.ViewName; }
            set { Converter.ViewName = value; }
        }

        public new ViewModelToViewConverter Converter => (ViewModelToViewConverter)base.Converter;

        #endregion

        #region Methods

        private void Initialize()
        {
            base.Converter = new ViewModelToViewConverter();
#if !WINDOWS_UWP
            ValidatesOnDataErrors = false;
            ValidatesOnExceptions = false;
#if !NET4
            ValidatesOnNotifyDataErrors = false;
#endif
#endif
            Mode = BindingMode.OneWay;
        }

        #endregion
    }
}
