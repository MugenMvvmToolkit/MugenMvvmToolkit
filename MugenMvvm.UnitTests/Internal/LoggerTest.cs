using System;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Internal;
using MugenMvvm.Internal.Components;
using MugenMvvm.UnitTests.Components;
using MugenMvvm.UnitTests.Internal.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Internal
{
    public class LoggerTest : ComponentOwnerTestBase<Logger>
    {
        #region Methods

        [Fact]
        public void CanLogShouldReturnFalseNoComponents() => new Logger().CanLog(LogLevel.Info, DefaultMetadata).ShouldBeFalse();

        [Fact]
        public void LogShouldNotThrowNoComponents() => new Logger().Log(LogLevel.Info, string.Empty, null, DefaultMetadata);

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
            var logger = new Logger();
            var invokeCount = 0;
            for (var i = 0; i < count; i++)
            {
                var canLog = count - 1 == i;
                var component = new DelegateLogger((level, s, arg3, arg4) => { }, (level, metadata) =>
                {
                    ++invokeCount;
                    level.ShouldEqual(logLevel);
                    if (withMetadata)
                        metadata.ShouldEqual(DefaultMetadata);
                    else
                        metadata.ShouldBeNull();
                    return canLog;
                });
                component.Priority = -i;
                logger.AddComponent(component);
            }

            logger.CanLog(logLevel, withMetadata ? DefaultMetadata : null).ShouldEqual(true);
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
            var logger = new Logger();
            var invokeCount = 0;
            for (var i = 0; i < count; i++)
            {
                var component = new DelegateLogger((level, m, exc, metadata) =>
                {
                    ++invokeCount;
                    message.ShouldEqual(m);
                    exception.ShouldEqual(exc);
                    level.ShouldEqual(logLevel);
                    if (withMetadata)
                        metadata.ShouldEqual(DefaultMetadata);
                    else
                        metadata.ShouldBeNull();
                }, (level, context) => true);
                component.Priority = -i;
                logger.AddComponent(component);
            }

            logger.Log(logLevel, message, exception, withMetadata ? DefaultMetadata : null);
            invokeCount.ShouldEqual(count);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void GetLoggerShouldBeHandledByComponents(int count)
        {
            var logger = new Logger();
            var request = new object();
            var result = new Logger();
            var invokeCount = 0;
            for (var i = 0; i < count; i++)
            {
                var isLast = count - 1 == i;
                logger.AddComponent(new TestLoggerProviderComponent(logger)
                {
                    Priority = -i,
                    TryGetLogger = (o, context) =>
                    {
                        ++invokeCount;
                        o.ShouldEqual(request);
                        context.ShouldEqual(DefaultMetadata);
                        if (isLast)
                            return result;
                        return null;
                    }
                });
            }

            logger.GetLogger(request, DefaultMetadata).ShouldEqual(result);
            invokeCount.ShouldEqual(count);
        }

        protected override Logger GetComponentOwner(IComponentCollectionManager? collectionProvider = null) => new Logger(collectionProvider);

        #endregion
    }
}