using System;
using PetSoLive.Core.Entities;
using PetSoLive.Core.Enums;

namespace PetSoLive.Core.Entities
{
    public class AdoptionRequest
    {
        public int Id { get; set; }
        public int PetId { get; set; }
        public int UserId { get; set; }
        public string? Message { get; set; }
        public AdoptionStatus Status { get; set; }
        public DateTime RequestDate { get; set; }

        public Pet Pet { get; set; }
        public User User { get; set; }
    }
}