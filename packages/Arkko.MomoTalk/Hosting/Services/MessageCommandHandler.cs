using Arkko.MomoTalk.Common;
using Arkko.MomoTalk.Conversion;
using Arkko.MomoTalk.Foundation.Utils;
using Arkko.MomoTalk.OneBot;
using Arkko.MomoTalk.OneBot.Protocol.Events;
using Arkko.MomoTalk.OneBot.Protocol.Messages;
using Microsoft.Extensions.DependencyInjection;
using System.Linq.Expressions;
using System.Reflection;

namespace Arkko.MomoTalk.Hosting.Services;

// todo
[Singleton]
public class MessageCommandHandler {
    private readonly IServiceProvider _serviceProvider;

    private readonly Dictionary<Type, IMessageConverter> _typeConverterMap = [];

    private readonly Dictionary<Type, Type> _messageToTypeMap = [];

    private readonly Dictionary<Delegate, DelegateInfo> _delegateInfoMap = [];

    private readonly Dictionary<string, Delegate> _aliasToDelegateMap = [];

    private readonly Dictionary<Delegate, MessageCommandInfo> _delegateToCommandInfoMap = [];

    private readonly Dictionary<Type, IMessageChainParser> _returnValueParserMap = [];

    private readonly Dictionary<Type, Type> _returnValueParserTypeMap = [];

    public MessageCommandHandler(IServiceProvider serviceProvider, BotCollectionService botCollectionService) {
        _serviceProvider = serviceProvider;

        RegisterMessageConverter(new SelfMessageConverter<ObFace>());
        RegisterMessageConverter(new SelfMessageConverter<ObReply>());

        RegisterMessageConverter(new SelfMessageConverter<ObAt>());
        RegisterMessageConverter(new ObAtMessageConverter());

        RegisterMessageConverter(new SelfMessageConverter<ObImage>());
        RegisterMessageConverter(new ObImageToByteArrayMessageConverter());
        RegisterMessageConverter(new ObImageToBase64StringMessageConverter());

        RegisterMessageConverter(new SelfMessageConverter<ObText>());
        RegisterMessageConverter(new ObTextMessageConverter<string>());
        RegisterMessageConverter(new ObTextMessageConverter<sbyte>());
        RegisterMessageConverter(new ObTextMessageConverter<short>());
        RegisterMessageConverter(new ObTextMessageConverter<int>());
        RegisterMessageConverter(new ObTextMessageConverter<long>());
        RegisterMessageConverter(new ObTextMessageConverter<byte>());
        RegisterMessageConverter(new ObTextMessageConverter<ushort>());
        RegisterMessageConverter(new ObTextMessageConverter<uint>());
        RegisterMessageConverter(new ObTextMessageConverter<ulong>());
        RegisterMessageConverter(new ObTextMessageConverter<float>());
        RegisterMessageConverter(new ObTextMessageConverter<double>());
        RegisterMessageConverter(new ObTextMessageConverter<decimal>());
        RegisterMessageConverter(new ObTextMessageConverter<bool>());
        RegisterMessageConverter(new ObTextMessageConverter<char>());
        RegisterMessageConverter(new ObTextMessageConverter<Enum>());

        IMessageChainParser objectParser = new MessageChainParsers.ObjectToText();

        foreach (Type t in new[] {
            typeof(string), typeof(sbyte), typeof(short), typeof(int), typeof(long),
            typeof(byte), typeof(ushort), typeof(uint), typeof(ulong), typeof(float),
            typeof(double), typeof(decimal), typeof(bool), typeof(char), typeof(Enum),
        }) {
            RegisterReturnValueParser(t, objectParser);
        }

        RegisterReturnValueParser(typeof(MessageChain), new MessageChainParsers.FromSelf());
        RegisterReturnValueParser(typeof(MessageBase), new MessageChainParsers.FromMessage());

        // repo.EventMessageGroup += OnEventMessageGroup;
        // repo.EventMessagePrivate += OnEventMessagePrivate;
    }

