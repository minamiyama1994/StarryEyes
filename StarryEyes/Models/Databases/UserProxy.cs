﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using StarryEyes.Annotations;
using StarryEyes.Anomaly.TwitterApi.DataModels;
using StarryEyes.Casket;
using StarryEyes.Casket.DatabaseModels;

namespace StarryEyes.Models.Databases
{
    public static class UserProxy
    {
        public static long GetId(string screenName)
        {
            return Database.UserCrud.GetId(screenName);
        }

        public static async Task StoreUserAsync(TwitterUser user)
        {
            var map = Mapper.Map(user);
            await Database.StoreUser(map.Item1, map.Item2, map.Item3);
        }

        public static async Task<TwitterUser> GetUserAsync(long id)
        {
            var u = await Database.UserCrud.GetAsync(id);
            if (u == null) return null;
            var ude = Database.UserDescriptionEntityCrud.GetEntitiesAsync(id);
            var uue = Database.UserUrlEntityCrud.GetEntitiesAsync(id);
            return Mapper.Map(u, await ude, await uue);
        }

        public static async Task<TwitterUser> GetUserAsync(string screenName)
        {
            var user = await Database.UserCrud.GetAsync(screenName);
            return user == null ? null : await LoadUserAsync(user);
        }

        public static async Task<IObservable<TwitterUser>> GetUsersAsync(string partOfScreenName)
        {
            return LoadUsersAsync(await Database.UserCrud.GetUsersAsync(partOfScreenName));
        }

        public static async Task<IEnumerable<Tuple<long, string>>> GetUsersFastAsync(string partOfScreenName)
        {
            var resp = await Database.UserCrud.GetUsersAsync(partOfScreenName);
            return resp.Guard().Select(d => Tuple.Create(d.Id, d.ScreenName));
        }

        public static IObservable<TwitterUser> LoadUsersAsync([NotNull] IEnumerable<DatabaseUser> dbusers)
        {
            if (dbusers == null) throw new ArgumentNullException("dbusers");
            return dbusers
                .ToObservable()
                .SelectMany(s => LoadUserAsync(s).ToObservable());
        }

        private static async Task<TwitterUser> LoadUserAsync([NotNull] DatabaseUser user)
        {
            if (user == null) throw new ArgumentNullException("user");
            var ude = Database.UserDescriptionEntityCrud.GetEntitiesAsync(user.Id);
            var uue = Database.UserUrlEntityCrud.GetEntitiesAsync(user.Id);
            return Mapper.Map(user, await ude, await uue);
        }

        public static async Task<bool> IsFollowingAsync(long userId, long targetId)
        {
            return await Database.RelationCrud.IsFollowingAsync(userId, targetId);
        }

        public static async Task<bool> IsFollowerAsync(long userId, long targetId)
        {
            return await Database.RelationCrud.IsFollowerAsync(userId, targetId);
        }

        public static async Task<bool> IsBlockingAsync(long userId, long targetId)
        {
            return await Database.RelationCrud.IsBlockingAsync(userId, targetId);
        }

        public static async Task<bool> IsNoRetweetsAsync(long userId, long targetId)
        {
            return await Database.RelationCrud.IsNoRetweetsAsync(userId, targetId);
        }

        public static async Task SetFollowingAsync(long userId, long targetId, bool following)
        {
            await Database.RelationCrud.SetFollowingAsync(userId, targetId, following);
        }

        public static async Task SetFollowerAsync(long userId, long targetId, bool followed)
        {
            await Database.RelationCrud.SetFollowerAsync(userId, targetId, followed);
        }

        public static async Task SetBlockingAsync(long userId, long targetId, bool blocking)
        {
            await Database.RelationCrud.SetBlockingAsync(userId, targetId, blocking);
        }

        public static async Task SetNoRetweetsAsync(long userId, long targetId, bool suppressing)
        {
            await Database.RelationCrud.SetNoRetweetsAsync(userId, targetId, suppressing);
        }

        public static async Task AddFollowingsAsync(long userId, IEnumerable<long> targetIds)
        {
            await Database.RelationCrud.AddFollowingsAsync(userId, targetIds);
        }

        public static async Task RemoveFollowingsAsync(long userId, IEnumerable<long> removals)
        {
            await Database.RelationCrud.RemoveFollowingsAsync(userId, removals);
        }

        public static async Task AddFollowersAsync(long userId, IEnumerable<long> targetIds)
        {
            await Database.RelationCrud.AddFollowersAsync(userId, targetIds);
        }

        public static async Task RemoveFollowersAsync(long userId, IEnumerable<long> removals)
        {
            await Database.RelationCrud.RemoveFollowersAsync(userId, removals);
        }

        public static async Task AddBlockingsAsync(long userId, IEnumerable<long> targetIds)
        {
            await Database.RelationCrud.AddBlockingsAsync(userId, targetIds);
        }

        public static async Task RemoveBlockingsAsync(long userId, IEnumerable<long> removals)
        {
            await Database.RelationCrud.RemoveBlockingsAsync(userId, removals);
        }

        public static async Task AddNoRetweetssAsync(long userId, IEnumerable<long> targetIds)
        {
            await Database.RelationCrud.AddNoRetweetsAsync(userId, targetIds);
        }

        public static async Task RemoveNoRetweetssAsync(long userId, IEnumerable<long> removals)
        {
            await Database.RelationCrud.RemoveNoRetweetsAsync(userId, removals);
        }

        public static async Task<IEnumerable<long>> GetFollowingsAsync(long userId)
        {
            return await Database.RelationCrud.GetFollowingsAsync(userId);
        }

        public static async Task<IEnumerable<long>> GetFollowersAsync(long userId)
        {
            return await Database.RelationCrud.GetFollowersAsync(userId);
        }

        public static async Task<IEnumerable<long>> GetBlockingsAsync(long userId)
        {
            return await Database.RelationCrud.GetBlockingsAsync(userId);
        }

        public static async Task<IEnumerable<long>> GetNoRetweetsAsync(long userId)
        {
            return await Database.RelationCrud.GetNoRetweetsAsync(userId);
        }

        public static async Task<IEnumerable<long>> GetFollowingsAllAsync()
        {
            return await Database.RelationCrud.GetFollowingsAllAsync();
        }

        public static async Task<IEnumerable<long>> GetFollowersAllAsync()
        {
            return await Database.RelationCrud.GetFollowersAllAsync();
        }

        public static async Task<IEnumerable<long>> GetBlockingsAllAsync()
        {
            return await Database.RelationCrud.GetBlockingsAllAsync();
        }

        public static async Task<IEnumerable<long>> GetNoRetweetsAllAsync()
        {
            return await Database.RelationCrud.GetNoRetweetsAllAsync();
        }
    }
}
