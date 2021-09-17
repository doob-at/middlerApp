using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using middlerApp.Auth.Entities;
using MiddlerApp.Ldap;

namespace middlerApp.Api.Providers
{
    public class WindowsAuthenticationUserFactory : IExternalUserFactory
    {
        private readonly string _providerId;
        private readonly ClaimsPrincipal _claimsPrincipal;
        private readonly Ldap _ldap;
        private readonly ProviderCache _providerCache;
        private MUser _mUser;

        private List<string> attributesToLoad = new List<string>
        {
            "ObjectGuid",
            "sn",
            "givenName",
            "mail",
            "memberof"
        };

        public WindowsAuthenticationUserFactory(string providerId, ClaimsPrincipal claimsPrincipal, Ldap ldap, ProviderCache providerCache)
        {
            _providerId = providerId;
            _claimsPrincipal = claimsPrincipal;
            _ldap = ldap;
            _providerCache = providerCache;
        }

        public string GetSubject()
        {
            return FetchUserFromLdap().Subject;
        }


        public MUser BuildUser()
        {
            return FetchUserFromLdap();
        }


        public void UpdateClaims(MUser oldMUser)
        {
            var newRoles = _mUser.ExternalClaims.Where(c => c.Type == "groupDN" && c.Issuer == _providerId).ToList();
            var newRolesValues = newRoles.Select(r => r.Value).ToList();
            var oldRoles = oldMUser.ExternalClaims.Where(c => c.Type == "groupDN" && c.Issuer == _providerId).ToList();
            var oldRolesValues = oldRoles.Select(r => r.Value).ToList();

            var removeRoles = new List<MExternalClaim>();

            foreach (var mUserClaim in oldRoles)
            {
                if (!newRolesValues.Contains(mUserClaim.Value))
                {
                    removeRoles.Add(mUserClaim);
                }
            }

            foreach (var mUserClaim in removeRoles)
            {
                oldMUser.ExternalClaims.Remove(mUserClaim);
            }

            foreach (var mUserClaim in newRoles)
            {
                if (!oldRolesValues.Contains(mUserClaim.Value))
                {
                    oldMUser.ExternalClaims.Add(mUserClaim);
                }
            }

        }

        private MUser FetchUserFromLdap()
        {

            if (_mUser == null)
            {

                var s = _claimsPrincipal.FindFirst(ClaimTypes.PrimarySid)?.Value;
                if (s == null)
                    return null;

                var ldapObject = _ldap.Search($"objectSID={s}", attributesToLoad.ToArray()).FirstOrDefault();


                _mUser = new MUser();
                _mUser.FirstName = ldapObject.GetValueOrDefault<string>("GivenName");
                _mUser.LastName = ldapObject.GetValueOrDefault<string>("sn");
                _mUser.Active = true;
                _mUser.Subject = ldapObject.GetValueOrDefault<Guid>("ObjectGuid").ToString();
                _mUser.UserName = _claimsPrincipal.Identity?.Name;
                _mUser.Email = ldapObject.GetValueOrDefault<string>("mail");
                //_mUser.DisplayName = ldapObject.GetValueOrDefault<string>("displayname");
                
                if (_claimsPrincipal.Identity is WindowsIdentity wi)
                {
                    
                    foreach (var identityReference in wi.Groups)
                    {
                        try
                        {
                            var groupObj = _ldap.GetObjectBySid(identityReference.Value);
                            if (groupObj is not null)
                            {
                                var mClaim = new MExternalClaim();
                                mClaim.Type = "role";
                                mClaim.Value = groupObj.GetValueOrDefault<string>("cn");
                                mClaim.Issuer = _providerId;
                                _mUser.ExternalClaims.Add(mClaim);

                                //var mClaim = new MRole();
                                //mClaim.Name = groupObj.GetValueOrDefault<string>("cn");
                                //mClaim.DisplayName = groupObj.GetValueOrDefault<string>("name");
                                //mClaim.Description = groupObj.GetValueOrDefault<string>("description");
                                //_mUser.Roles.Add(mClaim);
                            }
                            
                        }
                        catch { }

                    }
                }
                
                //foreach (var group in ldapObject.GetValuesOrDefault<string>("memberof"))
                //{
                //    var mClaim = new MExternalClaim();
                //    mClaim.Type = "groupDN";
                //    mClaim.Value = group;
                //    mClaim.Issuer = _providerId;
                //    _mUser.ExternalClaims.Add(mClaim);

                //}


            }

            return _mUser;
        }


    }
}