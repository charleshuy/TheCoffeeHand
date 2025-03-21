using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.ServiceInterfaces {
    public interface IRabbitMQService {
        Task SendMessageAsync(string queueName, string message);
    }

}
