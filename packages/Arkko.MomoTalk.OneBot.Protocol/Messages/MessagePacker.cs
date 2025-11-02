using Arkko.MomoTalk.OneBot.Protocol.Events;
using Microsoft.Extensions.Logging;
using System.Reflection;
using System.Text.Json;

namespace Arkko.MomoTalk.OneBot.Protocol.Messages;

public class MessagePacker {
    private readonly ILogger<MessagePacker> _logger;
    
    private readonly Dictionary<string, MessageBase> _factories = [];

    public MessagePacker(ILoggerFactory loggerFactory) {
        _logger = loggerFactory.CreateLogger<MessagePacker>();
        
        IEnumerable<Type> types = from type in Assembly.GetAssembly(typeof(MessageBase))!.GetTypes()
            where typeof(MessageBase).IsAssignableFrom(type)
            select type;

        foreach (Type type in types) {
            ConstructorInfo? ctor = type.GetConstructor([]);

            if (ctor == null) {
                if (_logger.IsEnabled(LogLevel.Warning)) {
                    _logger.LogWarning("unable to find no-param constructor for {}", type);
                }
                
                continue;
            }
            
            MessageBase factory = (MessageBase)ctor.Invoke(null);
            
            _factories[factory.TypeId] = factory;
        }
    }

    public MessageChain UnpackArrayMessages(EventMessage ev, in JsonElement j) {
        MessageChainBuilder builder = MessageChain.Builder;
        
        foreach (JsonElement e in j.EnumerateArray()) {
            string? type = e.GetProperty("type").GetString();

            if (type == null) {
                if (_logger.IsEnabled(LogLevel.Warning)) {
                    _logger.LogWarning("unknown message type for {} since it does not have a type field", e.ToString());
                }
                
                continue;
            }

            if (_factories.TryGetValue(type, out MessageBase? factory)) {
                builder.AppendAll(factory.UnpackArrayMessageJson(ev, e));
            }
        }
        
        return builder.Build();
    }

    public static List<ArrayMessage> PackArrayMessages(MessageChain messageChain) {
        List<ArrayMessage> arrayMessages = new(messageChain.Count);
        
        arrayMessages.AddRange(messageChain.Select(messageBase => messageBase.PackArrayMessage()));

        return arrayMessages;
    }
}
