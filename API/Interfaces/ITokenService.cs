using API.Entities;

namespace API.Intertfaces;

public interface ITokenService
  {
    string CreateToken (AppUser user);

  }