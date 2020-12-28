
using Com4Love.Qmax;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Com4Love.Qmax.Data
{
    [System.Serializable]
    public struct PreloadData
    {
        public Object assets;
        public int amount;
        public string key;

        [HideInInspector]
        public string path;     //基於Assets下的路徑
        [HideInInspector]
        public AssetsType type;
    }
}
