using System.Security.Claims;
using Application.Users;
using Application.Users.Models;
using AutoMapper;
using Domain.Entities;
using IdentityModel;
using Infrastructure.Helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services.Users;

public class UserService : IUserService
{
    private readonly IMapper _mapper;
    private readonly UserManager<ApplicationUser> _userManager;

    public UserService(UserManager<ApplicationUser> userManager, IMapper mapper)
    {
        _userManager = userManager;
        _mapper = mapper;
    }

    public async Task<AddedUserView> RegisterUser(UpdateIdentityUserModel identityUser, string phoneNumber)
    {
        var existedUser = phoneNumber == null ? null : await _userManager.FindByNameAsync(phoneNumber);
        if (existedUser == null)
        {
            if (string.IsNullOrEmpty(phoneNumber))
            {
                phoneNumber = _userManager.GenerateUsername();
            }

            var password = PasswordHelper.GeneratePassword(_userManager.Options.Password);
            var newUser = await AddNewUser(identityUser, password, phoneNumber.ConvertPhoneNumberFormat());
            await AddClaimsToUser(newUser);
            await AddRoleToUser(newUser, identityUser.Roles);
            var result = _mapper.Map<AddedUserView>(newUser);
            result.Login = phoneNumber;
            result.Password = password;
            return result;
        }

        var updatedUser = await InnerUpdateUser(identityUser, existedUser);
        await UpdateClaimsToUser(updatedUser);
        await AddRoleToUser(updatedUser, identityUser.Roles);
        return _mapper.Map<AddedUserView>(updatedUser);
    }

    public async Task<string> SendConfirmationChangePhoneNumber(string userId, string newPhoneNumber)
    {
        var existedUser = await _userManager.FindByIdAsync(userId);
        if (existedUser == null)
        {
            throw new ArgumentNullException(nameof(existedUser));
        }

        var checkUserWithPhoneNumber = await _userManager.FindByNameAsync(newPhoneNumber);
        if (checkUserWithPhoneNumber != null)
        {
            throw new ArgumentNullException();
        }

        var confirmationCode = await _userManager.GenerateChangePhoneNumberTokenAsync(existedUser, newPhoneNumber);

        return confirmationCode;
    }

    public async Task<UserView> UpdateUser(UpdateIdentityUserModel identityUser, string identityUserId)
    {
        var updatedUser = await InnerUpdateUser(identityUser, identityUserId);
        await UpdateClaimsToUser(updatedUser);
        await AddRoleToUser(updatedUser, identityUser.Roles);
        return _mapper.Map<UserView>(updatedUser);
    }

    public async Task<UserView> GetUserInfo(string userId)
    {
        var existedUser = await _userManager.FindByIdAsync(userId);
        if (existedUser == null)
        {
            throw new ArgumentNullException(nameof(existedUser));
        }

        return _mapper.Map<UserView>(existedUser);
    }

    public async Task AttachRoles(ManageRoleModel model)
    {
        var existedUser = await _userManager.FindByIdAsync(model.UserId);
        if (existedUser == null)
        {
            throw new ArgumentNullException(nameof(existedUser));
        }

        await AddRoleToUser(existedUser, model.IdentityRoles);
    }

    public async Task DetachRoles(ManageRoleModel model)
    {
        var existedUser = await _userManager.FindByIdAsync(model.UserId);
        if (existedUser == null)
        {
            throw new ArgumentNullException(nameof(existedUser));
        }

        await RemoveRoleToUser(existedUser, model.IdentityRoles);
    }

    public async Task DeleteUser(string identityUserId)
    {
        var existedUser = await _userManager.FindByIdAsync(identityUserId);
        if (existedUser == null)
        {
            throw new ArgumentNullException(nameof(existedUser));
        }

        await _userManager.DeleteAsync(existedUser);
    }

    public async Task<string> FindUserByLogPass(string login, string password)
    {
        var existedUser = await _userManager.FindByNameAsync(login);
        var result = await _userManager.CheckPasswordAsync(existedUser, password);
        if (result)
        {
            return existedUser.Id;
        }

        throw new ApplicationException("Неверный логин или пароль пользователя!");
    }

