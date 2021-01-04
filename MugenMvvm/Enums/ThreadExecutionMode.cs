using System;
using System.Runtime.Serialization;
using MugenMvvm.Attributes;
using MugenMvvm.Constants;

namespace MugenMvvm.Enums
{
    [Serializable]
    [DataContract(Namespace = BuildConstant.DataContractNamespace)]
    public class ThreadExecutionMode : EnumBase<ThreadExecutionMode, int>
    {
        #region Fields

        public static readonly ThreadExecutionMode Current = new(0);
        public static readonly ThreadExecutionMode Main = new(1) {IsSynchronized = true};
        public static readonly ThreadExecutionMode MainAsync = new(2) {IsSynchronized = true};
        public static readonly ThreadExecutionMode Background = new(3);
        public static readonly ThreadExecutionMode BackgroundAsync = new(4);

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        protected ThreadExecutionMode()
        {
        }

        public ThreadExecutionMode(int value, string? name = null) : base(value, name)
        {
        }

        #endregion

        #region Properties

        public bool IsSynchronized { get; set; }

        #endregion
    }
}