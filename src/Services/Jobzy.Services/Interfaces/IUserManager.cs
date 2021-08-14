﻿namespace Jobzy.Services.Interfaces
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using Jobzy.Common;
    using Jobzy.Web.ViewModels.Users;
    using Jobzy.Web.ViewModels.Users.Employers;
    using Jobzy.Web.ViewModels.Users.Freelancers;

    public interface IUserManager
    {
        Task<T> GetUserById<T>(string id);

        Task<IEnumerable<T>> GetAllFreelancers<T>(
            int rating = 0,
            string name = null,
            Sorting sorting = Sorting.Newest,
            int currentPage = 1);

        EmployerViewModel GetEmployer(string userId);

        FreelancerViewModel GetFreelancer(string userId);

        BaseUserViewModel GetUserSettings(string userId);

        Task UpdateUserProfilePicture(string pictureUrl, string userId);

        Task UpdateUserInfo(UserInfoInputModel input, string userId);

        Task UpdateUserOnlineStatus(string status, string userId);

        int GetAllFreelancersCount();
    }
}
