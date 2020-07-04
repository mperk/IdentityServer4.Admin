﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Skoruba.IdentityServer4.Admin.BusinessLogic.Dtos.Configuration.Tenants;
using Skoruba.IdentityServer4.Admin.BusinessLogic.Services.Interfaces;
using Skoruba.IdentityServer4.Admin.BusinessLogic.Shared.Dtos.Common;
using Skoruba.IdentityServer4.Admin.Configuration.Constants;
using Skoruba.IdentityServer4.Admin.ExceptionHandling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Skoruba.IdentityServer4.Admin.Controllers
{
    [Authorize(Policy = AuthorizationConsts.AdministrationPolicy)]
    [TypeFilter(typeof(ControllerExceptionFilterAttribute))]
    public class TenantController : BaseController
    {
        private readonly ITenantService TenantService;
        private readonly IEditionService EditionService;
        public TenantController(
            ILogger<TenantController> logger,
            ITenantService tenantService,
            IEditionService editionService)
             : base(logger)
        {
            TenantService = tenantService;
            EditionService = editionService;
        }

        [HttpGet]
        [Route("[controller]/[action]")]
        public async Task<IActionResult> Index(int? page, string search)
        {
            ViewBag.Search = search;
            var tenants = await TenantService.GetListAsync(search, page ?? 1);
            return View(tenants);
        }

        [HttpGet]
        [Route("[controller]/[action]/{id}")]
        public async Task<IActionResult> Details(string id)
        {
            var tenant = await TenantService.GetAsync(id);
            return View(tenant);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var vm = new CreateTenantDto();
            var editions = await EditionService.GetAllListAsync();
            vm.Editions = editions.Select(x => new SelectItemDto(x.Id.ToString(), x.Name)).ToList();
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateTenantDto dto)
        {
            if (!ModelState.IsValid)
            {
                return View(dto);
            }
            var newTenantDto = await TenantService.CreateAsync(dto);
            SuccessNotification("Saved successfully.", "Success");

            return RedirectToAction(nameof(Update), new { id = newTenantDto.Id });
        }

        [HttpGet]
        [Route("[controller]/[action]/{id}")]
        public async Task<IActionResult> Update(string id)
        {
            var tenantDto = await TenantService.GetAsync(id);
            if (tenantDto == null) return NotFound();
            var editions = await EditionService.GetAllListAsync();
            tenantDto.Editions = editions.Select(x => new SelectItemDto(x.Id.ToString(), x.Name)).ToList();
            return View(tenantDto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(UpdateTenantDto dto)
        {
            if (!ModelState.IsValid)
            {
                return View(dto);
            }
            var updatedTenantDto = await TenantService.UpdateAsync(dto);
            SuccessNotification("Updated successfully.", "Success");

            return RedirectToAction(nameof(Update), new { id = updatedTenantDto.Id });
        }

        [HttpGet]
        public async Task<IActionResult> Delete(string id)
        {
            var tenantDto = await TenantService.GetAsync(id);
            if (tenantDto == null) return NotFound();

            return View(tenantDto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(TenantDto dto)
        {
            await TenantService.DeleteAsync(dto.Id);
            SuccessNotification("Deleted successfully.", "Success");
            return RedirectToAction(nameof(Index));
        }


    }
}
