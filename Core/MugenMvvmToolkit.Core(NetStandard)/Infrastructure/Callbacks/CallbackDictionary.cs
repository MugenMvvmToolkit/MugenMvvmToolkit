using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using MugenMvvmToolkit.Collections;

namespace MugenMvvmToolkit.Infrastructure.Callbacks
{
    //NOTE we cannot use default dictionary, because MONO cannot deserialize it correctly.
    [DataContract(Namespace = ApplicationSettings.DataContractNamespace, IsReference = true)]
    [Serializable]
    public sealed class CallbackDictionary : LightDictionaryBase<string, List<object>>
    {
        #region Constructors

        public CallbackDictionary()
            : base(true)
        {
        }

        #endregion

        #region Overrides of LightDictionaryBase<string,List<object>>

        protected override bool Equals(string x, string y)
        {
            return x.Equals(y, StringComparison.Ordinal);
        }

        protected override int GetHashCode(string key)
        {
            return key.GetHashCode();
        }

        #endregion
    }
}