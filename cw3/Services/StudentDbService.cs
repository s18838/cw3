using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using cw3.DTOs;
using cw3.Models;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.VisualBasic.CompilerServices;

namespace cw3.DAL
{
    public class StudentDbService : IStudentDbService
    {
        
        private const string ConStr = "Data Source=db-mssql;Initial Catalog=s18838;Integrated Security=True";

        public IEnumerable<Student> GetStudents()
        {
            var students = new List<Student>();
            using (var con = new SqlConnection(ConStr))
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
                        BirthDate = DateType.FromString(rd["BirthDate"].ToString()),
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
                        StartDate = DateType.FromString(rd["StartDate"].ToString())
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

            Enrollment enrollment;

            using (var con = new SqlConnection(ConStr))
            {
                con.Open();


                var study = getStudy(con, request.Studies);

                if (study == null)
                {
                    throw new Exception();
                }

                enrollment = getLastEnrollmentForStudy(con, study.IdStudy);

                var transaction = con.BeginTransaction();

                if (enrollment == null)
                {
                    enrollment = new Enrollment()
                    {
                        Semester = 1,
                        IdStudy = study.IdStudy,
                        StartDate = DateType.FromString(DateTime.Now.ToString("MM.dd.yyyy"))
                    };
                    saveEnrollment(con, enrollment, transaction);
                }

                if (checkIfExists(con, request.IndexNumber, transaction))
                {
                    transaction.Rollback();
                    throw new Exception();
                }

                var student = new Student()
                {
                    IndexNumber = request.IndexNumber,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    BirthDate = DateType.FromString(request.BirthDate),
                    IdEnrollment = IntegerType.FromObject(enrollment.IdEnrollment)
                };

                saveStudent(con, student, transaction);
                transaction.Commit();

            }

            return enrollment;
        }

        public Enrollment PromoteStudents(PromoteStudentsRequest request)
        {
            using var con = new SqlConnection(ConStr);
            con.Open();

            var enrollment = getEnrollment(con, request.Studies, request.Semester);

            if (enrollment == null)
            {
                throw new Exception();
            }

            using var com = new SqlCommand()
            {
                Connection = con
            };

            com.CommandText = $"PromoteStudents";
            com.CommandType = CommandType.StoredProcedure;

            com.Parameters.AddWithValue("Studies", request.Studies);
            com.Parameters.AddWithValue("Semester", request.Semester);

            com.ExecuteNonQuery();

            enrollment.Semester++;

            return enrollment;
        }

        private Studies getStudy(SqlConnection con, string name)
        {
            using var com = new SqlCommand()
            {
                Connection = con
            };
            com.CommandText = $"SELECT * FROM Studies WHERE Name = @name";
            com.Parameters.AddWithValue("name", name);
            var rd = com.ExecuteReader();
            while (rd.Read())
            {
                var study = new Studies()
                {
                    IdStudy = IntegerType.FromObject(rd["IdStudy"]),
                    Name = rd["Name"].ToString()
                };
                rd.Close();
                return study;
            }
            return null;
        }

        private Enrollment getEnrollment(SqlConnection con, string name, int semester)
        {
            using var com = new SqlCommand()
            {
                Connection = con
            };
            com.CommandText = $"SELECT * FROM Enrollment JOIN Studies ON Enrollment.IdStudy = Studies.IdStudy WHERE Semester = @semester AND Name = @name";
            com.Parameters.AddWithValue("name", name);
            com.Parameters.AddWithValue("semester", semester);
            var rd = com.ExecuteReader();
            while (rd.Read())
            {
                var enrollment = new Enrollment()
                {
                    IdEnrollment = IntegerType.FromObject(rd["IdEnrollment"]),
                    Semester = IntegerType.FromObject(rd["Semester"]),
                    IdStudy = IntegerType.FromObject(rd["IdStudy"]),
                    StartDate = DateType.FromString(rd["StartDate"].ToString())
                };
                rd.Close();
                return enrollment;
            }

            return null;
        }

        private Enrollment getLastEnrollmentForStudy(SqlConnection con, int id)
        {
            using var com = new SqlCommand()
            {
                Connection = con
            };
            com.CommandText = $"SELECT * FROM Enrollment WHERE IdStudy = @id AND Semester = 1";
            com.Parameters.AddWithValue("id", id);

            var rd = com.ExecuteReader();
            while (rd.Read())
            {
                var enrollment = new Enrollment()
                {
                    IdEnrollment = IntegerType.FromObject(rd["IdEnrollment"]),
                    Semester = IntegerType.FromObject(rd["Semester"]),
                    IdStudy = IntegerType.FromObject(rd["IdStudy"]),
                    StartDate = DateType.FromString(rd["StartDate"].ToString())
                };
                rd.Close();
                return enrollment;
            }
            return null;
        }

        private void saveStudent(SqlConnection con, Student student, SqlTransaction transaction)
        {
            using var com = new SqlCommand()
            {
                Transaction = transaction,
                Connection = con
            };
            com.CommandText =
                $"INSERT INTO Student VALUES (@IndexNumber, @FirstName, @LastName, @BirthDate, @IdEnrollment)";
            com.Parameters.AddWithValue("IndexNumber", student.IndexNumber);
            com.Parameters.AddWithValue("FirstName", student.FirstName);
            com.Parameters.AddWithValue("LastName", student.LastName);
            com.Parameters.AddWithValue("BirthDate", student.BirthDate);
            com.Parameters.AddWithValue("IdEnrollment", student.IdEnrollment);
            com.ExecuteNonQuery();
        }

