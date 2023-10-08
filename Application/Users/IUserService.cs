using Application.Users.Models;

namespace Application.Users;

public interface IUserService
{
    Task<AddedUserView> RegisterUser(UpdateIdentityUserModel identityUser, string phoneNumber);
    Task<string> SendConfirmationChangePhoneNumber(string userId, string newPhoneNumber);
    Task ChangePhoneNumber(string userId, string newPhoneNumber, string confirmationCode);
    Task<UserView> UpdateUser(UpdateIdentityUserModel identityUser, string userId);
    Task<UserView> GetUserInfo(string userId);
    Task AttachRoles(ManageRoleModel model);
    Task DetachRoles(ManageRoleModel model);
    Task DeleteUser(string identityUserId);
    Task<string> FindUserByLogPass(string login, string password);
    Task<AddedUserView> GenerateNewPassword(string identityUserId);
    Task ActivateUser(string identityUserId);
    Task<IEnumerable<AddedUserView>> GenerateNewPasswordBatch(IEnumerable<string> identityUserIds);
    Task UpdateUserBatch(IEnumerable<UpdateIdentityUserBatchModel> modelIdentityUser);
}