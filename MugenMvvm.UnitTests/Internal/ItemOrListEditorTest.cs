using System;
using System.Collections.Generic;
using MugenMvvm.Extensions;
using MugenMvvm.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Internal
{
    public class ItemOrListEditorTest
    {
        #region Methods

        [Fact]
        public void IsNullOrEmptyShouldBeTrueDefault()
        {
            ItemOrListEditor<object, object[]> editor = default;
            editor.IsNullOrEmpty.ShouldBeTrue();
        }

        [Fact]
        public void IndexShouldThrowOutOfRange()
        {
            var iterator = new ItemOrListEditor<object, object[]>(o => o == null, () => new object[0]);
            try
            {
                var o = iterator[0];
                throw new NotSupportedException();
            }
            catch (ArgumentOutOfRangeException)
            {
                ;
            }
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(10)]
        public void IndexCountToItemOrListGetRawValueInternalShouldBeCorrect(int count)
        {
            var objects = new List<object>();
            for (var i = 0; i < count; i++)
                objects.Add(new object());
            var itemOrList = ItemOrList.FromList(objects);

            var editor = new ItemOrListEditor<object, List<object>>(itemOrList.Item, itemOrList.List, o => o == null, () => new List<object>());
            editor.Count.ShouldEqual(objects.Count);
            editor.IsNullOrEmpty.ShouldEqual(count == 0);
            for (var i = 0; i < editor.Count; i++)
                objects[i].ShouldEqual(editor[i]);

            editor.GetRawValueInternal().ShouldEqual(itemOrList.GetRawValue());

            var editorItemOrList = editor.ToItemOrList();
            editorItemOrList.Item.ShouldEqual(itemOrList.Item);
            editorItemOrList.List.ShouldEqual(itemOrList.List);

            editorItemOrList = editor.ToItemOrList<List<object>>();
            editorItemOrList.Item.ShouldEqual(itemOrList.Item);
            editorItemOrList.List.ShouldEqual(itemOrList.List);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(10)]
        public void AddRangeShouldAddItemOrList(int count)
        {
            var objects = new List<object>();
            for (var i = 0; i < count; i++)
                objects.Add(new object());
            var itemOrList = ItemOrList.FromList(objects);

            var editor = new ItemOrListEditor<object, List<object>>(null, null, o => o == null, () => new List<object>());
            editor.AddRange(itemOrList);

            var editorItemOrList = editor.ToItemOrList();
            editorItemOrList.Item.ShouldEqual(itemOrList.Item);
            editorItemOrList.List.ShouldEqual(itemOrList.List);

            editor.AddRange(itemOrList);
            objects.AddRange(objects);

            itemOrList = ItemOrList.FromList(objects);
            editorItemOrList = editor.ToItemOrList();
            editorItemOrList.Item.ShouldEqual(itemOrList.Item);
            editorItemOrList.List.ShouldEqual(itemOrList.List);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(10)]
        public void AddRemoveShouldBeCorrect(int count)
        {
            var editor = new ItemOrListEditor<object, List<object>>(null, null, o => o == null, () => new List<object>());
            var objects = new List<object>();
            for (var i = 0; i < count; i++)
            {
                var o = new object();
                objects.Add(o);
                editor.Add(o);
            }

            var itemOrList = ItemOrList.FromList(objects);
            var editorItemOrList = editor.ToItemOrList();
            editorItemOrList.Item.ShouldEqual(itemOrList.Item);
            editorItemOrList.List.ShouldEqual(itemOrList.List);

            var array = objects.ToArray();
            for (var i = 0; i < count; i++)
            {
                objects.Remove(array[i]);
                editor.Remove(array[i]);

                itemOrList = ItemOrList.FromList(objects);
                editorItemOrList = editor.ToItemOrList();
                editorItemOrList.Item.ShouldEqual(itemOrList.Item);
                editorItemOrList.List.ShouldEqual(itemOrList.List);
            }
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(10)]
        public void AddRemoveAtShouldBeCorrect(int count)
        {
            var editor = new ItemOrListEditor<object, List<object>>(null, null, o => o == null, () => new List<object>());
            var objects = new List<object>();
            for (var i = 0; i < count; i++)
            {
                var o = new object();
                objects.Add(o);
                editor.Add(o);
            }

            var itemOrList = ItemOrList.FromList(objects);
            var editorItemOrList = editor.ToItemOrList();
            editorItemOrList.Item.ShouldEqual(itemOrList.Item);
            editorItemOrList.List.ShouldEqual(itemOrList.List);

            for (var i = 0; i < count; i++)
            {
                objects.RemoveAt(0);
                editor.RemoveAt(0);

                itemOrList = ItemOrList.FromList(objects);
                editorItemOrList = editor.ToItemOrList();
                editorItemOrList.Item.ShouldEqual(itemOrList.Item);
                editorItemOrList.List.ShouldEqual(itemOrList.List);
            }
        }

        #endregion
    }
}