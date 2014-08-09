using System.ComponentModel;
using MugenMvvmToolkit.Binding.UiDesigner;

namespace $rootnamespace$
{
    /// <summary>
    /// This class allows you to use the bindings from the WinForms designer. 
    /// Drag the class from the Toolbox panel on your form.
    /// </summary>
    public class ViewBinder : Binder
    {
        public ViewBinder()
        {
        }

        public ViewBinder(IContainer container)
            : base(container)
        {
        }
    }
}