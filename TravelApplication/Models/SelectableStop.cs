using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TravelApplication.Models
{
    public class SelectableStop
    {
        public Stop Stop { get; set; }
        public bool IsSelectedForSorting { get; set; } = true; // mặc định chọn hết
    }
}
