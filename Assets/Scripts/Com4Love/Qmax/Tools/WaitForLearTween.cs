using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Com4Love.Qmax.Tools
{
    public class WaitForLearTween : IEnumerator
    {
        LTDescr ltD;

        public WaitForLearTween(LTDescr ltD)
        {
            this.ltD = ltD;
        }

        public object Current
        {
            get { return ltD; }
        }

        public bool MoveNext()
        {
            return ltD.toggle;
        }

        public void Reset()
        {

        }
    }
}
