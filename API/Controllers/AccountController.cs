using System.Security.Cryptography;
using System.Text;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Intertfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

public class AccountController : BaseApiController
  {
    private readonly DataContext _context;

    private readonly ITokenService _tokenService;
    public AccountController (DataContext context, ITokenService tokenService)
      {
        _tokenService = tokenService;
        _context      = context;
      }

      [HttpPost("register")] //api/account/register
    public async Task<ActionResult<UserDto>> Register (RegisterDto registerDto)
      {

        if (await UserExists (registerDto.Username))
          return BadRequest("Username is taken");

        using var
          hmac = new HMACSHA512 ();

        var
          user = new AppUser 
            {
              UserName = registerDto.Username.ToLower (),
              PasswordHash = hmac.ComputeHash (Encoding.UTF8.GetBytes (registerDto.Password)),
              PasswordSalt = hmac.Key
            };

        _context.Users.Add(user);

        await _context.SaveChangesAsync();

        return new UserDto
        {
          Username = user.UserName,
          Token    = _tokenService.CreateToken (user)
        };

      }

    [HttpPost("login")]
    public async Task<ActionResult<UserDto>> Login (LoginDto loginDto)
      {

        var
          user = await _context.Users.SingleOrDefaultAsync(x => x.UserName == loginDto.Username.ToLower());

        if ( user == null )
          return Unauthorized ("Invalid Username");

        using var
          hmac = new HMACSHA512 (user.PasswordSalt);

        var
          computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes (loginDto.Password));

        for (int j = 0; j < computedHash.Length; j++ )
          if ( computedHash[j] != user.PasswordHash[j] )
            return Unauthorized ("Invalid Password");

        return new UserDto
        {
          Username = user.UserName,
          Token    = _tokenService.CreateToken (user)
        };

      }

    private async Task<Boolean> UserExists (string username)
    {
      return await _context.Users.AnyAsync(x => x.UserName == username.ToLower ());
    }

  }