    public void RegisterCommand(MessageCommandInfo info, Delegate @delegate) {
        ParameterInfo[] parameters = @delegate.Method.GetParameters();
        ParamProvider[] paramProviders = new ParamProvider[parameters.Length];
        ServiceRegistryService? serviceRegistryService = _serviceProvider.GetService<ServiceRegistryService>();

        for (int i = 0; i < parameters.Length; i++) {
            ParameterInfo parameter = parameters[i];
            Type paramType = parameter.ParameterType;
            bool nullable = ReflectionUtils.IsParameterNullable(parameter);

            if (_typeConverterMap.ContainsKey(paramType)) {
                Type messageType = (from kv in _messageToTypeMap where kv.Value == paramType select kv.Key)
                    .First();

                paramProviders[i] = MakeMessageParamProvider(parameter, messageType, paramType, nullable);

                continue;
            }

            if (serviceRegistryService != null) {
                List<ServiceInfo> serviceInfos = serviceRegistryService.GetServiceInfo(paramType).ToList();

                if (serviceInfos.Count != 0) {
                    ServiceInfo serviceInfo = serviceInfos[0];

                    if (!serviceInfo.IsKeyed) {
                        paramProviders[i] = MakeServiceProvider(
                            parameter, paramType, sp => sp.GetRequiredService(paramType), nullable
                        );
                    } else {
                        FromKeyedServicesAttribute? attr = paramType.GetCustomAttribute<FromKeyedServicesAttribute>();

                        if (attr == null) {
                            throw new InvalidOperationException(
                                "FromKeyedService attribute is needed while injecting keyed service"
                            );
                        }

                        if (attr.Key != serviceInfo.Key) {
                            throw new InvalidOperationException("service with specific key not found");
                        }

                        paramProviders[i] = MakeServiceProvider(
                            parameter, paramType,
                            sp => sp.GetRequiredKeyedService(paramType, serviceInfo.Key), nullable
                        );
                    }
                } else if (serviceInfos.Count > 1) {
                    FromKeyedServicesAttribute? attr = paramType.GetCustomAttribute<FromKeyedServicesAttribute>();

                    if (attr == null) {
                        ServiceInfo? serviceInfo = (
                            from si in serviceInfos where !si.IsKeyed select si
                        ).FirstOrDefault();

                        if (serviceInfo == null) {
                            throw new InvalidOperationException(
                                "service not found, if you want to inject a keyed service, add FromKeyedServices attribute on parameter"
                            );
                        }

                        paramProviders[i] = MakeServiceProvider(
                            parameter, paramType, sp => sp.GetRequiredService(paramType), nullable
                        );
                    } else {
                        ServiceInfo? serviceInfo = (
                            from si in serviceInfos where si.Key == attr.Key select si
                        ).FirstOrDefault();

                        if (serviceInfo == null) {
                            throw new InvalidOperationException("service with specific key not found");
                        }

                        paramProviders[i] = MakeServiceProvider(
                            parameter,
                            paramType,
                            sp => sp.GetRequiredKeyedService(paramType, serviceInfo.Key),
                            nullable
                        );
                    }
                }
            }

            throw new InvalidOperationException(
                "unable to recognize whether parameter should be injected from host or be from message"
            );
        }

        ReturnInfo returnInfo = ReflectionUtils.AnalyzeReturn(@delegate.Method);

        _delegateInfoMap[@delegate] = new DelegateInfo {
            ParamProviders = paramProviders,
            ReturnProvider = returnInfo.HasReturnValue ? MakeReturnProvider(returnInfo.ReturnType!) : null,
            UseScoped = paramProviders.Any(p => p.ProvidedByHost),
            ReturnInfo = returnInfo,
        };

        foreach (string alias in info.Aliases) {
            _aliasToDelegateMap[alias] = @delegate;
        }

        _delegateToCommandInfoMap[@delegate] = info;
    }

    private void RegisterMessageConverter<TMessage, TOut>(IMessageConverter<TMessage, TOut> converter)
    where TMessage : MessageBase {
        _typeConverterMap[typeof(TOut)] = converter;
        _messageToTypeMap[typeof(TMessage)] = typeof(TOut);
    }

    private void RegisterReturnValueParser(Type returnType, IMessageChainParser parser) {
        _returnValueParserMap[returnType] = parser;
        _returnValueParserTypeMap[returnType] = parser.GetType();
    }

    private async Task OnEventMessageGroup(MomoTalk momoTalk, EventMessageGroup ev) {
        await HandleMessage(momoTalk, ev);
    }

    private async Task OnEventMessagePrivate(MomoTalk momoTalk, EventMessagePrivate ev) {
        await HandleMessage(momoTalk, ev);
    }

