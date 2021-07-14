﻿namespace Jobzy.Services
{
    using Jobzy.Services.Interfaces;

    public class FreelancePlatformManager : IFreelancePlatformManager
    {
        public FreelancePlatformManager(
            IJobManager jobManager,
            IBalanceManager balanceManager,
            IOfferManager offerManager,
            IContractManager contractManager)
        {
            this.JobManager = jobManager;
            this.BalanceManager = balanceManager;
            this.OfferManager = offerManager;
            this.ContractManager = contractManager;
        }

        public IJobManager JobManager { get; }

        public IBalanceManager BalanceManager { get; }

        public IOfferManager OfferManager { get; }

        public IContractManager ContractManager { get; }
    }
}
