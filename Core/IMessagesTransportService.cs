namespace Core
{
    using System.Threading.Tasks;
    using Core.Model;

    public interface IMessagesTransportService
    {
        Task SendMessage(Message message);

        Task Subscribe(string groupId);

        Task Unsubscribe(string groupId);
    }
}
