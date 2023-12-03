using Console.Models.Abstractions;
using Serilog;
using Splat;

namespace Console.Models.Views;

public abstract class BaseRoot : IRoot
{
    private Stack<IViewDefault> _views = new();

    public async Task Start(CancellationToken? token = null)
    {
        while (true)
        {
            if (token is not null && token.Value.IsCancellationRequested) break;
            if (!_views.TryPeek(out var view)) break;
            
            System.Console.Clear();
            System.Console.WriteLine("""
                                        .____    ________    ____________  ___
                                     /\ |    |   \_____  \  /  _____/\   \/  / /\
                                     \/ |    |    /   |   \/   \  ___ \     /  \/
                                     /\ |    |___/    |    \    \_\  \/     \  /\
                                     \/ |_______ \_______  /\______  /___/\  \ \/
                                                \/       \/        \/      \_/
                                    """ + $"\n{(Locator.Current.GetService<Settings>() is {Path.Length: >= 1} settings ? $"{settings.Path}\n" : "")}");

            try
            {
                await view.Build().WaitAsync(App.Source.Token);
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                System.Console.WriteLine("Error occurred while execution. Check log.txt for details.\nPress any key for continue");
                
                #if DEBUG
                throw;
                #endif
                
                System.Console.ReadKey(true);
            }
            
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Aggressive, true, true);
            
            await Task.Delay(500, App.Source.Token);
        }
    }

    public async Task<T> Show<T>(IViewResult<T> view)
    {
        view.Initialize();
        view.Activate();

        T? result;

        do
        {
            result = await view.Build();
        } while (result is null);
        
        view.Deactivate();
        view.Dispose();
        
        return result;
    }

    public void Pop()
    {
        if (!_views.TryPop(out var last)) return;
        
        last.Deactivate();
        last.Dispose();
        
        if (_views.TryPeek(out var view)) view.Activate();
    }
    
    public void PushRedirect<T>(T view) where T : IViewDefault
    {
        System.Console.Clear();

        if (_views.TryPeek(out var last)) last.Deactivate();
        
        view.Initialize();
        view.Activate();
        
        _views.Push(view);
    }

    public void PushRedirect<T>() where T : IViewDefault => PushRedirect(Locator.Current.GetService<T>()!);
}