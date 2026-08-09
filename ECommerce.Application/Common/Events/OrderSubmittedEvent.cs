using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Application.Common.Events;

public record OrderSubmittedEvent(Guid ProductId, int QuantityPurchased);
