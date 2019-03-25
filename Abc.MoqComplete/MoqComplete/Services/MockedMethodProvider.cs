﻿using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Resolve;
using JetBrains.ReSharper.Psi.Tree;

namespace MoqComplete.Services
{
    [SolutionComponent]
    public class MockedMethodProvider : IMockedMethodProvider
    {
        private readonly IMoqMethodIdentifier _methodIdentifier;

        public MockedMethodProvider(IMoqMethodIdentifier methodIdentifier)
        {
            _methodIdentifier = methodIdentifier;
        }

        public IMethod GetMockedMethodFromSetupMethod(IInvocationExpression invocationExpression)
        {
            var mockMethodInvocationExpression = GetMockedMethodInvocation(invocationExpression);

            if (mockMethodInvocationExpression?.Reference == null)
                return null;

            var targetMethodResolveResult = mockMethodInvocationExpression.Reference.Resolve();
            if (targetMethodResolveResult.ResolveErrorType == ResolveErrorType.OK)
                return targetMethodResolveResult.DeclaredElement as IMethod;

            return null;
        }

        public TreeNodeCollection<ICSharpArgument>? GetMockedMethodParametersFromSetupMethod(IInvocationExpression invocationExpression)
        {
            var mockMethodInvocationExpression = GetMockedMethodInvocation(invocationExpression);
            return mockMethodInvocationExpression?.ArgumentList?.Arguments;
        }

        private IInvocationExpression GetMockedMethodInvocation(IInvocationExpression invocationExpression)
        {
            if (invocationExpression == null || !_methodIdentifier.IsMoqSetupMethod(invocationExpression))
                return null;

            var methodArguments = invocationExpression.ArgumentList.Arguments;
            if (methodArguments.Count != 1)
                return null;

            var lambdaExpression = methodArguments[0].Value as ILambdaExpression;
            if (lambdaExpression == null)
                return null;

            return lambdaExpression.BodyExpression as IInvocationExpression;
        }
    }
}