using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibrarySystem
{

    public class StudentMember : Member
    {
        public StudentMember(
            string personId,
            string fullName,
            string phone)
            : base(personId, fullName, phone, 3, 0m)
        {
        }
    }

}
