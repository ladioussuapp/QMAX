using Com4Love.Qmax;
using Com4Love.Qmax.Net.Protocols;
using System;
using System.Collections.Generic;
using System.IO;
using Com4Love.Qmax.Net.Protocols.goods;
namespace Com4Love.Qmax.Net.Protocols.Stage
{
    public class BeginStageResponse:Protocol
    {
        /// <summary>
        ///  数值改变
        /// </summary>
        public ValueResultListResponse valueResultListResponse;
        /// <summary>
        ///  需要更新的关卡信息
        /// </summary>
        public Stage stage;
        /// <summary>
        ///  使用物品后刷新物品
        /// </summary>
        public List<GoodsItem> goodsItems;
        public BeginStageResponse()
        {
        }
        public BeginStageResponse(ValueResultListResponse valueResultListResponse,Stage stage,List<GoodsItem> goodsItems)
        {
            this.valueResultListResponse = valueResultListResponse;
            this.stage = stage;
            this.goodsItems = goodsItems;
        }
        public override int Deserialize(Stream stream, bool isLittleEndian = true)
        {
            long len = stream.Length;
            valueResultListResponse = Serializer.Read<ValueResultListResponse>(stream, isLittleEndian);
            stage = Serializer.Read<Stage>(stream, isLittleEndian);
            goodsItems = Serializer.ReadList<GoodsItem>(stream, isLittleEndian);
            return (int)(len - stream.Length);
        }
        public override int Serialize(Stream stream, bool isLittleEndian = true)
        {
            long len = stream.Length;
            Serializer.ToBytes(stream, valueResultListResponse, isLittleEndian);
            Serializer.ToBytes(stream, stage, isLittleEndian);
            Serializer.ToBytes(stream, goodsItems, isLittleEndian);
            return (int)(stream.Length - len);
        }
        public override string ToString ()
        {
            return string.Format("[BeginStageResponse]:valueResultListResponse={0},stage={1},goodsItems={2}",valueResultListResponse,stage,goodsItems);
        }
    }
}
