using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MmgMapAPI.Entities
{
    public class Type
    {
        public int Id { get; set; }
        public string Value { get; set; }

        public Type(int id, string value)
        {
            Id = id;
            Value = value;
        }

        public static Type GetType1()
        {
            return new Type(1, "С прикреплённым населением");
        }

        public static Type GetType2()
        {
            return new Type(2, "Работающие по собственному тарифу");
        }
    }
}
