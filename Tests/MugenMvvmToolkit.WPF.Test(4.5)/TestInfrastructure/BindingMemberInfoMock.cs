using System;
using System.Reflection;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Binding.Models;

namespace MugenMvvmToolkit.Test.TestInfrastructure
{
    public class BindingMemberInfoMock : IBindingMemberInfo
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public BindingMemberInfoMock()
        {
            Type = typeof(object);
            Path = string.Empty;
            MemberType = BindingMemberType.Unset;
        }

        #endregion

        #region Properties

        public Func<object, object[], object> GetValue;

        public Action<object, object[]> SetValue;

        public Func<object, IEventListener, IDisposable> TryObserveMember;

        #endregion

        #region Implementation of IBindingMemberInfo

        /// <summary>
        ///     Gets the path of member.
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        ///     Gets the type of member.
        /// </summary>
        public Type Type { get; set; }

        /// <summary>
        ///     Gets the underlying member, if any.
        /// </summary>
        public MemberInfo Member { get; set; }

        /// <summary>
        ///     Gets the member type.
        /// </summary>
        public BindingMemberType MemberType { get; set; }

        /// <summary>
        ///     Gets a value indicating whether the member can be read.
        /// </summary>
        public bool CanRead { get; set; }

        /// <summary>
        ///     Gets a value indicating whether the property can be written to.
        /// </summary>
        public bool CanWrite { get; set; }

        /// <summary>
        ///     Returns the member value of a specified object.
        /// </summary>
        /// <param name="source">The object whose member value will be returned.</param>
        /// <param name="args">Optional values for members.</param>
        /// <returns>The member value of the specified object.</returns>
        object IBindingMemberInfo.GetValue(object source, object[] args)
        {
            return GetValue(source, args);
        }

        /// <summary>
        ///     Sets the member value of a specified object.
        /// </summary>
        /// <param name="source">The object whose member value will be set.</param>
        /// <param name="args">Optional values for members..</param>
        object IBindingMemberInfo.SetValue(object source, object[] args)
        {
            SetValue(source, args);
            return null;
        }

        /// <summary>
        ///     Attempts to track the value change.
        /// </summary>
        IDisposable IBindingMemberInfo.TryObserve(object source, IEventListener listener)
        {
            return TryObserveMember(source, listener);
        }

        #endregion
    }
}