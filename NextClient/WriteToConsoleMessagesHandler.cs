namespace NextClient
{
    using System;
    using Core.Client;
    using Core.Model;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    internal sealed class WriteToConsoleMessagesHandler : INextChatClient
    {
        public Task OnHistory(IReadOnlyCollection<Message> history)
        {
            foreach (var message in history)
            {
                Console.WriteLine($"{message.GroupId} [{message.SendTime.ToLocalTime():g}]: {message.AuthorId}: {message.Text}");
            }

            return Task.CompletedTask;
        }

        public Task OnMessage(Message message)
        {
            Console.WriteLine($"{message.GroupId} [{message.SendTime.ToLocalTime():g}]: {message.AuthorId}: {message.Text}");

            return Task.CompletedTask;
        }
    }
}
