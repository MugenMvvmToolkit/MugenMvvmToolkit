using Java.Lang;
using MugenMvvm.Android.Members;
using MugenMvvm.Android.Native.Interfaces.Views;
using MugenMvvm.Binding.Extensions;
using MugenMvvm.Binding.Interfaces.Core;
using MugenMvvm.Binding.Members;
using MugenMvvm.Extensions;
using MugenMvvm.Extensions.Internal;

namespace MugenMvvm.Android.Binding
{
    public sealed class AndroidViewBindCallback : Object, IViewBindCallback
    {
        #region Fields

        private readonly IBindingManager? _bindingManager;

        #endregion

        #region Constructors

        public AndroidViewBindCallback(IBindingManager? bindingManager = null)
        {
            _bindingManager = bindingManager;
        }

        #endregion

        #region Implementation of interfaces

        public void Bind(Object view, IViewAttributeAccessor accessor)
        {
            var template = accessor.ItemTemplate;
            if (template != 0)
                BindableMembers.For<IListView>().ItemTemplateSelector().Override<object>().SetValue(view, SingleDataTemplateSelector.Get(template));
            var expression = _bindingManager.DefaultIfNull().TryParseBindingExpression(accessor.Bind);
            if (expression.Item != null)
                expression.Item.Build(view);
            else
            {
                for (var i = 0; i < expression.Count(); i++)
                    expression.Get(i).Build(view);
            }
        }

        #endregion
    }
}