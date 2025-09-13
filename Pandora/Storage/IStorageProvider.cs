using Pandora.Models;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Pandora.Storage
{
    public interface IStorageProvider
    {
        bool IsReadOnly { get; }

        Protocol Protocol { get; }

        Task<bool> ConnectAsync();

        void Disconnect();

        bool TryGetRootItems(out IEnumerable<RootItemInfo> rootItems);

        bool TryGetFileItems(string path, out IEnumerable<FileItemInfo> fileItems);

        bool TryRecurseFileItems(FileItemInfo folder, out IEnumerable<FileItemInfo> fileItems);

        bool TryCreateFolder(string path);

        bool TryDelete(FileItemInfo? fileItemInfo);

        bool TryRename(FileItemInfo? fileItemInfo, string new_name);

        Stream? OpenWriteStream(string path, long fileLen);

        Stream? OpenReadStream(string path, long fileLen);
    }
}
