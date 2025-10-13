using AutoMapper;
using Ecommerce.Base.Exceptions;
using HotelManagement.Data;
using HotelManagement.Extensions;
using HotelManagement.Helpers;
using HotelManagement.Models;
using HotelManagement.Services.Common;
using HotelManagement.Services.Dto;
using HotelManagement.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.Services;

public class UserService : IUserService
{
    private readonly AppDbContext _dbcontext;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly IMapper _mapper;

    public UserService(AppDbContext context, IPasswordHasher<User> passwordHasher, IMapper mapper)
    {
        _dbcontext = context;
        _passwordHasher = passwordHasher;
        _mapper = mapper;
    }

    public async Task<string> AddUserAsync(AddUserDto addUserDto)
    {
        var existingUser = await _dbcontext.Users.Where(u => u.Username == addUserDto.Username)
            .FirstOrDefaultAsync();

        if (existingUser != null)
        {
            var errors = new List<FieldError>
            {
                new()
                {
                    Field = "username",
                    Issue = "Username is already in use."
                }
            };
            throw new BadRequestException("INVALID_FIELD", errors);
        }

        var newUser = new User
        {
            Id = Guid.NewGuid(),
            Username = addUserDto.Username,
            PasswordHash = _passwordHasher.HashPassword(new User(), addUserDto.Password),
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            RoleId = addUserDto.RoleId,
        };

        _dbcontext.Users.Add(newUser);
        await _dbcontext.SaveChangesAsync();

        return "User has been created successfully.";
    }

    public async Task<PageList<UserResponseModel>> GetAllUsers(RequestParameters parameters)
    {
        var query = _dbcontext.Users.Include(u => u.Role).Where(u => u.Role.Name != "Admin");

        if (!string.IsNullOrWhiteSpace(parameters.SearchKey))
            query = query.Where(x =>
                x.Username != null && x.Username.Contains(parameters.SearchKey));

        if (string.IsNullOrWhiteSpace(parameters.OrderBy))
            query = query.OrderByDescending(x => x.CreatedAt).ThenBy(x => x.CreatedAt);
        else
            query = query.ApplySort(parameters.OrderBy);

        var users = await query.Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync();

        var userResponse = _mapper.Map<List<UserResponseModel>>(users);

        return new PageList<UserResponseModel>(userResponse, query.Count(),
            parameters.PageNumber,
            parameters.PageSize);
    }

    public async Task<UserResponseModel> GetUserById(Guid id)
    {
        var findUser = await _dbcontext.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Id == id);
        
        if(findUser == null)
            throw new NotFoundException("User not found.");
        
        var user = _mapper.Map<UserResponseModel>(findUser);
        return user;
    }

public async Task<string> DeleteUserAsync(Guid id)
    {
        var findUser = await _dbcontext.Users.FirstOrDefaultAsync(u => u.Id == id);
        
        if(findUser == null)
            throw new NotFoundException("User not found.");
        
        _dbcontext.Users.Remove(findUser);
        await _dbcontext.SaveChangesAsync();
        
        return "User has been deleted successfully.";
    }
    
    public async Task<string> InactiveUserAsync(Guid id)
    {
        var findUser = await _dbcontext.Users.FirstOrDefaultAsync(u => u.Id == id);
        
        if(findUser == null)
            throw new NotFoundException("User not found.");
        
        findUser.IsActive = !findUser.IsActive;
        findUser.UpdatedAt = DateTime.UtcNow;
        
        _dbcontext.Users.Update(findUser);
        await _dbcontext.SaveChangesAsync();
        
        return "User has been deactivated successfully.";
    }

    public async Task<string> EditUserAsync(Guid id,EditUserDto editUserDto)
    {
        var findUser = await _dbcontext.Users.FirstOrDefaultAsync(u => u.Id == id);
        
        if(findUser == null)
            throw new NotFoundException("User not found.");
        
        var existingUser = await _dbcontext.Users
            .FirstOrDefaultAsync(u => u.Username == editUserDto.Username && u.Id != id);

        if (existingUser != null)
        {
            var errors = new List<FieldError>
            {
                new()
                {
                    Field = "username",
                    Issue = "Username is already in use."
                }
            };
            throw new BadRequestException("INVALID_FIELD", errors);
        }
        
        findUser.Username = editUserDto.Username;
        findUser.UrlAvatar = editUserDto.UrlAvatar;
        findUser.RoleId = editUserDto.RoleId;
        findUser.IsActive = editUserDto.IsActive;
        findUser.UpdatedAt = DateTime.UtcNow;
        
        _dbcontext.Users.Update(findUser);
        await _dbcontext.SaveChangesAsync();
        
        return "User has been edited successfully.";
    }
}