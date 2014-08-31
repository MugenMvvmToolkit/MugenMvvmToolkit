#region Copyright
// ****************************************************************************
// <copyright file="DefaultValueOnExceptionBehavior.cs">
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
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Binding.Interfaces.Accessors;
using MugenMvvmToolkit.Binding.Interfaces.Sources;
using MugenMvvmToolkit.Binding.Models;
using MugenMvvmToolkit.Binding.Models.EventArg;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Binding.Behaviors
{
    /// <summary>
    ///     Represents the binding behavior that allows to set default value on a binding exception.
    /// </summary>
    public sealed class DefaultValueOnExceptionBehavior : IBindingBehavior
    {
        #region Fields

        /// <summary>
        ///     Gets the instance of a <see cref="DefaultValueOnExceptionBehavior" /> class.
        /// </summary>
        public static readonly DefaultValueOnExceptionBehavior Instance;

        /// <summary>
        ///     Gets the id of behavior.
        /// </summary>
        public static readonly Guid IdDefaultValuesOnExceptionBehavior;

        private static readonly EventHandler<IDataBinding, BindingExceptionEventArgs> BindingExceptionDelegate;

        #endregion

        #region Constructors

        static DefaultValueOnExceptionBehavior()
        {
            Instance = new DefaultValueOnExceptionBehavior();
            IdDefaultValuesOnExceptionBehavior = new Guid("BB266907-520E-4461-9D95-A549326049DA");
            BindingExceptionDelegate = OnBindingException;
        }

        private DefaultValueOnExceptionBehavior()
        {
        }

        #endregion

        #region Methods

        private static void OnBindingException(object sender, BindingExceptionEventArgs args)
        {
            var dataBinding = sender as IDataBinding;
            if (dataBinding != null && args.Action == BindingAction.UpdateSource)
                SetDefaultValue(dataBinding);
        }

        private static void SetDefaultValue(IDataBinding dataBinding)
        {
            var singleAccessor = dataBinding.SourceAccessor as ISingleBindingSourceAccessor;
            if (singleAccessor == null)
            {
                var sources = dataBinding.SourceAccessor.Sources;
                for (int index = 0; index < sources.Count; index++)
                    SetDefaultValue(sources[index]);
            }
            else
                SetDefaultValue(singleAccessor.Source);
        }

        private static void SetDefaultValue(IBindingSource source)
        {
            var pathMembers = source.GetPathMembers(false);
            if (pathMembers.AllMembersAvailable)
                pathMembers.LastMember.SetValue(pathMembers.PenultimateValue,
                    new[] { pathMembers.LastMember.Type.GetDefaultValue() });
        }

        #endregion

        #region Implementation of IBindingBehavior

        /// <summary>
        ///     Gets the id of behavior. Each <see cref="IDataBinding" /> can have only one instance with the same id.
        /// </summary>
        public Guid Id
        {
            get { return IdDefaultValuesOnExceptionBehavior; }
        }

        /// <summary>
        ///     Gets the behavior priority.
        /// </summary>
        public int Priority
        {
            get { return 0; }
        }

        /// <summary>
        ///     Attaches to the specified binding.
        /// </summary>
        /// <param name="binding">The binding to attach to.</param>
        public bool Attach(IDataBinding binding)
        {
            binding.BindingException += BindingExceptionDelegate;
            return true;
        }

        /// <summary>
        ///     Detaches this instance from its associated binding.
        /// </summary>
        public void Detach(IDataBinding binding)
        {
            binding.BindingException -= BindingExceptionDelegate;
        }

        /// <summary>
        ///     Creates a new binding behavior that is a copy of the current instance.
        /// </summary>
        public IBindingBehavior Clone()
        {
            return this;
        }

        #endregion
    }
}