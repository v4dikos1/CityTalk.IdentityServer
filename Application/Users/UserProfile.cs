using Application.Users.Models;
using AutoMapper;
using Domain.Entities;

namespace Application.Users;

public class UserProfile : Profile
{
    public UserProfile()
    {
        CreateMap<AddIdentityUserModel, ApplicationUser>()
            .ForMember(d => d.UserName, opt => opt.MapFrom(src => src.PhoneNumber));
        CreateMap<UpdateIdentityUserModel, ApplicationUser>();
        CreateMap<ApplicationUser, AddedUserView>()
            .ForMember(d => d.IdentityId, opt => opt.MapFrom(src => src.Id));
        CreateMap<ApplicationUser, UserView>()
            .ForMember(d => d.IdentityId, opt => opt.MapFrom(src => src.Id));
    }
}