using System;
using System.Linq.Expressions;
using MugenMvvm.Enums;

// ReSharper disable once CheckNamespace
namespace MugenMvvm.Binding
{
    public static partial class MugenBindingExtensions
    {
        #region Methods

        public static Expression GenerateExpression(this Expression left, Expression right, Func<Expression, Expression, Expression> getExpr)
        {
            Convert(ref left, ref right, true);
            return getExpr(left, right);
        }

        public static void Convert(ref Expression left, ref Expression right, bool exactly)
        {
            if (left.Type == right.Type)
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
            if (!target.IsValueType)
            {
                boxRequired = source.IsValueType;
                return target.IsAssignableFrom(source);
            }

            var st = GetNonNullableType(source);
            var tt = GetNonNullableType(target);
            if (st != source && tt == st)
                return false;
            var sc = st.IsEnum ? TypeCode.Object : Type.GetTypeCode(st);
            var tc = tt.IsEnum ? TypeCode.Object : Type.GetTypeCode(tt);
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
                target = null!;
            }
            return type;
        }

        internal static object? TryParseEnum(this Type type, string name)
        {
            if (!type.IsEnum)
                return null;

            foreach (var field in type.GetFields(BindingFlagsEx.StaticPublic))
            {
                if (field.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase))
                    return field.GetValue(null);
            }
            return null;
        }

        internal static bool IsNullableType(this Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        internal static Type GetNonNullableType(this Type type)
        {
            return IsNullableType(type) ? type.GetGenericArguments()[0] : type;
        }

        #endregion
    }
}