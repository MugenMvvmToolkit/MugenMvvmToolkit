#region Copyright

// ****************************************************************************
// <copyright file="RecyclerViewDataBindingModule.cs">
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

using System.Collections;
using MugenMvvmToolkit.Android.Binding;
using MugenMvvmToolkit.Android.RecyclerView.Infrastructure;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.Binding.Models;
using MugenMvvmToolkit.Binding.Models.EventArg;
using MugenMvvmToolkit.Modules;

namespace MugenMvvmToolkit.Android.RecyclerView.Modules
{
    public class RecyclerViewDataBindingModule : ModuleBase
    {
        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="RecyclerViewDataBindingModule" /> class.
        /// </summary>
        public RecyclerViewDataBindingModule()
            : base(true)
        {
        }

        #endregion

        #region Methods

        private static void RecyclerViewItemsSourceChanged(global::Android.Support.V7.Widget.RecyclerView recyclerView,
            AttachedMemberChangedEventArgs<IEnumerable> args)
        {
            var adapter = recyclerView.GetAdapter() as ItemsSourceRecyclerAdapter;
            if (adapter == null)
            {
                adapter = new ItemsSourceRecyclerAdapter(recyclerView);
                recyclerView.SetAdapter(adapter);
            }
            adapter.ItemsSource = args.NewValue;
        }

        #endregion

        #region Overrides of ModuleBase

        /// <summary>
        ///     Loads the current module.
        /// </summary>
        protected override bool LoadInternal()
        {
            BindingServiceProvider.MemberProvider.Register(
                AttachedBindingMember.CreateAutoProperty(
                    AttachedMembers.ViewGroup.ItemsSource.Override<global::Android.Support.V7.Widget.RecyclerView>(),
                    RecyclerViewItemsSourceChanged));
            return true;
        }

        /// <summary>
        ///     Unloads the current module.
        /// </summary>
        protected override void UnloadInternal()
        {
        }

        #endregion
    }
}