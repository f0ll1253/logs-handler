using Core.View.Models.Abstractions;
using Splat;

namespace Core.View.Models.Realizations;

public abstract class BaseRoot : IRoot
{
    public Stack<IView> Views { get; private set; } = new();

    public async Task Start(CancellationToken? token = null)
    {
        while (true)
        {
            if (token is not null && token.Value.IsCancellationRequested) break;
            if (!Views.TryPeek(out var view)) break;
            
            System.Console.Clear();
            System.Console.WriteLine("""
                                        .____    ________    ____________  ___
                                     /\ |    |   \_____  \  /  _____/\   \/  / /\
                                     \/ |    |    /   |   \/   \  ___ \     /  \/
                                     /\ |    |___/    |    \    \_\  \/     \  /\
                                     \/ |_______ \_______  /\______  /___/\  \ \/
                                                \/       \/        \/      \_/
                                                
                                            https://github.com/f0ll1253

                                     """);
                
            await view.Build().WaitAsync(App.Token);
            await Task.Delay(500, App.Token);
        }
    }
    
    public Task Redirect<T>(T view) where T : IView
    {
        System.Console.Clear();
        
        view.Initialize();
        
        Views.Push(view);

        return Task.CompletedTask;
    }

    public Task Redirect<T>() where T : IView => Redirect(Locator.Current.GetService<T>()!);
}