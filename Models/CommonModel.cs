namespace PSAPLCDashboard.Web.Dashboard.Models
{
    public class TrnasformerWiseData
    {
        public float MAXFREQ { get; set; }
        public float POWERFACTOR { get; set; }
        public float ACTIVEENERGYDELIVERED { get; set; }
        // public int Pending { get; set; }
    }

    public class BodyShopData
    {
        public float MAXFREQ { get; set; }
        public float POWERFACTOR { get; set; }
        public float ACTIVEENERGYDELIVERED { get; set; }
        //public int Pending { get; set; }
    }

    public class PaintShopData
    {
        public float MAXFREQ { get; set; }
        public float POWERFACTOR { get; set; }
        public float ACTIVEENERGYDELIVERED { get; set; }
        //public int Pending { get; set; }
    }
    public class UtilitiesData
    {
        public float MAXFREQ { get; set; }
        public float POWERFACTOR { get; set; }
        public float ACTIVEENERGYDELIVERED { get; set; }
        // public int Pending { get; set; }
    }

    public class OverAllData
    {
        public float MAXFREQ { get; set; }
        public float POWERFACTOR { get; set; }
        public float ACTIVEENERGYDELIVERED { get; set; }
        //public int Pending { get; set; }
    }

    public class TestModel
    {
        public string[] PowerFactor { get; set; }
        public string[] MeterName { get; set; }
        public string[] SYNCDATETIME { get; set; }
        public string[] MaxDemand { get; set; }
        public string[] ActiveEnergy { get; set; }


        public string[] BODY_SHOP_ActiveEnergy { get; set; }
        public string[] GENERAL_ASSEMBLY_ActiveEnergy { get; set; }
        public string[] HOSTED_SERVICES_ActiveEnergy { get; set; }
        public string[] PAINT_SHOP_ActiveEnergy { get; set; }
        public string[] TRANSFORMER_WISE_ActiveEnergy { get; set; }
        public string[] UTILITIES_ActiveEnergy { get; set; }
    }

    public class EnergyMeterModel
    {
        public string[] MeterName { get; set; }
        public string[] DateAndTime { get; set; }
        public string[] Current_A { get; set; }
        public string[] Current_B { get; set; }
        public string[] Current_C { get; set; }
        public string[] Voltage_A { get; set; }
        public string[] Voltage_B { get; set; }
        public string[] Voltage_C { get; set; }
        public string[] PowerFactor { get; set; }
        public string[] MaxDemand { get; set; }
        public string[] ActiveEnergy { get; set; }
        public string[] Consumption { get; set; }
        public string[] syncdatetime { get; set; }
    }
}