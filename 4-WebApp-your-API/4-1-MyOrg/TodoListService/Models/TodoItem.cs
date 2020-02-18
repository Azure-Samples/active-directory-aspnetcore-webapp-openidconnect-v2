﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TodoListService.Models
{
    public class Todo
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string Owner { get; set; }
    }
}
