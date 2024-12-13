// /PetSoLive.Core/Entities/Assistance.cs
using System;

namespace PetSoLive.Core.Entities
{
    public class Assistance
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime DateCreated { get; set; }
        public string ContactInfo { get; set; }
    }
}