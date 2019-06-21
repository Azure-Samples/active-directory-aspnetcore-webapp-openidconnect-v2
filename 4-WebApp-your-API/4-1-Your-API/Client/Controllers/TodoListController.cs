using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Graph;
using Microsoft.Identity.Web.Client;
using TodoListService.Models;
using TodoListClient.Utils;

namespace TodoListClient.Controllers
{
    public class TodoListController : Controller
    {
        ITokenAcquisition _tokenAcquisition;
        IList<Todo> Model = new List<Todo>();

        public TodoListController(ITokenAcquisition tokenAcquisition)
        {
            _tokenAcquisition = tokenAcquisition;


        }

        // GET: TodoList
        public ActionResult Index()
        {
            if (HttpContext.Session.Get<IList<Todo>>("ToDoList") == null)
            {
                Model.Add(new Todo() { Id = 1, Owner = "kkrishna@microsoft.com", Title = "do something" });
                Model.Add(new Todo() { Id = 2, Owner = "jmprieur@microsoft.com", Title = "do something else" });

                HttpContext.Session.Set<IList<Todo>>("ToDoList", Model);
            }

            Model = HttpContext.Session.Get<IList<Todo>>("ToDoList");

            return View(Model);
        }

        // GET: TodoList/Details/5
        public ActionResult Details(int id)
        {
            Model = HttpContext.Session.Get<IList<Todo>>("ToDoList");

            return View(Model.FirstOrDefault(x => x.Id == id));
        }

        // GET: TodoList/Create
        public ActionResult Create()
        {
            Todo todo = new Todo() { Owner = HttpContext.User.Identity.Name };
            return View(todo);
        }

        // POST: TodoList/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind("Title,Owner")] Todo todo)
        {
            try
            {
                Model = HttpContext.Session.Get<IList<Todo>>("ToDoList");

                int id = Model.OrderByDescending(x => x.Id).FirstOrDefault().Id + 1;

                Model.Add(new Todo() { Id = id, Owner = HttpContext.User.Identity.Name, Title = todo.Title });
                HttpContext.Session.Set<IList<Todo>>("ToDoList", Model);

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: TodoList/Edit/5
        public ActionResult Edit(int id)
        {
            Model = HttpContext.Session.Get<IList<Todo>>("ToDoList");

            Todo todo = Model.FirstOrDefault(x => x.Id == id);

            if (todo == null)
            {
                return NotFound();
            }

            return View(todo);
        }

        // POST: TodoList/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, [Bind("Id,Title,Owner")] Todo todo)
        {
            try
            {
                if (id != todo.Id)
                {
                    return NotFound();
                }

                Model = HttpContext.Session.Get<IList<Todo>>("ToDoList");

                if (Model.FirstOrDefault(x => x.Id == id) == null)
                {
                    return NotFound();
                }

                Model.Remove(Model.FirstOrDefault(x => x.Id == id));
                Model.Add(todo);

                HttpContext.Session.Set<IList<Todo>>("ToDoList", Model);
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: TodoList/Delete/5
        public ActionResult Delete(int id)
        {
            Model = HttpContext.Session.Get<IList<Todo>>("ToDoList");

            Todo todo = Model.FirstOrDefault(x => x.Id == id);

            if (todo == null)
            {
                return NotFound();
            }

            return View(todo);
        }

        // POST: TodoList/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, [Bind("Id,Title,Owner")] Todo todo)
        {
            try
            {
                if (id != todo.Id)
                {
                    return NotFound();
                }

                Model = HttpContext.Session.Get<IList<Todo>>("ToDoList");

                if (Model.FirstOrDefault(x => x.Id == id) == null)
                {
                    return NotFound();
                }

                Model.Remove(Model.FirstOrDefault(x => x.Id == id));
                HttpContext.Session.Set<IList<Todo>>("ToDoList", Model);

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}