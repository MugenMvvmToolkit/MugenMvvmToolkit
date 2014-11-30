#region Copyright
// ****************************************************************************
// <copyright file="ViewModelToViewBinding.cs">
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

using MugenMvvmToolkit.Binding.Converters;
#if NETFX_CORE || WINDOWSCOMMON
using Windows.UI.Xaml.Data;
using BindingEx = Windows.UI.Xaml.Data.Binding;
#else
using System.Windows.Data;
using BindingEx = System.Windows.Data.Binding;
#endif

namespace MugenMvvmToolkit.MarkupExtensions
{
    /// <summary>
    ///     Represents the binding that allows to convert a view model to view.
    /// </summary>
    public sealed class ViewModelToViewBinding : BindingEx
    {
        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="ViewModelToViewBinding" /> class.
        /// </summary>
        public ViewModelToViewBinding()
        {
            Initialize();
        }

#if !NETFX_CORE && !WINDOWSCOMMON
        /// <summary>
        ///     Initializes a new instance of the <see cref="ViewModelToViewBinding" /> class with an initial path.
        /// </summary>
        /// <param name="path">The initial <see cref="P:System.Windows.Data.Binding.Path" /> for the binding.</param>
        public ViewModelToViewBinding(string path)
            : base(path)
        {
            Initialize();
        }
#endif

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the default value that indicates that view converter should always create new view.
        /// </summary>
        public bool? AlwaysCreateNewView
        {
            get { return Converter.AlwaysCreateNewView; }
            set { Converter.AlwaysCreateNewView = value; }
        }

        /// <summary>
        /// Gets or sets the name of view.
        /// </summary>
        public string ViewName
        {
            get { return Converter.ViewName; }
            set { Converter.ViewName = value; }
        }

        /// <summary>
        /// Gets the converter.
        /// </summary>
        public new ViewModelToViewConverter Converter
        {
            get { return (ViewModelToViewConverter)base.Converter; }
        }

        #endregion

        #region Methods

        private void Initialize()
        {
            base.Converter = new ViewModelToViewConverter();
#if !NETFX_CORE && !WINDOWSCOMMON
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