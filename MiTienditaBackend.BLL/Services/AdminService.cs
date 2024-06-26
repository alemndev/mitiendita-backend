﻿using AutoMapper;
using MiTienditaBackend.DAL.Repository.Contract;
using MiTienditaBackend.DTO.Requests.Admin;
using MiTienditaBackend.BLL.Services.Contract;
using MiTienditaBackend.DTO;
using MiTienditaBackend.Entity;
using MiTienditaBackend.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiTienditaBackend.BLL.Services
{
    public class AdminService : IAdminService
  {
    private readonly IGenericRepository<User> _userRep;
    private readonly IMapper _mapper;
    private readonly PasswordHasher _passwordHasher = new();

    public AdminService(IGenericRepository<User> userRep, IMapper mapper)
    {
      _userRep = userRep;
      _mapper = mapper;
    }

    private async Task<bool> CheckSuperAdmin(string? superAdminPassword)
    {
      // Without admins
      if (await _userRep.Count() == 0)
        return true;

      // Admind needed
      if (await _userRep.Count() > 0 && superAdminPassword == null)
        return false;

      User? SuperAdmin = _userRep.Get(admin => admin.UserRole == 1);
      bool CheckPasswordSuperAdmin = _passwordHasher.Verify(SuperAdmin.Password, superAdminPassword);

      // Invalid password admin
      if (!CheckPasswordSuperAdmin)
        return false;

      return true;
    }

    public User? GetAdmin(int adminId)
    {
      try
      {
        User? FindAdmin = _userRep.Get(admin => admin.UserId == adminId && admin.UserRole == 2);

        return FindAdmin;
      }
      catch (Exception)
      {

        throw;
      }
    }

    public async Task<AdminDTO> CreateAdmin(CreateAdminRequesrtDTO model)
    {
      try
      {
        bool SuperAdminCheck = await CheckSuperAdmin(model.SuperAdminPassword);

        if (!SuperAdminCheck)
        {
          throw new TaskCanceledException("admin_superadmin_password_required_or_incorrect");
        }

        User? FindAdmin = _userRep.Get(admin => admin.Mail == model.Mail);
      
        if (FindAdmin != null)
        {
          throw new TaskCanceledException("admin_mail_taked");
        }

        User NewAdmin = new User();

        NewAdmin.Mail = model.Mail;
        NewAdmin.Password = _passwordHasher.Hash(model.Password);
        NewAdmin.PasswordHint = model.PasswordHint;

        if (SuperAdminCheck && await _userRep.Count() == 0)
        {
          NewAdmin.UserRole = 1;
        }
        else
        {
          NewAdmin.UserRole = 2;
        }

        User CreatedAdmin = await _userRep.Create(_mapper.Map<User>(NewAdmin));
       
        return _mapper.Map<AdminDTO>(CreatedAdmin);
      }
      catch (Exception)
      {
        throw;
      }
    }

    public async Task<AdminDTO> UpdateAdmin(UpdateAdminRequestDTO model)
    {
      try
      {
        User? FindAdmin = _userRep.Get(admin => admin.UserId == model.AdminId);

        if (FindAdmin == null)
        {
          throw new TaskCanceledException("admin_incorrect_id");
        }

        FindAdmin.Mail = model.Mail != null ? model.Mail : FindAdmin.Mail;
        FindAdmin.Password = model.Password != null ? _passwordHasher.Hash(model.Password) : FindAdmin.Password;
        FindAdmin.PasswordHint = model.PasswordHint != null ? model.PasswordHint : FindAdmin.PasswordHint;

        await _userRep.Update(FindAdmin);

        return _mapper.Map<AdminDTO>(FindAdmin);
      }
      catch (Exception)
      {
        throw;
      }
    }

    public async Task<bool> DeleteAdmin(int adminId)
    {
      try
      {
        User? FindAdmin = GetAdmin(adminId);

        if (FindAdmin == null)
        {
          throw new TaskCanceledException("admin_incorrect_id");
        }

        return await _userRep.Delete(FindAdmin);
      }
      catch (Exception)
      {
        throw;
      }
    }
  }
}
