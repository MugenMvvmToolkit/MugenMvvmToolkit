#region Copyright

// ****************************************************************************
// <copyright file="BootstrapCodeBuilder.cs">
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Infrastructure
{
    public class BootstrapCodeBuilder : IBootstrapCodeBuilder
    {
        #region Nested types

        public struct CodeEntry
        {
            #region Fields

            public readonly string Tag;
            public readonly string Code;
            public readonly int Line;
            public readonly int Priority;

            #endregion

            #region Constructors

            public CodeEntry(string tag, string code, int line, int priority)
            {
                Tag = tag;
                Code = code;
                Line = line;
                Priority = priority;
            }

            #endregion
        }

        public struct CodeBuilderResult
        {
            #region Fields

            public static readonly CodeBuilderResult Empty = new CodeBuilderResult(MugenMvvmToolkit.Empty.Array<string>());
            public readonly IList<string> GeneratedCode;

            #endregion

            #region Constructors

            public CodeBuilderResult(IList<string> generatedCode)
            {
                GeneratedCode = generatedCode;
            }

            #endregion

            #region Methods

            public override string ToString()
            {
                if (GeneratedCode == null)
                    return string.Empty;
                return string.Join(Environment.NewLine, GeneratedCode);
            }

            #endregion
        }

        #endregion

        #region Fields

        private readonly List<CodeEntry> _entries;
        private int _lineNumber;
        private const string ContextParameter = "MugenMvvmToolkit.Interfaces.Models.IModuleContext context";

        #endregion

        #region Constructors

        public BootstrapCodeBuilder()
        {
            _entries = new List<CodeEntry>();
        }

        #endregion

        #region Implementation of interfaces

        public void Append(string tag, string code, int priority = ApplicationSettings.CodeBuilderNormalPriority)
        {
            lock (_entries)
            {
                _entries.Add(new CodeEntry(tag, code, _lineNumber++, priority));
            }
            Updated?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region Methods

        public IList<CodeEntry> GetCodeEntries()
        {
            lock (_entries)
            {
                return _entries.ToList();
            }
        }

        public CodeBuilderResult Build(string methodName = "PrecompiledBoot")
        {
            var code = new List<string>();
            List<string> rawCode;
            lock (_entries)
            {
                rawCode = _entries.OrderByDescending(entry => entry.Priority).ThenBy(entry => entry.Line).Select(entry => entry.Code).ToList();
            }
            if (rawCode.Count == 0)
                return CodeBuilderResult.Empty;
            code.Add($"public static void {methodName}({ContextParameter})");
            code.Add("{");
            code.AddRange(rawCode);
            code.Add("}");
            return new CodeBuilderResult(code);
        }

        public CodeBuilderResult BuildBootstrapper(string className = "Bootstrapper", string methodName = "PrecompiledBoot")
        {
            var result = BuildMethods(methodName);
            if (result.GeneratedCode.Count == 0)
                return result;
            var code = new List<string>
            {
                "partial class " + className,
                "{",
                $"protected override IList<{typeof(Assembly).FullName}> GetAssemblies()",
                "{",
                "#if DEBUG",
                "return base.GetAssemblies();",
                "#else",
                $"return {typeof(Empty).FullName}.{nameof(Empty.Array)}<{typeof(Assembly).FullName}>();",
                "#endif",
                "}"
            };
            code.Add(Environment.NewLine);
            code.AddRange(result.GeneratedCode);
            code.Add("}");
            return new CodeBuilderResult(code);
        }

        public CodeBuilderResult BuildMethods(string methodName = "PrecompiledBoot")
        {
            var code = new List<string>();
            List<IGrouping<string, CodeEntry>> rawCode;
            lock (_entries)
            {
                rawCode = _entries.GroupBy(entry => entry.Tag).ToList();
            }
            if (rawCode.Count == 0)
                return CodeBuilderResult.Empty;
            var methods = new List<KeyValuePair<int, string>>();
            foreach (var codeEntry in rawCode)
            {
                methods.Add(new KeyValuePair<int, string>(codeEntry.Max(entry => entry.Priority), codeEntry.Key + "Generated(context);"));
                code.Add($"private static void {codeEntry.Key}Generated({ContextParameter})");
                code.Add("{");
                code.AddRange(codeEntry.OrderByDescending(entry => entry.Priority).ThenBy(entry => entry.Line).Select(entry => entry.Code));
                code.Add("}");
                code.Add(Environment.NewLine);
            }

            code.Add($"public static void {methodName}(MugenMvvmToolkit.Interfaces.Models.IModuleContext context)");
            code.Add("{");
            foreach (var method in methods.OrderByDescending(pair => pair.Key))
                code.Add(method.Value);
            code.Add("}");
            return new CodeBuilderResult(code);
        }

        #endregion

        #region Events

        public event EventHandler<BootstrapCodeBuilder, EventArgs> Updated;

        #endregion
    }
}