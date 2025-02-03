using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PStore.Provider;

namespace PStore;
internal class Install {
    public static void Main() {
        Console.WriteLine("Registering PStore...");
        ProviderRegistrar provider = new();
        provider.RegisterApp();
        Console.WriteLine("PStore registered!");

        Console.WriteLine("Would you like to unregister the app? (yes/[no]/all)");
        string? response = Console.ReadLine();
        if (response?.ToLower() is "yes" or "y") {
            provider.UnregisterApp();
            Console.WriteLine("PStore unregistered!");
        } else if (response?.ToLower() is "all" or "a") {
            ProviderRegistrar.UnregisterAll();
            Console.WriteLine("All PStore apps unregistered!");
        }
    }
}
