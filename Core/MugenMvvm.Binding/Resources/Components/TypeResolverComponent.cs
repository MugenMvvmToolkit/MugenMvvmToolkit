using System;
using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Interfaces.Resources.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Binding.Resources.Components
{
    public sealed class TypeResolverComponent : ITypeResolverComponent, IHasPriority
    {
        #region Constructors

        public TypeResolverComponent()
        {
            Types = new Dictionary<string, Type>();
            Types = new Dictionary<string, Type>
            {
                {"object", typeof(object)},
                {"bool", typeof(bool)},
                {"char", typeof(char)},
                {"string", typeof(string)},
                {"sbyte", typeof(sbyte)},
                {"byte", typeof(byte)},
                {"short", typeof(short)},
                {"ushort", typeof(ushort)},
                {"int", typeof(int)},
                {"uint", typeof(uint)},
                {"long", typeof(long)},
                {"ulong", typeof(ulong)},
                {"float", typeof(float)},
                {"double", typeof(double)},
                {"decimal", typeof(decimal)}
            };
            AddType(typeof(object));
            AddType(typeof(bool));
            AddType(typeof(char));
            AddType(typeof(string));
            AddType(typeof(sbyte));
            AddType(typeof(byte));
            AddType(typeof(short));
            AddType(typeof(ushort));
            AddType(typeof(int));
            AddType(typeof(uint));
            AddType(typeof(long));
            AddType(typeof(ulong));
            AddType(typeof(float));
            AddType(typeof(double));
            AddType(typeof(decimal));
            AddType(typeof(DateTime));
            AddType(typeof(TimeSpan));
            AddType(typeof(Guid));
            AddType(typeof(Math));
            AddType(typeof(Convert));
            AddType(typeof(Enumerable));
            AddType(typeof(Environment));
        }

        #endregion

        #region Properties

        public IDictionary<string, Type> Types { get; }

        public int Priority { get; set; } = ResourceComponentPriority.TypeResolver;

        #endregion

        #region Implementation of interfaces

        public Type? TryGetType(string name, IReadOnlyMetadataContext? metadata)
        {
            Types.TryGetValue(name, out var value);
            return value;
        }

        #endregion

        #region Methods

        public void AddType(Type type)
        {
            Should.NotBeNull(type, nameof(type));
            Types[type.Name] = type;
            var fullName = type.FullName;
            if (fullName != null)
                Types[fullName] = type;
        }

        #endregion
    }
}