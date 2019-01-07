using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MugenMvvmToolkit.Attributes;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Requests;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Models.IoC;

namespace MugenMvvmToolkit.Infrastructure.Requests
{
	public class RequestHandlerProvider : IRequestHandlerProvider
	{
		private readonly IIocContainer _iocContainer;
		private IEnumerable<Assembly> _assemblies;
		private Dictionary<Type, Type> _registeredHandlers;

		[Preserve(Conditional = true)]
		public RequestHandlerProvider([NotNull] IIocContainer iocContainer, [NotNull] IEnumerable<Assembly> assemblies)
		{
			Should.NotBeNull(iocContainer, nameof(iocContainer));
			Should.NotBeNull(assemblies, nameof(assemblies));
			_iocContainer = iocContainer;
			_assemblies = assemblies;
			_registeredHandlers = new Dictionary<Type, Type>();
		}

		public Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request) where TResponse : ResponseBase, new()
		{
			EnsureInitialized();

			if (!_registeredHandlers.TryGetValue(request.GetType(), out var handlerType))
				throw ExceptionManager.RequestHandlerNotFound(request.GetType());
			if (!_iocContainer.TryGet(handlerType, out var handler))
				throw ExceptionManager.RequestHandlerNotRegistered(handlerType);

			var method = handlerType.GetMethodEx("HandleAsync", MemberFlags.NonPublic | MemberFlags.Instance);
			var result = (Task<TResponse>)method.Invoke(handler, new object[] { request });
			return result;
		}

		private void EnsureInitialized()
		{
			if (_assemblies == null)
				return;
			if (ApplicationSettings.RequestHandlerProviderDisableAutoRegistration)
			{
				_assemblies = null;
				return;
			}

			lock (_registeredHandlers)
			{
				var assemblies = _assemblies;
				_assemblies = null;
				if (ToolkitServiceProvider.IsDesignMode)
					assemblies = assemblies.FilterDesignAssemblies();

				if (!ApplicationSettings.RequestHandlerProviderDisableAutoRegistration)
				{
					//TODO
					ToolkitServiceProvider.BootstrapCodeBuilder?.AppendStatic(nameof(ApplicationSettings),
						$"{typeof(ApplicationSettings).FullName}.{nameof(ApplicationSettings.RequestHandlerProviderDisableAutoRegistration)} = true;");
				}

				InitializeMapping(assemblies.SelectMany(assembly =>
					assembly.SafeGetTypes(!ToolkitServiceProvider.IsDesignMode)));
			}
		}

		protected virtual void InitializeMapping(IEnumerable<Type> types)
		{
			foreach (var type in types)
			{
#if NET_STANDARD
				var typeInfo = type.GetTypeInfo();
				if (typeInfo.IsAbstract || typeInfo.IsInterface)
					continue;

#else
                if (type.IsAbstract || type.IsInterface)
                    continue;

#endif

				if (InheritsOrImplements(type, typeof(RequestHandlerBase<,>)))
				{
					var requestType = BaseType(type).GetGenericArguments()[0];
					AddMapping(requestType, type);
				}

			}
		}

		public void AddMapping(Type request, Type requestHandler)
		{
			if (!_registeredHandlers.ContainsKey(request))
			{
				_registeredHandlers.Add(request, requestHandler);
			}
			else
			{
				_registeredHandlers[request] = requestHandler;
				_iocContainer.Unbind(requestHandler);
			}
			_iocContainer.Bind(requestHandler, requestHandler, DependencyLifecycle.SingleInstance);

			if (Tracer.TraceInformation)
				Tracer.Info("The request handler mapping to request was created: ({0} ---> {1})",
					request, requestHandler);
		}

		static bool InheritsOrImplements(Type child, Type parent)
		{
			parent = ResolveGenericTypeDefinition(parent);

			var currentChild = IsGenericType(child)
				? child.GetGenericTypeDefinition()
				: child;

			while (currentChild != typeof(object))
			{
				if (parent == currentChild || HasAnyInterfaces(parent, currentChild))
					return true;

				currentChild = BaseType(currentChild) != null
				               && IsGenericType(BaseType(currentChild))
					? BaseType(currentChild).GetGenericTypeDefinition()
					: BaseType(currentChild);

				if (currentChild == null)
					return false;
			}
			return false;
		}

		private static bool HasAnyInterfaces(Type parent, Type child)
		{
			return child.GetInterfaces()
				.Any(childInterface =>
				{
					var currentInterface = IsGenericType(childInterface)
						? childInterface.GetGenericTypeDefinition()
						: childInterface;

					return currentInterface == parent;
				});
		}

		private static Type ResolveGenericTypeDefinition(Type parent)
		{
			var shouldUseGenericType = !(IsGenericType(parent) && parent.GetGenericTypeDefinition() != parent);

			if (IsGenericType(parent) && shouldUseGenericType)
				parent = parent.GetGenericTypeDefinition();
			return parent;
		}

		private static bool IsGenericType(Type type)
		{
#if NET_STANDARD
			return type.GetTypeInfo().IsGenericType;
#else
			return type.IsGenericType;
#endif
		}

		private static Type BaseType(Type type)
		{
#if NET_STANDARD
			return type.GetTypeInfo().BaseType;
#else
			return type.BaseType;
#endif
		}
	}
}