using System;
using System.Linq;
using CommandLine;
using Core;
using Core.Model;
using Core.Server;
using NextClient;
using NextClient.CommandLine;
using SignalR;

var connected = false;
INextChatServer server = null;

while (true)
{
    try
    {
        if (!connected)
        {
            var command = Parser.Default.ParseArguments<ConnectCommand, ByeCommand>(
                Console.ReadLine().Split(' '));

            switch (command)
            {
                case Parsed<object> {Value: ByeCommand}:
                    Environment.Exit(0);
                    break;

                case Parsed<object> {Value: ConnectCommand connect}:
                    server = await NextChatServerClientFactory.CreateClient(new NextChatConfiguration
                        {
                            Uri = connect.Uri,
                            UserId = connect.UserId
                        },
                        new WriteToConsoleMessagesHandler());

                    connected = true;
                    break;
            }
        }
        else
        {
            var command = Parser.Default
                .ParseArguments<CreateCommand, JoinCommand, LeaveCommand, ListCommand, SayCommand, ByeCommand>(
                    Console.ReadLine().Split(' '));

            switch (command)
            {
                case Parsed<object> {Value: ByeCommand}:
                    Environment.Exit(0);
                    break;

                case Parsed<object> {Value: CreateCommand create}:
                    await server.CreateGroup(create.GroupId);
                    break;

                case Parsed<object> {Value: JoinCommand join}:
                    var joinResult = await server.JoinGroup(join.GroupId);
                    if (joinResult == GroupJoinResult.NoRoom)
                    {
                        Console.WriteLine("No room :(");
                    }
                    break;

                case Parsed<object> {Value: LeaveCommand leave}:
                    await server.LeaveGroup(leave.GroupId);
                    break;

                case Parsed<object> { Value: ListCommand list }:
                    var groups = await server.ListGroups();
                    Console.WriteLine(string.Join(", ", groups.Select(g => g.GroupId)));
                    break;

                case Parsed<object> {Value: SayCommand say}:
                    await server.SendMessage(say.GroupId, string.Join(' ', say.Message));
                    break;
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.Message);
    }
}