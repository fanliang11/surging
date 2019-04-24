using MessagePack;
using ProtoBuf;
using Surging.Core.System.Intercept;

namespace Surging.IModuleServices.Common.Models
{
    [ProtoContract]
    public class UserModel
    {

        [ProtoMember(1)]
        [CacheKey(1)]
        public int UserId { get; set; }

        [ProtoMember(2)]
        public string Name { get; set; }

        [ProtoMember(3)]
        public int Age { get; set; }

        [ProtoMember(4)]
        public Sex Sex { get; set; }

    }
}
