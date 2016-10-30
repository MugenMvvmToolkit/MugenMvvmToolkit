#region Copyright

// ****************************************************************************
// <copyright file="LinkerInclude.cs">
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

using Android.Content;
using Android.Runtime;
using Android.Widget;
using JetBrains.Annotations;

namespace MugenMvvmToolkit.Android
{
    [Preserve(AllMembers = true)]
    internal static class LinkerInclude
    {
        [Preserve(AllMembers = true)]
        private abstract class LinkerIncludeAdapter : AdapterView
        {
            private LinkerIncludeAdapter(Context context)
                : base(context)
            {
                RawAdapter = RawAdapter;
            }
        }

        [UsedImplicitly]
        private static void IncludeAdapterView<T>(AdapterView<T> adapter) where T : IAdapter
        {
            adapter.Adapter = adapter.Adapter;
        }
    }
}
