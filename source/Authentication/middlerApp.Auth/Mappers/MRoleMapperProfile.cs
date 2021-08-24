using AutoMapper;
using middlerApp.Auth.Entities;
using middlerApp.Auth.Models.DTO;

namespace middlerApp.Auth.Mappers
{
    public class MRoleMapperProfile : Profile
    {
        public MRoleMapperProfile()
        {
            CreateMap<MRole, MRoleDto>()
                .ForMember(dest => dest.Users, expression => expression.MapFrom((role, dto) => role.Users));

            CreateMap<MRoleDto, MRole>()
                .ForMember(dest => dest.Users, expression => expression.MapFrom((dto, role) => dto.Users));


            CreateMap<MRole, MRoleListDto>();
            //CreateMap<MRoleListDto, MRole>();
        }
    }
}
