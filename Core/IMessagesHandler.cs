namespace Core
{
    using Core.Model;


    public interface IMessagesHandler
    {
        void OnMessage(Message message);
    }
}
