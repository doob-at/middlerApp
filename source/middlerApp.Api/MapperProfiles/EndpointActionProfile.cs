using AutoMapper;
using doob.middler.Common.SharedModels.Models;
using doob.Reflectensions;
using middlerApp.Api.MapperProfiles.Formatters;
using middlerApp.DataAccess.Entities.Models;
using middlerApp.SharedModels;

namespace middlerApp.Api.MapperProfiles
{
    public class EndpointActionProfile: Profile
    {
        public EndpointActionProfile()
        {
            CreateMap(typeof(EndpointActionEntity), typeof(MiddlerAction<>))
                .ForMember("Parameters", mbr => mbr.ConvertUsing(new StringToJObjectFormatter(), "Parameters"));

            CreateMap(typeof(MiddlerAction<>), typeof(EndpointActionEntity))
                .ForMember("Parameters", mbr => mbr.ConvertUsing(new StringToJObjectFormatter(), "Parameters"));

            CreateMap<EndpointActionEntity, MiddlerAction>();

            CreateMap<EndpointActionEntity, EndpointActionDto>()
                .ForMember(dest => dest.Parameters, expression => expression.MapFrom(dto => Json.Converter.ToDictionary(dto.Parameters)));

            CreateMap<EndpointActionDto, EndpointActionEntity>()
                .ForMember(dest => dest.Parameters, expression => expression.MapFrom(dto => Json.Converter.ToJObject(dto.Parameters)));

            CreateMap<EndpointActionEntity, EndpointRuleListActionDto>();

            CreateMap<EndpointActionEntity, MiddlerAction>();
        }
    }
}
