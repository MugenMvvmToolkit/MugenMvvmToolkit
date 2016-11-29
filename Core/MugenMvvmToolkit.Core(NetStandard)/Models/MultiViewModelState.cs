#region Copyright

// ****************************************************************************
// <copyright file="MultiViewModelState.cs">
// Copyright (c) 2012-2016 Vyacheslav Volkov
// </copyright>
// ****************************************************************************
// <author>Vyacheslav Volkov</author>
// <email>vvs0205@outlook.com</email>
// <project>MugenMvvmToolkit</project>
// <web>https://github.com/MugenMvvmToolkit/MugenMvvmToolkit</web>
// <license>
// See license.txt in this solution or http://opensource.org/licenses/MS-PL
// </license>
// ****************************************************************************

#endregion

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using MugenMvvmToolkit.Attributes;
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

        [DataMember(Name = "s"), Preserve(Conditional = true)]
        public List<IDataContext> State;
    }
}