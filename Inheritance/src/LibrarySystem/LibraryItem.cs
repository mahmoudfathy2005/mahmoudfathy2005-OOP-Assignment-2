using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibrarySystem
{

    using System;

    public class LibraryItem
    {
        public string CatalogNumber { get; }
        public string Title { get; }
        public int LoanPeriodDays { get; }
        public decimal BaseLateFee { get; private set; }
        public decimal LateFeeMultiplier { get; }
        public bool IsWithdrawn { get; private set; }
        public bool IsOnLoan { get; private set; }

        protected LibraryItem(
            string catalogNumber,
            string title,
            int loanPeriodDays,
            decimal baseLateFee,
            decimal lateFeeMultiplier)
        {
            if (string.IsNullOrWhiteSpace(catalogNumber))
                throw new ArgumentException(
                    "Catalog number is required.");

            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException(
                    "Title is required.");

            if (loanPeriodDays <= 0)
                throw new ArgumentOutOfRangeException(
                    nameof(loanPeriodDays));

            if (baseLateFee <= 0)
                throw new ArgumentOutOfRangeException(
                    nameof(baseLateFee),
                    "Base late fee must be positive.");

            if (lateFeeMultiplier <= 0)
                throw new ArgumentOutOfRangeException(
                    nameof(lateFeeMultiplier));

            CatalogNumber = catalogNumber;
            Title = title;
            LoanPeriodDays = loanPeriodDays;
            BaseLateFee = baseLateFee;
            LateFeeMultiplier = lateFeeMultiplier;
            IsWithdrawn = false;
            IsOnLoan = false;
        }

        public decimal GetDailyLateFee()
        {
            return BaseLateFee * LateFeeMultiplier;
        }

        public void ChangeBaseLateFee(decimal newFee)
        {
            if (newFee <= 0)
                throw new ArgumentOutOfRangeException(
                    nameof(newFee),
                    "New late fee must be positive.");

            BaseLateFee = newFee;
        }

        public void Withdraw()
        {
            if (IsOnLoan)
                throw new InvalidOperationException(
                    "Cannot withdraw an item while it is on loan.");

            IsWithdrawn = true;
        }

        public void Restore()
        {
            if (IsOnLoan)
                throw new InvalidOperationException(
                    "Cannot restore an item while it is on loan.");

            IsWithdrawn = false;
        }

        internal void MarkBorrowed()
        {
            if (IsWithdrawn)
                throw new InvalidOperationException(
                    "Withdrawn items cannot be borrowed.");

            if (IsOnLoan)
                throw new InvalidOperationException(
                    "Item is already on loan.");

            IsOnLoan = true;
        }

        internal void MarkReturned()
        {
            IsOnLoan = false;
        }
    }

}
