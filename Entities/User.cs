using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MmgMapAPI.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public Role Role { get; set; }
        public bool isActive { get; set; }
        public List<Mo> Mos { get; set; }
        public DateTime Created { get; set; }
        public DateTime LastUpdated { get; set; }

        public User()
        {
        }

        public User(int id, string username, Role role, bool isActive, DateTime created, DateTime lastUpdated)
        {
            Id = id;
            Username = username;
            Role = role;
            this.isActive = isActive;
            Created = created;
            LastUpdated = lastUpdated;
        }
    }
    public enum Role
    {
        USER,
        ADMIN
    }
}
