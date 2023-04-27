using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XO.EntityFrameworkCore;

internal sealed class TestModel
{
    [Key]
    public int Id { get; set; } = TestContext.NextId();

    [Column(TypeName = "jsonb")]
    public TestJsonDataObject? Data { get; set; }

    [Column(TypeName = "jsonb")]
    public TestJsonDataObject? DataCustomValueComparer { get; set; }

    [Column(TypeName = "jsonb")]
    public TestJsonDataObject? DataCustomValueConverter { get; set; }

    [Column(TypeName = "jsonb")]
    public TestJsonDataObject? DataExplicit { get; set; }

    [Column(TypeName = "jsonb")]
    public TestJsonDataObject? DataExplicitNoValueComparer { get; set; }
}
