using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibrarySystem
{

    public class Magazine : LibraryItem
    {
        public Magazine(
            string catalogNumber,
            string title,
            decimal baseLateFee)
            : base(
                catalogNumber,
                title,
                3,
                baseLateFee,
                0.5m)
        {
        }
    }

}
