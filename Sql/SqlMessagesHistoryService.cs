namespace Sql
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;
    using Core;
    using Core.Model;
    using Dapper;
    using MySql.Data.MySqlClient;

    internal sealed class SqlMessagesHistoryService : IMessagesHistoryService
    {
        static SqlMessagesHistoryService()
        {
            SqlMapper.AddTypeHandler(new DateTimeOffsetHandler());
        }

        private readonly SqlMessagesHistoryServiceConfiguration configuration;

        public SqlMessagesHistoryService(SqlMessagesHistoryServiceConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public async Task<Message> CreateMessage(Message message)
        {
            var command =
                "SET @now = Now(); " +
                "INSERT INTO MessagesHistory(AuthorId, GroupId, Text, SendTime) " +
                "VALUES(@AuthorId, @GroupId, @Text, @now); " +
                "SELECT @now";

            await using var connection = this.CreateConnection();
            var sendTime = await connection.ExecuteScalarAsync<DateTimeOffset>(command, message);

            return message with { SendTime = sendTime };
        }

        public async Task<IReadOnlyCollection<Message>> GetHistory(string groupId)
        {
            var command = "SELECT * FROM MessagesHistory " +
                          "WHERE groupId=@groupId " +
                          "ORDER BY SendTime, ID";

            await using var connection = this.CreateConnection();
            var result = await connection.QueryAsync<Message>(command, new { groupId });

            return result.ToList();
        }

        internal async Task DeleteMessage(Message message)
        {
            var command =
                "DELETE FROM MessagesHistory " +
                "WHERE AuthorId = @AuthorId " +
                "AND GroupId = @GroupId " +
                "AND Text = @Text";

            await using var connection = this.CreateConnection();
            await connection.ExecuteAsync(command, message);
        }

        private MySqlConnection CreateConnection()
        {
            return new MySqlConnection(this.configuration.ConnectionString);
        }

        private class DateTimeOffsetHandler : SqlMapper.TypeHandler<DateTimeOffset>
        {
            public override void SetValue(IDbDataParameter parameter, DateTimeOffset value)
            {
                parameter.Value = value.ToUniversalTime();
            }

            public override DateTimeOffset Parse(object value)
                => DateTimeOffset.Parse(
                    value.ToString(), 
                    null,
                    DateTimeStyles.AssumeUniversal);
        }
    }
}
