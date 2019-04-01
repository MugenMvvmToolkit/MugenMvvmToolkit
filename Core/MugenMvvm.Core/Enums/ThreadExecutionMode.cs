using System;
using System.Runtime.Serialization;
using MugenMvvm.Attributes;
using MugenMvvm.Constants;
using MugenMvvm.Models;

namespace MugenMvvm.Enums
{
    [Serializable]
    [DataContract(Namespace = BuildConstants.DataContractNamespace)]
    public class ThreadExecutionMode : EnumBase<ThreadExecutionMode, int>
    {
        #region Fields

        public static readonly ThreadExecutionMode Main = new ThreadExecutionMode(1);
        public static readonly ThreadExecutionMode MainAsync = new ThreadExecutionMode(2);
        public static readonly ThreadExecutionMode Background = new ThreadExecutionMode(3);
        public static readonly ThreadExecutionMode Current = new ThreadExecutionMode(4);

        #endregion

        #region Constructors

        public ThreadExecutionMode(int value) : base(value)
        {
        }

        [Preserve(Conditional = true)]
        protected ThreadExecutionMode()
        {
        }

        #endregion
    }
}