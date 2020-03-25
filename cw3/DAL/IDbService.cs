using System;
using System.Collections.Generic;
using cw3.Models;

namespace cw3.DAL
{
    public interface IDBService
    {
        public IEnumerable<Student> GetStudents();
        public IEnumerable<Enrollment> GetEnrollments(string id);
    }
}
