namespace FieldOps.Api.Models;

// A spare part in the warehouse (sample data, seeded by the migration)
public class SparePart
{
    public int Id {get; set;}
    public string PartNumber {get; set;} = "";
    public string Name {get; set;} = "";
    public string Equipment {get; set;} = "";
    public int Quantity {get; set;}
    public string Bin {get; set;} = "";


}