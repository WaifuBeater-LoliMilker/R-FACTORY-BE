namespace R_Factory_BE.DTO
{
    public class ElectricUsageChartData
    {
        public DateTime DayDate { get; set; }
        public int YearValue { get; set; }
        public int MonthValue { get; set; }
        public int DayValue { get; set; }
        public decimal LogValue { get; set; }
    }
}
