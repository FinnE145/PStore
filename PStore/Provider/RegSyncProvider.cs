using Microsoft.Win32;
using System;

namespace PStore.Provider;

class RegSyncProvider {
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

    private static string[] GetCLSIDs() {
        return System.IO.File.ReadAllLines("CLSIDs.txt");
    }

    private static void AddCLSID(string clsid) {
        string[] clsids = GetCLSIDs();
        Array.Resize(ref clsids, clsids.Length + 1);
        clsids[^1] = clsid;
        System.IO.File.WriteAllLines("CLSIDs.txt", clsids);
    }

    public void RegisterApp() {
        string UserPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        this.CLSID = $"{{{Guid.NewGuid()}}}";                                  // Generated GUID
        string AppName = "PStore";                                             // Name of the app
        string IconPath = "C:\\Program Files\\PStore\\FEAssets\\PStore.ico";   // Location in app install folder
        string HostDll = "C:\\Windows\\System32\\shell32.dll";                 // Generic shell prodiver
        string InstanceCLSID = "{0E5AAE11-A475-4c5b-AB00-C66DE400274E}";       // CLSID for generic folder? maybe? - same one that OneDrive uses, not quite sure what it is exactly
        string TargetFolderPath = $@"{UserPath}\PStore";

        AddCLSID(this.CLSID);

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

    public void UnregisterApp() {
        if (this.CLSID == null) {
            return;
        }

        Registry.CurrentUser.DeleteSubKeyTree($@"Software\Classes\CLSID\{this.CLSID}");
        Registry.CurrentUser.DeleteSubKey($@"Software\Microsoft\Windows\CurrentVersion\Explorer\Desktop\NameSpace\{this.CLSID}");
        using RegistryKey? key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\HideDesktopIcons\NewStartPanel", true);
        key?.DeleteValue(this.CLSID, false);
    }

    public static void UnregisterAll() {
        string[] clsids = GetCLSIDs();
        foreach (string clsid in clsids) {
            Registry.CurrentUser.DeleteSubKeyTree($@"Software\Classes\CLSID\{clsid}");
            Registry.CurrentUser.DeleteSubKey($@"Software\Microsoft\Windows\CurrentVersion\Explorer\Desktop\NameSpace\{clsid}");
            using RegistryKey? key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\HideDesktopIcons\NewStartPanel", true);
            key?.DeleteValue(clsid, false);
        }
    }
}