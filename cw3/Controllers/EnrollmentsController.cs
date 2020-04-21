using System;
using cw3.DAL;
using cw3.DTOs;
using cw3.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic.CompilerServices;

namespace cw3.Controllers
{
    
    [ApiController]
    [Route("api/enrollments")]
    public class EnrollmentsController : ControllerBase
    {
        
        private readonly IStudentDbService _studentDbService;

        public EnrollmentsController(IStudentDbService studentDbService)
        {
            _studentDbService = studentDbService;
        }
        
        [HttpPost]
        [Authorize(Roles = "employee")]
        public IActionResult EnrollStudent(EnrollStudentRequest request)
        {
            try
            {
                return Created("",_studentDbService.EnrollStudent(request));
            }
            catch (Exception e)
            {
                return BadRequest();
            }
        }
        
        [HttpPost("promotions")]
        [Authorize(Roles = "employee")]
        public IActionResult PromoteStudents(PromoteStudentsRequest request)
        {
            try
            {
                return Created("",_studentDbService.PromoteStudents(request));
            }
            catch (Exception e)
            {
                return NotFound();
            }
        }
    }
}