namespace LittleBit.Modules.IAppModule.Data
{
    public class CrossPlatformTangles
    {
        private readonly byte[] _dataGoogleTangle;
        private readonly byte[] _dataAppleTangle;
        
        public CrossPlatformTangles(byte[] dataGoogleTangle, byte[] dataAppleTangle)
        {
            _dataGoogleTangle = dataGoogleTangle;
            _dataAppleTangle = dataAppleTangle;
        }

        public byte[] GetGoogleData() => _dataGoogleTangle;
        public byte[] GetAppleData() => _dataAppleTangle;

    }
}