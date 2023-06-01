using System;
using System.Diagnostics;
using System.IO;
using System.IO.Abstractions;

namespace JsonFlatFileDataStore
{
    internal static class FileAccess
    {
        internal static string ReadJsonFromFile(IFileSystem fs, string path, Func<string, string> encryptJson, Func<string, string> decryptJson)
        {
            Stopwatch sw = null;
            var json = "{}";

            while (true)
            {
                try
                {
                    json = fs.File.ReadAllText(path);
                    break;
                }
                catch (FileNotFoundException)
                {
                    json = encryptJson(json);
                    fs.File.WriteAllText(path, json);
                    break;
                }
                catch (IOException e) when (e.Message.Contains("because it is being used by another process"))
                {
                    // If some other process is using this file, retry operation unless elapsed times is greater than 10sec
                    sw ??= Stopwatch.StartNew();
                    if (sw.ElapsedMilliseconds > 10000)
                        throw;
                }
            }

            return decryptJson(json);
        }

        internal static bool WriteJsonToFile(IFileSystem fs, string path, Func<string, string> encryptJson, string content)
        {
            Stopwatch sw = null;

            while (true)
            {
                try
                {
                    fs.File.WriteAllText(path, encryptJson(content));
                    return true;
                }
                catch (IOException e) when (e.Message.Contains("because it is being used by another process"))
                {
                    // If some other process is using this file, retry operation unless elapsed times is greater than 10sec
                    sw ??= Stopwatch.StartNew();
                    if (sw.ElapsedMilliseconds > 10000)
                        return false;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }
    }
}