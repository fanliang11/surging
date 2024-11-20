using DotNetty.Buffers;
using Jint;
using Jint.Parser;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.Extensions.Logging;
using RulesEngine.Models;
using Surging.Core.CPlatform.Codecs.Core;
using Surging.Core.CPlatform.Utilities;
using Surging.Core.DeviceGateway.Runtime.Device.Message;
using Surging.Core.DeviceGateway.Runtime.Device.Message.Event;
using Surging.Core.DeviceGateway.Runtime.Device.Message.Property;
using Surging.Core.DeviceGateway.Runtime.Device.MessageCodec;
using Surging.Core.DeviceGateway.Runtime.RuleParser.Implementation;
using Surging.Core.DeviceGateway.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Surging.Core.DeviceGateway.Runtime.Device.Implementation
{
    public class ScriptDeviceMessageCodec : DeviceMessageCodec
    {
        public string GlobalVariable { get; private set; }
        public string EncoderScript { get; private set; }
        public string DecoderScript { get; private set; }
        public IObservable<Task<RulePipePayloadParser>> _rulePipePayload;
        private readonly ILogger<ScriptDeviceMessageCodec> _logger; 
        public ScriptDeviceMessageCodec(string script) {

            _logger = ServiceLocator.GetService<ILogger<ScriptDeviceMessageCodec>>();
            RegexOptions options = RegexOptions.Singleline | RegexOptions.IgnoreCase;
            string matchStr = Regex.Match(script, @"var\s*[\w$]*\s*\=.*function.*\(.*\)\s*\{[\s\S]*\}.*?v", options).Value;
            if (!string.IsNullOrEmpty(matchStr))
            {
                DecoderScript = matchStr.TrimEnd('v');
                DecoderScript= Regex.Replace(DecoderScript, @"var\s*[\w$]*\s*\=[.\r|\n|\t|\s]*?(function)\s*\([\w$]*\s*\)\s*\{", "", RegexOptions.IgnoreCase);
                DecoderScript= DecoderScript.Slice(0, DecoderScript.LastIndexOf('}'));
                EncoderScript = script.Replace(DecoderScript, ""); 
               
            }
             var matchStr1 = Regex.Matches(script, @"(?<=var).*?(?==)|(?=;)|(?=v)", options).FirstOrDefault(p=>!string.IsNullOrEmpty(p.Value))?.Value;
            if (!string.IsNullOrEmpty(matchStr1))
            {
                GlobalVariable = matchStr1.TrimEnd(';');
            }
            var ruleWorkflow = new RuleWorkflow(DecoderScript);
            _rulePipePayload= Observable.Return( GetParser( GetRuleEngine(ruleWorkflow), ruleWorkflow));


        }
        public override   IObservable<IDeviceMessage> Decode(MessageDecodeContext context)
        {
            var result = Observable.Return<IDeviceMessage>(null);
            _rulePipePayload.Subscribe(async p =>
            {
                var parser = await p;
                parser.Build(context.GetMessage().Payload);
                parser.HandlePayload().Subscribe(async p =>
                {
                    try
                    {
                        var headerBuffer=parser.GetResult().FirstOrDefault();
                        var buffer = parser.GetResult().LastOrDefault();
                        var str = buffer.GetString(buffer.ReaderIndex, buffer.ReadableBytes, Encoding.UTF8);
                        var buffer1 = Unpooled.Buffer();
                        buffer1.WriteString("8307\0{\"MessageType\":8,\"Data\":{\"deviceId\":\"scro-34\",\"level\":\"alarm\",\"alarmTime\":\"2024-11-07 19:47:00\",\"from\":\"device\",\"alarmType\":\"设备告警\",\"coordinate\":\"33.345,566.33\",\"createTime\":\"2024-11-07 19:47:00\",\"desc\":\"温度超过阈值\"},\"DeviceId\":\"scro-34\",\"EventId\":\"alarm\",\"Timestamp\":1726540220311}", Encoding.UTF8);
                        var strBuild = new StringBuilder();
                        var s = ByteBufferUtil.HexDump(buffer1);
                        var session = await context.GetSession();
                        if (session?.GetOperator() == null)
                        {
                            var onlineMessage = JsonSerializer.Deserialize<DeviceOnlineMessage>(str);
                            result = result.Publish(onlineMessage);
                        }
                        else
                        {
                            var messageType = headerBuffer.GetString(0, 1, Encoding.UTF8);
                            if (Enum.Parse<MessageType>(messageType.ToString()) == MessageType.READ_PROPERTY)
                            {
                                var onlineMessage = JsonSerializer.Deserialize<ReadPropertyMessage>(str);
                                result = result.Publish(onlineMessage);
                            }
                            else if (Enum.Parse<MessageType>(messageType.ToString()) == MessageType.EVENT)
                            {
                                var onlineMessage = JsonSerializer.Deserialize<EventMessage>(str);
                                result = result.Publish(onlineMessage);
                            }
                        }
                    }
                    catch (Exception e)
                    {

                    }
                    finally
                    {
                        p.Release();
                        parser.Close();
                    }
                });
            });
            return result;
        }

        public override IObservable<IEncodedMessage> Encode(MessageEncodeContext context)
        {
            context.Reply(((RespondDeviceMessage<IDeviceMessageReply>)context.Message).NewReply().Success(true));
            return Observable.Empty<IEncodedMessage>();
        }

        private RulesEngine.RulesEngine GetRuleEngine(RuleWorkflow ruleWorkflow)
        {
            var reSettingsWithCustomTypes = new ReSettings { CustomTypes = new Type[] { typeof(RulePipePayloadParser) } };
            var result = new RulesEngine.RulesEngine(new Workflow[] { ruleWorkflow.GetWorkflow() }, null, reSettingsWithCustomTypes);
            return result;
        }

        private async Task<RulePipePayloadParser> GetParser(RulesEngine.RulesEngine engine, RuleWorkflow ruleWorkflow)
        {
            var payloadParser = new RulePipePayloadParser();
            var ruleResult = await engine.ExecuteActionWorkflowAsync(ruleWorkflow.WorkflowName, ruleWorkflow.RuleName, new RuleParameter[] { new RuleParameter("parser", payloadParser) });
            if (ruleResult.Exception != null && _logger.IsEnabled(LogLevel.Error))
                _logger.LogError(ruleResult.Exception, ruleResult.Exception.Message);
            return payloadParser;
        }
    }
}
 