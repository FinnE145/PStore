using Vanara.PInvoke;
using static Vanara.PInvoke.Kernel32;

namespace PStore.Provider;

public class Placeholders {
    public static void Create(string sourcePathStr, string sourceSubDirStr, string destPath) {
        try {
            sourcePathStr = EnsureTrailingBackslash(sourcePathStr);
            sourceSubDirStr = EnsureTrailingBackslash(sourceSubDirStr);

            string filePattern = Path.Combine(sourcePathStr, sourceSubDirStr, "*");
            string fullDestPath = Path.Combine(destPath, sourceSubDirStr);

            foreach (string file in Directory.GetFiles(sourcePathStr, "*", SearchOption.AllDirectories)) {
                string relativeName = file[sourcePathStr.Length..];
                try {
                    Console.WriteLine($"Creating placeholder for {relativeName}");
                    // Read attributes from source file
                    using FileStream sourceFile = File.OpenRead(file);
                    GetFileInformationByHandle(sourceFile.SafeFileHandle, out BY_HANDLE_FILE_INFORMATION info);
                    info.dwFileAttributes |= FileFlagsAndAttributes.FILE_ATTRIBUTE_RECALL_ON_DATA_ACCESS | FileFlagsAndAttributes.FILE_ATTRIBUTE_OFFLINE | FileFlagsAndAttributes.FILE_ATTRIBUTE_NOT_CONTENT_INDEXED;
                    using FileStream destFile = new(relativeName, FileMode.CreateNew);
                    // set attributes of destination file
                    SetFileAttributes(destFile.Name, info.dwFileAttributes);
                } catch (Exception ex) {
                    Console.WriteLine($"Failed to create placeholder for {relativeName} with {ex.GetType()}: {ex.Message}");
                    continue;
                }
            }
        } catch (Exception ex) {
            Console.WriteLine($"Failed to create placeholders with {ex.GetType()}: {ex.Message}");
        }
    }

    private static string EnsureTrailingBackslash(string path) {
        return !string.IsNullOrEmpty(path) && !path.EndsWith('\\') ? path + "\\" : path;
    }
}
