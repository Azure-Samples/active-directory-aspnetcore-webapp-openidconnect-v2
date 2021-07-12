using System;
using System.Collections.Generic;
using System.Linq;

namespace AnonimusFuncs
{
    class Program
    {
        static void Main(string[] args)
        {
            var list = new List<int> { 2, 3, 4, 6 };

            //DO THIS
            Delegates<int>.Print(Delegates<int>.CompareTo(list, i => i > Delegates<int>.Threshold(3)));

            //OR THIS
            Delegates<int>.CompareTo(list, i => i > Delegates<int>.Threshold(3)).ToList().ForEach(i => Console.Write(i + " , "));
            Console.WriteLine();

            //OR THIS
            bool IsBiggerThan(int number) => number > Delegates<int>.Threshold(3);
            Delegates<int>.CompareTo(list, IsBiggerThan).ToList().ForEach(i => Console.Write(i + " , "));
            Console.WriteLine();


            ///Complex object
            var students = new List<StudentName>
            {
                new StudentName{FirstName = "A", LastName = "D"},
                new  StudentName{FirstName = "C", LastName = "B"}
            };

            Delegates<StudentName>.SortAndFilter(students, SortOrderEnum.FirstName).ToList().ForEach(i => Console.WriteLine(i.PrintMe()));
        }
    }

    class Delegates<T>
    {
        public static IEnumerable<T> CompareTo(IEnumerable<T> input, Func<T, bool> condition)
        {
            return input.Where(i => condition(i));
        }

        public static T Threshold(T input) => input;

        public static void Print(IEnumerable<T> input)
        {
            foreach (var item in input)
            {
                Console.Write(item + " , ");
            }

            Console.WriteLine();
        }

        public static IEnumerable<StudentName> SortAndFilter(IEnumerable<StudentName> input, SortOrderEnum sortOrder)
            => input.OrderBy(OrderBy(sortOrder));

        public static Func<StudentName, IComparable> OrderBy(SortOrderEnum sortOrder)
        {
            return sortOrder switch
            {
                SortOrderEnum.FirstName => student => student.FirstName,
                SortOrderEnum.LastName => student => student.LastName,
                _ => throw new InvalidOperationException("Unknown sorting parameter."),
            };
        }
    }

    class StudentName
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PrintMe()
        {
            return $"{FirstName},{LastName}";
        }
    }

    public enum SortOrderEnum
    {
        FirstName,
        LastName
    }
}
