namespace Core.Model
{
    using System;

    public record Message(string? Id, string AuthorId, string GroupId, DateTimeOffset SendTime, string Text);
}
