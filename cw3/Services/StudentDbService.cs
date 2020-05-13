using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using cw3.DTOs;
using cw3.Models;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic.CompilerServices;

namespace cw3.DAL
{
    public class StudentDbService : IStudentDbService
    {
        public IEnumerable<Student> GetStudents()
        {
            using (var db = new s18838Context()) {
                return db.Student.ToList();
            }
        }
        public void modifyStudent(string indexNumber, Student student)
        {
            using (var db = new s18838Context())
            {
                var s = db.Student.Where(s => s.IndexNumber == indexNumber).SingleOrDefault();
                s.IndexNumber = student.IndexNumber;
                s.FirstName = student.FirstName;
                s.BirthDate = student.BirthDate;
                s.IdEnrollment = student.IdEnrollment;
                s.Salt = student.Salt;
                s.Password = student.Password;
                db.SaveChanges();
            }
        }

        public void removeStudent(string indexNumber)
        {
            using (var db = new s18838Context())
            {
                var student = db.Student.Where(s => s.IndexNumber == indexNumber).SingleOrDefault();
                db.Student.Remove(student);
                db.SaveChanges();
            }
        }
        public IEnumerable<Enrollment> GetEnrollments(string indexNumber)
        {
            using (var db = new s18838Context())
            {
                return db.Student
                .Where(s => s.IndexNumber == indexNumber)
                .Select(e => e.IdEnrollmentNavigation);
            }
        }

        public Enrollment EnrollStudent(EnrollStudentRequest request)
        {
            using (var db = new s18838Context()) {
                if (request.Studies == null
                || request.BirthDate == null
                || request.FirstName == null
                || request.IndexNumber == null
                || request.LastName == null)
                {
                    throw new Exception();
                }
                using (var transaction = db.Database.BeginTransaction())
                {
                    var study = GetStudies(request.Studies);
                    if (study == null)
                    {
                        transaction.Rollback();
                        throw new Exception();
                    }
                    Enrollment enrollment = GetLastEnrollmentForStudy(study.IdStudy);
                    if (enrollment == null)
                    {
                        enrollment = new Enrollment()
                        {
                            Semester = 1,
                            IdStudy = study.IdStudy,
                            StartDate = DateType.FromString(DateTime.Now.ToString("MM.dd.yyyy"))
                        };

                        SaveEnrollment(db, enrollment);
                    }
                    if (CheckIfStudentExists(request.IndexNumber))
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
                    SaveStudent(db, student);
                    transaction.Commit();
                    return enrollment;
                }
            }
        }

        public Enrollment PromoteStudents(PromoteStudentsRequest request)
        {
            using (var db = new s18838Context())
            {
                var enrollment = GetEnrollment(request.Studies, request.Semester);
                if (enrollment == null)
                {
                    throw new Exception();
                }
                db.Database.ExecuteSqlRaw("exec PromoteStudents @Studies @Semester", request.Studies, request.Semester);
                enrollment.Semester++;
                return enrollment;
            }
        }

        private Studies GetStudies(string name)
        {
            using (var db = new s18838Context())
            {
                return db.Studies.Where(s => s.Name == name).SingleOrDefault();
            }
        }

        private Enrollment GetEnrollment(string name, int semester)
        {
            using (var db = new s18838Context())
            {
                return db.Enrollment
                .Where(e => e.Semester == semester)
                .Join(
                    db.Studies.Where(s => s.Name == name),
                    e => e.IdStudy, s => s.IdStudy,
                    (e, _) => e
                ).SingleOrDefault();
            }
        }

        private Enrollment GetLastEnrollmentForStudy(int id)
        {
            using (var db = new s18838Context())
            {
                return db.Enrollment
               .Where(e => e.IdStudy == id && e.Semester == 1)
               .FirstOrDefault();
            }
        }

        private void SaveStudent(s18838Context db, Student student)
        {
            db.Student.Add(student);
            db.SaveChanges();
        }
        public bool CheckIfStudentExists(string indexNumber)
        {
            using (var db = new s18838Context())
            {
                return db.Student
                 .Where(s => s.IndexNumber == indexNumber)
                 .Select(e => true)
                 .FirstOrDefault();
            }
        }

        private void SaveEnrollment(s18838Context db, Enrollment enrollment)
        {
            db.Enrollment.Add(enrollment);
            db.SaveChanges();
        }

        public bool LogIn(LoginCredentials loginCredentials)
        {
            using (var db = new s18838Context())
            {
                var salt = GetSalt(loginCredentials.Login);
                var valueBytes =
                       KeyDerivation.Pbkdf2(
                            loginCredentials.Password,
                            Encoding.UTF8.GetBytes(salt),
                            KeyDerivationPrf.HMACSHA512,
                            1000,
                            256 / 8
                        );

                var password = Convert.ToBase64String(valueBytes);
                return db.Student
                    .Where(s => s.IndexNumber == loginCredentials.Login && s.Password == password)
                    .Select(e => true)
                    .SingleOrDefault();
            }
        }

        public string GetSalt(string indexNumber)
        {
            using (var db = new s18838Context())
            {
                return db.Student
                .Where(s => s.IndexNumber == indexNumber)
                .Select(e => e.Salt)
                .SingleOrDefault();
            }
        }

        public void SaveRefreshToken(string refreshToken, string indexNumber)
        {
            using (var db = new s18838Context())
            {
                var token = new Token
                {
                    IndexNumber = indexNumber,
                    RefreshToken = refreshToken
                };
                db.Token.Add(token);
                db.SaveChanges();
            }
        }

        public string CheckRefreshToken(string refreshToken)
        {
            using (var db = new s18838Context())
            {
                return db.Token
                .Where(t => t.RefreshToken == refreshToken)
                .Select(e => e.IndexNumber)
                .FirstOrDefault();
            }
        }

        public void DeleteRefreshToken(string refreshToken)
        {
            using (var db = new s18838Context())
            {
                var token = db.Token
                    .Where(t => t.RefreshToken == refreshToken)
                    .SingleOrDefault();
                db.Token.Remove(token);
                db.SaveChanges();
            }
        }
    }
}