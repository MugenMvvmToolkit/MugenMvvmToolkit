#region Copyright

// ****************************************************************************
// <copyright file="IIocParameter.cs">
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

using MugenMvvmToolkit.Models.IoC;

namespace MugenMvvmToolkit.Interfaces
{
    public interface IIocParameter
    {
        string Name { get; }

        object Value { get; }

        IocParameterType ParameterType { get; }
    }
}
