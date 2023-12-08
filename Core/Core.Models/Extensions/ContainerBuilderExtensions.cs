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

        var build = configBuilder.Build();
        
        builder.RegisterInstance<IConfiguration>(build);
        builder.RegisterInstance(build.GetRequiredSection("Web3").Get<Web3Config>()!);
        builder.RegisterInstance(build.GetRequiredSection("ParsingConfig").Get<ParsingConfig>()!);

        return builder;
    }
}