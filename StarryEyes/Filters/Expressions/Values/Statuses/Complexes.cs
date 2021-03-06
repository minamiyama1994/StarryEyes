﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using StarryEyes.Anomaly.TwitterApi.DataModels;

namespace StarryEyes.Filters.Expressions.Values.Statuses
{
    public sealed class StatusTo : ValueBase
    {
        public override IEnumerable<FilterExpressionType> SupportedTypes
        {
            get
            {
                yield return FilterExpressionType.Numeric;
                yield return FilterExpressionType.String;
                yield return FilterExpressionType.Set;
            }
        }

        public override Func<TwitterStatus, long> GetNumericValueProvider()
        {
            return _ => _.StatusType == StatusType.Tweet ?
                _.GetOriginal().InReplyToUserId.GetValueOrDefault(-1) :
                _.Recipient.Id;
        }

        public override string GetNumericSqlQuery()
        {
            return Coalesce("InReplyToOrRecipientUserId", -1);
        }

        public override Func<TwitterStatus, string> GetStringValueProvider()
        {
            return _ => _.StatusType == StatusType.Tweet ?
                _.GetOriginal().InReplyToScreenName ?? String.Empty :
                _.Recipient.ScreenName;
        }

        public override string GetStringSqlQuery()
        {
            return Coalesce("InReplyToOrRecipientScreenName", "");
        }

        public override Func<TwitterStatus, IReadOnlyCollection<long>> GetSetValueProvider()
        {
            return _ => _.StatusType == StatusType.Tweet ?
                FilterSystemUtil.InReplyToUsers(_.GetOriginal()).ToList() :
                new[] { _.Recipient.Id }.ToList();
        }

        public override string GetSetSqlQuery()
        {
            var userMention = ((int)EntityType.UserMentions).ToString(CultureInfo.InvariantCulture);
            return "(select InReplyToOrRecipientUserId union " +
                   "select UserId from StatusEntity where " +
                   "ParentId = status.Id and " +
                   "EntityType = " + userMention + " and " +
                   "UserId is not null)";
        }

        public override string ToQuery()
        {
            return "to";
        }
    }

    public sealed class StatusFavorites : ValueBase
    {
        public override IEnumerable<FilterExpressionType> SupportedTypes
        {
            get
            {
                yield return FilterExpressionType.Set;
                yield return FilterExpressionType.Numeric;
            }
        }

        public override Func<TwitterStatus, IReadOnlyCollection<long>> GetSetValueProvider()
        {
            return _ => _.FavoritedUsers ?? new long[0];
        }

        public override string GetSetSqlQuery()
        {
            return "(select UserId from Favorites where StatusId = status.Id)";
        }

        public override Func<TwitterStatus, long> GetNumericValueProvider()
        {
            return _ => (_.FavoritedUsers ?? new long[0]).Length;
        }

        public override string GetNumericSqlQuery()
        {
            return "(select count(Id) from Favorites where StatusId = status.Id)";
        }

        public override string ToQuery()
        {
            return "favs";
        }
    }

    public sealed class StatusRetweets : ValueBase
    {
        public override IEnumerable<FilterExpressionType> SupportedTypes
        {
            get
            {
                yield return FilterExpressionType.Set;
                yield return FilterExpressionType.Numeric;
            }
        }

        public override Func<TwitterStatus, IReadOnlyCollection<long>> GetSetValueProvider()
        {
            return _ => _.RetweetedUsers ?? new long[0];
        }

        public override string GetSetSqlQuery()
        {
            return "(select UserId from Retweets where StatusId = status.Id)";
        }

        public override Func<TwitterStatus, long> GetNumericValueProvider()
        {
            return _ => (_.RetweetedUsers ?? new long[0]).Length;
        }

        public override string GetNumericSqlQuery()
        {
            return "(select count(Id) from Retweets where StatusId = status.Id)";
        }

        public override string ToQuery()
        {
            return "retweets";
        }
    }
}
