using UnityEngine;
using System.Collections;
using Com4Love.Qmax.Net.Protocols.sign;
using System.Collections.Generic;
using Com4Love.Qmax.Net.Protocols;

namespace Com4Love.Qmax.Data
{
    public class LoginGiveData
    {
        /// <summary>
        /// 獎勵信息///
        /// </summary>
        public List<ValueResult> list;

        /// <summary>
        /// 登陸天數////
        /// </summary>
        public int Days = -1;
        /// <summary>
        /// 當前天數獎勵是否可領取///
        /// </summary>
        public bool IsCanAward = false;

    }
}

