using Com4Love.Qmax;
using Com4Love.Qmax.Net.Protocols;
using Com4Love.Qmax.Net.Protocols.goods;
using System;
using System.Collections.Generic;
using System.IO;
namespace Com4Love.Qmax.Net.Protocols.ActorGame
{
    public class ActorGameAndGoodsResponse:Protocol
    {
        /// <summary>
        ///  角色信息
        /// </summary>
        public ActorGameResponse actorGameResponse;
        /// <summary>
        ///  物品信息
        /// </summary>
        public GoodsListResponse goodsListResponse;
        public ActorGameAndGoodsResponse()
        {
        }
        public ActorGameAndGoodsResponse(ActorGameResponse actorGameResponse,GoodsListResponse goodsListResponse)
        {
            this.actorGameResponse = actorGameResponse;
            this.goodsListResponse = goodsListResponse;
        }
        public override int Deserialize(Stream stream, bool isLittleEndian = true)
        {
            long len = stream.Length;
            actorGameResponse = Serializer.Read<ActorGameResponse>(stream, isLittleEndian);
            goodsListResponse = Serializer.Read<GoodsListResponse>(stream, isLittleEndian);
            return (int)(len - stream.Length);
        }
        public override int Serialize(Stream stream, bool isLittleEndian = true)
        {
            long len = stream.Length;
            Serializer.ToBytes(stream, actorGameResponse, isLittleEndian);
            Serializer.ToBytes(stream, goodsListResponse, isLittleEndian);
            return (int)(stream.Length - len);
        }
        public override string ToString ()
        {
            return string.Format("[ActorGameAndGoodsResponse]:actorGameResponse={0},goodsListResponse={1}",actorGameResponse,goodsListResponse);
        }
    }
}
