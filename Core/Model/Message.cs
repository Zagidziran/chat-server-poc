namespace Core.Model
{
    using System;

    public record Message()
    {
        public string AuthorId { get; init; }
        
        public string GroupId { get; init; }
        
        public DateTimeOffset SendTime { get; init; }
        
        public string Text { get; init; }
    }
}
