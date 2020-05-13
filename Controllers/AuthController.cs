using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using AuthExample.ViewModels;
using AuthExample.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace AuthExample.Controllers
{
  // [Route("api/[controller]")]
  [Route("auth")]
  [ApiController]
  public class AuthController : ControllerBase
  {

    private DatabaseContext _context;

    public AuthController(DatabaseContext context)
    {
      _context = context;
    }

    // Parameters we need to accept - {fullName: "", email: "", password: ""}
    [HttpPost("signup")]
    public async Task<ActionResult> SignUpUser(NewUser2 newUser)
    {
      if (newUser.Password.Length < 7)
      {
        return BadRequest("Password must be at least 7 characters.");
      }

      var doesUserExist = await _context.Users.AnyAsync(User => User.Email.ToLower() == newUser.Email.ToLower());
      if (doesUserExist)
      {
        return BadRequest("A user already exists with that email.");
      }

      var user = new User
      {
        Email = newUser.Email,
        FullName = newUser.FullName,
      };

      var hashed = new PasswordHasher<User>().HashPassword(user, newUser.Password);
      user.HashedPassword = hashed;
      _context.Users.Add(user);
      await _context.SaveChangesAsync();



      var expirationTime = DateTime.UtcNow.AddHours(10);
      return Ok(new { Token = CreateJWT(user), user = user });
    }
    private string CreateJWT(User user)
    {
      var expirationTime = DateTime.UtcNow.AddHours(10);
      var tokenDescriptor = new SecurityTokenDescriptor
      {
        Subject = new ClaimsIdentity(new[]
        {
            new Claim("id", user.Id.ToString()),
            new Claim("email", user.Email),
            new Claim("name", user.FullName)
      }),
        Expires = expirationTime,
        SigningCredentials = new SigningCredentials(
               new SymmetricSecurityKey(Encoding.ASCII.GetBytes("SOME REALLY LONG STRING")),
              SecurityAlgorithms.HmacSha256Signature
          )
      };

      var tokenHandler = new JwtSecurityTokenHandler();
      var token = tokenHandler.WriteToken(tokenHandler.CreateToken(tokenDescriptor));

      user.HashedPassword = null;
      return (token);
    }


    [HttpPost("login")]
    public async Task<ActionResult> Login(UserLogin useLogIn)
    {

      Console.WriteLine("========= EMAIL" + useLogIn.Email);
      Console.WriteLine("========= PASSWORD" + useLogIn.Password);

      // find the user
      var user = await _context
       .Users
       .FirstOrDefaultAsync(user => user.Email.ToLower() == useLogIn.Email.ToLower());
      if (user == null)
      {
        return BadRequest("User does not exists");
      }
      Console.WriteLine("========= Before Pass HAsh");
      var results = new PasswordHasher<User>().VerifyHashedPassword(user, user.HashedPassword, useLogIn.Password);

      Console.WriteLine("========= HASHED" + user.HashedPassword);

      if (results == PasswordVerificationResult.Success)
      {
        user.HashedPassword = null;
        return Ok(new { Token = CreateJWT(user), user = user });
      }
      else
      {
        return BadRequest("Incorrect password!");
      }
    }
  }
}