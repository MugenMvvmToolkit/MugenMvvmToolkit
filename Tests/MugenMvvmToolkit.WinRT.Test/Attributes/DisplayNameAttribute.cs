// ReSharper disable once CheckNamespace
namespace System.ComponentModel
{
    [AttributeUsage(
        AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Event)]
    public class DisplayNameAttribute : Attribute
    {
        public DisplayNameAttribute()
            : this(string.Empty)
        {
        }

        public DisplayNameAttribute(string displayName)
        {
            DisplayNameValue = displayName;
        }

        public virtual string DisplayName
        {
            get { return DisplayNameValue; }
        }

        protected string DisplayNameValue { get; set; }

        public override bool Equals(object obj)
        {
            if (obj == this)
                return true;
            var displayNameAttribute = obj as DisplayNameAttribute;
            if (displayNameAttribute != null)
                return displayNameAttribute.DisplayName == DisplayName;
            return false;
        }

        public override int GetHashCode()
        {
            return DisplayName.GetHashCode();
        }
    }
}
