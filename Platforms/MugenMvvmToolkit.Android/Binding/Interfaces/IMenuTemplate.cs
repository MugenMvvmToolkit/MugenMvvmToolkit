#region Copyright

// ****************************************************************************
// <copyright file="IMenuTemplate.cs">
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
using Android.Views;

namespace MugenMvvmToolkit.Android.Binding.Interfaces
{
    public interface IMenuTemplate
    {
        void Apply(IMenu menu, Context context, object parent);        
    }
}