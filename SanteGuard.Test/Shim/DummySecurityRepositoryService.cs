using SanteDB.Core.Model.Entities;
using SanteDB.Core.Model.Security;
using SanteDB.Core.Security;
using SanteDB.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace SanteGuard.Test.Shim
{
    public class DummySecurityRepositoryService : ISecurityRepositoryService
    {
        public SecurityUser ChangePassword(Guid userId, string password)
        {
            throw new NotImplementedException();
        }

        public SecurityApplication CreateApplication(SecurityApplication application)
        {
            throw new NotImplementedException();
        }

        public SecurityDevice CreateDevice(SecurityDevice device)
        {
            throw new NotImplementedException();
        }

        public SecurityPolicy CreatePolicy(SecurityPolicy policy)
        {
            throw new NotImplementedException();
        }

        public SecurityRole CreateRole(SecurityRole roleInfo)
        {
            throw new NotImplementedException();
        }

        public SecurityUser CreateUser(SecurityUser userInfo, string password)
        {
            throw new NotImplementedException();
        }

        public UserEntity CreateUserEntity(UserEntity userEntity)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<SecurityApplication> FindApplications(Expression<Func<SecurityApplication, bool>> query)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<SecurityApplication> FindApplications(Expression<Func<SecurityApplication, bool>> query, int offset, int? count, out int totalResults)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<SecurityDevice> FindDevices(Expression<Func<SecurityDevice, bool>> query)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<SecurityDevice> FindDevices(Expression<Func<SecurityDevice, bool>> query, int offset, int? count, out int totalResults)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<SecurityPolicy> FindPolicies(Expression<Func<SecurityPolicy, bool>> query)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<SecurityPolicy> FindPolicies(Expression<Func<SecurityPolicy, bool>> query, int offset, int? count, out int totalResults)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<SecurityRole> FindRoles(Expression<Func<SecurityRole, bool>> query)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<SecurityRole> FindRoles(Expression<Func<SecurityRole, bool>> query, int offset, int? count, out int totalResults)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<UserEntity> FindUserEntity(Expression<Func<UserEntity, bool>> expression)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<UserEntity> FindUserEntity(Expression<Func<UserEntity, bool>> expression, int offset, int? count, out int totalCount)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<SecurityUser> FindUsers(Expression<Func<SecurityUser, bool>> query)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<SecurityUser> FindUsers(Expression<Func<SecurityUser, bool>> query, int offset, int? count, out int totalResults)
        {
            throw new NotImplementedException();
        }

        public SecurityApplication GetApplication(Guid applicationId)
        {
            throw new NotImplementedException();
        }

        public SecurityDevice GetDevice(Guid deviceId)
        {
            throw new NotImplementedException();
        }

        public SecurityPolicy GetPolicy(Guid policyId)
        {
            throw new NotImplementedException();
        }

        public SecurityRole GetRole(Guid roleId)
        {
            throw new NotImplementedException();
        }

        public SecurityUser GetUser(string userName)
        {
            return new SecurityUser()
            {
                Key = Guid.Parse(AuthenticationContext.SystemUserSid),
                UserName = "SYSTEM"
            };
        }

        public SecurityUser GetUser(Guid userId)
        {
            throw new NotImplementedException();
        }

        public SecurityUser GetUser(IIdentity identity)
        {
            throw new NotImplementedException();
        }

        public UserEntity GetUserEntity(Guid id, Guid versionId)
        {
            throw new NotImplementedException();
        }

        public UserEntity GetUserEntity(IIdentity identity)
        {
            throw new NotImplementedException();
        }

        public void LockUser(Guid userId)
        {
            throw new NotImplementedException();
        }

        public SecurityApplication ObsoleteApplication(Guid applicationId)
        {
            throw new NotImplementedException();
        }

        public SecurityDevice ObsoleteDevice(Guid deviceId)
        {
            throw new NotImplementedException();
        }

        public SecurityPolicy ObsoletePolicy(Guid policyId)
        {
            throw new NotImplementedException();
        }

        public SecurityRole ObsoleteRole(Guid roleId)
        {
            throw new NotImplementedException();
        }

        public SecurityUser ObsoleteUser(Guid userId)
        {
            throw new NotImplementedException();
        }

        public UserEntity ObsoleteUserEntity(Guid id)
        {
            throw new NotImplementedException();
        }

        public SecurityApplication SaveApplication(SecurityApplication application)
        {
            throw new NotImplementedException();
        }

        public SecurityDevice SaveDevice(SecurityDevice device)
        {
            throw new NotImplementedException();
        }

        public SecurityPolicy SavePolicy(SecurityPolicy policy)
        {
            throw new NotImplementedException();
        }

        public SecurityRole SaveRole(SecurityRole role)
        {
            throw new NotImplementedException();
        }

        public SecurityUser SaveUser(SecurityUser user)
        {
            throw new NotImplementedException();
        }

        public UserEntity SaveUserEntity(UserEntity userEntity)
        {
            throw new NotImplementedException();
        }

        public void UnlockUser(Guid userId)
        {
            throw new NotImplementedException();
        }
    }
}
