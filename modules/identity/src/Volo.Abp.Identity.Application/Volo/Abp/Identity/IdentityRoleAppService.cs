﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.PermissionManagement;

namespace Volo.Abp.Identity
{
    [Authorize(IdentityPermissions.Roles.Default)]
    public class IdentityRoleAppService : ApplicationService, IIdentityRoleAppService
    {
        private readonly IdentityRoleManager _roleManager;
        private readonly IIdentityRoleRepository _roleRepository;
        private readonly IPermissionAppServiceHelper _permissionAppServiceHelper;

        public IdentityRoleAppService(
            IdentityRoleManager roleManager,
            IIdentityRoleRepository roleRepository, 
            IPermissionAppServiceHelper permissionAppServiceHelper)
        {
            _roleManager = roleManager;
            _roleRepository = roleRepository;
            _permissionAppServiceHelper = permissionAppServiceHelper;
        }

        public async Task<IdentityRoleDto> GetAsync(Guid id)
        {
            return ObjectMapper.Map<IdentityRole, IdentityRoleDto>(
                await _roleManager.GetByIdAsync(id)
            );
        }

        public async Task<PagedResultDto<IdentityRoleDto>> GetListAsync(GetIdentityRolesInput input) //TODO: Remove this method since it's not used
        {
            var count = (int) await _roleRepository.GetCountAsync();
            var list = await _roleRepository.GetListAsync();

            return new PagedResultDto<IdentityRoleDto>(
                count,
                ObjectMapper.Map<List<IdentityRole>, List<IdentityRoleDto>>(list)
            );
        }

        public async Task<List<IdentityRoleDto>> GetAllListAsync() //TODO: Rename to GetList (however it's not possible because of the design of the IAsyncCrudAppService)
        {
            var list = await _roleRepository.GetListAsync();

            return ObjectMapper.Map<List<IdentityRole>, List<IdentityRoleDto>>(list);
        }

        [Authorize(IdentityPermissions.Roles.ManagePermissions)]
        public async Task<GetPermissionListResultDto> GetPermissionsAsync(Guid id)
        {
            var role = await _roleRepository.GetAsync(id);
            return await _permissionAppServiceHelper.GetAsync(RolePermissionValueProvider.ProviderName, role.Name); //TODO: User normalized role name instad of name?
        }

        [Authorize(IdentityPermissions.Roles.ManagePermissions)]
        public async Task UpdatePermissionsAsync(Guid id, UpdatePermissionsDto input)
        {
            var role = await _roleRepository.GetAsync(id);
            await _permissionAppServiceHelper.UpdateAsync(RolePermissionValueProvider.ProviderName, role.Name, input);
        }

        [Authorize(IdentityPermissions.Roles.Create)]
        public async Task<IdentityRoleDto> CreateAsync(IdentityRoleCreateDto input)
        {
            var role = new IdentityRole(GuidGenerator.Create(), input.Name, CurrentTenant.Id);

            role.IsDefault = input.IsDefault;
            role.IsPublic = input.IsPublic;

            (await _roleManager.CreateAsync(role)).CheckErrors();
            await CurrentUnitOfWork.SaveChangesAsync();

            return ObjectMapper.Map<IdentityRole, IdentityRoleDto>(role);
        }

        [Authorize(IdentityPermissions.Roles.Update)]
        public async Task<IdentityRoleDto> UpdateAsync(Guid id, IdentityRoleUpdateDto input)
        {
            var role = await _roleManager.GetByIdAsync(id);
            role.ConcurrencyStamp = input.ConcurrencyStamp;

            (await _roleManager.SetRoleNameAsync(role, input.Name)).CheckErrors();

            role.IsDefault = input.IsDefault;
            role.IsPublic = input.IsPublic;

            (await _roleManager.UpdateAsync(role)).CheckErrors();
            await CurrentUnitOfWork.SaveChangesAsync();

            return ObjectMapper.Map<IdentityRole, IdentityRoleDto>(role);
        }

        [Authorize(IdentityPermissions.Roles.Delete)]
        public async Task DeleteAsync(Guid id)
        {
            var role = await _roleManager.FindByIdAsync(id.ToString());
            if (role == null)
            {
                return;
            }

            (await _roleManager.DeleteAsync(role)).CheckErrors();
        }
    }
}
