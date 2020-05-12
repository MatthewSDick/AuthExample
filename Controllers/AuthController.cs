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

      return Ok(user);
    }
  }
}