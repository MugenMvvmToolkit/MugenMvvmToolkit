#region Copyright

// ****************************************************************************
// <copyright file="DataAnnotationValidatior.cs">
// Copyright (c) 2012-2015 Vyacheslav Volkov
// </copyright>
// ****************************************************************************
// <author>Vyacheslav Volkov</author>
// <email>vvs0205@outlook.com</email>
// <project>MugenMvvmToolkit</project>
// <web>https://github.com/MugenMvvmToolkit/MugenMvvmToolkit</web>
// <license>
// See license.txt in this solution or http://opensource.org/licenses/MS-PL
// </license>
// ****************************************************************************

#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvmToolkit.Attributes;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Validation;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Infrastructure.Validation
{
    public class DataAnnotationValidatior : ValidatorBase<object>
    {
        #region Nested types

        internal interface IValidationElement
        {
            #region Methods

            IEnumerable<object> Validate(ValidationContext context);

            #endregion

        }

        internal sealed class ValidationContext
        {
            #region Fields

            private readonly IDictionary<object, object> _items;
            private readonly object _objectInstance;
            private readonly IServiceProvider _serviceProvider;
            private string _displayName;

            #endregion

            #region Constructors

            public ValidationContext(object instance, IServiceProvider serviceProvider, IDictionary<object, object> items)
            {
                Should.NotBeNull(instance, "instance");
                _objectInstance = instance;
                _serviceProvider = serviceProvider;
                _items = items;
            }

            #endregion

            #region Properties

            public object ObjectInstance
            {
                get { return _objectInstance; }
            }

            public string DisplayName
            {
                get { return _displayName; }
                set
                {
                    Should.PropertyNotBeNullOrEmpty(value, "DisplayName");
                    _displayName = value;
                }
            }

            public string MemberName { get; set; }

            public IDictionary<object, object> Items
            {
                get { return _items; }
            }

            public IServiceProvider ServiceProvider
            {
                get { return _serviceProvider; }
            }

            #endregion
        }

        internal sealed class DynamicValidatableObject : IValidationElement
        {
            #region Fields

            private readonly Func<string> _getDisplayName;
            private readonly Func<ValidationContext, object> _contexConverter;
            private readonly Func<object, object[], object> _methodDelegate;

            #endregion

            #region Constructors

            public DynamicValidatableObject(Func<string> getDisplayName, Func<ValidationContext, object> contexConverter, Func<object, object[], object> methodDelegate)
            {
                _getDisplayName = getDisplayName;
                _contexConverter = contexConverter;
                _methodDelegate = methodDelegate;
            }

            #endregion

            #region Overrides of ValidationElementBase

            public IEnumerable<object> Validate(ValidationContext context)
            {
                context.MemberName = null;
                context.DisplayName = _getDisplayName();
                return (IEnumerable<object>)_methodDelegate(context.ObjectInstance, new[] { _contexConverter(context) });
            }

            #endregion
        }

        internal sealed class DynamicValidationAttribute : IValidationElement
        {
            #region Fields

            private readonly Func<string> _getDisplayName;
            private readonly Func<object, object> _getPropertyValue;
            private readonly Func<object, ValidationContext, object> _getValidationResult;
            private readonly string _memberName;

            #endregion

            #region Constructors

            public DynamicValidationAttribute(string memberName, Func<string> getDisplayName,
                Func<object, object> getPropertyValue, Func<object, ValidationContext, object> getValidationResult)
            {
                _memberName = memberName;
                _getDisplayName = getDisplayName;
                _getPropertyValue = getPropertyValue;
                _getValidationResult = getValidationResult;
            }

            #endregion

            #region Implementation of IValidationElement

            public IEnumerable<object> Validate(ValidationContext validationContext)
            {
                validationContext.MemberName = _memberName;
                validationContext.DisplayName = _getDisplayName();
                object value = _getPropertyValue(validationContext.ObjectInstance);
                var validationResult = _getValidationResult(value, validationContext);
                if (validationResult == null)
                    return Enumerable.Empty<object>();
                return new[] { validationResult };
            }

            #endregion
        }

        #endregion

        #region Fields

        //Only for unit-test
        internal static Func<object, Dictionary<string, List<IValidationElement>>> GetValidationElementsDelegate;
        internal static IDisplayNameProvider DisplayNameProvider;
        internal static readonly Dictionary<Type, Dictionary<string, List<IValidationElement>>> ElementsCache;

        public const string ServiceProviderKey = "_ServiceProviderKey_";
        private const MemberFlags InstancePublicFlags = MemberFlags.Public | MemberFlags.Instance;
        private const string DataAnnotationsNamespace = "System.ComponentModel.DataAnnotations";
        private const string ValidationContextTypeName = DataAnnotationsNamespace + ".ValidationContext";
        private const string ValidationResultTypeName = DataAnnotationsNamespace + ".ValidationResult";

        private const string ValidatableObjectInterfaceShortName = "IValidatableObject";
        private const string MetadataTypeAttributeTypeShortName = "MetadataTypeAttribute";
        private const string ValidateMethodName = "Validate";
        private const string GetValidationResultMethodName = "GetValidationResult";
        private const string MemberNamesProperty = "MemberNames";
        private const string DisplayNameProperty = "DisplayName";
        private const string MemberNameProperty = "MemberName";
        private const string MetadataClassTypeProperty = "MetadataClassType";

        private static readonly Dictionary<Type, Func<ValidationContext, object>> ValidationContextCache;
        private static readonly Dictionary<Type, Func<object, IEnumerable<string>>> MemberNamesPropertyCache;
        private static readonly Dictionary<Type, Type[]> MetadataTypeCache;
        private static readonly Dictionary<object, object> CheckDictionary;
        private static readonly Type[] ValidationContextThreeTypesConstructor;
        private static readonly Type[] ValidationContextTwoTypesConstructor;

        private Dictionary<string, List<IValidationElement>> _validationElements;
        private ValidationContext _validationContext;

        #endregion

        #region Constructors

        static DataAnnotationValidatior()
        {
            ValidationContextCache = new Dictionary<Type, Func<ValidationContext, object>>();
            ElementsCache = new Dictionary<Type, Dictionary<string, List<IValidationElement>>>();
            MemberNamesPropertyCache = new Dictionary<Type, Func<object, IEnumerable<string>>>();
            CheckDictionary = new Dictionary<object, object>();
            MetadataTypeCache = new Dictionary<Type, Type[]>();
            ValidationContextThreeTypesConstructor = new[]
            {
                typeof (object),
                typeof (IServiceProvider),
                typeof (IDictionary<object, object>)
            };
            ValidationContextTwoTypesConstructor = new[]
            {
                typeof (object),
                typeof (IDictionary<object, object>)
            };
        }

        #endregion

        #region Overrides of ValidatorBase

        protected override bool CanValidateInternal(IValidatorContext validatorContext)
        {
            _validationElements = GetValidationElements(validatorContext.Instance);
            return _validationElements.Count > 0;
        }

        protected override void OnInitialized(IValidatorContext context)
        {
            _validationContext = new ValidationContext(context.Instance, context.ServiceProvider,
                context.ValidationMetadata.ToDictionary());
        }

        protected override Task<IDictionary<string, IEnumerable>> ValidateInternalAsync(string propertyName, CancellationToken token)
        {
            List<IValidationElement> list;
            if (!_validationElements.TryGetValue(propertyName, out list))
                return DoNothingResult;
            var result = new Dictionary<string, IEnumerable>();
            lock (_validationElements)
                Validate(result, list, _validationContext);
            return FromResult(result);
        }

        protected override Task<IDictionary<string, IEnumerable>> ValidateInternalAsync(CancellationToken token)
        {
            var result = new Dictionary<string, IEnumerable>();
            lock (_validationElements)
            {
                foreach (var element in _validationElements)
                    Validate(result, element.Value, _validationContext);
            }
            return FromResult(result);
        }

        protected override void OnDispose()
        {
            _validationContext = null;
            base.OnDispose();
        }

        #endregion

        #region Methods

        private static void Validate(Dictionary<string, IEnumerable> result, List<IValidationElement> elements, ValidationContext context)
        {
            if (context == null)
                return;
            var validationResults = ValidateElements(elements, context);
            for (int index = 0; index < validationResults.Count; index++)
            {
                var validationResult = validationResults[index];
                var memberNames = GetMemberNames(validationResult);
                bool hasValue = false;
                foreach (var member in memberNames)
                {
                    if (member == null)
                        continue;
                    hasValue = true;
                    IEnumerable value;
                    if (!result.TryGetValue(member, out value) || !(value is List<object>))
                    {
                        var objects = new List<object>();
                        if (value != null)
                            objects.AddRange(value.OfType<object>());
                        result[member] = objects;
                        value = objects;
                    }
                    ((IList)value).Add(validationResult);
                }
                if (!hasValue)
                    Tracer.Warn("The validation result for member '{0}' does not contain any MemberNames, ErrorMessage '{1}'.", context.MemberName, validationResult);
            }
        }

        private static IList<object> ValidateElements(List<IValidationElement> elements, ValidationContext context)
        {
            if (elements.Count == 0)
                return Empty.Array<object>();
            var results = new List<object>();
            for (int index = 0; index < elements.Count; index++)
            {
                var validationResults = elements[index].Validate(context);
                if (validationResults == null)
                    continue;
                foreach (var result in validationResults)
                {
                    if (result != null)
                        results.Add(result);
                }
            }
            return results;
        }

        private static IEnumerable<string> GetMemberNames(object validationResult)
        {
            var type = validationResult.GetType();
            Func<object, IEnumerable<string>> func;
            lock (MemberNamesPropertyCache)
            {
                if (!MemberNamesPropertyCache.TryGetValue(type, out func))
                {
                    var propertyInfo = type.GetPropertyEx(MemberNamesProperty, MemberFlags.Instance | MemberFlags.Public);
                    if (propertyInfo == null)
                        func = o => Empty.Array<string>();
                    else
                        func = ServiceProvider.ReflectionManager.GetMemberGetter<IEnumerable<string>>(propertyInfo);
                    MemberNamesPropertyCache[type] = func;
                }
            }
            return func(validationResult);
        }

        internal static Dictionary<string, List<IValidationElement>> GetValidationElements(object instance)
        {
            if (GetValidationElementsDelegate != null)
                return GetValidationElementsDelegate(instance);
            Should.NotBeNull(instance, "instance");
            Type type = instance.GetType();
            lock (ElementsCache)
            {
                Dictionary<string, List<IValidationElement>> result;
                if (!ElementsCache.TryGetValue(type, out result))
                {
                    result = new Dictionary<string, List<IValidationElement>>();
                    IList<IValidationElement> elements = TryGetValidatableObjectMethods(type);
                    if (elements.Count != 0)
                        result[string.Empty] = new List<IValidationElement>(elements);
                    FillValidationAttributes(type, result);
                    var toRemove = result.Where(pair => pair.Value.Count == 0).ToList();
                    for (int index = 0; index < toRemove.Count; index++)
                        result.Remove(toRemove[index].Key);
                    ElementsCache[type] = result;
                }
                return result;
            }
        }

        private static void FillValidationAttributes(Type type, IDictionary<string, List<IValidationElement>> elements)
        {
            const MemberFlags flags = MemberFlags.Public | MemberFlags.Instance;
            Dictionary<MemberInfo, HashSet<MemberInfo>> originalMembers = type
                .GetPropertiesEx(flags)
                .Where(info => info.CanRead && info.GetGetMethod(true).GetParameters().Length == 0)
                .ToDictionary(info => (MemberInfo)info, info => new HashSet<MemberInfo> { info });

            foreach (var field in type.GetFieldsEx(flags))
                originalMembers.Add(field, new HashSet<MemberInfo> { field });
            ICollection<Type> metadataTypes = GetMetadataTypes(type);
            foreach (Type metadataType in metadataTypes)
            {
                foreach (PropertyInfo metadataProperty in metadataType.GetPropertiesEx(flags))
                {
                    KeyValuePair<MemberInfo, HashSet<MemberInfo>> valuePair = originalMembers
                        .FirstOrDefault(pair => pair.Key is PropertyInfo && pair.Key.Name == metadataProperty.Name);
                    if (valuePair.Value == null)
                        throw ExceptionManager.MissingMetadataProperty(type, metadataProperty.Name, metadataProperty.DeclaringType);
                    valuePair.Value.Add(metadataProperty);
                }

                foreach (FieldInfo metadataField in metadataType.GetFieldsEx(flags))
                {
                    KeyValuePair<MemberInfo, HashSet<MemberInfo>> valuePair = originalMembers
                        .FirstOrDefault(pair => pair.Key is FieldInfo && pair.Key.Name == metadataField.Name);
                    if (valuePair.Value == null)
                        throw ExceptionManager.MissingMetadataField(type, metadataField.Name, metadataField.DeclaringType);
                    valuePair.Value.Add(metadataField);
                }
            }

            foreach (var keyValuePair in originalMembers)
            {
                List<IValidationElement> list;
                if (!elements.TryGetValue(keyValuePair.Key.Name, out list))
                {
                    list = new List<IValidationElement>();
                    elements[keyValuePair.Key.Name] = list;
                }
                foreach (MemberInfo member in keyValuePair.Value)
                {
                    list.AddRange(GetValidationAttributes(keyValuePair.Key, member));
                }
            }
        }

        private static List<IValidationElement> TryGetValidatableObjectMethods(Type type)
        {
            IEnumerable<MethodInfo> methods = type
                .GetInterfaces()
                .Where(t => t.Name == ValidatableObjectInterfaceShortName)
                .SelectMany(t => t.GetMethodsEx(InstancePublicFlags))
                .Where(info =>
                {
#if PCL_WINRT
                    if (info.Name != ValidateMethodName || !info.ReturnType.GetTypeInfo().IsGenericType
                                            || info.ReturnType.GetGenericArguments()[0].FullName != ValidationResultTypeName)
                        return false;
#else
                    if (info.Name != ValidateMethodName || !info.ReturnType.IsGenericType
                                            || info.ReturnType.GetGenericArguments()[0].FullName != ValidationResultTypeName)
                        return false;
#endif

                    ParameterInfo[] parameterInfos = info.GetParameters();
                    return parameterInfos.Length == 1 &&
                           parameterInfos[0].ParameterType.FullName == ValidationContextTypeName;
                });
            var elements = new List<IValidationElement>();
            foreach (MethodInfo methodInfo in methods)
            {
                Type validationContextType = methodInfo.GetParameters()[0].ParameterType;
                Func<ValidationContext, object> contextConverter = TryGetValidationContextConverter(validationContextType);
                if (contextConverter == null)
                    continue;
                Func<object, object[], object> methodDelegate = ServiceProvider.ReflectionManager.GetMethodDelegate(methodInfo);
#if PCL_WINRT
                var typeInfo = type.GetTypeInfo();
                Func<string> displayNameAccessor = GetDisplayNameAccessor(typeInfo);
#else
                Func<string> displayNameAccessor = GetDisplayNameAccessor(type);
#endif
                if (Tracer.TraceInformation)
                    Tracer.Info("Type {0} implements IValidatableObject", type);
                elements.Add(new DynamicValidatableObject(displayNameAccessor, contextConverter, methodDelegate));
            }
            return elements;
        }

        private static List<IValidationElement> GetValidationAttributes(MemberInfo originalMember, MemberInfo metadataMember)
        {
            var validationAttributes = new List<IValidationElement>();
            Attribute[] attributes = metadataMember.GetAttributes();
            foreach (Attribute attribute in attributes)
            {
                Type type = attribute.GetType();
                MethodInfo method = type
                    .GetMethodsEx(InstancePublicFlags)
                    .FirstOrDefault(info =>
                    {
                        if (info.Name != GetValidationResultMethodName ||
                            info.ReturnType.FullName != ValidationResultTypeName)
                            return false;
                        ParameterInfo[] parameters = info.GetParameters();
                        return parameters.Length == 2 && parameters[0].ParameterType.Equals(typeof(object)) &&
                               parameters[1].ParameterType.FullName == ValidationContextTypeName;
                    });
                if (method == null)
                    continue;
                ParameterInfo[] @params = method.GetParameters();

                var validationContextConverter = TryGetValidationContextConverter(@params[1].ParameterType);
                if (validationContextConverter == null)
                    continue;
                Func<object, object[], object> methodDelegate = ServiceProvider.ReflectionManager.GetMethodDelegate(method);
                Func<object, object> getPropertyValue = ServiceProvider.ReflectionManager.GetMemberGetter<object>(originalMember);
                Attribute attr = attribute;
                Func<string> displayNameAccessor = GetDisplayNameAccessor(originalMember);
                var element = new DynamicValidationAttribute(originalMember.Name, displayNameAccessor, getPropertyValue,
                    (o, context) => methodDelegate(attr, new[] { o, validationContextConverter(context) }));
                if (Tracer.TraceInformation)
                    Tracer.Info("Added a '{0}' validation attribute for type: '{1}', member: '{2}'", attr,
                        originalMember.DeclaringType, originalMember);
                validationAttributes.Add(element);
            }
            return validationAttributes;
        }

        private static Func<ValidationContext, object> TryGetValidationContextConverter(Type type)
        {
            lock (ValidationContextCache)
            {
                Func<ValidationContext, object> value;
                if (!ValidationContextCache.TryGetValue(type, out value))
                {
                    value = TryGetValidationContextConverterInternal(type);
                    ValidationContextCache[type] = value;
                }
                return value;
            }
        }

        private static Func<ValidationContext, object> TryGetValidationContextConverterInternal(Type type)
        {
            bool isThree = true;
            ConstructorInfo constructor = type.GetConstructor(ValidationContextThreeTypesConstructor);
            if (constructor != null)
            {
                //NOTE WinRT allows to get the constructor with three args but it fails on create.
                try
                {
                    constructor.InvokeEx(type, ServiceProvider.IocContainer, CheckDictionary);
                }
                catch
                {
                    constructor = null;
                }
            }

            if (constructor == null)
            {
                constructor = type.GetConstructor(ValidationContextTwoTypesConstructor);
                isThree = false;
            }
            if (constructor == null)
                return null;

            PropertyInfo displayNameProp = type.GetPropertyEx(DisplayNameProperty, InstancePublicFlags);
            PropertyInfo memberNameProp = type.GetPropertyEx(MemberNameProperty, InstancePublicFlags);
            if (displayNameProp == null || memberNameProp == null ||
                !displayNameProp.PropertyType.Equals(typeof(string)) ||
                !memberNameProp.PropertyType.Equals(typeof(string)))
                return null;
            Action<object, string> displayNameSetter = ServiceProvider.ReflectionManager.GetMemberSetter<string>(displayNameProp);
            Action<object, string> memberNameSetter = ServiceProvider.ReflectionManager.GetMemberSetter<string>(memberNameProp);
            Func<object[], object> activatorDelegate = ServiceProvider.ReflectionManager.GetActivatorDelegate(constructor);
            return context => ConverterValidationContext(context, activatorDelegate, displayNameSetter, memberNameSetter, isThree);
        }


        private static object ConverterValidationContext(ValidationContext context,
            Func<object[], object> activatorDelegate, Action<object, string> displayNameSetter,
            Action<object, string> memberNameSetter, bool isThree)
        {
            if (context.ServiceProvider != null)
                context.Items[ServiceProviderKey] = context.ServiceProvider;
            var contextResult = activatorDelegate(isThree
                    ? new[] { context.ObjectInstance, context.ServiceProvider, context.Items }
                    : new[] { context.ObjectInstance, context.Items });
            displayNameSetter(contextResult, context.DisplayName);
            memberNameSetter(contextResult, context.MemberName);
            return contextResult;
        }
        internal static Type[] GetMetadataTypes(Type type)
        {
            lock (MetadataTypeCache)
            {
                Type[] list;
                if (!MetadataTypeCache.TryGetValue(type, out list))
                {
                    Attribute[] attributes = type.GetAttributes();
                    var provider = ServiceProvider.EntityMetadataTypeProvider;
                    IEnumerable<Type> types = null;
                    if (provider != null)
                        types = provider(type);

                    var result = types == null ? new HashSet<Type>() : new HashSet<Type>(types);
                    foreach (Attribute attribute in attributes)
                    {
                        var metadataTypeAttribute = attribute as MetadataTypeAttribute;
                        if (metadataTypeAttribute != null)
                        {
                            foreach (var meta in metadataTypeAttribute.GetTypes(type))
                                result.Add(meta);
                            continue;
                        }

                        Type attrType = attribute.GetType();
                        if (attrType.Name != MetadataTypeAttributeTypeShortName)
                            continue;
                        PropertyInfo property = attrType.GetPropertyEx(MetadataClassTypeProperty, InstancePublicFlags);
                        if (property == null || !property.PropertyType.Equals(typeof(Type)))
                            continue;
                        var metaType = property.GetValueEx<Type>(attribute);
                        if (metaType != null)
                            result.Add(metaType);
                    }
                    if (Tracer.TraceInformation)
                    {
                        foreach (var metaType in result)
                            Tracer.Info("Added MetadataTypeAttribute for type: {0}, MetadataClassType: {1}", type, metaType);
                    }
                    list = result.ToArrayEx();
                    MetadataTypeCache[type] = list;
                }
                return list;
            }
        }

        private static Func<string> GetDisplayNameAccessor(MemberInfo member)
        {
            if (DisplayNameProvider == null && !ServiceProvider.TryGet(out DisplayNameProvider))
                DisplayNameProvider = new DisplayNameProvider();
            return DisplayNameProvider.GetDisplayNameAccessor(member);
        }

        #endregion
    }
}
