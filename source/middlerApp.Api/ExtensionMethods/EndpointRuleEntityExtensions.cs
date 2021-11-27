using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using doob.middler;
using doob.middler.Common.SharedModels.Attributes;
using doob.Reflectensions;
using middlerApp.Api.Helper;
using middlerApp.DataAccess.Entities.Models;
using middlerApp.SharedModels;


namespace middlerApp.API.ExtensionMethods
{
    public static class EndpointRuleEntityExtensions
    {
        public static EndpointRuleDto ToEndpointRuleDto(this EndpointRuleEntity entity, InternalHelper internalHelper)
        {
            if (entity == null)
                return null;

            var dto = new EndpointRuleDto();
            dto.Id = entity.Id;
            dto.Name = entity.Name;
            dto.Enabled = entity.Enabled;
            dto.Hostname = entity.Hostname;
            dto.Path = entity.Path;
            dto.Order = entity.Order;
            dto.Scheme = MappingHelper.Split(entity.Scheme);
            dto.HttpMethods = MappingHelper.Split(entity.HttpMethods);
            dto.Actions = entity.Actions.OrderBy(a => a.Order).Select(a => a.ToEndpointRuleDto(internalHelper)).ToList();
            dto.Permissions = entity.Permissions.OrderBy(a => a.Order).Select(ToDto).ToList();
            return dto;
        }

        public static EndpointActionDto ToEndpointRuleDto(this EndpointActionEntity entity, InternalHelper internalHelper)
        {

            var actionType = internalHelper.GetConcreteActionType(entity.ActionType);
            var optionsType = actionType.BaseType.GetGenericArguments().FirstOrDefault();

            var dto = new EndpointActionDto();
            dto.Id = entity.Id;
            dto.ActionType = entity.ActionType;
            dto.Enabled = entity.Enabled;
            dto.EndpointRuleEntityId = entity.EndpointRuleEntityId;
            dto.Order = entity.Order;
            dto.Terminating = entity.Terminating;
            dto.WriteStreamDirect = entity.WriteStreamDirect;
            var tempDict = Json.Converter.ToDictionary(entity.Parameters);
            var nDict = new Dictionary<string, object>();
            foreach (var (key, value) in tempDict)
            {
                var prop = optionsType.GetProperty(key);
                if(prop == null)
                    continue;

                var internalProp = prop.GetCustomAttribute<InternalAttribute>();
                if (internalProp == null)
                {
                    nDict.Add(key, value);
                }
            }

            dto.Parameters = nDict;
            

            return dto;
        }


        public static EndpointRulePermissionDto ToDto(this EndpointRulePermission endpointRulePermission)
        {
            var dto = new EndpointRulePermissionDto();
            dto.Order = endpointRulePermission.Order;
            dto.Id = endpointRulePermission.Id;
            dto.PrincipalName = endpointRulePermission.PrincipalName;
            dto.Type = endpointRulePermission.Type;
            dto.SourceAddress = endpointRulePermission.SourceAddress;
            dto.AccessMode = endpointRulePermission.AccessMode;

            return dto;
        }


        public static EndpointRuleEntity Patch(this EndpointRuleEntity entity, EndpointRuleDto dto)
        {
            entity.Order = dto.Order;
            entity.Name = dto.Name;
            entity.Path = dto.Path;
            entity.Enabled = dto.Enabled;
            entity.Hostname = dto.Hostname;
            entity.Scheme = string.Join(";", dto.Scheme);
            entity.HttpMethods = string.Join(";", dto.HttpMethods);

            entity.Actions = dto.Actions.Select(dtoAction => dtoAction.ToEntity()).ToList();
            entity.Permissions = dto.Permissions.Select(dtoPermission => dtoPermission.ToEntity()).ToList();

            return entity;
        }


        public static EndpointActionEntity ToEntity(this EndpointActionDto dto)
        {
            var entity = new EndpointActionEntity();
            entity.Id = dto.Id ?? Guid.Empty;
            entity.Order = dto.Order;
            entity.Enabled = dto.Enabled;
            entity.ActionType = dto.ActionType;
            entity.EndpointRuleEntityId = dto.EndpointRuleEntityId;
            entity.Terminating = dto.Terminating;
            entity.WriteStreamDirect = dto.WriteStreamDirect;
            entity.Parameters = Converter.Json.ToJObject(dto.Parameters);

            return entity;
        }

        public static EndpointRulePermission ToEntity(this EndpointRulePermissionDto dto)
        {
            var entity = new EndpointRulePermission();
            entity.Id = dto.Id ?? Guid.Empty;
            entity.Order = dto.Order;
            entity.PrincipalName = dto.PrincipalName;
            entity.Type = dto.Type;
            entity.SourceAddress = dto.SourceAddress;
            entity.AccessMode = dto.AccessMode;

            return entity;
        }
    }
}
