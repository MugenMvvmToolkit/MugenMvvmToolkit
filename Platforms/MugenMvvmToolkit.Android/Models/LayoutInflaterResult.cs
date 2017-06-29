using System.Collections.Generic;
using Android.Views;
using MugenMvvmToolkit.Android.Binding.Interfaces;
using MugenMvvmToolkit.Binding;

namespace MugenMvvmToolkit.Android.Models
{
    public class LayoutInflaterResult : List<KeyValuePair<object, string>>
    {
        #region Constructors

        protected internal LayoutInflaterResult()
        {
        }

        #endregion

        #region Properties

        public View View { get; protected internal set; }

        #endregion

        #region Methods

        public void ApplyBindings()
        {
            for (var index = 0; index < Count; index++)
            {
                var binding = this[index];
                var manualBindings = binding.Key as IManualBindings;

                if (manualBindings == null)
                    BindingServiceProvider.BindingProvider.CreateBindingsFromString(binding.Key, binding.Value);
                else
                    manualBindings.SetBindings(binding.Value);
            }
        }

        #endregion
    }
}