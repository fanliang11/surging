using System.ComponentModel.DataAnnotations;
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
        [CacheKey(2)]
        public string Name { get; set; }

        [ProtoMember(3)]
        [Range(0, 150, ErrorMessage = "年龄只能在0到150岁之间")]
        [CacheKey(3)]
        public int Age { get; set; }

        [ProtoMember(4)]
        [Range(0, 1, ErrorMessage = "性别只能选男或女")]
        public Sex Sex { get; set; }

    }
}
