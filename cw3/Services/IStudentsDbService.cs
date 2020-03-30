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
        public IEnumerable<Enrollment> GetEnrollments(string id);
        public Enrollment EnrollStudent(EnrollStudentRequest request);
        public Enrollment PromoteStudents(PromoteStudentsRequest request);
    }
}
