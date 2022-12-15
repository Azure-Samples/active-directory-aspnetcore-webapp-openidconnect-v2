// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace ToDoListService.Models
{
    public class TodoItem
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string AssignedTo { get; set; }
        public string Assignedby { get; set; }
        public string TenantId { get; set; }
    }
}