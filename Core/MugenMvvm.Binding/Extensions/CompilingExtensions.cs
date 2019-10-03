using System;
using System.Linq;
using System.Linq.Expressions;
using MugenMvvm.Collections.Internal;
using MugenMvvm.Enums;

// ReSharper disable once CheckNamespace
namespace MugenMvvm.Binding
{
    public static partial class BindingMugenExtensions
    {
        #region Fields

        private static readonly TypeLightDictionary<TypeCode> TypeCodeTable = new TypeLightDictionary<TypeCode>(15)
        {
            {typeof(bool), TypeCode.Boolean},
            {typeof(char), TypeCode.Char},
            {typeof(byte), TypeCode.Byte},
            {typeof(short), TypeCode.Int16},
            {typeof(int), TypeCode.Int32},
            {typeof(long), TypeCode.Int64},
            {typeof(sbyte), TypeCode.SByte},
            {typeof(ushort), TypeCode.UInt16},
            {typeof(uint), TypeCode.UInt32},
            {typeof(ulong), TypeCode.UInt64},
            {typeof(float), TypeCode.Single},
            {typeof(double), TypeCode.Double},
            {typeof(DateTime), TypeCode.DateTime},
            {typeof(decimal), TypeCode.Decimal},
            {typeof(string), TypeCode.String}
        };

        #endregion

        #region Methods

        public static Expression GenerateExpression(this Expression left, Expression right, Func<Expression, Expression, Expression> getExpr)
        {
            Convert(ref left, ref right, true);
            return getExpr(left, right);
        }

        public static void Convert(ref Expression left, ref Expression right, bool exactly)
        {
            if (left.Type.EqualsEx(right.Type))
                return;

            if (left.Type.IsCompatibleWith(right.Type))
                left = left.ConvertIfNeed(right.Type, exactly);
            else if (right.Type.IsCompatibleWith(left.Type))
                right = right.ConvertIfNeed(left.Type, exactly);
        }

        public static bool IsCompatibleWith(this Type source, Type target)
        {
            return IsCompatibleWith(source, target, out _);
        }

        public static bool IsCompatibleWith(this Type source, Type target, out bool boxRequired)
        {
            boxRequired = false;
            if (source == target)
                return true;
            if (!target.IsValueTypeUnified())
            {
                boxRequired = source.IsValueTypeUnified();
                return target.IsAssignableFromUnified(source);
            }

            var st = GetNonNullableType(source);
            var tt = GetNonNullableType(target);
            if (st != source && tt.EqualsEx(st))
                return false;
            var sc = st.IsEnumUnified() ? TypeCode.Object : st.GetTypeCode();
            var tc = tt.IsEnumUnified() ? TypeCode.Object : tt.GetTypeCode();
            switch (sc)
            {
                case TypeCode.SByte:
                    switch (tc)
                    {
                        case TypeCode.SByte:
                        case TypeCode.Int16:
                        case TypeCode.Int32:
                        case TypeCode.Int64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }

                    break;
                case TypeCode.Byte:
                    switch (tc)
                    {
                        case TypeCode.Byte:
                        case TypeCode.Int16:
                        case TypeCode.UInt16:
                        case TypeCode.Int32:
                        case TypeCode.UInt32:
                        case TypeCode.Int64:
                        case TypeCode.UInt64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }

                    break;
                case TypeCode.Int16:
                    switch (tc)
                    {
                        case TypeCode.Int16:
                        case TypeCode.Int32:
                        case TypeCode.Int64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }

                    break;
                case TypeCode.UInt16:
                    switch (tc)
                    {
                        case TypeCode.UInt16:
                        case TypeCode.Int32:
                        case TypeCode.UInt32:
                        case TypeCode.Int64:
                        case TypeCode.UInt64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }

                    break;
                case TypeCode.Int32:
                    switch (tc)
                    {
                        case TypeCode.Int32:
                        case TypeCode.Int64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }

                    break;
                case TypeCode.UInt32:
                    switch (tc)
                    {
                        case TypeCode.UInt32:
                        case TypeCode.Int64:
                        case TypeCode.UInt64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }

                    break;
                case TypeCode.Int64:
                    switch (tc)
                    {
                        case TypeCode.Int64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }

                    break;
                case TypeCode.UInt64:
                    switch (tc)
                    {
                        case TypeCode.UInt64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }

                    break;
                case TypeCode.Single:
                    switch (tc)
                    {
                        case TypeCode.Single:
                        case TypeCode.Double:
                            return true;
                    }

                    break;
                default:
                    if (st == tt)
                        return true;
                    break;
            }

            return false;
        }

        public static Type GetTargetType(ref Expression target)
        {
            var constant = target as ConstantExpression;
            Type type = target.Type;
            if (constant?.Value is Type value)
            {
                type = value;
                target = null;
            }
            return type;
        }

        internal static object TryParseEnum(this Type type, string name)
        {
            if (!type.IsEnumUnified())
                return null;

            foreach (var field in type.GetFieldsUnified(MemberFlags.StaticPublic))
            {
                if (field.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase))
                    return field.GetValue(null);
            }
            return null;
        }

        private static Type GetNonNullableType(this Type type)
        {
            return IsNullableType(type) ? type.GetGenericArgumentsUnified().First() : type;
        }

        private static bool IsNullableType(this Type type)
        {
            return type.IsGenericTypeUnified() && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        private static TypeCode GetTypeCode(this Type type)
        {
            if (type == null)
                return TypeCode.Empty;
            if (!TypeCodeTable.TryGetValue(type, out var result))
                result = TypeCode.Object;
            return result;
        }

        #endregion

        #region Nested types

        private enum TypeCode
        {
            Byte,
            Int16,
            Int32,
            Int64,
            SByte,
            UInt16,
            UInt32,
            UInt64,
            Single,
            Double,
            Char,
            Boolean,
            String,
            DateTime,
            Decimal,
            Empty,
            Object
        }

        #endregion
    }
}