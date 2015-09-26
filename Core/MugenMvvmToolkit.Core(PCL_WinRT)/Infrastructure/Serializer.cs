#region Copyright

// ****************************************************************************
// <copyright file="Serializer.cs">
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

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Infrastructure
{
    public class Serializer : ISerializer
    {
        #region Nested types

        [DataContract(Namespace = ApplicationSettings.DataContractNamespace, IsReference = true), Serializable]
        internal sealed class DataContainer
        {
            #region Properties

            [DataMember]
            public object Data { get; set; }

            #endregion
        }

        #endregion

        #region Fields

        private readonly HashSet<Type> _knownTypes;
        private DataContractSerializer _contractSerializer;
        private bool _isDirty;

        #endregion

        #region Constructors

        public Serializer(IEnumerable<Assembly> assembliesToScan)
        {
            _knownTypes = new HashSet<Type>();
            if (assembliesToScan != null)
                AddKnownTypes(assembliesToScan);
            _knownTypes.Add(typeof(DataConstant));
            _knownTypes.Add(typeof(DataContext));
            _knownTypes.Add(typeof(Dictionary<string, object>));
            _isDirty = true;
        }

        #endregion

        #region Implementation of ISerializer

        public void AddKnownType(Type type)
        {
            lock (_knownTypes)
            {
                if (_knownTypes.Add(type))
                    _isDirty = true;
            }
        }

        public bool RemoveKnownType(Type type)
        {
            lock (_knownTypes)
            {
                if (_knownTypes.Remove(type))
                {
                    _isDirty = false;
                    return true;
                }
            }
            return false;
        }

        public Stream Serialize(object item)
        {
            Should.NotBeNull(item, "item");
            AddKnownType(item.GetType());
            EnsureInitialized();
            item = new DataContainer { Data = item };
            var ms = new MemoryStream();
            _contractSerializer.WriteObject(ms, item);
            return ms;
        }

        public object Deserialize(Stream stream)
        {
            Should.NotBeNull(stream, "stream");
            EnsureInitialized();
            return ((DataContainer)_contractSerializer.ReadObject(stream)).Data;
        }

        #endregion

        #region Methods

        private void AddKnownTypes(IEnumerable<Assembly> assemblies)
        {
            foreach (Assembly assembly in assemblies)
            {
                foreach (Type type in assembly.SafeGetTypes(false))
                {
#if PCL_WINRT
                    var typeInfo = type.GetTypeInfo();
                    if (!typeInfo.IsAbstract && !typeInfo.IsGenericTypeDefinition && type.IsDefined(typeof(DataContractAttribute), true))
#else
                    if (!type.IsAbstract && !type.IsGenericTypeDefinition && type.IsDefined(typeof(DataContractAttribute), true))
#endif
                        _knownTypes.Add(type);
                }
            }
        }

        private void EnsureInitialized()
        {
            if (!_isDirty)
                return;
            lock (_knownTypes)
            {
                if (_isDirty)
                {
                    _contractSerializer = new DataContractSerializer(typeof(DataContainer), _knownTypes);
                    _isDirty = false;
                }
            }
        }

        #endregion
    }
}
