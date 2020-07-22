// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace TodoListAPI.Models
{
    public class TodoItem
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string Owner { get; set; }
        public string TenantId { get; set; }
    }
}