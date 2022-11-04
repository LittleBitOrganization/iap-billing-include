namespace LittleBit.Modules.IAppModule.Data
{
    public class CrossPlatformTangles
    {
        private readonly byte[] _dataGoogleTangle;
        private readonly byte[] _dataAppleTangle;
        private readonly byte[] _dataTestAppleTangle;

        public CrossPlatformTangles(byte[] dataGoogleTangle, byte[] dataAppleTangle, byte[] dataTestAppleTangle)
        {
            _dataGoogleTangle = dataGoogleTangle;
            _dataAppleTangle = dataAppleTangle;
            _dataTestAppleTangle = dataTestAppleTangle;
        }

        public byte[] GetGoogleData() => _dataGoogleTangle;
        public byte[] GetAppleData() => _dataAppleTangle;
        public byte[] GetAppleTestData() => _dataTestAppleTangle;

    }
}