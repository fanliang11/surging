namespace Surging.Core.CPlatform.Configurations
{
    /// <summary>
    /// Defines the <see cref="ProtocolPortOptions" />
    /// </summary>
    public class ProtocolPortOptions
    {
        #region 属性

        /// <summary>
        /// Gets or sets the HttpPort
        /// </summary>
        public int? HttpPort { get; set; }

        /// <summary>
        /// Gets or sets the MQTTPort
        /// </summary>
        public int MQTTPort { get; set; }

        /// <summary>
        /// Gets or sets the WSPort
        /// </summary>
        public int WSPort { get; set; }

        #endregion 属性
    }
}