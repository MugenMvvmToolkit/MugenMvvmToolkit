#region Copyright

// ****************************************************************************
// <copyright file="LinkerInclude.cs">
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

using Android.Widget;
using JetBrains.Annotations;
#if APPCOMPAT
using Android.Support.V4.View;
#endif

namespace MugenMvvmToolkit.ActionBarSupport
{
    [UsedImplicitly]
    internal static class LinkerInclude
    {
        [UsedImplicitly]
        public static void Include()
        {
            var searchView = new SearchView(null);
            searchView.QueryTextChange += (sender, args) => { };
            searchView.QueryTextChange -= (sender, args) => { };
        }
    }
}