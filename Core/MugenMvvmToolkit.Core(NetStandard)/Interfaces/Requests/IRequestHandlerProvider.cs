using System;
using System.Threading.Tasks;
using MugenMvvmToolkit.Infrastructure.Requests;

namespace MugenMvvmToolkit.Interfaces.Requests
{
	public interface IRequestHandlerProvider
	{
		Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request) where TResponse : ResponseBase, new();
		void AddMapping(Type request, Type requestHandler);
	}
}