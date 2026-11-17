using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CEMIS.Models;

[Table("bao_cao_xuat")]
public partial class BaoCaoXuat
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("id_to_chuc")]
    public long IdToChuc { get; set; }

    [Column("id_ky_bao_cao")]
    public long IdKyBaoCao { get; set; }

    [Column("id_nguoi_xuat")]
    public long IdNguoiXuat { get; set; }

    [Column("ten_bao_cao")]
    [StringLength(255)]
    public string TenBaoCao { get; set; } = null!;

    [Column("loai_bao_cao")]
    [StringLength(50)]
    public string LoaiBaoCao { get; set; } = null!;

    [Column("dinh_dang")]
    [StringLength(20)]
    public string DinhDang { get; set; } = null!;

    [Column("duong_dan_file")]
    public string DuongDanFile { get; set; } = null!;

    [Column("ngay_xuat", TypeName = "timestamp without time zone")]
    public DateTime NgayXuat { get; set; }

    [Column("ghi_chu")]
    public string? GhiChu { get; set; }

    [ForeignKey("IdKyBaoCao")]
    [InverseProperty("BaoCaoXuats")]
    public virtual KyBaoCao IdKyBaoCaoNavigation { get; set; } = null!;

    [ForeignKey("IdNguoiXuat")]
    [InverseProperty("BaoCaoXuats")]
    public virtual NguoiDung IdNguoiXuatNavigation { get; set; } = null!;

    [ForeignKey("IdToChuc")]
    [InverseProperty("BaoCaoXuats")]
    public virtual ToChuc IdToChucNavigation { get; set; } = null!;
}
