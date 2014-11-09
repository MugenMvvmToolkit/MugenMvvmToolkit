#region Copyright
// ****************************************************************************
// <copyright file="OneTimeBindingMode.cs">
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

using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Binding.Interfaces.Accessors;
using MugenMvvmToolkit.Binding.Interfaces.Sources;
using MugenMvvmToolkit.Binding.Models.EventArg;

namespace MugenMvvmToolkit.Binding.Behaviors
{
    /// <summary>
    ///     Updates the binding target when the application starts or when the data context changes. This type of binding is
    ///     appropriate if you are using data where either a snapshot of the current state is appropriate to use or the data is
    ///     truly static. This type of binding is also useful if you want to initialize your target property with some value
    ///     from a source property and the data context is not known in advance. This is essentially a simpler form of
    ///     OneWay binding that provides better performance in cases where the source value
    ///     does not change.
    /// </summary>
    public sealed class OneTimeBindingMode : BindingModeBase
    {
        #region Overrides of BindingBehaviorBase

        /// <summary>
        ///     Attaches to the specified binding.
        /// </summary>
        protected override bool OnAttached()
        {
            if (!IsSourceAvailable() || !Binding.UpdateTarget())
                SubscribeSources(OneTimeHandler);
            return true;
        }

        /// <summary>
        ///     Detaches this instance from its associated binding.
        /// </summary>
        protected override void OnDetached()
        {
            UnsubscribeSources(OneTimeHandler);
        }

        /// <summary>
        ///     Creates a new binding behavior that is a copy of the current instance.
        /// </summary>
        protected override IBindingBehavior CloneInternal()
        {
            return new OneTimeBindingMode();
        }

        #endregion

        #region Methods

        private bool IsSourceAvailable()
        {
            var sourceAccessor = Binding.SourceAccessor;
            var singleAccessor = sourceAccessor as ISingleBindingSourceAccessor;
            if (singleAccessor == null)
            {
                foreach (var source in sourceAccessor.Sources)
                {
                    if (!source.GetPathMembers(false).AllMembersAvailable)
                        return false;
                }
                return true;
            }
            return singleAccessor.Source.GetPathMembers(false).AllMembersAvailable;
        }

        private void OneTimeHandler(IBindingSource sender, ValueChangedEventArgs args)
        {
            IDataBinding binding = Binding;
            if (binding == null || !IsSourceAvailable() || !binding.UpdateTarget())
                return;
            UnsubscribeSources(OneTimeHandler);
            binding.Dispose();
        }

        #endregion
    }
}