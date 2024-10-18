using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Bsp01.Data;

[Table("benutzer")]
[Index("Email", IsUnique = true)]
public partial class Benutzer
{
    [Key]
    public long BenutzerId { get; set; }

    public string Name { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Passwort { get; set; } = null!;

    public string Salt { get; set; } = null!;

    public string Rolle { get; set; } = null!;
}
