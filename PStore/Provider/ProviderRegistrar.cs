using Microsoft.Win32;
using Windows.Win32;
using Windows.Win32.Storage.CloudFilters;
using Windows.Win32.Foundation;

namespace PStore.Provider;

class ProviderRegistrar {
    /* Registry commands to run (from https://learn.microsoft.com/en-us/windows/win32/shell/integrate-cloud-storage):
        reg add HKCU\Software\Classes\CLSID\{0672A6D1-A6E0-40FE-AB16-F25BADC6D9E3} /ve /t REG_SZ /d "MyCloudStorageApp" /f
        reg add HKCU\Software\Classes\CLSID\{0672A6D1-A6E0-40FE-AB16-F25BADC6D9E3}\DefaultIcon /ve /t REG_EXPAND_SZ /d %%SystemRoot%%\system32\imageres.dll,-1043 /f
        reg add HKCU\Software\Classes\CLSID\{0672A6D1-A6E0-40FE-AB16-F25BADC6D9E3} /v System.IsPinnedToNameSpaceTree /t REG_DWORD /d 0x1 /f
        reg add HKCU\Software\Classes\CLSID\{0672A6D1-A6E0-40FE-AB16-F25BADC6D9E3} /v SortOrderIndex /t REG_DWORD /d 0x42 /f
        reg add HKCU\Software\Classes\CLSID\{0672A6D1-A6E0-40FE-AB16-F25BADC6D9E3}\InProcServer32 /ve /t REG_EXPAND_SZ /d %%systemroot%%\system32\shell32.dll /f
        reg add HKCU\Software\Classes\CLSID\{0672A6D1-A6E0-40FE-AB16-F25BADC6D9E3}\Instance /v CLSID /t REG_SZ /d {0E5AAE11-A475-4c5b-AB00-C66DE400274E} /f
        reg add HKCU\Software\Classes\CLSID\{0672A6D1-A6E0-40FE-AB16-F25BADC6D9E3}\Instance\InitPropertyBag /v Attributes /t REG_DWORD /d 0x11 /f
        reg add HKCU\Software\Classes\CLSID\{0672A6D1-A6E0-40FE-AB16-F25BADC6D9E3}\Instance\InitPropertyBag /v TargetFolderPath /t REG_EXPAND_SZ /d %%USERPROFILE%%\MyCloudStorageApp /f
        reg add HKCU\Software\Classes\CLSID\{0672A6D1-A6E0-40FE-AB16-F25BADC6D9E3}\ShellFolder /v FolderValueFlags /t REG_DWORD /d 0x28 /f
        reg add HKCU\Software\Classes\CLSID\{0672A6D1-A6E0-40FE-AB16-F25BADC6D9E3}\ShellFolder /v Attributes /t REG_DWORD /d 0xF080004D /f
        reg add HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\Desktop\NameSpace\{0672A6D1-A6E0-40FE-AB16-F25BADC6D9E3} /ve /t REG_SZ /d MyCloudStorageApp /f
        reg add HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\HideDesktopIcons\NewStartPanel /v {0672A6D1-A6E0-40FE-AB16-F25BADC6D9E3} /t REG_DWORD /d 0x1 /f
    */

    public string? CLSID { get; private set; }

    public void RegRegister() {
        string UserPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        this.CLSID = $"{{{Guid.NewGuid()}}}";                                  // Generated GUID
        string AppName = "PStore";                                             // Name of the app
        string IconPath = "C:\\Program Files\\PStore\\FEAssets\\PStore.ico";   // Location in app install folder
        string HostDll = "C:\\Windows\\System32\\shell32.dll";                 // Generic shell prodiver
        string InstanceCLSID = "{0E5AAE11-A475-4c5b-AB00-C66DE400274E}";       // CLSID for generic folder? maybe? - same one that OneDrive uses, not quite sure what it is exactly
        string TargetFolderPath = $@"{UserPath}\PStore";

        using (RegistryKey key = Registry.CurrentUser.CreateSubKey($@"Software\Classes\CLSID\{this.CLSID}")) {
            key.SetValue("", AppName, RegistryValueKind.String);
            key.CreateSubKey("DefaultIcon").SetValue("", IconPath, RegistryValueKind.ExpandString);
            key.SetValue("System.IsPinnedToNameSpaceTree", 1, RegistryValueKind.DWord);
            key.SetValue("SortOrderIndex", 0x42, RegistryValueKind.DWord);
            key.CreateSubKey("InProcServer32").SetValue("", HostDll, RegistryValueKind.ExpandString);
            key.CreateSubKey("Instance").SetValue("CLSID", InstanceCLSID, RegistryValueKind.String);
            key.CreateSubKey(@"Instance\InitPropertyBag").SetValue("Attributes", 0x10, RegistryValueKind.DWord);
            key.CreateSubKey(@"Instance\InitPropertyBag").SetValue("TargetFolderPath", TargetFolderPath, RegistryValueKind.ExpandString);
            key.CreateSubKey("ShellFolder").SetValue("FolderValueFlags", 0x28, RegistryValueKind.DWord);
            key.CreateSubKey("ShellFolder").SetValue("Attributes", unchecked((int) 0xf080004d), RegistryValueKind.DWord);
        }

        using (RegistryKey key = Registry.CurrentUser.CreateSubKey($@"Software\Microsoft\Windows\CurrentVersion\Explorer\Desktop\NameSpace\{this.CLSID}")) {
            key.SetValue("", AppName, RegistryValueKind.String);
        }

        using (RegistryKey key = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\HideDesktopIcons\NewStartPanel")) {
            key.SetValue(this.CLSID, 1, RegistryValueKind.DWord);
        }
    }

