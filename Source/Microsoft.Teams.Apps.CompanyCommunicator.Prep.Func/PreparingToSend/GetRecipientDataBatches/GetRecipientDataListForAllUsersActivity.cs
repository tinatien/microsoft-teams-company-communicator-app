﻿// <copyright file="GetRecipientDataListForAllUsersActivity.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.CompanyCommunicator.Prep.Func.PreparingToSend.GetRecipientDataBatches
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Teams.Apps.CompanyCommunicator.Common.Repositories.NotificationData;
    using Microsoft.Teams.Apps.CompanyCommunicator.Common.Repositories.SentNotificationData;
    using Microsoft.Teams.Apps.CompanyCommunicator.Common.Repositories.UserData;

    /// <summary>
    /// This class contains the "get recipient data list for all users" durable activity.
    /// This activity prepares the SentNotification data table by filling it with an initialized row
    /// for each recipient - for "all users" every user in the user data table is a recipient.
    /// 1). It gets the recipient data list for all users stored in the user data table.
    /// 2). It initializes the sent notification data table with a row for each recipient.
    /// </summary>
    public class GetRecipientDataListForAllUsersActivity
    {
        private readonly UserDataRepository userDataRepository;
        private readonly SentNotificationDataRepository sentNotificationDataRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetRecipientDataListForAllUsersActivity"/> class.
        /// </summary>
        /// <param name="userDataRepository">User Data repository.</param>
        /// <param name="sentNotificationDataRepository">Sent notification data repository.</param>
        public GetRecipientDataListForAllUsersActivity(
            UserDataRepository userDataRepository,
            SentNotificationDataRepository sentNotificationDataRepository)
        {
            this.userDataRepository = userDataRepository;
            this.sentNotificationDataRepository = sentNotificationDataRepository;
        }

        /// <summary>
        /// Run the activity.
        /// </summary>
        /// <param name="context">Durable orchestration context.</param>
        /// <param name="notificationDataEntity">Notification data entity.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task RunAsync(
            DurableOrchestrationContext context,
            NotificationDataEntity notificationDataEntity)
        {
            await context.CallActivityWithRetryAsync<IEnumerable<UserDataEntity>>(
                nameof(GetRecipientDataListForAllUsersActivity.GetAllUsersRecipientDataListAsync),
                ActivitySettings.CommonActivityRetryOptions,
                notificationDataEntity.Id);
        }

        /// <summary>
        /// This method represents the "get recipient data list for all users" durable activity.
        /// 1). It gets the recipient data list for all users stored in the user data table.
        /// 2). It initializes the sent notification data table with a row for each recipient.
        /// </summary>
        /// <param name="notificationDataEntityId">Notification data entity id.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [FunctionName(nameof(GetAllUsersRecipientDataListAsync))]
        public async Task GetAllUsersRecipientDataListAsync(
            [ActivityTrigger] string notificationDataEntityId)
        {
            var allUsersRecipientDataList = await this.userDataRepository.GetAllAsync();

            await this.sentNotificationDataRepository
                .InitializeSentNotificationDataForUserRecipientBatchAsync(notificationDataEntityId, allUsersRecipientDataList);
        }
    }
}
