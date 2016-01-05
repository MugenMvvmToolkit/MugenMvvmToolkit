#region Copyright

// ****************************************************************************
// <copyright file="EntityStateManager.cs">
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
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using MugenMvvmToolkit.Collections;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Infrastructure
{
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

            protected override bool Equals(string x, string y)
            {
                return x.Equals(y, StringComparison.Ordinal);
            }

            protected override int GetHashCode(string key)
            {
                return key.GetHashCode();
            }

            #endregion

            #region Implementation of IEntitySnapshot

            public bool SupportChangeDetection => true;

            public void Restore(object entity)
            {
                Should.NotBeNull(entity, nameof(entity));
                foreach (var state in this)
                {
                    if (!Equals(state.Value.PropertyInfo.GetValueEx<object>(entity), state.Value.Value))
                        state.Value.PropertyInfo.SetValueEx(entity, state.Value.Value);
                }
                if (Tracer.TraceInformation)
                    Tracer.Info("The state of entity {0} was restored", entity.GetType());
            }

            public bool HasChanges(object entity)
            {
                Should.NotBeNull(entity, nameof(entity));
                foreach (var savedState in this)
                {
                    if (!Equals(savedState.Value.PropertyInfo.GetValueEx<object>(entity), savedState.Value.Value))
                        return true;
                }
                return false;
            }

            public bool HasChanges(object entity, string propertyName)
            {
                Should.NotBeNull(entity, nameof(entity));
                SavedState savedState;
                if (!TryGetValue(propertyName, out savedState))
                    return false;
                return !Equals(savedState.PropertyInfo.GetValueEx<object>(entity), savedState.Value);
            }

            public IDictionary<string, Tuple<object, object>> Dump(object entity)
            {
                Should.NotBeNull(entity, nameof(entity));
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

            public PropertyInfo PropertyInfo;
            public object Value;

            #endregion

            #region Constructors

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
            TypesToProperties = new Dictionary<Type, IList<PropertyInfo>>();
        }

        #endregion

        #region Implementation of IEntityStateManager

        public IEntitySnapshot CreateSnapshot(object entity, IDataContext context = null)
        {
            Should.NotBeNull(entity, nameof(entity));
            if (Tracer.TraceInformation)
                Tracer.Info("The state snapshot for the '{0}' was created", entity.GetType());
            return new EntitySnapshot(entity, GetPropertiesInternal(entity));
        }

        #endregion

        #region Methods

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
