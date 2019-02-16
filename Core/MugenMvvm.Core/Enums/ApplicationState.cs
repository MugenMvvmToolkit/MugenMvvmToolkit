using System;
using System.Runtime.Serialization;
using MugenMvvm.Attributes;
using MugenMvvm.Constants;
using MugenMvvm.Models;

namespace MugenMvvm.Enums
{
    [Serializable]
    [DataContract(Namespace = BuildConstants.DataContractNamespace)]
    public class ApplicationState : EnumBase<ApplicationState, int>
    {
        #region Fields

        public static readonly ApplicationState Active = new ApplicationState(1);
        public static readonly ApplicationState Background = new ApplicationState(2);

        #endregion

        #region Constructors

        public ApplicationState(int value) : base(value)
        {
        }

        [Preserve(Conditional = true)]
        protected ApplicationState()
        {
        }

        #endregion
    }
}