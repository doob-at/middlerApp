using System.Linq;
using doob.middler.Common.SharedModels.Enums;
using doob.middler.Common.SharedModels.Models;
using doob.Reflectensions.Common.Helper;
using middlerApp.DataAccess.Entities.Models;

namespace middlerApp.DataAccess.ExtensionMethods
{
    public static class EndpointRuleEntityExtensions
    {
        public static MiddlerRule ToMiddlerRule(this EndpointRuleEntity entity)
        {
            if (entity == null)
                return null;

            var mRule = new MiddlerRule();
            mRule.Name = entity.Name;
            mRule.Hostname = entity.Hostname;
            mRule.Path = entity.Path;
            mRule.Scheme = MappingHelper.Split(entity.Scheme);
            mRule.HttpMethods = MappingHelper.Split(entity.HttpMethods);
            mRule.Actions = entity.Actions.OrderBy(p => p.Order).Select(ToMiddlerAction).ToList();
            mRule.Permissions = entity.Permissions.OrderBy(p => p.Order).Select(ToMiddlerPermissionRule).ToList();

            return mRule;
        }

        public static MiddlerAction ToMiddlerAction(this EndpointActionEntity entity)
        {
            var mAction = new MiddlerAction();
            mAction.Id = entity.Id;
            mAction.ActionType = entity.ActionType;
            mAction.Enabled = entity.Enabled;
            mAction.Terminating = entity.Terminating;
            mAction.WriteStreamDirect = entity.WriteStreamDirect;
            mAction.Parameters = entity.Parameters;
            
            return mAction;
        }

        public static MiddlerRulePermission ToMiddlerPermissionRule(this EndpointRulePermission entity)
        {
            var mPerm = new MiddlerRulePermission();
            mPerm.Type = Enum<PrincipalType>.Find(entity.Type);
            mPerm.AccessMode = Enum<AccessMode>.Find(entity.AccessMode);
            mPerm.Client = entity.Client;
            mPerm.PrincipalName = entity.PrincipalName;
            mPerm.SourceAddress = entity.SourceAddress;

            return mPerm;
        }
    }
}
