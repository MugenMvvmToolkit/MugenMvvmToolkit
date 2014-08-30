#region Copyright
// ****************************************************************************
// <copyright file="StateTransitionManager.cs">
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
using System.Runtime.Serialization;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Infrastructure
{
    /// <summary>
    ///     Represents the class that provides methods for transiting one state to another state.
    /// </summary>
    [Serializable, DataContract(Namespace = ApplicationSettings.DataContractNamespace)]
    public sealed class StateTransitionManager : IStateTransitionManager
    {
        #region Fields

        /// <summary>
        ///     Gets the instance of <see cref="StateTransitionManager" />.
        /// </summary>
        public static readonly StateTransitionManager Instance;

        #endregion

        #region Constructors

        static StateTransitionManager()
        {
            Instance = new StateTransitionManager();
        }

        internal StateTransitionManager()
        {
        }

        #endregion

        #region Implementation of IStateTransitionManager

        /// <summary>
        ///     Changes state from one state to another state.
        /// </summary>
        /// <param name="from">The specified state from.</param>
        /// <param name="to">The specified state to.</param>
        /// <param name="validateState">The flag indicating that state will be validated before assigned.</param>
        /// <returns>An instance of state, if any.</returns>
        public EntityState ChangeState(EntityState @from, EntityState to, bool validateState)
        {
            switch (from)
            {
                case EntityState.Unchanged:
                    switch (to)
                    {
                        case EntityState.Added:
                            if (validateState)
                                throw ExceptionManager.NotConvertableState(from, to);
                            return to;
                        default:
                            return to;
                    }
                case EntityState.Added:
                    switch (to)
                    {
                        case EntityState.Unchanged:
                            if (validateState)
                                throw ExceptionManager.NotConvertableState(from, to);
                            return to;
                        case EntityState.Deleted:
                            return EntityState.Detached;
                        case EntityState.Modified:
                            return EntityState.Added;
                        default:
                            return to;
                    }
                case EntityState.Deleted:
                    switch (to)
                    {
                        case EntityState.Unchanged:
                            if (validateState)
                                throw ExceptionManager.NotConvertableState(from, to);
                            return to;
                        case EntityState.Added:
                            if (validateState)
                                throw ExceptionManager.NotConvertableState(from, to);
                            return to;
                        case EntityState.Modified:
                            if (validateState)
                                throw ExceptionManager.NotConvertableState(from, to);
                            return to;
                        default:
                            return to;
                    }
                case EntityState.Modified:
                    switch (to)
                    {
                        case EntityState.Unchanged:
                            if (validateState)
                                throw ExceptionManager.NotConvertableState(from, to);
                            return to;
                        case EntityState.Added:
                            if (validateState)
                                throw ExceptionManager.NotConvertableState(from, to);
                            return to;
                        default:
                            return to;
                    }
                case EntityState.Detached:
                    return to;
                default:
                    throw ExceptionManager.EnumOutOfRange("from", from);
            }
        }

        #endregion
    }
}