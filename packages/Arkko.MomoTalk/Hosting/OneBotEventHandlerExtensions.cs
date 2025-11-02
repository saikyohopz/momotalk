using Arkko.MomoTalk.Common;
using Arkko.MomoTalk.Foundation.Utils;
using Arkko.MomoTalk.OneBot;
using Arkko.MomoTalk.OneBot.Protocol.Events;
using System.Reflection;

namespace Arkko.MomoTalk.Hosting;

public static class OneBotEventHandlerExtensions {
    extension(OneBotEventHandlerRepository handler) {
        public void RegisterEventHandlerByAttribute(IServiceProvider serviceProvider) {
            IEnumerable<MethodInfo> methods = ReflectionUtils.FindMethodsWithAttribute<SocketEventHandlerAttribute>();

            foreach (MethodInfo method in methods) {
                if (method.ReturnType != typeof(Task)) {
                    throw new MethodAccessException(
                        "only Task is accepted as return type for any socket event handlers"
                    );
                }

                ParameterInfo[] parameters = method.GetParameters();

                if (parameters.Length != 2) {
                    throw new MethodAccessException(
                        "only <TEvent>(MomoTalk, TEvent) is accepted as parameters for any socket event handlers"
                    );
                }

                Type t1 = parameters[0].ParameterType;
                Type t2 = parameters[1].ParameterType;

                if (t1 != typeof(MomoTalk) || !typeof(EventBase).IsAssignableFrom(t2)) {
                    throw new MethodAccessException(
                        "only <TEvent>(MomoTalk, TEvent) is accepted as parameters for any socket event handlers"
                    );
                }

                Delegate @delegate;

                if (method.IsStatic) {
                    @delegate = Delegate.CreateDelegate(typeof(ObAsyncEventHandler<>).MakeGenericType(t2), method);
                } else {
                    object methodOwnerInstance = serviceProvider.GetService(method.DeclaringType
                        ?? throw new MethodAccessException(
                            "IL based method is not supported"
                        )
                    ) ?? throw new TypeAccessException(
                        "class instance declared non-static method may not be injected to host"
                    );

                    @delegate = Delegate.CreateDelegate(
                        typeof(ObAsyncEventHandler<>).MakeGenericType(t2), methodOwnerInstance, method
                    );
                }

                EventInfo eventInfo = typeof(OneBotEventHandlerRepository).GetEvent(t2.Name)
                    ?? throw new TypeAccessException("unrecognized event type");

                MethodInfo eventAddMethod
                    = eventInfo.AddMethod ?? throw new MethodAccessException("unable to add event handler");

                eventAddMethod.Invoke(handler, [@delegate]);
            }
        }
    }
}
