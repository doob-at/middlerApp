using System;
using System.Linq;
using AutoMapper;
using doob.middler.Common.SharedModels.Models;
using middlerApp.DataAccess.Entities.Models;
using middlerApp.SharedModels;

namespace middlerApp.Api.MapperProfiles
{
    public class EndpointRuleProfile : Profile
    {
        public EndpointRuleProfile()
        {
            //CreateMap<EndpointRuleEntity, MiddlerRule>()
            //    .ForMember(
            //        dest => dest.Scheme, 
            //        expression => expression.MapFrom(src => Split(src.Scheme)))
            //    .ForMember(
            //        dest => dest.HttpMethods,
            //        expression => expression.MapFrom(src => Split(src.HttpMethods)));

            //CreateMap<MiddlerRule, EndpointRuleEntity>()
            //    .ForMember(
            //        dest => dest.Scheme,
            //        expression => expression.MapFrom(src => String.Join("; ", src.Scheme)))
            //    .ForMember(
            //        dest => dest.HttpMethods,
            //        expression => expression.MapFrom(src => String.Join("; ", src.HttpMethods)));


            //CreateMap<EndpointActionEntity, EndpointActionDto>()
            //    .ForMember(dto => dto.Parameters,
            //        expression => expression.MapFrom(entity => entity.)
            //        )

            CreateMap<EndpointRuleEntity, EndpointRuleListDto>()
                .ForMember(dto => dto.Actions,
                    expression => expression.MapFrom(entity => entity.Actions.OrderBy(a => a.Order)))
                .ForMember(
                    dest => dest.Scheme,
                    expression => expression.MapFrom(src => Helper.MappingHelper.Split(src.Scheme)))
                .ForMember(
                    dest => dest.HttpMethods,
                    expression => expression.MapFrom(src => Helper.MappingHelper.Split(src.HttpMethods)));

            
            
            CreateMap<EndpointRuleEntity, EndpointRuleDto>()
                .ForMember(
                    dto => dto.Scheme,
                    opts => opts.MapFrom((dbModel) => Helper.MappingHelper.Split(dbModel.Scheme)))
                .ForMember(
                    dto => dto.HttpMethods,
                    opts => opts.MapFrom((dbModel) => Helper.MappingHelper.Split(dbModel.HttpMethods)));



            CreateMap<EndpointRuleDto, EndpointRuleEntity>()
                .ForMember(
                    dto => dto.Scheme,
                    opts => opts.MapFrom((dbModel) => String.Join("; ", dbModel.Scheme)))
                .ForMember(
                    dto => dto.HttpMethods,
                    opts => opts.MapFrom((dbModel) => String.Join("; ", dbModel.HttpMethods)));


            CreateMap<EndpointRuleEntity, MiddlerRule>()
                .ForMember(
                    dest => dest.Scheme,
                    expression => expression.MapFrom(src => Helper.MappingHelper.Split(src.Scheme)))
                .ForMember(
                    dest => dest.HttpMethods,
                    expression => expression.MapFrom(src => Helper.MappingHelper.Split(src.HttpMethods)));

        }

    }
}
