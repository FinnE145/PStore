using Windows.Win32.Foundation;

using static Vanara.PInvoke.Kernel32;

using Windows.Win32;
using Windows.Win32.Storage.CloudFilters;
using System.Text;
using System.Runtime.InteropServices;

using PStore;


namespace PStore.Provider;

public class Placeholders {
    private static void CreateFakePlaceholders(string destPath, string sourceSubDirStr, string relativeName, BY_HANDLE_FILE_INFORMATION info) {
        // Create directory if it doesn't exist
        Directory.CreateDirectory(Path.Combine(destPath, sourceSubDirStr));
        // Create placeholder file with same attributes + 'recall on data access', 'offline', and 'do not index content'
        using FileStream destFile = new(Path.Combine(destPath, relativeName), FileMode.CreateNew);
        SetFileAttributes(destFile.Name, info.dwFileAttributes);
    }

    private static void CreatePlaceholders(string destDirPath, string sourceDirPath) {
        List<CF_PLACEHOLDER_CREATE_INFO> placeholders = [];
        foreach (string file in Directory.GetFiles(sourceDirPath, "*", SearchOption.AllDirectories)) {
            Console.WriteLine($"Creating placeholder for {file}");
            string relativeName = file[sourceDirPath.Length..];
            Console.WriteLine($"Relative name: {relativeName}");
            try {
                Console.WriteLine($"Creating placeholder for {relativeName}");
                // Read attributes from source file
                using FileStream sourceFile = File.OpenRead(file);
                GetFileInformationByHandle(sourceFile.SafeFileHandle, out BY_HANDLE_FILE_INFORMATION info);
                unsafe {
                    fixed (char* p = relativeName) {
                        placeholders.Add(new CF_PLACEHOLDER_CREATE_INFO {
                            RelativeFileName = new PCWSTR(p),
                            FsMetadata = new CF_FS_METADATA {
                                FileSize = info.nFileSizeLow,
                                BasicInfo = new Windows.Win32.Storage.FileSystem.FILE_BASIC_INFO {
                                    CreationTime = FileTimeToLong(info.ftCreationTime),
                                    LastAccessTime = FileTimeToLong(info.ftLastAccessTime),
                                    LastWriteTime = FileTimeToLong(info.ftLastWriteTime),
                                    ChangeTime = FileTimeToLong(info.ftLastWriteTime),
                                    FileAttributes = (uint) info.dwFileAttributes
                                }
                            }
                        });
                    }
                }
            } catch (Exception ex) {
                Console.WriteLine($"Failed to create placeholder for {relativeName} with {ex.GetType()}: {ex.Message}");
                continue;
            }
        }
        unsafe {
            Utilities.Win32FuncRes(
                PInvoke.CfCreatePlaceholders(
                    destDirPath,
                    placeholders.ToArray(),
                    CF_CREATE_FLAGS.CF_CREATE_FLAG_NONE,
                    (uint*) IntPtr.Zero
                ),
                $"Failed to create placeholders"
            );
        }
    }
    public static void Create(string sourcePath, string subDirPath, string destPath) {
        try {
            sourcePath = EnsureTrailingBackslash(sourcePath);
            subDirPath = EnsureTrailingBackslash(subDirPath);

            /*foreach (string file in Directory.GetFiles(sourcePath, "*", SearchOption.AllDirectories)) {
                Console.WriteLine($"Creating placeholder for {file}");
                string relativeName = file[sourcePath.Length..];
                Console.WriteLine($"Relative name: {relativeName}");
                try {
                    Console.WriteLine($"Creating placeholder for {relativeName}");

                    // Read attributes from source file
                    using FileStream sourceFile = File.OpenRead(file);
                    GetFileInformationByHandle(sourceFile.SafeFileHandle, out BY_HANDLE_FILE_INFORMATION info);
                    info.dwFileAttributes |= FileFlagsAndAttributes.FILE_ATTRIBUTE_RECALL_ON_DATA_ACCESS | FileFlagsAndAttributes.FILE_ATTRIBUTE_OFFLINE | FileFlagsAndAttributes.FILE_ATTRIBUTE_NOT_CONTENT_INDEXED;

                    CreateFakePlaceholders(destPath, sourceSubDirStr, relativeName, info);
                } catch (Exception ex) {
                    Console.WriteLine($"Failed to create placeholder for {relativeName} with {ex.GetType()}: {ex.Message}");
                    continue;
                }
            }*/

            CreatePlaceholders(Path.Combine(destPath, subDirPath), Path.Combine(sourcePath, subDirPath));
        } catch (Exception ex) {
            Console.WriteLine($"Failed to create placeholders with {ex.GetType()}: {ex.Message}");
        }
    }

    private static string EnsureTrailingBackslash(string path) {
        return !string.IsNullOrEmpty(path) && !path.EndsWith('\\') ? path + "\\" : path;
    }

    private static long FileTimeToLong(System.Runtime.InteropServices.ComTypes.FILETIME ft) {
        return ((long) ft.dwHighDateTime << 32) + ft.dwLowDateTime;
    }
}
