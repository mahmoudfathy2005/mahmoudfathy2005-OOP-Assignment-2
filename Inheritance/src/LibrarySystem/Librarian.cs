using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibrarySystem
{

    using System;

    public class Librarian : Staff
    {
        public Librarian(
            string personId,
            string fullName,
            string phone,
            DateTime hireDate,
            decimal monthlySalary)
            : base(
                personId,
                fullName,
                phone,
                hireDate,
                monthlySalary,
                0m)
        {
        }

        public void ProcessReturn(
            Loan loan,
            DateTime returnDate)
        {
            if (loan == null)
                throw new ArgumentNullException(nameof(loan));

            loan.Return(returnDate);
        }

        public void MarkItemAsLost(Loan loan)
        {
            if (loan == null)
                throw new ArgumentNullException(nameof(loan));

            loan.MarkAsLost();
        }
    }

}
