using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibrarySystem
{

    public class Book : LibraryItem
    {
        public Book(
            string catalogNumber,
            string title,
            decimal baseLateFee)
            : base(
                catalogNumber,
                title,
                21,
                baseLateFee,
                1m)
        {
        }
    }

}
