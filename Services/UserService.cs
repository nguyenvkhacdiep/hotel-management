using HotelManagement.Data;
using HotelManagement.Models;
using HotelManagement.Services.Dto;
using HotelManagement.Services.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace HotelManagement.Services;

public class UserService  :IUserService
{
    private readonly AppDbContext _dbcontext;
    private readonly IPasswordHasher<User> _passwordHasher;
    
    
    public UserService(AppDbContext context, IPasswordHasher<User> passwordHasher)
    {
        _dbcontext = context;
        _passwordHasher = passwordHasher;
    }

    public async Task<string> AddUserAsync(AddUserDto addUserDto)
    {
        var newUser = new User
        {
            Id = Guid.NewGuid(),
            Username = addUserDto.Username,
            PasswordHash = _passwordHasher.HashPassword(new User(), addUserDto.PasswordHash),
            Role = addUserDto.Role,
        };
        
        _dbcontext.Users.Add(newUser);
        await _dbcontext.SaveChangesAsync();
        
        return "User created";
    }
}