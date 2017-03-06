#region Copyright

// ****************************************************************************
// <copyright file="IBindingValueConverter.cs">
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
using System.Globalization;
using MugenMvvmToolkit.Interfaces.Models;

namespace MugenMvvmToolkit.Binding.Interfaces
{
    public interface IBindingValueConverter
    {
        object Convert(object value, Type targetType, object parameter, CultureInfo culture, IDataContext context);

        object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture, IDataContext context);
    }
}
