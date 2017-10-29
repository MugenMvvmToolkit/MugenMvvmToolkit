#region Copyright

// ****************************************************************************
// <copyright file="Serializer.cs">
// Copyright (c) 2012-2017 Vyacheslav Volkov
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
using MugenMvvmToolkit.Attributes;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Infrastructure
{
    public class Serializer : ISerializer
    {
        #region Fields

        private readonly HashSet<Type> _knownTypes;
        private IEnumerable<Assembly> _assemblies;
        private DataContractSerializer _contractSerializer;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public Serializer(IEnumerable<Assembly> assembliesToScan)
        {
            _knownTypes = new HashSet<Type>();
            _assemblies = ApplicationSettings.SerializerDisableAutoRegistration ? null : assembliesToScan;
        }

        #endregion

        #region Implementation of ISerializer

        public Stream Serialize(object item)
        {
            Should.NotBeNull(item, nameof(item));
            EnsureInitialized();
            if (_knownTypes.Add(item.GetType()))
                _contractSerializer = new DataContractSerializer(typeof(SerializerDataContainer), _knownTypes);
            item = new SerializerDataContainer { Data = item };
            var ms = new MemoryStream();
            _contractSerializer.WriteObject(ms, item);
            return ms;
        }

        public object Deserialize(Stream stream)
        {
            Should.NotBeNull(stream, nameof(stream));
            EnsureInitialized();
            return ((SerializerDataContainer)_contractSerializer.ReadObject(stream)).Data;
        }

        public bool IsSerializable(Type type)
        {
#if NET_STANDARD
            return type == typeof(string) || type.IsDefined(typeof(DataContractAttribute), false) || type.GetTypeInfo().IsPrimitive;
#else
            return type == typeof(string) || type.IsDefined(typeof(DataContractAttribute), false) || type.IsPrimitive;
#endif
        }

        #endregion

        #region Methods

        public void AddKnownType(string fullName)
        {
            AddKnownType(Type.GetType(fullName, true));
        }

        public void AddKnownType(Type type)
        {
            if (_contractSerializer != null)
                throw ExceptionManager.ObjectInitialized(nameof(Serialize), null);
            _knownTypes.Add(type);
        }

        private void EnsureInitialized()
        {
            if (_contractSerializer == null)
            {
                lock (_knownTypes)
                {
                    if (_contractSerializer == null)
                    {
                        if (_assemblies != null)
                            AddKnownTypes(_assemblies);
                        _knownTypes.Add(typeof(DataConstant));
                        _knownTypes.Add(typeof(DataContext));
                        _knownTypes.Add(typeof(Dictionary<string, object>));
                        _contractSerializer = new DataContractSerializer(typeof(SerializerDataContainer), _knownTypes);
                        _assemblies = null;
                    }
                }
            }
        }

        private void AddKnownTypes(IEnumerable<Assembly> assemblies)
        {
            var builder = ToolkitServiceProvider.BootstrapCodeBuilder;
            if (!ApplicationSettings.SerializerDisableAutoRegistration)
                builder?.AppendStatic(nameof(ApplicationSettings), $"{typeof(ApplicationSettings).FullName}.{nameof(ApplicationSettings.SerializerDisableAutoRegistration)} = true;");
            bool initialized = false;
            foreach (Assembly assembly in assemblies)
            {
                foreach (Type type in assembly.SafeGetTypes(!ToolkitServiceProvider.IsDesignMode))
                {
#if NET_STANDARD
                    var typeInfo = type.GetTypeInfo();
                    if (!typeInfo.IsAbstract && !typeInfo.IsGenericTypeDefinition && type.IsDefined(typeof(DataContractAttribute), true))
#else
                    if (!type.IsAbstract && !type.IsGenericTypeDefinition && type.IsDefined(typeof(DataContractAttribute), true))
#endif
                    {
                        if (!_knownTypes.Add(type) || builder == null)
                            continue;
                        if (!initialized)
                        {
                            builder.Append(typeof(Serializer).Name, $"var serializer = ({typeof(Serializer).FullName}) context.{nameof(IModuleContext.IocContainer)}.Get(typeof({typeof(ISerializer).FullName}));");
                            initialized = true;
                        }
                        builder.Append(nameof(Serializer), type.IsPublic()
                            ? $"serializer.{nameof(AddKnownType)}(typeof({type.GetPrettyName()}));"
                            : $"serializer.{nameof(AddKnownType)}(\"{type.AssemblyQualifiedName}\");");
                    }
                }
            }
        }

        #endregion
    }
}