    public void RegUnregister() {
        if (this.CLSID == null) {
            return;
        }

        try {
            Registry.CurrentUser.DeleteSubKeyTree($@"Software\Classes\CLSID\{this.CLSID}");
            Registry.CurrentUser.DeleteSubKey($@"Software\Microsoft\Windows\CurrentVersion\Explorer\Desktop\NameSpace\{this.CLSID}");
            using RegistryKey? key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\HideDesktopIcons\NewStartPanel", true);
            key?.DeleteValue(this.CLSID, false);
        } catch {
            Console.WriteLine($"Failed to unregister the PStore instance.");
        }
    }

    public static void RegUnregisterAll() {
        int c = 0;
        using (RegistryKey? CLSIDKey = Registry.CurrentUser.OpenSubKey("Software\\Classes\\CLSID", true)) {
            foreach (string key in CLSIDKey?.GetSubKeyNames() ?? []) {
                if (key.StartsWith('{')) {
                    try {
                        using RegistryKey? subkey = CLSIDKey?.OpenSubKey(key);
                        if (subkey?.GetValue("")?.ToString() == "PStore") {
                            CLSIDKey?.DeleteSubKeyTree(key);
                            Registry.CurrentUser.DeleteSubKey($@"Software\Microsoft\Windows\CurrentVersion\Explorer\Desktop\NameSpace\{key}");
                            using RegistryKey? nkey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\HideDesktopIcons\NewStartPanel", true);
                            nkey?.DeleteValue(key, false);
                            c++;
                        }
                    } catch (Exception ex) {
                        Console.WriteLine($"Failed to check/unregister {key}: {ex.Message}");
                    }
                }
            }
        }
        Console.WriteLine(value: $"Unregistered {c} PStore instance(s) from this user.");
    }

    public static void Register(string syncRootPath, string providerName, string providerVersion) {
        unsafe {
            fixed (char* syncRootPathP = syncRootPath, providerNameP = providerName, providerVersionP = providerVersion) {
                CF_SYNC_REGISTRATION SyncReg = new() {
                    ProviderName = new PCWSTR(providerNameP),
                    ProviderVersion = new PCWSTR(providerVersionP),
                    SyncRootIdentity = (void*) IntPtr.Zero,
                    SyncRootIdentityLength = (uint) 0,
                    FileIdentity = (void*) IntPtr.Zero,
                    FileIdentityLength = (uint) 0,
                    ProviderId = Guid.NewGuid()
                };

                CF_SYNC_POLICIES SyncPol = new() {
                    Hydration = new CF_HYDRATION_POLICY { Primary = CF_HYDRATION_POLICY_PRIMARY.CF_HYDRATION_POLICY_FULL, Modifier = CF_HYDRATION_POLICY_MODIFIER.CF_HYDRATION_POLICY_MODIFIER_AUTO_DEHYDRATION_ALLOWED },
                    Population = new CF_POPULATION_POLICY { Primary = CF_POPULATION_POLICY_PRIMARY.CF_POPULATION_POLICY_FULL, Modifier = CF_POPULATION_POLICY_MODIFIER.CF_POPULATION_POLICY_MODIFIER_NONE },
                    InSync = CF_INSYNC_POLICY.CF_INSYNC_POLICY_NONE,
                    HardLink = CF_HARDLINK_POLICY.CF_HARDLINK_POLICY_NONE,
                    PlaceholderManagement = CF_PLACEHOLDER_MANAGEMENT_POLICY.CF_PLACEHOLDER_MANAGEMENT_POLICY_DEFAULT,
                };

                Utilities.Win32FuncRes(
                    PInvoke.CfRegisterSyncRoot(
                        syncRootPath,
                        SyncReg,
                        SyncPol,
                        CF_REGISTER_FLAGS.CF_REGISTER_FLAG_NONE
                    ),
                    "Failed to register PStore"
                );

                //Utilities.Win32Function((Func<string, CF_SYNC_REGISTRATION, CF_SYNC_POLICIES, CF_REGISTER_FLAGS, HRESULT>) PInvoke.CfRegisterSyncRoot, syncRootPath, SyncReg, SyncPol, CF_REGISTER_FLAGS.CF_REGISTER_FLAG_NONE);
            }
        }
    }


    public static void Unregister(string syncRootPath) {
        Utilities.Win32FuncRes(PInvoke.CfUnregisterSyncRoot(syncRootPath), "Failed to unregister PStore");
    }
}