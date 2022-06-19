using System;
using MugenMvvm.Collections;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Bindings.Members
{
    public class MemberTypesRequest : IHasName
    {
        public MemberTypesRequest(string name, ItemOrArray<Type> types)
        {
            Should.NotBeNull(name, nameof(name));
            Name = name;
            Types = types;
        }

        public ItemOrArray<Type> Types { get; protected set; }

        public string Name { get; protected set; }

        public override string ToString()
        {
            if (Types.IsEmpty)
                return Name;
            return $"{Name}({string.Join(",", (object[]) Types.AsList())})";
        }
    }
}