using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibrarySystem
{


    public class Member : Person
    {
        private readonly List<Loan> _loans = new List<Loan>();

        public IReadOnlyList<Loan> Loans
        {
            get { return _loans.AsReadOnly(); }
        }

        public int MaxLoans { get; }
        public decimal DiscountPercentage { get; }

        protected Member(
            string personId,
            string fullName,
            string phone,
            int maxLoans,
            decimal discountPercentage)
            : base(personId, fullName, phone)
        {
            if (maxLoans <= 0)
                throw new ArgumentOutOfRangeException(
                    nameof(maxLoans), "Loan limit must be positive.");

            if (discountPercentage < 0 || discountPercentage > 100)
                throw new ArgumentOutOfRangeException(
                    nameof(discountPercentage),
                    "Discount must be between 0 and 100.");

            MaxLoans = maxLoans;
            DiscountPercentage = discountPercentage;
        }

        public Loan Borrow(
            string loanId,
            LibraryItem item,
            DateTime borrowDate)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            int activeLoans = 0;

            foreach (Loan loan in _loans)
            {
                if (loan.Status == LoanStatus.Borrowed)
                    activeLoans++;
            }

            if (activeLoans >= MaxLoans)
                throw new InvalidOperationException(
                    "Member has reached the loan limit.");

            if (item.IsWithdrawn)
                throw new InvalidOperationException(
                    "Withdrawn items cannot be borrowed.");

            if (item.IsOnLoan)
                throw new InvalidOperationException(
                    "This item is already on loan.");

            Loan newLoan = new Loan(
                loanId, borrowDate, this, item);

            item.MarkBorrowed();
            _loans.Add(newLoan);

            return newLoan;
        }
    }

}
