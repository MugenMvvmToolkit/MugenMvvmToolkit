using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MugenMvvmToolkit.Interfaces.Requests;

namespace MugenMvvmToolkit.Infrastructure.Requests
{
	public abstract class RequestHandlerBase<TRequest, TResponse>
		where TRequest : IRequest<TResponse>
		where TResponse : ResponseBase, new()
	{

		IRequestHandlerProvider _requestHandlerProvider;

		IRequestHandlerProvider RequestHandler
		{
			get
			{
				if (_requestHandlerProvider != null)
					return _requestHandlerProvider;
				_requestHandlerProvider = ToolkitServiceProvider.RequestHandlerProvider;
				return _requestHandlerProvider;
			}
		}

		protected Task<TInternalResponse> SendAsync<TInternalResponse>(IRequest<TInternalResponse> request) where TInternalResponse : ResponseBase, new()
		{
			return RequestHandler.SendAsync(request);
		}

		protected abstract Task<TResponse> HandleAsync(TRequest request);

		protected Task<TResponse> Ok(TResponse response)
		{
#if NET_STANDARD
			return Task.FromResult(response);
#else
			return new Task<TResponse>(() => response);
#endif
		}

		protected Task<TResponse> Fail(string errorCode)
		{
#if NET_STANDARD
			return Task.FromResult(new TResponse()
			{
				ResponseStatus = new ResponseStatus(errorCode)
			});
#else
			return new Task<TResponse>(() => new TResponse()
			{
				ResponseStatus = new ResponseStatus(errorCode)
			});
#endif
		}

		protected Task<TResponse> Fail(string errorCode, string message)
		{
#if NET_STANDARD
			return Task.FromResult(new TResponse()
			{
				ResponseStatus = new ResponseStatus(errorCode, message)
			});
#else
			return new Task<TResponse>(() => new TResponse()
			{
				ResponseStatus = new ResponseStatus(errorCode, message)
			});
#endif
		}

		protected Task<TResponse> FailWithUnauthorized()
		{
#if NET_STANDARD
			return Task.FromResult(new TResponse()
			{
				ResponseStatus = new ResponseStatus(nameof(ResponseBase.Unauthorized), ResponseBase.Unauthorized)
			});
#else
			return new Task<TResponse>(() => new TResponse()
			{
				ResponseStatus = new ResponseStatus(nameof(ResponseBase.Unauthorized), ResponseBase.Unauthorized)
			});
#endif
		}

		protected Task<TResponse> FailWithException(Exception e)
		{

			var response = new ResponseStatus(nameof(ResponseBase.UnhandledException), ResponseBase.UnhandledException)
			{
				Errors = new List<ResponseError>()
				{
					new ResponseError()
					{
						ErrorCode = e.GetType().ToString(),
						Message = e.Message,
						StackTrace = e.StackTrace
					}
				}
			};
#if NET_STANDARD
			return Task.FromResult(new TResponse()
			{
				ResponseStatus = response
			});
#else
			return new Task<TResponse>(() => new TResponse()
			{
				ResponseStatus = response
			});
#endif
		}

		protected Task<TResponse> FailWithNoNetwork()
		{
#if NET_STANDARD
			return Task.FromResult(new TResponse()
			{
				ResponseStatus = new ResponseStatus(nameof(ResponseBase.NoNetwork), ResponseBase.NoNetwork)
			});
#else
			return new Task<TResponse>(() => new TResponse()
			{
				ResponseStatus = new ResponseStatus(nameof(ResponseBase.NoNetwork), ResponseBase.NoNetwork)
			});
#endif
		}
	}
}