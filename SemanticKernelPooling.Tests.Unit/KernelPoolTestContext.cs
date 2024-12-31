using Microsoft.Extensions.DependencyInjection;
using System;

namespace SemanticKernelPooling.Tests.Unit;

public class KernelPoolTestContext: IDisposable
{
    public IServiceProvider ServiceProvider { get; }
    private readonly ServiceProvider _disposableServiceProvider;

    public void Dispose()
    {
        _disposableServiceProvider?.Dispose();
    }
}
