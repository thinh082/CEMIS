using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CEMIS.Models;

[Table("he_so_phat_thai")]
public partial class HeSoPhatThai
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("id_nguon_phat_thai")]
    public long IdNguonPhatThai { get; set; }

    [Column("id_gwp")]
    public long? IdGwp { get; set; }

    [Column("ten_he_so")]
    [StringLength(255)]
    public string TenHeSo { get; set; } = null!;

    [Column("nguon_goc")]
    [StringLength(150)]
    public string NguonGoc { get; set; } = null!;

    [Column("nam_ap_dung")]
    public int NamApDung { get; set; }

    [Column("he_so_co2")]
    [Precision(15, 6)]
    public decimal? HeSoCo2 { get; set; }

    [Column("he_so_ch4")]
    [Precision(15, 6)]
    public decimal? HeSoCh4 { get; set; }

    [Column("he_so_n2o")]
    [Precision(15, 6)]
    public decimal? HeSoN2o { get; set; }

    [Column("tong_he_so_co2e")]
    [Precision(15, 6)]
    public decimal TongHeSoCo2e { get; set; }

    [Column("don_vi_he_so")]
    [StringLength(50)]
    public string DonViHeSo { get; set; } = null!;

    [Column("ap_dung")]
    public bool? ApDung { get; set; }

    [Column("ghi_chu")]
    public string? GhiChu { get; set; }

    [ForeignKey("IdGwp")]
    [InverseProperty("HeSoPhatThais")]
    public virtual HeSoGwp? IdGwpNavigation { get; set; }

    [ForeignKey("IdNguonPhatThai")]
    [InverseProperty("HeSoPhatThais")]
    public virtual DanhMucNguonPhatThai IdNguonPhatThaiNavigation { get; set; } = null!;

    [InverseProperty("IdHeSoPhatThaiNavigation")]
    public virtual ICollection<KetQuaTinhToan> KetQuaTinhToans { get; set; } = new List<KetQuaTinhToan>();
}
