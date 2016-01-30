#region Copyright

// ****************************************************************************
// <copyright file="INotifyDataErrorInfo.cs">
// Copyright (c) 2012-2016 Vyacheslav Volkov
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

#if NONOTIFYDATAERROR
using System;
using System.Collections;
using MugenMvvmToolkit.Models.EventArg;

namespace MugenMvvmToolkit.Interfaces.Validation
{
    public interface INotifyDataErrorInfo
    {
        bool HasErrors { get; }

        event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        IEnumerable GetErrors(string propertyName);
    }
}
#endif
