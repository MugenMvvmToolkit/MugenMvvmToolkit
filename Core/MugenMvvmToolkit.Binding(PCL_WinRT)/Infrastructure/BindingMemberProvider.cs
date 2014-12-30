#region Copyright

// ****************************************************************************
// <copyright file="BindingMemberProvider.cs">
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
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using JetBrains.Annotations;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Binding.Models;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Binding.Infrastructure
{
    /// <summary>
    ///     Represents the binding member provider.
    /// </summary>
    public class BindingMemberProvider : IBindingMemberProvider
    {
        #region Nested types

        [StructLayout(LayoutKind.Auto)]
        private struct CacheKey
        {
            #region Fields

            public readonly string Path;
            public readonly Type Type;
            public readonly int Hash;
            public readonly bool IgnoreAttachedMembers;

            #endregion

            #region Constructors

            public CacheKey(Type type, string path, bool ignoreAttachedMembers)
            {
                Type = type;
                if (path == null)
                    path = string.Empty;
                Path = path;
                IgnoreAttachedMembers = ignoreAttachedMembers;
                unchecked
                {
                    Hash = (((type.GetHashCode() * 397) ^ path.GetHashCode()) * 397) ^ ignoreAttachedMembers.GetHashCode();
                }
            }

            #endregion

            #region Overrides of Object

            public override string ToString()
            {
                return string.Format("Type: {0}, Path: {1}, IgnoreAttachedMembers: {2}", Type, Path, IgnoreAttachedMembers);
            }

            #endregion
        }

        private sealed class CacheKeyComparer : IEqualityComparer<CacheKey>
        {
            #region Fields

            public static readonly CacheKeyComparer Instance;

            #endregion

            #region Constructors

            static CacheKeyComparer()
            {
                Instance = new CacheKeyComparer();
            }

            private CacheKeyComparer()
            {
            }

            #endregion

            #region Implementation of IEqualityComparer<in CacheKey>

            /// <summary>
            ///     Determines whether the specified objects are equal.
            /// </summary>
            /// <returns>
            ///     true if the specified objects are equal; otherwise, false.
            /// </returns>
            public bool Equals(CacheKey x, CacheKey y)
            {
                return x.IgnoreAttachedMembers == y.IgnoreAttachedMembers &&
                       string.Equals(x.Path, y.Path, StringComparison.Ordinal) && x.Type.Equals(y.Type);
            }

            /// <summary>
            ///     Returns a hash code for the specified object.
            /// </summary>
            /// <returns>
            ///     A hash code for the specified object.
            /// </returns>
            /// <param name="obj">The <see cref="T:System.Object" /> for which a hash code is to be returned.</param>
            /// <exception cref="T:System.ArgumentNullException">
            ///     The type of <paramref name="obj" /> is a reference type and
            ///     <paramref name="obj" /> is null.
            /// </exception>
            public int GetHashCode(CacheKey obj)
            {
                return obj.Hash;
            }

            #endregion
        }

        #endregion

        #region Fields

        private readonly Dictionary<CacheKey, IBindingMemberInfo> _attachedMembers;
        private readonly Dictionary<CacheKey, IBindingMemberInfo> _explicitMembersCache;
        private readonly Dictionary<CacheKey, IBindingMemberInfo> _tempMembersCache;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="BindingMemberProvider" /> class.
        /// </summary>
        public BindingMemberProvider()
        {
            _attachedMembers = new Dictionary<CacheKey, IBindingMemberInfo>(CacheKeyComparer.Instance);
            _tempMembersCache = new Dictionary<CacheKey, IBindingMemberInfo>(CacheKeyComparer.Instance);
            _explicitMembersCache = new Dictionary<CacheKey, IBindingMemberInfo>(CacheKeyComparer.Instance);
            FieldFlags = MemberFlags.Public | MemberFlags.Instance | MemberFlags.NonPublic;
            EventFlags = MemberFlags.Public | MemberFlags.Instance;
            PropertyFlags = FieldFlags;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="BindingMemberProvider" /> class.
        /// </summary>
        public BindingMemberProvider([NotNull] BindingMemberProvider provider)
        {
            Should.NotBeNull(provider, "provider");
            _attachedMembers = provider._attachedMembers;
            _tempMembersCache = provider._tempMembersCache;
            _explicitMembersCache = provider._explicitMembersCache;
            FieldFlags = provider.FieldFlags;
            EventFlags = provider.EventFlags;
            PropertyFlags = provider.PropertyFlags;
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets the binding context member.
        /// </summary>
        public static IBindingMemberInfo BindingContextMember
        {
            get { return BindingMemberInfo.BindingContextMember; }
        }

        /// <summary>
        ///     Gets the unset member.
        /// </summary>
        public static IBindingMemberInfo Unset
        {
            get { return BindingMemberInfo.Unset; }
        }

        /// <summary>
        ///     Gets the empty member.
        /// </summary>
        public static IBindingMemberInfo Empty
        {
            get { return BindingMemberInfo.Empty; }
        }

        /// <summary>
        /// Gets or sets the member flags for field.
        /// </summary>
        public MemberFlags FieldFlags { get; set; }

        /// <summary>
        /// Gets or sets the member flags for event.
        /// </summary>
        public MemberFlags PropertyFlags { get; set; }

        /// <summary>
        /// Gets or sets the member flags for event.
        /// </summary>
        public MemberFlags EventFlags { get; set; }

        #endregion

        #region Implementation of IBindingMemberProvider

        /// <summary>
        ///     Gets an instance of <see cref="IBindingMemberInfo" /> using the source type and binding path.
        /// </summary>
        /// <param name="sourceType">The specified source type.</param>
        /// <param name="path">The specified binding path.</param>
        /// <param name="ignoreAttachedMembers">If <c>true</c> provider ignores attached members.</param>
        /// <param name="throwOnError">
        ///     true to throw an exception if the member cannot be found; false to return null. Specifying
        ///     false also suppresses some other exception conditions, but not all of them.
        /// </param>
        /// <returns>The instance of <see cref="IBindingMemberInfo" />.</returns>
        public IBindingMemberInfo GetBindingMember(Type sourceType, string path, bool ignoreAttachedMembers,
            bool throwOnError)
        {
            Should.NotBeNull(sourceType, "sourceType");
            Should.NotBeNull(path, "path");
            IBindingMemberInfo bindingMember;
            var key = new CacheKey(sourceType, path, ignoreAttachedMembers);
            lock (_tempMembersCache)
            {
                if (!_tempMembersCache.TryGetValue(key, out bindingMember))
                {
                    if (!ignoreAttachedMembers)
                        bindingMember = GetAttachedBindingMember(ref key);
                    if (bindingMember == null)
                    {
                        if (!_explicitMembersCache.TryGetValue(key, out bindingMember))
                        {
                            bindingMember = GetExplicitBindingMember(sourceType, path);
                            _explicitMembersCache[key] = bindingMember;
                        }
                    }

                    if (bindingMember == null)
                    {
                        foreach (var prefix in BindingServiceProvider.FakeMemberPrefixes)
                        {
                            if (path.StartsWith(prefix, StringComparison.Ordinal))
                            {
                                bindingMember = BindingMemberInfo.EmptyHasSetter;
                                break;
                            }
                        }
                    }
                    _tempMembersCache[key] = bindingMember;
                }
            }

            if (throwOnError && bindingMember == null)
                throw BindingExceptionManager.InvalidBindingMember(sourceType, path);
            return bindingMember;
        }

        /// <summary>
        ///     Registers the specified member.
        /// </summary>
        /// <param name="type">The specified type.</param>
        /// <param name="member">The specified member.</param>
        /// <param name="rewrite"><c>true</c> rewrite exist member, <c>false</c> throw an exception.</param>
        public void Register(Type type, IBindingMemberInfo member, bool rewrite)
        {
            Register(type, member.Path, member, rewrite);
        }

        /// <summary>
        ///     Registers the specified member.
        /// </summary>
        /// <param name="type">The specified type.</param>
        /// <param name="path">The path of member.</param>
        /// <param name="member">The specified member.</param>
        /// <param name="rewrite"><c>true</c> rewrite exist member, <c>false</c> throw an exception.</param>
        public void Register(Type type, string path, IBindingMemberInfo member, bool rewrite)
        {
            Should.NotBeNull(type, "type");
            Should.NotBeNullOrEmpty(path, "path");
            Should.NotBeNull(member, "member");
            lock (_attachedMembers)
            {
                var key = new CacheKey(type, path, false);
                if (_attachedMembers.ContainsKey(key))
                {
                    if (rewrite)
                        Tracer.Warn("The member {0} on type {1} has been overwritten", type, path);
                    else
                        throw BindingExceptionManager.DuplicateBindingMember(type, path);
                }
                _attachedMembers[key] = member;
            }
            lock (_tempMembersCache)
                _tempMembersCache.Clear();
            Tracer.Info("The attached property (path: {0}, type: {1}, target type: {2}) was registered.", path, member.Type, type);
        }

        /// <summary>
        ///     Unregisters the specified member using the type and member path.
        /// </summary>
        public bool Unregister(Type type, string path)
        {
            Should.NotBeNull(type, "type");
            bool removed;
            lock (_attachedMembers)
                removed = _attachedMembers.Remove(new CacheKey(type, path, false));
            if (removed)
            {
                lock (_tempMembersCache)
                    _tempMembersCache.Clear();
            }
            return removed;
        }

        /// <summary>
        ///     Unregisters the members using the type.
        /// </summary>
        public bool Unregister(Type type)
        {
            lock (_attachedMembers)
            {
                List<CacheKey> toRemove = null;
                foreach (var bindingMemberInfo in _attachedMembers)
                {
                    if (bindingMemberInfo.Key.Type != type)
                        continue;
                    if (toRemove == null)
                        toRemove = new List<CacheKey>();
                    toRemove.Add(bindingMemberInfo.Key);
                }
                if (toRemove == null)
                    return false;
                for (int index = 0; index < toRemove.Count; index++)
                    _attachedMembers.Remove(toRemove[index]);
            }
            lock (_tempMembersCache)
                _tempMembersCache.Clear();
            return true;
        }

        /// <summary>
        ///     Gets the list of attached members for the specified type.
        /// </summary>
        public ICollection<KeyValuePair<string, IBindingMemberInfo>> GetAttachedMembers(Type type)
        {
            Should.NotBeNull(type, "type");
            var names = new HashSet<KeyValuePair<string, IBindingMemberInfo>>();
            lock (_attachedMembers)
            {
                foreach (var member in _attachedMembers)
                {
                    if (member.Key.Type.IsAssignableFrom(type))
                        names.Add(new KeyValuePair<string, IBindingMemberInfo>(member.Key.Path, member.Value));
                }
            }
            return names;
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Gets an instance of <see cref="IBindingMemberInfo" /> using the source type and binding path.
        /// </summary>
        /// <param name="sourceType">The specified source type.</param>
        /// <param name="path">The specified binding path.</param>
        /// <returns>The instance of <see cref="IBindingMemberInfo" />.</returns>
        [CanBeNull]
        protected virtual IBindingMemberInfo GetExplicitBindingMember([NotNull] Type sourceType, [NotNull] string path)
        {
            path = path.Trim();
            if (typeof(IDynamicObject).IsAssignableFrom(sourceType))
                return new BindingMemberInfo(path);

            int indexerCounts = 0;
            if (path.StartsWith("[") && path.EndsWith("]"))
            {
                indexerCounts = 1;
                for (int index = 0; index < path.Length; index++)
                {
                    if (path[index] == ',')
                        indexerCounts++;
                }
            }

            var types = BindingReflectionExtensions.SelfAndBaseTypes(sourceType);
            foreach (var type in types)
            {
                if (indexerCounts == 0)
                {
                    PropertyInfo property = type.GetPropertyEx(path, PropertyFlags);
                    if (property != null)
                        return new BindingMemberInfo(path, property, sourceType);
                }
                else
                {
                    PropertyInfo property = type
                        .GetPropertiesEx(PropertyFlags)
                        .FirstOrDefault(info => info.GetIndexParameters().Length == indexerCounts);
                    if (property != null)
                        return new BindingMemberInfo(path, property, sourceType);

                    if (type.IsArray && type.GetArrayRank() == indexerCounts)
                        return new BindingMemberInfo(path, type);
                }
                EventInfo @event = type.GetEventEx(path, EventFlags);
                if (@event != null)
                    return new BindingMemberInfo(path, @event);

                FieldInfo field = type.GetFieldEx(path, FieldFlags);
                if (field != null)
                    return new BindingMemberInfo(path, field, sourceType);
            }
            return null;
        }


        /// <summary>
        ///     Gets an instance of <see cref="IBindingMemberInfo" /> using the source type and binding path.
        /// </summary>
        /// <param name="sourceType">The specified source type.</param>
        /// <param name="path">The specified binding path.</param>
        /// <returns>The instance of <see cref="IBindingMemberInfo" />.</returns>
        [CanBeNull]
        protected IBindingMemberInfo GetAttachedBindingMember([NotNull] Type sourceType, [NotNull] string path)
        {
            var key = new CacheKey(sourceType, path, false);
            return GetAttachedBindingMember(ref key);
        }

        /// <summary>
        ///     Gets an attached binding member.
        /// </summary>
        private IBindingMemberInfo GetAttachedBindingMember(ref CacheKey key)
        {
            IBindingMemberInfo bindingMember;
            lock (_attachedMembers)
            {
                _attachedMembers.TryGetValue(key, out bindingMember);
                if (bindingMember == null)
                {
                    List<KeyValuePair<Type, IBindingMemberInfo>> types = null;
                    foreach (var keyPair in _attachedMembers)
                    {
                        if (!IsAssignableFrom(keyPair.Key.Type, key.Type) || keyPair.Key.Path != key.Path)
                            continue;
                        if (types == null)
                            types = new List<KeyValuePair<Type, IBindingMemberInfo>>();
                        types.Add(new KeyValuePair<Type, IBindingMemberInfo>(keyPair.Key.Type, keyPair.Value));
                    }
                    if (types != null)
                        bindingMember = FindBestMember(types);
                }
            }
            if (bindingMember == null && BindingServiceProvider.DataContextMemberAliases.Contains(key.Path))
                return BindingMemberInfo.BindingContextMember;
            return bindingMember;
        }

        private static IBindingMemberInfo FindBestMember(List<KeyValuePair<Type, IBindingMemberInfo>> members)
        {
            if (members.Count == 0)
                return null;
            if (members.Count == 1)
                return members[0].Value;

            for (int i = 0; i < members.Count; i++)
            {
                KeyValuePair<Type, IBindingMemberInfo> memberValue = members[i];
#if PCL_WINRT
                bool isInterface = memberValue.Key.GetTypeInfo().IsInterface;
#else
                bool isInterface = memberValue.Key.IsInterface;
#endif
                for (int j = 0; j < members.Count; j++)
                {
                    if (i == j)
                        continue;
                    var pair = members[j];
                    if (isInterface && memberValue.Key.IsAssignableFrom(pair.Key))
                    {
                        members.RemoveAt(i);
                        i--;
                        break;
                    }
#if PCL_WINRT
                    if (pair.Key.GetTypeInfo().IsSubclassOf(memberValue.Key))
#else
                    if (pair.Key.IsSubclassOf(memberValue.Key))
#endif
                    {
                        members.RemoveAt(i);
                        i--;
                        break;
                    }
                }
            }
            return members[0].Value;
        }

        private static bool IsAssignableFrom(Type attachedMemberType, Type sourceType)
        {
#if PCL_WINRT
            if (attachedMemberType.GetTypeInfo().IsGenericTypeDefinition &&
                BindingReflectionExtensions.FindCommonType(attachedMemberType, sourceType) != null)
#else
            if (attachedMemberType.IsGenericTypeDefinition &&
                BindingReflectionExtensions.FindCommonType(attachedMemberType, sourceType) != null)
#endif
                return true;
            return attachedMemberType.IsAssignableFrom(sourceType);
        }

        #endregion
    }
}