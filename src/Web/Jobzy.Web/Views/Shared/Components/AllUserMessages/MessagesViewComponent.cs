﻿namespace Jobzy.Web.ViewComponents
{
    using System.Threading.Tasks;

    using Jobzy.Data.Models;
    using Jobzy.Services.Interfaces;
    using Jobzy.Web.ViewModels.Messages.AllConversations;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;

    [ViewComponent(Name = "AllUserMessages")]
    public class MessagesViewComponent : ViewComponent
    {
        private readonly IFreelancePlatform freelancePlatform;
        private readonly UserManager<ApplicationUser> userManager;

        public MessagesViewComponent(
            IFreelancePlatform freelancePlatform,
            UserManager<ApplicationUser> userManager)
        {
            this.freelancePlatform = freelancePlatform;
            this.userManager = userManager;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var currentUserId = this.userManager
                .GetUserId(this.UserClaimsPrincipal);
            var userConversations = await this.freelancePlatform.MessageManager
                .GetAllUserConversationsAsync<AllUserConversationsViewModel>(currentUserId);

            foreach (var user in userConversations)
            {
                user.LastMessage = await this.freelancePlatform.MessageManager
                    .GetConversationLastMessageAsync(currentUserId, user.Id);

                user.ReceivedDate =
                    await this.freelancePlatform.MessageManager
                    .GetConversationLastMessageSentDateAsync(currentUserId, user.Id);
            }

            return this.View(userConversations);
        }
    }
}
