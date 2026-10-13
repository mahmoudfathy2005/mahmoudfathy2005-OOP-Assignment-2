using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibrarySystem
{

    public enum LoanStatus
    {
        Borrowed,
        Returned,
        Lost
    }

    

public class Loan
    {
        public string LoanId { get; }
        public DateTime BorrowDate { get; }
        public DateTime DueDate { get; }
        public Member Member { get; }
        public LibraryItem Item { get; }

        public LoanStatus Status { get; private set; }
        public DateTime? ReturnDate { get; private set; }

        public decimal LateFee
        {
            get
            {
                if (Status != LoanStatus.Returned ||
                    ReturnDate == null)
                {
                    return 0m;
                }

                int lateDays =
                    (ReturnDate.Value.Date - DueDate.Date).Days;

                if (lateDays <= 0)
                    return 0m;

                decimal fee =
                    lateDays * Item.GetDailyLateFee();

                return fee *
                    (1m - Member.DiscountPercentage / 100m);
            }
        }

        internal Loan(
            string loanId,
            DateTime borrowDate,
            Member member,
            LibraryItem item)
        {
            if (string.IsNullOrWhiteSpace(loanId))
                throw new ArgumentException(
                    "Loan ID is required.");

            if (member == null)
                throw new ArgumentNullException(nameof(member));

            if (item == null)
                throw new ArgumentNullException(nameof(item));

            LoanId = loanId;
            BorrowDate = borrowDate.Date;
            Member = member;
            Item = item;

            DueDate = BorrowDate.AddDays(item.LoanPeriodDays);
            Status = LoanStatus.Borrowed;
            ReturnDate = null;
        }

        public void Return(DateTime returnDate)
        {
            if (Status != LoanStatus.Borrowed)
                throw new InvalidOperationException(
                    "Only borrowed loans can be returned.");

            if (returnDate.Date < BorrowDate.Date)
                throw new ArgumentException(
                    "Return date cannot be before borrow date.");

            ReturnDate = returnDate.Date;
            Status = LoanStatus.Returned;

            Item.MarkReturned();
        }

        public void MarkAsLost()
        {
            if (Status != LoanStatus.Borrowed)
                throw new InvalidOperationException(
                    "Only borrowed loans can be marked as lost.");

            Status = LoanStatus.Lost;
            Item.MarkReturned();
        }
    }

}
