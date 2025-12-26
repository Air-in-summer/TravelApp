using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging.Messages;
namespace TravelApplication.Services
{
    public class UpdateStopMessage : ValueChangedMessage<int>
    {
        public UpdateStopMessage(int id) : base(id) { }
    }

    public class CancelUpdateStopMessage : ValueChangedMessage<int> 
    {
        public CancelUpdateStopMessage(int id) : base(id) { }
    }

}
