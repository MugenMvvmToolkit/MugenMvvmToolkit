using System;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Collections;
using MugenMvvm.Interfaces.Commands;
using MugenMvvm.Interfaces.Commands.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Extensions.Components
{
    public static class CommandComponentExtensions
    {
        public static ICompositeCommand? TryGetCommand<TParameter>(this ItemOrArray<ICommandProviderComponent> components, ICommandManager commandManager, object? owner,
            object request, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(commandManager, nameof(commandManager));
            Should.NotBeNull(request, nameof(request));
            foreach (var c in components)
            {
                var result = c.TryGetCommand<TParameter>(commandManager, owner, request, metadata);
                if (result != null)
                    return result;
            }

            return null;
        }

        public static void OnCommandCreated<TParameter>(this ItemOrArray<ICommandManagerListener> listeners, ICommandManager commandManager, ICompositeCommand command,
            object? owner, object request,
            IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(commandManager, nameof(commandManager));
            Should.NotBeNull(command, nameof(command));
            foreach (var c in listeners)
                c.OnCommandCreated<TParameter>(commandManager, command, owner, request, metadata);
        }

        public static bool HasCanExecute(this ItemOrArray<ICommandConditionComponent> components, ICompositeCommand command, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(command, nameof(command));
            foreach (var c in components)
                if (c.HasCanExecute(command, metadata))
                    return true;

            return false;
        }

        public static bool CanExecute(this ItemOrArray<ICommandConditionComponent> components, ICompositeCommand command, object? parameter, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(command, nameof(command));
            foreach (var c in components)
                if (!c.CanExecute(command, parameter, metadata))
                    return false;

            return true;
        }

        public static void AddCanExecuteChanged(this ItemOrArray<ICommandEventHandlerComponent> components, ICompositeCommand command, EventHandler? handler,
            IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(command, nameof(command));
            Should.NotBeNull(handler, nameof(handler));
            foreach (var c in components)
                c.AddCanExecuteChanged(command, handler, metadata);
        }

        public static void RemoveCanExecuteChanged(this ItemOrArray<ICommandEventHandlerComponent> components, ICompositeCommand command, EventHandler? handler,
            IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(command, nameof(command));
            Should.NotBeNull(handler, nameof(handler));
            foreach (var c in components)
                c.RemoveCanExecuteChanged(command, handler, metadata);
        }

        public static void RaiseCanExecuteChanged(this ItemOrArray<ICommandEventHandlerComponent> components, ICompositeCommand command, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(command, nameof(command));
            foreach (var c in components)
                c.RaiseCanExecuteChanged(command, metadata);
        }

        public static Task ExecuteAsync(this ItemOrArray<ICommandExecutorComponent> components, ICompositeCommand command, object? parameter, CancellationToken cancellationToken,
            IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(command, nameof(command));
            return components.InvokeAllAsync((command, parameter), cancellationToken, metadata, (component, s, c, m) => component.ExecuteAsync(s.command, s.parameter, c, m));
        }
    }
}