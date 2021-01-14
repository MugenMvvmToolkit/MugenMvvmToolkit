using System;
using System.Collections.Generic;
using MugenMvvm.Collections;
using MugenMvvm.Extensions;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Collections
{
    public class ItemOrListEditorTest
    {
        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(10)]
        public void IndexCountToItemOrListGetRawValueInternalShouldBeCorrect(int count)
        {
            var objects = new List<object>();
            for (var i = 0; i < count; i++)
                objects.Add(new object());
            ItemOrIReadOnlyList<object> itemOrList = objects;

            var editor = new ItemOrListEditor<object>(itemOrList);
            editor.Count.ShouldEqual(objects.Count);
            editor.IsEmpty.ShouldEqual(count == 0);
            for (var i = 0; i < editor.Count; i++)
                objects[i].ShouldEqual(editor[i]);

            editor.GetRawValueInternal().ShouldEqual(itemOrList.GetRawValue());

            var editorItemOrList = editor.ToItemOrList();
            editorItemOrList.Item.ShouldEqual(itemOrList.Item);
            editorItemOrList.List.ShouldEqual(itemOrList.List);

            editorItemOrList = editor.ToItemOrList();
            editorItemOrList.Item.ShouldEqual(itemOrList.Item);
            editorItemOrList.List.ShouldEqual(itemOrList.List);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(10)]
        public void AddRangeClearShouldAddItemOrListClear(int count)
        {
            var objects = new List<object>();
            for (var i = 0; i < count; i++)
                objects.Add(new object());
            ItemOrIEnumerable<object> itemOrList = objects;

            var editor = new ItemOrListEditor<object>(null, null, false);
            editor.AddRange(itemOrList);

            var editorItemOrList = editor.ToItemOrList();
            editorItemOrList.Item.ShouldEqual(itemOrList.Item);
            editorItemOrList.List.ShouldEqual(itemOrList.List);

            editor.AddRange(itemOrList);
            objects.AddRange(objects);

            itemOrList = objects;
            editorItemOrList = editor.ToItemOrList();
            editorItemOrList.Item.ShouldEqual(itemOrList.Item);
            editorItemOrList.List.ShouldEqual(itemOrList.List);

            editor.Clear();
            editor.IsEmpty.ShouldBeTrue();
            editor.Count.ShouldEqual(0);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(10)]
        public void AddRemoveShouldBeCorrect(int count)
        {
            ItemOrListEditor<object> editor = default;
            var objects = new List<object>();
            for (var i = 0; i < count; i++)
            {
                var o = new object();
                objects.Add(o);
                editor.Add(o);
            }

            ItemOrIReadOnlyList<object> itemOrList = objects;
            var editorItemOrList = editor.ToItemOrList();
            editorItemOrList.Item.ShouldEqual(itemOrList.Item);
            editorItemOrList.List.ShouldEqual(itemOrList.List);

            var array = objects.ToArray();
            for (var i = 0; i < count; i++)
            {
                objects.Remove(array[i]);
                editor.Remove(array[i]);

                itemOrList = objects;
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
            var editor = new ItemOrListEditor<object>(null, null, false);
            var objects = new List<object>();
            for (var i = 0; i < count; i++)
            {
                var o = new object();
                objects.Add(o);
                editor.Add(o);
            }

            ItemOrIReadOnlyList<object> itemOrList = objects;
            var editorItemOrList = editor.ToItemOrList();
            editorItemOrList.Item.ShouldEqual(itemOrList.Item);
            editorItemOrList.List.ShouldEqual(itemOrList.List);

            for (var i = 0; i < count; i++)
                objects.IndexOf(objects[i]).ShouldEqual(i);

            for (var i = 0; i < count; i++)
            {
                objects.RemoveAt(0);
                editor.RemoveAt(0);

                itemOrList = objects;
                editorItemOrList = editor.ToItemOrList();
                editorItemOrList.Item.ShouldEqual(itemOrList.Item);
                editorItemOrList.List.ShouldEqual(itemOrList.List);
            }
        }

        [Fact]
        public void IndexShouldThrowOutOfRange() => Assert.Throws<ArgumentOutOfRangeException>(() => new ItemOrListEditor<object>()[0]);

        [Fact]
        public void IsNullOrEmptyShouldBeTrueDefault()
        {
            ItemOrListEditor<object> editor = default;
            editor.IsEmpty.ShouldBeTrue();
        }
    }
}