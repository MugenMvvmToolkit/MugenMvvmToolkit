using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Xml;
using MugenMvvm.Infrastructure.Internal;
using MugenMvvm.Infrastructure.Serialization;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Serialization;

namespace MugenMvvm.UnitTest.TestInfrastructure
{
    public class DataContractSerializerImpl : SerializerBase, IDataContractSurrogate
    {
        #region Fields

        private readonly DataContractSerializer _serializer;
        private readonly Dictionary<Type, KeyValuePair<ISurrogateProviderSerializerHandler, Type>> _typeToSurrogateHandler;

        #endregion

        #region Constructors

        public DataContractSerializerImpl(IServiceProvider serviceProvider, DataContractSerializerSettings? settings = null,
            IComponentCollection<ISerializerHandler>? handlers = null)
            : base(serviceProvider, handlers)
        {
            _typeToSurrogateHandler = new Dictionary<Type, KeyValuePair<ISurrogateProviderSerializerHandler, Type>>(MemberInfoEqualityComparer.Instance);
            if (settings == null)
            {
                settings = new DataContractSerializerSettings
                {
                    PreserveObjectReferences = true,
                    SerializeReadOnlyTypes = false
                };
            }

            settings.DataContractResolver = new DataContractResolverImpl(this);
            settings.DataContractSurrogate = this;
            _serializer = new DataContractSerializer(typeof(object), settings);
        }

        #endregion

        #region Properties

        public override bool IsOnSerializingSupported => true;

        public override bool IsOnSerializedSupported => false;

        public override bool IsOnDeserializingSupported => false;

        public override bool IsOnDeserializedSupported => true;

        #endregion

        #region Implementation of interfaces

        object IDataContractSurrogate.GetObjectToSerialize(object obj, Type targetType)
        {
            OnSerializing(obj);
            var surrogateProvider = GetSurrogateProviderInfo(obj.GetType()).Key;
            var serializableItem = obj;
            if (surrogateProvider != null)
            {
                serializableItem = surrogateProvider.GetObjectToSerialize(this, obj, CurrentSerializationContext);
                if (serializableItem != null && !ReferenceEquals(obj, serializableItem))
                    OnSerializing(serializableItem);
            }

            return SerializableNullValue.To(serializableItem);
        }

        object IDataContractSurrogate.GetDeserializedObject(object obj, Type targetType)
        {
            if (SerializableNullValue.IsNull(obj))
                return null;

            OnDeserialized(obj);
            var deserializedObject = obj;
            var surrogateProvider = GetSurrogateProviderInfo(obj.GetType()).Key;
            if (surrogateProvider != null)
            {
                deserializedObject = surrogateProvider.GetDeserializedObject(this, obj, CurrentSerializationContext);
                if (deserializedObject != null && !ReferenceEquals(deserializedObject, obj))
                    OnDeserialized(deserializedObject);
            }

            return deserializedObject;
        }

        object IDataContractSurrogate.GetCustomDataToExport(MemberInfo memberInfo, Type dataContractType)
        {
            return null;
        }

        object IDataContractSurrogate.GetCustomDataToExport(Type clrType, Type dataContractType)
        {
            return null;
        }

        void IDataContractSurrogate.GetKnownCustomDataTypes(Collection<Type> customDataTypes)
        {
        }

        Type IDataContractSurrogate.GetReferencedTypeOnImport(string typeName, string typeNamespace, object customData)
        {
            return null;
        }

        CodeTypeDeclaration IDataContractSurrogate.ProcessImportedType(CodeTypeDeclaration typeDeclaration, CodeCompileUnit compileUnit)
        {
            return typeDeclaration;
        }

        Type IDataContractSurrogate.GetDataContractType(Type type)
        {
            return GetSurrogateProviderInfo(type).Value;
        }

        #endregion

        #region Methods

        protected override Stream SerializeInternal(object item)
        {
            var memoryStream = new MemoryStream();
            _serializer.WriteObject(memoryStream, item);
            return memoryStream;
        }

        protected override object DeserializeInternal(Stream stream)
        {
            return _serializer.ReadObject(stream);
        }

        protected override bool CanSerializeInternal(Type type, IReadOnlyMetadataContext metadata)
        {
            return base.CanSerializeInternal(type, metadata) || type.IsDefinedUnified(typeof(DataContractAttribute), true);
        }

        protected override void OnHandlerAdded(ISerializerHandler handler)
        {
            UpdateHandlers(handler);
        }

        protected override void OnHandlerRemoved(ISerializerHandler handler)
        {
            UpdateHandlers(handler);
        }

        private void UpdateHandlers(ISerializerHandler handler)
        {
            if (handler is ISurrogateProviderSerializerHandler)
            {
                lock (_typeToSurrogateHandler)
                {
                    _typeToSurrogateHandler.Clear();
                }
            }
        }

        private KeyValuePair<ISurrogateProviderSerializerHandler, Type> GetSurrogateProviderInfo(Type type)
        {
            lock (_typeToSurrogateHandler)
            {
                if (!_typeToSurrogateHandler.TryGetValue(type, out var value))
                {
                    if (TryGetSurrogateSerializerHandler(type, out var provider, out var surrogateType))
                        value = new KeyValuePair<ISurrogateProviderSerializerHandler, Type>(provider, surrogateType);
                    else
                        value = new KeyValuePair<ISurrogateProviderSerializerHandler, Type>(null, type);
                    _typeToSurrogateHandler[type] = value;
                }

                return value;
            }
        }

        #endregion

        #region Nested types

        private sealed class DataContractResolverImpl : DataContractResolver
        {
            #region Fields

            private readonly XmlDictionary _dictionary;
            private readonly DataContractSerializerImpl _serializer;

            #endregion

            #region Constructors

            public DataContractResolverImpl(DataContractSerializerImpl serializer)
            {
                _serializer = serializer;
                _dictionary = new XmlDictionary();
            }

            #endregion

            #region Methods

            public override Type ResolveName(string typeName, string typeNamespace, Type declaredType, DataContractResolver knownTypeResolver)
            {
                var type = knownTypeResolver.ResolveName(typeName, typeNamespace, declaredType, null);
                if (type == null)
                {
                    type = _serializer.TryResolveType(typeNamespace, typeName);
                    if (type == null)
                    {
                        try
                        {
                            type = Type.GetType(typeName + ", " + typeNamespace, false);
                        }
                        catch
                        {
                            ;
                        }
                    }
                }

                return type;
            }

            public override bool TryResolveType(Type dataContractType, Type declaredType, DataContractResolver knownTypeResolver, out XmlDictionaryString typeName,
                out XmlDictionaryString typeNamespace)
            {
                knownTypeResolver.TryResolveType(dataContractType, declaredType, null, out typeName, out typeNamespace);
                if (typeName == null || typeNamespace == null)
                {
                    if (!_serializer.TryResolveName(dataContractType, out var assemblyName, out var name))
                    {
                        assemblyName = dataContractType.GetAssemblyUnified().FullName;
                        name = dataContractType.FullName;
                    }

                    typeNamespace = _dictionary.Add(assemblyName);
                    typeName = _dictionary.Add(name);
                }

                return true;
            }

            #endregion
        }

        #endregion
    }
}