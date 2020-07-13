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

        public static EventBuilder<TTarget> Event<TTarget>(string name, Type? declaringType = null, Type? eventType = null) where TTarget : class?
        {
            return new EventBuilder<TTarget>(name, declaringType ?? typeof(TTarget), eventType ?? typeof(EventHandler));
        }

        public static PropertyBuilder<TTarget, TValue> Property<TTarget, TValue>(string name, Type? declaringType = null, Type? propertyType = null)
            where TTarget : class?
        {
            return new PropertyBuilder<TTarget, TValue>(name, declaringType ?? typeof(TTarget), propertyType ?? typeof(TValue));
        }

        public static MethodBuilder<TTarget, TReturn> Method<TTarget, TReturn>(string name, Type? declaringType = null, Type? returnType = null) where TTarget : class?
        {
            return new MethodBuilder<TTarget, TReturn>(name, declaringType ?? typeof(TTarget), returnType ?? typeof(TReturn));
        }

        public static ParameterBuilder Parameter<TType>(string? name = null)
        {
            return new ParameterBuilder(name ?? "", typeof(TType));
        }

        public static ParameterBuilder Parameter(string name, Type type)
        {
            return new ParameterBuilder(name, type);
        }

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

        public static MethodBuilder<TTarget, TReturn> GetBuilder<TTarget, TReturn>(this BindableMethodDescriptor<TTarget, TReturn> descriptor) where TTarget : class
        {
            Should.BeSupported(!descriptor.IsStatic, nameof(descriptor.IsStatic));
            return Method<TTarget, TReturn>(descriptor.Name, typeof(TTarget), typeof(TReturn));
        }

        public static MethodBuilder<TTarget, TReturn> GetBuilder<TTarget, TArg1, TReturn>(this BindableMethodDescriptor<TTarget, TArg1, TReturn> descriptor) where TTarget : class
        {
            return descriptor.RawMethod.GetBuilder().WithParameters(Parameter<TArg1>("p1").Build());
        }

        public static MethodBuilder<TTarget, TReturn> GetBuilder<TTarget, TArg1, TArg2, TReturn>(this BindableMethodDescriptor<TTarget, TArg1, TArg2, TReturn> descriptor) where TTarget : class
        {
            return descriptor.RawMethod.GetBuilder().WithParameters(Parameter<TArg1>("p1").Build(), Parameter<TArg2>("p2").Build());
        }

        public static MethodBuilder<TTarget, TReturn> GetBuilder<TTarget, TArg1, TArg2, TArg3, TReturn>(this BindableMethodDescriptor<TTarget, TArg1, TArg2, TArg3, TReturn> descriptor) where TTarget : class
        {
            return descriptor.RawMethod.GetBuilder().WithParameters(Parameter<TArg1>("p1").Build(), Parameter<TArg2>("p2").Build(), Parameter<TArg3>("p3").Build());
        }

        public static MethodBuilder<TTarget, TReturn> GetBuilder<TTarget, TArg1, TArg2, TArg3, TArg4, TReturn>(this BindableMethodDescriptor<TTarget, TArg1, TArg2, TArg3, TArg4, TReturn> descriptor) where TTarget : class
        {
            return descriptor.RawMethod.GetBuilder().WithParameters(Parameter<TArg1>("p1").Build(), Parameter<TArg2>("p2").Build(), Parameter<TArg3>("p3").Build(), Parameter<TArg4>("p4").Build());
        }

        public static MethodBuilder<TTarget, TReturn> GetBuilder<TTarget, TArg1, TArg2, TArg3, TArg4, TArg5, TReturn>(this BindableMethodDescriptor<TTarget, TArg1, TArg2, TArg3, TArg4, TArg5, TReturn> descriptor)
            where TTarget : class
        {
            return descriptor.RawMethod.GetBuilder().WithParameters(Parameter<TArg1>("p1").Build(), Parameter<TArg2>("p2").Build(), Parameter<TArg3>("p3").Build(), Parameter<TArg4>("p4").Build(),
                Parameter<TArg5>("p5").Build());
        }

        internal static void RaiseMemberAttached<TTarget, TMember>(string id, TTarget target, TMember member, MemberAttachedDelegate<TMember, TTarget> handler, IReadOnlyMetadataContext? metadata)
            where TTarget : class?
            where TMember : class, IMemberInfo
        {
            Should.NotBeNull(member, nameof(member));
            Should.NotBeNull(handler, nameof(handler));
            var attachedValueManager = MugenService.AttachedValueManager;
            var key = member.GetTarget(target);
            if (!attachedValueManager.Contains(key, id))
            {
#pragma warning disable 8634
                attachedValueManager.GetOrAdd(key, id, (member, handler, metadata), (t, state) =>
                {
                    state.handler(state.member, member.AccessModifiers.HasFlagEx(MemberFlags.Static) ? null! : (TTarget)t, state.metadata);
                    return (object?)null;
                });
#pragma warning restore 8634
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static object GetTarget(this IMemberInfo member, object? target)
        {
            if (member.AccessModifiers.HasFlagEx(MemberFlags.Static))
                return member.DeclaringType;
            return target!;
        }

        internal static MemberFlags GetFlags(bool isStatic)
        {
            return isStatic ? MemberFlags.StaticPublic | MemberFlags.Attached : MemberFlags.InstancePublic | MemberFlags.Attached;
        }

        internal static string GenerateMemberId(string prefix, Type declaringType, string name)
        {
            return prefix + declaringType.FullName!.Length.ToString(CultureInfo.InvariantCulture) + declaringType.Name + declaringType.AssemblyQualifiedName!.Length.ToString(CultureInfo.InvariantCulture) + name;
        }

        #endregion
    }
}