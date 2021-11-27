using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using doob.Reflectensions;
using middlerApp.Auth.Entities;
using middlerApp.Auth.Models.DTO;

namespace middlerApp.Auth.Mappers
{
    public class MClientMapperProfile: Profile
    {
        public MClientMapperProfile()
        {

            CreateMap<ClientPostLogoutRedirectUri, string>()
                .ConstructUsing(src => src.PostLogoutRedirectUri)
                .ReverseMap()
                .ForMember(dest => dest.PostLogoutRedirectUri, opt => opt.MapFrom(src => src));

            CreateMap<ClientRedirectUri, string>()
                .ConstructUsing(src => src.RedirectUri)
                .ReverseMap()
                .ForMember(dest => dest.RedirectUri, opt => opt.MapFrom(src => src));

            CreateMap<Client, MClientDto>()
                .ForMember(dest => dest.RequirePkce, opt => opt.MapFrom(src => src.Requirements.Contains("ft:pkce")));

            CreateMap<MClientDto, Client>()
                .ForMember(dest => dest.Requirements, opt => opt.MapFrom((src, dest) =>
                {
                    var ls = dest.Requirements?.StartsWith("[") == true
                        ? Json.Converter.ToObject<List<string>>(dest.Requirements)
                        : new List<string>();

                    
                    if (src.RequirePkce)
                    {
                        if (!ls.Contains("ft:pkce"))
                        {
                            ls.Add("ft:pkce");
                        }
                    }
                    else
                    {
                        if (ls.Contains("ft:pkce"))
                        {
                            ls.Remove("ft:pkce");
                        }
                    }

                    return Json.Converter.ToJson(ls);
                }));
        }
    }
}
