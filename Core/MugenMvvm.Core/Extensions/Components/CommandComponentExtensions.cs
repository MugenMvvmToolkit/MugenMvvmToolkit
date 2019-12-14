using System;
using System.Threading.Tasks;
using MugenMvvm.Interfaces.Commands;
using MugenMvvm.Interfaces.Commands.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Extensions.Components
{
    public static class CommandComponentExtensions
    {
        #region Methods

        public static ICompositeCommand? TryGetCommand<TRequest>(this ICommandProviderComponent[] components, in TRequest request, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            for (var i = 0; i < components.Length; i++)
            {
                var result = components[i].TryGetCommand(request, metadata);
                if (result != null)
                    return result;
            }

            return null;
        }

        public static void OnCommandCreated<TRequest>(this ICommandProviderListener[] listeners, ICommandProvider provider, in TRequest request, ICompositeCommand command, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(listeners, nameof(listeners));
            Should.NotBeNull(provider, nameof(provider));
            Should.NotBeNull(command, nameof(command));
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnCommandCreated(provider, request, command, metadata);
        }

        public static bool HasCanExecute(this IConditionCommandComponent[] components)
        {
            Should.NotBeNull(components, nameof(components));
            for (var i = 0; i < components.Length; i++)
            {
                if (components[i].HasCanExecute())
                    return true;
            }

            return false;
        }

        public static bool CanExecute(this IConditionCommandComponent[] components, object? parameter)
        {
            Should.NotBeNull(components, nameof(components));
            for (var i = 0; i < components.Length; i++)
            {
                if (!components[i].CanExecute(parameter))
                    return false;
            }

            return true;
        }

        public static void AddCanExecuteChanged(this IConditionEventCommandComponent[] components, EventHandler handler)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(handler, nameof(handler));
            for (var i = 0; i < components.Length; i++)
                components[i].AddCanExecuteChanged(handler);
        }

        public static void RemoveCanExecuteChanged(this IConditionEventCommandComponent[] components, EventHandler handler)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(handler, nameof(handler));
            for (var i = 0; i < components.Length; i++)
                components[i].RemoveCanExecuteChanged(handler);
        }

        public static void RaiseCanExecuteChanged(this IConditionEventCommandComponent[] components)
        {
            Should.NotBeNull(components, nameof(components));
            for (var i = 0; i < components.Length; i++)
                components[i].RaiseCanExecuteChanged();
        }

        public static Task ExecuteAsync(this IExecutorCommandComponent[] components, object? parameter)
        {
            Should.NotBeNull(components, nameof(components));
            if (components.Length == 0)
                return Task.CompletedTask;
            if (components.Length == 1)
                return components[0].ExecuteAsync(parameter);
            var tasks = new Task[components.Length];
            for (var i = 0; i < components.Length; i++)
                tasks[i] = components[i].ExecuteAsync(parameter);
            return Task.WhenAll(tasks);
        }

        #endregion
    }
}