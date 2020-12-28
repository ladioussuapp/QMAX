using Com4Love.Qmax;
using Com4Love.Qmax.Net.Protocols;
using System;
using System.Collections.Generic;
using System.IO;
namespace Com4Love.Qmax.Net.Protocols.recharge
{
    public class PurchaseRequest:Protocol
    {
        /// <summary>
        ///  ≥‰÷µreceipt
        /// </summary>
        public String receipt;
        public PurchaseRequest()
        {
        }
        public PurchaseRequest(String receipt)
        {
            this.receipt = receipt;
        }
        public override int Deserialize(Stream stream, bool isLittleEndian = true)
        {
            long len = stream.Length;
            receipt = Serializer.Read<String>(stream, isLittleEndian);
            return (int)(len - stream.Length);
        }
        public override int Serialize(Stream stream, bool isLittleEndian = true)
        {
            long len = stream.Length;
            Serializer.ToBytes(stream, receipt, isLittleEndian);
            return (int)(stream.Length - len);
        }
        public override string ToString ()
        {
            return string.Format("[PurchaseRequest]:receipt={0}",receipt);
        }
    }
}