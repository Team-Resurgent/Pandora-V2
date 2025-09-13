using ManagedBass;

namespace Pandora.Utils
{
    public static class SoundPlayer
    {
        private static bool _initialized = false;

        private static int _ftpConnectHandle;
        private static int _ftpDisconnectHandle;
        private static int _driveConnectHandle;
        private static int _driveDisconnectHandle;

        static SoundPlayer()
        {
            Initialize();
        }

        private static void Initialize()
        {
            if (_initialized)
            {
                return;
            }
            Bass.Init();
            var ftpConnectData = AssetLoader.LoadAssetBytes("Sounds/ftp-connect.mp3");
            _ftpConnectHandle = Bass.CreateStream(ftpConnectData, 0, ftpConnectData.Length, BassFlags.Default);
            var ftpDisconnectData = AssetLoader.LoadAssetBytes("Sounds/ftp-disconnect.mp3");
            _ftpDisconnectHandle = Bass.CreateStream(ftpDisconnectData, 0, ftpDisconnectData.Length, BassFlags.Default);
            var driveConnectData = AssetLoader.LoadAssetBytes("Sounds/drive-connect.mp3");
            _driveConnectHandle = Bass.CreateStream(driveConnectData, 0, driveConnectData.Length, BassFlags.Default);
            var driveDisconnectData = AssetLoader.LoadAssetBytes("Sounds/drive-disconnect.mp3");
            _driveDisconnectHandle = Bass.CreateStream(driveDisconnectData, 0, driveDisconnectData.Length, BassFlags.Default);
            _initialized = true;
        }

        public static void PlayFtpConnect()
        {
            if (!_initialized)
            {
                Initialize();
            }
            Bass.ChannelStop(_ftpDisconnectHandle);
            Bass.ChannelPlay(_ftpConnectHandle, true);
        }

        public static void PlayFtpDisconnect()
        {
            if (!_initialized)
            {
                Initialize();
            }
            Bass.ChannelStop(_ftpConnectHandle);
            Bass.ChannelPlay(_ftpDisconnectHandle, true);
        }

        public static void PlayDriveConnect()
        {
            if (!_initialized)
            {
                Initialize();
            }
            Bass.ChannelStop(_driveDisconnectHandle);
            Bass.ChannelPlay(_driveConnectHandle, true);
        }

        public static void PlayDriveDisconnect()
        {
            if (!_initialized)
            {
                Initialize();
            }
            Bass.ChannelStop(_driveConnectHandle);
            Bass.ChannelPlay(_driveDisconnectHandle, true);
        }


        public static void Cleanup()
        {
            if (!_initialized)
            {
                return;
            }
            Bass.StreamFree(_ftpConnectHandle);
            Bass.StreamFree(_ftpDisconnectHandle);
            Bass.StreamFree(_driveConnectHandle);
            Bass.StreamFree(_driveDisconnectHandle);
            Bass.Free();
            _initialized = false;
        }
    }
}
