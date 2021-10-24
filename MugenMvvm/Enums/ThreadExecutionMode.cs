using System;
using System.Runtime.Serialization;
using MugenMvvm.Attributes;
using MugenMvvm.Constants;

namespace MugenMvvm.Enums
{
    [Serializable]
    [DataContract(Namespace = InternalConstant.DataContractNamespace)]
    public class ThreadExecutionMode : EnumBase<ThreadExecutionMode, string>
    {
        public static readonly ThreadExecutionMode Current = new(nameof(Current));
        public static readonly ThreadExecutionMode Main = new(nameof(Main)) {IsSynchronized = true};
        public static readonly ThreadExecutionMode MainAsync = new(nameof(MainAsync)) {IsSynchronized = true};
        public static readonly ThreadExecutionMode Background = new(nameof(Background));
        public static readonly ThreadExecutionMode BackgroundAsync = new(nameof(BackgroundAsync));

        public ThreadExecutionMode(string value, string? name = null) : base(value, name)
        {
        }

        [Preserve(Conditional = true)]
        protected ThreadExecutionMode()
        {
        }

        public bool IsSynchronized { get; set; }
    }
}