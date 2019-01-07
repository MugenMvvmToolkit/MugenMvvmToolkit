namespace MugenMvvmToolkit.Infrastructure.Requests
{
	public class ResponseBase
	{
		public ResponseStatus ResponseStatus { get; set; }

		public bool IsSuccess()
		{
			return ResponseStatus == null;
		}

		public bool IsFailure() => !IsSuccess();

		public bool IsNoNetworkFailure()
		{
			if (IsSuccess()) return false;
			return ResponseStatus.ErrorCode == nameof(NoNetwork);
		}

		public bool IsUnauthorized()
		{
			if (IsSuccess()) return false;
			return ResponseStatus.ErrorCode == nameof(Unauthorized);
		}

		internal const string NoNetwork = "No network connection available";
		internal const string UnhandledException = "Unexpected error occured";
		internal const string Unauthorized = "Not authorized to perform this action";
	}
}