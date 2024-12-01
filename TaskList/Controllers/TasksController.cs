using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TaskList.Models;

namespace TaskList.Controllers
{
    public class TasksController : Controller
    {
        private readonly Context _context;

        public TasksController(Context context)
        {
            _context = context;
        }

        //Método GET Index
        public async Task<IActionResult> Index()
        {
            //Variável para ordenar dados de acordo com Ordem de Apresentação 
            var tasksOrdered = _context.Tasks.OrderBy(t => t.PresentationOrder).ToList();

            //Retorno da View ordenada de acordo com Ordem de Apresentação
            return View(tasksOrdered);
        }

        //Método GET Create
        public IActionResult Create()
        {
            return View();
        }

        //Metódo para reordenar com base na ordem de apresentação
        public async Task<IActionResult> ChangeOrder(int id, string direction)
        {
            var currentTask = await _context.Tasks.FindAsync(id);
            if (currentTask == null)
            {
                return NotFound();
            }

            Tasks adjacentTask = null;

            if (direction == "up")
            {
                adjacentTask = await _context.Tasks
                    .Where(t => t.PresentationOrder < currentTask.PresentationOrder)
                    .OrderByDescending(t => t.PresentationOrder)
                    .FirstOrDefaultAsync();
            }
            else if (direction == "down")
            {
                adjacentTask = await _context.Tasks
                    .Where(t => t.PresentationOrder > currentTask.PresentationOrder)
                    .OrderBy(t => t.PresentationOrder)
                    .FirstOrDefaultAsync();
            }

            if (adjacentTask == null)
            {
                return BadRequest("Movimento inválido.");
            }

            // Troca as ordens
            int tempOrder = currentTask.PresentationOrder;
            currentTask.PresentationOrder = adjacentTask.PresentationOrder;
            adjacentTask.PresentationOrder = tempOrder;

            _context.Tasks.Update(currentTask);
            _context.Tasks.Update(adjacentTask);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }


        //Método POST Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Cost,Deadline")] Tasks task)
        {
            if (ModelState.IsValid)
            {
                //Variável para conferir existência do nome 
                var TaskExisting = _context.Tasks.FirstOrDefault(t => t.Name == task.Name);

                if (TaskExisting != null)
                {
                    //Adicionando erro ao modelo indicado
                    ModelState.AddModelError("Name", "Tarefa com nome já existente.");
                    return View(task);
                }

                //Preencher automaticamente o campo Ordem de Apresentação 
                task.PresentationOrder = _context.Tasks.Any() ? _context.Tasks.Max(t => t.PresentationOrder) + 1 : 1;

                _context.Add(task);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(task);
        }

        //Método GET Edit
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var task = await _context.Tasks.FindAsync(id);
            if (task == null)
            {
                return NotFound();
            }
            return View(task);
        }

        //Método POST Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Cost,Deadline")] Tasks task)
        {
            if (id != task.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                // Verificar existência de tarefa com o mesmo nome (exceto a atual)
                var taskExisting = _context.Tasks.FirstOrDefault(t => t.Name == task.Name && t.Id != id);

                if (taskExisting != null)
                {
                    // Adicionando erro ao modelo
                    ModelState.AddModelError("Name", "Tarefa com nome já existente.");
                    return View(task);
                }

                try
                {
                    // Buscar o valor atual de PresentationOrder
                    var existingTask = await _context.Tasks.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id);
                    if (existingTask == null)
                    {
                        return NotFound();
                    }

                    // Preservar PresentationOrder
                    task.PresentationOrder = existingTask.PresentationOrder;

                    _context.Update(task);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TaskExists(task.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                return RedirectToAction(nameof(Index));
            }
            return View(task);
        }


        //Método GET Delete
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var task = await _context.Tasks
                .FirstOrDefaultAsync(m => m.Id == id);
            if (task == null)
            {
                return NotFound();
            }

            return View(task);
        }

        //Método POST Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task != null)
            {
                _context.Tasks.Remove(task);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TaskExists(int id)
        {
            return _context.Tasks.Any(e => e.Id == id);
        }
    }
}
