#region Copyright

// ****************************************************************************
// <copyright file="LinkerInclude.cs">
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

using Android.Content;
using Android.Runtime;
using Android.Widget;
using JetBrains.Annotations;

namespace MugenMvvmToolkit.Android
{
    internal static class LinkerInclude
    {
        public abstract class LinkerIncludeAdapter : AdapterView
        {
            [Preserve(Conditional = true)]
            public LinkerIncludeAdapter(Context context)
                : base(context)
            {
                RawAdapter = RawAdapter;
            }
        }

        [UsedImplicitly]
        public static void IncludeAdapterView<T>(AdapterView<T> adapterView, LinkerIncludeAdapter rawAdapterView) where T : IAdapter
        {
            if (adapterView != null)
                adapterView.Adapter = adapterView.Adapter;
        }
    }
}
