﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using StarryEyes.Annotations;
using StarryEyes.Anomaly.TwitterApi.DataModels;
using StarryEyes.Filters;
using StarryEyes.Models.Accounting;
using StarryEyes.Models.Databases;
using StarryEyes.Models.Receiving.Handling;
using StarryEyes.Models.Subsystems.Notifications;
using StarryEyes.Models.Timelines.Statuses;
using StarryEyes.Models.Timelines.Tabs;
using StarryEyes.Settings;

namespace StarryEyes.Models.Subsystems
{
    /// <summary>
    /// Controls around notification.
    /// Delivering entities, Optimizing, Blocking, etc.
    /// </summary>
    public static class NotificationService
    {
        private static INotificationSink _head;
        private static NotificationProxy _tail;
        private static readonly INotificationSink _sink = new NotificationSink();

        private static INotificationSink Head { get { return _head ?? _sink; } }

        public static void RegisterProxy([NotNull] NotificationProxy proxy)
        {
            if (proxy == null) throw new ArgumentNullException("proxy");
            if (_head == null)
            {
                _head = proxy;
            }
            else
            {
                _tail.Next = proxy;
            }
            proxy.Next = _sink;
            _tail = proxy;
        }

        internal static void NotifyReceived(TwitterStatus status)
        {
            if (status.RetweetedOriginal != null)
            {
                NotifyRetweeted(status.User, status.RetweetedOriginal);
            }
            Head.NotifyReceived(status);
        }

        #region New Arrival Control

        private static readonly Dictionary<long, List<TabModel>> _acceptingArrivals = new Dictionary<long, List<TabModel>>();

        internal static void StartAcceptNewArrival(TwitterStatus status)
        {
            lock (_acceptingArrivals)
            {
                _acceptingArrivals[status.Id] = new List<TabModel>();
            }
        }

        internal static void NotifyNewArrival(TwitterStatus status, TabModel model)
        {
            lock (_acceptingArrivals)
            {
                List<TabModel> list;
                if (_acceptingArrivals.TryGetValue(status.Id, out list))
                {
                    list.Add(model);
                }
            }
        }

        internal static void EndAcceptNewArrival(TwitterStatus status)
        {
            List<TabModel> removal;
            lock (_acceptingArrivals)
            {
                if (!_acceptingArrivals.TryGetValue(status.Id, out removal) || removal.Count == 0) return;
            }
            var soundSource = removal
                .Select(t => t.NotifySoundSource)
                .Where(s => !String.IsNullOrEmpty(s))
                .Where(File.Exists)
                .FirstOrDefault();
            if (status.StatusType == StatusType.DirectMessage)
            {
                Task.Run(() => NotifyNewArrival(status, NotificationType.DirectMessage, soundSource));
            }
            else if (FilterSystemUtil.InReplyToUsers(status).Intersect(Setting.Accounts.Ids).Any())
            {
                Task.Run(() => NotifyNewArrival(status, NotificationType.Mention, soundSource));
            }
            else
            {
                Task.Run(() => NotifyNewArrival(status, NotificationType.Normal, soundSource));
            }
        }

        private static void NotifyNewArrival(TwitterStatus status, NotificationType type, string explicitSoundSource)
        {
            Head.NotifyNewArrival(status, type, explicitSoundSource);
        }

        #endregion

        internal static void NotifyFollowed(TwitterUser source, TwitterUser target)
        {
            Head.NotifyFollowed(source, target);
        }

        internal static void NotifyUnfollowed(TwitterUser source, TwitterUser target)
        {
            Head.NotifyUnfollowed(source, target);
        }

        internal static void NotifyBlocked(TwitterUser source, TwitterUser target)
        {
            Head.NotifyBlocked(source, target);
        }

        internal static void NotifyUnblocked(TwitterUser source, TwitterUser target)
        {
            Head.NotifyUnblocked(source, target);
        }

        internal static void NotifyFavorited(TwitterUser source, TwitterStatus status)
        {
            Task.Run(() => UserProxy.StoreUserAsync(source));
            Task.Run(() => StatusModel.UpdateStatusInfo(
                status.Id,
                model => model.AddFavoritedUser(source),
                async _ =>
                {
                    await StatusProxy.AddFavoritorAsync(status.Id, source.Id);
                    StatusBroadcaster.Republish(status);
                }));
            Head.NotifyFavorited(source, status);
        }

        internal static void NotifyUnfavorited(TwitterUser source, TwitterStatus status)
        {
            Task.Run(() => StatusModel.UpdateStatusInfo(
                status.Id,
                model => model.RemoveFavoritedUser(source.Id),
                async _ =>
                {
                    await StatusProxy.RemoveFavoritorAsync(status.Id, source.Id);
                    StatusBroadcaster.Republish(status);
                }));
            Head.NotifyUnfavorited(source, status);
        }

        internal static void NotifyRetweeted(TwitterUser source, TwitterStatus status)
        {
            Task.Run(() => UserProxy.StoreUserAsync(source));
            Task.Run(() => StatusModel.UpdateStatusInfo(
                status.Id,
                model => model.AddRetweetedUser(source),
                async _ =>
                {
                    await StatusProxy.AddRetweeterAsync(status.Id, source.Id);
                    StatusBroadcaster.Republish(status);
                }));
            Head.NotifyRetweeted(source, status);
        }

        internal static void NotifyDeleted(long statusId, TwitterStatus deleted)
        {
            if (deleted != null && deleted.RetweetedOriginal != null)
            {
                Task.Run(() => StatusModel.UpdateStatusInfo(
                    deleted.RetweetedOriginal.Id,
                    model => model.RemoveRetweetedUser(
                        deleted.User.Id),
                    async _ => await StatusProxy.RemoveRetweeterAsync(
                        deleted.RetweetedOriginal.Id, deleted.User.Id)));
            }
            Head.NotifyDeleted(statusId, deleted);
        }

        internal static void NotifyLimitationInfoGot(TwitterAccount account, int trackLimit)
        {
            Head.NotifyLimitationInfoGot(account, trackLimit);
        }

        internal static void NotifyUserUpdated(TwitterUser source)
        {
            Head.NotifyUserUpdated(source);
        }
    }
}
