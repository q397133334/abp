using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Guids;
using Volo.Docs.Projects;

namespace Volo.Docs.Admin.Projects
{
    public class ProjectAdminAppService : ApplicationService, IProjectAdminAppService
    {
        private readonly IProjectRepository _projectRepository;
        private readonly IGuidGenerator _guidGenerator;

        public ProjectAdminAppService(
            IProjectRepository projectRepository, IGuidGenerator guidGenerator)
        {
            _projectRepository = projectRepository;
            _guidGenerator = guidGenerator;
        }

        public async Task<PagedResultDto<ProjectDto>> GetListAsync(PagedAndSortedResultRequestDto input)
        {
            var blogs = await _projectRepository.GetListAsync(input.Sorting, input.MaxResultCount, input.SkipCount);

            var totalCount = await _projectRepository.GetTotalProjectCount();

            var dtos = ObjectMapper.Map<List<Project>, List<ProjectDto>>(blogs);

            return new PagedResultDto<ProjectDto>(totalCount, dtos);
        }

        public async Task<ProjectDto> GetAsync(Guid id)
        {
            var project = await _projectRepository.GetAsync(id);

            return ObjectMapper.Map<Project, ProjectDto>(project);
        }

        public async Task<ProjectDto> CreateAsync(CreateProjectDto input)
        {
            var project = new Project(_guidGenerator.Create(),
                input.Name,
                input.ShortName,
                input.DocumentStoreType,
                input.Format,
                input.DefaultDocumentName,
                input.NavigationDocumentName
            )
            {
                MinimumVersion = input.MinimumVersion,
                MainWebsiteUrl = input.MainWebsiteUrl,
                LatestVersionBranchName = input.LatestVersionBranchName
            };

            foreach (var extraProperty in input.ExtraProperties)
            {
                project.ExtraProperties.Add(extraProperty.Key,extraProperty.Value);
            }

            project = await _projectRepository.InsertAsync(project);

            return ObjectMapper.Map<Project, ProjectDto>(project);
        }

        public async Task<ProjectDto> UpdateAsync(Guid id, UpdateProjectDto input)
        {
            var project = await _projectRepository.GetAsync(id);

            project.SetName(input.Name);
            project.SetFormat(input.Format);
            project.SetNavigationDocumentName(input.NavigationDocumentName);
            project.SetDefaultDocumentName(input.DefaultDocumentName);

            project.MinimumVersion = input.MinimumVersion;
            project.MainWebsiteUrl = input.MainWebsiteUrl;
            project.LatestVersionBranchName = input.LatestVersionBranchName;

            foreach (var extraProperty in input.ExtraProperties)
            {
                project.ExtraProperties[extraProperty.Key] = extraProperty.Value;
            }

            project = await _projectRepository.UpdateAsync(project);

            return ObjectMapper.Map<Project, ProjectDto>(project);
        }

        public async Task DeleteAsync(Guid id)
        {
            await _projectRepository.DeleteAsync(id);
        }
    }
}