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

        [DataContract(Namespace = ApplicationSettings.DataContractNamespace, IsReference = true, Name = "sdc"), Serializable]
        internal sealed class DataContainer
        {
            #region Properties

            [DataMember(Name = "d")]
            public object Data { get; set; }

            #endregion
        }

        #endregion

        #region Fields

        private readonly HashSet<Type> _knownTypes;
        private DataContractSerializer _contractSerializer;

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
            _contractSerializer = new DataContractSerializer(typeof(DataContainer), _knownTypes);
        }

        #endregion

        #region Implementation of ISerializer

        public Stream Serialize(object item)
        {
            Should.NotBeNull(item, "item");
            if (_knownTypes.Add(item.GetType()))
                _contractSerializer = new DataContractSerializer(typeof(DataContainer), _knownTypes);
            item = new DataContainer { Data = item };
            var ms = new MemoryStream();
            _contractSerializer.WriteObject(ms, item);
            return ms;
        }

        public object Deserialize(Stream stream)
        {
            Should.NotBeNull(stream, "stream");
            return ((DataContainer)_contractSerializer.ReadObject(stream)).Data;
        }

        public bool IsSerializable(Type type)
        {
#if PCL_WINRT
            return type == typeof(string) || type.IsDefined(typeof(DataContractAttribute), false) || type.GetTypeInfo().IsPrimitive;
#else
            return type == typeof(string) || type.IsDefined(typeof(DataContractAttribute), false) || type.IsPrimitive;
#endif
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

        #endregion
    }
}
