using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using MugenMvvm.Binding.Delegates;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Extensions;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Members.Descriptors;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Members.Builders
{
    public static class AttachedMemberBuilder
    {
        #region Methods

        public static EventBuilder<TTarget> Event<TTarget>(string name, Type? declaringType = null, Type? eventType = null) where TTarget : class? =>
            new EventBuilder<TTarget>(name, declaringType ?? typeof(TTarget), eventType ?? typeof(EventHandler));

        public static PropertyBuilder<TTarget, TValue> Property<TTarget, TValue>(string name, Type? declaringType = null, Type? propertyType = null)
            where TTarget : class? =>
            new PropertyBuilder<TTarget, TValue>(name, declaringType ?? typeof(TTarget), propertyType ?? typeof(TValue));

        public static MethodBuilder<TTarget, TReturn> Method<TTarget, TReturn>(string name, Type? declaringType = null, Type? returnType = null) where TTarget : class? =>
            new MethodBuilder<TTarget, TReturn>(name, declaringType ?? typeof(TTarget), returnType ?? typeof(TReturn));

        public static ParameterBuilder Parameter<TType>(string? name = null) => new ParameterBuilder(name ?? "", typeof(TType));

        public static ParameterBuilder Parameter(string name, Type type) => new ParameterBuilder(name, type);

        public static EventBuilder<TTarget> GetBuilder<TTarget>(this BindableEventDescriptor<TTarget> descriptor, Type? eventType = null) where TTarget : class
        {
            Should.BeSupported(!descriptor.IsStatic, nameof(descriptor.IsStatic));
            return Event<TTarget>(descriptor.Name, null, eventType);
        }

        public static PropertyBuilder<TTarget, TValue> GetBuilder<TTarget, TValue>(this BindablePropertyDescriptor<TTarget, TValue> descriptor)
            where TTarget : class
        {
            Should.BeSupported(!descriptor.IsStatic, nameof(descriptor.IsStatic));
            return Property<TTarget, TValue>(descriptor.Name);
        }

        public static MethodBuilder<TTarget, TReturn> GetBuilder<TTarget, TReturn>(this BindableMethodDescriptor<TTarget, TReturn> descriptor, bool addParameters = true) where TTarget : class
        {
            Should.BeSupported(!descriptor.IsStatic, nameof(descriptor.IsStatic));
            var m = Method<TTarget, TReturn>(descriptor.Request!.Name, typeof(TTarget), typeof(TReturn));
            var types = descriptor.Request!.Types;
            if (types.Length != 0 && addParameters)
            {
                var parameters = new IParameterInfo[types.Length];
                for (var i = 0; i < types.Length; i++)
                    parameters[i] = Parameter("", types[i]).Build();
                m = m.WithParameters(parameters);
            }

            return m;
        }

        public static MethodBuilder<TTarget, TReturn> GetBuilder<TTarget, TArg1, TReturn>(this BindableMethodDescriptor<TTarget, TArg1, TReturn> descriptor) where TTarget : class => descriptor.RawMethod.GetBuilder();

        public static MethodBuilder<TTarget, TReturn> GetBuilder<TTarget, TArg1, TArg2, TReturn>(this BindableMethodDescriptor<TTarget, TArg1, TArg2, TReturn> descriptor) where TTarget : class =>
            descriptor.RawMethod.GetBuilder();

        public static MethodBuilder<TTarget, TReturn> GetBuilder<TTarget, TArg1, TArg2, TArg3, TReturn>(this BindableMethodDescriptor<TTarget, TArg1, TArg2, TArg3, TReturn> descriptor) where TTarget : class =>
            descriptor.RawMethod.GetBuilder();

        public static MethodBuilder<TTarget, TReturn> GetBuilder<TTarget, TArg1, TArg2, TArg3, TArg4, TReturn>(this BindableMethodDescriptor<TTarget, TArg1, TArg2, TArg3, TArg4, TReturn> descriptor)
            where TTarget : class => descriptor.RawMethod.GetBuilder();

        public static MethodBuilder<TTarget, TReturn> GetBuilder<TTarget, TArg1, TArg2, TArg3, TArg4, TArg5, TReturn>(this BindableMethodDescriptor<TTarget, TArg1, TArg2, TArg3, TArg4, TArg5, TReturn> descriptor)
            where TTarget : class =>
            descriptor.RawMethod.GetBuilder();

        internal static void RaiseMemberAttached<TTarget, TMember>(string id, TTarget target, TMember member, MemberAttachedDelegate<TMember, TTarget> handler, IReadOnlyMetadataContext? metadata)
            where TTarget : class?
            where TMember : class, IMemberInfo
        {
            Should.NotBeNull(member, nameof(member));
            Should.NotBeNull(handler, nameof(handler));
            var attachedValues = MugenService.AttachedValueManager.TryGetAttachedValues(member.GetTarget(target), metadata);
            if (!attachedValues.Contains(id))
            {
                attachedValues.GetOrAdd(id, (member, handler, metadata), (t, s) =>
                {
                    s.handler(s.member, member.AccessModifiers.HasFlagEx(MemberFlags.Static) ? null! : (TTarget) t, s.metadata);
                    return (object?) null;
                });
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static object GetTarget(this IMemberInfo member, object? target)
        {
            if (member.AccessModifiers.HasFlagEx(MemberFlags.Static))
                return member.DeclaringType;
            return target!;
        }

        internal static MemberFlags GetFlags(bool isStatic) => isStatic ? MemberFlags.StaticPublic | MemberFlags.Attached : MemberFlags.InstancePublic | MemberFlags.Attached;

        internal static string GenerateMemberId(string prefix, Type declaringType, string name) => prefix + declaringType.FullName!.Length.ToString(CultureInfo.InvariantCulture) + declaringType.Name +
                                                                                                   declaringType.AssemblyQualifiedName!.Length.ToString(CultureInfo.InvariantCulture) + name;

        #endregion
    }
}