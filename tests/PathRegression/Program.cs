using System;
using System.Collections.ObjectModel;
using System.IO;
using Pandora.Logging;
using Pandora.Models;
using Pandora.Storage;

var logger = new SilentLogger();
var local = new ConnectionLocal();
var proxy = new StorageProviderProxy(logger, new ObservableCollection<IConnection> { local }, new ConnectionManager(logger));
var provider = new LocalStorageProvider(logger, local);
var sandbox = Path.Combine(Path.GetTempPath(), "pandora-path-" + Guid.NewGuid());
Directory.CreateDirectory(sandbox);
var previousDirectory = Environment.CurrentDirectory;
Environment.CurrentDirectory = sandbox;
try
{
    var downloads = Path.Combine(sandbox, "Downloads");
    Directory.CreateDirectory(downloads);
    // Match the copy command: combine the destination and filename, then the
    // relative suffix (empty for a file, slash-prefixed for a folder's children).
    var target = proxy.CombinePath(downloads, "example.7z");
    target = proxy.CombinePath(target, "");
    using (var stream = provider.OpenWriteStream(target, 1) ?? throw new Exception("Cannot open destination"))
        stream.WriteByte(42);
    var expected = Path.Combine(downloads, "example.7z");
    if (!File.Exists(expected) || File.ReadAllBytes(expected)[0] != 42)
        throw new Exception($"Download missing from destination. Actual path: {target}");

    var folder = proxy.CombinePath(downloads, "folder");
    var nested = proxy.CombinePath(folder, "/child/example.7z");
    var expectedChild = Path.Combine(downloads, "folder", "child", "example.7z");
    Directory.CreateDirectory(Path.GetDirectoryName(expectedChild)!);
    using (var stream = provider.OpenWriteStream(nested, 1) ?? throw new Exception("Cannot open nested destination"))
        stream.WriteByte(43);
    if (!File.Exists(expectedChild) || File.ReadAllBytes(expectedChild)[0] != 43)
        throw new Exception($"Nested download missing from destination. Actual path: {nested}");

    if (!OperatingSystem.IsWindows())
    {
        var literal = proxy.CombinePath(downloads, @"literal\name.7z");
        if (literal != Path.Combine(downloads, @"literal\name.7z"))
            throw new Exception("Local filename's literal backslash was changed");
    }
    Console.WriteLine("PASS: local downloads keep native absolute paths and nested destinations.");
}
finally
{
    Environment.CurrentDirectory = previousDirectory;
    Directory.Delete(sandbox, true);
}

sealed class SilentLogger : ILogger
{
    public void LogMessage(string type, string message) { }
}
