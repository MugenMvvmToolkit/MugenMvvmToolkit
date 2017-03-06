#region Copyright

// ****************************************************************************
// <copyright file="LazySerializableContainer.cs">
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

using System.IO;
using System.Runtime.Serialization;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.UWP.Models
{
    [DataContract]
    public sealed class LazySerializableContainer
    {
        #region Fields

        [IgnoreDataMember]
        private readonly ISerializer _serializer;

        [IgnoreDataMember]
        private byte[] _bytes;

        [IgnoreDataMember]
        private IDataContext _context;

        #endregion

        #region Constructors

        //Only for serialization
        public LazySerializableContainer()
        {
        }

        public LazySerializableContainer(ISerializer serializer, IDataContext context)
        {
            _serializer = serializer;
            _context = context;
        }

        #endregion

        #region Properties

        [DataMember(Name = "b")]
        internal byte[] Bytes
        {
            get
            {
                if (_bytes == null && _context.Count != 0)
                    _bytes = _serializer.Serialize(_context).ToArray();
                return _bytes;
            }
            set { _bytes = value; }
        }

        #endregion

        #region Methods

        public IDataContext GetContext(ISerializer serializer)
        {
            if (_context == null)
            {
                if (_bytes == null)
                    return DataContext.Empty;
                using (var ms = new MemoryStream(_bytes))
                {
                    _context = (IDataContext) serializer.Deserialize(ms);
                }
            }
            return _context;
        }

        #endregion
    }
}