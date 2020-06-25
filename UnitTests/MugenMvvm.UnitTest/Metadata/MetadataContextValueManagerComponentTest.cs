using System.Collections.Generic;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Metadata;
using MugenMvvm.UnitTest.Metadata.Internal;

namespace MugenMvvm.UnitTest.Metadata
{
    public class MetadataContextValueManagerComponentTest : MetadataContextTest
    {
        #region Methods

        protected override MetadataContext GetMetadataContext(IReadOnlyCollection<MetadataContextValue>? values = null)
        {
            var dict = new Dictionary<IMetadataContextKey, object?>();
            var ctx = base.GetMetadataContext();
            ctx.AddComponent(new TestMetadataContextValueManagerComponent
            {
                GetCount = () => dict.Count,
                Contains = key => dict.ContainsKey(key),
                Clear = () => dict.Clear(),
                TryClear = key => dict.Remove(key),
                TryGetValue = key =>
                {
                    var r = dict.TryGetValue(key, out var value);
                    return (r, value);
                },
                GetValues = () => dict,
                TrySetValue = (key, o) =>
                {
                    dict[key] = o;
                    return true;
                }
            });
            if (values != null)
                ctx.Merge(values);
            return ctx;
        }

        #endregion
    }
}