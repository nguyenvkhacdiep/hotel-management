using System.Linq.Dynamic.Core;
using AutoMapper;
using HotelManagement.Data;
using HotelManagement.Models.Common;
using HotelManagement.Services.Dto;
using HotelManagement.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.Services;

public class CustomerService :ICustomerService
{
    private readonly AppDbContext _dbcontext;
    private readonly IMapper _mapper;
    
    public CustomerService(AppDbContext context, IMapper mapper)
    {
        _dbcontext = context;
        _mapper = mapper;
    }
    
    public async Task<CustomerResponseModel> GetCustomerByPhone(string phone)
    {
        var query = await _dbcontext.Customers.FirstOrDefaultAsync(b => b.Phone == phone);
        
        var response = _mapper.Map<CustomerResponseModel>(query);
        return response;
    }
}