#region Copyright

// ****************************************************************************
// <copyright file="IStableIdProvider.cs">
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
using JetBrains.Annotations;

namespace MugenMvvmToolkit.Android.Interfaces
{
    public interface IStableIdProvider
    {
        long GetItemId([CanBeNull] object item);
    }
}