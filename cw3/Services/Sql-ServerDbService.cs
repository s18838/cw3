using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using cw3.DTOs;
using cw3.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic.CompilerServices;

namespace cw3.DAL
{
    public class StudentDbService : IStudentDbService
    {
        
        private const string ConStr = "Data Source=db-mssql;Initial Catalog=2019SBD;Integrated Security=True";
        public IEnumerable<Student> GetStudents()
        {
            var students = new List<Student>();
            using(var con = new SqlConnection())
            {
                using var com = new SqlCommand()
                {
                    Connection = con,
                    CommandText = "select * from Student"
                };
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
            using(var con = new SqlConnection())
            {
                using var com = new SqlCommand()
                {
                    Connection = con,
                    CommandText = $"SELECT * FROM Enrollment e JOIN Student s on e.IdEnrollment = s.IdEnrollment where s.IndexNumber = @id"
                };
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

        public Enrollment EnrollStudent(EnrollStudentRequest request)
        {

            if (request.Studies == null 
                || request.BirthDate == null 
                || request.FirstName == null
                || request.IndexNumber == null
                || request.LastName == null)
            {
                throw new Exception();
            }

            using var con = new SqlConnection(ConStr);
            using var com = new SqlCommand()
            {
                Connection = con,
                CommandText = $"INSERT INTO Enrollment SELECT NULLIF(MAX(E.IdEnrollment) + 1, 0), @Semester, @IdStudy, @StartDate"
            };
            con.Open();

                
            var study = getStudy(com, request.Studies);
                
            if (study == null)
            {
                throw new Exception();
            }
                
            var enrollment = getLastEnrollmentForStudy(com, study.IdStudy);
                
            var transaction = con.BeginTransaction();
                
            if (enrollment == null)
            {
                enrollment = new Enrollment()
                {
                    Semester = 1,
                    IdStudy = study.IdStudy,
                    StartDate = DateTime.Now.ToString("MM.dd.yyyy")
                };
                saveEnrollment(com, enrollment);
            }
                
            if (checkIfExists(com, request.IndexNumber))
            {
                transaction.Rollback();
                throw new Exception();
            }
                
            var student = new Student()
            {
                IndexNumber = request.IndexNumber,
                FirstName = request.FirstName,
                LastName = request.LastName,
                BirthDate = request.BirthDate,
                IdEnrollment = IntegerType.FromObject(enrollment.IdEnrollment)
            };
                
            saveStudent(com, student);
            transaction.Commit();

            return enrollment;
        }

        public Enrollment PromoteStudents(PromoteStudentsRequest request)
        {
            using var con = new SqlConnection(ConStr);
            using var com = new SqlCommand()
            {
                Connection = con
            };
            con.Open();

            var enrollment = getEnrollment(com, request.Studies, request.Semester);

            if (enrollment == null)
            {
                throw new Exception();
            }

            com.CommandText = $"PromoteStudents";
            com.CommandType = CommandType.StoredProcedure;

            com.Parameters.AddWithValue("Studies", request.Studies);
            com.Parameters.AddWithValue("Semester", request.Semester);

            com.ExecuteNonQuery();

            enrollment.Semester++;
            
            return enrollment;
        }

        private Study getStudy(SqlCommand com, string name)
        {
            com.CommandText = $"SELECT * FROM Studies WHERE Name = @name";
            com.Parameters.AddWithValue("name", name);
            var rd = com.ExecuteReader();
            while (rd.Read())
            {
                return new Study()
                {
                    IdStudy = IntegerType.FromObject(rd["IdStudy"]),
                    Name = rd["Name"].ToString()
                };
            }
            return null;
        }
        
        private Enrollment getEnrollment(SqlCommand com, string name, int semester)
        {
            com.CommandText = $"SELECT * FROM Enrollment JOIN Studies ON Enrollment.IdStudy = Studies.IdStudy WHERE Semester = @semester AND Name = @name)";
            com.Parameters.AddWithValue("name", name);
            com.Parameters.AddWithValue("semester", semester);
            var rd = com.ExecuteReader();
            while (rd.Read())
            {
                return new Enrollment()
                {
                    IdEnrollment = IntegerType.FromObject(rd["IdEnrollment"]),
                    Semester = IntegerType.FromObject(rd["Semester"]),
                    IdStudy = IntegerType.FromObject(rd["IdStudy"]),
                    StartDate = rd["StartDate"].ToString()
                };
            }

            return null;
        }
        
        private Enrollment getLastEnrollmentForStudy(SqlCommand com, int id)
        {
            com.CommandText = $"SELECT * FROM Enrollment WHERE IdStudy = @id AND Semester = 1";
            com.Parameters.AddWithValue("id", id);
            
            var rd = com.ExecuteReader();
            while (rd.Read())
            {
                return new Enrollment()
                {
                    IdEnrollment = IntegerType.FromObject(rd["IdEnrollment"]),
                    Semester = IntegerType.FromObject(rd["Semester"]),
                    IdStudy = IntegerType.FromObject(rd["IdStudy"]),
                    StartDate = rd["StartDate"].ToString()
                };
            }
            return null;
        }
        
        private void saveStudent(SqlCommand com, Student student)
        {
            com.CommandText =
                $"INSERT INTO Student VALUES (@IndexNumber, @FirstName, @LastName, @BirthDate, @IdEnrollment)";
            com.Parameters.AddWithValue("IndexNumber", student.IndexNumber);
            com.Parameters.AddWithValue("FirstName", student.FirstName);
            com.Parameters.AddWithValue("LastName", student.LastName);
            com.Parameters.AddWithValue("BirthDate", student.BirthDate);
            com.Parameters.AddWithValue("IdEnrollment", student.IdEnrollment);
            com.ExecuteNonQuery();
        }

        private bool checkIfExists(SqlCommand com, string indexNumber)
        {
            com.CommandText = $"SELECT * FROM Student WHERE IndexNumber=@indexNumber";
            com.Parameters.AddWithValue("indexNumber", indexNumber);
            var rd = com.ExecuteReader();
            while (rd.Read())
            {
                return true;
            }
            return false;
        }
        
        private void saveEnrollment(SqlCommand com, Enrollment enrollment)
        {
            com.CommandText =
                $"INSERT INTO Enrollment(Semester, IdStudy, StartDate) VALUES (@Semester, @IdStudy, @StartDate)";
            com.Parameters.AddWithValue("Semester", enrollment.Semester);
            com.Parameters.AddWithValue("IdStudy", enrollment.IdStudy);
            com.Parameters.AddWithValue("StartDate", enrollment.StartDate);
            com.ExecuteNonQuery();
        }
    }
}