    public async Task<AddedUserView> GenerateNewPassword(string identityUserId)
    {
        var existedUser = await _userManager.FindByIdAsync(identityUserId);
        if (existedUser == null)
        {
            throw new ArgumentNullException(nameof(existedUser),
                $"Не найден пользователь с идентификатором {identityUserId}");
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(existedUser);
        var newPassword = PasswordHelper.GeneratePassword(_userManager.Options.Password);
        var result = await _userManager.ResetPasswordAsync(existedUser, token, newPassword);
        if (result.Succeeded)
        {
            var view = _mapper.Map<AddedUserView>(existedUser);
            view.Login = existedUser.PhoneNumber;
            view.Password = newPassword;
            return view;
        }

        throw new ApplicationException(result.Errors.Aggregate(string.Empty,
            (s, error) => s + Environment.NewLine + error.Description));
    }

    public async Task ActivateUser(string identityUserId)
    {
        var existedUser = await _userManager.FindByIdAsync(identityUserId);
        if (existedUser == null)
        {
            throw new ArgumentNullException(nameof(existedUser),
                $"Не найден пользователь с идентификатором {identityUserId}");
        }

        existedUser.IsActivated = true;
        await _userManager.UpdateAsync(existedUser);
    }

    public async Task<IEnumerable<AddedUserView>> GenerateNewPasswordBatch(IEnumerable<string> identityUserIds)
    {
        var existedUsers = await _userManager.Users
            .Where(x => identityUserIds.Contains(x.Id))
            .ToListAsync();
        var resultList = new List<AddedUserView>();
        foreach (var existedUser in existedUsers)
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(existedUser);
            var newPassword = PasswordHelper.GeneratePassword(_userManager.Options.Password);
            var result = await _userManager.ResetPasswordAsync(existedUser, token, newPassword);
            if (result.Succeeded)
            {
                var view = _mapper.Map<AddedUserView>(existedUser);
                view.Login = existedUser.PhoneNumber;
                view.Password = newPassword;
                resultList.Add(view);
            }
            else
            {
                throw new ApplicationException(result.Errors.Aggregate(string.Empty,
                    (s, error) => s + Environment.NewLine + error.Description));
            }
        }

        return resultList;
    }

    public async Task UpdateUserBatch(IEnumerable<UpdateIdentityUserBatchModel> modelIdentityUser)
    {
        foreach (var model in modelIdentityUser)
        {
            var updatedUser = await InnerUpdateUser(model, model.IdentityUserId);
            await UpdateClaimsToUser(updatedUser);
            await AddRoleToUser(updatedUser, model.Roles);
        }
    }


    public async Task ChangePhoneNumber(string userId, string newPhoneNumber, string confirmationCode)
    {
        var existedUser = await _userManager.FindByIdAsync(userId);
        if (existedUser == null)
        {
            throw new ArgumentNullException(nameof(existedUser),
                $"Не найден пользователь с идентификатором {userId}");
        }

        var checkUserWithPhoneNumber = await _userManager.FindByNameAsync(newPhoneNumber);
        if (checkUserWithPhoneNumber != null)
        {
            throw new ArgumentNullException();
        }

        await UpdatePhoneNumber(existedUser, newPhoneNumber, confirmationCode);
    }

    private async Task<ApplicationUser> InnerUpdateUser(UpdateIdentityUserModel identityUser, string identityUserId)
    {
        var existingUser = await _userManager.FindByIdAsync(identityUserId);
        if (existingUser == null)
        {
            throw new ArgumentNullException(nameof(existingUser));
        }

        return await InnerUpdateUser(identityUser, existingUser);
    }

    private async Task<ApplicationUser> InnerUpdateUser(UpdateIdentityUserModel newUserData,
        ApplicationUser existedUser)
    {
        var mergedUser = MergeUser(newUserData, existedUser);
        var result = await _userManager.UpdateAsync(mergedUser);
        if (!result.Succeeded)
        {
            throw new Exception(result.Errors.First().Description);
        }

        if (!string.IsNullOrEmpty(newUserData.Email))
        {
            await UpdateEmail(mergedUser, newUserData.Email);
        }

        return mergedUser;
    }

