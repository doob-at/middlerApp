using AutoMapper;
using AutoMapper.EquivalencyExpression;
using middlerApp.DataAccess.Entities.Models;
using middlerApp.SharedModels;

namespace middlerApp.Api.MapperProfiles
{
    public class EndpointRulePermissionProfile : Profile
    {
        
        public EndpointRulePermissionProfile()
        {


            CreateMap<EndpointRulePermission, EndpointRulePermissionDto>()
                .EqualityComparison((entity, dto) => entity.Id == dto.Id);

            CreateMap<EndpointRulePermissionDto, EndpointRulePermission>()
                .EqualityComparison((dto, entity) => dto.Id == entity.Id);

          
        }
    }
}
