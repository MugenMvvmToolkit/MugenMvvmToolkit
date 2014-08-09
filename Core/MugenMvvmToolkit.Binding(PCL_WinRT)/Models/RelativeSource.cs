#region Copyright
// ****************************************************************************
// <copyright file="RelativeSource.cs">
// Copyright © Vyacheslav Volkov 2012-2014
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
using System;
using JetBrains.Annotations;
using MugenMvvmToolkit.Binding.Behaviors;
using MugenMvvmToolkit.Binding.Core;
using MugenMvvmToolkit.Binding.DataConstants;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Binding.Interfaces.Parse.Nodes;
using MugenMvvmToolkit.Binding.Interfaces.Sources;
using MugenMvvmToolkit.Binding.Parse.Nodes;
using MugenMvvmToolkit.Binding.Sources;

namespace MugenMvvmToolkit.Binding.Models
{
    public sealed class RelativeSource : BindingBehaviorBase
    {
        #region Nested types

        private sealed class ParentSourceValue : IEventListener
        {
            #region Fields

            private object _value;
            private readonly IRelativeSourceExpressionNode _node;
            private readonly WeakReference _targetReference;
            private readonly IDisposable _subscriber;
            private bool _hasParent;
            private readonly bool _isElementSource;

            #endregion

            #region Constructors

            /// <summary>
            ///     Initializes a new instance of the <see cref="ParentSourceValue" /> class.
            /// </summary>
            public ParentSourceValue(object target, IRelativeSourceExpressionNode node)
            {
                _node = node;
                _isElementSource = _node.Type == RelativeSourceExpressionNode.ElementSourceType;
                _targetReference = MvvmExtensions.GetWeakReference(target);
                _subscriber = BindingProvider.Instance.ObserverProvider.TryObserveParent(target, this);
                Handle(null, null);
            }

            #endregion

            #region Properties

            public object Value
            {
                get
                {
                    if (_value != null)
                        return _value;
                    if (_isElementSource)
                        throw BindingExceptionManager.ElementSourceNotFound(_targetReference.Target, _node.ElementName);
                    throw BindingExceptionManager.RelativeSourceNotFound(_targetReference.Target, _node.Type,
                        _node.Level);
                }
                set
                {
                    if (Equals(value, _value)) return;
                    _value = value;
                    OnValueChanged();
                }
            }

            #endregion

            #region Events

            public event EventHandler ValueChanged;

            #endregion

            #region Methods

            private void OnValueChanged()
            {
                var handler = ValueChanged;
                if (handler != null) handler(this, EventArgs.Empty);
            }

            #endregion

            #region Implementation of IEventListener

            /// <summary>
            ///     Handles the message.
            /// </summary>
            /// <param name="sender">The object that raised the event.</param>
            /// <param name="message">Information about event.</param>
            public void Handle(object sender, object message)
            {
                var target = _targetReference.Target;
                if (target == null)
                {
                    Value = null;
                    if (_subscriber != null)
                        _subscriber.Dispose();
                    return;
                }
                _hasParent = BindingProvider.Instance.VisualTreeManager.FindParent(target) != null;
                var value = _isElementSource
                    ? BindingProvider.Instance.VisualTreeManager.FindByName(target, _node.ElementName)
                    : BindingProvider.Instance.VisualTreeManager.FindRelativeSource(target, _node.Type, _node.Level);
                if (value != null || _hasParent)
                    Value = value;
                else
                    Value = BindingConstants.UnsetValue;
            }

            #endregion
        }

        #endregion

        #region Fields

        private readonly Guid _id;
        private readonly IRelativeSourceExpressionNode _relativeSourceNode;
        private IBindingSource _bindingSource;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="RelativeSource" /> class.
        /// </summary>
        public RelativeSource(IRelativeSourceExpressionNode relativeSource)
        {
            _id = Guid.NewGuid();
            _relativeSourceNode = relativeSource;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the additional path.
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Gets the relative source node.
        /// </summary>
        public IRelativeSourceExpressionNode RelativeSourceNode
        {
            get { return _relativeSourceNode; }
        }

        /// <summary>
        ///     Gets the current <see cref="IBindingSource" />.
        /// </summary>
        public IBindingSource BindingSource
        {
            get { return _bindingSource; }
        }

        /// <summary>
        ///     Gets the current value.
        /// </summary>
        [CanBeNull]
        public object Value
        {
            get
            {
                if (BindingSource == null)
                    return null;
                IBindingPathMembers pathMembers = BindingSource.GetPathMembers(true);
                object value = pathMembers.LastMember.GetValue(pathMembers.PenultimateValue, null);
                if (value.IsUnsetValue())
                    return null;
                var memberValue = value as BindingMemberValue;
                if (memberValue == null)
                    return value;
                return memberValue.GetValue(null);
            }
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Updates the relative source value.
        /// </summary>
        public void UpdateSource([NotNull]object target)
        {
            if (target == null)
                throw BindingExceptionManager.InvalidBindingTarget(RelativeSourceNode.Path);
            var path = RelativeSourceNode.Path ?? String.Empty;
            if (!String.IsNullOrEmpty(Path))
                path = BindingExtensions.MergePath(path, Path);


            if (RelativeSourceNode.Type != RelativeSourceExpressionNode.SelfType)
            {
                if (RelativeSourceNode.Type == RelativeSourceExpressionNode.MemberSourceType)
                    target = BindingProvider.Instance.ContextManager.GetBindingContext(target);
                else
                {
                    var source = target;
                    target = new ParentSourceValue(target, RelativeSourceNode);
                    ServiceProvider.AttachedValueProvider.SetValue(source, "@$@$parentvalue" + target.GetHashCode(),
                        target);
                    path = BindingExtensions.MergePath("Value", path);
                }
            }
            IObserver observer = BindingProvider.Instance.ObserverProvider.Observe(target, BindingPath.Create(path), false);
            _bindingSource = new BindingSource(observer);
        }

        #endregion

        #region Overrides of BindingBehaviorBase

        /// <summary>
        ///     Gets the id of behavior. Each <see cref="IDataBinding" /> can have only one instance with the same id.
        /// </summary>
        public override Guid Id
        {
            get { return _id; }
        }

        /// <summary>
        ///     Gets the behavior priority.
        /// </summary>
        public override int Priority
        {
            get { return int.MaxValue; }
        }

        /// <summary>
        ///     Attaches to the specified binding.
        /// </summary>
        protected override bool OnAttached()
        {
            OnDetached();
            UpdateSource(Binding.TargetAccessor.Source.GetSource(true));
            return true;
        }

        /// <summary>
        ///     Detaches this instance from its associated binding.
        /// </summary>
        protected override void OnDetached()
        {
            if (BindingSource != null)
                BindingSource.Dispose();
        }

        /// <summary>
        ///     Creates a new binding behavior that is a copy of the current instance.
        /// </summary>
        protected override IBindingBehavior CloneInternal()
        {
            return new RelativeSource(RelativeSourceNode);
        }

        #endregion
    }
}