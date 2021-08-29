namespace Core
{
    using Core.Model;
    using System;
    using System.Threading.Tasks;

    public interface IGroupParticipation : IDisposable
    {
        Task SendMessage(string text);

        Task<Message> GetMessages();
    }
}
