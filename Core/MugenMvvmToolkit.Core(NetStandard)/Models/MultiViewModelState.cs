using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using MugenMvvmToolkit.Interfaces.Models;

namespace MugenMvvmToolkit.Models
{
    //NOTE we cannot use default list, because MONO cannot deserialize it correctly.
    [DataContract(Namespace = ApplicationSettings.DataContractNamespace, IsReference = true), Serializable]
    internal sealed class MultiViewModelState
    {
        internal static readonly DataConstant<MultiViewModelState> ViewModelState;
        internal static readonly DataConstant<int> SelectedIndex;

        static MultiViewModelState()
        {
            var type = typeof(MultiViewModelState);
            ViewModelState = DataConstant.Create<MultiViewModelState>(type, nameof(ViewModelState), true);
            SelectedIndex = DataConstant.Create<int>(type, nameof(SelectedIndex));
        }

        [DataMember(Name = "s")]
        public List<IDataContext> State;
    }
}