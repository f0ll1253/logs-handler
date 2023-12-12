using Autofac;
using Core.Models.Configs;
using Microsoft.Extensions.Configuration;

namespace Core.Models.Extensions;

public static class ContainerBuilderExtensions
{
    public static ContainerBuilder RegisterConfiguration(this ContainerBuilder builder)
    {
        var configBuilder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json");

        builder.RegisterInstance<IConfiguration>(configBuilder.Build());
        builder.Register(x => x.Resolve<IConfiguration>().GetRequiredSection("ParsingConfig").Get<ParsingConfig>()!)
            .SingleInstance()
            .AsSelf();
        builder.Register(x => x.Resolve<IConfiguration>().GetRequiredSection("Mega").Get<MegaConfig>()!)
            .SingleInstance()
            .AsSelf();

        return builder;
    }
}