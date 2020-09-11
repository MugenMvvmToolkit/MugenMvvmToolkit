using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using MugenMvvm.Components;
using MugenMvvm.Extensions;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.Serialization;
using MugenMvvm.Interfaces.Serialization.Components;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace MugenMvvm.Common
{
    public class MugenNewtonsoftJsonSerializer : AttachableComponentBase<ISerializer>, ISerializerComponent, IContractResolver, ISerializationBinder, IHasPriority
    {
        #region Fields

        private readonly ISerializationBinder _binder;
        private readonly IContractResolver _contractResolver;
        private readonly SerializationCallback _deserializedCallback;
        private readonly SerializationCallback _deserializingCallback;

        private readonly Encoding _encoding;
        private readonly HashSet<Type> _handledContracts;
        private readonly JsonSerializer _jsonSerializer;
        private readonly SerializationCallback _serializedCallback;
        private readonly SerializationCallback _serializingCallback;
        private ISerializationContext? _serializationContext;

        #endregion

        #region Constructors

        public MugenNewtonsoftJsonSerializer(JsonSerializer? serializer = null, Encoding? encoding = null)
        {
            _handledContracts = new HashSet<Type>();
            _encoding = encoding ?? new UTF8Encoding(false);
            _jsonSerializer = serializer ?? new JsonSerializer();
            _jsonSerializer.ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor;
            _jsonSerializer.PreserveReferencesHandling = PreserveReferencesHandling.All;
            _jsonSerializer.TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Full;
            _jsonSerializer.TypeNameHandling = TypeNameHandling.All;
            _binder = _jsonSerializer.SerializationBinder;
            _jsonSerializer.SerializationBinder = this;
            _contractResolver = _jsonSerializer.ContractResolver;
            _jsonSerializer.ContractResolver = this;
            _serializingCallback = OnSerializing;
            _serializedCallback = OnSerialized;
            _deserializingCallback = OnDeserializing;
            _deserializedCallback = OnDeserialized;
        }

        #endregion

        #region Properties

        public bool WriteIndented { get; set; }

        public int Priority { get; set; } = SerializationComponentPriority.Serializer;

        #endregion

        #region Implementation of interfaces

        public JsonContract ResolveContract(Type type)
        {
            var contract = _contractResolver.ResolveContract(type);
            if (_handledContracts.Add(type))
            {
                var provider = Owner.GetComponents<ISurrogateProviderResolverComponent>().TryGetSurrogateProvider(Owner, type, _serializationContext);
                if (provider != null)
                    contract.Converter = new SurrogateJsonConverter(this, provider);

                contract.OnDeserializedCallbacks.Add(_deserializedCallback);
                contract.OnDeserializingCallbacks.Add(_deserializingCallback);
                contract.OnSerializingCallbacks.Add(_serializingCallback);
                contract.OnSerializedCallbacks.Add(_serializedCallback);
            }

            return contract;
        }

        public Type BindToType(string? assemblyName, string typeName) =>
            Owner.GetComponents<ITypeResolverComponent>().TryResolveType(Owner, assemblyName, typeName, _serializationContext) ?? _binder.BindToType(assemblyName, typeName);

        public void BindToName(Type serializedType, out string? assemblyName, out string? typeName)
        {
            if (!Owner.GetComponents<ITypeResolverComponent>().TryResolveName(Owner, serializedType, _serializationContext, out assemblyName, out typeName))
                _binder.BindToName(serializedType, out assemblyName, out typeName);
        }

        public bool TrySerialize(ISerializer serializer, object request, ISerializationContext serializationContext)
        {
            _serializationContext = serializationContext;
            var sw = new StreamWriter(serializationContext.Stream, _encoding, 1024, true);
            using var jsonWriter = new JsonTextWriter(sw) { Formatting = WriteIndented ? Formatting.Indented : Formatting.None };
            _jsonSerializer.Serialize(jsonWriter, request);
            _serializationContext = null;
            return true;
        }

        public bool TryDeserialize(ISerializer serializer, ISerializationContext serializationContext, out object? value)
        {
            _serializationContext = serializationContext;
            var sw = new StreamReader(serializationContext.Stream, _encoding, false, 1024, true);
            using var textReader = new JsonTextReader(sw);
            value = _jsonSerializer.Deserialize(textReader, typeof(NonSerializableObject));
            _serializationContext = null;
            return true;
        }

        #endregion

        #region Methods

        private void OnSerializing(object o, StreamingContext context = default) => Owner.GetComponents<ISerializerListener>().OnSerializing(Owner, o, _serializationContext!);

        private void OnSerialized(object o, StreamingContext context = default) => Owner.GetComponents<ISerializerListener>().OnSerialized(Owner, o, _serializationContext!);

        private void OnDeserialized(object o, StreamingContext context = default) => Owner.GetComponents<ISerializerListener>().OnDeserialized(Owner, o, _serializationContext!);

        private void OnDeserializing(object o, StreamingContext context = default) => Owner.GetComponents<ISerializerListener>().OnDeserializing(Owner, o, _serializationContext!);

        #endregion

        #region Nested types

        private sealed class SurrogateJsonConverter : JsonConverter
        {
            #region Fields

            private readonly MugenNewtonsoftJsonSerializer _serializer;
            private readonly ISurrogateProvider _surrogateProvider;

            #endregion

            #region Constructors

            public SurrogateJsonConverter(MugenNewtonsoftJsonSerializer serializer, ISurrogateProvider surrogateProvider)
            {
                _serializer = serializer;
                _surrogateProvider = surrogateProvider;
            }

            #endregion

            #region Methods

            public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
            {
                if (value != null)
                    _serializer.OnSerializing(value);
                var surrogate = _surrogateProvider.GetObjectToSerialize(value, _serializer._serializationContext!);
                serializer.Serialize(writer, surrogate);
                if (value != null)
                    _serializer.OnSerialized(value);
            }

            public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
            {
                var value = serializer.Deserialize(reader, _surrogateProvider.SurrogateType);
                var result = _surrogateProvider.GetDeserializedObject(value, _serializer._serializationContext!);
                if (result != null)
                {
                    _serializer.OnDeserializing(result);
                    _serializer.OnDeserialized(result);
                }

                return result;
            }

            public override bool CanConvert(Type objectType) => _surrogateProvider.SurrogateType.IsAssignableFrom(objectType);

            #endregion
        }

        #endregion
    }
}
