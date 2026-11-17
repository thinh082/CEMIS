using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CEMIS.Models;

[Table("he_so_gwp")]
public partial class HeSoGwp
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("phien_ban_ipcc")]
    [StringLength(50)]
    public string PhienBanIpcc { get; set; } = null!;

    [Column("gwp_co2")]
    [Precision(10, 4)]
    public decimal? GwpCo2 { get; set; }

    [Column("gwp_ch4")]
    [Precision(10, 4)]
    public decimal GwpCh4 { get; set; }

    [Column("gwp_n2o")]
    [Precision(10, 4)]
    public decimal GwpN2o { get; set; }

    [Column("ghi_chu")]
    public string? GhiChu { get; set; }

    [InverseProperty("IdGwpNavigation")]
    public virtual ICollection<HeSoPhatThai> HeSoPhatThais { get; set; } = new List<HeSoPhatThai>();
}
