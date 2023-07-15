using MmgMapAPI.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MmgMapAPI.DTO
{
    public class MoDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string Coordinates { get; set; }
        public string ResponsibleEmployeeFIO { get; set; }
        public string ResponsibleEmployeePhoneNumber { get; set; }
        public string ResponsibleEmployeeEmail { get; set; }
        public PartialData PartialData { get; set; }
        public DateTime Created { get; set; }
        public DateTime LastUpdated { get; set; }

        public MoDTO (Mo mo, PartialData partialData)
        {
            Id = mo.Id;
            Name = mo.Name;
            Address = mo.Address;
            Coordinates = mo.Coordinates;
            ResponsibleEmployeeFIO = mo.ResponsibleEmployeeFIO;
            ResponsibleEmployeePhoneNumber = mo.ResponsibleEmployeePhoneNumber;
            ResponsibleEmployeeEmail = mo.ResponsibleEmployeeEmail;
            Created = mo.Created;
            LastUpdated = mo.LastUpdated;
            this.PartialData = partialData;
        }
    }
}
