﻿namespace Core
{
    using System.Threading.Tasks;
    using Core.Model;


    public interface IMessagesHandler
    {
        Task OnMessage(Message message);
    }
}
