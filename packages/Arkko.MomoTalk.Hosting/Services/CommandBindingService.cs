using Arkko.MomoTalk.Foundation.Utils;
using Arkko.MomoTalk.Hosting.Attributes;
using Arkko.MomoTalk.Hosting.Common;
using Arkko.MomoTalk.Hosting.Conversions;
using Arkko.MomoTalk.OneBot.Protocol.Events;
using Arkko.MomoTalk.OneBot.Protocol.Messages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Arkko.MomoTalk.Hosting.Services;

[Singleton]
public class CommandBindingService {
    private readonly Dictionary<string, Command> _commands = [];

    private readonly Dictionary<Type, IMessageConverter> _converters = [];

    private readonly ILogger<CommandBindingService> _logger;

    private readonly Dictionary<Type, IMessageChainParser> _returnParsers = [];

    private readonly IServiceProvider _serviceProvider;

    public CommandBindingService(
        ILogger<CommandBindingService> logger, IServiceProvider serviceProvider, FileDownloadService fileDownloadService
    ) {
        _logger = logger;
        _serviceProvider = serviceProvider;

        RegisterMessageConverter(new SelfMessageConverter<ObFace>());
        RegisterMessageConverter(new SelfMessageConverter<ObReply>());

        RegisterMessageConverter(new SelfMessageConverter<ObAt>());
        RegisterMessageConverter(new ObAtMessageConverter());

        RegisterMessageConverter(new SelfMessageConverter<ObImage>());
        RegisterMessageConverter(new ObImageMessageConverters.ToBase64String(fileDownloadService));
        RegisterMessageConverter(new ObImageMessageConverters.ToByteArray(fileDownloadService));

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

        RegisterReturnValueParser(typeof(byte[]), new MessageChainParsers.ByteArrayToImage());
        RegisterReturnValueParser(typeof(MessageChain), new MessageChainParsers.FromSelf());
        RegisterReturnValueParser(typeof(MessageBase), new MessageChainParsers.FromMessage());

        IEnumerable<MethodInfo> methods = ReflectionUtils.FindMethodsWithAttribute<MessageCommandMappingAttribute>();

        foreach (MethodInfo method in methods) {
            Delegate fn;

            if (method.IsStatic) {
                fn = CreateStrongTypedExpressionDelegate(method);
            } else {
                object service = serviceProvider.GetRequiredService(method.DeclaringType
                    ?? throw new MethodAccessException(
                        "IL based method is not supported"
                    )
                );

                fn = CreateStrongTypedExpressionDelegate(method, service);
            }

            MessageCommandMappingAttribute mappingAttribute
                = method.GetCustomAttribute<MessageCommandMappingAttribute>()!;

            string category = "default";

            MessageCommandCategoryAttribute? categoryAttribute
                = method.GetCustomAttribute<MessageCommandCategoryAttribute>();

            if (categoryAttribute != null) {
                category = categoryAttribute.Category;
            } else {
                MessageCommandCategoryAttribute? classCategoryAttribute
                    = method.DeclaringType?.GetCustomAttribute<MessageCommandCategoryAttribute>();

                if (classCategoryAttribute != null) {
                    category = classCategoryAttribute.Category;
                }
            }

            MessageCommandInfo info = new() {
                Category = category,
                Aliases = mappingAttribute.Aliases,
                Description = mappingAttribute.Description,
                Example = mappingAttribute.Example,
                HiddenFromHelp = mappingAttribute.HiddenFromHelp,
            };

            RegisterCommand(info, fn, method.GetParameters());
        }

        RegisterCommand(new MessageCommandInfo {
            Category = "default",
            Aliases = ["help"],
            Description = "获取指令帮助",
            Example = "",
            HiddenFromHelp = false,
        }, (string? category) => BuildHelpCommandText(category ?? "default"));
    }

