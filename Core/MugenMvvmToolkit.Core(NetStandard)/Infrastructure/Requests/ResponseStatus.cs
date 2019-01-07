using System.Collections.Generic;

namespace MugenMvvmToolkit.Infrastructure.Requests
{
	public sealed class ResponseStatus
	{
		public ResponseStatus()
		{
		}

		public ResponseStatus(string errorCode)
		{
			ErrorCode = errorCode;
		}

		public ResponseStatus(string errorCode, string message)
		{
			ErrorCode = errorCode;
			Message = message;
		}

		public string ErrorCode { get; set; }
		public string Message { get; set; }
		public List<ResponseError> Errors { get; set; }
		public Dictionary<string, string> Meta { get; set; }
	}
}