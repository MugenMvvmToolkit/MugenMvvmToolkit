using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
#if WPF
using System.Runtime.Serialization.Formatters.Binary;
#endif
using System.Runtime.Serialization.Json;
using System.Text;
using System.Xml.Serialization;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Should;

namespace MugenMvvmToolkit.Test
{
    [TestClass]
    public abstract class SerializationTestBase<T> : TestBase
    {
        #region Methods

        [TestMethod]
        public virtual void TestXmlSerialization()
        {
            object obj = GetObject();
            using (var memStream = new MemoryStream())
            {
                var serializer = new XmlSerializer(obj.GetType(), GetKnownTypes().ToArray());
                serializer.Serialize(memStream, obj);
                memStream.Position = 0;
                Print(memStream);
                var result = serializer.Deserialize(memStream);
                result.ShouldBeType(obj.GetType());
                AssertObject((T)result);
            }
        }

        [TestMethod]
        public virtual void TestDataContractSerialization()
        {
            object obj = GetObject();
            using (var memStream = new MemoryStream())
            {
                var serializer = new DataContractSerializer(obj.GetType(), GetKnownTypes());
                serializer.WriteObject(memStream, obj);
                memStream.Position = 0;
                Print(memStream);
                var result = serializer.ReadObject(memStream);
                result.ShouldBeType(obj.GetType());
                AssertObject((T)result);
            }
        }

#if WPF
        [TestMethod]
        public virtual void TestBinarySerialization()
        {
            object obj = GetObject();
            using (var memStream = new MemoryStream())
            {
                var serializer = new BinaryFormatter();
                serializer.Serialize(memStream, obj);
                memStream.Position = 0;
                Print(memStream);
                var result = serializer.Deserialize(memStream);
                result.ShouldBeType(obj.GetType());
                AssertObject((T)result);
            }
        }
#endif
        protected abstract T GetObject();

        protected abstract void AssertObject(T deserializedObj);

        protected virtual IEnumerable<Type> GetKnownTypes()
        {
            return Enumerable.Empty<Type>();
        }

        private static void Print(Stream stream)
        {
            stream.Position = 0;
            Tracer.Warn(new StreamReader(stream, Encoding.UTF8).ReadToEnd());
            stream.Position = 0;
        }

        #endregion
    }
}
