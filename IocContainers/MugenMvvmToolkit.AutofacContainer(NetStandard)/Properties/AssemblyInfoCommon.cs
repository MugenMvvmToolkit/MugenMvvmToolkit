#region Copyright

// ****************************************************************************
// <copyright file="AssemblyInfoCommon.cs">
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

using System.Reflection;
using MugenMvvmToolkit;
using MugenMvvmToolkit.Attributes;

[assembly: AssemblyCompany(ApplicationSettings.AssemblyCompany)]
[assembly: AssemblyCopyright(ApplicationSettings.AssemblyCopyright)]
[assembly: AssemblyVersion(ApplicationSettings.AssemblyVersion)]
[assembly: AssemblyFileVersion(ApplicationSettings.AssemblyVersion)]
[assembly: LinkerSafe]
[assembly: AssemblyKeyFile(@"..\..\Solution Items\sigkey.snk")]