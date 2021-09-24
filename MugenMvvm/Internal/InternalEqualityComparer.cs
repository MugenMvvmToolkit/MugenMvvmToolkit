﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using MugenMvvm.Collections;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Internal
{
    internal sealed class InternalEqualityComparer : IEqualityComparer<MemberInfo?>, IEqualityComparer<(object?, object?)>, IEqualityComparer<Type?>,
        IEqualityComparer<KeyValuePair<Type, MethodInfo>>, IEqualityComparer<KeyValuePair<Type, MemberInfo>>, IEqualityComparer<KeyValuePair<Type, Type>>,
        IEqualityComparer<IMetadataContextKey?>
    {
        private static readonly InternalEqualityComparer Comparer = new();
        public static readonly IEqualityComparer<MemberInfo> MemberInfo = Comparer;
        public static readonly IEqualityComparer<(object?, object?)> ValueTupleReference = Comparer;
        public static readonly IEqualityComparer<object?> Reference = new ReferenceComparer();
        public static readonly IEqualityComparer<Type> Type = Comparer;
        public static readonly IEqualityComparer<KeyValuePair<Type, MethodInfo>> TypeMethod = Comparer;
        public static readonly IEqualityComparer<KeyValuePair<Type, MemberInfo>> TypeMember = Comparer;
        public static readonly IEqualityComparer<KeyValuePair<Type, Type>> TypeType = Comparer;
        public static readonly IEqualityComparer<IMetadataContextKey> MetadataContextKey = Comparer;


        private InternalEqualityComparer()
        {
        }

        public static IEqualityComparer<T> GetReferenceComparer<T>() => typeof(T).IsValueType ? EqualityComparer<T>.Default : (IEqualityComparer<T>) Reference;

        public static bool Equals(Type[] x, Type[] y)
        {
            if (x == y)
                return true;
            if (x.Length != y.Length)
                return false;
            for (var i = 0; i < x.Length; i++)
            {
                if (x[i] != y[i])
                    return false;
            }

            return true;
        }

        public static bool Equals(ItemOrArray<Type> x, ItemOrArray<Type> y)
        {
            if (x.Count != y.Count)
                return false;
            if (x.List != null)
            {
                if (y.List == null)
                    return false;
                return Equals(x.List, y.List);
            }

            return x.Item == y.Item;
        }

        bool IEqualityComparer<(object?, object?)>.Equals((object?, object?) x, (object?, object?) y) => x.Item1 == y.Item1 && x.Item2 == y.Item2;

        int IEqualityComparer<(object?, object?)>.GetHashCode((object?, object?) key)
        {
            if (key.Item1 == null)
                return key.Item2 == null ? 0 : RuntimeHelpers.GetHashCode(key.Item2);

            if (key.Item2 == null)
                return RuntimeHelpers.GetHashCode(key.Item1);
            return HashCode.Combine(RuntimeHelpers.GetHashCode(key.Item1), RuntimeHelpers.GetHashCode(key.Item2));
        }

        bool IEqualityComparer<IMetadataContextKey?>.Equals(IMetadataContextKey? x, IMetadataContextKey? y) => x!.Equals(y);

        int IEqualityComparer<IMetadataContextKey?>.GetHashCode(IMetadataContextKey? obj) => obj!.GetHashCode();

        bool IEqualityComparer<KeyValuePair<Type, MemberInfo>>.Equals(KeyValuePair<Type, MemberInfo> x, KeyValuePair<Type, MemberInfo> y) => x.Key == y.Key && x.Value == y.Value;

        int IEqualityComparer<KeyValuePair<Type, MemberInfo>>.GetHashCode(KeyValuePair<Type, MemberInfo> key) => HashCode.Combine(key.Key, key.Value);

        bool IEqualityComparer<KeyValuePair<Type, MethodInfo>>.Equals(KeyValuePair<Type, MethodInfo> x, KeyValuePair<Type, MethodInfo> y) => x.Key == y.Key && x.Value == y.Value;

        int IEqualityComparer<KeyValuePair<Type, MethodInfo>>.GetHashCode(KeyValuePair<Type, MethodInfo> key) => HashCode.Combine(key.Key, key.Value);

        bool IEqualityComparer<KeyValuePair<Type, Type>>.Equals(KeyValuePair<Type, Type> x, KeyValuePair<Type, Type> y) => x.Key == y.Key && x.Value == y.Value;

        int IEqualityComparer<KeyValuePair<Type, Type>>.GetHashCode(KeyValuePair<Type, Type> obj) => HashCode.Combine(obj.Key, obj.Value);

        bool IEqualityComparer<MemberInfo?>.Equals(MemberInfo? x, MemberInfo? y) => x == y;

        int IEqualityComparer<MemberInfo?>.GetHashCode(MemberInfo? obj) => obj!.GetHashCode();

        bool IEqualityComparer<Type?>.Equals(Type? x, Type? y) => x == y;

        int IEqualityComparer<Type?>.GetHashCode(Type? obj) => obj!.GetHashCode();

        private sealed class ReferenceComparer : IEqualityComparer<object?>
        {
            bool IEqualityComparer<object?>.Equals(object? x, object? y) => x == y;

            int IEqualityComparer<object?>.GetHashCode(object? obj) => obj == null ? 0 : RuntimeHelpers.GetHashCode(obj);
        }
    }
}