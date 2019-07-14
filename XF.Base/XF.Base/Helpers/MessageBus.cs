using System;
using Xamarin.Forms;

namespace XF.Base.Helpers
{
    public class MessageBus
    {
        static readonly Lazy<MessageBus> LazyInstance = new Lazy<MessageBus>(() => new MessageBus(), true);
        static MessageBus Instance => LazyInstance.Value;

        MessageBus()
        {
        }

        public static void SendMessage(object message)
        {
            MessagingCenter.Send(Instance, message.ToString());
        }

        public static void SendMessage<TArgs>(object message, TArgs args)
        {
            MessagingCenter.Send(Instance, message.ToString(), args);
        }

        public static void Subscribe(object subscriber, object message, Action callback)
        {
            MessagingCenter.Subscribe<MessageBus>(subscriber, message.ToString(), (bus) => {
                callback?.Invoke();
            });
        }

        public static void Subscribe<TArgs>(object subscriber, object message, Action<TArgs> callback)
        {
            MessagingCenter.Subscribe<MessageBus, TArgs>(subscriber, message.ToString(), (bus, args) => {
                callback?.Invoke(args);
            });
        }

        public static void SubscribeForOnePush(object subscriber, object message, Action callback)
        {
            MessagingCenter.Subscribe<MessageBus>(subscriber, message.ToString(), (bus) => {
                Unsubscribe(subscriber, message);
                callback?.Invoke();
            });
        }

        public static void SubscribeForOnePush<TArgs>(object subscriber, object message, Action<TArgs> callback)
        {
            MessagingCenter.Subscribe<MessageBus, TArgs>(subscriber, message.ToString(), (bus, args) => {
                Unsubscribe<TArgs>(subscriber, message);
                callback?.Invoke(args);
            });
        }

        public static void Unsubscribe(object subscriber, object message)
        {
            MessagingCenter.Unsubscribe<MessageBus>(subscriber, message.ToString());
        }

        public static void Unsubscribe<TArgs>(object subscriber, object message)
        {
            MessagingCenter.Unsubscribe<MessageBus, TArgs>(subscriber, message.ToString());
        }
    }
}
