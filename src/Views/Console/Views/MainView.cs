using Console.Models;
using Console.Models.Abstractions;

namespace Console.Views;

public class MainView : ArgsView
{
    public MainView(IRoot root) : base(root)
    {
    }
    
    private Task Parse_Wallets()
    {
        return Task.CompletedTask;
    }
}