    private void RegisterCommand(MessageCommandInfo info, Delegate fn, ParameterInfo[] parameterInfos) {
        Command command = new(info, fn, parameterInfos);

        foreach (string alias in info.Aliases) {
            _commands[alias] = command;
        }
    }

    private void RegisterCommand(MessageCommandInfo info, Delegate fn) {
        RegisterCommand(info, fn, fn.Method.GetParameters());
    }

    private void RegisterMessageConverter<TMessage, TOut>(IMessageConverter<TMessage, TOut> converter)
    where TMessage : MessageBase {
        _converters[typeof(TOut)] = converter;
    }

    private void RegisterReturnValueParser(Type returnType, IMessageChainParser parser) {
        _returnParsers[returnType] = parser;
    }

    /*
     * 1st types > types that have registered converters > other types
     * 1st types:
     *      EventMessage, MomoTalk, MessageChain
     * for other types, try looking for them from service provider, or throw exception
     */

    [SocketEventHandler]
    public async Task HandleGroupMessage(MomoTalk momoTalk, EventMessageGroup ev) {
        await HandleMessage(momoTalk, ev).ConfigureAwait(false);
    }

    [SocketEventHandler]
    public async Task HandlePrivateMessage(MomoTalk momoTalk, EventMessagePrivate ev) {
        await HandleMessage(momoTalk, ev).ConfigureAwait(false);
    }

