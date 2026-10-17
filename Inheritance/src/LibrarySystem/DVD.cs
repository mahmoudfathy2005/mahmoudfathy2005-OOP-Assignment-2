using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibrarySystem
{

    public class DVD : LibraryItem
    {
        public DVD(
            string catalogNumber,
            string title,
            decimal baseLateFee)
            : base(
                catalogNumber,
                title,
                7,
                baseLateFee,
                2m)
        {
        }
    }

}
