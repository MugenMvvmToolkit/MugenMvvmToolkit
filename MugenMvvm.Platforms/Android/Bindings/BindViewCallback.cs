using Java.Lang;
using MugenMvvm.Android.Native.Interfaces.Views;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Bindings.Members;
using MugenMvvm.Bindings.Members.Descriptors;

namespace MugenMvvm.Android.Bindings
{
    public sealed class BindViewCallback : Object, IBindViewCallback
    {
        public static readonly BindViewCallback Instance = new();
        private static readonly BindablePropertyDescriptor<object, object?> ItemTemplateSelector = nameof(ItemTemplateSelector);

        private IViewAttributeAccessor _accessor = null!;

        private static void Bind(Object view, string? bind)
        {
            if (bind != null)
                view.Bind(bind, includeResult: false);
        }

        public void SetViewAccessor(IViewAttributeAccessor accessor) => _accessor = accessor;

        public void OnSetView(Object owner, Object view) => view.BindableMembers().SetParent(owner);

        public void Bind(Object view)
        {
            var template = _accessor.ItemTemplate;
            if (template != 0)
                ItemTemplateSelector.SetValue(view, SingleResourceTemplateSelector.Get(template));
            Bind(view, _accessor.Bind);
            Bind(view, _accessor.BindStyle);
        }
    }
}