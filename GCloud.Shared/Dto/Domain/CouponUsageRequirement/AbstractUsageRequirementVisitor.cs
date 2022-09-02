using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GCloud.Shared.Dto.Domain.CouponUsageRequirement
{
    public abstract class AbstractUsageRequirementVisitor
    {
        public abstract bool Visit(ProductRequiredUsageRequirementDto usageRequirement, CouponDto couponDto);

        public abstract bool Visit(MinimumTurnoverRequirementDto minimumTurnoverRequirementDto, CouponDto couponDto);
    }
}
