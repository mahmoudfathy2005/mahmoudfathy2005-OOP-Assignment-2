using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibrarySystem
{

    using System;

    public class Staff : Person
    {
        public DateTime HireDate { get; }
        public decimal MonthlySalary { get; private set; }
        public decimal ResponsibilityAllowance { get; }

        protected Staff(
            string personId,
            string fullName,
            string phone,
            DateTime hireDate,
            decimal monthlySalary,
            decimal responsibilityAllowance)
            : base(personId, fullName, phone)
        {
            if (monthlySalary < 0)
                throw new ArgumentOutOfRangeException(
                    nameof(monthlySalary),
                    "Salary cannot be negative."
                );

            if (responsibilityAllowance < 0)
                throw new ArgumentOutOfRangeException(
                    nameof(responsibilityAllowance),
                    "Allowance cannot be negative."
                );

            HireDate = hireDate;
            MonthlySalary = monthlySalary;
            ResponsibilityAllowance = responsibilityAllowance;
        }

        public void GiveRaise(decimal percentage)
        {
            if (percentage <= 0)
                throw new ArgumentOutOfRangeException(
                    nameof(percentage),
                    "Raise percentage must be positive.");

            MonthlySalary += MonthlySalary * percentage / 100m;
        }

        public decimal GetMonthlyPay()
        {
            return MonthlySalary + ResponsibilityAllowance;
        }
    }

}
