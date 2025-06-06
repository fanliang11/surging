using Surging.Core.CPlatform.Address;
using Surging.Core.CPlatform.Utilities;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Surging.Core.CPlatform.Runtime.Client.Address.Resolvers.Implementation.Selectors.Implementation
{
    /// <summary>
    /// 地址选择器基类。
    /// </summary>
    public abstract class AddressSelectorBase : IAddressSelector
    {
        #region Implementation of IAddressSelector

        private const int DEFAULT_WARMUP = 10 * 60 * 1000;

        /// <summary>
        /// 计算权重
        /// </summary>
        /// <param name="uptime">启动持续时间</param>
        /// <param name="warmup">预热时间</param>
        /// <param name="weight">权重</param>
        /// <returns></returns>
        public static int CalculateWarmupWeight(int uptime, int warmup, int weight)
        {
            int ww = (int)(uptime / ((float)warmup / weight));
            return ww < 1 ? 1 : (Math.Min(ww, weight));
        }

        /// <summary>
        /// 获取权重
        /// </summary>
        /// <param name="addressModel"></param>
        /// <returns></returns>
        public static int GetWeight(AddressModel addressModel)
        {
            int weight;
            weight = addressModel.Weight;
            if (weight > 0)
            {
                long timestamp = addressModel.Timestamp;
                if (timestamp > 0L)
                {
                    var uptime = (System.DateTime.Now -  DateTimeConverter.UnixTimestampToDateTime(timestamp)).TotalMilliseconds;
                    if (uptime < 0)
                    {
                        return 1;
                    }
                    if (uptime > 0 && uptime < DEFAULT_WARMUP)
                    {
                        weight = CalculateWarmupWeight((int)uptime, DEFAULT_WARMUP, weight);
                    }
                }
            }

            return Math.Max(weight, 0);
        }

        /// <summary>
        /// 选择一个地址。
        /// </summary>
        /// <param name="context">地址选择上下文。</param>
        /// <returns>地址模型。</returns>
        async ValueTask<AddressModel> IAddressSelector.SelectAsync(AddressSelectContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));
            if (context.Descriptor == null)
                throw new ArgumentNullException(nameof(context.Descriptor));
            if (context.Address == null)
                throw new ArgumentNullException(nameof(context.Address));

            //  var address = context.Address.ToArray();
            if (context.Address.Count() == 0)
                throw new ArgumentException("没有任何地址信息。", nameof(context.Address));
             
            if (context.Address.Count() == 1)
            {
                return context.Address.First();
            }
            else
            {
                var vt = SelectAsync(context);
                return vt.IsCompletedSuccessfully ? vt.Result : await vt;
            }
        }

        #endregion Implementation of IAddressSelector

        /// <summary>
        /// 选择一个地址。
        /// </summary>
        /// <param name="context">地址选择上下文。</param>
        /// <returns>地址模型。</returns>
        protected abstract ValueTask<AddressModel> SelectAsync(AddressSelectContext context);
    }
}