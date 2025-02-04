using Windows.Win32;
using Windows.Win32.Storage.CloudFilters;
using Windows.Win32.Foundation;

using PStore.Provider;
using System.Runtime.InteropServices;

namespace PStore;
internal class Install {
    public static void Main() {
        Console.WriteLine("Unregistering previous PStore instances...");
        ProviderRegistrar.RegUnregisterAll();

        Console.WriteLine("Registering PStore...");
        ProviderRegistrar.Register(@"C:\Users\finne\PStore", "PStore2", "1.2.1");
        /*ProviderRegistrar provider = new();
        provider.RegRegister();
        Console.WriteLine("PStore registered!");*/

        /*unsafe {
            fixed (char* providerNameP = "PStore", providerVersionP = "1.1.1") {
                CF_SYNC_REGISTRATION SyncReg = new() {
                    ProviderName = new PCWSTR(providerNameP),
                    ProviderVersion = new PCWSTR(providerVersionP)
                };

                CF_SYNC_POLICIES SyncPol = new() {
                    Hydration = new CF_HYDRATION_POLICY { Primary = CF_HYDRATION_POLICY_PRIMARY.CF_HYDRATION_POLICY_FULL, Modifier = CF_HYDRATION_POLICY_MODIFIER.CF_HYDRATION_POLICY_MODIFIER_AUTO_DEHYDRATION_ALLOWED },
                    Population = new CF_POPULATION_POLICY { Primary = CF_POPULATION_POLICY_PRIMARY.CF_POPULATION_POLICY_FULL, Modifier = CF_POPULATION_POLICY_MODIFIER.CF_POPULATION_POLICY_MODIFIER_NONE },
                    InSync = CF_INSYNC_POLICY.CF_INSYNC_POLICY_NONE,
                    HardLink = CF_HARDLINK_POLICY.CF_HARDLINK_POLICY_NONE,
                    PlaceholderManagement = CF_PLACEHOLDER_MANAGEMENT_POLICY.CF_PLACEHOLDER_MANAGEMENT_POLICY_DEFAULT,
                };

                HRESULT res = PInvoke.CfRegisterSyncRoot(
                    @"C:\Users\finne\PStore",
                    SyncReg,
                    SyncPol,
                    CF_REGISTER_FLAGS.CF_REGISTER_FLAG_NONE
                );

                if (res != HRESULT.S_OK) {
                    Console.WriteLine($"Failed with {(uint) res:X8}");
                    throw Marshal.GetExceptionForHR((int) res) ?? new Exception($"Unidentified Error - {(uint) res:X8}");
                }

                //Utilities.Win32Function((Func<string, CF_SYNC_REGISTRATION, CF_SYNC_POLICIES, CF_REGISTER_FLAGS, HRESULT>) PInvoke.CfRegisterSyncRoot, syncRootPath, SyncReg, SyncPol, CF_REGISTER_FLAGS.CF_REGISTER_FLAG_NONE);
            }
        }*/

        Placeholders.Create(@"C:\TestPStore", "Documents", Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "PStore"));

        Console.WriteLine("Would you like to unregister the app? (yes/[no]/all)");
        string? response = Console.ReadLine();
        if (response?.ToLower() is "yes" or "y") {
            //provider.RegUnregister();
            ProviderRegistrar.Unregister(@"C:\Users\finne\PStore");
            Console.WriteLine("PStore unregistered!");
        } else if (response?.ToLower() is "all" or "a") {
            ProviderRegistrar.RegUnregisterAll();
        }
    }
}
