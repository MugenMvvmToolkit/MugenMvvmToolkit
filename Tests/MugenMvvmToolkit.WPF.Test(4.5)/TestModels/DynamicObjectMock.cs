#region Copyright

// ****************************************************************************
// <copyright file="DynamicObjectMock.cs">
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
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.Models;

namespace MugenMvvmToolkit.Test.TestModels
{
    public class DynamicObjectMock : IDynamicObject
    {
        #region Properties

        public Func<string, IEventListener, IDisposable> TryObserve { get; set; }

        public Func<string, IList<object>, object> GetMember { get; set; }

        public Func<string, IList<object>, IList<Type>, IDataContext, object> InvokeMember { get; set; }

        public Func<IList<object>, IDataContext, object> GetIndex { get; set; }

        public Action<IList<object>, IDataContext> SetIndex { get; set; }

        public Action<string, IList<object>> SetMember { get; set; }

        #endregion

        #region Implementation of IDynamicObject

        IDisposable IDynamicObject.TryObserve(string member, IEventListener listener)
        {
            if (TryObserve == null)
                return null;
            return TryObserve(member, listener);
        }

        object IDynamicObject.GetMember(string member, IList<object> args)
        {
            if (GetMember == null)
                return null;
            return GetMember(member, args);
        }

        void IDynamicObject.SetMember(string member, IList<object> args)
        {
            if (SetMember != null)
                SetMember(member, args);
        }

        object IDynamicObject.InvokeMember(string member, IList<object> args, IList<Type> typeArgs, IDataContext context)
        {
            if (InvokeMember == null)
                return null;
            return InvokeMember(member, args, typeArgs, context);
        }

        object IDynamicObject.GetIndex(IList<object> indexes, IDataContext context)
        {
            if (GetIndex == null)
                return null;
            return GetIndex(indexes, context);
        }

        void IDynamicObject.SetIndex(IList<object> indexes, IDataContext context)
        {
            if (SetIndex != null)
                SetIndex(indexes, context);
        }

        #endregion
    }
}
