using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using cw3.DTOs;
using cw3.Models;
using Microsoft.AspNetCore.Mvc;

namespace cw3.DAL
{
    public interface IStudentDbService
    {
        public IEnumerable<Student> GetStudents();
        public void modifyStudent(string indexNumber, Student student);
        public void removeStudent(string indexNumber);
        public IEnumerable<Enrollment> GetEnrollments(string indexNumber);
        public Enrollment EnrollStudent(EnrollStudentRequest request);
        public Enrollment PromoteStudents(PromoteStudentsRequest request);
        public bool LogIn(LoginCredentials loginCredentials);
        public string GetSalt(string indexNumber);
        public void SaveRefreshToken(String refreshToken, string indexNumber);
        public string CheckRefreshToken(string refreshToken);
        public void DeleteRefreshToken(string refreshToken);
    }
}
