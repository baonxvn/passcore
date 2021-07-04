using System;

namespace DependencyInjectionConsoleApp
{
    public class DefaultOperation : ITransientOperation, IScopedOperation, ISingletonOperation
    {
        public string OperationId { get; } = new Guid().ToString()[^4..];
        public string FuncScopedOperation()
        {
            throw new NotImplementedException();
        }
    }
}