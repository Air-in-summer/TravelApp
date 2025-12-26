using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
namespace TravelApplication.Models
{
    public partial class User : ObservableObject
    {
        [ObservableProperty]
        public int userId;

        [ObservableProperty]
        public string? username;

        [ObservableProperty]
        public string? email;

        [ObservableProperty]
        public string? passwordHash;

        [ObservableProperty]
        public DateTime createdAt;

        [ObservableProperty]
        public string? role;

        [ObservableProperty]
        public string? avatarUrl;
    }
}