    private async Task HandleMessage(MomoTalk momoTalk, EventMessage ev) {
        if (ev.UserId != 1781176460) {
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

        if (!_commands.TryGetValue(commandText, out Command? command)) {
            return;
        }

        List<MessageBase> messageParams = new(messageChain.Count);

        ObReply? replyEntity = (ObReply?)messageChain.FirstOrDefault(m => m is ObReply);

        if (replyEntity != null) {
            messageParams.Add(replyEntity);
        }

        List<MessageBase> rawMessageParams = messageChain[(commandTextIndex + 1)..];

        foreach (MessageBase msg in rawMessageParams) {
            if (msg is not ObText t || !string.IsNullOrWhiteSpace(t.Text)) {
                messageParams.Add(msg);
            }
        }

        await using AsyncServiceScope serviceScope = _serviceProvider.CreateAsyncScope();
        ParameterInfo[] parameterInfos = command.ParameterInfos;
        object?[] parameters = new object[parameterInfos.Length];

        int currentMessageParamIndex = 0;

        for (int i = 0; i < parameterInfos.Length; i++) {
            ParameterInfo parameterInfo = parameterInfos[i];
            Type parameterType = parameterInfo.ParameterType;

            if (parameterType == typeof(EventMessage)) {
                parameters[i] = ev;
                continue;
            }

            if (parameterType == typeof(EventMessageGroup)) {
                if (ev is EventMessageGroup emg) {
                    parameters[i] = emg;
                    continue;
                }

                return;
            }

            if (parameterType == typeof(EventMessagePrivate)) {
                if (ev is EventMessagePrivate emp) {
                    parameters[i] = emp;
                    continue;
                }

                return;
            }

            if (parameterType == typeof(MomoTalk)) {
                parameters[i] = momoTalk;
                continue;
            }

            if (parameterType == typeof(MessageChain)) {
                if (parameterInfo.GetCustomAttribute<RawParameterChainAttribute>() != null) {
                    MessageChain chain = new(rawMessageParams);

                    if (chain.FirstOrDefault().TryGet(out MessageBase? msg)
                        && msg is ObText t
                        && string.IsNullOrWhiteSpace(t.Text)
                    ) {
                        if (t.Text.Length == 1) {
                            chain.Remove(t);
                        } else {
                            chain[0] = new ObText(t.Text[1..]);
                        }
                    }

                    parameters[i] = chain;
                } else if (parameterInfo.GetCustomAttribute<SplitParameterChainAttribute>() != null) {
                    parameters[i] = new MessageChain(messageParams);
                } else {
                    parameters[i] = ev.Message;
                }

                continue;
            }

            if (parameterInfo.GetCustomAttribute<UserIdAttribute>() != null) {
                parameters[i] = ev.UserId;

                continue;
            }

            if (parameterInfo.GetCustomAttribute<GroupIdAttribute>() != null) {
                if (ev is EventMessageGroup emg) {
                    parameters[i] = emg.GroupId;
                } else if (ev is EventMessagePrivate && !ReflectionUtils.IsParameterNullable(parameterInfo)) {
                    return;
                } else {
                    parameters[i] = null;
                }

                continue;
            }

            if (_converters.TryGetValue(parameterType, out IMessageConverter? converter)) {
                MessageBase message;

                try {
                    message = messageParams[currentMessageParamIndex];
                } catch (ArgumentOutOfRangeException) {
                    if (parameterInfo.HasDefaultValue) {
                        parameters[i] = parameterInfo.DefaultValue;

                        continue;
                    }

                    if (ReflectionUtils.IsParameterNullable(parameterInfo)) {
                        continue;
                    }

                    await ev.ReplyAsync(MessageChain.BuildTextMessage($"参数过少\n{BuildCommandHelpText(command)}"));

                    return;
                }

                try {
                    parameters[i] = converter.ConvertMessage(message);

                    currentMessageParamIndex++;
                } catch (Exception ex) {
                    if (_logger.IsEnabled(LogLevel.Trace)) {
                        _logger.LogTrace("message conversion error: {}", ex);
                    }

                    await ev.ReplyAsync(MessageChain.BuildTextMessage(
                        $"参数 #{currentMessageParamIndex + 1} {BuildParameterHelpText(parameterInfo, converter)} 非法\n{BuildCommandHelpText(command)}"
                    ));

                    return;
                }
            } else {
                FromKeyedServicesAttribute? fromKeyedServicesAttribute
                    = parameterInfo.GetCustomAttribute<FromKeyedServicesAttribute>();

                if (fromKeyedServicesAttribute == null) {
                    parameters[i] = serviceScope.ServiceProvider.GetRequiredService(parameterType);
                } else {
                    parameters[i] = serviceScope.ServiceProvider.GetRequiredKeyedService(
                        parameterType, fromKeyedServicesAttribute.Key
                    );
                }
            }
        }

        ReturnInfo returnInfo = ReflectionUtils.AnalyzeReturn(command.Fn.Method);

        if (returnInfo.HasReturnValue) {
            object? rawResult = returnInfo.IsTaskBased
                ? await (dynamic)command.Fn.DynamicInvoke(parameters)!
                : command.Fn.DynamicInvoke(parameters);

            if (rawResult == null) {
                return;
            }

            if (!_returnParsers.TryGetValue(rawResult.GetType(), out IMessageChainParser? parser)) {
                throw new InvalidOperationException($"unparsable return type: {rawResult.GetType()}");
            }

            MessageChain reply = parser.Parse(rawResult);

            await ev.ReplyAsync(reply);
        } else {
            if (returnInfo.IsTaskBased) {
                await (dynamic)command.Fn.DynamicInvoke(parameters)!;
            } else {
                command.Fn.DynamicInvoke(parameters);
            }
        }
    }

    private static bool CommandNameStartsWithPrefix(string commandName) {
        return commandName.StartsWith('/') || commandName.StartsWith('.') || commandName.StartsWith('!');
    }

    private static Delegate CreateStrongTypedExpressionDelegate(MethodInfo method, object? target = null) {
        ArgumentNullException.ThrowIfNull(method);

        if (!method.IsPublic) {
            throw new InvalidOperationException($"non-public method: {method.Name}");
        }

        ParameterInfo[] parameters = method.GetParameters();
        Type[] paramTypes = parameters.Select(p => p.ParameterType).ToArray();

        ParameterExpression[] paramExprs = new ParameterExpression[paramTypes.Length];

        for (int i = 0; i < paramTypes.Length; i++) {
            paramExprs[i] = Expression.Parameter(paramTypes[i], parameters[i].Name);
        }

        Expression callExpr;

        if (method.IsStatic) {
            callExpr = Expression.Call(method, paramExprs);
        } else {
            ConstantExpression targetExpr = Expression.Constant(target, method.DeclaringType!);
            callExpr = Expression.Call(targetExpr, method, paramExprs);
        }

        LambdaExpression lambdaExpr = Expression.Lambda(method.ReturnType == typeof(void)
            ? GetActionType(paramTypes)
            : GetFuncType(paramTypes, method.ReturnType), callExpr, paramExprs);

        return lambdaExpr.Compile();
    }

    private static Type GetActionType(Type[] paramTypes) {
        return paramTypes.Length switch {
            0 => typeof(Action),
            1 => typeof(Action<>).MakeGenericType(paramTypes[0]),
            2 => typeof(Action<,>).MakeGenericType(paramTypes[0], paramTypes[1]),
            3 => typeof(Action<,,>).MakeGenericType(paramTypes[0], paramTypes[1], paramTypes[2]),
            4 => typeof(Action<,,,>).MakeGenericType(paramTypes[0], paramTypes[1], paramTypes[2], paramTypes[3]),
            5 => typeof(Action<,,,,>).MakeGenericType(paramTypes[0], paramTypes[1], paramTypes[2], paramTypes[3],
                paramTypes[4]),
            6 => typeof(Action<,,,,,>).MakeGenericType(paramTypes[0], paramTypes[1], paramTypes[2], paramTypes[3],
                paramTypes[4], paramTypes[5]),
            7 => typeof(Action<,,,,,,>).MakeGenericType(paramTypes[0], paramTypes[1], paramTypes[2], paramTypes[3],
                paramTypes[4], paramTypes[5], paramTypes[6]),
            8 => typeof(Action<,,,,,,,>).MakeGenericType(paramTypes[0], paramTypes[1], paramTypes[2], paramTypes[3],
                paramTypes[4], paramTypes[5], paramTypes[6], paramTypes[7]),
            9 => typeof(Action<,,,,,,,,>).MakeGenericType(paramTypes[0], paramTypes[1], paramTypes[2], paramTypes[3],
                paramTypes[4], paramTypes[5], paramTypes[6], paramTypes[7], paramTypes[8]),
            10 => typeof(Action<,,,,,,,,,>).MakeGenericType(paramTypes[0], paramTypes[1], paramTypes[2], paramTypes[3],
                paramTypes[4], paramTypes[5], paramTypes[6], paramTypes[7], paramTypes[8], paramTypes[9]),
            11 => typeof(Action<,,,,,,,,,,>).MakeGenericType(paramTypes[0], paramTypes[1], paramTypes[2], paramTypes[3],
                paramTypes[4], paramTypes[5], paramTypes[6], paramTypes[7], paramTypes[8], paramTypes[9],
                paramTypes[10]),
            12 => typeof(Action<,,,,,,,,,,,>).MakeGenericType(paramTypes[0], paramTypes[1], paramTypes[2],
                paramTypes[3], paramTypes[4], paramTypes[5], paramTypes[6], paramTypes[7], paramTypes[8], paramTypes[9],
                paramTypes[10], paramTypes[11]),
            13 => typeof(Action<,,,,,,,,,,,,>).MakeGenericType(paramTypes[0], paramTypes[1], paramTypes[2],
                paramTypes[3], paramTypes[4], paramTypes[5], paramTypes[6], paramTypes[7], paramTypes[8], paramTypes[9],
                paramTypes[10], paramTypes[11], paramTypes[12]),
            14 => typeof(Action<,,,,,,,,,,,,,>).MakeGenericType(paramTypes[0], paramTypes[1], paramTypes[2],
                paramTypes[3], paramTypes[4], paramTypes[5], paramTypes[6], paramTypes[7], paramTypes[8], paramTypes[9],
                paramTypes[10], paramTypes[11], paramTypes[12], paramTypes[13]),
            15 => typeof(Action<,,,,,,,,,,,,,,>).MakeGenericType(paramTypes[0], paramTypes[1], paramTypes[2],
                paramTypes[3], paramTypes[4], paramTypes[5], paramTypes[6], paramTypes[7], paramTypes[8], paramTypes[9],
                paramTypes[10], paramTypes[11], paramTypes[12], paramTypes[13], paramTypes[14]),
            16 => typeof(Action<,,,,,,,,,,,,,,,>).MakeGenericType(paramTypes[0], paramTypes[1], paramTypes[2],
                paramTypes[3], paramTypes[4], paramTypes[5], paramTypes[6], paramTypes[7], paramTypes[8], paramTypes[9],
                paramTypes[10], paramTypes[11], paramTypes[12], paramTypes[13], paramTypes[14], paramTypes[15]),
            _ => throw new NotSupportedException($"too many arguments"),
        };
    }

    private static Type GetFuncType(Type[] paramTypes, Type returnType) {
        Type[] allTypes = paramTypes.Concat([returnType]).ToArray();

        return allTypes.Length switch {
            1 => typeof(Func<>).MakeGenericType(allTypes[0]),
            2 => typeof(Func<,>).MakeGenericType(allTypes[0], allTypes[1]),
            3 => typeof(Func<,,>).MakeGenericType(allTypes[0], allTypes[1], allTypes[2]),
            4 => typeof(Func<,,,>).MakeGenericType(allTypes[0], allTypes[1], allTypes[2], allTypes[3]),
            5 => typeof(Func<,,,,>).MakeGenericType(allTypes[0], allTypes[1], allTypes[2], allTypes[3], allTypes[4]),
            6 => typeof(Func<,,,,,>).MakeGenericType(allTypes[0], allTypes[1], allTypes[2], allTypes[3], allTypes[4],
                allTypes[5]),
            7 => typeof(Func<,,,,,,>).MakeGenericType(allTypes[0], allTypes[1], allTypes[2], allTypes[3], allTypes[4],
                allTypes[5], allTypes[6]),
            8 => typeof(Func<,,,,,,,>).MakeGenericType(allTypes[0], allTypes[1], allTypes[2], allTypes[3], allTypes[4],
                allTypes[5], allTypes[6], allTypes[7]),
            9 => typeof(Func<,,,,,,,,>).MakeGenericType(allTypes[0], allTypes[1], allTypes[2], allTypes[3], allTypes[4],
                allTypes[5], allTypes[6], allTypes[7], allTypes[8]),
            10 => typeof(Func<,,,,,,,,,>).MakeGenericType(allTypes[0], allTypes[1], allTypes[2], allTypes[3],
                allTypes[4], allTypes[5], allTypes[6], allTypes[7], allTypes[8], allTypes[9]),
            11 => typeof(Func<,,,,,,,,,,>).MakeGenericType(allTypes[0], allTypes[1], allTypes[2], allTypes[3],
                allTypes[4], allTypes[5], allTypes[6], allTypes[7], allTypes[8], allTypes[9], allTypes[10]),
            12 => typeof(Func<,,,,,,,,,,,>).MakeGenericType(allTypes[0], allTypes[1], allTypes[2], allTypes[3],
                allTypes[4], allTypes[5], allTypes[6], allTypes[7], allTypes[8], allTypes[9], allTypes[10],
                allTypes[11]),
            13 => typeof(Func<,,,,,,,,,,,,>).MakeGenericType(allTypes[0], allTypes[1], allTypes[2], allTypes[3],
                allTypes[4], allTypes[5], allTypes[6], allTypes[7], allTypes[8], allTypes[9], allTypes[10],
                allTypes[11], allTypes[12]),
            14 => typeof(Func<,,,,,,,,,,,,,>).MakeGenericType(allTypes[0], allTypes[1], allTypes[2], allTypes[3],
                allTypes[4], allTypes[5], allTypes[6], allTypes[7], allTypes[8], allTypes[9], allTypes[10],
                allTypes[11], allTypes[12], allTypes[13]),
            15 => typeof(Func<,,,,,,,,,,,,,,>).MakeGenericType(allTypes[0], allTypes[1], allTypes[2], allTypes[3],
                allTypes[4], allTypes[5], allTypes[6], allTypes[7], allTypes[8], allTypes[9], allTypes[10],
                allTypes[11], allTypes[12], allTypes[13], allTypes[14]),
            16 => typeof(Func<,,,,,,,,,,,,,,,>).MakeGenericType(allTypes[0], allTypes[1], allTypes[2], allTypes[3],
                allTypes[4], allTypes[5], allTypes[6], allTypes[7], allTypes[8], allTypes[9], allTypes[10],
                allTypes[11], allTypes[12], allTypes[13], allTypes[14], allTypes[15]),
            17 => typeof(Func<,,,,,,,,,,,,,,,,>).MakeGenericType(allTypes[0], allTypes[1], allTypes[2], allTypes[3],
                allTypes[4], allTypes[5], allTypes[6], allTypes[7], allTypes[8], allTypes[9], allTypes[10],
                allTypes[11], allTypes[12], allTypes[13], allTypes[14], allTypes[15], allTypes[16]),
            _ => throw new NotSupportedException("too many arguments"),
        };
    }

    private static string BuildParameterHelpText(ParameterInfo parameterInfo, IMessageConverter converter) {
        string displayTypeName = "???";
        Type messageType = converter.InType;

        if (messageType.IsAssignableTo(typeof(ObAt))) {
            displayTypeName = "At";
        } else if (messageType == typeof(ObFace)) {
            displayTypeName = "内置表情";
        } else if (messageType == typeof(ObImage)) {
            displayTypeName = "图片";
        } else if (messageType == typeof(ObReply)) {
            displayTypeName = "回复消息";
        } else if (messageType == typeof(ObText)) {
            displayTypeName = converter.OutType.IsPrimitive ? converter.OutType.Name : "文本";
        }

        return ReflectionUtils.IsParameterNullable(parameterInfo)
            ? $"[{parameterInfo.Name}: {displayTypeName}]"
            : $"<{parameterInfo.Name}: {displayTypeName}>";
    }

    private string BuildCommandHelpText(Command command) {
        StringBuilder sb = new();

        sb.Append(string.Join('|', command.Info.Aliases.Select(a => $"!{a}")));

        foreach (ParameterInfo parameterInfo in command.ParameterInfos) {
            if (!_converters.TryGetValue(parameterInfo.ParameterType, out IMessageConverter? converter)) {
                continue;
            }

            sb.Append($" {BuildParameterHelpText(parameterInfo, converter)}");
        }

        if (!string.IsNullOrWhiteSpace(command.Info.Description)) {
            sb.AppendLine().Append(command.Info.Description);
        }

        if (!string.IsNullOrWhiteSpace(command.Info.Example)) {
            sb.AppendLine().AppendLine("使用实例：").Append(command.Info.Example);
        }

        return sb.ToString();
    }

    private string BuildHelpCommandText(string category) {
        List<MessageCommandInfo> commandInfos = (
            from command in _commands.Values
            where command.Info.Category == category && !command.Info.HiddenFromHelp
            select command.Info
        ).ToList();

        if (commandInfos.Count == 0) {
            return "该类目下没有可用指令";
        }

        StringBuilder sb = new();

        sb.AppendLine("可用指令：");

        foreach (MessageCommandInfo commandInfo in commandInfos) {
            sb.AppendLine($" - {string.Join("|", commandInfo.Aliases)}：{commandInfo.Description}");
        }

        sb.Append("* 指令尾随 --help 将输出指令的详细帮助");

        return sb.ToString();
    }

    private record Command(MessageCommandInfo Info, Delegate Fn, ParameterInfo[] ParameterInfos);
}
