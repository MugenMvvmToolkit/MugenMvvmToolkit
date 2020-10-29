using System;

namespace MugenMvvm.Bindings.Members
{
    public class MemberTypesRequest
    {
        #region Constructors

        public MemberTypesRequest(string name, Type[] types)
        {
            Should.NotBeNull(name, nameof(name));
            Should.NotBeNull(types, nameof(types));
            Name = name;
            Types = types;
        }

        #endregion

        #region Properties

        public string Name { get; protected set; }

        public Type[] Types { get; protected set; }

        #endregion

        #region Methods

        public override string ToString()
        {
            if (Types.Length == 0)
                return Name;
            return $"{Name}({string.Join(",", (object[]) Types)})";
        }

        #endregion
    }
}