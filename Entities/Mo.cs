using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MmgMapAPI.Entities
{
    public class Mo
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string Coordinates { get; set; }
        public string ResponsibleEmployeeFIO { get; set; }
        public string ResponsibleEmployeePhoneNumber { get; set; }
        public string ResponsibleEmployeeEmail { get; set; }
        public DateTime Created { get; set; }
        public DateTime LastUpdated { get; set; }

        public Mo()
        {
        }

        public Mo(int id, string name, string address, string coordinates, string responsibleEmployeeFIO, string responsibleEmployeePhoneNumber, string responsibleEmployeeEmail, DateTime created, DateTime lastUpdated)
        {
            Id = id;
            Name = name;
            Address = address;
            Coordinates = coordinates;
            ResponsibleEmployeeFIO = responsibleEmployeeFIO;
            ResponsibleEmployeePhoneNumber = responsibleEmployeePhoneNumber;
            ResponsibleEmployeeEmail = responsibleEmployeeEmail;
            Created = created;
            LastUpdated = lastUpdated;
        }

        public override bool Equals(object obj)
        {
            return obj is Mo mo &&
                   Id == mo.Id;
        }
    }
}
