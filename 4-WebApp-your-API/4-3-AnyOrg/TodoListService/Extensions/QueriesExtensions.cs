using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ToDoListService.Models;

namespace ToDoListService.Extensions;
public static class QueriesExtensions
{
    public static async Task<IEnumerable<TodoItem>> WhereAsync(this DbSet<TodoItem> dbSet, Expression<Func<TodoItem,bool>> predicate)
    {
        return await dbSet.Where(predicate).ToArrayAsync();
    }
}
