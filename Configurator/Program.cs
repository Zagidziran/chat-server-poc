using System;
using System.IO;
using System.Threading.Tasks;
using Configurator;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using Polly;

var configurationBuilder = new ConfigurationBuilder()
    .AddYamlFile("configuration.yaml")
    .AddEnvironmentVariables();

var configuration = configurationBuilder
    .Build()
    .Get<Configuration>();

var retryPolicy = Policy
    .Handle<Exception>()
    .WaitAndRetryAsync(configuration.Attempts, _ => configuration.AttemptDelay);

await retryPolicy.ExecuteAsync(ConfigureAction);

async Task ConfigureAction()
{
    var connectionStringBuilder = new MySqlConnectionStringBuilder(configuration.Sql.ConnectionString);
    var database = connectionStringBuilder.Database;
    connectionStringBuilder.Database = null;
    var connectionStringWithoutDatabase = connectionStringBuilder.ToString();
    await using var connectionToServer = new MySqlConnection(connectionStringWithoutDatabase);
    connectionToServer.Open();
    await using var createDatabaseCommand = new MySqlCommand(
        $"CREATE DATABASE IF NOT EXISTS `{database}`",
        connectionToServer);
    await createDatabaseCommand.ExecuteNonQueryAsync();

    await using var connection = new MySqlConnection(configuration.Sql.ConnectionString);
    connection.Open();
    await using var command = new MySqlCommand(
        File.ReadAllText("initialize-script.sql"),
        connection);
    await command.ExecuteNonQueryAsync();
}