    private ApplicationUser MergeUser(UpdateIdentityUserModel newUserData, ApplicationUser existedUser)
    {
        existedUser.Name = newUserData.Name;
        existedUser.Surname = newUserData.Surname;
        existedUser.DateBirth = newUserData.DateBirth;
        existedUser.Patronymic = newUserData.Patronymic;
        return existedUser;
    }

    private async Task UpdateEmail(ApplicationUser user, string email)
    {
        if (!email.Equals(user.Email, StringComparison.CurrentCultureIgnoreCase))
        {
            var result = await _userManager.SetEmailAsync(user, email);
            if (!result.Succeeded)
            {
                throw new Exception(result.Errors.First().Description);
            }
        }
    }

    private async Task UpdatePhoneNumber(ApplicationUser user, string phoneNumber, string confirmationCode)
    {
        if (phoneNumber.Equals(user.PhoneNumber, StringComparison.CurrentCultureIgnoreCase))
        {
            var result = await _userManager.ChangePhoneNumberAsync(user, phoneNumber, confirmationCode);
            if (!result.Succeeded)
            {
                throw new Exception(result.Errors.First().Description);
            }

            result = await _userManager.SetUserNameAsync(user, phoneNumber);
            if (!result.Succeeded)
            {
                throw new Exception(result.Errors.First().Description);
            }
        }
    }


    private async Task<ApplicationUser> AddNewUser(UpdateIdentityUserModel modelIdentityUser, string password,
        string phoneNumber)
    {
        if (!phoneNumber.IsPhoneValidFormat())
        {
            throw new ApplicationException("Номер телефона имеет неверный формат!");
        }

        var newUser = _mapper.Map<ApplicationUser>(modelIdentityUser);
        newUser.PhoneNumber = phoneNumber;
        newUser.PhoneNumberSequence = _userManager.Users.Max(x => x.PhoneNumberSequence) + 1;
        newUser.UserName = phoneNumber;
        var result = await _userManager.CreateAsync(newUser, password);
        if (!result.Succeeded)
        {
            throw new Exception(result.Errors.First().Description);
        }

        return newUser;
    }

    private async Task AddClaimsToUser(ApplicationUser user)
    {
        var result = await _userManager.AddClaimsAsync(user,
            new Claim[]
            {
                new(JwtClaimTypes.Name, user.Name ?? string.Empty),
                new(JwtClaimTypes.GivenName, user.Patronymic ?? string.Empty),
                new(JwtClaimTypes.FamilyName, user.Surname ?? string.Empty),
                new(JwtClaimTypes.PhoneNumber, user.PhoneNumber ?? string.Empty),
                new(JwtClaimTypes.PhoneNumberVerified, user.PhoneNumberConfirmed.ToString()),
                new(JwtClaimTypes.Email, user.Email ?? string.Empty),
                new(JwtClaimTypes.EmailVerified, user.EmailConfirmed.ToString())
            });
        if (!result.Succeeded)
        {
            throw new Exception(result.Errors.First().Description);
        }
    }

    private async Task UpdateClaimsToUser(ApplicationUser user)
    {
        var existedClaims = await _userManager.GetClaimsAsync(user);
        var result = await _userManager.RemoveClaimsAsync(user, existedClaims);
        if (!result.Succeeded)
        {
            throw new Exception(result.Errors.First().Description);
        }

        await AddClaimsToUser(user);
    }

    private async Task RemoveRoleToUser(ApplicationUser user, IEnumerable<string> roles)
    {
        if (roles == null)
        {
            return;
        }

        foreach (var role in roles)
        {
            await RemoveRoleToUser(user, role);
        }
    }

    private async Task RemoveRoleToUser(ApplicationUser user, string role)
    {
        if (await _userManager.IsInRoleAsync(user, role))
        {
            await _userManager.RemoveFromRoleAsync(user, role);
        }
    }

    private async Task AddRoleToUser(ApplicationUser user, IEnumerable<string> roles)
    {
        if (roles == null)
        {
            return;
        }

        foreach (var role in roles)
        {
            await AddRoleToUser(user, role);
        }
    }

    private async Task AddRoleToUser(ApplicationUser user, string role)
    {
        if (!await _userManager.IsInRoleAsync(user, role))
        {
            await _userManager.AddToRoleAsync(user, role);
        }
    }
}