        private bool checkIfExists(SqlConnection con, string indexNumber, SqlTransaction transaction)
        {
            using var com = new SqlCommand()
            {
                Connection = con,
                Transaction = transaction
            };
            com.CommandText = $"SELECT * FROM Student WHERE IndexNumber=@indexNumber";
            com.Parameters.AddWithValue("indexNumber", indexNumber);
            var rd = com.ExecuteReader();
            while (rd.Read())
            {
                rd.Close();
                return true;
            }
            rd.Close();
            return false;
        }

        public bool checkIfExists(string indexNumber)
        {
            using (var con = new SqlConnection(ConStr))
            {
                using (var com = new SqlCommand())
                {
                    con.Open();
                    com.Connection = con;
                    com.CommandText = $"SELECT * FROM Student WHERE IndexNumber=@indexNumber";
                    com.Parameters.AddWithValue("indexNumber", indexNumber);
                    var rd = com.ExecuteReader();
                    while (rd.Read())
                    {
                        rd.Close();
                        return true;
                    }
                    rd.Close();
                }
            }
            return false;
        }

        private void saveEnrollment(SqlConnection con, Enrollment enrollment, SqlTransaction transaction)
        {
            using var com = new SqlCommand()
            {
                Transaction = transaction,
                Connection = con,
                CommandText = $"INSERT INTO Enrollment SELECT NULLIF(MAX(E.IdEnrollment) + 1, 0), @Semester, @IdStudy, @StartDate"
            };
            com.Parameters.AddWithValue("Semester", enrollment.Semester);
            com.Parameters.AddWithValue("IdStudy", enrollment.IdStudy);
            com.Parameters.AddWithValue("StartDate", enrollment.StartDate);
            com.ExecuteNonQuery();
        }

        public bool LogIn(LoginCredentials loginCredentials)
        {
            var salt = GetSalt(loginCredentials.Login);
            var valueBytes = KeyDerivation.Pbkdf2(
                                                loginCredentials.Password,
                                                Encoding.UTF8.GetBytes(salt),
                                                KeyDerivationPrf.HMACSHA512,
                                                1000,
                                                256 / 8);

            var password = Convert.ToBase64String(valueBytes);
            using (var con = new SqlConnection(ConStr))
            {
                using (var com = new SqlCommand())
                {
                    con.Open();
                    com.Connection = con;
                    com.CommandText = $"SELECT * FROM Student WHERE Password=@password AND IndexNumber=@indexNumber";
                    com.Parameters.AddWithValue("password", password);
                    com.Parameters.AddWithValue("indexNumber", loginCredentials.Login);
                    var rd = com.ExecuteReader();
                    while (rd.Read())
                    {
                        rd.Close();
                        return true;
                    }
                    rd.Close();
                }
            }
            return false;
        }

        public string GetSalt(string indexNumber)
        {
            using (var con = new SqlConnection(ConStr))
            {
                using (var com = new SqlCommand())
                {
                    con.Open();
                    com.Connection = con;
                    com.CommandText = $"SELECT Salt FROM Student WHERE IndexNumber=@indexNumber";
                    com.Parameters.AddWithValue("indexNumber", indexNumber);
                    var rd = com.ExecuteReader();
                    while (rd.Read())
                    {
                        var salt = rd["Salt"].ToString();
                        rd.Close();
                        return salt;
                    }
                    rd.Close();
                }
            }
            return null;
        }

        public void SaveRefreshToken(string refreshToken, string indexNumber)
        {
            using (var con = new SqlConnection(ConStr))
            {
                using (var com = new SqlCommand())
                {
                    con.Open();
                    com.Connection = con;
                    com.CommandText = $"INSERT INTO Token Values(@refreshToken, @indexNumber)";
                    com.Parameters.AddWithValue("refreshToken", refreshToken);
                    com.Parameters.AddWithValue("indexNumber", indexNumber);
                    com.ExecuteNonQuery();
                }
            }
        }

        public string CheckRefreshToken(RefreshTokenDTO refreshTokenDto)
        {
            using (var con = new SqlConnection(ConStr))
            {
                using (var com = new SqlCommand())
                {
                    con.Open();
                    com.Connection = con;
                    com.CommandText = $"SELECT IndexNumber FROM Token WHERE RefreshToken=@refreshToken";
                    com.Parameters.AddWithValue("refreshToken", refreshTokenDto.RefreshToken);
                    var rd = com.ExecuteReader();
                    while (rd.Read())
                    {
                        var indexNumber = rd["IndexNumber"].ToString();
                        rd.Close();
                        return indexNumber;
                    }
                    rd.Close();
                }
            }
            return null;
        }

        public void DeleteRefreshToken(RefreshTokenDTO refreshTokenDto)
        {
            using (var con = new SqlConnection(ConStr))
            {
                using (var com = new SqlCommand())
                {
                    con.Open();
                    com.Connection = con;
                    com.CommandText = $"DELETE FROM Token WHERE RefreshToken=@refreshToken";
                    com.Parameters.AddWithValue("refreshToken", refreshTokenDto.RefreshToken);
                    com.ExecuteNonQuery();
                }
            }
        }
    }
}