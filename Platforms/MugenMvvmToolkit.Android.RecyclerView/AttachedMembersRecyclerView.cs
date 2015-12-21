#region Copyright

// ****************************************************************************
// <copyright file="AttachedMembersRecyclerView.cs">
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
using Android.Views;
using MugenMvvmToolkit.Android.Binding;
using MugenMvvmToolkit.Binding.Models;

namespace MugenMvvmToolkit.Android.RecyclerView
{
    public static class AttachedMembersRecyclerView
    {
        #region Nested types

        public abstract class RecyclerView : AttachedMembers.ViewGroup
        {
            #region Fields

            public static readonly BindingMemberDescriptor<global::Android.Support.V7.Widget.RecyclerView,
                Func<LayoutInflater, ViewGroup, int, global::Android.Support.V7.Widget.RecyclerView.ViewHolder>> CreateViewHolderDelegate;

            #endregion

            #region Constructors

            static RecyclerView()
            {
                CreateViewHolderDelegate = new BindingMemberDescriptor<global::Android.Support.V7.Widget.RecyclerView,
                    Func<LayoutInflater, ViewGroup, int, global::Android.Support.V7.Widget.RecyclerView.ViewHolder>>(nameof(CreateViewHolderDelegate));
            }

            #endregion
        }

        #endregion
    }
}