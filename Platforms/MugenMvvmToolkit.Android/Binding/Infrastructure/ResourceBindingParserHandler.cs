#region Copyright

// ****************************************************************************
// <copyright file="ResourceBindingParserHandler.cs">
// Copyright (c) 2012-2017 Vyacheslav Volkov
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
using System.Collections.Generic;
using Android.Content;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.Binding.DataConstants;
using MugenMvvmToolkit.Binding.Interfaces.Parse;
using MugenMvvmToolkit.Binding.Interfaces.Parse.Nodes;
using MugenMvvmToolkit.Binding.Parse.Nodes;
using MugenMvvmToolkit.Interfaces.Models;

namespace MugenMvvmToolkit.Android.Binding.Infrastructure
{
    public sealed class ResourceBindingParserHandler : IBindingParserHandler, IExpressionVisitor
    {
        #region Nested types

        public sealed class ResourceDescriptor
        {
            #region Fields

            public readonly Type ExtType;
            public readonly Func<Context, string, object> GetResource;
            public readonly string MethodName;

            #endregion

            #region Constructors

            public ResourceDescriptor(Type extType, string methodName, Func<Context, string, object> getResource)
            {
                Should.NotBeNull(extType, nameof(extType));
                Should.NotBeNull(methodName, nameof(methodName));
                ExtType = extType;
                MethodName = methodName;
                GetResource = getResource;
            }

            #endregion
        }

        #endregion

        #region Fields

        private readonly Dictionary<string, ResourceDescriptor> _resourceMemberToDescriptor;
        private IDataContext _lastContext;

        #endregion

        #region Constructors

        public ResourceBindingParserHandler(Dictionary<string, ResourceDescriptor> resourceMemberToDescriptor)
        {
            Should.NotBeNull(resourceMemberToDescriptor, nameof(resourceMemberToDescriptor));
            _resourceMemberToDescriptor = resourceMemberToDescriptor;
        }

        #endregion

        #region Properties

        public bool IsPostOrder => false;

        #endregion

        #region Implementation of interfaces

        public void Handle(ref string bindingExpression, IDataContext context)
        {
        }

        public void HandleTargetPath(ref string targetPath, IDataContext context)
        {
        }

        public Action<IDataContext> Handle(ref IExpressionNode expression, bool isPrimaryExpression, IDataContext context)
        {
            if (expression != null)
            {
                _lastContext = context;
                expression = expression.Accept(this);
                _lastContext = null;
            }
            return null;
        }

        public IExpressionNode Visit(IExpressionNode node)
        {
            var member = node as IMemberExpressionNode;
            if (member?.Target == null)
                return node;
            var target = member.Target as IMemberExpressionNode;
            var resourceType = target?.Target as ResourceExpressionNode;
            ResourceDescriptor resourceDescriptor;
            if (resourceType == null || !_resourceMemberToDescriptor.TryGetValue(target.Member, out resourceDescriptor))
                return node;

            if (resourceType.Dynamic || resourceDescriptor.GetResource == null)
            {
                return new MethodCallExpressionNode(new ConstantExpressionNode(resourceDescriptor.ExtType, typeof(Type)), resourceDescriptor.MethodName, new IExpressionNode[]
                {
                    new ConstantExpressionNode(member.Member, typeof(string)),
                    new MemberExpressionNode(ResourceExpressionNode.DynamicInstance, BindingServiceProvider.ResourceResolver.SelfResourceName)
                }, null);
            }

            var ctx = BindingResourceExtensions.GetContext(_lastContext?.GetData(BindingBuilderConstants.Target));
            return new ConstantExpressionNode(resourceDescriptor.GetResource(ctx, member.Member));
        }

        #endregion
    }
}