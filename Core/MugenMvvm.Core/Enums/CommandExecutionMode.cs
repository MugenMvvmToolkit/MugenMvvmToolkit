namespace MugenMvvm.Enums
{
    public enum CommandExecutionMode : byte
    {
        None = 0,
        CanExecuteBeforeExecute = 1,
        CanExecuteBeforeExecuteWithException = 2
    }
}