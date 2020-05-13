using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using cw3.DAL;
using cw3.DTOs;
using cw3.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace cw3.Controllers
{
    [ApiController]
    [Route("api/account")]
    public class AuthorizationController : ControllerBase
    {
        private readonly IStudentDbService _studentDbService;
        public IConfiguration Configuration { get; set; }

        public AuthorizationController(IStudentDbService studentDbService, IConfiguration configuration)
        {
            _studentDbService = studentDbService;
            Configuration = configuration;
        }

        [HttpPost("logIn")]
        public IActionResult LogIn(LoginCredentials loginCredentials)
        {
            if (_studentDbService.LogIn(loginCredentials))
            {
                var token = CreateJwtToken(loginCredentials.Login);
                _studentDbService.SaveRefreshToken(token.RefreshToken, loginCredentials.Login);
                return Ok(token);
            }
            return Unauthorized();
        }

        [HttpPost("refreshToken")]
        public IActionResult RefreshToken(RefreshTokenDTO refreshTokenDto)
        {
            var login = _studentDbService.CheckRefreshToken(refreshTokenDto.RefreshToken);
            if (login != null)
            {
                var token = CreateJwtToken(login);
                _studentDbService.DeleteRefreshToken(refreshTokenDto.RefreshToken);
                _studentDbService.SaveRefreshToken(token.RefreshToken, login);
                return Ok(token);
            }
            return Forbid();
        }

        public AppToken CreateJwtToken(string login)
        {
            var claims = new[]
               {
                    new Claim(ClaimTypes.NameIdentifier, "1"),
                    new Claim(ClaimTypes.Name, login),
                    new Claim(ClaimTypes.Role, "employee")
                };
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["SecretKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken
            (
                issuer: "Gakko",
                audience: "Students",
                claims: claims,
                expires: DateTime.Now.AddMinutes(10),
                signingCredentials: creds
            );
            var refreshToken = Guid.NewGuid();
            return new AppToken
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                RefreshToken = refreshToken.ToString()
            };
        }
    }
}