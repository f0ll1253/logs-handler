﻿using Autofac;
using Core.View.Models.Abstractions;
using Microsoft.Extensions.Configuration;
using Splat;
using Splat.Autofac;

namespace Core.View;

public static class App
{
    private static readonly CancellationTokenSource _source = new();
    
    public static Task Initialize(Action<ContainerBuilder>? other = null)
    {
        var builder = new ContainerBuilder()
            .RegisterConfiguration();
        
        other?.Invoke(builder);

        var resolver = builder.UseAutofacDependencyResolver();
        builder.RegisterInstance(resolver);
        resolver.SetLifetimeScope(builder.Build());
        
        return Task.CompletedTask;
    }

    public static async Task Run(IView start)
    {
        var root = Locator.Current.GetService<IRoot>()!;
        await root.Redirect(start);
        await root.Start(Token);
    }

    public static CancellationToken Token => _source.Token;
    
    private static ContainerBuilder RegisterConfiguration(this ContainerBuilder builder)
    {
        var configBuilder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json");

        builder.RegisterInstance((IConfiguration) configBuilder.Build());

        return builder;
    }
}