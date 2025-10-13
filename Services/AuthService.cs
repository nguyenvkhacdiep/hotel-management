using AutoMapper;
using Ecommerce.Base.Helpers;
using HotelManagement.Data;
using HotelManagement.Extensions;
using HotelManagement.Models;
using HotelManagement.Services.Dto;
using HotelManagement.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.Services;

public class AuthService:IAuthService
{
    private readonly AppDbContext _dbcontext;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly IMapper _mapper;
    private readonly JwtTokenGenerator _jwtTokenGenerator;
    
    public AuthService(AppDbContext context, IPasswordHasher<User> passwordHasher, IMapper mapper, JwtTokenGenerator jwtTokenGenerator)
    {
        _dbcontext = context;
        _passwordHasher = passwordHasher;
        _mapper = mapper;
        _jwtTokenGenerator = jwtTokenGenerator;
    }
    
    public async Task<UserLoginResponseModel> Login(UserLoginDto userLoginDto)
    {
        var user = await _dbcontext.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Username == userLoginDto.Username);

        if (user == null)
        {
            var errors = new List<FieldError>
            {
                new()
                {
                    Field = "Username",
                    Issue = "Username is incorrect or account does not exist."
                }
            };
            throw new BadRequestException("INVALID_FIELD", errors);
        }

        if (!user.IsActive)
            throw new BadRequestException(
                "Account is inactive. Please check your email or resend activation link.");

        var passwordVerificationResult =
            _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, userLoginDto.Password);
        if (passwordVerificationResult == PasswordVerificationResult.Failed)
        {
            var errors = new List<FieldError>
            {
                new()
                {
                    Field = "password",
                    Issue = "Password is incorrect."
                }
            };
            throw new BadRequestException("INVALID_FIELD", errors);
        }

        var (token, expiresIn) =
            _jwtTokenGenerator.GenerateToken(user.Id, user.Username, user.Role.Name);

        return new UserLoginResponseModel
        {
            User = _mapper.Map<UserResponseModel>(user),
            Token = token,
            TokenExpiresIn = expiresIn
        };
    }
}