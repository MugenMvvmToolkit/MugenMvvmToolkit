#region Copyright
// ****************************************************************************
// <copyright file="Binder.cs">
// Copyright © Vyacheslav Volkov 2012-2014
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
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;
using MugenMvvmToolkit.Binding.Interfaces;

namespace MugenMvvmToolkit.Binding.UiDesigner
{
    [Description("Provides a data binding for controls.")]
    [ToolboxItem(true), ProvideProperty("Bindings", typeof(object))]
    public partial class Binder : Component, IExtenderProvider, ISupportInitialize
    {
        #region Fields

        private readonly Dictionary<object, Dictionary<string, string>> _controlBindings;
        private string _bindings;

        #endregion

        #region Constructors

        public Binder()
        {
            InitializeComponent();
            _controlBindings = new Dictionary<object, Dictionary<string, string>>();
            RootTagName = "Bindings";
            IgnoreControlException = true;
        }

        public Binder(IContainer container)
            : this()
        {
            container.Add(this);
        }

        #endregion

        #region Properties

        public string RootTagName { get; set; }

        public bool IgnoreControlException { get; set; }

        public ContainerControl ContainerControl { get; set; }

        [Editor(typeof(BindingEditorUITypeEditor), typeof(UITypeEditor))]
        public string Bindings
        {
            get
            {
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

        public object GetBindings(object control)
        {
            Dictionary<string, string> value;
            _controlBindings.TryGetValue(control, out value);
            if (value == null)
                return "No bindings";
            var name = PlatformExtensions.TryGetValue(control, "Name");
            if (name == null)
                return null;
            var stringBuilder = new StringBuilder();
            foreach (var binding in value)
                stringBuilder.Append(string.Format(" {0}=\"{1}\"", binding.Key, binding.Value));
            return string.Format("<{0}{1}/>", name, stringBuilder);
        }

        private void BindControls()
        {
            SetBindings(Bindings);
            IBindingProvider bindingProvider = BindingServiceProvider.BindingProvider;
            foreach (var controlBinding in _controlBindings)
            {
                string value;
                if (controlBinding.Value.TryGetValue(AttachedMemberConstants.DataContext, out value))
                {
                    controlBinding.Value.Remove(AttachedMemberConstants.DataContext);
                    bindingProvider.CreateBindingFromString(controlBinding.Key, AttachedMemberConstants.DataContext, value);
                }
                foreach (var binding in controlBinding.Value)
                    bindingProvider.CreateBindingFromString(controlBinding.Key, binding.Key, binding.Value);
            }
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
                    throw new ArgumentException(string.Format("The root tag: {0} is not found.", RootTagName),
                        "bindingsString");
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
                var msg = string.Format("The control with name '{0}' is not found", name);
                if (throwOnError)
                    throw new ArgumentException(msg);
                Tracer.Error(msg);
                return;
            }

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
            if (ContainerControl == null || ContainerControl.Name == name)
                return ContainerControl;
            var findByName = BindingServiceProvider.VisualTreeManager.FindByName(ContainerControl, name);
            if (findByName != null)
                return findByName;

            var type = ContainerControl.GetType();
            var field = type.GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (field == null)
                return null;
            return field.GetValue(ContainerControl);
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

        #endregion

        #region Implementation of IExtenderProvider

        public bool CanExtend(object extendee)
        {
            return extendee is Control;
        }

        #endregion

        #region Implementation of ISupportInitialize

        public void BeginInit()
        {
        }

        public void EndInit()
        {
            if (DesignMode)
                SetBindings(Bindings);
            else
                BindControls();
        }

        #endregion
    }
}