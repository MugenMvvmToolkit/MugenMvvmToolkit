using Java.Lang;
using MugenMvvm.Android.Members;
using MugenMvvm.Android.Native.Interfaces.Views;
using MugenMvvm.Binding.Build;
using MugenMvvm.Binding.Extensions;
using MugenMvvm.Binding.Members;

namespace MugenMvvm.Android.Binding
{
    public sealed class AndroidViewBindCallback : Object, IViewBindCallback
    {
        #region Implementation of interfaces

        public void Bind(Object view, IViewAttributeAccessor accessor)
        {
            var template = accessor.ItemTemplate;
            if (template != 0)
                BindableMembers.For<IListView>().ItemTemplateSelector().Override<object>().SetValue(view, SingleDataTemplateSelector.Get(template));
            view.BindWithoutResult(accessor.Bind);
        }

        public void OnSetView(Object owner, Object view)
        {
            view.BindableMembers().SetParent(owner);
        }

        #endregion
    }
}