
namespace Com4Love.Qmax.Tools
{
    /// <summary>
    /// 存儲reslist裡的條目的結構
    /// </summary>
    public struct AssetBundleInfo
    {

        public string Path;
        public int Version;
        public string Hash;
        public int Size;

        /// <summary>
        ///是否隨包打包，存儲在StreamingAssetPath目錄裡
        /// </summary>
        public bool Packaged;
    }
}
