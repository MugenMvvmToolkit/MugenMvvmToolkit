using System;
using MugenMvvm.Collections;

namespace MugenMvvm.Bindings.Members
{
    public class MemberTypesRequest
    {
        public MemberTypesRequest(string name, ItemOrArray<Type> types)
        {
            Should.NotBeNull(name, nameof(name));
            Name = name;
            Types = types;
        }

        public string Name { get; protected set; }

        public ItemOrArray<Type> Types { get; protected set; }

        public override string ToString()
        {
            if (Types.IsEmpty)
                return Name;
            return $"{Name}({string.Join(",", (object[]) Types.AsList())})";
        }
    }
}