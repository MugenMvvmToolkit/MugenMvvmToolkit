namespace MugenMvvm.Interfaces.Requests
{
    public interface ICancelableRequest
    {
        object? State { get; set; }

        bool? Cancel { get; set; }
    }
}