﻿namespace Jobzy.Web.Controllers
{
    using System.Threading.Tasks;

    using Jobzy.Common;
    using Jobzy.Data.Models;
    using Jobzy.Services.Interfaces;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;

    public class OfferController : BaseController
    {
        private readonly IFreelancePlatform freelancePlatform;
        private readonly UserManager<ApplicationUser> userManager;

        public OfferController(
            IFreelancePlatform freelancePlatform,
            UserManager<ApplicationUser> userManager)
        {
            this.freelancePlatform = freelancePlatform;
            this.userManager = userManager;
        }

        [Route("/Job/Offers")]
        [Authorize(Roles = "Administrator, Employer")]
        public IActionResult GetJobOffers(string id)
        {
            var offers = this.freelancePlatform.OfferManager.GetJobOffers(id);

            return this.View(offers);
        }

        [HttpPost]
        [Route("/Job/")]
        [Authorize(Roles = "Administrator, Freelancer")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddOffer(string jobId, int fixedPrice, int deliveryDays)
        {
            var user = await this.userManager.GetUserAsync(this.User);

            await this.freelancePlatform.OfferManager.AddAsync(jobId, user.Id, fixedPrice, deliveryDays);

            return this.Redirect("/");
        }

        [HttpPost]
        [Route("/Job/Offers")]
        [Authorize(Roles = "Administrator, Employer")]
        public async Task<IActionResult> AcceptOffer(string offerId, string jobId)
        {
            await this.freelancePlatform.OfferManager.AcceptOffer(offerId);
            await this.freelancePlatform.JobManager.SetJobStatus(JobStatus.InContract, jobId);
            var contractId =
                await this.freelancePlatform.ContractManager.AddAsync(offerId);

            return this.Redirect($"/Contract?id={contractId}");
        }
    }
}
