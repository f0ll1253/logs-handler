using Console.Models.Abstractions;
using Serilog;
using Splat;

namespace Console.Models.Views;

public abstract class BaseRoot : IRoot
{
    private Stack<IView> _views = new();

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
                await view.Build().WaitAsync(App.Token);
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
            
            await Task.Delay(500, App.Token);
        }
    }

    public void Pop()
    {
        if (!_views.TryPop(out var last)) return;
        
        last.Deactivate();
        last.Dispose();
        
        if (_views.TryPeek(out var view)) view.Activate();
    }
    
    public void PushRedirect<T>(T view) where T : IView
    {
        System.Console.Clear();

        if (_views.TryPeek(out var last)) last.Deactivate();
        
        view.Initialize();
        view.Activate();
        
        _views.Push(view);
    }

    public void PushRedirect<T>() where T : IView => PushRedirect(Locator.Current.GetService<T>()!);
}