using System.Collections.Generic;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Metadata;
using MugenMvvm.Tests.Metadata;

namespace MugenMvvm.UnitTests.Metadata
{
    public class MetadataContextValueManagerComponentTest : MetadataContextTest
    {
        protected override MetadataContext GetMetadataContext(IReadOnlyCollection<KeyValuePair<IMetadataContextKey, object?>>? values = null)
        {
            var dict = new Dictionary<IMetadataContextKey, object?>();
            var ctx = base.GetMetadataContext();
            ctx.AddComponent(new TestMetadataValueManagerComponent
            {
                GetCount = c =>
                {
                    c.ShouldEqual(ctx);
                    return dict.Count;
                },
                Contains = (c, key) =>
                {
                    c.ShouldEqual(ctx);
                    return dict.ContainsKey(key);
                },
                Clear = c =>
                {
                    c.ShouldEqual(ctx);
                    dict.Clear();
                },
                TryClear = (c, key) =>
                {
                    c.ShouldEqual(ctx);
                    return dict.Remove(key);
                },
                TryGetValue = (c, key, type) =>
                {
                    c.ShouldEqual(ctx);
                    var r = dict.TryGetValue(key, out var value);
                    return (r, value);
                },
                GetValues = c =>
                {
                    c.ShouldEqual(ctx);
                    return dict;
                },
                TrySetValue = (c, key, o) =>
                {
                    c.ShouldEqual(ctx);
                    dict[key] = o;
                    return true;
                }
            });
            if (values != null)
                ctx.Merge(values);
            return ctx;
        }
    }
}