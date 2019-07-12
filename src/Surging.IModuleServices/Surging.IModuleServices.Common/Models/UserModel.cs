using MessagePack;
using ProtoBuf;
using Surging.Core.System.Intercept;

namespace Surging.IModuleServices.Common.Models
{
    /// <summary>
    /// Defines the <see cref="UserModel" />
    /// </summary>
    [ProtoContract]
    public class UserModel
    {
        #region 属性

        /// <summary>
        /// Gets or sets the Age
        /// </summary>
        [ProtoMember(3)]
        public int Age { get; set; }

        /// <summary>
        /// Gets or sets the Name
        /// </summary>
        [ProtoMember(2)]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the Sex
        /// </summary>
        [ProtoMember(4)]
        public Sex Sex { get; set; }

        /// <summary>
        /// Gets or sets the UserId
        /// </summary>
        [ProtoMember(1)]
        [CacheKey(1)]
        public int UserId { get; set; }

        #endregion 属性
    }
}