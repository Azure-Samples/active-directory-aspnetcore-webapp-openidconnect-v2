namespace ToDoListClient.Models
{
    public class ToDoItem
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string AssignedTo { get; set; }

        public string AssignedBy { get; set; }
        
        public string TenantId { get; set; }
    }
}
