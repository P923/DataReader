namespace GUI.Utils
{
    public class Observable
    {
        private readonly List<EventSubscriber> subscribers;

        public Observable() { subscribers = new List<EventSubscriber>(); }
        public void AddSubscriber(EventSubscriber s)
        {
            subscribers.Add(s);
        }

        public void SendToSubscriber(int code, object obj)
        {
            foreach (EventSubscriber? subscriber in subscribers)
            {
                subscriber.Result(code, obj);
            }
        }
    }
}
