using System;
using System.Runtime.Serialization;
using MugenMvvm.Attributes;
using MugenMvvm.Constants;

namespace MugenMvvm.Enums
{
    [Serializable]
    [DataContract(Namespace = BuildConstant.DataContractNamespace)]
    public class LogLevel : EnumBase<LogLevel, string>
    {
        #region Fields

        public static readonly LogLevel Trace = new LogLevel(nameof(Trace));
        public static readonly LogLevel Debug = new LogLevel(nameof(Debug));
        public static readonly LogLevel Info = new LogLevel(nameof(Info));
        public static readonly LogLevel Warning = new LogLevel(nameof(Warning));
        public static readonly LogLevel Error = new LogLevel(nameof(Error));
        public static readonly LogLevel Fatal = new LogLevel(nameof(Fatal));

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        protected LogLevel()
        {
        }

        public LogLevel(string value) : base(value)
        {
        }

        #endregion
    }
}