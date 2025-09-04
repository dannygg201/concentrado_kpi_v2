using System.Collections.Generic;

namespace ConcentradoKPI.App.Models
{
    public class TableData
    {
        public string Name { get; set; } = "";
        public List<string> Columns { get; set; } = new();
        public List<List<string>> Rows { get; set; } = new();
    }
}
