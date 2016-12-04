#region Copyright

// ****************************************************************************
// <copyright file="BindingMemberProvider.cs">
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

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using JetBrains.Annotations;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Binding.Models;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Binding.Infrastructure
{
    public class BindingMemberProvider : IBindingMemberProvider
    {
        #region Nested types

        [StructLayout(LayoutKind.Auto)]
        private struct CacheKey
        {
            #region Fields

            public string Path;
            public Type Type;
            public int Hash;
            public bool IgnoreAttachedMembers;

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
                return $"Type: {Type}, Path: {Path}, IgnoreAttachedMembers: {IgnoreAttachedMembers}";
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

            public bool Equals(CacheKey x, CacheKey y)
            {
                return x.IgnoreAttachedMembers == y.IgnoreAttachedMembers &&
                       x.Path.Equals(y.Path, StringComparison.Ordinal) && x.Type.Equals(y.Type);
            }

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
        private readonly HashSet<string> _currentPaths;

        #endregion

        #region Constructors

        public BindingMemberProvider()
        {
            _currentPaths = new HashSet<string>(StringComparer.Ordinal);
            _attachedMembers = new Dictionary<CacheKey, IBindingMemberInfo>(CacheKeyComparer.Instance);
            _tempMembersCache = new Dictionary<CacheKey, IBindingMemberInfo>(CacheKeyComparer.Instance);
            _explicitMembersCache = new Dictionary<CacheKey, IBindingMemberInfo>(CacheKeyComparer.Instance);
        }

        #endregion

        #region Properties

        public static IBindingMemberInfo BindingContextMember => BindingMemberInfo.BindingContextMember;

        public static IBindingMemberInfo Unset => BindingMemberInfo.Unset;

        public static IBindingMemberInfo Empty => BindingMemberInfo.Empty;

        #endregion

        #region Implementation of IBindingMemberProvider

        public IBindingMemberInfo GetBindingMember(Type sourceType, string path, bool ignoreAttachedMembers, bool throwOnError)
        {
            Should.NotBeNull(sourceType, nameof(sourceType));
            Should.NotBeNull(path, nameof(path));
            IBindingMemberInfo bindingMember;
            var key = new CacheKey(sourceType, path, ignoreAttachedMembers);
            bool lockTaken = false;
            try
            {
                Monitor.Enter(_tempMembersCache, ref lockTaken);
                //To prevent cyclic dependency
                if (!_currentPaths.Add(path))
                    return null;
                if (!_tempMembersCache.TryGetValue(key, out bindingMember))
                {
                    if (!ignoreAttachedMembers)
                        bindingMember = GetAttachedBindingMember(key);
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
            finally
            {
                _currentPaths.Remove(path);
                if (lockTaken)
                    Monitor.Exit(_tempMembersCache);
            }
            if (throwOnError && bindingMember == null)
                throw BindingExceptionManager.InvalidBindingMember(sourceType, path);
            return bindingMember;
        }

        public void Register(Type type, IBindingMemberInfo member, bool rewrite)
        {
            Register(type, member.Path, member, rewrite);
        }

        public void Register(Type type, string path, IBindingMemberInfo member, bool rewrite)
        {
            Should.NotBeNull(type, nameof(type));
            Should.NotBeNullOrEmpty(path, nameof(path));
            Should.NotBeNull(member, nameof(member));
            lock (_attachedMembers)
            {
                var key = new CacheKey(type, path, false);
                if (_attachedMembers.ContainsKey(key))
                {
                    if (rewrite)
                        Tracer.Warn("The member {0} on type {1} has been overwritten", path, type);
                    else
                        throw BindingExceptionManager.DuplicateBindingMember(type, path);
                }
                _attachedMembers[key] = member;
            }
            lock (_tempMembersCache)
                _tempMembersCache.Clear();
            if (Tracer.TraceInformation)
                Tracer.Info("The attached property (path: {0}, type: {1}, target type: {2}) was registered.", path, member.Type, type);
        }

        public bool Unregister(Type type, string path)
        {
            Should.NotBeNull(type, nameof(type));
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

        public ICollection<KeyValuePair<string, IBindingMemberInfo>> GetAttachedMembers(Type type)
        {
            Should.NotBeNull(type, nameof(type));
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

        [CanBeNull]
        protected virtual IBindingMemberInfo GetExplicitBindingMember([NotNull] Type sourceType, [NotNull] string path)
        {
            string[] indexerArgs = null;
            if (path.StartsWith("Item[", StringComparison.Ordinal))
                path = path.Substring(4);
            if (path.StartsWith("[", StringComparison.Ordinal) && path.EndsWith("]", StringComparison.Ordinal))
            {
                indexerArgs = path
                    .RemoveBounds()
                    .Split(BindingReflectionExtensions.CommaSeparator, StringSplitOptions.RemoveEmptyEntries);
            }

            MemberFlags memberFlags = MemberFlags.Public | MemberFlags.Instance | MemberFlags.NonPublic;
            var types = BindingReflectionExtensions.SelfAndBaseTypes(sourceType);
            foreach (var type in types)
            {
                if (indexerArgs == null)
                {
                    PropertyInfo property = type.GetPropertyEx(path, memberFlags);
                    if (property != null)
                        return new BindingMemberInfo(path, property, sourceType);
                }
                else
                {
                    PropertyInfo candidate = null;
                    int valueTypeCount = -1;
                    foreach (var property in type.GetPropertiesEx(memberFlags))
                    {
                        var indexParameters = property.GetIndexParameters();
                        if (indexParameters.Length != indexerArgs.Length)
                            continue;
                        try
                        {
                            int count = 0;
                            for (int i = 0; i < indexParameters.Length; i++)
                            {
                                var arg = indexerArgs[i];
                                var paramType = indexParameters[i].ParameterType;
                                if (arg.StartsWith("\"", StringComparison.Ordinal) && arg.EndsWith("\"", StringComparison.Ordinal))
                                {
                                    if (paramType != typeof(string))
                                        break;
                                }
                                else
                                {
                                    BindingServiceProvider.ValueConverter(Empty, paramType, arg);
                                    if (paramType.IsValueType())
                                        count++;
                                }
                            }
                            if (valueTypeCount < count)
                            {
                                candidate = property;
                                valueTypeCount = count;
                            }
                        }
                        catch
                        {
                            ;
                        }
                    }
                    if (candidate != null)
                        return new BindingMemberInfo(path, candidate, sourceType);
                    if (type.IsArray && type.GetArrayRank() == indexerArgs.Length)
                        return new BindingMemberInfo(path, type);
                }
                EventInfo @event = type.GetEventEx(path, MemberFlags.Public | MemberFlags.Instance);
                if (@event != null)
                    return new BindingMemberInfo(path, @event, null);

                FieldInfo field = type.GetFieldEx(path, memberFlags);
                if (field != null && !field.IsPrivate)
                    return new BindingMemberInfo(path, field, sourceType);
            }

            if (typeof(IDynamicObject).IsAssignableFrom(sourceType))
                return new BindingMemberInfo(path, false);
#if NET4
            if (typeof(System.Dynamic.ExpandoObject).IsAssignableFrom(sourceType))
                return new BindingMemberInfo(path, true);
#else
            //this allow linker strip ExpandoObject if it's not used
            if (sourceType.FullName == "System.Dynamic.ExpandoObject")
                return new BindingMemberInfo(path, true);
#endif


            if (path.EndsWith(AttachedMemberConstants.ChangedEventPostfix, StringComparison.Ordinal))
            {
                var memberName = path.Substring(0, path.Length - 7);
                var member = GetBindingMember(sourceType, memberName, false, false);
                if (member != null && member.CanObserve)
                    return new BindingMemberInfo(path, null, member);
            }
            return null;
        }


        [CanBeNull]
        protected IBindingMemberInfo GetAttachedBindingMember([NotNull] Type sourceType, [NotNull] string path)
        {
            return GetAttachedBindingMember(new CacheKey(sourceType, path, false));
        }

        private IBindingMemberInfo GetAttachedBindingMember(CacheKey key)
        {
            bool isIndexer = false;
            string path = null;
            if (key.Path.StartsWith("Item[", StringComparison.Ordinal) || key.Path.StartsWith("[", StringComparison.Ordinal))
            {
                isIndexer = true;
                path = key.Path;
                key = new CacheKey(key.Type, ReflectionExtensions.IndexerName, key.IgnoreAttachedMembers);
            }
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
            if (isIndexer && bindingMember != null)
                return new BindingMemberInfo(bindingMember, path);
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
#if NET_STANDARD
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
#if NET_STANDARD
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
#if NET_STANDARD
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
