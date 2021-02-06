using System;
using Android.Runtime;
using Java.Interop;
using MugenMvvm.Android.Internal;
using MugenMvvm.Android.Native.Interfaces.Views;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Bindings.Interfaces.Core;
using MugenMvvm.Bindings.Members;
using MugenMvvm.Bindings.Members.Descriptors;
using MugenMvvm.Collections;
using MugenMvvm.Extensions;
using Object = Java.Lang.Object;

namespace MugenMvvm.Android.Bindings
{
    public sealed class BindViewCallback : Object, IBindViewCallback
    {
        public static readonly BindViewCallback Instance = new();
        private static readonly BindablePropertyDescriptor<object, object?> ItemTemplateSelector = nameof(ItemTemplateSelector);
        private readonly IBindingManager? _bindingManager;

        private readonly NativeStringAccessor _stringAccessor;
        private IViewAttributeAccessor _accessor = null!;
        private IntPtr _getBindMethod;
        private IntPtr _getBindStyleMethod;

        public BindViewCallback(IBindingManager? bindingManager = null)
        {
            _bindingManager = bindingManager;
            _stringAccessor = new NativeStringAccessor();
        }

        public void SetViewAccessor(IViewAttributeAccessor accessor)
        {
            _accessor = accessor;
            if (accessor == null)
            {
                _getBindMethod = default;
                _getBindStyleMethod = default;
            }
            else
            {
                var @class = JNIEnv.GetObjectClass(accessor.Handle);
                _getBindMethod = JNIEnv.GetMethodID(@class, "getBind", "()Ljava/lang/String;");
                _getBindStyleMethod = JNIEnv.GetMethodID(@class, "getBindStyle", "()Ljava/lang/String;");
                JNIEnv.DeleteLocalRef(@class);
            }
        }

        public void OnSetView(Object owner, Object view) => view.BindableMembers().SetParent(owner);

        public void Bind(Object view)
        {
            var template = _accessor.ItemTemplate;
            if (template != 0)
                ItemTemplateSelector.SetValue(view, SingleResourceTemplateSelector.Get(template));
            Bind(view, _getBindStyleMethod);
            Bind(view, _getBindMethod);
        }

        private unsafe void Bind(Object view, IntPtr method)
        {
            var jniObjectReference = new JniObjectReference(JNIEnv.CallObjectMethod(_accessor.Handle, method));
            if (!jniObjectReference.IsValid)
                return;

            ItemOrIReadOnlyList<IBindingBuilder> expressions;
            char* chars = default;
            try
            {
                var length = JniEnvironment.Strings.GetStringLength(jniObjectReference);
                if (length == 0)
                    return;

                var copy = false;
                chars = JniEnvironment.Strings.GetStringChars(jniObjectReference, &copy);
                _stringAccessor.Initialize(new IntPtr(chars), length);
                expressions = _bindingManager.DefaultIfNull().ParseBindingExpression(_stringAccessor);
                _stringAccessor.Initialize(default, 0);
            }
            finally
            {
                if (chars != default)
                    JniEnvironment.Strings.ReleaseStringChars(jniObjectReference, chars);
                JNIEnv.DeleteLocalRef(jniObjectReference.Handle);
            }

            foreach (var expression in expressions)
                expression.Build(view);
        }
    }
}