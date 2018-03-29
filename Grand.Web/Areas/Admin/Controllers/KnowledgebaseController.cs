﻿using Grand.Framework.Kendoui;
using Grand.Framework.Mvc.Filters;
using Grand.Services.Knowledgebase;
using Grand.Services.Localization;
using Grand.Services.Security;
using Grand.Web.Areas.Admin.Extensions;
using Grand.Web.Areas.Admin.Models.Knowledgebase;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Web.Areas.Admin.Controllers
{
    public class KnowledgebaseController : BaseAdminController
    {
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;
        private readonly IKnowledgebaseService _knowledgebaseService;

        public KnowledgebaseController(ILocalizationService localizationService, IPermissionService permissionService, IKnowledgebaseService knowledgebaseService)
        {
            this._localizationService = localizationService;
            this._permissionService = permissionService;
            this._knowledgebaseService = knowledgebaseService;
        }

        public IActionResult Index()
        {
            return RedirectToAction("List");
        }

        public IActionResult List()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageKnowledgebase))
                return AccessDeniedView();

            return View();
        }

        [HttpPost]
        public IActionResult CategoryList(DataSourceRequest command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageKnowledgebase))
                return AccessDeniedView();

            var categories = _knowledgebaseService.GetKnowledgebaseCategories();
            var gridModel = new DataSourceResult
            {
                Data = categories,
                Total = categories.Count
            };

            return Json(gridModel);
        }

        public IActionResult CreateCategory()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageKnowledgebase))
                return AccessDeniedView();

            return View(new KnowledgebaseCategoryModel { DisplayOrder = 1, ParentCategoryId = 0 });
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public IActionResult CreateCategory(KnowledgebaseCategoryModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageKnowledgebase))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var knowledgebaseCategory = model.ToEntity();
                knowledgebaseCategory.CreatedOnUtc = DateTime.UtcNow;
                knowledgebaseCategory.UpdatedOnUtc = DateTime.UtcNow;
                _knowledgebaseService.InsertKnowledgebaseCategory(knowledgebaseCategory);

                SuccessNotification(_localizationService.GetResource("Admin.ContentManagement.Knowledgebase.KnowledgebaseCategory.Added"));
                return continueEditing ? RedirectToAction("EditCategory", new { knowledgebaseCategory.Id }) : RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
            return View(model);
        }

        public IActionResult EditCategory(string id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageKnowledgebase))
                return AccessDeniedView();

            var knowledgebaseCategory = _knowledgebaseService.GetKnowledgebaseCategory(id);
            if (knowledgebaseCategory == null)
                return RedirectToAction("List");

            var model = knowledgebaseCategory.ToModel();
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public IActionResult EditCategory(KnowledgebaseCategoryModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageKnowledgebase))
                return AccessDeniedView();

            var knowledgebaseCategory = _knowledgebaseService.GetKnowledgebaseCategory(model.Id);
            if (knowledgebaseCategory == null)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                knowledgebaseCategory = model.ToEntity(knowledgebaseCategory);
                knowledgebaseCategory.UpdatedOnUtc = DateTime.UtcNow;
                _knowledgebaseService.UpdateKnowledgebaseCategory(knowledgebaseCategory);

                SuccessNotification(_localizationService.GetResource("Admin.ContentManagement.Knowledgebase.KnowledgebaseCategory.Updated"));
                return continueEditing ? RedirectToAction("EditCategory", new { id = knowledgebaseCategory.Id }) : RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpPost]
        public IActionResult DeleteCategory(string id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageKnowledgebase))
                return AccessDeniedView();

            var knowledgebaseCategory = _knowledgebaseService.GetKnowledgebaseCategory(id);
            if (knowledgebaseCategory == null)
                return RedirectToAction("List");

            _knowledgebaseService.DeleteKnowledgebaseCategory(knowledgebaseCategory);

            SuccessNotification(_localizationService.GetResource("Admin.ContentManagement.Knowledgebase.KnowledgebaseCategory.Deleted"));
            return RedirectToAction("List");
        }
    }
}
