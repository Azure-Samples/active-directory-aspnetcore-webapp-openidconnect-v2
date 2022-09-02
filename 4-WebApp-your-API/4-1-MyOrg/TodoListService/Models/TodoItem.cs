namespace TodoListService.Models
{
    /// <summary>
    /// Data object to transfer information between client/server/database
    /// </summary>
    public class Todo
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string Owner { get; set; }
    }
}
