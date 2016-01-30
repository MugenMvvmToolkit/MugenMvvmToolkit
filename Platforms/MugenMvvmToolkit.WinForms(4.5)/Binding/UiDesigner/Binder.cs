#region Copyright

// ****************************************************************************
// <copyright file="Binder.cs">
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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Xml.Linq;
using JetBrains.Annotations;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.Binding.Builders;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.WinForms.Binding.UiDesigner
{
    [Description("Provides a data binding for controls."), ToolboxItem(true)]
    public class Binder : Component, ISupportInitialize
    {
        #region Fields

        private readonly Dictionary<object, Dictionary<string, string>> _controlBindings;
        private string _bindings;
        private IList<IDataBinding> _dataBindings;
        private Type _containerControlType;
        private ContainerControl _containerControl;

        #endregion

        #region Constructors

        public Binder()
        {
            _controlBindings = new Dictionary<object, Dictionary<string, string>>();
            RootTagName = "Bindings";
            IgnoreControlException = true;
        }

        public Binder([NotNull] IContainer container)
            : this()
        {
            Should.NotBeNull(container, nameof(container));
            container.Add(this);
        }

        #endregion

        #region Properties

        public string RootTagName { get; set; }

        public bool IgnoreControlException { get; set; }

        public ContainerControl ContainerControl
        {
            get { return _containerControl; }
            set
            {
                _containerControl = value;
                _containerControlType = value == null ? null : value.GetType();
            }
        }

        [Editor(typeof(BindingEditorUITypeEditor), typeof(UITypeEditor))]
        public string Bindings
        {
            get
            {
                if (DesignMode)
                    BindingEditorView.CurrentControl = ContainerControl;
                if (DesignMode && string.IsNullOrEmpty(_bindings))
                    return string.Format(@"<{0}>
</{0}>", RootTagName);
                return _bindings;
            }
            set
            {
                _bindings = value;
                if (DesignMode)
                    SetBindings(value);
            }
        }

        #endregion

        #region Methods

        private void BindControls()
        {
            ClearBindings();
            SetBindings(Bindings);
            var bindingSet = new BindingSet();
            foreach (var controlBinding in _controlBindings)
            {
                foreach (var binding in controlBinding.Value)
                    bindingSet.BindFromExpression(controlBinding.Key, binding.Key, binding.Value);
            }
            _dataBindings = bindingSet.ApplyWithBindings();
            _controlBindings.Clear();
        }

        private void SetBindings(string bindingsString)
        {
            _controlBindings.Clear();
            if (string.IsNullOrEmpty(bindingsString))
                return;
            try
            {
                XElement xElement = XElement.Parse(bindingsString);
                XElement element = xElement.Name == RootTagName ? xElement : xElement.Element(RootTagName);
                if (element == null)
                    throw new ArgumentException($"The root tag: {RootTagName} is not found.", nameof(bindingsString));
                foreach (XElement descendant in element.Descendants())
                    UpdateControlBinding(descendant);
            }
            catch (Exception e)
            {
                if (DesignMode)
                    MessageBox.Show(e.Flatten(false));
                else
                    Tracer.Error(e.Flatten(false));
            }
        }

        private void UpdateControlBinding(XElement element)
        {
            var name = element.Name.LocalName;
            object component = FindComponent(name);
            bool throwOnError = !DesignMode || !IgnoreControlException;
            if (component == null)
            {
                var msg = $"The control with name '{name}' is not found";
                if (throwOnError)
                    throw new ArgumentException(msg);
                Tracer.Error(msg);
                return;
            }
            var container = ContainerControl;
            if (container != null && !(component is Control))
                component.SetBindingMemberValue(AttachedMembers.Object.Parent, container);

            Dictionary<string, string> bindings;
            if (!_controlBindings.TryGetValue(component, out bindings))
            {
                bindings = new Dictionary<string, string>();
                _controlBindings[component] = bindings;
            }
            foreach (XAttribute attribute in element.Attributes())
                bindings[attribute.Name.LocalName] = attribute.Value;
        }

        private object FindComponent(string name)
        {
            if (DesignMode)
            {
                var container = Site.Container;
                if (container == null)
                    return null;
                for (int i = 0; i < container.Components.Count; i++)
                {
                    var cmp = container.Components[i];
                    if (PlatformExtensions.TryGetValue(cmp, "Name") == name)
                        return cmp;
                }
                return null;
            }
            var containerControl = ContainerControl;
            if (containerControl == null || containerControl.Name == name)
                return containerControl;
            var field = _containerControlType.GetFieldEx(name, MemberFlags.Public | MemberFlags.NonPublic | MemberFlags.Instance);
            if (field == null)
                return BindingServiceProvider.VisualTreeManager.FindByName(containerControl, name);
            return field.GetValueEx<object>(containerControl);
        }

        private void ClearBindings()
        {
            var dataBindings = _dataBindings;
            _dataBindings = null;
            if (dataBindings == null)
                return;
            for (int i = 0; i < dataBindings.Count; i++)
            {
                try
                {
                    dataBindings[i].Dispose();
                }
                catch (Exception e)
                {
                    Tracer.Error(e.Flatten(true));
                }
            }
        }

        #endregion

        #region Overrides of Component

        public override ISite Site
        {
            get { return base.Site; }
            set
            {
                base.Site = value;
                if (value == null)
                    return;
                var host = value.GetService<IDesignerHost>();
                if (host == null)
                    return;
                IComponent componentHost = host.RootComponent;
                ContainerControl = componentHost as ContainerControl;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                ClearBindings();
            base.Dispose(disposing);
        }

        #endregion

        #region Implementation of interfaces

        public virtual void BeginInit()
        {
        }

        public virtual void EndInit()
        {
            if (DesignMode)
                SetBindings(Bindings);
            else
                BindControls();
        }

        #endregion
    }
}
