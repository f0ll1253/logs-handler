using Autofac;
using Microsoft.EntityFrameworkCore;

namespace TelegramBot.Extensions;

internal static class AutofacExtensions
{
    public static ContainerBuilder RegisterContext<TContext>(this ContainerBuilder builder,
        Action<IComponentContext,
            DbContextOptionsBuilder<TContext>> configurator)
        where TContext : DbContext
    {
        builder.Register<TContext>(x =>
               {
                   var optionsBuilder = new DbContextOptionsBuilder<TContext>();

                   configurator.Invoke(x, optionsBuilder);

                   return (TContext)Activator.CreateInstance(typeof(TContext), optionsBuilder.Options)!;
               })
               .SingleInstance()
               .AsSelf();

        return builder;
    }
}