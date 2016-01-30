#region Copyright

// ****************************************************************************
// <copyright file="PreferenceItemsSourceGenerator.cs">
// Copyright (c) 2012-2016 Vyacheslav Volkov
// </copyright>
// ****************************************************************************
// <author>Vyacheslav Volkov</author>
// <email>vvs0205@outlook.com</email>
// <project>MugenMvvmToolkit</project>
// <web>https://github.com/MugenMvvmToolkit/MugenMvvmToolkit</web>
// <license>
// See license.txt in this solution or http://opensource.org/licenses/MS-PL
// </license>
// ****************************************************************************

#endregion

using System.Collections;
using Android.Preferences;
using MugenMvvmToolkit.Android.Binding.Interfaces;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.Binding.Interfaces.Models;

namespace MugenMvvmToolkit.Android.Binding.Infrastructure
{
    internal class PreferenceItemsSourceGenerator : ItemsSourceGeneratorBase
    {
        #region Fields

        private readonly IBindingMemberInfo _collectionViewManagerMember;
        private readonly PreferenceGroup _preference;

        #endregion

        #region Constructors

        public PreferenceItemsSourceGenerator(PreferenceGroup preference)
        {
            Should.NotBeNull(preference, nameof(preference));
            _preference = preference;
            _collectionViewManagerMember = BindingServiceProvider
                .MemberProvider
                .GetBindingMember(preference.GetType(), AttachedMembers.PreferenceGroup.CollectionViewManager, false,
                    false);
            TryListenActivity(preference.Context);
        }

        #endregion

        #region Properties

        protected override IEnumerable ItemsSource { get; set; }

        protected override bool IsTargetDisposed => !_preference.IsAlive();

        #endregion

        #region Methods

        protected override void Add(int insertionIndex, int count)
        {
            var collectionViewManager = GetCollectionViewManager();
            for (int i = 0; i < count; i++)
            {
                int index = insertionIndex + i;
                if (collectionViewManager == null)
                    _preference.AddPreference((Preference)SelectTemplate(index));
                else
                    collectionViewManager.Insert(_preference, index, SelectTemplate(index));
            }
        }

        protected override void Remove(int removalIndex, int count)
        {
            var collectionViewManager = GetCollectionViewManager();
            for (int i = 0; i < count; i++)
            {
                if (collectionViewManager == null)
                {
                    var preference = _preference.GetPreference(removalIndex + i);
                    _preference.RemovePreference(preference);
                    preference.SetBindingMemberValue(AttachedMembers.Object.Parent, BindingExtensions.NullValue);
                }
                else
                {
                    collectionViewManager.RemoveAt(_preference, removalIndex + i);
                }
            }
        }

        protected override void Replace(int startIndex, int count)
        {
            for (int i = 0; i < count; i++)
            {
                int index = startIndex + i;
                Remove(index, 1);
                Add(index, 1);
            }
        }

        protected override void Refresh()
        {
            var collectionViewManager = GetCollectionViewManager();
            if (collectionViewManager == null)
            {
                var oldValues = new Preference[_preference.PreferenceCount];
                for (int i = 0; i < _preference.PreferenceCount; i++)
                    oldValues[i] = _preference.GetPreference(i);
                _preference.RemoveAll();
                for (int i = 0; i < oldValues.Length; i++)
                    oldValues[i].SetBindingMemberValue(AttachedMembers.Object.Parent, BindingExtensions.NullValue);
            }
            else
                collectionViewManager.Clear(_preference);
            Add(0, ItemsSource.Count());
        }

        private ICollectionViewManager GetCollectionViewManager()
        {
            return _collectionViewManagerMember == null
                ? null
                : _collectionViewManagerMember.GetValue(_preference, null) as ICollectionViewManager;
        }

        private object SelectTemplate(int index)
        {
            var item = GetItem(index);
            var selector = _preference.GetBindingMemberValue(AttachedMembers.PreferenceGroup.ItemTemplateSelector);
            if (selector != null)
            {
                var template = selector.SelectTemplate(item, _preference);
                if (template != null)
                    template.SetDataContext(item);
                item = template;
            }
            var preference = item as Preference;
            if (preference != null)
            {
                preference.SetBindingMemberValue(AttachedMembers.Object.Parent, _preference);
                preference.Order = index;
            }
            return item;
        }

        #endregion
    }
}
