#region Copyright
// ****************************************************************************
// <copyright file="EntityStateManager.cs">
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
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using MugenMvvmToolkit.Collections;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Infrastructure
{
    /// <summary>
    ///     Represents the base entity state manager that uses the property to save state.
    /// </summary>
    public class EntityStateManager : IEntityStateManager
    {
        #region Nested types

        private sealed class EntitySnapshot : LightDictionaryBase<string, SavedState>, IEntitySnapshot
        {
            #region Constructors

            public EntitySnapshot(object entity, IList<PropertyInfo> properties)
                : base(properties.Count)
            {
                for (int index = 0; index < properties.Count; index++)
                {
                    PropertyInfo propertyInfo = properties[index];
                    var oldState = propertyInfo.GetValueEx<object>(entity);
                    Add(propertyInfo.Name, new SavedState(propertyInfo, oldState));
                }
            }

            #endregion

            #region Overrides of LightDictionaryBase<PropertyInfo,object>

            /// <summary>
            ///     Determines whether the specified objects are equal.
            /// </summary>
            protected override bool Equals(string x, string y)
            {
                return x.Equals(y, StringComparison.Ordinal);
            }

            /// <summary>
            ///     Returns a hash code for the specified object.
            /// </summary>
            protected override int GetHashCode(string key)
            {
                return key.GetHashCode();
            }

            #endregion

            #region Implementation of IEntitySnapshot

            /// <summary>
            ///     Gets a value indicating whether the snapshot supports change detection.
            /// </summary>
            public bool SupportChangeDetection
            {
                get { return true; }
            }

            /// <summary>
            ///     Restores the state of entity.
            /// </summary>
            /// <param name="entity">The specified entity to restore state.</param>
            public void Restore(object entity)
            {
                Should.NotBeNull(entity, "entity");
                foreach (var state in this)
                {
                    if (!Equals(state.Value.PropertyInfo.GetValueEx<object>(entity), state.Value.Value))
                        state.Value.PropertyInfo.SetValueEx(entity, state.Value.Value);
                }
                Tracer.Info("The state of entity {0} was restored", entity.GetType());
            }

            /// <summary>
            ///     Gets a value indicating whether the entity has changes.
            /// </summary>
            public bool HasChanges(object entity)
            {
                Should.NotBeNull(entity, "entity");
                foreach (var savedState in this)
                {
                    if (!Equals(savedState.Value.PropertyInfo.GetValueEx<object>(entity), savedState.Value.Value))
                        return true;
                }
                return false;
            }

            /// <summary>
            ///     Gets a value indicating whether the entity has changes.
            /// </summary>
            public bool HasChanges(object entity, string propertyName)
            {
                Should.NotBeNull(entity, "entity");
                SavedState savedState;
                if (!TryGetValue(propertyName, out savedState))
                    return false;
                return !Equals(savedState.PropertyInfo.GetValueEx<object>(entity), savedState.Value);
            }

            /// <summary>
            ///     Dumps the state of object.
            /// </summary>
            public IDictionary<string, Tuple<object, object>> Dump(object entity)
            {
                Should.NotBeNull(entity, "entity");
                var dictionary = new Dictionary<string, Tuple<object, object>>();
                foreach (var savedState in this)
                {
                    var newValue = savedState.Value.PropertyInfo.GetValueEx<object>(entity);
                    if (!Equals(newValue, savedState.Value.Value))
                        dictionary[savedState.Key] = Tuple.Create(savedState.Value.Value, newValue);
                }
                return dictionary;
            }

            #endregion
        }

        [StructLayout(LayoutKind.Auto)]
        private struct SavedState
        {
            #region Fields

            public readonly PropertyInfo PropertyInfo;

            public readonly object Value;

            #endregion

            #region Constructors

            /// <summary>
            ///     Initializes a new instance of the <see cref="SavedState" /> class.
            /// </summary>
            public SavedState(PropertyInfo propertyInfo, object value)
            {
                PropertyInfo = propertyInfo;
                Value = value;
            }

            #endregion
        }

        #endregion

        #region Fields

        private static readonly Dictionary<Type, IList<PropertyInfo>> TypesToProperties;

        #endregion

        #region Constructors

        static EntityStateManager()
        {
            TypesToProperties= new Dictionary<Type, IList<PropertyInfo>>();
        }

        #endregion

        #region Implementation of IEntityStateManager

        /// <summary>
        ///     Creates an instance of <see cref="IEntitySnapshot" />
        /// </summary>
        /// <param name="entity">The specified entity to create snapshot.</param>
        /// <returns>An instance of <see cref="IEntitySnapshot" /></returns>
        public IEntitySnapshot CreateSnapshot(object entity)
        {
            Should.NotBeNull(entity, "entity");
            Tracer.Info("The state snapshot of entity {0} was created", entity.GetType());
            return new EntitySnapshot(entity, GetPropertiesInternal(entity));
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Gets properties for save state.
        /// </summary>
        /// <param name="entity">The specified entity.</param>
        /// <returns>
        ///     A series of instances of <see cref="PropertyInfo" />.
        /// </returns>
        private IList<PropertyInfo> GetPropertiesInternal(object entity)
        {
            Type type = entity.GetType();
            lock (TypesToProperties)
            {
                IList<PropertyInfo> list;
                if (!TypesToProperties.TryGetValue(type, out list))
                {
                    bool shouldCache;
                    list = GetProperties(entity, out shouldCache);
                    if (shouldCache)
                        TypesToProperties[type] = list;
                }
                return list;
            }
        }

        /// <summary>
        ///     Gets properties for save state.
        /// </summary>
        /// <param name="entity">The specified entity.</param>
        /// <param name="shouldCache"></param>
        /// <returns>
        ///     A series of instances of <see cref="PropertyInfo" />.
        /// </returns>
        protected virtual IList<PropertyInfo> GetProperties(object entity, out bool shouldCache)
        {
            Type type = entity.GetType();
            shouldCache = true;
            return type
                .GetPropertiesEx(MemberFlags.Public | MemberFlags.Instance)
                .Where(info => info.CanRead && info.CanWrite && info.GetIndexParameters().Length == 0)
                .ToArray();
        }

        #endregion
    }
}