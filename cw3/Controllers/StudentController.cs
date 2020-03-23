using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using cw3.DAL;
using cw3.Models;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace cw3.Controllers
{
    [ApiController]
    [Route("api/students")]
    public class StudentController : ControllerBase
    {

        private readonly IDbService _dbService;

        public StudentController(IDbService dbService)
        {
            _dbService = dbService;
        }

        [HttpGet]
        public IActionResult GetStudents()
        {

            List<Student> list = new List<Student>();

            using (var con = new SqlConnection("Data Source=db-mssql;Initial Catalog=2019SBD;Integrated Security=True"))
                using(var com = new SqlCommand())
            {
                com.Connection = con;
                com.CommandText = "SELECT * FROM Student";

                con.Open();

                var dr = com.ExecuteReader();

               

                while (dr.Read()) {
                    var student = new Student
                    {
                        FirstName = dr["FirstName"].ToString(),
                        LastName = dr["LastName"].ToString()
                    };

                    list.Add(student);
                }
            }

            return Ok(list);
        }

        [HttpGet("{id}/enrollments")]
        public IActionResult GetEnrollments(int id)
        {

            List<Student> list = new List<Student>(); // change to Enrollment

            using (var con = new SqlConnection("Data Source=db-mssql;Initial Catalog=2019SBD;Integrated Security=True"))
            using (var com = new SqlCommand())
            {
                com.Connection = con;
                com.CommandText = "SELECT FROM Enrollment WHERE IdEnrollment IN (Select IdEnrollment from Student where IndexNumber = @id)";
                com.Parameters.AddWithValue("id", id);

                con.Open();

                var dr = com.ExecuteReader();

                while (dr.Read())
                {
                    // Parse to Enrollment
                }
            }

            return Ok(list);
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
