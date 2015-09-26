using MugenMvvmToolkit.Android.Binding;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.Binding.Models;

namespace MugenMvvmToolkit.Android.Design
{
    public static class AttachedMembersDesign
    {
        #region Nested types

        public class NavigationView : AttachedMembers.ViewGroup
        {
            #region Fields

            public static readonly BindingMemberDescriptor<global::Android.Support.Design.Widget.NavigationView, int> MenuTemplate;

            #endregion

            #region Constructors

            static NavigationView()
            {
                MenuTemplate = new BindingMemberDescriptor<global::Android.Support.Design.Widget.NavigationView, int>("MenuTemplate");
            }

            protected NavigationView()
            {
            }

            #endregion
        }

        public class TabLayout : AttachedMembers.ViewGroup
        {
            #region Fields

            public static readonly BindingMemberDescriptor<global::Android.Support.Design.Widget.TabLayout, object> SelectedItem;
            public static readonly BindingMemberDescriptor<global::Android.Support.Design.Widget.TabLayout, bool?> RestoreSelectedIndex;

            #endregion

            #region Constructors

            static TabLayout()
            {
                SelectedItem = new BindingMemberDescriptor<global::Android.Support.Design.Widget.TabLayout, object>(AttachedMemberConstants.SelectedItem);
                RestoreSelectedIndex = AttachedMembers.TabHost.RestoreSelectedIndex.Override<global::Android.Support.Design.Widget.TabLayout>();
            }

            protected TabLayout()
            {
            }

            #endregion
        }

        #endregion
    }
}
