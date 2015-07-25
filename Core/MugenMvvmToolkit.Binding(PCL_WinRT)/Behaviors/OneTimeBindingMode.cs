#region Copyright

// ****************************************************************************
// <copyright file="OneTimeBindingMode.cs">
// Copyright (c) 2012-2015 Vyacheslav Volkov
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
        #region Fields

        private readonly bool _disposeBinding;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="OneTimeBindingMode" /> class.
        /// </summary>
        public OneTimeBindingMode()
            : this(true)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="OneTimeBindingMode" /> class.
        /// </summary>
        public OneTimeBindingMode(bool disposeBinding)
        {
            _disposeBinding = disposeBinding;
        }

        #endregion

        #region Overrides of BindingBehaviorBase

        /// <summary>
        ///     Attaches to the specified binding.
        /// </summary>
        protected override bool OnAttached()
        {
            if (!Binding.SourceAccessor.IsAllMembersAvailable() || !Binding.UpdateTarget())
            {
                SubscribeSources(OneTimeHandler);
                return true;
            }
            if (_disposeBinding)
                Binding.Dispose();
            return false;
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
            return new OneTimeBindingMode(_disposeBinding);
        }

        #endregion

        #region Methods

        private void OneTimeHandler(IBindingSource sender, ValueChangedEventArgs args)
        {
            IDataBinding binding = Binding;
            if (binding == null || !binding.SourceAccessor.IsAllMembersAvailable() || !binding.UpdateTarget())
                return;
            UnsubscribeSources(OneTimeHandler);
            if (_disposeBinding)
                binding.Dispose();
        }

        #endregion
    }
}