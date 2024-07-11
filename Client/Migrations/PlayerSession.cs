using System;
using System.Collections.Generic;

namespace Client.Migrations;

public partial class PlayerSession
{
    public string PlayerId { get; set; } = null!;

    public string Date { get; set; } = null!;

    public int? TimePlayedInSession { get; set; }

    public virtual Player Player { get; set; } = null!;
}
