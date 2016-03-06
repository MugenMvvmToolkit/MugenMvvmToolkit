#region Copyright

// ****************************************************************************
// <copyright file="IBootstrapCodeBuilder.cs">
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

using JetBrains.Annotations;

namespace MugenMvvmToolkit.Interfaces
{
    public interface IBootstrapCodeBuilder
    {
        #region Methods

        void AppendStatic([NotNull] string tag, [NotNull] string code);

        void Append([NotNull] string tag, [NotNull] string code);

        #endregion
    }
}