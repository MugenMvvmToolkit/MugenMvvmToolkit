using System;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Internal;
using MugenMvvm.Internal.Components;
using MugenMvvm.Tests.Internal;
using MugenMvvm.UnitTests.Components;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Internal
{
    public class LoggerTest : ComponentOwnerTestBase<Logger>
    {
        [Theory]
        [InlineData(1, "Info", true)]
        [InlineData(1, "Info", false)]
        [InlineData(1, "Warning", false)]
        [InlineData(10, "Info", true)]
        [InlineData(10, "Info", false)]
        [InlineData(10, "Error", false)]
        public void CanLogShouldBeHandledByComponents(int count, string l, bool withMetadata)
        {
            var logLevel = LogLevel.Get(l);
            var invokeCount = 0;
            for (var i = 0; i < count; i++)
            {
                var canLog = count - 1 == i;
                Logger.AddComponent(new DelegateLogger((level, s, arg3, arg4) => { }, (level, metadata) =>
                {
                    ++invokeCount;
                    level.ShouldEqual(logLevel);
                    if (withMetadata)
                        metadata.ShouldEqual(DefaultMetadata);
                    else
                        metadata.ShouldBeNull();
                    return canLog;
                }) { Priority = -i });
            }

            Logger.CanLog(logLevel, withMetadata ? DefaultMetadata : null).ShouldEqual(true);
            invokeCount.ShouldEqual(count);
        }

        [Fact]
        public void CanLogShouldReturnFalseNoComponents() => Logger.CanLog(LogLevel.Info, DefaultMetadata).ShouldBeFalse();

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void GetLoggerShouldBeHandledByComponents(int count)
        {
            var request = new object();
            var result = new Logger(ComponentCollectionManager);
            var invokeCount = 0;
            for (var i = 0; i < count; i++)
            {
                var isLast = count - 1 == i;
                Logger.AddComponent(new TestLoggerProviderComponent
                {
                    Priority = -i,
                    TryGetLogger = (l, o, context) =>
                    {
                        l.ShouldEqual(Logger);
                        o.ShouldEqual(request);
                        context.ShouldEqual(DefaultMetadata);
                        ++invokeCount;
                        if (isLast)
                            return result;
                        return null;
                    }
                });
            }

            Logger.GetLogger(request, DefaultMetadata).ShouldEqual(result);
            invokeCount.ShouldEqual(count);
        }

        [Theory]
        [InlineData(1, "Info", true)]
        [InlineData(1, "Info", false)]
        [InlineData(1, "Warning", false)]
        [InlineData(10, "Info", true)]
        [InlineData(10, "Info", false)]
        [InlineData(10, "Error", false)]
        public void LogShouldBeHandledByComponents(int count, string l, bool withMetadata)
        {
            var logLevel = LogLevel.Get(l);
            var message = l;
            var exception = withMetadata ? new Exception() : null;
            var invokeCount = 0;
            for (var i = 0; i < count; i++)
            {
                Logger.AddComponent(new DelegateLogger((level, m, exc, metadata) =>
                {
                    ++invokeCount;
                    message.ShouldEqual(m);
                    exception.ShouldEqual(exc);
                    level.ShouldEqual(logLevel);
                    if (withMetadata)
                        metadata.ShouldEqual(DefaultMetadata);
                    else
                        metadata.ShouldBeNull();
                }, (level, context) => true) { Priority = -i });
            }

            Logger.Log(logLevel, message, exception, withMetadata ? DefaultMetadata : null);
            invokeCount.ShouldEqual(count);
        }

        [Fact]
        public void LogShouldNotThrowNoComponents() => Logger.Log(LogLevel.Info, string.Empty, null, DefaultMetadata);

        protected override ILogger GetLogger() => GetComponentOwner(ComponentCollectionManager);

        protected override Logger GetComponentOwner(IComponentCollectionManager? componentCollectionManager = null) => new(componentCollectionManager);
    }
}