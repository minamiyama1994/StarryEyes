﻿using System;
using System.Collections.Generic;
using StarryEyes.SweetLady.DataModel;

namespace StarryEyes.Mystique.Filters.Expressions.Values.Statuses
{
    public sealed class StatusId : ValueBase
    {
        public override IEnumerable<FilterExpressionType> SupportedTypes
        {
            get { yield return FilterExpressionType.Numeric; }
        }

        public override Func<TwitterStatus, long> GetNumericValueProvider()
        {
            return _ => _.GetOriginal().Id;
        }

        public override string ToQuery()
        {
            return "id";
        }
    }
}
