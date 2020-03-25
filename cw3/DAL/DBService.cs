using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using cw3.Models;
using Microsoft.VisualBasic.CompilerServices;

namespace cw3.DAL
{
    public class DBService : IDBService
    {
        private const string ConStr = "Data Source=db-mssql;Initial Catalog=2019SBD;Integrated Security=True";
        public IEnumerable<Student> GetStudents()
        {

            var students = new List<Student>();

            using (var con = new SqlConnection(ConStr))
            using (var com = new SqlCommand()
            {
                Connection = con,
                CommandText = "select * from Student"
            })
            {
                con.Open();

                var rd = com.ExecuteReader();

                while (rd.Read())
                {
                    students.Add(new Student
                    {
                        IndexNumber = rd["IndexNumber"].ToString(),
                        FirstName = rd["FirstName"].ToString(),
                        LastName = rd["LastName"].ToString(),
                        BirthDate = rd["BirthDate"].ToString(),
                        IdEnrollment = IntegerType.FromObject(rd["IdEnrollment"])
                    });
                }
            }

            return students;

        }

        public IEnumerable<Enrollment> GetEnrollments(string id)
        {

            var enrollments = new List<Enrollment>();

            using (var con = new SqlConnection(ConStr))
            using (var com = new SqlCommand()
            {
                Connection = con,
                CommandText = $"SELECT * FROM Enrollment e JOIN Student s on e.IdEnrollment = s.IdEnrollment where s.IndexNumber = @id"
            })
            {

                com.Parameters.AddWithValue("id", id);

                con.Open();

                var rd = com.ExecuteReader();

                while (rd.Read())
                {
                    enrollments.Add(new Enrollment()
                    {
                        IdEnrollment = IntegerType.FromObject(rd["IdEnrollment"]),
                        Semester = IntegerType.FromObject(rd["Semester"]),
                        IdStudy = IntegerType.FromObject(rd["IdStudy"]),
                        StartDate = rd["StartDate"].ToString()
                    });
                }

            }

            return enrollments;
        }
    }
}