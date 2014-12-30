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
using JetBrains.Annotations;
using MugenMvvmToolkit.Binding.Interfaces.Models;

namespace MugenMvvmToolkit.Binding.Models
{
    /// <summary>
    ///     Represents a data structure for describing a member as a path below another member, or below an owning type.
    /// </summary>
    public sealed class BindingPath : IBindingPath
    {
        #region Fields

        /// <summary>
        /// Gets the non existing path.
        /// </summary>
        public static readonly IBindingPath None;

        /// <summary>
        /// Gets the empty path.
        /// </summary>
        public static readonly IBindingPath Empty;

        /// <summary>
        /// Gets the data context path.
        /// </summary>
        public static readonly IBindingPath DataContext;

        private static readonly Dictionary<string, IBindingPath> Cache;
        private readonly string _path;
        private string[] _items;
        private readonly bool _isEmpty;
        private readonly bool _isSingle;

        #endregion

        #region Constructors

        static BindingPath()
        {
            Cache = new Dictionary<string, IBindingPath>();
            None = Create("##none##");
            Empty = Create(string.Empty);
            DataContext = Create(AttachedMemberConstants.DataContext);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="BindingPath" /> class.
        /// </summary>
        private BindingPath(string path)
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

        /// <summary>
        ///     Gets the string that describes the path.
        /// </summary>
        public string Path
        {
            get { return _path; }
        }

        /// <summary>
        ///     Gets the path members.
        /// </summary>
        public IList<string> Parts
        {
            get
            {
                if (_items == null)
                    _items = new[] { _path };
                return _items;
            }
        }

        /// <summary>
        ///     Gets the value that indicates that path is empty.
        /// </summary>
        public bool IsEmpty
        {
            get { return _isEmpty; }
        }

        /// <summary>
        ///     Gets the value that indicates that path has a single item.
        /// </summary>
        public bool IsSingle
        {
            get { return _isSingle; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Creates a new instance of the <see cref="BindingPath" /> class.
        /// </summary>
        [NotNull]
        public static IBindingPath Create([NotNull]string path)
        {
            lock (Cache)
            {
                IBindingPath value;
                if (!Cache.TryGetValue(path, out value))
                {
                    value = new BindingPath(path);
                    Cache[path] = value;
                }
                return value;
            }
        }

        /// <summary>
        /// Creates a new instance of the <see cref="BindingPath" /> class.
        /// </summary>
        [NotNull]
        public static IBindingPath Create([NotNull]string path, bool useCache)
        {
            if (useCache)
                return Create(path);
            return new BindingPath(path);
        }

        #endregion

        #region Overrides of Object

        /// <summary>
        ///     Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        ///     A string that represents the current object.
        /// </returns>
        public override string ToString()
        {
            return _path;
        }

        #endregion
    }
}