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

        [HttpGet("{indexNumber}/enrollments")]
        public IActionResult GetEnrollments(string indexNumber)
        {
            var res = _studentDbService.GetEnrollments(indexNumber);

            if (res != null)
                return Ok(res);

            return NotFound();
        }

        [HttpPut("{indexNumber}")]
        public IActionResult ChangeStudent(string indexNumber, Student student)
        {
            _studentDbService.modifyStudent(indexNumber, student);
            return Ok("Aktualizacja dokonana");
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteStudent(string id)
        {
            _studentDbService.removeStudent(id);
            return Ok("Usuwanie ukończone");
        }
    }
}
