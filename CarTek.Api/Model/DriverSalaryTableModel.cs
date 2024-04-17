namespace CarTek.Api.Model
{
    public class DriverSalaryTableModel
    {
        public DateTime StartDate { get; set; } 
        public DateTime EndDate { get; set; }

        public string Name { get; set; }

        public int TasksCount { get; set; }

        public double TotalSalary { get; set; }
    }
}
