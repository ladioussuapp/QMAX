
namespace Com4Love.Qmax.Tools
{
    /// <summary>
    /// 操作锁
    /// </summary>
    public class OperLock
    {
        private int lockCount = 0;

        public int PlusLock()
        {
            Q.Assert(lockCount >= 0);
            lockCount++;
            return lockCount;
        }


        public int MinusLock()
        {
            Q.Assert(lockCount >= 0);
            lockCount--;
            return lockCount;
        }


        public int GetValue()
        {
            return lockCount;
        }

        public void Clear()
        {
            lockCount = 0;
        }
    }
}
