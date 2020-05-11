using System;
using cw3.DAL;
using cw3.Models;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace cw3.Controllers
{
    [ApiController]
    [Route("api/students")]
    public class StudentController : ControllerBase
    {

        private readonly IStudentDbService _studentDbService;

        public StudentController(IStudentDbService studentDbService)
        {
            _studentDbService = studentDbService;
        }

        [HttpGet]
        public IActionResult GetStudents()
        {
            return Ok(_studentDbService.GetStudents());
        }

        [HttpGet("{id}/enrollments")]
        public IActionResult GetEnrollments(string id)
        {
            var res = _studentDbService.GetEnrollments(id);

            if (res != null)
                return Ok(res);

            return NotFound();
        }

        [HttpPost]
        public IActionResult CreateStudent(Student student)
        {
            // creating in database
            student.IndexNumber = $"s{new Random().Next(1, 20000)}";
            return Ok(student);
        }

        [HttpPut("{id}")]
        public IActionResult ChangeStudent(int id, Student student)
        {
            // changing in database
            return Ok("Aktualizacja dokonana");
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteStudent(int id)
        {
            // deleting from database
            return Ok("Usuwanie ukończone");
        }
    }
}
