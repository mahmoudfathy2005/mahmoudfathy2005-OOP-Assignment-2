namespace LibrarySystem
{
    internal class Program
    {
        public static void Main()
        {
            Console.WriteLine("=== Library System ===");

            // Staff examples
            Librarian librarian = new Librarian(
                "P001", "Ahmed Ali", "01000000001",
                new DateTime(2022, 1, 1), 8000m);

            Shelver shelver = new Shelver(
                "P002", "Mona Samir", "01000000002",
                new DateTime(2023, 3, 1), 6000m, "Science");

            HeadLibrarian head = new HeadLibrarian(
                "P003", "Omar Hassan", "01000000003",
                new DateTime(2020, 5, 1), 12000m);

            List<Staff> staffMembers = new List<Staff>
        {
            librarian,
            shelver,
            head
        };

            Console.WriteLine("\nMonthly pay:");

            foreach (Staff staff in staffMembers)
            {
                Console.WriteLine(
                    staff.FullName + ": " +
                    staff.GetMonthlyPay());
            }

            // Library items
            Book book = new Book(
                "B001", "C# Basics", 2m);

            DVD dvd = new DVD(
                "D001", "Learning .NET", 2m);

            Magazine magazine = new Magazine(
                "M001", "Tech Monthly", 2m);

            List<LibraryItem> items = new List<LibraryItem>
        {
            book,
            dvd,
            magazine
        };

            Console.WriteLine("\nItem loan periods and daily fees:");

            foreach (LibraryItem item in items)
            {
                Console.WriteLine(
                    item.Title +
                    " | Loan days: " + item.LoanPeriodDays +
                    " | Daily late fee: " + item.GetDailyLateFee());
            }

            // Members
            StudentMember student = new StudentMember(
                "P004", "Sara Mohamed", "01000000004");

            PremiumMember premium = new PremiumMember(
                "P005", "Youssef Adel", "01000000005", 10m);

            // Borrowing a book
            Loan studentLoan = student.Borrow(
                "L001", book, new DateTime(2026, 1, 1));

            Console.WriteLine("\nStudent borrowed: " +
                studentLoan.Item.Title);

            // Student loan limit: three active loans
            Book book2 = new Book("B002", "OOP", 2m);
            Book book3 = new Book("B003", "UML", 2m);
            Book book4 = new Book("B004", "Design Patterns", 2m);

            student.Borrow("L002", book2, new DateTime(2026, 1, 1));
            student.Borrow("L003", book3, new DateTime(2026, 1, 1));

            TryAction("Borrowing a fourth active item", () =>
            {
                student.Borrow(
                    "L004", book4, new DateTime(2026, 1, 1));
            });

            // Withdrawn items cannot be borrowed
            Book withdrawnBook = new Book(
                "B005", "Withdrawn Book", 2m);

            head.WithdrawItem(withdrawnBook);

            TryAction("Borrowing a withdrawn item", () =>
            {
                student.Borrow(
                    "L005", withdrawnBook, new DateTime(2026, 1, 1));
            });

            // An item cannot be borrowed twice at the same time
            Book sharedBook = new Book(
                "B006", "Shared Book", 2m);

            student.Borrow(
                "L006", sharedBook, new DateTime(2026, 1, 1));

            TryAction("Borrowing an already-loaned item", () =>
            {
                premium.Borrow(
                    "L007", sharedBook, new DateTime(2026, 1, 1));
            });

            // Premium member returns a DVD five days late
            DVD lateDVD = new DVD(
                "D002", "Late DVD", 2m);

            Loan premiumLoan = premium.Borrow(
                "L008", lateDVD, new DateTime(2026, 1, 1));

            // DVD loan period = 7 days, due date = January 8.
            // Return date = January 13, so it is 5 days late.
            librarian.ProcessReturn(
                premiumLoan, new DateTime(2026, 1, 13));

            Console.WriteLine("\nPremium member late return:");
            Console.WriteLine("Due date: " +
                premiumLoan.DueDate.ToShortDateString());

            Console.WriteLine("Return date: " +
                premiumLoan?.ReturnDate!.Value.ToShortDateString());

            Console.WriteLine("Late fee: " + premiumLoan?.LateFee);
            Console.WriteLine("Reading points: " + premium.ReadingPoints);

            // Cannot return the same loan twice
            TryAction("Returning the same loan twice", () =>
            {
                librarian.ProcessReturn(
                    premiumLoan!, new DateTime(2026, 1, 14));
            });

            // Cannot mark a returned loan as lost
            TryAction("Marking a returned loan as lost", () =>
            {
                librarian.MarkItemAsLost(premiumLoan);
            });

            // Raise and section reassignment
            librarian.GiveRaise(10m);
            shelver.Reassign("Technology");

            Console.WriteLine("\nAfter updates:");
            Console.WriteLine("Librarian salary: " +
                librarian.MonthlySalary);

            Console.WriteLine("Shelver section: " +
                shelver.Section);

            // Invalid raise must fail
            TryAction("Giving a zero raise", () =>
            {
                librarian.GiveRaise(0m);
            });

           

            Console.WriteLine("\n=== End of tests ===");
        }

        private static void TryAction(
            string description,
            Action action)
        {
            try
            {
                action();
                Console.WriteLine(description + ": unexpectedly succeeded.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    description + ": rejected (" + ex.Message + ")");
            }
        }
    }
}
