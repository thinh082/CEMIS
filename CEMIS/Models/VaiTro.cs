using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CEMIS.Models;

[Table("vai_tro")]
[Index("MaVaiTro", Name = "vai_tro_ma_vai_tro_key", IsUnique = true)]
public partial class VaiTro
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("ma_vai_tro")]
    [StringLength(50)]
    public string MaVaiTro { get; set; } = null!;

    [Column("ten_vai_tro")]
    [StringLength(100)]
    public string TenVaiTro { get; set; } = null!;

    [Column("mo_ta")]
    public string? MoTa { get; set; }

    [InverseProperty("IdVaiTroNavigation")]
    public virtual ICollection<NguoiDung> NguoiDungs { get; set; } = new List<NguoiDung>();
}
