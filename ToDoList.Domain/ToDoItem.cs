namespace ToDoList.Domain
{
    public class ToDoItem
    {
        public required Guid Id { get; set; }
        public required DateTime TimeOfExpiry { get; set; }
        public required string Title { get; set; }
        public string? Description { get; set; }
        public required int PercentCompleted { get; set; }
    }
}
