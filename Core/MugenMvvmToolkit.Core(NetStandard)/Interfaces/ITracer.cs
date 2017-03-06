#region Copyright

// ****************************************************************************
// <copyright file="ITracer.cs">
// Copyright (c) 2012-2017 Vyacheslav Volkov
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

using JetBrains.Annotations;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Interfaces
{
    public interface ITracer
    {
        void TraceViewModel(ViewModelLifecycleType lifecycleType, [NotNull] IViewModel viewModel);

        void Trace(TraceLevel level, string message);

        [StringFormatMethod("format")]
        void Trace(TraceLevel level, string format, params object[] args);
    }
}
