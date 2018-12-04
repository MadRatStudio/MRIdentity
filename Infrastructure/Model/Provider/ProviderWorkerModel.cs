using Infrastructure.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Infrastructure.Model.Provider
{
    public class ProviderWorkerDisplayModel
    {
        public string UserId { get; set; }
        public string UserEmail { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }

        public UserStatus Status { get; set; }

        public string AvatarUrl { get; set; }

        public List<ProviderWorkerRole> Roles { get; set; }

        public ProviderWorkerDisplayModel ApplyUser(AppUser user)
        {
            if (user == null) return this;

            FirstName = user.FirstName;
            LastName = user.LastName;

            Status = user.Status;

            AvatarUrl = user.Avatar?.Src;

            return this;
        }
    }

    public class ProviderWorkerUpdateModel
    {
        [Required]
        public string UserId { get; set; }

        [Required]
        public List<ProviderWorkerRole> Roles { get; set; }
    }

    public class ProviderWorkerCreateModel
    {
        [Required]
        public string Email { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        public List<ProviderWorkerRole> Roles { get; set; }
    }
}
