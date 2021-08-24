using System;
using System.Linq;
using AutoMapper;
using middlerApp.Auth.Entities;
using middlerApp.Auth.Models.DTO;

namespace middlerApp.Auth.Mappers
{
    public class MUserMapperProfile : Profile
    {
        public MUserMapperProfile()
        {
            CreateMap<MUserDto, MUser>()
                .ForMember(dest => dest.Roles, 
                    expression => expression.MapFrom((dto, user) => dto.Roles.Select(r => new MRole() {Id = r.Id})));

            CreateMap<MUser, MUserDto>()
                .ForMember(dest => dest.Roles,
                    expression => expression.MapFrom((user, dto) => user.Roles))
                .ForMember(dest => dest.HasPassword,
                    expression => expression.MapFrom((user, dto) => !String.IsNullOrWhiteSpace(user.Password)));

            CreateMap<MUserClaim, MUserClaimDto>().ReverseMap();

            CreateMap<MExternalClaim, MExternalClaimDto>().ReverseMap();

            CreateMap<MUser, MUserListDto>()
                .ForMember(dest => dest.HasPassword,
                    expression => expression.MapFrom((user, dto) => !String.IsNullOrWhiteSpace(user.Password)))
                .ForMember(dest => dest.Logins, expression => expression.MapFrom((user, dto) => user.Logins.Select(l => l.Provider)));

            CreateMap<MUserListDto, MUser>();
        }
    }
}
