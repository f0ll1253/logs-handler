using Autofac;
using Core.Models.Factories;
using Newtonsoft.Json;
using Splat;
using Splat.Autofac;

namespace logs_handler.Services;

public static class Bootstrapper
{
    private static readonly Config _config = JsonConvert.DeserializeObject<Config>(File.ReadAllText("config.json"))!;
    
    public static void Initialize()
    {
        var container = new ContainerBuilder()
            .RegisterServices();

        var resolver = container.UseAutofacDependencyResolver();
        container.RegisterInstance(resolver);
        resolver.InitializeSplat();
        
        resolver.SetLifetimeScope(container.Build());
    }

    private static ContainerBuilder RegisterServices(this ContainerBuilder builder)
    {
        //builder.RegisterInstance(new MongoClient(_config.ConnectionStrings["Mongo"]));
        builder.RegisterType<RedlineFactory>();
        
        return builder;
    }
}