    private async Task HandleMessage(MomoTalk momoTalk, EventMessage ev) {
        // test
        if (ev is not EventMessagePrivate { UserId: 1781176460 }) {
            return;
        }
        
        MessageChain messageChain = ev.Message;

        int commandTextIndex = messageChain.FindIndex(msg => msg is ObText t && !string.IsNullOrWhiteSpace(t.Text));

        if (commandTextIndex < 0) {
            return;
        }

        string commandText = ((ObText)messageChain[commandTextIndex]).Text;

        if (CommandNameStartsWithPrefix(commandText) && commandText.Length > 1) {
            commandText = commandText[(commandTextIndex + 1)..];
        }

        if (!_aliasToDelegateMap.TryGetValue(commandText, out Delegate? @delegate)) {
            return;
        }

        List<MessageBase> messageParams = new(messageChain.Count);

        foreach (MessageBase msg in messageChain[(commandTextIndex + 1)..]) {
            if (msg is not ObText t || !string.IsNullOrWhiteSpace(t.Text)) {
                messageParams.Add(msg);
            }
        }


        DelegateInfo delegateInfo = _delegateInfoMap[@delegate];

        using IServiceScope scope = _serviceProvider.CreateScope();

        object?[] parameters = new object?[delegateInfo.ParamProviders.Length];

        int j = 0;

        for (int i = 0; i < delegateInfo.ParamProviders.Length; i++) {
            ParamProvider provider = delegateInfo.ParamProviders[i];

            if (provider.ProvidedByHost) {
                parameters[i] = provider.Delegate.DynamicInvoke();
            } else {
                if (j < messageParams.Count) {
                    parameters[i] = @delegate.DynamicInvoke(messageParams[j++]);
                } else if (!provider.Nullable) {
                    await ev.ReplyAsync(MessageChain.Builder.Text(
                        $"not provided: {provider.ParameterInfo.Name ?? "*unnamed*"}"
                    ).Build());

                    return;
                }
            }
        }

        object? ret = await ReflectionUtils.AwaitObjectAsync(@delegate.DynamicInvoke(parameters));

        if (ret == null) {
            return;
        }

        MessageChain? reply = (MessageChain?)delegateInfo.ReturnProvider?.DynamicInvoke(ret);

        if (reply != null) {
            await ev.ReplyAsync(reply);
        }
    }

    private static bool CommandNameStartsWithPrefix(string commandName) {
        return commandName.StartsWith('/') || commandName.StartsWith('.') || commandName.StartsWith('!');
    }

    private ParamProvider MakeMessageParamProvider(
        ParameterInfo parameterInfo, Type messageType, Type targetType, bool nullable
    ) {
        // messageType message
        ParameterExpression paramExpr = Expression.Parameter(messageType, "message");

        // _typeConverterMap[messageType]
        IMessageConverter converter = _typeConverterMap[messageType];
        ConstantExpression converterExpr = Expression.Constant(converter);

        // (IMessageConverter<messageType, targetType>)_typeConverterMap[messageType]
        Type converterInterfaceType = typeof(IMessageConverter<,>).MakeGenericType(messageType, targetType);
        UnaryExpression cast = Expression.Convert(converterExpr, converterInterfaceType);

        // ((IMessageConverter<messageType, targetType>)_typeConverterMap[messageType]).ConvertMessage(message)
        MethodInfo convertMethod = converterInterfaceType.GetMethod("ConvertMessage")!;
        MethodCallExpression methodCall = Expression.Call(cast, convertMethod, paramExpr);

        // (messageType message) => ((IMessageConverter<messageType, targetType>)_typeConverterMap[messageType]).ConvertMessage(message)
        Expression body = Expression.Return(Expression.Label(targetType), methodCall);
        Type fnType = typeof(Func<,>).MakeGenericType(messageType, typeof(object));
        Delegate @delegate = Expression.Lambda(fnType, body, paramExpr).Compile();

        return new ParamProvider {
            Delegate = @delegate,
            ParameterInfo = parameterInfo,
            ProvidedByHost = false,
            Nullable = nullable,
        };
    }

    private static ParamProvider MakeServiceProvider(
        ParameterInfo parameterInfo, Type serviceType, Func<IServiceProvider, object> provider, bool nullable
    ) {
        return new ParamProvider {
            Delegate = provider,
            ParameterInfo = parameterInfo,
            ProvidedByHost = true,
            Nullable = nullable,
        };
    }

    private Delegate MakeReturnProvider(Type returnType) {
        if (!_returnValueParserMap.TryGetValue(returnType, out IMessageChainParser? parser)) {
            throw new InvalidOperationException("unsupported return type");
        }

        // returnType returnValue
        ParameterExpression paramExpr = Expression.Parameter(returnType, "returnValue");
        ConstantExpression parserExpr = Expression.Constant(parser);
        Type parserType = _returnValueParserTypeMap[returnType];

        // (parserType)parser
        UnaryExpression cast = Expression.Convert(parserExpr, parserType);

        // ((parserType)parser).Parse(returnValue)
        MethodInfo convertMethod = parserType.GetMethod("Parse")!;
        MethodCallExpression methodCall = Expression.Call(cast, convertMethod, paramExpr);

        // (returnType delegateReturn) => ((parserType)parser).Parse(delegateReturn)
        Expression body = Expression.Return(Expression.Label(typeof(MessageChain)), methodCall);
        Type fnType = typeof(Func<,>).MakeGenericType(returnType, typeof(MessageChain));
        return Expression.Lambda(fnType, body, paramExpr).Compile();
    }

    private struct DelegateInfo {
        public required ParamProvider[] ParamProviders { get; set; }
        public required Delegate? ReturnProvider { get; set; }
        public required bool UseScoped { get; set; }
        public required ReturnInfo ReturnInfo { get; set; }
    }

    private struct ParamProvider {
        public required Delegate Delegate { get; set; }
        public required ParameterInfo ParameterInfo { get; set; }
        public required bool ProvidedByHost { get; set; }
        public required bool Nullable { get; set; }
    }
}
