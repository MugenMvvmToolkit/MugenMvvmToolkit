using System.Collections.Generic;

namespace MugenMvvmToolkit.Infrastructure.Requests
{
	public sealed class ResponseError
	{
		public string ErrorCode { get; set; }
		public string FieldName { get; set; }
		public string Message { get; set; }
		public string StackTrace { get; set; }
		public Dictionary<string, string> Meta { get; set; }
	}
}