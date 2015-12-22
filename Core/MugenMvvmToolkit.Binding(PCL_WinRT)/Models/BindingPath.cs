#region Copyright

// ****************************************************************************
// <copyright file="BindingPath.cs">
// Copyright (c) 2012-2015 Vyacheslav Volkov
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
using MugenMvvmToolkit.Binding.Interfaces.Models;

namespace MugenMvvmToolkit.Binding.Models
{
    public sealed class BindingPath : IBindingPath
    {
        #region Fields

        public static readonly IBindingPath None;
        public static readonly IBindingPath Empty;
        public static readonly IBindingPath DataContext;

        private readonly string _path;
        private string[] _items;
        private readonly bool _isEmpty;
        private readonly bool _isSingle;

        #endregion

        #region Constructors

        static BindingPath()
        {
            None = new BindingPath("##none##");
            Empty = new BindingPath(string.Empty);
            DataContext = new BindingPath(AttachedMemberConstants.DataContext);
        }

        internal BindingPath(string path)
        {
            const StringComparison comparison = StringComparison.Ordinal;
            if (string.IsNullOrEmpty(path))
            {
                _path = string.Empty;
                _items = MugenMvvmToolkit.Empty.Array<string>();
                _isEmpty = true;
                return;
            }
            _path = path;
            if (path.IndexOf(".", comparison) < 0 && path.IndexOf("[", comparison) < 0)
            {
                _isSingle = true;
                return;
            }
            string[] strings = path.Split(new[] { "." }, StringSplitOptions.RemoveEmptyEntries);
            var items = new List<string>();
            for (int index = 0; index < strings.Length; index++)
            {
                string s = strings[index];
                int start = s.IndexOf('[');
                int end = s.IndexOf(']');
                if (start <= 0 || end < 0)
                {
                    items.Add(s.Trim());
                    continue;
                }
                string indexer = s.Substring(start, end - start + 1).Trim();
                items.Add(s.Substring(0, start).Trim());
                items.Add(indexer);
            }
            _path = path;
            _items = items.ToArray();
        }

        #endregion

        #region Implementation of IBindingPath

        public string Path => _path;

        public IList<string> Parts
        {
            get
            {
                if (_items == null)
                    _items = new[] { _path };
                return _items;
            }
        }

        public bool IsEmpty => _isEmpty;

        public bool IsSingle => _isSingle;

        #endregion

        #region Overrides of Object

        public override string ToString()
        {
            return _path;
        }

        #endregion
    }
}
