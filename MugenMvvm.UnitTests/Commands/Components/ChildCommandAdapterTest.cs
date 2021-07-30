﻿using System.Threading.Tasks;
using MugenMvvm.Commands;
using MugenMvvm.Commands.Components;
using MugenMvvm.Extensions;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Commands.Components
{
    public class ChildCommandAdapterTest : UnitTestBase
    {
        private readonly ChildCommandAdapter _adapter;

        public ChildCommandAdapterTest()
        {
            _adapter = new ChildCommandAdapter();
            Command.AddComponent(new CommandEventHandler());
            Command.AddComponent(_adapter);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void AddRemoveCommandShouldRaiseCommandChanged(bool extMethod)
        {
            var cmd = CompositeCommand.Create(this, commandManager: CommandManager);
            var invokeCount = 0;
            Command.CanExecuteChanged += (sender, _) =>
            {
                sender.ShouldEqual(Command);
                ++invokeCount;
            };

            if (extMethod)
                Command.AddChildCommand(cmd);
            else
                _adapter.Add(cmd);
            _adapter.Contains(cmd).ShouldBeTrue();
            invokeCount.ShouldEqual(1);

            if (extMethod)
                Command.RemoveChildCommand(cmd);
            else
                _adapter.Remove(cmd);
            _adapter.Contains(cmd).ShouldBeFalse();
            invokeCount.ShouldEqual(2);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CanExecuteShouldBeHandledByCommands(bool canExecuteIfAnyCanExecute)
        {
            _adapter.CanExecuteIfAnyCanExecute = canExecuteIfAnyCanExecute;
            var parameter = this;
            var canExecute1 = false;
            var canExecute2 = false;

            void Assert()
            {
                if (canExecuteIfAnyCanExecute)
                    Command.CanExecute(parameter, Metadata).ShouldEqual(canExecute1 || canExecute2);
                else
                    Command.CanExecute(parameter, Metadata).ShouldEqual(canExecute1 && canExecute2);
            }

            var cmd1 = CompositeCommand.Create<object?>(this, (_, _) => { }, (p, m) =>
            {
                p.ShouldEqual(parameter);
                m.ShouldEqual(Metadata);
                return canExecute1;
            });
            var cmd2 = CompositeCommand.Create<object?>(this, (_, _) => { }, (p, m) =>
            {
                p.ShouldEqual(parameter);
                m.ShouldEqual(Metadata);
                return canExecute2;
            });

            Command.AddChildCommand(cmd1);
            Command.AddChildCommand(cmd2);
            Assert();

            canExecute1 = true;
            Assert();

            canExecute1 = true;
            canExecute2 = true;
            Assert();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CanExecuteShouldReturnDefaultResultNoCommands(bool result)
        {
            _adapter.CanExecuteEmptyResult = result;
            Command.CanExecute(null, Metadata).ShouldEqual(result);
        }

        [Fact]
        public async Task ExecuteAsyncShouldBeHandledByCommands()
        {
            var parameter = this;
            var cmd1Count = 0;
            var cmd2Count = 0;
            var cmd1 = CompositeCommand.Create<object?>(this, (p, m) =>
            {
                p.ShouldEqual(parameter);
                m.ShouldEqual(Metadata);
                ++cmd1Count;
            });
            var cmd2 = CompositeCommand.CreateFromTask<object?>(this, (p, c, m) =>
            {
                p.ShouldEqual(parameter);
                c.ShouldEqual(DefaultCancellationToken);
                m.ShouldEqual(Metadata);
                ++cmd2Count;
                return Task.CompletedTask;
            });

            Command.AddChildCommand(cmd1);
            Command.AddChildCommand(cmd2);
            (await Command.ExecuteAsync(parameter, DefaultCancellationToken, Metadata)).ShouldBeTrue();

            cmd1Count.ShouldEqual(1);
            cmd2Count.ShouldEqual(1);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task ExecuteAsyncShouldBeHandledByCommandsSequentially(bool result)
        {
            var parameter = this;
            var cmd1Count = 0;
            var cmd2Count = 0;
            var cmd1 = CompositeCommand.CreateFromTask<object?>(this, (p, c, m) =>
            {
                p.ShouldEqual(parameter);
                m.ShouldEqual(Metadata);
                ++cmd1Count;
                return Task.FromResult(result);
            });
            var cmd2 = CompositeCommand.CreateFromTask<object?>(this, (p, c, m) =>
            {
                p.ShouldEqual(parameter);
                c.ShouldEqual(DefaultCancellationToken);
                m.ShouldEqual(Metadata);
                ++cmd2Count;
                return Task.FromResult(true);
            });

            Command.AddChildCommand(cmd1);
            Command.AddChildCommand(cmd2);
            Command.GetComponent<ChildCommandAdapter>().ExecuteSequentially = true;
            (await Command.ExecuteAsync(parameter, DefaultCancellationToken, Metadata)).ShouldBeTrue();

            cmd1Count.ShouldEqual(1);
            cmd2Count.ShouldEqual(result ? 0 : 1);
        }

        [Fact]
        public void ShouldListenCanExecuteChangedFromChildCommands()
        {
            var cmd = CompositeCommand.Create(this, commandManager: CommandManager);
            Command.AddChildCommand(cmd);

            var invokeCount = 0;
            Command.CanExecuteChanged += (sender, _) =>
            {
                sender.ShouldEqual(Command);
                ++invokeCount;
            };
            invokeCount.ShouldEqual(0);

            cmd.RaiseCanExecuteChanged(Metadata);
            invokeCount.ShouldEqual(1);

            cmd.RaiseCanExecuteChanged(Metadata);
            invokeCount.ShouldEqual(2);
        }

        [Fact]
        public async Task SuppressExecuteShouldIgnoreExecute()
        {
            var count = 0;
            var cmd = CompositeCommand.Create<object?>(this, (p, m) => { ++count; }, commandManager: CommandManager);

            Command.AddChildCommand(cmd);
            (await Command.ExecuteAsync(null, DefaultCancellationToken, Metadata)).ShouldBeTrue();
            count.ShouldEqual(1);

            _adapter.SuppressExecute = true;
            (await Command.ExecuteAsync(null, DefaultCancellationToken, Metadata)).ShouldBeFalse();
            count.ShouldEqual(1);

            _adapter.SuppressExecute = false;
            (await Command.ExecuteAsync(null, DefaultCancellationToken, Metadata)).ShouldBeTrue();
            count.ShouldEqual(2);
        }

        [Fact]
        public void SuppressExecuteShouldRaiseCanExecute()
        {
            var cmd = CompositeCommand.Create(this, commandManager: CommandManager);
            Command.AddChildCommand(cmd);

            var invokeCount = 0;
            Command.CanExecuteChanged += (sender, _) =>
            {
                sender.ShouldEqual(Command);
                ++invokeCount;
            };
            invokeCount.ShouldEqual(0);

            _adapter.SuppressExecute = true;
            invokeCount.ShouldEqual(1);

            _adapter.SuppressExecute = false;
            invokeCount.ShouldEqual(2);
        }

        [Fact]
        public void SuppressExecuteShouldReturnCanExecuteTrue()
        {
            var cmd = CompositeCommand.Create<object?>(this, (_, _) => { }, (p, m) => false, commandManager: CommandManager);
            Command.AddChildCommand(cmd);

            Command.CanExecute(null, Metadata).ShouldBeFalse();

            _adapter.SuppressExecute = true;
            Command.CanExecute(null, Metadata).ShouldBeTrue();

            _adapter.SuppressExecute = false;
            Command.CanExecute(null, Metadata).ShouldBeFalse();
        }
    }
}